using Gallery.Core.Domain;
using Gallery.Core.Enums;
using Moq;
using NUnit.Framework;
using Orchard.ContentManagement;
using Orchard.Gallery.GalleryServer;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;
using Orchard.Gallery.PackageSynchronization;
using System.Linq;
using Orchard.Tasks.Indexing;

namespace Orchard.Gallery.UnitTests.PackageSynchronization.PackagePartUpdater {
    [TestFixture]
    public class ModifyExistingPackagePartTester {
        private const int TOTAL_DOWNLOAD_COUNT = 12;

        private IPackagePartUpdater _updater;
        private Mock<IPackageService> _mockedPackageService;
        private Mock<IPackagePartMapper> _mockedPackagePartMapper;
        private Mock<IOrchardServices> _mockedOrchardServices;
        private Mock<IODataContext> _mockedODataContext;
        private Mock<IPackagePartExtendedInfoUpdater> _mockedPackagePartExtendedInfoUpdater;
        private Mock<IContentManager> _mockedContentManager;
        private Mock<IRecommendedVersionUpdater> _mockedRecommendedVersionUpdater;
        private Mock<IIndexingTaskManager> _mockedIndexingTaskManager;
        private Mock<IPackagePartPublishingService> _mockedPackagePartPublishingService;

        [SetUp]
        public void Setup() {
            _mockedPackageService = new Mock<IPackageService>();
            _mockedPackagePartMapper = new Mock<IPackagePartMapper>();
            _mockedOrchardServices = new Mock<IOrchardServices>();
            _mockedODataContext = new Mock<IODataContext>();
            _mockedPackagePartExtendedInfoUpdater = new Mock<IPackagePartExtendedInfoUpdater>();
            _mockedContentManager = new Mock<IContentManager>();
            _mockedRecommendedVersionUpdater = new Mock<IRecommendedVersionUpdater>();
            _mockedPackagePartPublishingService = new Mock<IPackagePartPublishingService>();

            _mockedODataContext.SetupGet(oc => oc.Packages).Returns(
                new[] {
                    new PublishedPackage { Id = "Pack1", Version = "V1"},
                    new PublishedPackage { Id = "Pack1", Version = "V2"},
                    new PublishedPackage { Id = "Pack2", Version = "V1"},
                    new PublishedPackage { Id = "Pack3", Version = "V1", DownloadCount = TOTAL_DOWNLOAD_COUNT, VersionDownloadCount = 5},
                    new PublishedPackage { Id = "Pack3", Version = "V2", DownloadCount = TOTAL_DOWNLOAD_COUNT, VersionDownloadCount = 6},
                    new PublishedPackage { Id = "Pack3", Version = "V3", DownloadCount = TOTAL_DOWNLOAD_COUNT, VersionDownloadCount = 7},
                }.AsQueryable());

            _mockedOrchardServices.SetupGet(os => os.ContentManager).Returns(_mockedContentManager.Object);
            _mockedIndexingTaskManager = new Mock<IIndexingTaskManager>();
            _updater = new Gallery.PackageSynchronization.PackagePartUpdater(_mockedPackageService.Object, _mockedPackagePartMapper.Object,
                _mockedODataContext.Object, _mockedPackagePartExtendedInfoUpdater.Object, _mockedRecommendedVersionUpdater.Object,
                _mockedIndexingTaskManager.Object, _mockedPackagePartPublishingService.Object);
        }

        private static PackagePart GetPackagePart(string packageId = "packId", string packageVersion = "1.0")
        {
            return new PackagePart { Record = new PackagePartRecord(), PackageID = packageId, PackageVersion = packageVersion};
        }

