using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement;
using Orchard.Autoroute.Models;
using Orchard.Tags.Models;
using Orchard.Core.Title.Models;

namespace Orchard.Gallery.Models {
    public class PackagePart : ContentPart<PackagePartRecord> {

        //TODO:Check
        public string Title {
            get { return this.As<TitlePart>().Title; }
            set { this.As<TitlePart>().Title = value; }
        }

        //TODO:CHECK
        public string Slug
        {
            get { return this.As<AutoroutePart>().DisplayAlias; }
            set { this.As<AutoroutePart>().DisplayAlias = value; }
        }

        public IEnumerable<TagRecord> Tags
        {
            get { return this.As<TagsPart>().CurrentTags; }
        }

        [Required]
        public string PackageID {
            get { return Record.PackageID; }
            set { Record.PackageID = value; }
        }

        public string PackageVersion {
            get { return Record.PackageVersion; }
            set { Record.PackageVersion = value; }
        }

        public string Description {
            get { return Record.Description; }
            set { Record.Description = value; }
        }

        public string Summary {
            get { return Record.Summary; }
            set { Record.Summary = value; }
        }

        public string Authors {
            get { return Record.Authors; }
            set { Record.Authors = value; }
        }

        public string DownloadUrl {
            get { return Record.DownloadUrl; }
            set { Record.DownloadUrl = value; }
        }

        public int DownloadCount {
            get { return Record.DownloadCount; }
            set { Record.DownloadCount = value; }
        }

        public int TotalDownloadCount {
            get { return Record.TotalDownloadCount; }
            set { Record.TotalDownloadCount = value; }
        }

        public string Copyright {
            get { return Record.Copyright; }
            set { Record.Copyright = value; }
        }

        public string ProjectUrl {
            get { return Record.ProjectUrl; }
            set { Record.ProjectUrl = value; }
        }

        public string LicenseUrl {
            get { return Record.LicenseUrl; }
            set { Record.LicenseUrl = value; }
        }

        public string IconUrl {
            get { return Record.IconUrl; }
            set { Record.IconUrl = value; }
        }

        public string PackageHashAlgorithm {
            get { return Record.PackageHashAlgorithm; }
            set { Record.PackageHashAlgorithm = value; }
        }

        public string PackageHash {
            get { return Record.PackageHash; }
            set { Record.PackageHash = value; }
        }

        public long PackageSize {
            get { return Record.PackageSize; }
            set { Record.PackageSize = value; }
        }

        public string ExternalPackageUrl {
            get { return Record.ExternalPackageUrl; }
            set { Record.ExternalPackageUrl = value; }
        }

        public string ReportAbuseUrl {
            get { return Record.ReportAbuseUrl; }
            set { Record.ReportAbuseUrl = value; }
        }

        public decimal Price {
            get { return Record.Price; }
            set { Record.Price = value; }
        }

        public DateTime? Created {
            get { return Record.Created; }
            set { Record.Created = value; }
        }

        public DateTime? Published {
            get { return Record.Published; }
            set { Record.Published = value; }
        }

        public DateTime? LastUpdated {
            get { return Record.LastUpdated; }
            set { Record.LastUpdated = value; }
        }

        public bool IsRecommendedVersion {
            get { return Record.IsRecommendedVersion; }
            set { Record.IsRecommendedVersion = value; }
        }
    }
}