using System;
using System.Web;
using System.Web.Mvc;
using Contrib.Taxonomies.Models;
using Contrib.Taxonomies.Services;
using Orchard.DisplayManagement;
using Orchard.Gallery.Attributes;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.ViewModels;
using Orchard.Indexing;
using Orchard.Settings;
using Orchard.Tags.Models;
using Orchard.Themes;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.UI.Navigation;

namespace Orchard.Gallery.Controllers {
    [Themed]
    public class PackagesController : Controller {
        private readonly IOrchardServices _orchardServices;
        private readonly ITaxonomyService _taxonomyService;
        private readonly ISiteService _siteService;
        private readonly IIndexManager _indexManager;
        private readonly ICategoryGetter _categoryGetter;

        const string ALL_CATEGORIES = "All Categories";
        private const string SORT_BY_DOWNLOAD_COUNT = "package-download-count";
        private const string SORT_BY_ALPHABET = "package-title";
        private const string SORT_BY_RELEVANCE = "sort-by-relevance";

        public PackagesController(IOrchardServices orchardServices, ITaxonomyService taxonomyService, IShapeFactory shapeFactory, ISiteService siteService,
            IIndexManager indexManager, ICategoryGetter categoryGetter)
        {
            _orchardServices = orchardServices;
            _taxonomyService = taxonomyService;
            _siteService = siteService;
            _indexManager = indexManager;
            _categoryGetter = categoryGetter;

            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }

        ISearchBuilder Search()
        {
            return _indexManager.HasIndexProvider()
                ? _indexManager.GetSearchIndexProvider().CreateSearchBuilder("Search")
                : new NullSearchBuilder();
        }

        [HttpGet]
        [StoreLastVisitedPackageList]
        public ActionResult List(string packageType, PagerParameters pagerParameters) {
            var viewModel = BuildList(string.Empty, packageType, string.Empty, pagerParameters, SORT_BY_DOWNLOAD_COUNT);
            if (viewModel == null) {
                throw new HttpException(404, "");
            }
            viewModel.SearchTarget = packageType;
            return View(viewModel);
        }

        [HttpGet]
        [StoreLastVisitedPackageList]
        public ActionResult Search(string searchTerm, string packageType, string searchCategory, PagerParameters pagerParameters, string sortOrder) {
            string category = searchCategory == ALL_CATEGORIES ? string.Empty : searchCategory;

            var viewModel = BuildList(searchTerm, packageType, category, pagerParameters, sortOrder);
            if (viewModel == null) {
                throw new HttpException(404, "");
            }

            viewModel.SearchTerm = searchTerm;
            viewModel.SearchTarget = searchCategory;
            if (string.IsNullOrWhiteSpace(searchTerm) && (searchCategory == ALL_CATEGORIES || string.IsNullOrWhiteSpace(searchCategory))) {
                viewModel.SearchTarget = packageType;
            }

            return View("List",viewModel);
        }

