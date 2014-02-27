using Gallery.Core.Domain;
using Moq;
using NUnit.Framework;
using Orchard.ContentManagement;
using Orchard.Gallery.GalleryServer;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;
using Orchard.Gallery.PackageSynchronization;
using System.Linq;
using Orchard.Logging;

namespace Orchard.Gallery.UnitTests.PackageSynchronization.PackagePartCreator {
    [TestFixture]
    public class CreateNewPackagePartTester {
        private Gallery.PackageSynchronization.PackagePartCreator _creator;

        private Mock<IRecommendedVersionUpdater> _mockedRecommendedVersionUpdater;
        private Mock<IODataContext> _mockedOdataContext;
        private Mock<IContentManager> _mockedContentManager;
        private Mock<IPackageService> _mockedPackageService;
        private Mock<ILogger> _mockedLogger;

        [SetUp]
        public void SetUp() {
            _mockedRecommendedVersionUpdater = new Mock<IRecommendedVersionUpdater>();
            _mockedOdataContext = new Mock<IODataContext>();
            _mockedPackageService = new Mock<IPackageService>();
            _mockedLogger = new Mock<ILogger>();

            var mockedOrchardServices = new Mock<IOrchardServices>();

            _creator = new Gallery.PackageSynchronization.PackagePartCreator(new Mock<IPackagePartMapper>().Object, mockedOrchardServices.Object,
                _mockedOdataContext.Object, new Mock<IPackagePartExtendedInfoUpdater>().Object, _mockedRecommendedVersionUpdater.Object,
                _mockedPackageService.Object) {Logger = _mockedLogger.Object};
            _mockedContentManager = new Mock<IContentManager>();
            _mockedLogger.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            mockedOrchardServices.SetupGet(os => os.ContentManager).Returns(_mockedContentManager.Object);
        }

        [Test]
        public void ShouldCallRecommendedVersionUpdater() {
            PackagePart newPackagePart = new PackagePart { Record = new PackagePartRecord(), PackageID = "foo", PackageVersion = "1.0" };
            var contentItem = new ContentItem { ContentType = "Package" };
            contentItem.Weld(newPackagePart);
            PublishedPackage publishedPackage = new PublishedPackage { Id = newPackagePart.PackageID, Version = newPackagePart.PackageVersion };
            _mockedOdataContext.SetupGet(oc => oc.Packages).Returns(new[] { publishedPackage }.AsQueryable());
            _mockedContentManager.Setup(cm => cm.New(It.IsAny<string>())).Returns(contentItem);

            _creator.CreateNewPackagePart(new PackageLogEntry { PackageId = newPackagePart.PackageID, PackageVersion = newPackagePart.PackageVersion});

            _mockedRecommendedVersionUpdater.Verify(rvu => rvu.SetRecommendedVersionFlagsOfOtherPackagesWithSameId(newPackagePart));
        }

        [Test]
        public void ShouldCreateNewPackagePartWhenNoExistingOrchardVersionsOfPackageExist() {
            PackagePart newPackagePart = new PackagePart { Record = new PackagePartRecord(), PackageID = "foo", PackageVersion = "1.0" };
            var contentItem = new ContentItem { ContentType = "Package" };
            contentItem.Weld(newPackagePart);
            PublishedPackage publishedPackage = new PublishedPackage { Id = newPackagePart.PackageID, Version = newPackagePart.PackageVersion };
            _mockedOdataContext.SetupGet(oc => oc.Packages).Returns(new[] { publishedPackage }.AsQueryable());
            _mockedContentManager.Setup(cm => cm.New(It.IsAny<string>())).Returns(contentItem);
            _mockedPackageService.Setup(ps => ps.PackageExists(newPackagePart.PackageID, newPackagePart.PackageVersion, It.IsAny<VersionOptions>())).Returns(false);

            _creator.CreateNewPackagePart(new PackageLogEntry { PackageId = newPackagePart.PackageID, PackageVersion = newPackagePart.PackageVersion });

            _mockedContentManager.Verify(cm => cm.Create(It.IsAny<ContentItem>(), It.IsAny<VersionOptions>()), Times.Once());
        }

