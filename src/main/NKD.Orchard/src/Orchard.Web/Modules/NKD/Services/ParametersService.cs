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
using System.Transactions;
using NKD.ViewModels;
using NKD.Helpers;
using NKD.Module.BusinessObjects;

namespace NKD.Services {

    [UsedImplicitly]
    public class ParametersService : IParametersService
    {
        private readonly IOrchardServices _orchardServices;
        private readonly IPrivateDataService _privateService;
        private readonly IUsersService _users;
        public ParametersService(IOrchardServices orchardServices, IPrivateDataService privateService, IUsersService users)
        {
            _users = users;
            _orchardServices = orchardServices;
            _privateService = privateService;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }


        public IEnumerable<DictionaryUnit> GetUnits()
        {
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);
                return d.DictionaryUnits.ToArray();
            }
        }

        public BlockModelParameterViewModel GetParameter(Guid id)
        {
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);
                return (from g in
                            (from mod in d.BlockModelMetadatas.Where(mod => mod.IsColumnData == true && mod.BlockModelMetadataID==id)
                             from a in d.BlockModels.Where(a => a.BlockModelID == mod.BlockModelID)
                             from p in d.Parameters.Where(p => mod.ParameterID == p.ParameterID)
                             from u in d.DictionaryUnits.Where(u => u.UnitID == p.UnitID).DefaultIfEmpty()
                             select new { mod.BlockModelMetadataID, p.ParameterName, p.Description, p.ParameterID, u.StandardUnitName, u.UnitID, BlockModelName = a.Alias})
                           .GroupBy(x => x.BlockModelMetadataID, (x, y) => new { Key = x, Value = y.FirstOrDefault() })
                           //May be able to remove group by later as alias is now fixed to single model
                        select new BlockModelParameterViewModel { BlockModelMetadataID=g.Value.BlockModelMetadataID, ParameterName=g.Value.ParameterName, ParameterDescription=g.Value.Description, ParameterID = g.Value.ParameterID, BlockModelAlias = g.Value.BlockModelName, UnitID=g.Value.UnitID}).FirstOrDefault();
            }
        }

        public BlockModelApproveViewModel GetApproval(Guid id)
        {
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);
                var m = new BlockModelApproveViewModel();
                m.BlockModelID = id;
                m.BlockModelAlias = (from a in d.BlockModels.Where(a => a.BlockModelID == id && a.Version == 0).DefaultIfEmpty() select a.Alias).FirstOrDefault();
                var ps = (from md in d.BlockModelMetadatas.Where(md => md.BlockModelID == id && md.IsColumnData == false && md.ParameterID != _privateService.NKD_GUID_LOG)
                                join p in d.Parameters on md.ParameterID equals p.ParameterID
                                select new { md.BlockModelMetadataID, p.ParameterName, md.BlockModelMetadataText }).OrderBy(x=>x.ParameterName);
                m.Parameters = new Dictionary<Guid,Tuple<string,string>>();
                foreach (var x in ps)
                    m.Parameters.Add(x.BlockModelMetadataID, new Tuple<string,string>(x.ParameterName, x.BlockModelMetadataText));
                var o = (from a in d.BlockModelMetadatas.Where(a => a.BlockModelMetadataText != null && a.BlockModelID == id && a.ParameterID == _privateService.NKD_GUID_LOG && a.Version == 0) 
                         orderby a.VersionUpdated descending
                         select a.BlockModelMetadataText);
                m.Updates = new List<Occurrence>();
                foreach (var x in o)
                    m.Updates.Add(x.Deserialize<Occurrence>());
                m.Updates = m.Updates.OrderByDescending(x => x.Occurred).ToList();
                return m;
            }
        }
    }
}
