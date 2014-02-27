using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Contrib.Taxonomies.Helpers;
using Contrib.Taxonomies.Models;
using Contrib.Taxonomies.Services;
using Gallery.Core.Domain;
using Orchard.Gallery.Attributes;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;
using Orchard.Gallery.PackageSynchronization;
using Orchard.Gallery.ViewModels;
using Orchard.Localization;
using Orchard.Themes;
using Orchard.ContentManagement;
using System.Linq;
using Orchard.UI.Notify;

namespace Orchard.Gallery.Controllers {

    [Themed]
    public class PackageController : Controller {
        private readonly IOrchardServices _services;
        private readonly ITaxonomyService _taxonomyService;
        private readonly IGalleryPackageService _galleryPackageService;
        private readonly IPackageService _packageService;
        private readonly IPackageViewModelMapper _packageViewModelMapper;
        private readonly IUserPackageAuthorizer _userPackageAuthorizer;
        private readonly IParameterFormatValidator _parameterFormatValidator;
        private readonly IPackageVisitTracker _packageVisitTracker;
        private readonly IGalleryPackagePublishingService _packagePublishingService;
        private readonly IPackagePartPublishingService _packagePartPublishingService;

        public Localizer T { get; set; }

        public PackageController(IOrchardServices services, ITaxonomyService taxonomyService, IGalleryPackageService galleryPackageService,
            IPackageService packageService, IPackageViewModelMapper packageViewModelMapper, IServiceUriBuilder serviceUriBuilder,
            IUserPackageAuthorizer userPackageAuthorizer, IParameterFormatValidator parameterFormatValidator, IPackageVisitTracker packageVisitTracker,
            IGalleryPackagePublishingService packagePublishingService, IPackagePartPublishingService packagePartPublishingService) {
            _services = services;
            _packagePartPublishingService = packagePartPublishingService;
            _userPackageAuthorizer = userPackageAuthorizer;
            _parameterFormatValidator = parameterFormatValidator;
            _taxonomyService = taxonomyService;
            _galleryPackageService = galleryPackageService;
            _packageService = packageService;
            _packageViewModelMapper = packageViewModelMapper;
            _packageVisitTracker = packageVisitTracker;
            _packagePublishingService = packagePublishingService;

            T = NullLocalizer.Instance;
        }

        [HttpGet]
        [StoreLastVisitedPackageDetailsLink]
        public ActionResult DetailsForId(string packageType, string packageId) {
            ValidateParameterFormatsForDetails(packageId);
            string packageVersion = Request.QueryString["packageVersion"];
            if (!string.IsNullOrWhiteSpace(packageVersion)) {
                RedirectToActionPermanent("DetailsForIdAndVersion", new {packageType, packageId, packageVersion});
            }
            IEnumerable<PackagePart> packageParts = _packageService.GetById(packageId, true);
            PackagePart packagePartToDisplay = packageParts.SingleOrDefault(p => p.IsRecommendedVersion);
            if (packagePartToDisplay == null && packageParts.Any()) {
                packagePartToDisplay = packageParts.Select(p => new { Package = p, Version = new Version(p.PackageVersion) })
                    .OrderByDescending(v => v.Version).First().Package;
            }
            return DisplayPackage(packageType, packagePartToDisplay);
        }

        [HttpGet]
        [StoreLastVisitedPackageDetailsLink]
        public ActionResult DetailsForIdAndVersion(string packageType, string packageId, string packageVersion) {
            Version dummy;
            if (!Version.TryParse(packageVersion, out dummy)) {
                return RedirectToCorrectAction(packageType, packageVersion);
            }
            ValidateParameterFormatsForEdit(packageId, packageVersion);
            PackagePart packagePartToDisplay = _packageService.Get(packageId, packageVersion, true);
            return DisplayPackage(packageType, packagePartToDisplay);
        }

        private ActionResult RedirectToCorrectAction(string packageType, string packageSlug) {
            _parameterFormatValidator.ValidateSlugFormat(packageSlug);
            var part = _packageService.Get(packageSlug);
            if (part == null) {
                throw new HttpException(404, T("Could not find the requested package").Text);
            }
            if (part.IsRecommendedVersion) {
                return RedirectToActionPermanent("DetailsForId", new { packageType, packageId = part.PackageID });
            }
            return RedirectToActionPermanent("DetailsForIdAndVersion",
                new { packageType, packageId = part.PackageID, packageVersion = part.PackageVersion });
        }

