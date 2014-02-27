namespace Orchard.Gallery.PackageSynchronization {
    public interface IScreenshotPartDeleter : IDependency {
        void DeleteScreenshotsForPackage(string packageId, string packageVersion);
    }
}