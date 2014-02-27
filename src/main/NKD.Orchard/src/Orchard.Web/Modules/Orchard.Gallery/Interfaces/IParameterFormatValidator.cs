using System;

namespace Orchard.Gallery.Interfaces {
    public interface IParameterFormatValidator : IDependency {
        void ValidatePackageIdFormat(string packageId);
        void ValidatePackageVersionFormat(string packageVersion);
        void ValidateUriFormat(string uri, UriKind allowedUriKind);
        void ValidateScreenshotIdFormat(string screenshotId);
        void ValidateSlugFormat(string slug);
        void ValidateUserKeyFormat(string key);
    }
}