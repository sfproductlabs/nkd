using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Orchard;
using NKD.Models;
using System.ServiceModel;
using Orchard.Media.Models;
using NKD.ViewModels;
using System.Threading.Tasks;
using Orchard.ContentManagement;

namespace NKD.Services
{
     [ServiceContract]
    public interface IGeophysicsService : IDependency 
    {        

         [OperationContract]
         Task<IReport> ReportGeophysicsAsync(GeophysicsReportViewModel m);

         [OperationContract]
         IReport ReportGeophysics(GeophysicsReportViewModel m);

    }
}