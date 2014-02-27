using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Gallery.Core.Domain;
using Orchard.Gallery.Exceptions;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.ViewModels;
using Orchard.Localization;
using Orchard.Themes;
using Orchard.UI.Notify;

namespace Orchard.Gallery.Controllers {
    [Themed]
    public class UploadPackageLogoAndScreenshotsController : Controller {
        private readonly IGalleryPackageService _galleryPackageService;
        private readonly IOrchardServices _orchardServices;
        private readonly IUserPackageAuthorizer _userPackageAuthorizer;
        private readonly IPackageIconUploader _packageIconUploader;
        private readonly IPackageScreenshotUploader _packageScreenshotUploader;
        private readonly IPackageScreenshotDeleter _packageScreenshotDeleter;
        private readonly IParameterFormatValidator _parameterFormatValidator;

        public Localizer T { get; set; }

        public UploadPackageLogoAndScreenshotsController(IGalleryPackageService galleryPackageService, IOrchardServices orchardServices,
            IUserPackageAuthorizer userPackageAuthorizer, IPackageIconUploader packageIconUploader,
            IPackageScreenshotUploader packageScreenshotUploader, IGalleryScreenshotService galleryScreenshotService,
            IPackageScreenshotDeleter packageScreenshotDeleter, IParameterFormatValidator parameterFormatValidator) {
            _galleryPackageService = galleryPackageService;
            _packageScreenshotDeleter = packageScreenshotDeleter;
            _parameterFormatValidator = parameterFormatValidator;
            _packageScreenshotUploader = packageScreenshotUploader;
            _packageIconUploader = packageIconUploader;
            _userPackageAuthorizer = userPackageAuthorizer;
            _orchardServices = orchardServices;

            T = NullLocalizer.Instance;
        }

        [HttpGet]
        [Authorize]
        public ActionResult Edit(string packageId, string packageVersion) {
            ValidateParameterFormatsForEdit(packageId, packageVersion);
            return DisplayEditPage(packageId, packageVersion);
        }

        [HttpGet]
        [Authorize]
        public ActionResult New(string packageId, string packageVersion) {
            ValidateParameterFormatsForNew(packageId, packageVersion);
            return DisplayEditPage(packageId, packageVersion, true);
        }

        private ActionResult DisplayEditPage(string packageId, string packageVersion, bool isNewPackage = false) {
            if (!_userPackageAuthorizer.AuthorizedToEditPackage(packageId)) {
                return new HttpUnauthorizedAccessToPackageResult(packageId);
            }
            Package package = _galleryPackageService.GetPackage(packageId, packageVersion);
            IEnumerable<ScreenshotViewModel> screenshotViewModels = package.Screenshots
                .Select(s => new ScreenshotViewModel { Id = s.Id, Uri = s.ScreenshotUri });
            var packageLogoAndScreenshotsViewModel = new PackageLogoAndScreenshotsViewModel {
                PackageId = package.Id,
                PackageVersion = package.Version,
                IconUrl = package.IconUrl,
                PackageType = package.PackageType,
                Screenshots = screenshotViewModels,
                IsNewPackage = isNewPackage
            };
            return View("Index", packageLogoAndScreenshotsViewModel);
        }

        [HttpPost]
        [Authorize]
        public ActionResult AddImageForPackage(string packageId, string packageVersion, string uploadType, string addImageActionTaken,
            HttpPostedFileBase imageFile, string externalImageUrl, bool isNewPackage) {

            ValidateParameterFormatsForAddImageForPackage(packageId, packageVersion, uploadType, addImageActionTaken,
                                                    imageFile, externalImageUrl, isNewPackage);
            if (!_userPackageAuthorizer.AuthorizedToEditPackage(packageId)) {
                return new HttpUnauthorizedAccessToPackageResult(packageId);
            }
            try {
                if (uploadType == "local") {
                    if (addImageActionTaken == "Update Logo") {
                        UploadLocalPackageIcon(packageId, packageVersion, imageFile);
                    }
                    if (addImageActionTaken == "Add Screenshot") {
                        UploadLocalPackageScreenshot(packageId, packageVersion, imageFile);
                    }
                } else if (uploadType == "external") {
                    if (addImageActionTaken == "Update Logo") {
                        UploadExternalPackageIcon(packageId, packageVersion, externalImageUrl);
                    }
                    if (addImageActionTaken == "Add Screenshot") {
                        UploadExternalPackageScreenshot(packageId, packageVersion, externalImageUrl);
                    }
                }
            }
            catch (Exception ex) {
                _orchardServices.Notifier.Error(T("Image could not be added: " + ex.Message));
            }
            return GetIndexViewActionResult(packageId, packageVersion, isNewPackage);
        }