        [Test]
        public void ShouldUpdatePackagePartOnlyWhenGivenUpdateLogAction() {
            PackageLogEntry entryToProcess = new PackageLogEntry { PackageId = "Pack1", PackageVersion = "V1", Action = PackageLogAction.Update };
            _mockedPackageService.Setup(ps => ps.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(new PackagePart());

            _updater.ModifyExistingPackagePart(entryToProcess);

            _mockedIndexingTaskManager.Verify(itm => itm.CreateUpdateIndexTask(It.IsAny<ContentItem>()), Times.Once());
        }

        [Test]
        public void ShouldPublishPackagePartWhenGivenRePublishLogAction()
        {
            PackageLogEntry log = new PackageLogEntry { PackageId = "Pack1", PackageVersion = "V1", Action = PackageLogAction.RePublish };
            var packagePart = GetPackagePart(log.PackageId, log.PackageVersion);
            _mockedPackageService.Setup(ps => ps.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(packagePart);

            _updater.ModifyExistingPackagePart(log);

            _mockedPackagePartPublishingService.Verify(ppps => ppps.Publish(packagePart.PackageID, packagePart.PackageVersion), Times.Once());
        }

        [Test]
        public void ShouldNotPublishPackageWhenGivenLogActionIsNotRePublish()
        {
            PackageLogEntry log = new PackageLogEntry { PackageId = "Pack1", PackageVersion = "V1", Action = PackageLogAction.Update };
            var packagePart = GetPackagePart(log.PackageId, log.PackageVersion);
            _mockedPackageService.Setup(ps => ps.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(packagePart);

            _updater.ModifyExistingPackagePart(log);

            _mockedPackagePartPublishingService.Verify(ppps => ppps.Publish(packagePart.PackageID, packagePart.PackageVersion), Times.Never());
        }

        [Test]
        public void ShouldUpdateAllPackagePartsWithSameIdWhenGivenDownloadLogAction()
        {
            PackageLogEntry entryToProcess = new PackageLogEntry { PackageId = "Pack3", PackageVersion = "V2", Action = PackageLogAction.Download };
            _mockedPackageService.Setup(ps => ps.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(GetPackagePart());

            _updater.ModifyExistingPackagePart(entryToProcess);

            _mockedIndexingTaskManager.Verify(itm => itm.CreateUpdateIndexTask(It.IsAny<ContentItem>()), Times.Exactly(3));
        }

        [Test]
        public void ShouldNotMapPackagePartWhenGivenDownloadLogAction() {
            PackageLogEntry entryToProcess = new PackageLogEntry { PackageId = "Pack1", PackageVersion = "V1", Action = PackageLogAction.Download };
            _mockedPackageService.Setup(ps => ps.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(GetPackagePart());

            _updater.ModifyExistingPackagePart(entryToProcess);

            _mockedPackagePartMapper.Verify(ppm => ppm.MapPublishedPackageToPackagePart(It.IsAny<PublishedPackage>(), It.IsAny<PackagePart>()),
                Times.Never());
        }

        [Test]
        public void ShouldMapTotalDownloadCountForAllPackagesWithSameIdWhenGivenDownloadLogAction() {
            PackageLogEntry entryToProcess = new PackageLogEntry { PackageId = "Pack3", PackageVersion = "V2", Action = PackageLogAction.Download };
            var package1 = new PackagePart { Record = new PackagePartRecord()};
            var package2 = new PackagePart { Record = new PackagePartRecord()};
            var package3 = new PackagePart { Record = new PackagePartRecord()};

            _mockedPackageService.Setup(ps => ps.Get("Pack3", "V1", It.IsAny<bool>())).Returns(package1);
            _mockedPackageService.Setup(ps => ps.Get("Pack3", "V2", It.IsAny<bool>())).Returns(package2);
            _mockedPackageService.Setup(ps => ps.Get("Pack3", "V3", It.IsAny<bool>())).Returns(package3);

            _updater.ModifyExistingPackagePart(entryToProcess);

            Assert.AreEqual(TOTAL_DOWNLOAD_COUNT, package1.TotalDownloadCount);
            Assert.AreEqual(TOTAL_DOWNLOAD_COUNT, package2.TotalDownloadCount);
            Assert.AreEqual(TOTAL_DOWNLOAD_COUNT, package3.TotalDownloadCount);
        }

        [Test]
        public void ShouldMapDownloadCountForAllPackagesWithSameIdWhenGivenDownloadLogAction()
        {
            PackageLogEntry entryToProcess = new PackageLogEntry { PackageId = "Pack3", PackageVersion = "V2", Action = PackageLogAction.Download };
            var package1 = new PackagePart { Record = new PackagePartRecord() };
            var package2 = new PackagePart { Record = new PackagePartRecord() };
            var package3 = new PackagePart { Record = new PackagePartRecord() };

            _mockedPackageService.Setup(ps => ps.Get("Pack3", "V1", It.IsAny<bool>())).Returns(package1);
            _mockedPackageService.Setup(ps => ps.Get("Pack3", "V2", It.IsAny<bool>())).Returns(package2);
            _mockedPackageService.Setup(ps => ps.Get("Pack3", "V3", It.IsAny<bool>())).Returns(package3);

            _updater.ModifyExistingPackagePart(entryToProcess);

            Assert.AreEqual(5, package1.DownloadCount);
            Assert.AreEqual(6, package2.DownloadCount);
            Assert.AreEqual(7, package3.DownloadCount);
        }

        [Test]
        public void ShouldCallRecommendedVersionUpdaterForPackageUpdateAction() {
            PackageLogEntry entryToProcess = new PackageLogEntry { PackageId = "Pack1", PackageVersion = "V1", Action = PackageLogAction.Update };
            var existingPackage = new PackagePart();
            _mockedPackageService.Setup(ps => ps.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(existingPackage);

            _mockedODataContext.SetupGet(oc => oc.Packages).Returns(
                new[] {
                    new PublishedPackage { Id = "Pack1", Version = "V1" },
                }.AsQueryable());

            _updater.ModifyExistingPackagePart(entryToProcess);

            _mockedRecommendedVersionUpdater.Verify(rvu => rvu.SetRecommendedVersionFlagsOfOtherPackagesWithSameId(existingPackage), Times.Once());
        }

        [Test]
        public void ShouldNotCallRecommendedVersionUpdaterForPackageDownloadAction()
        {
            const PackageLogAction packageLogAction = PackageLogAction.Download;
            PackageLogEntry entryToProcess = new PackageLogEntry { PackageId = "Pack1", PackageVersion = "V1", Action = packageLogAction };
            _mockedPackageService.Setup(ps => ps.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(GetPackagePart());

            _mockedODataContext.SetupGet(oc => oc.Packages).Returns(
                new[] {
                    new PublishedPackage { Id = "Pack1", Version = "V1", IsLatestVersion = true },
                }.AsQueryable());

            _updater.ModifyExistingPackagePart(entryToProcess);

            _mockedRecommendedVersionUpdater.Verify(rvu => rvu.SetRecommendedVersionFlagsOfOtherPackagesWithSameId(It.IsAny<PackagePart>()), Times.Never());
        }
    }
}