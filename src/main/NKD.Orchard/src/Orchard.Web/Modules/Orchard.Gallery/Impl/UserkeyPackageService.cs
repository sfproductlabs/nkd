using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Gallery.Exceptions;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;
using Orchard.Security;
using Orchard.Services;
using Orchard.Users.Models;

namespace Orchard.Gallery.Impl {
    [UsedImplicitly]
    public class UserkeyPackageService : IUserkeyPackageService {
        private readonly IRepository<UserkeyPackage> _userkeyPackageRepository;
        private readonly IRepository<Userkey> _userkeyRepository;
        private readonly IUserkeyService _userkeyService;
        private readonly IOrchardServices _orchardServices;
        private readonly IAdminPackagePrivilegeChecker _packagePrivilegeChecker;
        private readonly IClock _clock;

        public UserkeyPackageService(IRepository<UserkeyPackage> userkeyPackageRepository, IRepository<Userkey> userkeyRepository,
            IUserkeyService userkeyService, IOrchardServices orchardServices, IAdminPackagePrivilegeChecker packagePrivilegeChecker, IClock clock) {
            _userkeyPackageRepository = userkeyPackageRepository;
            _packagePrivilegeChecker = packagePrivilegeChecker;
            _userkeyService = userkeyService;
            _orchardServices = orchardServices;
            _userkeyRepository = userkeyRepository;
            _clock = clock;
        }

        public bool PackageIdIsRegistered(string packageId) {
            return _userkeyPackageRepository.Count(up => up.PackageId == packageId) > 0;
        }

        public void RegisterPackageId(string packageId, int userId) {
            Userkey userkey = _userkeyRepository.Get(u => u.UserId == userId);
            int userkeyId = userkey != null ? userkey.Id : _userkeyService.SaveKeyForUser(userId, Guid.NewGuid()).Id;
            CreateUserkeyPackage(packageId, userkeyId);
        }

        private void RegisterPackageId(string packageId, string accessKey) {
            Userkey userkey = _userkeyService.GetUserkey(accessKey);
            if (userkey == null) {
                throw new AccessKeyDoesNotExistException(accessKey);
            }
            CreateUserkeyPackage(packageId, userkey.Id);
        }

        private void CreateUserkeyPackage(string packageId, int userkeyId) {
            DateTime? registeredUtc = _clock.UtcNow;
            var existingUserkeyPackage = _userkeyPackageRepository.Fetch(up => up.PackageId == packageId).FirstOrDefault();
            if (existingUserkeyPackage != null) {
                registeredUtc = existingUserkeyPackage.RegisteredUtc;
            }
            var userkeyPackage = new UserkeyPackage { PackageId = packageId, UserkeyId = userkeyId, RegisteredUtc = registeredUtc };
            _userkeyPackageRepository.Create(userkeyPackage);
        }

        public bool UserCanAccessPackage(string packageId, int userId) {

            Userkey userkey = _userkeyRepository.Get(u => u.UserId == userId);
            if (userkey == null) {
                return false;
            }
            return _userkeyPackageRepository.Fetch(up => up.UserkeyId == userkey.Id && up.PackageId == packageId).Any();
        }

        public bool KeyCanAccessPackage(string packageId, string accessKey, bool claimPackageIdIfAvailable) {
            if (string.IsNullOrWhiteSpace(packageId) || string.IsNullOrWhiteSpace(accessKey)) {
                return false;
            }
            if (claimPackageIdIfAvailable && !PackageIdIsRegistered(packageId)) {
                RegisterPackageId(packageId, accessKey);
                return true;
            }
            Userkey userkey = _userkeyRepository.Get(u => u.AccessKey == new Guid(accessKey));
            if (userkey == null) {
                return false;
            }
            return _userkeyPackageRepository.Fetch(up => up.PackageId == packageId && up.UserkeyId == userkey.Id).Any();
        }

        public IEnumerable<PackagePart> GetPackagesByUserkey(int userKeyId, bool includeAllVersions) {
            IEnumerable<UserkeyPackage> userkeyPackages = _userkeyPackageRepository.Fetch(ukp => ukp.UserkeyId == userKeyId);
            if (includeAllVersions) {
                return userkeyPackages.SelectMany(ukp =>
                    _orchardServices.ContentManager.Query<PackagePart, PackagePartRecord>(VersionOptions.AllVersions)
                    .Where(p => p.PackageID == ukp.PackageId).List());
            }
            return userkeyPackages.SelectMany(ukp =>
                _orchardServices.ContentManager.Query<PackagePart, PackagePartRecord>()
                .Where(p => p.PackageID == ukp.PackageId).List());
        }

