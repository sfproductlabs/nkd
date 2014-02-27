using System;
using Machine.Specifications;
using Moq;
using Orchard.ContentManagement;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.ManagePackageIds;
using Orchard.Gallery.Models;
using Orchard.Logging;
using Orchard.Services;

namespace Orchard.Gallery.Specs.ManagePackageIds {
    public abstract class with_expired_package_id_task_runner {
        protected static ExpiredPackageIdTaskRunner TaskRunner;

        protected static Mock<IOrchardServices> mockedOrchardServices;
        protected static Mock<IClock> mockedClock;
        protected static Mock<IPackageIdExpirationCoordinator> mockedExpiredPackageIdDeleter;
        protected static Mock<ITypeCaster> mockedTypeCaster;
        protected static Mock<ILogger> mockedLogger;

        protected static DateTime utcNow;
        protected static GallerySettingsPart gallerySettingsPart;

        Establish context = () =>
        {
            mockedOrchardServices = new Mock<IOrchardServices>();
            mockedClock = new Mock<IClock>();
            mockedExpiredPackageIdDeleter = new Mock<IPackageIdExpirationCoordinator>();
            mockedTypeCaster = new Mock<ITypeCaster>();
            mockedLogger = new Mock<ILogger>();

            TaskRunner = new ExpiredPackageIdTaskRunner(mockedOrchardServices.Object, mockedClock.Object, mockedExpiredPackageIdDeleter.Object,
                mockedTypeCaster.Object) {
                Logger = mockedLogger.Object
            };
            mockedLogger.Setup(l => l.IsEnabled(Moq.It.IsAny<LogLevel>())).Returns(true);

            utcNow = DateTime.UtcNow;
            gallerySettingsPart = new GallerySettingsPart { Record = new GallerySettingsPartRecord() };

            mockedClock.SetupGet(c => c.UtcNow).Returns(utcNow);
            mockedOrchardServices.SetupGet(os => os.WorkContext).Returns(new Mock<WorkContext>().Object);
            mockedTypeCaster.Setup(tc => tc.CastTo<GallerySettingsPart>(Moq.It.IsAny<IContent>())).Returns(gallerySettingsPart);
        };
    }
}