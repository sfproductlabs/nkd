using System;
using Gallery.Core.Domain;
using Moq;
using NUnit.Framework;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.ViewModels;
using Orchard.Services;

namespace Orchard.Gallery.UnitTests.PackageViewModelMapper {
    [TestFixture]
    public class MapViewModelToPackageTester {
        private IPackageViewModelMapper _packageViewModelMapper;
        private Mock<IClock> _mockedClock;

        [SetUp]
        public void SetUp() {
            _mockedClock = new Mock<IClock>();
            _packageViewModelMapper = new Impl.PackageViewModelMapper(_mockedClock.Object);
        }

        [Test]
        public void BasicPropertiesShouldBeMapped() {
            PackageViewModel packageViewModelToMapFrom = new PackageViewModel {
                PackageId = "packageId",
                PackageVersion = "packageVersion",
                Title = "title",
                Summary = "summary",
                Description = "description",
                Authors = "authors",
                LicenseURL = "licenseUrl",
                PackageType = "packageType",
                IsLatestVersion = true,
                ExternalPackageUrl = "external",
                ProjectUrl = "project",
                RequireLicenseAcceptance = false,
                Copyright = "copy",
            };
            Package packageToMapTo = new Package();

            _packageViewModelMapper.MapViewModelToPackage(packageViewModelToMapFrom, packageToMapTo);

            Assert.AreEqual(packageViewModelToMapFrom.PackageId, packageToMapTo.Id, "Id was not mapped correctly.");
            Assert.AreEqual(packageViewModelToMapFrom.PackageVersion, packageToMapTo.Version, "Version was not mapped correctly.");
            Assert.AreEqual(packageViewModelToMapFrom.Title, packageToMapTo.Title, "Title was not mapped correctly.");
            Assert.AreEqual(packageViewModelToMapFrom.Summary, packageToMapTo.Summary, "Summary was not mapped correctly.");
            Assert.AreEqual(packageViewModelToMapFrom.Description, packageToMapTo.Description, "Description was not mapped correctly.");
            Assert.AreEqual(packageViewModelToMapFrom.Authors, packageToMapTo.Authors, "Authors was not mapped correctly.");
            Assert.AreEqual(packageViewModelToMapFrom.LicenseURL, packageToMapTo.LicenseUrl, "LicenseUrl was not mapped correctly.");
            Assert.AreEqual(packageViewModelToMapFrom.PackageType, packageToMapTo.PackageType, "PackageType was not mapped correctly.");
            Assert.AreEqual(packageViewModelToMapFrom.IsLatestVersion, packageToMapTo.IsLatestVersion, "IsLatestVersion was not mapped correctly.");
            Assert.AreEqual(packageViewModelToMapFrom.ExternalPackageUrl, packageToMapTo.ExternalPackageUrl,
                "ExternalPackageUrl was not mapped correctly.");
            Assert.AreEqual(packageViewModelToMapFrom.ProjectUrl, packageToMapTo.ProjectUrl, "ProjectUrl was not mapped correctly.");
            Assert.AreEqual(packageViewModelToMapFrom.RequireLicenseAcceptance, packageToMapTo.RequireLicenseAcceptance,
                "RequireLicenseAcceptance was not mapped correctly.");
            Assert.AreEqual(packageViewModelToMapFrom.Copyright, packageToMapTo.Copyright, "Copyright was not mapped correctly.");
        }

        [Test]
        public void ShouldSetLastUpdatedToUtcNow() {
            DateTime expectedLastUpdatedDate = new DateTime(2003, 3, 3);
            _mockedClock.SetupGet(c => c.UtcNow).Returns(expectedLastUpdatedDate);
            Package packageToMapTo = new Package();

            _packageViewModelMapper.MapViewModelToPackage(new PackageViewModel(), packageToMapTo);

            Assert.AreEqual(expectedLastUpdatedDate, packageToMapTo.LastUpdated, "LastUpdated should be set to the current time UTC.");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void TagsShouldBeSetToGivenTagsWhenTheyAreNullEmptyOrWhiteSpace(string tags) {
            Package packageToMapTo = new Package();

            _packageViewModelMapper.MapViewModelToPackage(new PackageViewModel { Tags = tags}, packageToMapTo);

            Assert.AreEqual(tags, packageToMapTo.Tags, "Tags should have been equal to each other.");
        }

        [Test]
        public void ShouldReplaceCommasWithSpacesInTags() {
            const string tags = "one,two,three";
            const string expectedTags = "one two three";
            Package packageToMapTo = new Package();

            _packageViewModelMapper.MapViewModelToPackage(new PackageViewModel { Tags = tags }, packageToMapTo);

            Assert.AreEqual(expectedTags, packageToMapTo.Tags, "Tags were not formatted correctly.");
        }

        [Test]
        public void ShouldTrimSpacesAroundTagsSeparatedByCommas()
        {
            const string tags = "   one,two,three  ";
            const string expectedTags = "one two three";
            Package packageToMapTo = new Package();

            _packageViewModelMapper.MapViewModelToPackage(new PackageViewModel { Tags = tags }, packageToMapTo);

            Assert.AreEqual(expectedTags, packageToMapTo.Tags, "Tags were not formatted correctly.");
        }
    }
}