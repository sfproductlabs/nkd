using System;
using JetBrains.Annotations;
using Orchard.Data;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;
using Orchard.Logging;
using System.Linq;

namespace Orchard.Gallery.ManagePackageIds {
    [UsedImplicitly]
    public class PackageIdExpirationCoordinator : IPackageIdExpirationCoordinator {
        private readonly IRepository<UserkeyPackage> _userkeyPackageRepository;
        private readonly IODataContext _oDataContext;
        private readonly IGalleryPackageService _galleryPackageService;
        private readonly IUserkeyPackageService _userkeyPackageService;
        private readonly IExpiredPackageIdNotifier _expiredPackageIdNotifier;

        public ILogger Logger { get; set; }

        public PackageIdExpirationCoordinator(IRepository<UserkeyPackage> userkeyPackageRepository, IODataContext oDataContext,
            IGalleryPackageService galleryPackageService, IUserkeyPackageService userkeyPackageService,
            IExpiredPackageIdNotifier expiredPackageIdNotifier) {
            _userkeyPackageRepository = userkeyPackageRepository;
            _oDataContext = oDataContext;
            _galleryPackageService = galleryPackageService;
            _userkeyPackageService = userkeyPackageService;
            _expiredPackageIdNotifier = expiredPackageIdNotifier;

            Logger = NullLogger.Instance;
        }

        public void ProcessExpirations(int numberOfDaysUntilPackageIdExpires, DateTime utcNow) {
            foreach (var userKeyPackage in _userkeyPackageService.GetPackageIdsThatAreNotPackageParts()) {
                UserkeyPackage userKeyPackageClosure = userKeyPackage;
                if (userKeyPackageClosure.RegisteredUtc == null) {
                    continue;
                }
                DateTime expirationDate = userKeyPackageClosure.RegisteredUtc.Value.AddDays(numberOfDaysUntilPackageIdExpires);
                bool packageIdIsExpired = expirationDate <= utcNow;
                bool packageIdIsNotInUseOnFeed = !_oDataContext.Packages.Where(p => p.Id == userKeyPackageClosure.PackageId).ToList().Any();
                if (packageIdIsExpired && packageIdIsNotInUseOnFeed) {
                    _galleryPackageService.DeleteUnfinishedPackages(userKeyPackageClosure.PackageId);
                    _userkeyPackageRepository.Delete(userKeyPackageClosure);
                    Logger.Information("Deleted expired Package ID '{0}' (ID {1}).", userKeyPackageClosure.PackageId, userKeyPackageClosure.Id);
                } else {
                    _expiredPackageIdNotifier.NotifyUserIfPackageIdIsAboutToExpire(userKeyPackageClosure, utcNow, expirationDate, packageIdIsNotInUseOnFeed);
                }
            }
        }
    }
}