using JetBrains.Annotations;
using Orchard.Gallery.Interfaces;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace Orchard.Gallery {
    [UsedImplicitly]
    public class AdminMenu : INavigationProvider {
        private readonly IPackageService _packageService;

        public AdminMenu(IPackageService packageService) {
            _packageService = packageService;
        }

        public string MenuName {
            get { return "admin"; }
        }

        public Localizer T { get; set; }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Gallery"), "1", BuildMenu);
        }

        private void BuildMenu(NavigationItemBuilder menu) {
            var values = new { area = "Orchard.Gallery" };
            const string packageAdminControllerName = "PackageAdmin";

            if (_packageService.CountOfPackages() > 0) {
                menu.Add(T("List Packages"), "1.1", item => item.Action("List", packageAdminControllerName, values));
            }
        }
    }
}