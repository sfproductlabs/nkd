using Machine.Specifications;
using Moq;
using Orchard.Data;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.ManagePackageIds;
using Orchard.Gallery.Models;
using Orchard.Logging;

namespace Orchard.Gallery.Specs.ManagePackageIds {
    public abstract class with_a_package_id_expiration_coordinator {
        protected static PackageIdExpirationCoordinator coordinator;

        protected static Mock<IRepository<UserkeyPackage>> mockedUserKeyPackageRepository;
        protected static Mock<IODataContext> mockedODataContext;
        protected static Mock<IGalleryPackageService> mockedGalleryPackageService;
        protected static Mock<IUserkeyPackageService> mockedUserKeyPackageService;
        protected static Mock<IExpiredPackageIdNotifier> mockedExpiredPackageIdNotifier;
        protected static Mock<ILogger> mockedLogger;

        Establish context = () => {
            mockedUserKeyPackageRepository = new Mock<IRepository<UserkeyPackage>>();
            mockedODataContext = new Mock<IODataContext>();
            mockedGalleryPackageService = new Mock<IGalleryPackageService>();
            mockedUserKeyPackageService = new Mock<IUserkeyPackageService>();
            mockedExpiredPackageIdNotifier = new Mock<IExpiredPackageIdNotifier>();
            mockedLogger = new Mock<ILogger>();

            coordinator = new PackageIdExpirationCoordinator(mockedUserKeyPackageRepository.Object, mockedODataContext.Object,
                mockedGalleryPackageService.Object, mockedUserKeyPackageService.Object, mockedExpiredPackageIdNotifier.Object);
            coordinator.Logger = mockedLogger.Object;
            mockedLogger.Setup(l => l.IsEnabled(Moq.It.IsAny<LogLevel>())).Returns(true);
        };
    }
}