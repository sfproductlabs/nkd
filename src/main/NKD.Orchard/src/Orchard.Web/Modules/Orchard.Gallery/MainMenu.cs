using Contrib.Taxonomies.Services;
using Orchard.Localization;
using Orchard.UI.Navigation;
using System.Linq;
using Contrib.Taxonomies.Helpers;

namespace Orchard.Gallery {
    public class MainMenu : INavigationProvider {
        private readonly ITaxonomyService _taxonomyService;

        public MainMenu(ITaxonomyService taxonomyService) {
            _taxonomyService = taxonomyService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public string MenuName {
            get { return "main"; }
        }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T(""), "2", BuildMenu);
        }

        private void BuildMenu(NavigationItemBuilder menu)
        {
            var taxonomy = _taxonomyService.GetTaxonomyByName("Package Types");
            var terms = _taxonomyService.GetTerms(taxonomy.Id).Where(t => t.GetLevels() == 0);

            foreach (var term in terms) {
                var routeValues = new { area = "Orchard.Gallery", packageType = term.Slug };
                menu.Add(T(term.Slug), "2", item => item.Action("List", "Packages", routeValues));
            }

            menu.Add(T("Contribute"), "3", item => item.Action("Index", "Contribute", new {area = "Orchard.Gallery"}));
        }
    }
}