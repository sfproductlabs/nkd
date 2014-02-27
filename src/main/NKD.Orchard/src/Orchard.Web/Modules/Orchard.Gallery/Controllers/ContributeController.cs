using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Gallery.Core.Domain;
using JetBrains.Annotations;
using Orchard.DisplayManagement;
using Orchard.Gallery.Attributes;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;
using Orchard.Gallery.Models.ViewModels;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Tags.Models;
using Orchard.Themes;
using System.Linq;
using Orchard.UI.Navigation;
using Orchard.ContentManagement;

namespace Orchard.Gallery.Controllers {
    [Themed]
    public class ContributeController : Controller {
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserkeyService _userkeyService;
        private readonly IUserkeyPackageService _userkeyPackageService;
        private readonly IOrchardServices _services;
        private readonly IGalleryPackageService _galleryPackageService;
        private readonly IPackageService _packageService;
        private readonly ISiteService _siteService;
        private readonly IAdminPackagePrivilegeChecker _packagePrivilegeChecker;

        public ContributeController(IAuthenticationService authenticationService, IUserkeyService userkeyService, IUserkeyPackageService userkeyPackageService,
            IOrchardServices services, IGalleryPackageService galleryPackageService, IPackageService packageService, ISiteService siteService,
            IShapeFactory shapeFactory, IAdminPackagePrivilegeChecker packagePrivilegeChecker) {
            _authenticationService = authenticationService;
            _packagePrivilegeChecker = packagePrivilegeChecker;
            _siteService = siteService;
            _packageService = packageService;
            _userkeyService = userkeyService;
            _userkeyPackageService = userkeyPackageService;
            _services = services;
            _galleryPackageService = galleryPackageService;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }

        [HttpGet]
        public ActionResult Index() {
            dynamic viewModel = _services.New.ViewModel();
            viewModel.HasUnpublishedPackages = false;
            if (Request.IsAuthenticated) {
                IUser authenticatedUser = _authenticationService.GetAuthenticatedUser();
                Userkey key = _userkeyService.GetAccessKeyForUser(authenticatedUser.Id);

                IEnumerable<Package> unpublishedPackages = GetUserUnfinshedPackages(authenticatedUser.Id, key);
                viewModel.HasUnpublishedPackages = unpublishedPackages.Any();
            }
            return View(viewModel);
        }

        [HttpGet]
        [Authorize]
        public ActionResult MyAccount() {
            IUser authenticatedUser = _authenticationService.GetAuthenticatedUser();
            return View(_userkeyService.GetAccessKeyForUser(authenticatedUser.Id));
        }

        [HttpPost]
        [Authorize]
        [UsedImplicitly]
        public ActionResult Generate() {
            IUser authenticatedUser = _authenticationService.GetAuthenticatedUser();
            _userkeyService.SaveKeyForUser(authenticatedUser.Id, Guid.NewGuid());
            return RedirectToAction("MyAccount");
        }

        [HttpGet]
        [StoreLastVisitedPackageList]
        [Authorize]
        public ActionResult MyPackages(PagerParameters pagerParameters) {
            Pager pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            IUser authenticatedUser = _authenticationService.GetAuthenticatedUser();
            var key = _userkeyService.GetAccessKeyForUser(authenticatedUser.Id);
            dynamic list = _services.New.List();

            int startingPackageIndex = pager.GetStartIndex();
            IEnumerable<PackagePart> packages = GetPublishedPackagesUserCanManage(authenticatedUser, key, startingPackageIndex, pager.PageSize);
            int totalPackageCount = GetCountOfPublishedPackagesUserCanManage(authenticatedUser, key);

            IEnumerable<PackagePart> unpublishedPackages = GetUnpublishedPackagesUserCanManage(authenticatedUser, key);

            list.AddRange(packages.Select(p => {
                                        dynamic packageShape = _services.ContentManager.BuildDisplay(p, "SummaryOwner");
                                        packageShape.HasTags = p.As<TagsPart>().CurrentTags.Count() > 0;
                                        return packageShape;
                                    }
                              ));


            var viewModel = new MyPackagesViewModel {
                Packages = list,
                TotalNumberOfPackages = totalPackageCount,
                StartingNumber = startingPackageIndex + 1,
                EndingNumber = totalPackageCount < startingPackageIndex + pager.PageSize ? totalPackageCount : startingPackageIndex + pager.PageSize,
                Pager = Shape.Pager(pager).TotalItemCount(totalPackageCount),
                UnpublishedPackages = unpublishedPackages
            };
            return View(viewModel);
        }

        [HttpGet]
        [StoreLastVisitedPackageList]
        [Authorize]
        public ActionResult MyUnfinishedPackages() {
            IUser authenticatedUser = _authenticationService.GetAuthenticatedUser();
            var key = _userkeyService.GetAccessKeyForUser(authenticatedUser.Id);

            var unfinishedPackages = GetUserUnfinshedPackages(authenticatedUser.Id, key);

            return View(unfinishedPackages.Select(up => new SimplePackage { PackageId = up.Id, PackageVersion = up.Version, Title = up.Title }));
        }

        private IEnumerable<PackagePart> GetPublishedPackagesUserCanManage(IUser user, Userkey key, int startingIndex, int pageSize) {
            IEnumerable<PackagePart> recommendedPackages;
            if (user != null) {
                recommendedPackages = _packagePrivilegeChecker.UserCanManageAllPackages(user) ?
                    _packageService.Get(startingIndex, pageSize, p => p.IsRecommendedVersion) :
                    _userkeyPackageService.GetPackagesByUserkey(key.Id, startingIndex, pageSize, p => p.IsRecommendedVersion);
            } else {
                recommendedPackages = new List<PackagePart>();
            }
            return recommendedPackages;
        }

        private IEnumerable<PackagePart> GetUnpublishedPackagesUserCanManage(IUser user, Userkey key) {
            if (user != null) {
                IEnumerable<PackagePart> packagesByUserkey = _userkeyPackageService.GetPackagesByUserkey(key.Id, true);
                return packagesByUserkey.Where(up => !up.ContentItem.IsPublished());
            }
            else {
                return new List<PackagePart>();
            }
        }

        private int GetCountOfPublishedPackagesUserCanManage(IUser user, Userkey key) {
            if (user != null) {
                return _packagePrivilegeChecker.UserCanManageAllPackages(user) ? _packageService.CountOfPackages(p => p.IsRecommendedVersion) : _userkeyPackageService.CountOfUsersPackages(key.Id, p => p.IsRecommendedVersion);
            }
            return 0;
        }

        private IEnumerable<Package> GetUserUnfinshedPackages(int userId, Userkey key) {
            return _galleryPackageService.GetUnfinishedPackages(_userkeyPackageService.GetUserkeyPackagesForUser(userId).Select(up => up.PackageId), key.AccessKey);
        }
    }
}