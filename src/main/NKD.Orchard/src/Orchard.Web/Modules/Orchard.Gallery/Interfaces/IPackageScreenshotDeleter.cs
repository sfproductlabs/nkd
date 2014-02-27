namespace Orchard.Gallery.Interfaces {
    public interface IPackageScreenshotDeleter : IDependency {
        void DeletePackageScreenshot(string packageId, string packageVersion, string screenshotId, string screenshotUrl);
    }
}