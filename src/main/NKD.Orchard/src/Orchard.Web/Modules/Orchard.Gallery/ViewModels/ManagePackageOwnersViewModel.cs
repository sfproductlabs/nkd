using System.Collections.Generic;
using Orchard.Security;

namespace Orchard.Gallery.ViewModels {
    public class ManagePackageOwnersViewModel {
        public string PackageId { get; set; }
        public IUser OwnerViewingPage { get; set; }
        public IEnumerable<IUser> OtherOwners { get; set; }
    }
}