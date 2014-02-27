using Moq;
using NUnit.Framework;
using Orchard.ContentManagement;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;
using Orchard.Gallery.PackageSynchronization;
using Orchard.Tasks.Indexing;

namespace Orchard.Gallery.UnitTests.PackageSynchronization.RecommendedVersionUpdater {

    [TestFixture]
    public class SetRecommendedVersionFlagsOfOtherPackagesWithSameIdTester {

        private IRecommendedVersionUpdater _updater;
        private Mock<IPackageService> _mockedPackageService;

        [SetUp]
        public void Setup() {
            _mockedPackageService = new Mock<IPackageService>();
            _updater = new Gallery.PackageSynchronization.RecommendedVersionUpdater(_mockedPackageService.Object, new Mock<IIndexingTaskManager>().Object);
        }

        [Test]
        public void ShouldUpdateIsRecommendedVersionFieldForAllOtherPackagesWithTheSameId() {
            var recommendedPackage = new PackagePart {Record = new PackagePartRecord(), PackageID = "Pack1", PackageVersion = "V2", IsRecommendedVersion = true};
            var otherVersionOfPackage = new PackagePart { Record = new PackagePartRecord(), PackageID = "Pack1", PackageVersion = "V1", IsRecommendedVersion = true };
            _mockedPackageService.Setup(ps => ps.GetById(recommendedPackage.PackageID, It.IsAny<bool>()))
                .Returns(new[] { recommendedPackage, otherVersionOfPackage});

            _updater.SetRecommendedVersionFlagsOfOtherPackagesWithSameId(recommendedPackage);

            Assert.IsTrue(recommendedPackage.IsRecommendedVersion);
            Assert.IsFalse(otherVersionOfPackage.IsRecommendedVersion);
        }

        [Test]
        public void ShouldDoNothingWhenGivenPackageIsNotRecommendedVersion()
        {
            var recommendedPackage = new PackagePart { Record = new PackagePartRecord(), IsRecommendedVersion = false };

            _updater.SetRecommendedVersionFlagsOfOtherPackagesWithSameId(recommendedPackage);

            _mockedPackageService.Verify(ps => ps.GetById(recommendedPackage.PackageID, It.IsAny<bool>()), Times.Never());
        }
    }
}