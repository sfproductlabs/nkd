using System.Collections.Generic;
using System.Linq;
using Contrib.Taxonomies.Helpers;
using Contrib.Taxonomies.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Gallery.Models;
using Orchard.Gallery.Models.ViewModels;

namespace Orchard.Gallery.Drivers {
    public class MostPopularPackageWidgetPartDriver : ContentPartDriver<MostPopularPackageWidgetPart> {
        private readonly IContentManager _contentManager;
        private readonly ITaxonomyService _taxonomyService;

        public MostPopularPackageWidgetPartDriver(IContentManager contentManager, ITaxonomyService taxonomyService) {
            _contentManager = contentManager;
            _taxonomyService = taxonomyService;
        }

        protected override DriverResult Display(MostPopularPackageWidgetPart part, string displayType, dynamic shapeHelper) {
            IEnumerable<PackagePart> mostPopularPackageParts = _contentManager.Query<PackagePart, PackagePartRecord>()
                .Where(p => p.IsRecommendedVersion).OrderByDescending(p => p.TotalDownloadCount).Slice(5);
            IEnumerable<MostPopularPackageWidgetViewModel> packageWidgetViewModels =
                from p in mostPopularPackageParts
                let terms = _taxonomyService.GetTermsForContentItem(p.Id)
                let packageTypeTerms = terms.Where(t => t.GetLevels() == 0)
                let packageTypeSlug = packageTypeTerms.Count() == 1 ? packageTypeTerms.First().Slug : string.Empty
                let packageTypeName = packageTypeTerms.Count() == 1 ? packageTypeTerms.First().Name : string.Empty
                select new MostPopularPackageWidgetViewModel
                    {PackageId = p.PackageID, PackageTitle = p.Title, PackageTypeSlug = packageTypeSlug,
                        PackageTypeName = packageTypeName, TotalDownloadCount = p.TotalDownloadCount, IconUrl = p.IconUrl};

            return ContentShape("Parts_MostPopularPackageWidget",
                () => shapeHelper.Parts_MostPopularPackageWidget(Packages: packageWidgetViewModels));
        }
    }
}