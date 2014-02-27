using System;
using System.Collections.Generic;
using System.Linq;
using Gallery.Core.Domain;
using Machine.Specifications;
using Moq;
using Orchard.ContentManagement;
using Orchard.Gallery.GalleryServer;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.ManagePackageIds;
using Orchard.Gallery.Models;
using Orchard.Security;
using Orchard.Users.Models;

namespace Orchard.Gallery.Specs.ManagePackageIds {
    public abstract class with_package_id_in_use_checker {
        protected static PackageIdInUseChecker Checker;

        protected static Mock<IPackageService> MockedPackageService = new Mock<IPackageService>();
        protected static Mock<IODataContext> MockedODataContext = new Mock<IODataContext>();
        protected static Mock<IGalleryPackageService> MockedGalleryPackageService = new Mock<IGalleryPackageService>();
        protected static Mock<IUserkeyService> MockedUserkeyService = new Mock<IUserkeyService>();
        protected static Mock<IAuthenticationService> MockedAuthenticationService = new Mock<IAuthenticationService>();

        protected const string PackageId = "packageId";

        Establish context = () => {
            MockedPackageService = new Mock<IPackageService>();
            MockedODataContext = new Mock<IODataContext>();
            MockedGalleryPackageService = new Mock<IGalleryPackageService>();
            MockedUserkeyService = new Mock<IUserkeyService>();
            MockedAuthenticationService = new Mock<IAuthenticationService>();
            Checker = new PackageIdInUseChecker(MockedPackageService.Object, MockedODataContext.Object, MockedGalleryPackageService.Object,
                MockedUserkeyService.Object, MockedAuthenticationService.Object);

            MockedAuthenticationService.Setup(auth => auth.GetAuthenticatedUser()).Returns(new UserPart { ContentItem = new ContentItem() });
            MockedUserkeyService.Setup(uks => uks.GetAccessKeyForUser(Moq.It.IsAny<int>(), Moq.It.IsAny<bool>())).Returns(new Userkey());
            MockedODataContext.SetupGet(odc => odc.Packages).Returns(new PublishedPackage[0].AsQueryable());
            MockedGalleryPackageService.Setup(gps => gps.GetUnfinishedPackages(Moq.It.IsAny<IEnumerable<string>>(), Moq.It.IsAny<Guid>())).
                Returns(new Package[0]);
        };
    }
}