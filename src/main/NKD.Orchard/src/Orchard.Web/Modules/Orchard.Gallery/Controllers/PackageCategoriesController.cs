using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Contrib.Taxonomies.Helpers;
using Contrib.Taxonomies.Models;
using Contrib.Taxonomies.Services;

namespace Orchard.Gallery.Controllers {
    public class PackageCategoriesController : Controller {
        private const string PackageTaxonomyName = "Package Types";
        private readonly ITaxonomyService _taxonomyService;

        public PackageCategoriesController(ITaxonomyService taxonomyService) {
            _taxonomyService = taxonomyService;
        }

        [HttpGet]
        public ActionResult Categories(string packageType) {
            int taxonomyId = _taxonomyService.GetTaxonomyByName(PackageTaxonomyName).Id;
            IEnumerable<TermPart> parentTerms = _taxonomyService.GetTerms(taxonomyId).Where(t => t.GetLevels() == 0);

            TermPart selectedParentTerm = parentTerms.SingleOrDefault(t => t.Name == packageType);
            IEnumerable<string> categories = selectedParentTerm != null
                ? _taxonomyService.GetChildren(selectedParentTerm).OrderBy(category => category).Select(t => t.Name) : new List<string>();

            return Json(categories, JsonRequestBehavior.AllowGet);
        }
    }
}