using System;
using System.Net;
using JetBrains.Annotations;
using Microsoft.Http;
using Orchard.ContentManagement;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;
using Orchard.Gallery.Extensions;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Notify;

namespace Orchard.Gallery.Impl {
    [UsedImplicitly]
    public class GalleryPackagePublishingService : IGalleryPackagePublishingService {
        private readonly IOrchardServices _orchardServices;
        private readonly IUserkeyService _userkeyService;
        private readonly IAuthenticationService _authenticationService;

        public Localizer T { get; set; }
        public GalleryPackagePublishingService(IOrchardServices orchardServices, IUserkeyService userkeyService, IAuthenticationService authenticationService) {
            T = NullLocalizer.Instance;
            _orchardServices = orchardServices;
            _userkeyService = userkeyService;
            _authenticationService = authenticationService;
        }

        public void PublishPackage(string packageId, string packageVersion) {
            CallService(packageId, packageVersion, "Publish");
        }

        public void UnpublishPackage(string packageId, string packageVersion) {
            CallService(packageId, packageVersion, "Unpublish");
        }

        public void RePublishPackage(string packageId, string packageVersion) {
            CallService(packageId, packageVersion, "RePublish");
        }

        private void CallService(string packageId, string packageVersion, string serviceMethod) {
            string serviceRoot = _orchardServices.WorkContext.CurrentSite.As<GallerySettingsPart>().ServiceRoot;
            using (var client = new HttpClient(serviceRoot)) {
                string accessKey = _userkeyService.GetAccessKeyForUser(_authenticationService.GetAuthenticatedUser().Id).AccessKey.ToString();
                string uri = string.Format("{0}/{1}", ServiceConstants.PublishedPackageServiceName, serviceMethod);
                var postData = new { key = accessKey, id = packageId, version = packageVersion };
                using (HttpResponseMessage response = client.PostJson(uri, postData)) {
                    if (response.StatusCode != HttpStatusCode.OK) {
                        throw new Exception(response.ReadContentAsStringWithoutQuotes());
                    }
                }
            }
        }
    }
}