using Gallery.Core.Interfaces;
using Moq;
using NUnit.Framework;
using Orchard.ContentManagement;

using Orchard.Autoroute.Models;
using Orchard.Gallery.GalleryServer;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;

namespace Orchard.Gallery.UnitTests.PackagePartMapper
{
    [TestFixture]
    //[Ignore("this.As<RoutePart> within PackagePart.Title's setter throws a NullReferenceException.")]
    public class MapPublishedPackageToPackagePartTester
    {
        private readonly IPackagePartMapper _packagePartMapper = new Impl.PackagePartMapper(new Mock<IPackageSlugCreator>().Object);

        private void SetupRoutePart(PackagePart packagePartToMapTo) {
            var contentItem = new ContentItem { ContentType = "Package" };
            var routePart = new AutoroutePart {Record = new AutoroutePartRecord()};
            contentItem.Weld(routePart);
            contentItem.Weld(packagePartToMapTo);

        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void ShouldSetTitleToIdWhenPublishedPackageTitleIsNullOrEmpty(string title)
        {
            PublishedPackage publishedPackage = new PublishedPackage { Id="SomeId", Title = title};
            PackagePart packagePartToMapTo = new PackagePart {Record = new PackagePartRecord()};
            SetupRoutePart(packagePartToMapTo);

            _packagePartMapper.MapPublishedPackageToPackagePart(publishedPackage, packagePartToMapTo);

            Assert.AreEqual(publishedPackage.Id, packagePartToMapTo.Title, "Id should have been mapped to Title.");
        }

        [Test]
        public void ShouldSetDownloadCountToThePublishedPackageVersionDownloadCount() {
            //The names of these fields don't match up because they were changed on the gallery server feed not long before the sites went live,
            //and we didn't get the PackagePart updated before they went live. So we didn't want to rename columns on the live sites.
            PublishedPackage publishedPackage = new PublishedPackage {Id = "PackageId", Version = "Version", VersionDownloadCount = 5};
            PackagePart packagePartToMapTo = new PackagePart {Record = new PackagePartRecord()};
            SetupRoutePart(packagePartToMapTo);

            _packagePartMapper.MapPublishedPackageToPackagePart(publishedPackage, packagePartToMapTo);

            Assert.AreEqual(5, packagePartToMapTo.DownloadCount);
        }

        [Test]
        public void ShouldSetTotalDownloadCountToThePublishedPackageDownloadCount()
        {
            //The names of these fields don't match up because they were changed on the gallery server feed not long before the sites went live,
            //and we didn't get the PackagePart updated before they went live. So we didn't want to rename columns on the live sites.
            PublishedPackage publishedPackage = new PublishedPackage { Id = "PackageId", Version = "Version", DownloadCount = 27 };
            PackagePart packagePartToMapTo = new PackagePart { Record = new PackagePartRecord() };
            SetupRoutePart(packagePartToMapTo);

            _packagePartMapper.MapPublishedPackageToPackagePart(publishedPackage, packagePartToMapTo);

            Assert.AreEqual(27, packagePartToMapTo.TotalDownloadCount);
        }
    }
}