        private ActionResult DisplayPackage(string packageType, PackagePart packagePartToDisplay) {
            if (packagePartToDisplay == null) {
                throw new HttpException(404, T("Could not find the requested package").Text);
            }
            bool allowOwnerActions = _userPackageAuthorizer.AuthorizedToEditPackage(packagePartToDisplay.PackageID);
            IContentQuery<ScreenshotPart, ScreenshotPartRecord> screenshots = _services.ContentManager.Query<ScreenshotPart, ScreenshotPartRecord>()
                .Where(sp => sp.PackageID == packagePartToDisplay.PackageID && sp.PackageVersion == packagePartToDisplay.PackageVersion);

            var allVersionsOfPackage = _services.ContentManager.Query<PackagePart, PackagePartRecord>(VersionOptions.AllVersions)
                .Where(pp => pp.PackageID == packagePartToDisplay.PackageID).List().OrderByDescending(pp => pp.PackageVersion);
            var allVersions = new List<dynamic>();
            foreach (var version in allVersionsOfPackage) {
                var versionShape = _services.ContentManager.BuildDisplay(version, "VersionHistory");
                versionShape.AllowOwnerActions = allowOwnerActions;
                versionShape.CurrentPackageId = packagePartToDisplay.PackageID;
                versionShape.CurrentPackageVersion = new Version(packagePartToDisplay.PackageVersion);
                versionShape.PackageTypeSlug = packageType;
                versionShape.IsPublished = version.ContentItem.IsPublished();
                allVersions.Add(versionShape);
            }

            dynamic packageDisplay = _services.ContentManager.BuildDisplay(packagePartToDisplay, "Details");
            packageDisplay.HasScreenshots = screenshots.Count() > 0;
            packageDisplay.AllVersionsOfPackage = allVersions;
            packageDisplay.AllowOwnerActions = allowOwnerActions;

            return View("Details", packageDisplay);
        }

        [HttpGet]
        [Authorize]
        public ActionResult Edit(string packageId, string packageVersion) {
            ValidateParameterFormatsForEdit(packageId, packageVersion);

            return DisplayEditPage(packageId, packageVersion, false);
        }

        [HttpGet]
        [Authorize]
        public ActionResult New(string packageId, string packageVersion) {
            ValidateParameterFormatsForNew(packageId, packageVersion);

            return DisplayEditPage(packageId, packageVersion, true);
        }

        private ActionResult DisplayEditPage(string packageId, string packageVersion, bool isNewPackage) {
            if (!_userPackageAuthorizer.AuthorizedToEditPackage(packageId)) {
                return new HttpUnauthorizedAccessToPackageResult(packageId);
            }

            Package package = _galleryPackageService.GetPackage(packageId, packageVersion);
            if (package == null) {
                if (HttpContext.Request.UrlReferrer != null) {
                    return new RedirectResult(HttpContext.Request.UrlReferrer.ToString());
                }
                throw new HttpException(404, T("Could not find the requested package").Text);
            }

            var packageViewModel = _packageViewModelMapper.MapPackageToViewModel(package, isNewPackage);
            PackagePart currentlyRecommendedPackage = _packageService.Get()
                .SingleOrDefault(p => p.PackageID == packageViewModel.PackageId && p.IsRecommendedVersion);
            string currentlyRecommendedPackageVersion = null;
            if (currentlyRecommendedPackage != null) {
                currentlyRecommendedPackageVersion = currentlyRecommendedPackage.PackageVersion;
                packageViewModel.RecommendedVersionPackageType = _taxonomyService.GetTermsForContentItem(currentlyRecommendedPackage.ContentItem.Id).Single(t => t.GetLevels() == 0).Slug;
            }
            packageViewModel.RecommendedVersion = currentlyRecommendedPackageVersion;
            SetViewModelTaxonomyFields(packageViewModel);
            return View("Edit", packageViewModel);
        }

        [HttpPost]
        [Authorize]
        public ActionResult Edit(PackageViewModel packageViewModelToUpdate) {
            try {
                ValidateParameterFormatsForEdit(packageViewModelToUpdate);
            }
            catch (UriFormatException ex) {
                _services.Notifier.Error(T("The URL '{0}' is not valid.", ex.Message));
                SetViewModelTaxonomyFields(packageViewModelToUpdate);
                return View(packageViewModelToUpdate);
            }
            if (!_userPackageAuthorizer.AuthorizedToEditPackage(packageViewModelToUpdate.PackageId)) {
                return new HttpUnauthorizedAccessToPackageResult(packageViewModelToUpdate.PackageId);
            }
            if (packageViewModelToUpdate.IsExternalPackage && !Uri.IsWellFormedUriString
                (packageViewModelToUpdate.ExternalPackageUrl, UriKind.Absolute)) {
                ModelState.AddModelError("ExternalPackageUrl", "The External Package URL is not valid.");
            }
            if (!ModelState.IsValid) {
                SetViewModelTaxonomyFields(packageViewModelToUpdate);
                return View(packageViewModelToUpdate);
            }
            Package package = _galleryPackageService.GetPackage(packageViewModelToUpdate.PackageId, packageViewModelToUpdate.PackageVersion);
            _packageViewModelMapper.MapViewModelToPackage(packageViewModelToUpdate, package);
            _galleryPackageService.UpdatePackage(package);
            string action = packageViewModelToUpdate.IsNewPackage ? "New" : "Edit";
            return RedirectToAction(action, "UploadPackageLogoAndScreenshots", new { packageId = package.Id, packageVersion = package.Version});
        }

