using Orchard.ContentManagement;
using System.ComponentModel.DataAnnotations;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;

namespace NKD.Models {

    public class OwnerGuidPartRecord : ContentPartRecord
    {
        public virtual System.Guid OwnerGuid { get; set; }

    }

    public class OwnerGuidPart : ContentPart<OwnerGuidPartRecord> 
    {
        [Required]
        public System.Guid OwnerGuid
        {
            get { return Record.OwnerGuid; }
            set { Record.OwnerGuid = value; }
        }
    }
}