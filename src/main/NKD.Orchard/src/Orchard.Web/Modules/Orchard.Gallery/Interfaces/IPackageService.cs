using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Orchard.ContentManagement;
using Orchard.Gallery.Models;

namespace Orchard.Gallery.Interfaces {
    public interface IPackageService : IDependency {
        PackagePart Get(string slug);
        PackagePart Get(string packageId, string packageVersion, bool includeAllVersions = false);
        IEnumerable<PackagePart> Get(Expression<Func<PackagePartRecord, bool>> filter = null, bool includeAllVersions = false);
        IEnumerable<PackagePart> Get(int startingIndex, int pageSize, Expression<Func<PackagePartRecord,bool>> filter = null);
        IEnumerable<PackagePart> GetById(string packageId, bool includeAllVersions = false);
        void Delete(string packageId, string packageVersion);
        int CountOfPackages(Expression<Func<PackagePartRecord,bool>> filter = null, bool includeAllVersions = false);
        bool PackageIdExists(string packageId);
        bool PackageExists(string packageId, string packageVersion, VersionOptions versionOptions = null);
        void ResetRecommendedVersionForPackage(PackagePart packagePart);
    }
}