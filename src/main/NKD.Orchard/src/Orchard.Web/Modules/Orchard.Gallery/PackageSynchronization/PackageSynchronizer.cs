using System;
using System.Collections.Generic;
using Gallery.Core.Domain;
using Gallery.Core.Enums;
using JetBrains.Annotations;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;
using Orchard.Gallery.Services;
using Orchard.Indexing;
using Orchard.Logging;

namespace Orchard.Gallery.PackageSynchronization
{
    [UsedImplicitly]
    public class PackageSynchronizer : ThreadSafeActionBase, IPackageSynchronizer {
        private readonly IPackagePartCreator _packagePartCreator;
        private readonly IPackagePartUpdater _packagePartUpdater;
        private readonly IPackagePartDeleter _packagePartDeleter;
        private readonly IPackageLogEntryService _packageLogEntryService;
        private readonly IOrchardServices _orchardServices;
        private readonly ITypeCaster _typeCaster;
        private readonly IIndexNotifierHandler _indexNotifierHandler;
        private readonly IPackagePartPublishingService _packagePartPublishingService;

        public ILogger Logger { get; set; }

        public PackageSynchronizer(IPackagePartCreator packagePartCreator, IPackagePartUpdater packagePartUpdater, IPackagePartDeleter packagePartDeleter,
            IPackageLogEntryService packageLogEntryService, IOrchardServices orchardServices, ITypeCaster typeCaster, IIndexNotifierHandler indexNotifierHandler,
            IPackagePartPublishingService packagePartPublishingService) {
            _packagePartCreator = packagePartCreator;
            _indexNotifierHandler = indexNotifierHandler;
            _packagePartPublishingService = packagePartPublishingService;
            _typeCaster = typeCaster;
            _packagePartUpdater = packagePartUpdater;
            _packagePartDeleter = packagePartDeleter;
            _packageLogEntryService = packageLogEntryService;
            _orchardServices = orchardServices;

            Logger = NullLogger.Instance;
        }

        protected override void ExecuteThreadSafeAction() {
            Logger.Information("Running Package synchronization against OData feed.");
            IEnumerable<PackageLogEntry> logs = _packageLogEntryService.GetUnprocessedLogEntries();

            foreach (var log in logs) {
                try {
                    switch (log.Action) {
                        case PackageLogAction.Create:
                            _packagePartCreator.CreateNewPackagePart(log);
                            break;
                        case PackageLogAction.Update:
                        case PackageLogAction.RePublish:
                            _packagePartUpdater.ModifyExistingPackagePart(log, true);
                            break;
                        case PackageLogAction.Delete:
                            _packagePartDeleter.DeletePackage(log);
                            break;
                        case PackageLogAction.Download:
                            _packagePartUpdater.ModifyExistingPackagePart(log);
                            break;
                        case PackageLogAction.Unpublish:
                            _packagePartPublishingService.Unpublish(log.PackageId, log.PackageVersion);
                            break;
                    }
                    _typeCaster.CastTo<GallerySettingsPart>(_orchardServices.WorkContext.CurrentSite).LastPackageLogId = log.Id;
                }
                catch (Exception ex) {
                    Logger.Error(ex, "An error occurred during Package Synchronization while processing Log Entry #{0} ({1} on Package '{2}', Version {3}).",
                        log.Id, log.Action, log.PackageId, log.PackageVersion);
                    break;
                }
            }
            _indexNotifierHandler.UpdateIndex("Search");
        }
    }
}