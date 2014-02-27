using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using JetBrains.Annotations;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;

namespace Orchard.Gallery.Controllers
{
    public class PackageAuthenticationController : Controller {
        private readonly IUserkeyPackageService _userkeyPackageService;
        private readonly IUserkeyService _userKeyService;
        private readonly IAdminPackagePrivilegeChecker _packagePrivilegeChecker;
        private readonly IParameterFormatValidator _parameterFormatValidator;

        public PackageAuthenticationController(IUserkeyPackageService userkeyPackageService, IUserkeyService userKeyService,
            IAdminPackagePrivilegeChecker packagePrivilegeChecker, IParameterFormatValidator parameterFormatValidator) {
            _userkeyPackageService = userkeyPackageService;
            _packagePrivilegeChecker = packagePrivilegeChecker;
            _parameterFormatValidator = parameterFormatValidator;
            _userKeyService = userKeyService;
        }

        [HttpGet]
        [UsedImplicitly]
        public JsonResult ValidatePackageKey(string key, string packageId, string packageVersion) {
            ValidateParameterFormatsForValidatePackageKey(key, packageId, packageVersion);

            Userkey userkey = _userKeyService.GetUserkey(key);
            bool keyCanAccessPackage = false;
            if (userkey != null) {
                keyCanAccessPackage = _packagePrivilegeChecker.UserCanManageAllPackages(userkey.UserId)
                    || _userkeyPackageService.KeyCanAccessPackage(packageId, key);
            }
            return new JsonResult { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = keyCanAccessPackage };
        }

        [HttpPost]
        [UsedImplicitly]
        public JsonResult AuthorizePackageIds(string key, IEnumerable<string> packageIds) {
            ValidateParameterFormatsForAuthorizePackageIds(key, packageIds);

            if (packageIds == null || !packageIds.Any()) {
                return new JsonResult { Data = true };
            }
            Userkey userkey = _userKeyService.GetUserkey(key);
            if (_packagePrivilegeChecker.UserCanManageAllPackages(userkey.UserId)) {
                return new JsonResult {Data = true};
            }
            bool userCanAccessAllPackages = packageIds.All(packageId => _userkeyPackageService.KeyCanAccessPackage(packageId, key, false));
            return new JsonResult { Data = userCanAccessAllPackages };
        }

        private void ValidateParameterFormatsForAuthorizePackageIds(string key, IEnumerable<string> packageIds) {
            _parameterFormatValidator.ValidateUserKeyFormat(key);
            foreach (var packageId in packageIds) {
                _parameterFormatValidator.ValidatePackageIdFormat(packageId);
            }
        }

        private void ValidateParameterFormatsForValidatePackageKey(string key, string packageId, string packageVersion) {
            _parameterFormatValidator.ValidatePackageIdFormat(packageId);
            _parameterFormatValidator.ValidatePackageVersionFormat(packageVersion);
            _parameterFormatValidator.ValidateUserKeyFormat(key);
        }
    }
}