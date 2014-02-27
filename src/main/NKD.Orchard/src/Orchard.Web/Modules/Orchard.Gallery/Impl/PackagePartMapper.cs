using Gallery.Core.Impl;
using Gallery.Core.Interfaces;
using JetBrains.Annotations;
using Orchard.Gallery.GalleryServer;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;

namespace Orchard.Gallery.Impl {
    [UsedImplicitly]
    public class PackagePartMapper : IPackagePartMapper {
        private readonly IPackageSlugCreator _packageSlugCreator;

        public PackagePartMapper()
            : this(new PackageSlugCreator())
        { }

        public PackagePartMapper(IPackageSlugCreator packageSlugCreator) {
            _packageSlugCreator = packageSlugCreator;
        }

        public void MapPublishedPackageToPackagePart(PublishedPackage publishedPackage, PackagePart packagePart) {
            packagePart.PackageID = publishedPackage.Id;
            packagePart.PackageVersion = publishedPackage.Version;
            packagePart.Title = string.IsNullOrWhiteSpace(publishedPackage.Title) ? publishedPackage.Id : publishedPackage.Title;
            packagePart.Description = publishedPackage.Description;
            packagePart.Summary = publishedPackage.Summary;

            packagePart.Slug = _packageSlugCreator.CreateSlug(packagePart.PackageID, packagePart.PackageVersion);

            packagePart.Authors = publishedPackage.Authors;
            packagePart.DownloadCount = publishedPackage.VersionDownloadCount;
            packagePart.TotalDownloadCount = publishedPackage.DownloadCount;
            packagePart.Copyright = publishedPackage.Copyright;
            packagePart.ProjectUrl = publishedPackage.ProjectUrl;
            packagePart.LicenseUrl = publishedPackage.LicenseUrl;
            packagePart.IconUrl = publishedPackage.IconUrl;
            packagePart.PackageHashAlgorithm = publishedPackage.PackageHashAlgorithm;
            packagePart.PackageHash = publishedPackage.PackageHash;
            packagePart.PackageSize = publishedPackage.PackageSize;
            packagePart.ExternalPackageUrl = publishedPackage.ExternalPackageUrl;
            packagePart.ReportAbuseUrl = publishedPackage.ReportAbuseUrl;
            packagePart.LastUpdated = publishedPackage.LastUpdated;
            packagePart.Price = publishedPackage.Price;
            packagePart.Created = publishedPackage.Created;
            packagePart.Published = publishedPackage.Published;
            packagePart.IsRecommendedVersion = publishedPackage.IsLatestVersion;
        }
    }
}