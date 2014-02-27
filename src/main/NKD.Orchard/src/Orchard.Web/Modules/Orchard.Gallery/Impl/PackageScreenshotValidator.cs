using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Gallery.Exceptions;
using Orchard.Gallery.Interfaces;

namespace Orchard.Gallery.Impl {
    public class PackageScreenshotValidator : IPackageScreenshotValidator
    {
        private readonly IList<string> _allowableExtensions = new[] { "png", "jpg", "jpeg", "gif", "bmp" };

        public void ValidateProjectScreenshot(string fileExtension) {
            if (!_allowableExtensions.Any(ae => string.Equals(fileExtension, ae, StringComparison.OrdinalIgnoreCase))) {
                throw new InvalidPackageScreenshotFileException();
            }
        }
    }
}