using System;
using System.IO;
using Orchard.Gallery.Interfaces;

namespace Orchard.Gallery.Impl {
    public class PackageMediaDirectoryHelper : IPackageMediaDirectoryHelper {
        private const string PackageBaseDirectoryFormatString = @"Packages\{0}\{1}";
        // HACK: Is there a better way to retrieve the "Media\Default" string?
        private const string MediaDirectory = @"Media\Default";

        public string GetPackageIconDirectory(string packageId, string packageVersion) {
            return string.Format(PackageBaseDirectoryFormatString, packageId, packageVersion);
        }

        public string GetAbsolutePathToPackageIconDirectory(string packageId, string packageVersion) {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, MediaDirectory, GetPackageIconDirectory(packageId, packageVersion));
        }

        public string GetPackageScreenshotsDirectory(string packageId, string packageVersion) {
            return string.Format(PackageBaseDirectoryFormatString + @"\Screenshots", packageId, packageVersion);
        }

        public string GetAbsolutePathtoPackageScreenshotsDirectory(string packageId, string packageVersion) {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, MediaDirectory, GetPackageScreenshotsDirectory(packageId, packageVersion));
        }

        public string GetAbsolutePathToPackageMediaDirectory(string packageId, string packageVersion) {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, MediaDirectory, string.Format(PackageBaseDirectoryFormatString, packageId, packageVersion));
        }
    }
}