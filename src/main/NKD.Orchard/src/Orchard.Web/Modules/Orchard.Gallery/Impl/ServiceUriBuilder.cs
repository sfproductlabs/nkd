using System;
using JetBrains.Annotations;
using Orchard.Gallery.Interfaces;
using Orchard.Security;

namespace Orchard.Gallery.Impl {
    [UsedImplicitly]
    public class ServiceUriBuilder : IServiceUriBuilder {
        private readonly IUserkeyService _userkeyService;
        private readonly IAuthenticationService _authenticationService;

        public ServiceUriBuilder(IUserkeyService userkeyService, IAuthenticationService authenticationService) {
            _userkeyService = userkeyService;
            _authenticationService = authenticationService;
        }

        public string BuildServiceUri(string serviceName, string packageId, string packageVersion)
        {
            Guid accessKey = _userkeyService.GetAccessKeyForUser(_authenticationService.GetAuthenticatedUser().Id).AccessKey;
            return BuildServiceUri(serviceName, packageId, packageVersion, accessKey);
        }

        public string BuildServiceUri(string serviceName, string packageId, string packageVersion, Guid accessKey) {
            return string.Format("{0}/{1}/{2}/{3}", serviceName, accessKey, packageId, packageVersion);
        }
    }
}