using System;

namespace Orchard.Gallery.Models {
    public class Userkey {
        public virtual int Id { get; set; }
        public virtual int UserId { get; set; }
        public virtual Guid AccessKey { get; set; }
    }
}