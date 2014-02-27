using System;
using System.Collections;
using System.Collections.Generic;
using Contrib.Taxonomies.Helpers;
using Contrib.Taxonomies.Models;
using Contrib.Taxonomies.Services;
using Gallery.Core.Domain;
using JetBrains.Annotations;
using Orchard.Gallery.Interfaces;
using System.Linq;
using Orchard.Gallery.Models;
using Orchard.ContentManagement;

namespace Orchard.Gallery.ManagePackageIds {
    [UsedImplicitly]
    public class RegisteredPackageIdGetter : IRegisteredPackageIdGetter {
        private readonly IUserkeyPackageService _userkeyPackageService;
        private readonly IPackageService _packageService;
        private readonly IOrchardServices _orchardServices;
        private readonly IUserkeyService _userkeyService;
        private readonly IGalleryPackageService _galleryPackageService;
        private readonly ITaxonomyService _taxonomyService;

        private readonly TaxonomyPart _taxonomy;

        public RegisteredPackageIdGetter(IUserkeyPackageService userkeyPackageService, IPackageService packageService, IOrchardServices orchardServices,
            IUserkeyService userkeyService, IGalleryPackageService galleryPackageService, ITaxonomyService taxonomyService) {
            _userkeyPackageService = userkeyPackageService;
            _packageService = packageService;
            _orchardServices = orchardServices;
            _userkeyService = userkeyService;
            _galleryPackageService = galleryPackageService;
            _taxonomyService = taxonomyService;

            _taxonomy = _taxonomyService.GetTaxonomyBySlug("PackageTypes");
        }

        public IEnumerable<UserkeyPackageViewModel> GetRegisteredPackageIdsForUser(int userId) {
            var key = _userkeyService.GetAccessKeyForUser(userId, false);

            IEnumerable<UserkeyPackage> userkeyPackagesForUser = _userkeyPackageService.GetUserkeyPackagesForUser(userId, false);
            ICollection<string> allRegisteredPackageIds = userkeyPackagesForUser.Select(p => p.PackageId).ToList();
            IEnumerable<Package> unfinishedPackages = _galleryPackageService.GetUnfinishedPackages(allRegisteredPackageIds, key.AccessKey);
            IEnumerable<string> packageIdsOfExistingPackageParts = _packageService.Get(p => allRegisteredPackageIds.Contains(p.PackageID), true)
                .GroupBy(p => p.PackageID).Select(p => p.Key);

            return from userkeyPackage in userkeyPackagesForUser
                let status = GetPackageStatus(userkeyPackage.PackageId, packageIdsOfExistingPackageParts, unfinishedPackages)
                let expirationDate = GetExpirationDate(userkeyPackage, status)
                let packageType = GetPackageType(userkeyPackage.PackageId, packageIdsOfExistingPackageParts, unfinishedPackages)
                select new UserkeyPackageViewModel {
                    UserkeyPackage = userkeyPackage,
                    ExpirationDate = expirationDate,
                    Status = status,
                    PackageType = packageType
                };
        }

        private string GetPackageType(string packageId, IEnumerable<string> packageIdsOfExistingPackageParts, IEnumerable<Package> unfinishedPackages) {
            if (packageIdsOfExistingPackageParts.Contains(packageId)) {
                var packagePart = _packageService.GetById(packageId).FirstOrDefault();
                if (packagePart != null) {
                    var terms = _taxonomyService.GetTermsForContentItem(packagePart.Id).Where(t => t.GetLevels() == 0);
                    if (terms.Any()) {
                        return terms.First().Slug;
                    }
                }
            } else if (unfinishedPackages.Any(up => up.Id == packageId)) {
                var term = _taxonomyService.GetTermByName(_taxonomy.Id, unfinishedPackages.First(up => up.Id == packageId).PackageType);
                if (term != null) {
                    return term.Slug;
                }
            }
            return "";
        }

        private PackageIdStatus GetPackageStatus(string packageId, IEnumerable<string> packageIdsOfExistingPackageParts, IEnumerable<Package> unfinishedPackages) {
            var unfinishedPackageIds = unfinishedPackages.Select(up => up.Id);

            return
                packageIdsOfExistingPackageParts.Contains(packageId) ? PackageIdStatus.InUse :
                    unfinishedPackageIds.Contains(packageId) ? PackageIdStatus.Unfinished : PackageIdStatus.Registered;
        }

        public int GetNumberOfPreregisteredPackageIdsForUser(int userId) {
            return GetRegisteredPackageIdsForUser(userId).Count(p => p.IsPreregistered);
        }

        private DateTime? GetExpirationDate(UserkeyPackage userkeyPackage, PackageIdStatus status) {
            int? daysBeforeExpiration = _orchardServices.WorkContext.CurrentSite.As<GallerySettingsPart>().NumberOfDaysUntilPreregisteredPackageIdExpires;
            DateTime? expirationDate = null;
            bool isPreregistered = status == PackageIdStatus.Registered || status == PackageIdStatus.Unfinished;
            if (isPreregistered && userkeyPackage.RegisteredUtc.HasValue && daysBeforeExpiration.HasValue) {
                expirationDate = userkeyPackage.RegisteredUtc.Value.AddDays(daysBeforeExpiration.Value);
            }
            return expirationDate;
        }
    }
}