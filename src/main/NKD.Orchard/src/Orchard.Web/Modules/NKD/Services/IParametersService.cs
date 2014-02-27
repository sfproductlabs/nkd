using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Orchard;
using NKD.Models;
using System.ServiceModel;
using NKD.ViewModels;
using NKD.Module.BusinessObjects;

namespace NKD.Services
{
    [ServiceContract]
    public interface IParametersService : IDependency {

        [OperationContract]
        IEnumerable<DictionaryUnit> GetUnits();

        [OperationContract]
        BlockModelParameterViewModel GetParameter(Guid id);

        [OperationContract]
        BlockModelApproveViewModel GetApproval(Guid id);
    }
}