using System;
using System.Collections.Generic;
using Gallery.Core.Domain;
using Microsoft.Http;

namespace Orchard.Gallery.Interfaces {
    public interface IGalleryPackageService : IDependency {
        Package CreatePackage(Func<HttpClient, HttpResponseMessage> createPackagePostMethod);
        Package GetPackage(string packageId, string packageVersion);
        void UpdatePackage(Package packageToUpdate);
        IEnumerable<Package> GetUnfinishedPackages(IEnumerable<string> packageIDs, Guid accessKey);
        IEnumerable<PackageLogEntry> GetNewPackageLogs(int lastPackageLogId);
        void DeletePackage(string packageId, string packageVersion);
        void DeleteUnfinishedPackages(string packageId);
    }
}