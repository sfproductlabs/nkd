using System;
using System.Text.RegularExpressions;
using Gallery.Core.Exceptions;
using Gallery.Core.Impl;
using Gallery.Core.Interfaces;
using JetBrains.Annotations;
using Orchard.Gallery.Interfaces;
using Orchard.Localization;

namespace Orchard.Gallery.Impl {
    [UsedImplicitly]
    public class ParameterFormatValidator : IParameterFormatValidator {
        private readonly IPackageIdValidator _packageIdValidator;
        private readonly IPackageVersionValidator _packageVersionValidator;
        private readonly IGalleryUriValidator _galleryUriValidator;

        public Localizer T { get; set; }

        public ParameterFormatValidator() {
            _packageIdValidator = new PackageIdValidator();
            _packageVersionValidator = new PackageVersionValidator();
            _galleryUriValidator = new GalleryUriValidator();
            T = NullLocalizer.Instance;
        }

        public void ValidatePackageIdFormat(string packageId) {
            if (!_packageIdValidator.IsValidPackageId(packageId)) {
                throw new InvalidPackageIdException(packageId);
            }
        }

        public void ValidatePackageVersionFormat(string packageVersion) {
            if (!_packageVersionValidator.IsValidPackageVersion(packageVersion)) {
                throw new InvalidPackageVersionException(packageVersion);
            }
        }

        public void ValidateUriFormat(string uri, UriKind allowedUriKind)
        {
            if (!_galleryUriValidator.IsValidUri(uri, allowedUriKind)) {
                throw new UriFormatException(uri);
            }
        }

        public void ValidateScreenshotIdFormat(string screenshotId) {
            int output;
            if (!int.TryParse(screenshotId, out output)) {
                ThrowNewArgumentException("screenshotId");
            }
        }

        public void ValidateSlugFormat(string slug) {
            var regex = new Regex(@"^[a-z0-9]+([_-][a-z0-9]+)*$", RegexOptions.IgnoreCase);

            if ((slug == null) || (!regex.IsMatch(slug))) {
                    ThrowNewArgumentException("slug");
                }
            }

        public void ValidateUserKeyFormat(string key) {
            Guid output;
            if (!Guid.TryParse(key, out output)) {
                ThrowNewArgumentException("key");
            }
        }

        private static void ThrowNewArgumentException(string paramName) {
            throw new ArgumentException("Parameter is not valid.", paramName);
        }
    }
}