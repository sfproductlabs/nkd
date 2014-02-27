using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;

namespace Orchard.Gallery.Models {
    public class ScreenshotPart : ContentPart<ScreenshotPartRecord> {
        [Required]
        public string PackageID
        {
            get { return Record.PackageID; }
            set { Record.PackageID = value; }
        }

        [Required]
        public string PackageVersion
        {
            get { return Record.PackageVersion; }
            set { Record.PackageVersion = value; }
        }

        public string ScreenshotUri
        {
            get { return Record.ScreenshotUri; }
            set { Record.ScreenshotUri = value; }
        }
    }
}