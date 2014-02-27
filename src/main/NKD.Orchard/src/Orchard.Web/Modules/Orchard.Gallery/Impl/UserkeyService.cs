using System;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Data;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;
using Orchard.Security;
using Orchard.Users.Models;
using Orchard.ContentManagement;

namespace Orchard.Gallery.Impl {
    [UsedImplicitly]
    public class UserkeyService : IUserkeyService {
        private readonly IRepository<Userkey> _userkeyContentItemRepository;
        private readonly IOrchardServices _orchardServices;
        private readonly IMembershipService _membershipService;

        public UserkeyService(IRepository<Userkey> userkeyContentItemRepository, IOrchardServices orchardServices, IMembershipService membershipService) {
            _userkeyContentItemRepository = userkeyContentItemRepository;
            _orchardServices = orchardServices;
            _membershipService = membershipService;
        }

        public Userkey GetAccessKeyForUser(int userId, bool generateIfNonexistent) {
            Userkey accessKeyForUser = _userkeyContentItemRepository.Get(uci => uci.UserId == userId);
            if (generateIfNonexistent && accessKeyForUser == null) {
                return SaveKeyForUser(userId, Guid.NewGuid());
            }
            return accessKeyForUser;
        }

        public Userkey SaveKeyForUser(int userId, Guid newKey) {
            Userkey existingUserkey = GetAccessKeyForUser(userId, false);
            if (existingUserkey != null) {
                existingUserkey.AccessKey = newKey;
                _userkeyContentItemRepository.Update(existingUserkey);
                return existingUserkey;
            }
            var newUserkey = new Userkey { UserId = userId, AccessKey = newKey};
            _userkeyContentItemRepository.Create(newUserkey);
            return newUserkey;
        }

        public Userkey GetUserkey(string accessKey) {
            return _userkeyContentItemRepository.Get(uci => uci.AccessKey == new Guid(accessKey));
        }

        public UserPart GetUserForUserKey(int userKeyId) {
            Userkey userKey = _userkeyContentItemRepository.Get(uci => uci.Id == userKeyId);
            if (userKey == null) {
                return null;
            }
            return _orchardServices.ContentManager.Query<UserPart, UserPartRecord>().Where(u => u.Id == userKey.UserId).List().SingleOrDefault();
        }

        public Userkey GetSuperUserKey() {
            int adminUserId = _membershipService.GetUser(_orchardServices.WorkContext.CurrentSite.SuperUser).Id;
            return GetAccessKeyForUser(adminUserId, false);
        }
    }
}