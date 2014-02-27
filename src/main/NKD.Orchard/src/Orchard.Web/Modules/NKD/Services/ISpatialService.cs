using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Orchard;
using NKD.Models;
using System.ServiceModel;

namespace NKD.Services
{
    [ServiceContract]
    public interface ISpatialService : IDependency
    {
        [OperationContract]
        void TestSpatial();

    }
}
