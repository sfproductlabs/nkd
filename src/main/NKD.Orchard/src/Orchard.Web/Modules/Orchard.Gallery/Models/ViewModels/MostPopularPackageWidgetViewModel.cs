namespace Orchard.Gallery.Models.ViewModels {
    public class MostPopularPackageWidgetViewModel {
        public string PackageId { get; set; }
        public string PackageTitle { get; set; }
        public string PackageTypeSlug { get; set; }
        public string PackageTypeName { get; set; }
        public int TotalDownloadCount { get; set; }
        public string IconUrl { get; set; }
    }
}