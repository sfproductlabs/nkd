using System.Linq;
using Gallery.Core.Domain;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Gallery.GalleryServer;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;
using Orchard.Logging;

namespace Orchard.Gallery.PackageSynchronization {
    [UsedImplicitly]
    public class PackagePartCreator : IPackagePartCreator {
        private readonly IPackagePartMapper _packagePartMapper;
        private readonly IOrchardServices _orchardServices;
        private readonly IODataContext _oDataContext;
        private readonly IPackagePartExtendedInfoUpdater _packagePartExtendedInfoUpdater;
        private readonly IRecommendedVersionUpdater _recommendedVersionUpdater;
        private readonly IPackageService _packageService;

        public ILogger Logger { get; set; }

        public PackagePartCreator(IPackagePartMapper packagePartMapper, IOrchardServices orchardServices, IODataContext oDataContext,
            IPackagePartExtendedInfoUpdater packagePartExtendedInfoUpdater, IRecommendedVersionUpdater recommendedVersionUpdater,
            IPackageService packageService) {
            _packagePartMapper = packagePartMapper;
            _packagePartExtendedInfoUpdater = packagePartExtendedInfoUpdater;
            _recommendedVersionUpdater = recommendedVersionUpdater;
            _packageService = packageService;
            _orchardServices = orchardServices;
            _oDataContext = oDataContext;

            Logger = NullLogger.Instance;
        }

        public void CreateNewPackagePart(PackageLogEntry log) {
            PublishedPackage publishedPackage = _oDataContext.Packages.Where(pp => pp.Id == log.PackageId && pp.Version == log.PackageVersion)
                .ToList().FirstOrDefault();
            if (publishedPackage == null) {
                Logger.Information("Package '{0}', version {1} was not created since it was not found on the Feed.", log.PackageId, log.PackageVersion);
                return;
            }
            if (_packageService.PackageExists(log.PackageId, log.PackageVersion, VersionOptions.AllVersions)) {
                Logger.Information("Package '{0}', version {1} was not created since it already exists in the gallery.", log.PackageId, log.PackageVersion);
                return;
            }
            PackagePart packagePart = _orchardServices.ContentManager.New<PackagePart>("Package");
            _packagePartMapper.MapPublishedPackageToPackagePart(publishedPackage, packagePart);
            _orchardServices.ContentManager.Create(packagePart, VersionOptions.Published);

            _packagePartExtendedInfoUpdater.UpdateExtendedPackageInfo(publishedPackage, packagePart);
            _recommendedVersionUpdater.SetRecommendedVersionFlagsOfOtherPackagesWithSameId(packagePart);
            Logger.Information("Created PackagePart '{0}', version {1}.", log.PackageId, log.PackageVersion);
        }
    }
}