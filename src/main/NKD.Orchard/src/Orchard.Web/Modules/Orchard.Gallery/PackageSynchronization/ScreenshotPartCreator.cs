using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Gallery.GalleryServer;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;
using Orchard.Logging;

namespace Orchard.Gallery.PackageSynchronization {
    [UsedImplicitly]
    public class ScreenshotPartCreator : IScreenshotPartCreator {
        private readonly IScreenshotPartMapper _screenshotPartMapper;
        private readonly IOrchardServices _orchardServices;
        private readonly IODataContext _oDataContext;

        public ILogger Logger { get; set; }

        public ScreenshotPartCreator(IScreenshotPartMapper screenshotPartMapper, IOrchardServices orchardServices, IODataContext oDataContext) {
            _screenshotPartMapper = screenshotPartMapper;
            _orchardServices = orchardServices;
            _oDataContext = oDataContext;

            Logger = NullLogger.Instance;
        }

        public void AddScreenshotsForPackage(string packageId, string packageVersion) {
            IQueryable<PublishedScreenshot> publishedScreenshots = _oDataContext.Screenshots
                .Where(s => s.PublishedPackageId == packageId && s.PublishedPackageVersion == packageVersion);
            foreach (PublishedScreenshot publishedScreenshot in publishedScreenshots) {
                var screenshotPart = _orchardServices.ContentManager.New<ScreenshotPart>("Screenshot");
                _screenshotPartMapper.MapPublishedScreenshotToScreenshotPart(publishedScreenshot, screenshotPart);
                _orchardServices.ContentManager.Create(screenshotPart, VersionOptions.Published);
            }
            Logger.Information("Created {0} ScreenshotPart(s) for Package '{1}', version {2}.", publishedScreenshots.ToList().Count(), packageId, packageVersion);
        }
    }
}