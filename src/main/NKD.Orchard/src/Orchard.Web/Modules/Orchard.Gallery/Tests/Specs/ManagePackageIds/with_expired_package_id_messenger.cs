using Machine.Specifications;
using Moq;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.ManagePackageIds;
using Orchard.Gallery.Models;
using Orchard.Messaging.Services;
using Orchard.Settings;

namespace Orchard.Gallery.Specs.ManagePackageIds {
    public abstract class with_expired_package_id_messenger {
        protected static ExpiredPackageIdMessenger expiredPackageIdMessenger;

        protected static Mock<IUserkeyService> mockedUserKeyService;
        protected static Mock<IOrchardServices> mockedOrchardService;
        protected static Mock<IMessageManager> mockedMessageManager;

        protected static readonly UserkeyPackage userKeyPackage = new UserkeyPackage { UserkeyId = 45, PackageId = "Package ID"};
        protected const string siteName = "my cool site";

        Establish context = () => {
            mockedUserKeyService = new Mock<IUserkeyService>();
            mockedOrchardService = new Mock<IOrchardServices>();
            mockedMessageManager = new Mock<IMessageManager>();
            expiredPackageIdMessenger = new ExpiredPackageIdMessenger(mockedUserKeyService.Object, mockedOrchardService.Object, mockedMessageManager.Object);

            var mockedWorkContext = new Mock<WorkContext>();
            mockedOrchardService.SetupGet(os => os.WorkContext).Returns(mockedWorkContext.Object);
            var mockedSite = new Mock<ISite>();
            mockedSite.SetupGet(s => s.SiteName).Returns(siteName);
            mockedWorkContext.Setup(wc => wc.GetState<ISite>(Moq.It.Is<string>(s => s == "CurrentSite"))).Returns(mockedSite.Object);
        };
    }
}