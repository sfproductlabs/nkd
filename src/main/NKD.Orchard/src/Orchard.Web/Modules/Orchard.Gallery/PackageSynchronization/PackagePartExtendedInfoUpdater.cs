using JetBrains.Annotations;
using Orchard.Gallery.GalleryServer;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;

namespace Orchard.Gallery.PackageSynchronization {
    [UsedImplicitly]
    public class PackagePartExtendedInfoUpdater : IPackagePartExtendedInfoUpdater {
        private readonly IODataContext _oDataContext;
        private readonly IScreenshotPartCreator _screenshotPartCreator;
        private readonly IScreenshotPartDeleter _screenshotPartDeleter;
        private readonly IPackagePartTagUpdater _packagePartTagUpdater;
        private readonly IPackagePartTaxonomyUpdater _packagePartTaxonomyUpdater;

        public PackagePartExtendedInfoUpdater(IODataContext oDataContext, IScreenshotPartCreator screenshotPartCreator,
            IScreenshotPartDeleter screenshotPartDeleter, IPackagePartTagUpdater packagePartTagUpdater,
            IPackagePartTaxonomyUpdater packagePartTaxonomyUpdater) {
            _oDataContext = oDataContext;
            _screenshotPartCreator = screenshotPartCreator;
            _screenshotPartDeleter = screenshotPartDeleter;
            _packagePartTagUpdater = packagePartTagUpdater;
            _packagePartTaxonomyUpdater = packagePartTaxonomyUpdater;
        }

        public void UpdateExtendedPackageInfo(PublishedPackage publishedPackage, PackagePart packagePart) {
            packagePart.DownloadUrl = _oDataContext.GetDownloadUrl(publishedPackage);
            _packagePartTagUpdater.UpdateTags(publishedPackage, packagePart);
            _packagePartTaxonomyUpdater.UpdatePackageTaxonomy(publishedPackage, packagePart);
            _screenshotPartDeleter.DeleteScreenshotsForPackage(publishedPackage.Id, publishedPackage.Version);
            _screenshotPartCreator.AddScreenshotsForPackage(publishedPackage.Id, publishedPackage.Version);
        }
    }
}