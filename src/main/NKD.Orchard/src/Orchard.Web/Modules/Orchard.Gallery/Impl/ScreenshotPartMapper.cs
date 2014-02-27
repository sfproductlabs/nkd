using JetBrains.Annotations;
using Orchard.Gallery.GalleryServer;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;

namespace Orchard.Gallery.Impl {
    [UsedImplicitly]
    public class ScreenshotPartMapper : IScreenshotPartMapper {
        public void MapPublishedScreenshotToScreenshotPart(PublishedScreenshot publishedScreenshot, ScreenshotPart screenshotPart) {
            screenshotPart.PackageID = publishedScreenshot.PublishedPackageId;
            screenshotPart.PackageVersion = publishedScreenshot.PublishedPackageVersion;
            screenshotPart.ScreenshotUri = publishedScreenshot.ScreenshotUri;
        }
    }
}