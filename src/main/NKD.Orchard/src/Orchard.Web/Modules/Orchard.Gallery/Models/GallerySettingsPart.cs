using System;
using Orchard.ContentManagement;

namespace Orchard.Gallery.Models {
    public class GallerySettingsPart : ContentPart<GallerySettingsPartRecord> {
        public string FeedUrl {
            get { return Record.FeedUrl; }
            set { Record.FeedUrl = value; }
        }

        public string ServiceRoot {
            get {
                string serviceRoot = Record.ServiceRoot;
                if (!string.IsNullOrWhiteSpace(serviceRoot) && !serviceRoot.EndsWith("/")) {
                    serviceRoot += "/";
                }
                return serviceRoot;
            }
            set { Record.ServiceRoot = value; }
        }

        public int LastPackageLogId {
            get { return Record.LastPackageLogId; }
            set { Record.LastPackageLogId = value; }
        }

        public string ReportAbuseUserName
        {
            get { return Record.ReportAbuseUserName; }
            set { Record.ReportAbuseUserName = value; }
        }

        public DateTime? LastRatingSyncTime {
            get { return Record.LastRatingSyncTime; }
            set { Record.LastRatingSyncTime = value; }
        }

        public int? MaxNumberOfAllowedPreregisteredPackageIds {
            get { return Record.MaxNumberOfAllowedPreregisteredPackageIds; }
            set { Record.MaxNumberOfAllowedPreregisteredPackageIds = value; }
        }

        public int? NumberOfDaysUntilPreregisteredPackageIdExpires {
            get { return Record.NumberOfDaysUntilPreregisteredPackageIdExpires; }
            set { Record.NumberOfDaysUntilPreregisteredPackageIdExpires = value; }
        }

        public int? DaysInAdvanceToWarnUserOfExpiration {
            get { return Record.DaysInAdvanceToWarnUserOfExpiration; }
            set { Record.DaysInAdvanceToWarnUserOfExpiration = value; }
        }

        public DateTime? LastPackageIdExpirationCheckTime {
            get { return Record.LastPackageIdExpirationCheckTime; }
            set { Record.LastPackageIdExpirationCheckTime = value; }
        }
    }
}