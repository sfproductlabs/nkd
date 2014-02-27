using Orchard.Gallery.GalleryServer;
using Orchard.Gallery.Models;

namespace Orchard.Gallery.PackageSynchronization {
    public interface IPackagePartExtendedInfoUpdater : IDependency {
        void UpdateExtendedPackageInfo(PublishedPackage publishedPackage, PackagePart packagePart);
    }
}