namespace Orchard.Gallery.Interfaces {
    public interface IUserPackageAuthorizer : IDependency {
        bool AuthorizedToEditPackage(string packageId);
    }
}