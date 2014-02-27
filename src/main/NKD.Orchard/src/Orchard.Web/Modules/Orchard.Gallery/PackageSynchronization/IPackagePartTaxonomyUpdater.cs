using Orchard.Gallery.GalleryServer;
using Orchard.Gallery.Models;

namespace Orchard.Gallery.PackageSynchronization {
    public interface IPackagePartTaxonomyUpdater : IDependency {
        void UpdatePackageTaxonomy(PublishedPackage publishedPackage, PackagePart packagePart);
    }
}