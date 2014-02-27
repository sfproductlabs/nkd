using System;

namespace Orchard.Gallery.ManagePackageIds {
    public interface IPackageIdExpirationCoordinator : IDependency {
        void ProcessExpirations(int numberOfDaysUntilPackageIdExpires, DateTime utcNow);
    }
}