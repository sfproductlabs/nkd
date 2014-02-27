using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Gallery.Models;
using Orchard.Localization;
using Orchard.Security;

namespace Orchard.Gallery.Drivers {

    public class GallerySettingsPartDriver : ContentPartDriver<GallerySettingsPart> {
        private const string TemplateName = "Parts/GallerySettings";
        private readonly IMembershipService _membershipService;

        public GallerySettingsPartDriver(IMembershipService membershipService) {
            T = NullLocalizer.Instance;
            _membershipService = membershipService;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "GallerySettings"; } }

        protected override DriverResult Editor(GallerySettingsPart part, dynamic shapeHelper) {
            return ContentShape("Parts_GallerySettings_Edit",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(GallerySettingsPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);

            if (string.IsNullOrWhiteSpace(part.ServiceRoot) || !Uri.IsWellFormedUriString(part.ServiceRoot, UriKind.Absolute)) {
                updater.AddModelError("ServiceRoot", T("A valid URL is required for the Gallery Server Service Root."));
            }

            if (string.IsNullOrWhiteSpace(part.FeedUrl) || !Uri.IsWellFormedUriString(part.FeedUrl, UriKind.Absolute)) {
                updater.AddModelError("FeedUrl", T("A valid URL is required for the Gallery Server Feed URL."));
            }

            if (string.IsNullOrWhiteSpace(part.ReportAbuseUserName)) {
                updater.AddModelError("ReportAbuseUserName", T("A user to report abuse to is required."));
            }
            else {
                IUser userToReportAbuseTo = _membershipService.GetUser(part.ReportAbuseUserName);
                if (userToReportAbuseTo == null || string.IsNullOrWhiteSpace(userToReportAbuseTo.Email)) {
                    updater.AddModelError("ReportAbuseUserName", T("A valid user with an e-mail address must be given to report abuse to."));
                }
            }

            if (part.MaxNumberOfAllowedPreregisteredPackageIds.HasValue && part.MaxNumberOfAllowedPreregisteredPackageIds < 0) {
                updater.AddModelError("MaxNumberOfAllowedPreregisteredPackageIds",
                    T("Max number of allowed preregistered Package IDs cannot be negative."));
            }

            if (part.NumberOfDaysUntilPreregisteredPackageIdExpires.HasValue && part.NumberOfDaysUntilPreregisteredPackageIdExpires < 0) {
                updater.AddModelError("NumberOfDaysUntilPreregisteredPackageIdExpires",
                    T("Number of days until preregistered Package ID expires cannot be negative."));
            }

            if (part.DaysInAdvanceToWarnUserOfExpiration.HasValue) {
                if (!part.NumberOfDaysUntilPreregisteredPackageIdExpires.HasValue) {
                    updater.AddModelError("DaysInAdvanceToWarnUserOfExpiration", T("Cannot set days in advance to warn user if Package IDs never expire."));
                } else if (part.DaysInAdvanceToWarnUserOfExpiration.Value >= part.NumberOfDaysUntilPreregisteredPackageIdExpires.Value) {
                    updater.AddModelError("DaysInAdvanceToWarnUserOfExpiration",
                        T("Days in advance to warn user must be smaller than days until expiration for Package IDs."));
                } else if (part.DaysInAdvanceToWarnUserOfExpiration < 1) {
                    updater.AddModelError("DaysInAdvanceToWarnUserOfExpiration",
                        T("Days in advance to warn user must either be blank or greater than 0."));
                }
            }

            return ContentShape("Parts_GallerySettings_Edit",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }
    }
}