using System;
using Orchard.Gallery.Models;

namespace Orchard.Gallery.ManagePackageIds {
    public interface IExpiredPackageIdMessenger : IDependency {
        void SendMessage(UserkeyPackage userkeyPackage, DateTime expirationDate);
    }
}