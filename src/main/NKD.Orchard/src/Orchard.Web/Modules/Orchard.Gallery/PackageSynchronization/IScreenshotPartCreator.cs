namespace Orchard.Gallery.PackageSynchronization {
    public interface IScreenshotPartCreator : IDependency {
        void AddScreenshotsForPackage(string packageId, string packageVersion);
    }
}