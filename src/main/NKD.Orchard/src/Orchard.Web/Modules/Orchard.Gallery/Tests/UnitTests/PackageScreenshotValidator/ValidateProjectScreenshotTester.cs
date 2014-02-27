using NUnit.Framework;
using Orchard.Gallery.Exceptions;
using Orchard.Gallery.Interfaces;

namespace Orchard.Gallery.UnitTests.PackageScreenshotValidator
{
    [TestFixture]
    public class ValidateProjectScreenshotTester
    {
        private readonly IPackageScreenshotValidator _packageScreenshotValidator = new Impl.PackageScreenshotValidator();

        [TestCase("png")]
        [TestCase("jpg")]
        [TestCase("jpeg")]
        [TestCase("gif")]
        [TestCase("bmp")]
        public void ShouldAllowImageFileExtensions(string fileExtension)
        {
            TestDelegate methodToTest = () => _packageScreenshotValidator.ValidateProjectScreenshot(fileExtension);

            Assert.DoesNotThrow(methodToTest, "Image file extenion '{0}' should have been considered valid.");
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void ShouldThrowWhenGivenNullOrEmptyOrWhiteSpaceExtension(string fileExtension)
        {
            TestDelegate methodToTest = () => _packageScreenshotValidator.ValidateProjectScreenshot(fileExtension);

            Assert.Throws<InvalidPackageScreenshotFileException>(methodToTest,
                "A null or empty file extension should have caused an exception to have been thrown.");
        }

        [TestCase("txt")]
        [TestCase("exe")]
        [TestCase("zip")]
        [TestCase("mpeg")]
        public void ShouldNotAllowNotImageFileExtensions(string fileExtension)
        {
            TestDelegate methodToTest = () => _packageScreenshotValidator.ValidateProjectScreenshot(fileExtension);

            Assert.Throws<InvalidPackageScreenshotFileException>(methodToTest, "A non-image file extension should have caused an exception to have been thrown.");
        }
    }
}