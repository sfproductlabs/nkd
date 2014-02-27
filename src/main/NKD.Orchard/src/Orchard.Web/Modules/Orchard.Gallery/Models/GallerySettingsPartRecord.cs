using System;
using Orchard.ContentManagement.Records;

namespace Orchard.Gallery.Models {
    public class GallerySettingsPartRecord : ContentPartRecord {
        public virtual string FeedUrl { get; set; }
        public virtual string ServiceRoot { get; set; }
        public virtual int LastPackageLogId { get; set; }
        public virtual string ReportAbuseUserName { get; set; }
        public virtual DateTime? LastRatingSyncTime { get; set; }
        public virtual int? MaxNumberOfAllowedPreregisteredPackageIds { get; set; }
        public virtual int? NumberOfDaysUntilPreregisteredPackageIdExpires { get; set; }
        public virtual int? DaysInAdvanceToWarnUserOfExpiration { get; set; }
        public virtual DateTime? LastPackageIdExpirationCheckTime { get; set; }
    }
}