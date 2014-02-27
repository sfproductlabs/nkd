using Gallery.Core.Domain;
using Gallery.Core.Enums;
using Gallery.Core.Exceptions;
using Moq;
using NUnit.Framework;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;

namespace Orchard.Gallery.UnitTests.PackageSynchronization.PackagePartUnpublisher {
    [TestFixture]
    public class UnpublishTester {
        private Gallery.PackageSynchronization.PackagePartPublishingService _publishingService;
        private Mock<IPackageService> _mockedPackageService;
        private Mock<IContentManager> _mockedContentManager;

        private PackagePart _existingPackagePart;
        private PackageLogEntry _log;

        [SetUp]
        public void Setup() {
            _mockedContentManager = new Mock<IContentManager>();
            _mockedPackageService = new Mock<IPackageService>();

            _existingPackagePart = new PackagePart {
                Record = new PackagePartRecord { PackageID = "PackageId", PackageVersion = "1.3" },
                ContentItem = new ContentItem { VersionRecord = new ContentItemVersionRecord { Published = true} }
            };
            _log = new PackageLogEntry { PackageId = _existingPackagePart.PackageID, PackageVersion = _existingPackagePart.PackageVersion,
                Action = PackageLogAction.Unpublish };
            _mockedPackageService.Setup(ps => ps.Get(_existingPackagePart.PackageID, _existingPackagePart.PackageVersion, true))
                .Returns(_existingPackagePart);

            _publishingService = new Gallery.PackageSynchronization.PackagePartPublishingService(_mockedContentManager.Object, _mockedPackageService.Object);
        }

        [Test]
        public void ShouldRetrievePackagePartFromPackageService() {
            _publishingService.Unpublish(_log.PackageId, _log.PackageVersion);

            _mockedPackageService.Verify(ps => ps.Get(_log.PackageId, _log.PackageVersion, true), Times.Once());
        }

        [Test]
        public void ShouldUnpublishContentItem() {
            _publishingService.Unpublish(_log.PackageId, _log.PackageVersion);

            _mockedContentManager.Verify(cm => cm.Unpublish(_existingPackagePart.ContentItem), Times.Once());
        }

        [Test]
        public void ShouldThrowWhenPackagePartNotFound() {
            _log.PackageId = "Foo";

            TestDelegate methodThatShouldThrow = () => _publishingService.Unpublish(_log.PackageId, _log.PackageVersion);

            Assert.Throws<PackageDoesNotExistException>(methodThatShouldThrow);
        }

        [Test]
        public void ShouldNotResetRecommendedVersionWhenPackageToUnpublishIsNotRecommended() {
            _existingPackagePart.IsRecommendedVersion = false;

            _publishingService.Unpublish(_log.PackageId, _log.PackageVersion);

            _mockedPackageService.Verify(ps => ps.ResetRecommendedVersionForPackage(_existingPackagePart), Times.Never());
        }

        [Test]
        public void ShouldResetRecommendedVersionWhenPackageToUnpublishIsTheRecommended() {
            _existingPackagePart.IsRecommendedVersion = true;

            _publishingService.Unpublish(_log.PackageId, _log.PackageVersion);

            _mockedPackageService.Verify(ps => ps.ResetRecommendedVersionForPackage(_existingPackagePart), Times.Once());
        }

        [Test]
        public void ShouldNotUnpublishWhenUnpublishedPackageFound() {
            _existingPackagePart.ContentItem.VersionRecord.Published = false;

            _publishingService.Unpublish(_log.PackageId, _log.PackageVersion);

            _mockedContentManager.Verify(cm => cm.Unpublish(_existingPackagePart.ContentItem), Times.Never());
        }
    }
}