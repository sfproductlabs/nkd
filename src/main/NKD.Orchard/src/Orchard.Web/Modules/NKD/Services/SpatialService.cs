using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.FileSystems.Media;
using Orchard.Localization;
using Orchard;
using System.Transactions;
using Orchard.Messaging.Services;
using Orchard.Logging;
using NKD.Helpers;
using Orchard.Tasks.Scheduling;
using Orchard.Data;
using Orchard.Caching;
using Orchard.Services;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using NKD.Models;
using NKD.Module.BusinessObjects;

namespace NKD.Services {
    
    [UsedImplicitly]
    public class SpatialService : ISpatialService
    {
        private readonly IOrchardServices _orchardServices;
        //private readonly IContentManager _contentManager;
        //private readonly IMessageManager _messageManager;
        //private readonly IScheduledTaskManager _taskManager;
        //private readonly ISignals _signals;
        //private readonly IClock _clock;
        //private readonly ICacheManager _cache;
        private readonly IUsersService _users;
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public SpatialService(
            //IContentManager contentManager, 
            IOrchardServices orchardServices, 
            //IMessageManager messageManager, 
            //IScheduledTaskManager taskManager, 
            //ICacheManager cache, 
            //IClock clock, 
            //ISignals signals,
            IUsersService users)
        {
            _users = users;
            //_signals = signals;
            //_clock = clock;
            //_cache = cache;
            //_orchardServices = orchardServices;
            //_contentManager = contentManager;
            //_messageManager = messageManager;
            //_taskManager = taskManager;
            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;            
        }
              

        public void TestSpatial()
        {
           
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                using (var context = new NKDC(_users.ApplicationConnectionString,null))
                {
                    var s = (from o in context.Locations select o).FirstOrDefault();
                    
                    //context.Universities.Add(new University()
                    //{
                    //    Name = "Graphic Design Institute",
                    //    Location = DbGeography.FromText("POINT(-122.336106 47.605049)"),
                    //});
                    //context.SaveChanges();

                    //var myLocation = DbGeography.FromText("POINT(-122.296623 47.640405)");

                    //var university = (from u in context.Universities
                    //                  orderby u.Location.Distance(myLocation)
                    //                  select u).FirstOrDefault();

                }                
            }
        }      

       
        
       
    }
}
