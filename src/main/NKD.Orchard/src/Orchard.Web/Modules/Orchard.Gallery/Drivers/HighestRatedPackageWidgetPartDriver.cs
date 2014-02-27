using System.Collections.Generic;
using System.Linq;
using Contrib.Reviews.Models;
using Contrib.Taxonomies.Helpers;
using Contrib.Taxonomies.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Gallery.Models;
using Orchard.Gallery.Models.ViewModels;

namespace Orchard.Gallery.Drivers {
    public class HighestRatedPackageWidgetPartDriver : ContentPartDriver<HighestRatedPackageWidgetPart> {
        private readonly IContentManager _contentManager;
        private readonly ITaxonomyService _taxonomyService;

        public HighestRatedPackageWidgetPartDriver(IContentManager contentManager, ITaxonomyService taxonomyService) {
            _contentManager = contentManager;
            _taxonomyService = taxonomyService;
        }

        protected override DriverResult Display(HighestRatedPackageWidgetPart part, string displayType, dynamic shapeHelper) {
            IEnumerable<PackagePart> mostPopularPackageParts = _contentManager.Query<PackagePart, PackagePartRecord>()
                .Where(p => p.IsRecommendedVersion).Slice(5);
            IEnumerable<HighestRatedPackageWidgetViewModel> packageWidgetViewModels =
                from p in mostPopularPackageParts
                let currentVotingResult = p.As<ReviewsPart>().Rating.CurrentVotingResult
                let averageRating = currentVotingResult != null ? currentVotingResult.Value : 0
                let numberOfRatings = currentVotingResult != null ? currentVotingResult.Count : 0
                let terms = _taxonomyService.GetTermsForContentItem(p.Id)
                let packageTypeTerms = terms.Where(t => t.GetLevels() == 0)
                let packageTypeSlug = packageTypeTerms.Count() == 1 ? packageTypeTerms.First().Slug : string.Empty
                select new HighestRatedPackageWidgetViewModel
                    {PackageId = p.PackageID, PackageType = packageTypeSlug, AverageRating = averageRating, NumberOfRatings = numberOfRatings, ContentItem = p.ContentItem};

            return ContentShape("Parts_HighestRatedPackageWidget",
                () => shapeHelper.Parts_HighestRatedPackageWidget(Packages: packageWidgetViewModels));
        }
    }
}