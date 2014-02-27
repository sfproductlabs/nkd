using Orchard.ContentManagement;

namespace Orchard.Gallery.Models.ViewModels {
    public class HighestRatedPackageWidgetViewModel {
        public string PackageId { get; set; }
        public string PackageType { get; set; }
        public double AverageRating { get; set; }
        public int NumberOfRatings { get; set; }
        public ContentItem ContentItem { get; set; }
    }
}