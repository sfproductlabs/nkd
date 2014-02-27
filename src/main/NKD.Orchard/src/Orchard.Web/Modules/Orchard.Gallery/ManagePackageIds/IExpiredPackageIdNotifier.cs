using System;
using Orchard.Gallery.Models;

namespace Orchard.Gallery.ManagePackageIds {
    public interface IExpiredPackageIdNotifier : IDependency {
        void NotifyUserIfPackageIdIsAboutToExpire(UserkeyPackage userkeyPackage, DateTime utcNow, DateTime expirationDate, bool packageIdIsNotInUseOnFeed);
    }
}