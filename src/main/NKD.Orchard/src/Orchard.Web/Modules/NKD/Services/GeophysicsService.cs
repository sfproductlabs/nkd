using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Ionic.Zip;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.FileSystems.Media;
using Orchard.Localization;
using NKD.Models;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Validation;
using Orchard;
using Orchard.Media.Models;
using Orchard.Media.Services;
using System.Transactions;
using Orchard.Logging;
using NKD.Import;
using NKD.ViewModels;
using System.Threading.Tasks;
using NKD.Reports;
using NKD.Import.FormatSpecification;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using NKD.Helpers;
using Orchard.Tasks.Scheduling;
using NKD.Module.BusinessObjects;

namespace NKD.Services {

    [UsedImplicitly]
    public class GeophysicsService : IGeophysicsService {
        private readonly IStorageProvider _storageProvider;
        private readonly IOrchardServices _orchardServices;
        private readonly IMediaService _mediaServices;
        private readonly IPrivateDataService _privateServices;
        private readonly IUsersService _users;
        private readonly IContentManager _contentManager;
        private readonly IConcurrentTaskService _concurrentTasks;

        public GeophysicsService(
            IStorageProvider storageProvider, 
            IOrchardServices orchardServices, 
            IMediaService mediaServices, 
            IPrivateDataService privateService, 
            IUsersService users, 
            IContentManager contentManager,
            IConcurrentTaskService concurrentTasks
          )
        {
            _contentManager = contentManager;
            _storageProvider = storageProvider;
            _orchardServices = orchardServices;
            _mediaServices = mediaServices;
            _privateServices = privateService;
            _users = users;
            _concurrentTasks = concurrentTasks;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        public async Task<IReport> ReportGeophysicsAsync(GeophysicsReportViewModel m)
        {
            m.ReportResult = ReportGeophysicsResult;
            return await Task<IReport>.Run(() => ReportGeophysics(m));
        }

        public IReport ReportGeophysics(GeophysicsReportViewModel m)
        {
            m.ReportResult = ReportGeophysicsResult;
            return AllReports.CreateModel(m);
        }

        //Static Methods
        public DataSet ReportGeophysicsResult(object model)
        {
            var m = (GeophysicsReportViewModel)model;

            DataSet ds = new DataSet("ReportResult");

            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);

                var cmd = d.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[X_SP_GetGeophysicsByParameters]";
                cmd.CommandType = CommandType.StoredProcedure;

                var parm1 = cmd.CreateParameter();
                parm1.ParameterName = "@projectid";
                parm1.DbType = DbType.Guid;
                parm1.Value = m.ProjectID;
                cmd.Parameters.Add(parm1);
               
                try
                {
                    //Let's actually run the queries
                    d.Connection.Open();
                    cmd.CommandTimeout = DBHelper.DefaultTimeout;
                    var reader = cmd.ExecuteReader();
                    ds.Load(reader, LoadOption.OverwriteChanges, "t");                    
                }
                finally
                {
                    d.Connection.Close();
                }
                return ds;
            }
        }


       
    }
}
