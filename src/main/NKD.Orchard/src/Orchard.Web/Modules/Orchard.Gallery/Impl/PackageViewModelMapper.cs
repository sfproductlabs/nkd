using System;
using System.Text.RegularExpressions;
using Gallery.Core.Domain;
using JetBrains.Annotations;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.ViewModels;
using Orchard.Services;
using System.Linq;

namespace Orchard.Gallery.Impl {
    [UsedImplicitly]
    public class PackageViewModelMapper : IPackageViewModelMapper {
        private readonly IClock _clock;

        public PackageViewModelMapper(IClock clock) {
            _clock = clock;
        }

        public PackageViewModel MapPackageToViewModel(Package package, bool isNewPackage) {
            return new PackageViewModel {
                PackageId = package.Id,
                PackageVersion = package.Version,
                Title = package.Title,
                Summary = package.Summary,
                Description = package.Description,
                Authors = package.Authors,
                LicenseURL = package.LicenseUrl,
                PackageType = package.PackageType,
                Tags = package.Tags,
                IsLatestVersion = package.IsLatestVersion,
                ExternalPackageUrl = package.ExternalPackageUrl,
                ProjectUrl = package.ProjectUrl,
                RequireLicenseAcceptance = package.RequireLicenseAcceptance,
                Copyright = package.Copyright,
                PrimaryCategory = GetPrimaryCategoryName(package),
                IsExternalPackage = !string.IsNullOrWhiteSpace(package.ExternalPackageUrl),
                IsNewPackage =  isNewPackage
            };
        }

        private static string GetPrimaryCategoryName(Package package) {
            if (package.Categories == null) return string.Empty;
            var categories = package.Categories.Split(',');
            if (categories.Count() == 0) return string.Empty;
            return categories[0];
        }

        public void MapViewModelToPackage(PackageViewModel packageViewModel, Package packageToMapTo) {
            packageToMapTo.Id = packageViewModel.PackageId;
            packageToMapTo.Version = packageViewModel.PackageVersion;
            packageToMapTo.Title = packageViewModel.Title;
            packageToMapTo.Summary = packageViewModel.Summary;
            packageToMapTo.Description = packageViewModel.Description;
            packageToMapTo.Authors = packageViewModel.Authors;
            packageToMapTo.LicenseUrl = packageViewModel.LicenseURL;
            packageToMapTo.PackageType = packageViewModel.PackageType;
            packageToMapTo.Categories = packageViewModel.PrimaryCategory;
            packageToMapTo.IsLatestVersion = packageViewModel.IsLatestVersion;
            packageToMapTo.ExternalPackageUrl = packageViewModel.ExternalPackageUrl;
            packageToMapTo.ProjectUrl = packageViewModel.ProjectUrl;
            packageToMapTo.RequireLicenseAcceptance = packageViewModel.RequireLicenseAcceptance;
            packageToMapTo.Copyright = packageViewModel.Copyright;

            packageToMapTo.LastUpdated = _clock.UtcNow;
            packageToMapTo.Tags = CleanTags(packageViewModel.Tags);
        }

        private static string CleanTags(string tags) {
            return !string.IsNullOrWhiteSpace(tags) ? new Regex(@",\s*").Replace(tags.Trim(), " ") : tags;
        }
    }
}
