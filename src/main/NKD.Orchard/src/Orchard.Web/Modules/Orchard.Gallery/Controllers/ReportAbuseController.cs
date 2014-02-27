using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;
using Orchard.Gallery.ViewModels;
using Orchard.Localization;
using Orchard.Messaging.Services;
using Orchard.Security;
using Orchard.Themes;
using Orchard.UI.Notify;

namespace Orchard.Gallery.Controllers
{
    [Themed]
    public class ReportAbuseController : Controller
    {
        private readonly IOrchardServices _orchardServices;
        private readonly IMembershipService _membershipService;
        private readonly IMessageManager _messageManager;
        private readonly IPackageService _packageService;
        private readonly IParameterFormatValidator _parameterFormatValidator;
        private readonly Lazy<string> _reportAbuseUserName;
        private readonly IPackageVisitTracker _packageVisitTracker;

        public Localizer T { get; set; }

        public ReportAbuseController(IOrchardServices orchardServices, IMembershipService membershipService, IMessageManager messageManager,
            IPackageService packageService, IParameterFormatValidator parameterFormatValidator, IPackageVisitTracker packageVisitTracker) {
            _orchardServices = orchardServices;
            _packageVisitTracker = packageVisitTracker;
            _messageManager = messageManager;
            _packageService = packageService;
            _parameterFormatValidator = parameterFormatValidator;
            _membershipService = membershipService;

            _reportAbuseUserName =
                new Lazy<string>(() => _orchardServices.WorkContext.CurrentSite.As<GallerySettingsPart>().ReportAbuseUserName);

            T = NullLocalizer.Instance;
        }

        [HttpGet]
        [Authorize]
        public ActionResult Index(string packageId, string packageVersion) {
            ValidateParameterFormatsForIndex(packageId, packageVersion);

            bool isEnabled = !string.IsNullOrWhiteSpace(_reportAbuseUserName.Value);
            if (!isEnabled) {
                _orchardServices.Notifier.Error(T("Sorry, but this feature is currently disabled."));
            } else if (_packageService.Get(packageId, packageVersion) == null) {
                isEnabled = false;
                _orchardServices.Notifier.Error(T("Sorry, but Package '{0}' Version '{1}' does not exist.", packageId, packageVersion));
            }
            string urlReferrer = GetReferringUrlString();

            return View(new ReportAbuseViewModel {
                PackageId = packageId,
                PackageVersion = packageVersion,
                IsEnabled = isEnabled,
                UrlReferrer = urlReferrer
            });
        }

        private string GetReferringUrlString() {
            if (Request.UrlReferrer != null) return Request.UrlReferrer.ToString();
            if (Request.Url != null) return Request.Url.ToString();
            return "~";
        }

        [HttpPost]
        [Authorize]
        public ActionResult SendAbuseReport(string packageId, string packageVersion, string reportBody, string urlReferrer) {
            ValidateSendAbuseReportParameters(packageId, packageVersion, reportBody, urlReferrer);

            try {
                ValidateReportBody(reportBody);
                if (!string.IsNullOrWhiteSpace(_reportAbuseUserName.Value) && _packageService.Get(packageId, packageVersion) != null) {
                    IUser user = _membershipService.GetUser(_reportAbuseUserName.Value);
                    var terms = new Dictionary<string, string> {
                        {"PackageId", packageId},
                        {"PackageVersion", packageVersion},
                        {"ReportBody", HttpUtility.HtmlEncode(reportBody)},
                        {"ReporterUserName", _orchardServices.WorkContext.CurrentUser.UserName},
                        {"ReporterEmail", _orchardServices.WorkContext.CurrentUser.Email}
                    };
                    _messageManager.Send(user.ContentItem.Record, GalleryMessageTypes.ReportAbuse, "email", terms);
                    _orchardServices.Notifier.Information(T("Thank you for submitting your report."));
                }
            }
            catch (Exception ex) {
                _orchardServices.Notifier.Error(T("There was a problem with your request: {0}", ex.Message));
            }

            return new RedirectResult(_packageVisitTracker.RetrieveLastVisitedPackageDetailsLink(HttpContext).ToString());
        }

        private void ValidateSendAbuseReportParameters(string packageId, string packageVersion, string reportBody, string urlReferrer) {
            _parameterFormatValidator.ValidatePackageIdFormat(packageId);
            _parameterFormatValidator.ValidatePackageVersionFormat(packageVersion);
            _parameterFormatValidator.ValidateUriFormat(urlReferrer, UriKind.RelativeOrAbsolute);
        }

        private void ValidateParameterFormatsForIndex(string packageId, string packageVersion) {
            _parameterFormatValidator.ValidatePackageIdFormat(packageId);
            _parameterFormatValidator.ValidatePackageVersionFormat(packageVersion);
        }

        private void ValidateReportBody(string reportBody) {
            if (string.IsNullOrWhiteSpace(reportBody)) {
                throw new ArgumentException(T("Parameter is not valid.").ToString(), "reportBody");
            }
        }
    }
}