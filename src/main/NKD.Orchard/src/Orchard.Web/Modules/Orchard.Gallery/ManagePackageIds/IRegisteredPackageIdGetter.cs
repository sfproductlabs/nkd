using System.Collections.Generic;

namespace Orchard.Gallery.ManagePackageIds {
    public interface IRegisteredPackageIdGetter : IDependency {
        IEnumerable<UserkeyPackageViewModel> GetRegisteredPackageIdsForUser(int userId);
        int GetNumberOfPreregisteredPackageIdsForUser(int userId);
    }
}