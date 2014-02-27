using JetBrains.Annotations;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Gallery.Models;

namespace Orchard.Gallery.Handlers {
    [UsedImplicitly]
    public class ScreenshotPartHandler : ContentHandler {
        public ScreenshotPartHandler(IRepository<ScreenshotPartRecord> screenshotPartRepository) {
            Filters.Add(StorageFilter.For(screenshotPartRepository));

            OnRemoved<ScreenshotPart>((contentContext, part) => screenshotPartRepository.Delete(part.Record));
        }
    }
}