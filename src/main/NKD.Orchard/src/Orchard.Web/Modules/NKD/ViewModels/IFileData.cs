using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DevExpress.XtraReports.UI;

namespace NKD.ViewModels
{
    public interface IFileData
    {
        Guid FileDataID { get; set; }
        string FileName { get; set; }
    }
}