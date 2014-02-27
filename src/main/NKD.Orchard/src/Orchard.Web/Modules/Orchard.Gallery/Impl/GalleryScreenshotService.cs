using System.Net;
using Gallery.Core.Domain;
using JetBrains.Annotations;
using Microsoft.Http;
using Orchard.ContentManagement;
using Orchard.Gallery.Extensions;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Notify;

namespace Orchard.Gallery.Impl {
    [UsedImplicitly]
    public class GalleryScreenshotService : IGalleryScreenshotService {
        private readonly IOrchardServices _orchardServices;
        private readonly IUserkeyService _userkeyService;
        private readonly IAuthenticationService _authenticationService;

        public Localizer T { get; set; }

        public GalleryScreenshotService(IOrchardServices orchardServices, IUserkeyService userkeyService,
            IAuthenticationService authenticationService) {
            T = NullLocalizer.Instance;
            _orchardServices = orchardServices;
            _userkeyService = userkeyService;
            _authenticationService = authenticationService;
        }

        public void CreateScreenshot(string packageId, string packageVersion, string externalScreenshotUrl) {
            var screenshot = new Screenshot {
                ScreenshotUri = externalScreenshotUrl,
                PackageId = packageId,
                PackageVersion = packageVersion
            };
            string serviceRoot = _orchardServices.WorkContext.CurrentSite.As<GallerySettingsPart>().ServiceRoot;
            string accessKey = _userkeyService.GetAccessKeyForUser(_authenticationService.GetAuthenticatedUser().Id).AccessKey.ToString();
            string uri = string.Format("{0}/{1}", ServiceConstants.ScreenshotServiceName, accessKey);
            using (var client = new HttpClient(serviceRoot))
            {
                HttpContent screenshotContent = HttpContentExtensions.CreateDataContract(screenshot);
                using (HttpResponseMessage response = client.Post(uri, screenshotContent)) {
                    if (response.StatusCode != HttpStatusCode.OK) {
                        _orchardServices.Notifier.Error(T("Could not create screenshot. {0}", response.ReadContentAsStringWithoutQuotes()));
                    }
                }
            }
        }

        public void DeleteScreenshot(string idOfScreenshotToDelete) {
            string serviceRoot = _orchardServices.WorkContext.CurrentSite.As<GallerySettingsPart>().ServiceRoot;
            string accessKey = _userkeyService.GetAccessKeyForUser(_authenticationService.GetAuthenticatedUser().Id).AccessKey.ToString();
            string uri = string.Format("{0}/{1}/{2}", ServiceConstants.ScreenshotServiceName, accessKey, idOfScreenshotToDelete);

            using (var client = new HttpClient(serviceRoot)) {
                using (HttpResponseMessage response = client.Delete(uri)) {
                    if (response.StatusCode != HttpStatusCode.OK) {
                        _orchardServices.Notifier.Error(T("Could not delete screenshot. {0}", response.ReadContentAsStringWithoutQuotes()));
                    }
                }
            }
        }
    }
}