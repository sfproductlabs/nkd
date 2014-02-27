using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Gallery.Exceptions;
using Orchard.Gallery.Interfaces;

namespace Orchard.Gallery.Impl {
    public class PackageIconValidator : IPackageIconValidator {
        private readonly IList<string> _allowableExtensions = new[] { "png", "jpg", "jpeg", "gif", "bmp" };

        public void ValidateProjectIcon(string fileExtension) {
            if (!_allowableExtensions.Any(ae => string.Equals(fileExtension, ae, StringComparison.OrdinalIgnoreCase))) {
                throw new InvalidPackageIconFileException();
            }
        }
    }
}