using System.Collections.Generic;

namespace Orchard.Gallery.ViewModels {
    public class CategoriesSidebarViewModel {
        public string PackageType { get; set; }
        public List<string> Categories { get; set; }
        public string SelectedCategory { get; set; }
    }
}