        public IEnumerable<PackagePart> GetPackagesByUserkey(int userKeyId, int startingIndex, int pageSize, Expression<Func<PackagePartRecord,bool>> filter) {
            if (filter == null) {
                filter = p => true;
            }
            return _userkeyPackageRepository.Fetch(ukp => ukp.UserkeyId == userKeyId)
                .SelectMany(ukp =>
                            _orchardServices.ContentManager.Query<PackagePart, PackagePartRecord>()
                                .Where(p => p.PackageID == ukp.PackageId).Where(filter).List())
                .Skip(startingIndex).Take(pageSize);
        }

        public IEnumerable<UserkeyPackage> GetPackageIdsThatAreNotPackageParts() {
            IEnumerable<UserkeyPackage> userkeyPackages = _userkeyPackageRepository.Fetch(p => true);
            var userkeyPackageIds = userkeyPackages.Select(up => up.PackageId).Distinct();
            IEnumerable<string> packagePartPackageIds = _orchardServices.ContentManager.Query<PackagePart, PackagePartRecord>().List()
                .Select(pp => pp.PackageID);
            return userkeyPackages.Where(up => userkeyPackageIds.Except(packagePartPackageIds).Contains(up.PackageId));
        }

        public IEnumerable<IUser> GetAllOwnersForPackage(string packageId) {
            var userKeyIds = _userkeyPackageRepository.Fetch(up => up.PackageId == packageId).Select(ukp => ukp.UserkeyId).ToList();
            var userIds = _userkeyRepository.Fetch(u => userKeyIds.Contains(u.Id)).ToList().Select(u => u.UserId);

            return userIds.Select(
                userId => _orchardServices.ContentManager.Query<UserPart, UserPartRecord>()
                    .Where(u => u.Id == userId).List().Single());
        }

        public IEnumerable<IUser> GetContactableOwnersForPackage(string packageId) {
            return GetAllOwnersForPackage(packageId).Where(OptedIn);
        }

        private static bool OptedIn(IUser user) {
            return !user.ContentItem.Parts.Single(p => p.PartDefinition.Name == "ProfilePart")
                .Fields.Single(f => f.Name == "OptOut").Storage.Get<bool>("UserOptsOutOfEmails");
        }

        public void RemovePackageIdRegistration(string packageId, int userId) {
            Userkey userKeyToRemove = _userkeyService.GetAccessKeyForUser(userId, false);
            if (userKeyToRemove != null) {
                IEnumerable<UserkeyPackage> userkeyPackagesToRemove = _userkeyPackageRepository
                    .Fetch(up => up.UserkeyId == userKeyToRemove.Id && up.PackageId == packageId);
                foreach (UserkeyPackage userkeyPackage in userkeyPackagesToRemove) {
                    _userkeyPackageRepository.Delete(userkeyPackage);
                }
            }
        }

        public int CountOfUsersPackages(int userKeyId, Func<PackagePart, bool> filter)
        {
            //TODO: See if we can get this count with a single database query, instead of having to List() it
            if (filter == null) {
                filter = p => true;
            }

            return _userkeyPackageRepository.Fetch(ukp => ukp.UserkeyId == userKeyId)
                .SelectMany(ukp =>
                            _orchardServices.ContentManager.Query<PackagePart, PackagePartRecord>("Package")
                            .Where(p => p.PackageID == ukp.PackageId).List()
                            .Where(filter))
                            .Count();
        }

        public void DeletePackageIdRegistration(string packageId) {
            List<UserkeyPackage> userkeyPackagesToDelete = _userkeyPackageRepository.Fetch(up => up.PackageId == packageId).ToList();
            userkeyPackagesToDelete.ForEach(_userkeyPackageRepository.Delete);
        }

        public IEnumerable<UserkeyPackage> GetUserkeyPackagesForUser(int userId, bool includeImplicitOwnership) {
            if (includeImplicitOwnership && _packagePrivilegeChecker.UserCanManageAllPackages(userId)) {
                return _userkeyPackageRepository.Fetch(p => true);
            }
            var userKeyId = _userkeyService.GetAccessKeyForUser(userId, false);
            return _userkeyPackageRepository.Fetch(kp => kp.UserkeyId == userKeyId.Id);
        }
    }
}