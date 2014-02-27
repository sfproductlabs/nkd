using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Gallery.Models;
using Orchard.Logging;

namespace Orchard.Gallery.PackageSynchronization {
    [UsedImplicitly]
    public class ScreenshotPartDeleter : IScreenshotPartDeleter {
        private readonly IContentManager _contentManager;

        public ILogger Logger { get; set; }

        public ScreenshotPartDeleter(IContentManager contentManager) {
            _contentManager = contentManager;
            Logger = NullLogger.Instance;
        }

        public void DeleteScreenshotsForPackage(string packageId, string packageVersion)
        {
            IEnumerable<ScreenshotPart> screenshotPartsToDelete = _contentManager.Query<ScreenshotPart, ScreenshotPartRecord>()
                .Where(pp => pp.PackageID == packageId && pp.PackageVersion == packageVersion).List();
            foreach (ScreenshotPart screenshotPartToDelete in screenshotPartsToDelete) {
                _contentManager.Remove(screenshotPartToDelete.ContentItem);
            }
            Logger.Information("Deleted {0} ScreenshotPart(s) for Package '{1}', version {2}.", screenshotPartsToDelete.Count(),
                packageId, packageVersion);
        }
    }
}