using Contrib.Taxonomies.Helpers;
using Contrib.Taxonomies.Services;
using Orchard.ContentManagement.Drivers;
using Orchard.Gallery.Models;
using System.Linq;

namespace Orchard.Gallery.Drivers {
    public class GallerySearchWidgetPartDriver : ContentPartDriver<GallerySearchWidgetPart> {
        private readonly ITaxonomyService _taxonomyService;

        public GallerySearchWidgetPartDriver(ITaxonomyService taxonomyService) {
            _taxonomyService = taxonomyService;
        }

        protected override DriverResult Display(GallerySearchWidgetPart part, string displayType, dynamic shapeHelper) {
            var taxonomy = _taxonomyService.GetTaxonomyBySlug("PackageTypes");
            var packageTypes = _taxonomyService.GetTerms(taxonomy.Id)
                .Where(t => t.GetLevels() == 0)
                .Select(t => t.Slug)
                .OrderBy(slug => slug);

            return ContentShape("Parts_GallerySearchWidget",
                () => shapeHelper.Parts_GallerySearchWidget(PackageTypes: packageTypes));
        }
    }
}