        [HttpPost]
        [Authorize]
        public ActionResult DeleteScreenshot(string packageId, string packageVersion, string idOfScreenshotToDelete, string urlOfScreenshotToDelete, bool isNewPackage) {
            string decodedUrl = HttpUtility.UrlDecode(urlOfScreenshotToDelete);

            ValidateParameterFormatsForDeleteScreenshot(packageId, packageVersion, idOfScreenshotToDelete);

            if (!_userPackageAuthorizer.AuthorizedToEditPackage(packageId)) {
                return new HttpUnauthorizedAccessToPackageResult(packageId);
            }
            try {
                _packageScreenshotDeleter.DeletePackageScreenshot(packageId, packageVersion, idOfScreenshotToDelete, decodedUrl);
                _orchardServices.Notifier.Information(T("The specified Screenshot has been removed."));
            }
            catch (Exception) {
                _orchardServices.Notifier.Error(T("The specified Screenshot could not be removed due to an error. Please try again."));
            }
            return GetIndexViewActionResult(packageId, packageVersion, isNewPackage);
        }

        private void UploadLocalPackageIcon(string packageId, string packageVersion, HttpPostedFileBase iconFile) {
            if (iconFile == null) {
                _orchardServices.Notifier.Error(T("Please select an image file to upload for your Package's icon."));
                return;
            }
            try {
                string externalIconUrl = _packageIconUploader.UploadPackageIcon(iconFile, packageId, packageVersion);
                UpdatePackageWithIconUrl(packageId, packageVersion, externalIconUrl);
                _orchardServices.Notifier.Information(T("Icon for your Package has been uploaded."));
            }
            catch (PackageIconUploadFailedException ex) {
                _orchardServices.Notifier.Error(T(ex.Message));
            }
        }

        private void UploadExternalPackageIcon(string packageId, string packageVersion, string externalIconUrl) {
            if (Uri.IsWellFormedUriString(externalIconUrl, UriKind.Absolute)) {
                UpdatePackageWithIconUrl(packageId, packageVersion, externalIconUrl);
                _orchardServices.Notifier.Information(T("Icon for your Package has been updated."));
            } else {
                _orchardServices.Notifier.Error(T("External URL given for icon was not valid."));
            }
        }

        private void UploadLocalPackageScreenshot(string packageId, string packageVersion, HttpPostedFileBase screenshotFile) {
            if (screenshotFile == null) {
                _orchardServices.Notifier.Error(T("Please select an image file to upload as a Screenshot for your Package."));
                return;
            }
            try {
                _packageScreenshotUploader.UploadPackageScreenshot(screenshotFile, packageId, packageVersion);
                _orchardServices.Notifier.Information(T("Screenshot for your Package has been added."));
            }
            catch (PackageScreenshotUploadFailedException ex) {
                _orchardServices.Notifier.Error(T(ex.Message));
            }
        }

        private void UploadExternalPackageScreenshot(string packageId, string packageVersion, string externalScreenshotUrl) {
            if (Uri.IsWellFormedUriString(externalScreenshotUrl, UriKind.Absolute)) {
                _packageScreenshotUploader.UploadPackageExternalScreenshot(packageId, packageVersion, externalScreenshotUrl);
                _orchardServices.Notifier.Information(T("Screenshot added."));
            } else {
                _orchardServices.Notifier.Error(T("External URL given for Screenshot was not valid."));
            }
        }

        private void UpdatePackageWithIconUrl(string packageId, string packageVersion, string externalIconUrl) {
            Package package = _galleryPackageService.GetPackage(packageId, packageVersion);
            package.IconUrl = externalIconUrl;
            _galleryPackageService.UpdatePackage(package);
        }

        private ActionResult GetIndexViewActionResult(string packageId, string packageVersion, bool isNewPackage) {
            string action = isNewPackage ? "New" : "Edit";
            return RedirectToAction(action, new { packageId, packageVersion });
        }

        private void ValidateParameterFormatsForEdit(string packageId, string packageVersion) {
            ValidatePackageIdAndVersionFormats(packageId, packageVersion);
        }

        private void ValidateParameterFormatsForNew(string packageId, string packageVersion) {
            ValidatePackageIdAndVersionFormats(packageId, packageVersion);
        }

        private void ValidateParameterFormatsForAddImageForPackage(string packageId, string packageVersion, string uploadType, string addImageActionTaken, HttpPostedFileBase imageFile, string externalImageUrl, bool isNewPackage) {
            ValidatePackageIdAndVersionFormats(packageId, packageVersion);
            _parameterFormatValidator.ValidateUriFormat(externalImageUrl, UriKind.Absolute);
        }

        private void ValidateParameterFormatsForDeleteScreenshot(string packageId, string packageVersion, string idOfScreenshotToDelete) {
            ValidatePackageIdAndVersionFormats(packageId, packageVersion);
            _parameterFormatValidator.ValidateScreenshotIdFormat(idOfScreenshotToDelete);
        }

        protected void ValidatePackageIdAndVersionFormats(string packageId, string packageVersion) {
            _parameterFormatValidator.ValidatePackageIdFormat(packageId);
            _parameterFormatValidator.ValidatePackageVersionFormat(packageVersion);
        }
    }
}