using System.Linq;
using JetBrains.Annotations;
using Orchard.Gallery.Exceptions;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;
using Orchard.Security;

namespace Orchard.Gallery.ManagePackageIds {
    [UsedImplicitly]
    public class PackageIdInUseChecker : IPackageIdInUseChecker {
        private readonly IPackageService _packageService;
        private readonly IODataContext _oDataContext;
        private readonly IGalleryPackageService _galleryPackageService;
        private readonly IUserkeyService _userkeyService;
        private readonly IAuthenticationService _authenticationService;

        public PackageIdInUseChecker(IPackageService packageService, IODataContext oDataContext, IGalleryPackageService galleryPackageService,
            IUserkeyService userkeyService, IAuthenticationService authenticationService) {
            _packageService = packageService;
            _oDataContext = oDataContext;
            _galleryPackageService = galleryPackageService;
            _userkeyService = userkeyService;
            _authenticationService = authenticationService;
        }

        public bool IsPackageIdInUse(string packageId) {
            var authenticatedUser = _authenticationService.GetAuthenticatedUser();
            Userkey accessKeyForUser = _userkeyService.GetAccessKeyForUser(authenticatedUser.Id, false);
            if (accessKeyForUser == null) {
                throw new UserDoesNotHaveAccessToPackageException(authenticatedUser.Id, packageId);
            }
            return _packageService.PackageIdExists(packageId) || _oDataContext.Packages.Where(p => p.Id == packageId).ToList().Any()
                || _galleryPackageService.GetUnfinishedPackages(new[] {packageId}, accessKeyForUser.AccessKey).Any();
        }
    }
}