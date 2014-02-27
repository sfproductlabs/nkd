using System;
using Orchard.Gallery.Models;
using Orchard.Users.Models;

namespace Orchard.Gallery.Interfaces {
    public interface IUserkeyService : IDependency {
        Userkey GetAccessKeyForUser(int userId, bool generateIfNonexistent = true);
        Userkey SaveKeyForUser(int userId, Guid newKey);
        Userkey GetUserkey(string accessKey);
        UserPart GetUserForUserKey(int userKeyId);
        Userkey GetSuperUserKey();
    }
}