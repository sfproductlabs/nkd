using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Gallery.Core.Exceptions;
using JetBrains.Annotations;
using Orchard.Gallery.Exceptions;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.ManagePackageIds;
using Orchard.Gallery.Models;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Themes;
using Orchard.UI.Notify;
using Orchard.ContentManagement;

namespace Orchard.Gallery.Controllers {
    [Themed]
    public class ManagePackageIdsController : Controller {
        private readonly IRegisteredPackageIdGetter _registeredPackageIdGetter;
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserPackageAuthorizer _userPackageAuthorizer;
        private readonly IPackageIdInUseChecker _packageIdInUseChecker;
        private readonly IUserkeyPackageService _userkeyPackageService;
        private readonly IParameterFormatValidator _parameterFormatValidator;
        private readonly IPackageService _packageService;
        private readonly IOrchardServices _orchardServices;

        private readonly INotifier _notifier;
        public Localizer T { get; set; }

        public ManagePackageIdsController(IRegisteredPackageIdGetter registeredPackageIdGetter, IAuthenticationService authenticationService,
            IUserPackageAuthorizer userPackageAuthorizer, IPackageIdInUseChecker packageIdInUseChecker, IUserkeyPackageService userkeyPackageService,
            IParameterFormatValidator parameterFormatValidator, INotifier notifier, IPackageService packageService, IOrchardServices orchardServices) {
            _registeredPackageIdGetter = registeredPackageIdGetter;
            _authenticationService = authenticationService;
            _userPackageAuthorizer = userPackageAuthorizer;
            _packageIdInUseChecker = packageIdInUseChecker;
            _userkeyPackageService = userkeyPackageService;
            _parameterFormatValidator = parameterFormatValidator;
            _packageService = packageService;
            _orchardServices = orchardServices;
            _notifier = notifier;

            T = NullLocalizer.Instance;
        }

        [HttpGet]
        [Authorize]
        [UsedImplicitly]
        public ActionResult Index() {
            IUser authenticatedUser = _authenticationService.GetAuthenticatedUser();
            IEnumerable<UserkeyPackageViewModel> registeredPackageIdsForUser = _registeredPackageIdGetter.GetRegisteredPackageIdsForUser(authenticatedUser.Id);
            bool canRegisterNewPackageIds = true;
            int? maxNumberOfAllowedPreregisteredPackageIds = _orchardServices.WorkContext.CurrentSite.
                As<GallerySettingsPart>().MaxNumberOfAllowedPreregisteredPackageIds;
            if (maxNumberOfAllowedPreregisteredPackageIds.HasValue) {
                int numberOfPreregisteredIds = registeredPackageIdsForUser.Count(p => p.IsPreregistered);
                canRegisterNewPackageIds = numberOfPreregisteredIds < maxNumberOfAllowedPreregisteredPackageIds.Value;
            }
            return View(new ManagePackageIdsViewModel
                { CanRegisterNewPackageIds = canRegisterNewPackageIds, RegisteredPackageIdsForUser = registeredPackageIdsForUser});
        }

        [HttpPost]
        [Authorize]
        [UsedImplicitly]
        public ActionResult DeletePackageIdRegistration(string packageId) {
            _parameterFormatValidator.ValidatePackageIdFormat(packageId);
            if (!_userPackageAuthorizer.AuthorizedToEditPackage(packageId)) {
                return new HttpUnauthorizedAccessToPackageResult(packageId);
            }
            bool packageIdIsInUse;
            try {
                packageIdIsInUse = _packageIdInUseChecker.IsPackageIdInUse(packageId);
            }
            catch (UserDoesNotHaveAccessToPackageException) {
                return new HttpUnauthorizedAccessToPackageResult(packageId);
            }

            if (!packageIdIsInUse) {
                _userkeyPackageService.DeletePackageIdRegistration(packageId);
                _notifier.Information(T("Registration for the Package ID '{0}' has been deleted.", packageId));
            }
            else {
                _notifier.Error(T("You cannot delete this Package ID because it is currently in use."));
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize]
        [UsedImplicitly]
        public ActionResult RegisterPackageId(string packageIdToRegister) {
            TempData["PackageIdToRegister"] = packageIdToRegister;
            if (!IsPackageIdToRegisterValid(packageIdToRegister) || UserHasExceededMaxNumberOfAllowedPreregistrations()) {
                return RedirectToAction("Index");
            }
            int currentlyAuthenticatedUserId = _authenticationService.GetAuthenticatedUser().Id;
            if (_userkeyPackageService.PackageIdIsRegistered(packageIdToRegister) || _packageService.PackageIdExists(packageIdToRegister)) {
                if (_userkeyPackageService.UserCanAccessPackage(packageIdToRegister, currentlyAuthenticatedUserId)) {
                    _notifier.Warning(T("You can already submit packages with the ID '{0}'.", packageIdToRegister));
                } else {
                    _notifier.Error(T("The ID '{0}' is already in use.", packageIdToRegister));
                }
                return RedirectToAction("Index");
            }

            _userkeyPackageService.RegisterPackageId(packageIdToRegister, currentlyAuthenticatedUserId);
            _notifier.Information(T("You have successfully registered the package ID '{0}'.", packageIdToRegister));
            TempData["PackageIdToRegister"] = null;
            return RedirectToAction("Index");
        }

        private bool UserHasExceededMaxNumberOfAllowedPreregistrations() {
            int? maxNumberOfAllowedPreregisteredPackageIds = _orchardServices.WorkContext.CurrentSite.
                As<GallerySettingsPart>().MaxNumberOfAllowedPreregisteredPackageIds;
            if (maxNumberOfAllowedPreregisteredPackageIds.HasValue)
            {
                int userId = _authenticationService.GetAuthenticatedUser().Id;
                int numberOfPreregisteredPackageIdsForUser = _registeredPackageIdGetter.GetNumberOfPreregisteredPackageIdsForUser(userId);
                if (numberOfPreregisteredPackageIdsForUser >= maxNumberOfAllowedPreregisteredPackageIds.Value)
                {
                    _notifier.Error(T("You have reached you maximum number of allowed preregistered Package IDs."));
                    return true;
                }
            }
            return false;
        }

        private bool IsPackageIdToRegisterValid(string packageIdToRegister) {
            try {
                _parameterFormatValidator.ValidatePackageIdFormat(packageIdToRegister);
            }
            catch (InvalidPackageIdException) {
                _notifier.Error(T("Please enter a valid Package ID."));
                return false;
            }
            packageIdToRegister = packageIdToRegister.Trim();
            if (string.IsNullOrWhiteSpace(packageIdToRegister)) {
                _notifier.Error(T("Please enter a package ID."));
                return false;
            }
            if (packageIdToRegister.Length > 150) {
                _notifier.Error(T("Please limit your package ID to 150 characters or less."));
                return false;
            }
            return true;
        }
    }
}