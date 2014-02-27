using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;
using Orchard.Logging;

namespace Orchard.Gallery.PackageSynchronization {
    [UsedImplicitly]
    public class PackagePartPublishingService : IPackagePartPublishingService {
        private readonly IContentManager _contentManager;
        private readonly IPackageService _packageService;

        public ILogger Logger { get; set; }


        public PackagePartPublishingService(IContentManager contentManager, IPackageService packageService) {
            _contentManager = contentManager;
            _packageService = packageService;

            Logger = NullLogger.Instance;
        }

        public void Unpublish(string packageId, string packageVersion) {
            PackagePart packagePart = _packageService.Get(packageId, packageVersion, true);
            if (packagePart == null) {
                Logger.Information("Could not Unpublish Package {0}, Version {1} because it does not exist.");
                return;
            }
            if (!packagePart.ContentItem.IsPublished()) {
                return;
            }

            _contentManager.Unpublish(packagePart.ContentItem);
            _packageService.ResetRecommendedVersionForPackage(packagePart);
        }

        public void Publish(string packageId, string packageVersion) {
            PackagePart packagePart = _packageService.Get(packageId, packageVersion, true);
            if (packagePart == null) {
                Logger.Information("Could not Publish Package {0}, Version {1} because it does not exist.");
                return;
            }
            if (packagePart.ContentItem.IsPublished()) {
                return;
            }

            _contentManager.Publish(packagePart.ContentItem);
        }
    }
}