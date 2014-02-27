using JetBrains.Annotations;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Gallery.Models;

namespace Orchard.Gallery.Handlers {
    [UsedImplicitly]
    public class GallerySettingsPartHandler : ContentHandler {
        public GallerySettingsPartHandler(IRepository<GallerySettingsPartRecord> repository) {
            Filters.Add(new ActivatingFilter<GallerySettingsPart>("Site"));
            Filters.Add(StorageFilter.For(repository));
        }
    }
}