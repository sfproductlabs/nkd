namespace Orchard.Gallery.Interfaces {
    public interface IGalleryPackagePublishingService : IDependency {
        void PublishPackage(string packageId, string packageVersion);
        void UnpublishPackage(string packageId, string packageVersion);
        void RePublishPackage(string packageId, string packageVersion);
    }
}