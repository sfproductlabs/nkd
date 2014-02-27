using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NKD.Models
{
    public interface IVerifyMobile
    {
        Guid VerificationID { get; set; }
        string Mobile { get; set; }
        string VerificationCode { get; set; }
        string TableType { get; set; }
        Guid ReferenceID { get; set; }
        Guid ContactID { get; set; }
        string ReferenceName { get; set; }
        DateTime? Sent { get; set; }
        DateTime? Verified { get; set; }
    }
}