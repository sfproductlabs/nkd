using System.Collections.Generic;
using System.Web.Mvc;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.ViewModels;
using Orchard.Localization;
using Orchard.Security;
using Orchard.Themes;
using Orchard.UI.Notify;
using System.Linq;

namespace Orchard.Gallery.Controllers {
    [Themed]
    public class ManagePackageOwnersController : Controller {
        private readonly IUserkeyPackageService _userkeyPackageService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IMembershipService _membershipService;
        private readonly IOrchardServices _orchardServices;
        private readonly IUserPackageAuthorizer _userPackageAuthorizer;
        private readonly IParameterFormatValidator _parameterFormatValidator;

        public Localizer T { get; set; }

        private IUser _authenticatedUser;
        private IUser AuthenticatedUser {
            get { return _authenticatedUser ?? (_authenticatedUser = _authenticationService.GetAuthenticatedUser()); }
        }

        public ManagePackageOwnersController(IUserkeyPackageService userkeyPackageService, IAuthenticationService authenticationService,
            IMembershipService membershipService, IOrchardServices orchardServices,
            IUserPackageAuthorizer userPackageAuthorizer, IParameterFormatValidator parameterFormatValidator) {
            _userkeyPackageService = userkeyPackageService;
            _userPackageAuthorizer = userPackageAuthorizer;
            _parameterFormatValidator = parameterFormatValidator;
            _orchardServices = orchardServices;
            _membershipService = membershipService;
            _authenticationService = authenticationService;

            T = NullLocalizer.Instance;
        }

        [HttpGet]
        [Authorize]
        public ActionResult Index(string packageId) {
            ValidateParameterFormatsForIndex(packageId);

            if (!_userPackageAuthorizer.AuthorizedToEditPackage(packageId) &&
                !_userkeyPackageService.UserCanAccessPackage(packageId, AuthenticatedUser.Id)) {
                return new HttpUnauthorizedAccessToPackageResult(packageId);
            }
            IEnumerable<IUser> allOwnersForPackage = _userkeyPackageService.GetAllOwnersForPackage(packageId);
            IEnumerable<IUser> otherOwners = allOwnersForPackage.Where(u => u.Id != AuthenticatedUser.Id);
            return View(new ManagePackageOwnersViewModel { PackageId = packageId, OtherOwners = otherOwners, OwnerViewingPage = AuthenticatedUser});
        }

        [HttpPost]
        [Authorize]
        public ActionResult AddNewOwner(string packageId, string newOwnerUserName) {
            ValidateParameterFormatsForAddNewOwner(packageId, newOwnerUserName);

            if (!_userPackageAuthorizer.AuthorizedToEditPackage(packageId) &&
                !_userkeyPackageService.UserCanAccessPackage(packageId, AuthenticatedUser.Id)) {
                return new HttpUnauthorizedAccessToPackageResult(packageId);
            }
            IUser userToAdd = _membershipService.GetUser(newOwnerUserName);
            if (userToAdd != null) {
                if (!_userkeyPackageService.UserCanAccessPackage(packageId, userToAdd.Id)) {
                    _userkeyPackageService.RegisterPackageId(packageId, userToAdd.Id);
                    _orchardServices.Notifier.Information(T("The user '{0}' has been added as an owner for Package '{1}'.", newOwnerUserName, packageId));
                }
                else {
                    _orchardServices.Notifier.Error(T("The user '{0}' is already an owner of Package '{1}'.", newOwnerUserName, packageId));
                }
            }
            else {
                _orchardServices.Notifier.Error(T("The user '{0}' was not found.", newOwnerUserName));
            }
            return RedirectToAction("Index", new { packageId });
        }

        [HttpPost]
        [Authorize]
        public ActionResult RemoveOwner(string packageId, string idOfUserToRemove, string nameOfUserToRemove) {
            ValidateParameterFormatsForRemoveOwner(packageId, idOfUserToRemove, nameOfUserToRemove);

            if (!_userPackageAuthorizer.AuthorizedToEditPackage(packageId) &&
                !_userkeyPackageService.UserCanAccessPackage(packageId, AuthenticatedUser.Id)) {
                return new HttpUnauthorizedAccessToPackageResult(packageId);
            }
            int userIdToRemove;
            if (int.TryParse(idOfUserToRemove, out userIdToRemove) && userIdToRemove != AuthenticatedUser.Id) {
                _userkeyPackageService.RemovePackageIdRegistration(packageId, userIdToRemove);
                _orchardServices.Notifier.Information(T("'{0}' is no longer an owner of Package '{1}'.", nameOfUserToRemove, packageId));
            }
            return RedirectToAction("Index", new { packageId });
        }

        private void ValidateParameterFormatsForRemoveOwner(string packageId, string idOfUserToRemove, string nameOfUserToRemove) {
            _parameterFormatValidator.ValidatePackageIdFormat(packageId);
        }

        private void ValidateParameterFormatsForIndex(string packageId) {
            _parameterFormatValidator.ValidatePackageIdFormat(packageId);
        }

        private void ValidateParameterFormatsForAddNewOwner(string packageId, string newOwnerUserName) {
            _parameterFormatValidator.ValidatePackageIdFormat(packageId);
        }
    }
}