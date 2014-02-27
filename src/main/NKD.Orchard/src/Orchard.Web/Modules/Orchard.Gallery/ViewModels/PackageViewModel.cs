using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Orchard.Gallery.ViewModels {
    public class PackageViewModel {
        public string PackageId { get; set; }
        public string PackageVersion { get; set; }
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Summary is required")]
        public string Summary { get; set; }
        [Required(ErrorMessage = "Description is required")]
        [StringLength(4000, ErrorMessage = "Description can not be longer than 4,000 characters")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Authors is required")]
        public string Authors { get; set; }
        public string LicenseURL { get; set; }
        public string Tags { get; set; }
        public bool IsLatestVersion { get; set; }

        public string PackageType { get; set; }
        public IEnumerable<string> PackageTypes { get; set; }
        public string PrimaryCategory { get; set; }
        public IEnumerable<string> PackageCategories { get; set; }
        public string ExternalPackageUrl { get; set; }
        public string ProjectUrl { get; set; }
        public string ReportAbuseUrl { get; set; }
        public bool RequireLicenseAcceptance { get; set; }
        public string Copyright { get; set; }
        public bool IsExternalPackage { get; set; }
        public bool IsNewPackage { get; set; }

        public string RecommendedVersion { get; set; }
        public string RecommendedVersionPackageType { get; set; }
    }
}