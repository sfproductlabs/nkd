using Gallery.Core.Domain;
using NUnit.Framework;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.ViewModels;

namespace Orchard.Gallery.UnitTests.PackageViewModelMapper {
    [TestFixture]
    public class MapPackageToViewModelTester {
        private readonly IPackageViewModelMapper _packageViewModelMapper = new Impl.PackageViewModelMapper(null);

        [Test]
        public void BasicPropertiesShouldBeMapped() {
            Package packageToMapFrom = new Package {
                Id = "Id",
                Version = "Version",
                Title = "Title",
                Summary = "Summary",
                Description = "Description",
                Authors = "Authors",
                LicenseUrl = "LicenseUrl",
                PackageType = "PackageType",
                Tags = "Tags",
                IsLatestVersion = false,
                ExternalPackageUrl = "ExternalPackageUrl",
                ProjectUrl = "ProjectUrl",
                RequireLicenseAcceptance = true,
                Copyright = "Copyright",
            };

            PackageViewModel packageViewModel = _packageViewModelMapper.MapPackageToViewModel(packageToMapFrom, false);

            Assert.AreEqual(packageToMapFrom.Id, packageViewModel.PackageId, "PackageId was not mapped correctly.");
            Assert.AreEqual(packageToMapFrom.Version, packageViewModel.PackageVersion, "PackageVersion was not mapped correctly.");
            Assert.AreEqual(packageToMapFrom.Title, packageViewModel.Title, "Title was not mapped correctly.");
            Assert.AreEqual(packageToMapFrom.Summary, packageViewModel.Summary, "Summary was not mapped correctly.");
            Assert.AreEqual(packageToMapFrom.Description, packageViewModel.Description, "Description was not mapped correctly.");
            Assert.AreEqual(packageToMapFrom.Authors, packageViewModel.Authors, "Authors was not mapped correctly.");
            Assert.AreEqual(packageToMapFrom.LicenseUrl, packageViewModel.LicenseURL, "LicenseURL was not mapped correctly.");
            Assert.AreEqual(packageToMapFrom.PackageType, packageViewModel.PackageType, "PackageType was not mapped correctly.");
            Assert.AreEqual(packageToMapFrom.Tags, packageViewModel.Tags, "Tags was not mapped correctly.");
            Assert.AreEqual(packageToMapFrom.IsLatestVersion, packageViewModel.IsLatestVersion, "IsLatestVersion was not mapped correctly.");
            Assert.AreEqual(packageToMapFrom.ExternalPackageUrl, packageViewModel.ExternalPackageUrl,
                "ExternalPackageUrl was not mapped correctly.");
            Assert.AreEqual(packageToMapFrom.ProjectUrl, packageViewModel.ProjectUrl, "ProjectUrl was not mapped correctly.");
            Assert.AreEqual(packageToMapFrom.RequireLicenseAcceptance, packageViewModel.RequireLicenseAcceptance,
                "RequireLicenseAcceptance was not mapped correctly.");
            Assert.AreEqual(packageToMapFrom.Copyright, packageViewModel.Copyright, "Copyright was not mapped correctly.");
        }

        [TestCase(null)]
        [TestCase("")]
        public void PrimaryCategoryShouldBeEmptyStringWhenCategoriesIsNullOrEmpty(string categories) {
            Package packageToMapFrom = new Package {Categories = categories};

            PackageViewModel packageViewModel = _packageViewModelMapper.MapPackageToViewModel(packageToMapFrom, false);

            Assert.IsEmpty(packageViewModel.PrimaryCategory, "PrimaryCategory should be empty.");
        }

        [Test]
        public void PrimaryCategoryShouldBeFirstCategoryInListOfCategories() {
            const string expectedCategory = "Fish";
            Package packageToMapFrom = new Package { Categories = string.Format("{0},Chips,Meat", expectedCategory) };

            PackageViewModel packageViewModel = _packageViewModelMapper.MapPackageToViewModel(packageToMapFrom, false);

            Assert.AreEqual(expectedCategory, packageViewModel.PrimaryCategory, "PrimaryCategory was not mapped correctly.");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void IsExternalPackageShouldBeFalseWhenExternalPackageUrlIsNullEmptyOrWhiteSpace(string externalPackageUrl)
        {
            Package packageToMapFrom = new Package { ExternalPackageUrl = externalPackageUrl };

            PackageViewModel packageViewModel = _packageViewModelMapper.MapPackageToViewModel(packageToMapFrom, false);

            Assert.IsFalse(packageViewModel.IsExternalPackage, "IsExternalPackage should be false when ExternalPackageUrl is not present.");
        }

        [Test]
        public void IsExternalPackageShouldBeTrueWhenExternalPackageUrlHasValue()
        {
            Package packageToMapFrom = new Package { ExternalPackageUrl = "http://google.com" };

            PackageViewModel packageViewModel = _packageViewModelMapper.MapPackageToViewModel(packageToMapFrom, false);

            Assert.IsTrue(packageViewModel.IsExternalPackage, "IsExternalPackage should be true when ExternalPackageUrl is present.");
        }

        [TestCase(false)]
        [TestCase(true)]
        public void IsNewPackageShouldBeSetToGivenValue(bool isNewPackage)
        {
            PackageViewModel packageViewModel = _packageViewModelMapper.MapPackageToViewModel(new Package(), isNewPackage);

            Assert.AreEqual(isNewPackage, packageViewModel.IsNewPackage, "IsNewPackage was not mapped correctly.");
        }
    }
}