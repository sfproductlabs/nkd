using Gallery.Core.Domain;

namespace Orchard.Gallery.PackageSynchronization {
    public interface IPackagePartCreator : IDependency {
        void CreateNewPackagePart(PackageLogEntry log);
    }
}