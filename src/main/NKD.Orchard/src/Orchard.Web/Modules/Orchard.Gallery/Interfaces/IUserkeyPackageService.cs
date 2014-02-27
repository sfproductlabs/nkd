using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Orchard.Gallery.Models;
using Orchard.Security;

namespace Orchard.Gallery.Interfaces {
    public interface IUserkeyPackageService : IDependency {
        bool PackageIdIsRegistered(string packageId);
        void RegisterPackageId(string packageId, int userId);
        bool UserCanAccessPackage(string packageId, int userId);
        bool KeyCanAccessPackage(string packageId, string accessKey, bool claimPackageIdIfAvailable = true);
        IEnumerable<PackagePart> GetPackagesByUserkey(int userKeyId, bool includeAllVersions = false);
        IEnumerable<PackagePart> GetPackagesByUserkey(int userKeyId, int startingIndex, int pageSize, Expression<Func<PackagePartRecord, bool>> filter = null);
        IEnumerable<UserkeyPackage> GetUserkeyPackagesForUser(int userId, bool includeImplicitOwnership = true);
        IEnumerable<UserkeyPackage> GetPackageIdsThatAreNotPackageParts();
        IEnumerable<IUser> GetAllOwnersForPackage(string packageId);
        IEnumerable<IUser> GetContactableOwnersForPackage(string packageId);
        void RemovePackageIdRegistration(string packageId, int userId);
        int CountOfUsersPackages(int userKeyId, Func<PackagePart, bool> filter = null);
        void DeletePackageIdRegistration(string packageId);
    }
}