        [Test]
        public void ShouldNotCreateNewPackagePartWhenAnyExistingOrchardVersionOfPackageExists()
        {
            PackagePart newPackagePart = new PackagePart { Record = new PackagePartRecord(), PackageID = "foo", PackageVersion = "1.0" };
            var contentItem = new ContentItem { ContentType = "Package" };
            contentItem.Weld(newPackagePart);
            PublishedPackage publishedPackage = new PublishedPackage { Id = newPackagePart.PackageID, Version = newPackagePart.PackageVersion };
            _mockedOdataContext.SetupGet(oc => oc.Packages).Returns(new[] { publishedPackage }.AsQueryable());
            _mockedContentManager.Setup(cm => cm.New(It.IsAny<string>())).Returns(contentItem);
            _mockedPackageService.Setup(ps => ps.PackageExists(newPackagePart.PackageID, newPackagePart.PackageVersion, It.IsAny<VersionOptions>())).Returns(true);

            _creator.CreateNewPackagePart(new PackageLogEntry { PackageId = newPackagePart.PackageID, PackageVersion = newPackagePart.PackageVersion });

            _mockedContentManager.Verify(cm => cm.Create(It.IsAny<ContentItem>(), It.IsAny<VersionOptions>()), Times.Never());
        }

        [Test]
        public void ShouldLogMessageWhenPackageIsCreated()
        {
            PackagePart newPackagePart = new PackagePart { Record = new PackagePartRecord(), PackageID = "foo", PackageVersion = "1.0" };
            var contentItem = new ContentItem { ContentType = "Package" };
            contentItem.Weld(newPackagePart);
            PublishedPackage publishedPackage = new PublishedPackage { Id = newPackagePart.PackageID, Version = newPackagePart.PackageVersion };

            _mockedOdataContext.SetupGet(oc => oc.Packages).Returns(new[] { publishedPackage }.AsQueryable());
            _mockedContentManager.Setup(cm => cm.New(It.IsAny<string>())).Returns(contentItem);

            _creator.CreateNewPackagePart(new PackageLogEntry { PackageId = newPackagePart.PackageID, PackageVersion = newPackagePart.PackageVersion });

            _mockedLogger.Verify(l => l.Log(LogLevel.Information, null, It.Is<string>(s => s.StartsWith("Created PackagePart")),
                It.IsAny<object[]>()), Times.Once());
        }

        [Test]
        public void ShouldLogMessageWhenPackageDoesNotExistOnFeed()
        {
            PackagePart newPackagePart = new PackagePart { Record = new PackagePartRecord(), PackageID = "foo", PackageVersion = "1.0" };
            var contentItem = new ContentItem { ContentType = "Package" };
            contentItem.Weld(newPackagePart);

            _mockedOdataContext.SetupGet(oc => oc.Packages).Returns(new PublishedPackage[0].AsQueryable());
            _mockedContentManager.Setup(cm => cm.New(It.IsAny<string>())).Returns(contentItem);

            _creator.CreateNewPackagePart(new PackageLogEntry { PackageId = newPackagePart.PackageID, PackageVersion = newPackagePart.PackageVersion });

            _mockedLogger.Verify(l => l.Log(LogLevel.Information, null, It.Is<string>(s => s.Contains("not found on the Feed")),
                It.IsAny<object[]>()), Times.Once());
        }

        [Test]
        public void ShouldLogMessageWhenPackagePartAlreadyExists()
        {
            PackagePart newPackagePart = new PackagePart { Record = new PackagePartRecord(), PackageID = "foo", PackageVersion = "1.0" };
            var contentItem = new ContentItem { ContentType = "Package" };
            contentItem.Weld(newPackagePart);

            PublishedPackage publishedPackage = new PublishedPackage { Id = newPackagePart.PackageID, Version = newPackagePart.PackageVersion };

            _mockedOdataContext.SetupGet(oc => oc.Packages).Returns(new[] { publishedPackage }.AsQueryable());
            _mockedContentManager.Setup(cm => cm.New(It.IsAny<string>())).Returns(contentItem);
            _mockedPackageService.Setup(ps => ps.PackageExists(newPackagePart.PackageID, newPackagePart.PackageVersion, It.IsAny<VersionOptions>()))
                .Returns(true);

            _creator.CreateNewPackagePart(new PackageLogEntry { PackageId = newPackagePart.PackageID, PackageVersion = newPackagePart.PackageVersion });

            _mockedLogger.Verify(l => l.Log(LogLevel.Information, null, It.Is<string>(s => s.Contains("already exists in the gallery")),
                It.IsAny<object[]>()), Times.Once());
        }
    }
}