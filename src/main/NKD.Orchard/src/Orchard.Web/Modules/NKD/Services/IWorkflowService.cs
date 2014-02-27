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
    public interface IWorkflowService : IDependency 
    {

         [OperationContract]
         Guid AssignResponsibility(Guid companyID, Guid contactID, Guid? tryWorkflowID = null, Guid referenceID = default(Guid), string referenceClass = null, string referenceTable = null);

         string CurrentState
         {
             [OperationContract]
             get;
         }

         [OperationContract]
         Guid GetResponsibleCompanyID(Guid? workflowID);

         [OperationContract]
         Guid GetResponsibleContactID(Guid? workflowID);

         [OperationContract]
         void CompleteProcess(Guid companyID, Guid contactID, Guid workflowID);



    }
}