using System.Collections.Generic;

namespace Orchard.Gallery.ViewModels {
    public class PackageLogoAndScreenshotsViewModel {
        public string PackageId { get; set; }
        public string PackageVersion { get; set; }
        public string IconUrl { get; set; }
        public string PackageType { get; set; }
        public IEnumerable<ScreenshotViewModel> Screenshots { get; set; }
        public bool IsNewPackage { get; set; }
    }
}