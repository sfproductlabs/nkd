namespace Orchard.Gallery.Interfaces {
    public interface IPackageScreenshotValidator : IDependency {
        void ValidateProjectScreenshot(string fileExtension);
    }
}