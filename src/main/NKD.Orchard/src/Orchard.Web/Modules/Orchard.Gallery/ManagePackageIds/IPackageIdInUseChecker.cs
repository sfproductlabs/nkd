namespace Orchard.Gallery.ManagePackageIds {
    public interface IPackageIdInUseChecker : IDependency {
        bool IsPackageIdInUse(string packageId);
    }
}