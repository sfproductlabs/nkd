using System.Collections.Generic;

namespace Orchard.Gallery.ManagePackageIds {
    public class ManagePackageIdsViewModel {
        public IEnumerable<UserkeyPackageViewModel> RegisteredPackageIdsForUser { get; set; }
        public bool CanRegisterNewPackageIds { get; set; }
    }
}