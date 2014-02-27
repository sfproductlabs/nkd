using Gallery.Core.Domain;
using Orchard.Gallery.ViewModels;

namespace Orchard.Gallery.Interfaces {
    public interface IPackageViewModelMapper :IDependency {
        PackageViewModel MapPackageToViewModel(Package package, bool isNewPackage);
        void MapViewModelToPackage(PackageViewModel packageViewModel, Package packageToMapTo);
    }
}