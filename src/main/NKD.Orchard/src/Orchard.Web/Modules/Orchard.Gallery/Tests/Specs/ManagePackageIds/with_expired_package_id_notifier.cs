using Machine.Specifications;
using Moq;
using Orchard.ContentManagement;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.ManagePackageIds;
using Orchard.Gallery.Models;

namespace Orchard.Gallery.Specs.ManagePackageIds {
    public abstract class with_expired_package_id_notifier {
        protected static ExpiredPackageIdNotifier notifier;

        protected static Mock<IOrchardServices> mockedOrchardServices;
        protected static Mock<IExpiredPackageIdMessenger> mockedExpiredPackageIdMessenger;
        protected static Mock<ITypeCaster> mockedTypeCaster;

        protected static readonly GallerySettingsPart gallerySettingsPart = new GallerySettingsPart {Record = new GallerySettingsPartRecord()};

        Establish context = () => {
            mockedOrchardServices = new Mock<IOrchardServices>();
            mockedExpiredPackageIdMessenger = new Mock<IExpiredPackageIdMessenger>();
            mockedTypeCaster = new Mock<ITypeCaster>();

            notifier = new ExpiredPackageIdNotifier(mockedOrchardServices.Object, mockedExpiredPackageIdMessenger.Object, mockedTypeCaster.Object);

            mockedOrchardServices.SetupGet(os => os.WorkContext).Returns(new Mock<WorkContext>().Object);
            mockedTypeCaster.Setup(tc => tc.CastTo<GallerySettingsPart>(Moq.It.IsAny<IContent>())).Returns(gallerySettingsPart);
        };
    }
}