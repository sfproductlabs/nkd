using System.Collections.Generic;

namespace Orchard.Gallery.ViewModels {
    public class SearchViewModel {
        public string SearchTerm { get; set; }
        public string PackageType { get; set; }
        public List<string> Categories { get; set; }
        public string SelectedCategory { get; set; }
    }
}