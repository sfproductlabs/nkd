using Gallery.Core.Domain;
using JetBrains.Annotations;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;
using Orchard.Logging;
using System.Linq;

namespace Orchard.Gallery.PackageSynchronization {
    [UsedImplicitly]
    public class PackagePartDeleter : IPackagePartDeleter {
        private readonly IPackageService _packageService;
        private readonly IScreenshotPartDeleter _screenshotPartDeleter;
        private readonly IODataContext _oDataContext;

        public ILogger Logger { get; set; }

        public PackagePartDeleter(IPackageService packageService, IOrchardServices orchardServices,
            IScreenshotPartDeleter screenshotPartDeleter, IODataContext oDataContext) {
            _packageService = packageService;
            _screenshotPartDeleter = screenshotPartDeleter;
            _oDataContext = oDataContext;

            Logger = NullLogger.Instance;
        }

        public void DeletePackage(PackageLogEntry log) {
            _packageService.Delete(log.PackageId, log.PackageVersion);
            _screenshotPartDeleter.DeleteScreenshotsForPackage(log.PackageId, log.PackageVersion);
            foreach (var publishedPackage in _oDataContext.Packages.Where(p => p.Id == log.PackageId)) {
                PackagePart packagePart = _packageService.Get(publishedPackage.Id, publishedPackage.Version, true);
                if (packagePart != null) {
                    packagePart.TotalDownloadCount = publishedPackage.DownloadCount;
                } else {
                    Logger.Information("Tried to update total download count for package {0}, version {1}, but did not find it in Orchard database.");
                }
            }
            Logger.Information("Deleted PackagePart '{0}', version {1} from Orchard database.", log.PackageId, log.PackageVersion);
        }
    }
}