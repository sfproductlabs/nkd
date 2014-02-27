using Orchard.Gallery.GalleryServer;
using Orchard.Gallery.Models;

namespace Orchard.Gallery.Interfaces {
    public interface IScreenshotPartMapper : IDependency {
        void MapPublishedScreenshotToScreenshotPart(PublishedScreenshot publishedScreenshot, ScreenshotPart screenshotPart);
    }
}