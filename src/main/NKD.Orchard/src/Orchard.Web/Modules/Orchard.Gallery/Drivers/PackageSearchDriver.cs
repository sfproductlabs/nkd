using Orchard.ContentManagement.Drivers;
using Orchard.Gallery.Models;

namespace Orchard.Gallery.Drivers {
    public class PackageSearchDriver : ContentPartDriver<PackageSearchPart> {
        protected override DriverResult Display(PackageSearchPart part, string displayType, dynamic shapeHelper) {
            return ContentShape("Parts_PackageSearch",
                         () => shapeHelper.Parts_PackageSearch());
        }
    }
}