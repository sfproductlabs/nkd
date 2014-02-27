namespace Orchard.Gallery.ViewModels {
    public class ReportAbuseViewModel {
        public string PackageId { get; set; }
        public string PackageVersion { get; set; }
        public bool IsEnabled { get; set; }
        public string UrlReferrer { get; set; }
    }
}