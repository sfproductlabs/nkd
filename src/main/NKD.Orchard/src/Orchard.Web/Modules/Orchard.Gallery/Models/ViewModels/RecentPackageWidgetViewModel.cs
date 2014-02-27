using System;

namespace Orchard.Gallery.Models.ViewModels {
    public class RecentPackageWidgetViewModel {
        public string PackageId { get; set; }
        public string PackageVersion { get; set; }
        public string PackageTitle { get; set; }
        public string PackageTypeSlug { get; set; }
        public string PackageTypeName { get; set; }
        public string IconUrl { get; set; }
        public DateTime Created { get; set; }
    }
}