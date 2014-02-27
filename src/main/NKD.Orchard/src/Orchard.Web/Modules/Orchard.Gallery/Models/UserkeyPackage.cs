using System;

namespace Orchard.Gallery.Models {
    public class UserkeyPackage {
        public virtual int Id { get; set; }
        public virtual string PackageId { get; set; }
        public virtual int UserkeyId { get; set; }
        public virtual DateTime? RegisteredUtc { get; set; }
    }
}