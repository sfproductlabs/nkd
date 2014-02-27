namespace Orchard.Gallery.ViewModels {
    public class PackageListViewModel {
        public string PackageType { get; set; }
        public int TotalNumberOfPackages { get; set; }
        public int StartingNumber { get; set; }
        public int EndingNumber { get; set; }
        public dynamic List { get; set; }
        public CategoriesSidebarViewModel CategorySettings { get; set; }
        public SearchViewModel SearchSettings { get; set; }
        public dynamic Pager { get; set; }
        public string SelectedCategory { get; set; }
        public string SearchTerm { get; set; }
        public string SearchTarget { get; set; }
    }
}