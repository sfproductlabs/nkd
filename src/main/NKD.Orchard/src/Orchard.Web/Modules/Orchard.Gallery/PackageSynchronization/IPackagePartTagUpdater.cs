using Orchard.Gallery.GalleryServer;
using Orchard.Gallery.Models;

namespace Orchard.Gallery.PackageSynchronization {
    public interface IPackagePartTagUpdater : IDependency {
        void UpdateTags(PublishedPackage publishedPackage, PackagePart packagePart);
    }
}