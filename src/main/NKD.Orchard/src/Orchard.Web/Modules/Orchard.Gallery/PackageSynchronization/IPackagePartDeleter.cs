using Gallery.Core.Domain;

namespace Orchard.Gallery.PackageSynchronization {
    public interface IPackagePartDeleter : IDependency {
        void DeletePackage(PackageLogEntry log);
    }
}