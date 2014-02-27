using Orchard.Gallery.GalleryServer;
using Orchard.Gallery.Models;

namespace Orchard.Gallery.Interfaces {
    public interface IPackagePartMapper : IDependency {
        void MapPublishedPackageToPackagePart(PublishedPackage publishedPackage, PackagePart packagePart);
    }
}