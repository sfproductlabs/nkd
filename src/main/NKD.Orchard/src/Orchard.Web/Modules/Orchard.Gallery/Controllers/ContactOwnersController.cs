using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.ViewModels;
using Orchard.Localization;
using Orchard.Messaging.Services;
using Orchard.Themes;
using Orchard.UI.Notify;
using Orchard.Users.Models;

namespace Orchard.Gallery.Controllers
{
    [Themed]
    public class ContactOwnersController : Controller
    {
        private readonly IOrchardServices _orchardServices;
        private readonly IMessageManager _messageManager;
        private readonly IPackageService _packageService;
        private readonly IUserkeyPackageService _userkeyPackageService;
        private readonly IParameterFormatValidator _parameterFormatValidator;
        private readonly IPackageVisitTracker _packageVisitTracker;

        public Localizer T { get; set; }

        public ContactOwnersController(IOrchardServices orchardServices, IMessageManager messageManager, IPackageService packageService,
            IUserkeyPackageService userkeyPackageService, IParameterFormatValidator parameterFormatValidator, IPackageVisitTracker packageVisitTracker) {
            _orchardServices = orchardServices;
            _packageVisitTracker = packageVisitTracker;
            _messageManager = messageManager;
            _packageService = packageService;
            _userkeyPackageService = userkeyPackageService;
            _parameterFormatValidator = parameterFormatValidator;

            T = NullLocalizer.Instance;
        }

        [HttpGet]
        [Authorize]
        public ActionResult Index(string packageId, string urlReferrer = null) {
            ValidateParameterFormatsForIndex(packageId);
            bool isEnabled = true;
            var allContactableOwnersForPackage = _userkeyPackageService.GetContactableOwnersForPackage(packageId);

            if (!_packageService.PackageIdExists(packageId)) {
                _orchardServices.Notifier.Error(T("Sorry, but Package '{0}' does not exist.", packageId));
                isEnabled = false;
            } else if (!allContactableOwnersForPackage.Any()) {
                _orchardServices.Notifier.Error(T("Sorry, but there are no owners registered for Package '{0}'.", packageId));
                isEnabled = false;
            }
            if (string.IsNullOrWhiteSpace(urlReferrer)) {
                urlReferrer = GetReferringUrlString();
            }

            return View(new ContactOwnersViewModel {
                PackageId = packageId,
                UrlReferrer = urlReferrer,
                Owners = allContactableOwnersForPackage.Select(u => u.UserName),
                IsEnabled = isEnabled
            });
        }

        private string GetReferringUrlString() {
            if (Request.UrlReferrer != null) return Request.UrlReferrer.ToString();
            if (Request.Url != null) return Request.Url.ToString();
            return "~";
        }

        [HttpPost]
        [Authorize]
        public ActionResult ContactOwners(string packageId, string reportBody, string urlReferrer) {
            ValidateContactOwnersParameters(packageId, urlReferrer);

            try {
                ValidateReportBody(reportBody);
                var terms = new Dictionary<string, string> {
                    {"PackageId", packageId},
                    {"ReportBody", HttpUtility.HtmlEncode(reportBody)},
                    {"ReporterUserName", _orchardServices.WorkContext.CurrentUser.UserName},
                    {"ReporterEmail", _orchardServices.WorkContext.CurrentUser.Email}
                };
                foreach (var owner in _userkeyPackageService.GetContactableOwnersForPackage(packageId)) {
                    _messageManager.Send(owner.ContentItem.Record, GalleryMessageTypes.ContactOwners, "email", terms);
                }
                _orchardServices.Notifier.Information(T("Your message has been sent to the owners of {0}.", packageId));
            }
            catch (Exception ex) {
                _orchardServices.Notifier.Error(T(ex.Message));
                TempData["ReportBody"] = reportBody;
                return RedirectToAction("Index", new {packageId, urlReferrer});
            }

            return new RedirectResult(_packageVisitTracker.RetrieveLastVisitedPackageDetailsLink(HttpContext).ToString());
        }

        private void ValidateContactOwnersParameters(string packageId, string urlReferrer) {
            _parameterFormatValidator.ValidatePackageIdFormat(packageId);
            _parameterFormatValidator.ValidateUriFormat(urlReferrer, UriKind.RelativeOrAbsolute);
        }

        private void ValidateParameterFormatsForIndex(string packageId) {
            _parameterFormatValidator.ValidatePackageIdFormat(packageId);
        }

        private void ValidateReportBody(string reportBody) {
            if (string.IsNullOrWhiteSpace(reportBody)) {
                throw new Exception(T("Please enter a message to send.").Text);
            }
            const int maxLength = 4000;
            if (reportBody.Length > maxLength) {
                throw new Exception(T("Your message is too long. Please enter a message no greater than {0} characters.", maxLength).Text);
            }
        }
    }
}