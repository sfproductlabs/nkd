using Orchard.Security;

namespace Orchard.Gallery.Interfaces {
    public interface IPackagePrivilegeChecker : IDependency {
        bool UserCanManagePackage(IUser user, string packageId);
    }
}