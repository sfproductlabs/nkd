using Orchard.ContentManagement.Records;

namespace Orchard.Gallery.Models {
    public class ScreenshotPartRecord : ContentPartRecord {
        public virtual string PackageID { get; set; }
        public virtual string PackageVersion { get; set; }
        public virtual string ScreenshotUri { get; set; }
    }
}