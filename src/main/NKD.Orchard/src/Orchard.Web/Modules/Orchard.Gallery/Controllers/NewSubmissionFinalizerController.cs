using System;
using System.Web.Mvc;
using Orchard.Gallery.Interfaces;
using Orchard.Localization;
using Orchard.Themes;
using Orchard.UI.Notify;

namespace Orchard.Gallery.Controllers {
    [Themed]
    public class NewSubmissionFinalizerController : Controller {
        private readonly IGalleryPackagePublishingService _galleryPackagePublishingService;
        private readonly IParameterFormatValidator _parameterFormatValidator;
        private readonly IOrchardServices _orchardServices;

        public Localizer T { get; set; }

        public NewSubmissionFinalizerController(IOrchardServices orchardServices, IGalleryPackagePublishingService galleryPackagePublishingService, IParameterFormatValidator parameterFormatValidator) {
            _orchardServices = orchardServices;
            _galleryPackagePublishingService = galleryPackagePublishingService;
            _parameterFormatValidator = parameterFormatValidator;

            T = NullLocalizer.Instance;
        }

        [HttpPost]
        [Authorize]
        public ActionResult FinalizeSubmission(string packageId, string packageVersion) {
            ValidateParameterFormatsForFinalizeSubmission(packageId, packageVersion);

            try {
                _galleryPackagePublishingService.PublishPackage(packageId, packageVersion);
                _orchardServices.Notifier.Information(T("Submission complete!<br/> Your Package will appear on the Gallery website in approximately one minute."));
            }
            catch (Exception ex) {
                _orchardServices.Notifier.Error(T(ex.Message));
                return RedirectToAction("MyUnfinishedPackages", "Contribute");
            }
            return RedirectToAction("Index", "Contribute");
        }

        private void ValidateParameterFormatsForFinalizeSubmission(string packageId, string packageVersion) {
            _parameterFormatValidator.ValidatePackageIdFormat(packageId);
            _parameterFormatValidator.ValidatePackageVersionFormat(packageVersion);
        }
    }
}