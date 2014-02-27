using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Gallery.Core.Domain;
using Gallery.Core.Extensions;
using Microsoft.Http;
using Orchard.Gallery.Extensions;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Themes;
using Orchard.UI.Notify;

namespace Orchard.Gallery.Controllers {

    [Themed]
    public class UploadPackageController : Controller {
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserkeyService _userkeyService;
        private readonly IOrchardServices _orchardServices;
        private readonly IGalleryPackageService _galleryPackageService;
        private readonly IParameterFormatValidator _parameterFormatValidator;
        private readonly Lazy<int> _userId;

        public Localizer T { get; set; }

        public UploadPackageController(IAuthenticationService authenticationService, IUserkeyPackageService userkeyPackageService,
            IUserkeyService userkeyService, IOrchardServices orchardServices, IGalleryPackageService galleryPackageService, IParameterFormatValidator parameterFormatValidator) {
            _authenticationService = authenticationService;
            _galleryPackageService = galleryPackageService;
            _parameterFormatValidator = parameterFormatValidator;
            _orchardServices = orchardServices;
            _userkeyService = userkeyService;

            T = NullLocalizer.Instance;
            _userId = new Lazy<int>(() => _authenticationService.GetAuthenticatedUser().Id);
        }

        [HttpGet]
        [Authorize]
        public ActionResult Index() {
            return View();
        }

        [HttpPost]
        [Authorize]
        public ActionResult Upload(HttpPostedFileBase packageFile)
        {
            if (packageFile == null)
            {
                _orchardServices.Notifier.Error(T("Please select a package file to upload."));
                return RedirectToAction("Index");
            }
            Stream fileStream = packageFile.InputStream;
            HttpContent content = HttpContent.Create(fileStream, "application/octet-stream", null);
            string accessKey = _userkeyService.GetAccessKeyForUser(_userId.Value).AccessKey.ToString();
            string uri = string.Format("{0}/{1}/{2}", ServiceConstants.PackageFileServiceName, accessKey, packageFile.FileName.GetFileExtension());

            return CreatePackage(client => client.Post(uri, content));
        }

        [HttpPost]
        [Authorize]
        public ActionResult ExternalUrl(string externalPackageUrl) {
            ValidateParameterFormatsForExternalUrl(externalPackageUrl);

            string accessKey = _userkeyService.GetAccessKeyForUser(_userId.Value).AccessKey.ToString();
            var postData = new {key = accessKey, fileExtension = "nupkg", externalPackageUrl };
            return CreatePackage(client => client.PostJson(ServiceConstants.PackageFileServiceName + "/CreateFromExternalUrl", postData));
        }

        private void ValidateParameterFormatsForExternalUrl(string externalPackageUrl) {
            _parameterFormatValidator.ValidateUriFormat(externalPackageUrl, UriKind.Absolute);
        }

        private ActionResult CreatePackage(Func<HttpClient, HttpResponseMessage> createPackagePostMethod) {
            Package createdPackage = _galleryPackageService.CreatePackage(createPackagePostMethod);
            if (createdPackage != null) {
                return RedirectToAction("New", "Package", new { packageId = createdPackage.Id, packageVersion = createdPackage.Version });
            }
            return RedirectToAction("Index");
        }
    }
}
