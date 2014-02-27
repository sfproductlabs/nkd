using System;
using Orchard.Gallery.Models;

namespace Orchard.Gallery.ManagePackageIds {
    public class UserkeyPackageViewModel {
        public UserkeyPackage UserkeyPackage { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public PackageIdStatus Status { get; set; }
        public string PackageType { get; set; }
        public bool IsPreregistered { get { return Status == PackageIdStatus.Registered || Status == PackageIdStatus.Unfinished; } }
    }
}