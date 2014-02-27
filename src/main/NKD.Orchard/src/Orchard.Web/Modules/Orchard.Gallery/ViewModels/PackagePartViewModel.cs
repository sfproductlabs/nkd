using System;

namespace Orchard.Gallery.ViewModels {
    public class PackagePartViewModel {
        public string PackageID { get; set; }
        public Version PackageVersion { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string PackageTypeSlug { get; set; }
        public bool IsRecommendedVersion { get; set; }
        public DateTime LastUpdated { get; set; }
        public int Downloads { get; set; }
    }
}