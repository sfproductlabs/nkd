using System;
using System.Linq;
using System.Linq.Expressions;
using Gallery.Core.Domain;
using Gallery.Core.Enums;
using JetBrains.Annotations;
using Orchard.Gallery.GalleryServer;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;
using Orchard.Logging;
using Orchard.Tasks.Indexing;

namespace Orchard.Gallery.PackageSynchronization {
    [UsedImplicitly]
    public class PackagePartUpdater : IPackagePartUpdater {
        private readonly IPackageService _packageService;
        private readonly IPackagePartMapper _packagePartMapper;
        private readonly IODataContext _oDataContext;
        private readonly IPackagePartExtendedInfoUpdater _packagePartExtendedInfoUpdater;
        private readonly IRecommendedVersionUpdater _recommendedVersionUpdater;
        private readonly IIndexingTaskManager _indexingTaskManager;
        private readonly IPackagePartPublishingService _packagePartPublishingService;

        public ILogger Logger { get; set; }

        public PackagePartUpdater(IPackageService packageService, IPackagePartMapper packagePartMapper,
            IODataContext oDataContext, IPackagePartExtendedInfoUpdater packagePartExtendedInfoUpdater,
            IRecommendedVersionUpdater recommendedVersionUpdater, IIndexingTaskManager indexingTaskManager,
            IPackagePartPublishingService packagePartPublishingService) {
            _packageService = packageService;
            _indexingTaskManager = indexingTaskManager;
            _packagePartPublishingService = packagePartPublishingService;
            _packagePartExtendedInfoUpdater = packagePartExtendedInfoUpdater;
            _recommendedVersionUpdater = recommendedVersionUpdater;
            _oDataContext = oDataContext;
            _packagePartMapper = packagePartMapper;

            Logger = NullLogger.Instance;
        }

        public void ModifyExistingPackagePart(PackageLogEntry log, bool updateExtendedPackageInfo) {
            foreach (var publishedPackage in _oDataContext.Packages.Where(GetODataQuery(log))) {
                var packagePart = _packageService.Get(publishedPackage.Id, publishedPackage.Version);
                if (packagePart != null) {
                    UpdatePackage(log, publishedPackage, packagePart);
                    if (updateExtendedPackageInfo) {
                        _packagePartExtendedInfoUpdater.UpdateExtendedPackageInfo(publishedPackage, packagePart);
                    }
                    if (log.Action == PackageLogAction.RePublish) {
                        _packagePartPublishingService.Publish(packagePart.PackageID, packagePart.PackageVersion);
                    }
                    _indexingTaskManager.CreateUpdateIndexTask(packagePart.ContentItem);
                    Logger.Information("Updated PackagePart '{0}', version {1}. Extended info updated: {2}", log.PackageId, log.PackageVersion,
                        updateExtendedPackageInfo);
                }
            }
        }

        private static Expression<Func<PublishedPackage, bool>> GetODataQuery(PackageLogEntry log) {
            Expression<Func<PublishedPackage, bool>> query;
            if (log.Action == PackageLogAction.Download) {
                query = pp => pp.Id == log.PackageId;
            } else {
                query = pp => pp.Id == log.PackageId && pp.Version == log.PackageVersion;
            }
            return query;
        }

        private void UpdatePackage(PackageLogEntry log, PublishedPackage publishedPackage, PackagePart packagePart) {
            if (log.Action == PackageLogAction.Update || log.Action == PackageLogAction.RePublish) {
                _packagePartMapper.MapPublishedPackageToPackagePart(publishedPackage, packagePart);
                _recommendedVersionUpdater.SetRecommendedVersionFlagsOfOtherPackagesWithSameId(packagePart);
            }
            else {
                packagePart.DownloadCount = publishedPackage.VersionDownloadCount;
                packagePart.TotalDownloadCount = publishedPackage.DownloadCount;
            }
        }
    }
}