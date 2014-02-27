namespace Orchard.Gallery.Interfaces {
    public interface IGalleryScreenshotService : IDependency {
        void CreateScreenshot(string packageId, string packageVersion, string externalScreenshotUrl);
        void DeleteScreenshot(string idOfScreenshotToDelete);
    }
}