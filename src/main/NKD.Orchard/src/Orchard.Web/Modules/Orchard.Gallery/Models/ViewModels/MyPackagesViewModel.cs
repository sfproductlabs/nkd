using System.Collections.Generic;

namespace Orchard.Gallery.Models.ViewModels {
    public class MyPackagesViewModel {
        public dynamic Packages { get; set; }
        public int TotalNumberOfPackages { get; set; }
        public int StartingNumber { get; set; }
        public int EndingNumber { get; set; }
        public dynamic Pager { get; set; }

        public IEnumerable<PackagePart> UnpublishedPackages { get; set; }
    }

    public class SimplePackage {
        public string PackageId { get; set; }
        public string PackageVersion { get; set; }
        public string Title { get; set; }
    }
}