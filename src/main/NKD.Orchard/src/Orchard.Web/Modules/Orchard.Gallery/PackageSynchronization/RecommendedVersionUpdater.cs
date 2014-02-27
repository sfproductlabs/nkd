using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;
using System.Linq;
using Orchard.Tasks.Indexing;

namespace Orchard.Gallery.PackageSynchronization {
    public class RecommendedVersionUpdater : IRecommendedVersionUpdater {
        private readonly IPackageService _packageService;
        private readonly IIndexingTaskManager _indexingTaskManager;

        public RecommendedVersionUpdater(IPackageService packageService, IIndexingTaskManager indexingTaskManager) {
            _packageService = packageService;
            _indexingTaskManager = indexingTaskManager;
        }

        public void SetRecommendedVersionFlagsOfOtherPackagesWithSameId(PackagePart packagePart) {
            if (packagePart.IsRecommendedVersion) {
                IEnumerable<PackagePart> packagePartsToUpdate = _packageService.GetById(packagePart.PackageID)
                    .Where(pp => pp.PackageVersion != packagePart.PackageVersion);

                foreach (var packagePartToUpdate in packagePartsToUpdate) {
                    packagePartToUpdate.IsRecommendedVersion = false;
                    _indexingTaskManager.CreateUpdateIndexTask(packagePartToUpdate.ContentItem);
                }
            }
        }
    }
}