        [HttpPost]
        [Authorize]
        public ActionResult Delete(string packageId, string packageVersion, bool shouldRedirect) {
            ValidateParameterFormatsForDelete(packageId, packageVersion);

            if (!_userPackageAuthorizer.AuthorizedToEditPackage(packageId)) {
                return new HttpUnauthorizedAccessToPackageResult(packageId);
            }

            _galleryPackageService.DeletePackage(packageId, packageVersion);

            Uri uri = shouldRedirect
                ? _packageVisitTracker.RetrieveLastVisitedPackageList(HttpContext)
                : _packageVisitTracker.RetrieveLastVisitedPackageDetailsLink(HttpContext);
            if (uri != null) {
                return Redirect(uri.PathAndQuery);
            }
            return RedirectToAction("MyPackages", "Contribute");
        }

        [HttpPost]
        [Authorize]
        public ActionResult Unpublish(string packageId, string packageVersion) {
            try {
                _packagePublishingService.UnpublishPackage(packageId, packageVersion);
                _packagePartPublishingService.Unpublish(packageId, packageVersion);
                _services.Notifier.Information(T("The package {0}, version {1}, has been unpublished", packageId, packageVersion));
            }
            catch (Exception ex) {
                _services.Notifier.Error(T(ex.Message));
            }
            Uri referrer = HttpContext.Request.UrlReferrer;
            if (referrer != null) {
                return new RedirectResult(referrer.AbsoluteUri);
            }
            return RedirectToAction("MyPackages", "Contribute");
        }

        [HttpGet]
        [Authorize]
        public ActionResult Publish(string packageId, string packageVersion) {
            try {
                _packagePublishingService.RePublishPackage(packageId, packageVersion);
                _packagePartPublishingService.Publish(packageId, packageVersion);
                _services.Notifier.Information(T("The package {0}, version {1} has been published.", packageId, packageVersion));
            }
            catch (Exception ex) {
                _services.Notifier.Error(T(ex.Message));
            }
            return new RedirectResult(_packageVisitTracker.RetrieveLastVisitedPackageDetailsLink(HttpContext).ToString());
        }

        private void SetViewModelTaxonomyFields(PackageViewModel packageViewModel)
        {
            int taxonomyId = _taxonomyService.GetTaxonomyByName("Package Types").Id;
            IEnumerable<TermPart> parentTerms = _taxonomyService.GetTerms(taxonomyId).Where(t => t.GetLevels() == 0);
            packageViewModel.PackageTypes = parentTerms.Select(t => t.Name);

            var parentTerm = string.IsNullOrEmpty(packageViewModel.PackageType) ? parentTerms.First()
                : parentTerms.Single(t => t.Name == packageViewModel.PackageType);
            packageViewModel.PackageCategories = _taxonomyService.GetChildren(parentTerm).Select(t => t.Name).OrderBy(category => category);
        }

        private void ValidateParameterFormatsForDetails(string packageId) {
            _parameterFormatValidator.ValidatePackageIdFormat(packageId);
        }

        private void ValidateParameterFormatsForEdit(string packageId, string packageVersion) {
            _parameterFormatValidator.ValidatePackageIdFormat(packageId);
            _parameterFormatValidator.ValidatePackageVersionFormat(packageVersion);
        }

        private void ValidateParameterFormatsForEdit(PackageViewModel model) {
            _parameterFormatValidator.ValidatePackageIdFormat(model.PackageId);
            _parameterFormatValidator.ValidatePackageVersionFormat(model.PackageVersion);
            _parameterFormatValidator.ValidateUriFormat(model.ExternalPackageUrl, UriKind.Absolute);
            _parameterFormatValidator.ValidateUriFormat(model.LicenseURL, UriKind.Absolute);
            _parameterFormatValidator.ValidateUriFormat(model.ProjectUrl, UriKind.Absolute);
            _parameterFormatValidator.ValidateUriFormat(model.ReportAbuseUrl, UriKind.Absolute);
        }

        private void ValidateParameterFormatsForNew(string packageId, string packageVersion) {
            _parameterFormatValidator.ValidatePackageIdFormat(packageId);
            _parameterFormatValidator.ValidatePackageVersionFormat(packageVersion);
        }

        private void ValidateParameterFormatsForDelete(string packageId, string packageVersion) {
            _parameterFormatValidator.ValidatePackageIdFormat(packageId);
            _parameterFormatValidator.ValidatePackageVersionFormat(packageVersion);
        }
    }
}
