namespace Orchard.Gallery.PackageSynchronization {
    public interface IPackagePartPublishingService : IDependency {
        void Unpublish(string packageId, string packageVersion);
        void Publish(string packageId, string packageVersion);
    }
}