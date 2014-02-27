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
    public interface IProjectsService : IDependency {

        [OperationContract]
        IEnumerable<Project> GetProjects();

        [OperationContract]
        IEnumerable<ProjectPlanTask> GetStages(Guid projectID);

        [OperationContract]
        void DeleteProject(Guid projectID, Guid contactID);

        [OperationContract]
        void EmailAllProjectOwners(string subject, string body);

        [OperationContract]
        void UpdateProject(ProjectViewModel m);

        [OperationContract]
        void CreateProject(Project o);

    }
}