        private PackageListViewModel BuildList(string searchTerm, string packageType, string searchCategory, PagerParameters pagerParameters, string sortOrder) {
            TempData["SortOrder"] = sortOrder;
            ISite siteSettings = _siteService.GetSiteSettings();
            TempData["PageSize"] = (pagerParameters != null && pagerParameters.PageSize.HasValue) ? pagerParameters.PageSize.Value : siteSettings.PageSize;
            Pager pager = new Pager(siteSettings, pagerParameters);

            var packageTypeTerm = GetPackageTypeTerm(packageType);
            if (packageTypeTerm == null) {
                return null;
            }

            int termId = packageTypeTerm.Id;

            if (!string.IsNullOrWhiteSpace(searchCategory)) {
                var categoryTerm = GetPackageCategoryTerm(packageTypeTerm, searchCategory);
                if (categoryTerm == null) {
                    return null;
                }
                termId = categoryTerm.Id;
            }

            ISearchBuilder searchBuilder = CreateSearchBuilder(sortOrder, packageType, searchTerm, termId);
            var searchHits = searchBuilder.Search().Select(hit => hit.ContentItemId);
            int totalPackageCount = searchHits.Count();
            int startingNumber = pager.GetStartIndex();

            var itemIds = searchHits
                .Skip(startingNumber)
                .Take(pager.PageSize);

            var contentItems = itemIds
                .Select(cid => _orchardServices.ContentManager.Get(cid))
                .Where(ci => ci != null)
                .ToList();

            var list = Shape.List();
            foreach (var contentItem in contentItems)
            {
                var shape = _orchardServices.ContentManager.BuildDisplay(contentItem, "Summary");
                shape.HasTags = contentItem.As<TagsPart>().CurrentTags.Count() > 0;
                list.Add(shape);
            }

            var categoriesList = _categoryGetter.GetCategoriesWithAssociatedContentItems(packageTypeTerm);

            string selectedCategory = string.IsNullOrWhiteSpace(searchCategory) ? ALL_CATEGORIES : searchCategory;
            string displaySearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? String.Format("Search {0}", packageTypeTerm.Slug) : searchTerm;
            return new PackageListViewModel
            {
                PackageType = packageType,
                TotalNumberOfPackages = totalPackageCount,
                StartingNumber = startingNumber + 1,
                EndingNumber = totalPackageCount < startingNumber + pager.PageSize ? totalPackageCount : startingNumber + pager.PageSize,
                List = list,
                Pager = Shape.Pager(pager).TotalItemCount(totalPackageCount),
                SelectedCategory = selectedCategory,
                CategorySettings = new CategoriesSidebarViewModel { PackageType = packageType, Categories = categoriesList, SelectedCategory = selectedCategory },
                SearchSettings = new SearchViewModel { SearchTerm = displaySearchTerm, PackageType = packageTypeTerm.Slug,
                    Categories = categoriesList, SelectedCategory = selectedCategory }
            };
        }

        private ISearchBuilder CreateSearchBuilder(string sortOrder, string packageType, string searchTerm, int taxonomyTermId) {
            ISearchBuilder searchBuilder = Search();
            if (searchTerm == null) searchTerm = "";
            string[] searchTermParts = searchTerm.Split(":".ToCharArray(), 2);
            if (searchTermParts[0].Trim() == "author" && searchTermParts.Count() > 1) {
                searchBuilder = searchBuilder.WithField("package-authors", searchTermParts[1].Trim());
            } else if (searchTermParts[0].Trim() == "tag" && searchTermParts.Count() > 1) {
                searchBuilder = searchBuilder.WithField("tags", searchTermParts[1].Trim());
            } else if (!string.IsNullOrWhiteSpace(searchTerm)) {
                searchBuilder = searchBuilder.Parse(
                    new[] {"package-id", "package-authors", "package-summary", "package-description", "title", "tags"}, searchTerm);
            }
            searchBuilder = searchBuilder.WithField("is-recommended-version", true).Mandatory().AsFilter();
            if (!string.IsNullOrWhiteSpace(packageType)) {
                searchBuilder = searchBuilder.WithField("term-id", taxonomyTermId).ExactMatch().Mandatory().AsFilter();
            }
            if (sortOrder != SORT_BY_RELEVANCE) {
                searchBuilder = searchBuilder.SortBy(sortOrder);
                if (sortOrder == SORT_BY_ALPHABET) {
                    searchBuilder = searchBuilder.Ascending();
                }
            }
            return searchBuilder;
        }

        private TermPart GetPackageTypeTerm(string packageType) {
            var taxonomy = _taxonomyService.GetTaxonomyBySlug("PackageTypes");
            //TODO: Check
            return _taxonomyService.GetTermByName(taxonomy.Id, packageType);
        }

        private TermPart GetPackageCategoryTerm(TermPart packageTypeTerm, string packageCategory) {
            return _taxonomyService.GetChildren(packageTypeTerm).Where(t => t.Name == packageCategory).FirstOrDefault();
        }

        [HttpGet]
        [StoreLastVisitedPackageList]
        public ActionResult ByCategory(string packageType, string categoryName, PagerParameters pagerParameters) {
            var viewModel = BuildList(string.Empty, packageType, categoryName, pagerParameters, SORT_BY_DOWNLOAD_COUNT);
            if (viewModel == null) {
                throw new HttpException(404, "");
            }
            viewModel.SearchTarget = categoryName;

            return View("List", viewModel);
        }
    }
}