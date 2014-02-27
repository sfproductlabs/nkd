using System;
using System.Collections.Generic;
using System.Linq;
using Contrib.Taxonomies.Helpers;
using Contrib.Taxonomies.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Gallery.Models;
using Orchard.Gallery.Models.ViewModels;

namespace Orchard.Gallery.Drivers {
    public class RecentPackageWidgetPartDriver : ContentPartDriver<RecentPackageWidgetPart> {
        private readonly IContentManager _contentManager;
        private readonly ITaxonomyService _taxonomyService;

        public RecentPackageWidgetPartDriver(IContentManager contentManager, ITaxonomyService taxonomyService) {
            _contentManager = contentManager;
            _taxonomyService = taxonomyService;
        }

        protected override DriverResult Display(RecentPackageWidgetPart part, string displayType, dynamic shapeHelper) {
            IEnumerable<PackagePart> mostPopularPackageParts = _contentManager.Query<PackagePart, PackagePartRecord>()
                .Where(p => p.IsRecommendedVersion).OrderByDescending(p => p.Created).Slice(5);
            IEnumerable<RecentPackageWidgetViewModel> packageWidgetViewModels =
                from p in mostPopularPackageParts
                let terms = _taxonomyService.GetTermsForContentItem(p.Id)
                let packageTypeTerms = terms.Where(t => t.GetLevels() == 0)
                let packageTypeSlug = packageTypeTerms.Count() == 1 ? packageTypeTerms.First().Slug : string.Empty
                let packageTypeName = packageTypeTerms.Count() == 1 ? packageTypeTerms.First().Name : string.Empty
                select new RecentPackageWidgetViewModel
                    {PackageId = p.PackageID, PackageVersion = p.PackageVersion, PackageTitle = p.Title, PackageTypeSlug = packageTypeSlug,
                        PackageTypeName = packageTypeName, IconUrl = p.IconUrl, Created = p.Created ?? DateTime.MinValue};

            return ContentShape("Parts_RecentPackageWidget",
                () => shapeHelper.Parts_RecentPackageWidget(Packages: packageWidgetViewModels));
        }
    }
}