using System.Collections.Generic;
using System.Linq;
using Contrib.Taxonomies.Helpers;
using Contrib.Taxonomies.Models;
using Contrib.Taxonomies.Services;
using JetBrains.Annotations;
using Orchard.Gallery.GalleryServer;
using Orchard.Gallery.Models;

namespace Orchard.Gallery.PackageSynchronization {
    [UsedImplicitly]
    public class PackagePartTaxonomyUpdater : IPackagePartTaxonomyUpdater {
        private readonly ITaxonomyService _taxonomyService;

        private const string PACKAGE_TYPE_TAXONOMY_FIELD_NAME = "PackageType";

        public PackagePartTaxonomyUpdater(ITaxonomyService taxonomyService) {
            _taxonomyService = taxonomyService;
        }

        public void UpdatePackageTaxonomy(PublishedPackage publishedPackage, PackagePart packagePart) {
            int packageTypesTaxonomyId = _taxonomyService.GetTaxonomyByName("Package Types").Id;
            if (!string.IsNullOrEmpty(publishedPackage.PackageType)) {
                SetPackagePartTaxonomyToPackageTypeTerm(packageTypesTaxonomyId, packagePart, publishedPackage);
            }
            else {
                SetPackagePartTaxonomyToDefaultTerm(packageTypesTaxonomyId, packagePart);
            }
        }

        private void SetPackagePartTaxonomyToPackageTypeTerm(int packageTypesTaxonomyId, PackagePart packagePart, PublishedPackage publishedPackage) {
            TermPart packageTypeTerm = _taxonomyService.GetTermByName(packageTypesTaxonomyId, publishedPackage.PackageType);
            var terms = new List<TermPart> { packageTypeTerm };

            if (!string.IsNullOrWhiteSpace(publishedPackage.Categories)) {
                string category = publishedPackage.Categories.Split(',').Last();
                TermPart packageCategoryTerm = _taxonomyService.GetChildren(packageTypeTerm).FirstOrDefault(t => t.Name == category);
                if (packageCategoryTerm != null) {
                    terms.Add(packageCategoryTerm);
                }
            }
            _taxonomyService.UpdateTerms(packagePart.ContentItem, terms, PACKAGE_TYPE_TAXONOMY_FIELD_NAME);
        }

        private void SetPackagePartTaxonomyToDefaultTerm(int packageTypesTaxonomyId, PackagePart packagePart) {
            IEnumerable<TermPart> termParts = _taxonomyService.GetTerms(packageTypesTaxonomyId)
                .Where(tp => tp.GetLevels() == 0);
            if (termParts.Count() == 1)
            {
                var terms = new List<TermPart> { termParts.Single() };
                _taxonomyService.UpdateTerms(packagePart.ContentItem, terms, PACKAGE_TYPE_TAXONOMY_FIELD_NAME);
            }
        }
    }
}