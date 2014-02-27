using Gallery.Core.Domain;

namespace Orchard.Gallery.PackageSynchronization {
    public interface IPackagePartUpdater : IDependency {
        void ModifyExistingPackagePart(PackageLogEntry log, bool updateExtendedPackageInfo = false);
    }
}