using Orchard.Gallery.Models;

namespace Orchard.Gallery.PackageSynchronization {
    public interface IRecommendedVersionUpdater : IDependency {
        void SetRecommendedVersionFlagsOfOtherPackagesWithSameId(PackagePart packagePart);
    }
}