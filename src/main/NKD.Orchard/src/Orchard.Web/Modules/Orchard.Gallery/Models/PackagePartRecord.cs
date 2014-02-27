using System;
using Orchard.ContentManagement.Records;

namespace Orchard.Gallery.Models {
    public class PackagePartRecord : ContentPartRecord {
        public virtual string PackageID { get; set; }
        public virtual string PackageVersion { get; set; }
        public virtual string Description { get; set; }
        public virtual string Summary { get; set; }
        public virtual string Authors { get; set; }
        public virtual string DownloadUrl { get; set; }
        public virtual int DownloadCount { get; set; }
        public virtual int TotalDownloadCount { get; set; }
        public virtual string Copyright { get; set; }
        public virtual string ProjectUrl { get; set; }
        public virtual string LicenseUrl { get; set; }
        public virtual string IconUrl { get; set; }
        public virtual string PackageHashAlgorithm { get; set; }
        public virtual string PackageHash { get; set; }
        public virtual long PackageSize { get; set; }
        public virtual double RatingAverage { get; set; }
        public virtual int RatingsCount { get; set; }
        public virtual string ExternalPackageUrl { get; set; }
        public virtual string ReportAbuseUrl { get; set; }
        public virtual decimal Price { get; set; }
        public virtual DateTime? Created { get; set; }
        public virtual DateTime? Published { get; set; }
        public virtual DateTime? LastUpdated { get; set; }
        public virtual bool IsRecommendedVersion { get; set; }
    }
}