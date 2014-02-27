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
    public class BlockModelService : IBlockModelService {
        private readonly IStorageProvider _storageProvider;
        private readonly IOrchardServices _orchardServices;
        private readonly IMediaService _mediaServices;
        private readonly IPrivateDataService _privateServices;
        private readonly IUsersService _users;
        private readonly IContentManager _contentManager;
        private readonly IConcurrentTaskService _concurrentTasks;

        public BlockModelService(
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

        public IEnumerable<MediaFile> GetNewBlockModelFiles()
        {
            var relativePath = @"NKD\BlockModels";
            _storageProvider.TryCreateFolder(relativePath);
            return _mediaServices.GetMediaFiles(relativePath); 
        }
        public IEnumerable<MediaFile> GetUpdateFileNameList()
        {
            var relativePath = @"NKD\ModelUpdates";
            _storageProvider.TryCreateFolder(relativePath);
            return _mediaServices.GetMediaFiles(relativePath); 
        }
        
        public IEnumerable<MediaFile> GetNewFormatFiles()
        {
            var relativePath = @"NKD\Definitions";
            _storageProvider.TryCreateFolder(relativePath);
            return _mediaServices.GetMediaFiles(relativePath); 
        }


        public List<string> GetImportFileColumnsAsList(Guid guid, string bmFileName, string modelAlias)
        {

            List<string> columnNames = new List<string>();
            
            BaseImportTools bit = new BaseImportTools();

            string targetFolder;
            bool attmemptModelLoad;
            string originalName = bmFileName;
            bmFileName = ExtractBlockModelFromZip(bmFileName, out targetFolder, out attmemptModelLoad);
            IStorageFile bmFile = _storageProvider.GetFile(bmFileName);
            Stream bmFileStream = bmFile.OpenRead();
            NKD.Import.FormatSpecification.ImportDataMap idm = null;
            double _originX = -1;
            double _originY = -1;
            double _originZ = -1;

            try
            {
                StreamReader sr = new StreamReader(bmFileStream);
                string headerLine = "";
                string firstDataLine = "";
                if (sr != null)
                {
                    headerLine = sr.ReadLine();
                    firstDataLine = sr.ReadLine();

                    bit.ParseDataLinesForOrigins(headerLine, firstDataLine, out _originX, out _originY, out _originZ);
                    // auto generate a format defintion based on Goldfields typical input column data
                    idm = bit.AutoGenerateFormatDefinition(headerLine);
                }
                sr.Close();

            }
            catch
            {

            }

            if (idm != null)
            {


                foreach (ColumnMap cm in idm.columnMap)
                {
                    columnNames.Add(cm.sourceColumnName);
                }
            }
            return columnNames;
        }


        public void AppendModelAsync(Guid bmGuid, string bmFileName, string alias, string columnNameToImport, int columnIndexToImport, Guid userGuid, int allowImportAfterMinutes = 0)
        {
            var emails = _users.GetUserEmails(new Guid[] { userGuid }).ToArray();
            var repeats = _contentManager.Query<BlockModelPart, BlockModelPartRecord>("BlockModel").Where(f => f.BmGuid == bmGuid && f.BmFileName == bmFileName && f.ColumnIndexToAdd == columnIndexToImport && f.ColumnNameToAdd == columnNameToImport && f.Processed > DateTime.UtcNow.AddMinutes(-allowImportAfterMinutes)).Count();
            if (repeats == 0)
            {
                var m = _contentManager.New<BlockModelPart>("BlockModel");
                m.BmGuid = bmGuid;
                m.Recipients = emails.FlattenStringArray();
                m.BmFileName = bmFileName;
                m.Alias = alias;
                m.UserID = userGuid;
                m.ColumnNameToAdd = columnNameToImport;
                m.ColumnIndexToAdd = columnIndexToImport;
                m.Processed = DateTime.UtcNow;
                _contentManager.Create(m, VersionOptions.Published);
                _concurrentTasks.ExecuteAsyncTask(AppendModel, m.ContentItem);
                //_taskManager.AppendModelAsync(m.ContentItem);
            }
            else
            {
                Logger.Information(string.Format("Can't re-append file:{0} with the same settings within the currently selected time-frame",bmFileName));
            }
        }

        public void AppendModel(ContentItem c)
        {
            var m = c.As<BlockModelPart>();

            string statusMessage = "";            
            bool completed = false;
            BaseImportTools bit = new BaseImportTools();
            string targetFolder;
            bool attmemptModelLoad;
            var bmFileName = ExtractBlockModelFromZip(m.BmFileName, out targetFolder, out attmemptModelLoad);
            IStorageFile bmFile = _storageProvider.GetFile(bmFileName);
            Stream bmFileStream = bmFile.OpenRead();
            ModelImportStatus mos = new ModelImportStatus();
            try
            {

                // Special append method for GF requirements - contains X, Y, Z, and value to append.
                // target model must have matching X, Y and Z centroids.
                // auto generate a format defintion based on Goldfields typical input column data 
                var opts = new TransactionOptions();
                opts.IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted;
                using (new TransactionScope(TransactionScopeOption.Suppress, opts))
                {
                    mos = bit.PerformBMAppend(bmFileStream, m.BmGuid, m.Alias, m.ColumnNameToAdd, m.ColumnIndexToAdd, _users.ApplicationConnectionString);
                    mos.importTextFileName = m.BmFileName;
                    mos.targetModelName = m.Alias;
                }
                statusMessage += string.Format("Successfully appended data to block model:\n{0} - {1}\n\nFrom file:{2}\n\n", m.Alias, m.BmGuid, m.BmFileName);
                completed = true;


            }
            catch (Exception ex)
            {
                statusMessage += string.Format("Error appending data to block model:\n{0} - {1}\n\nFrom file:{2}\n\nError:\n{3}\n\n", m.Alias, m.BmGuid, m.BmFileName, ex.ToString());
            }
            finally {
                statusMessage += mos.GenerateStringMessage();
                bmFileStream.Close();
                _storageProvider.DeleteFile(bmFileName);
                _storageProvider.DeleteFolder(targetFolder);
            }
            statusMessage += string.Format("Time Elapsed: {0}\n\n", m.Processed.HasValue ? (DateTime.UtcNow - m.Processed.Value).ToString(@"hh\h\:mm\m\:ss\s\:fff\m\s") : "Unknown");
            
            Logger.Information(statusMessage);

            _users.EmailUsers(m.Recipients.SplitStringArray(), completed ? "Model Append Succeeded" : "Model Append Failed", statusMessage);

        }


        /// <summary>
        /// Could maybe use ProcessModelScheduledTaskHandler if break any dependencies
        /// </summary>
        /// <param name="bmFileName"></param>
        /// <param name="formatFileName"></param>
        /// <param name="projectID"></param>
        /// <param name="alias"></param>
        /// <param name="userGuid"></param>
        /// <param name="notes"></param>
        /// <param name="stage"></param>
        /// <param name="stageMetaID"></param>
        /// <returns></returns>
        public void ProcessModelAsync(string bmFileName, string formatFileName, string projectName, string alias, Guid userGuid, string notes, string stage, Guid stageMetaID, int allowImportAfterMinutes = 0)
        {
            var repeats = _contentManager.Query<BlockModelPart, BlockModelPartRecord>("BlockModel").Where(f => f.BmFileName == bmFileName && f.FormatFileName == formatFileName && f.ProjectName==projectName && f.Processed > DateTime.UtcNow.AddMinutes(-allowImportAfterMinutes)).Count();
            if (repeats == 0)
            {
                var emails = _users.GetUserEmails(new Guid[] { userGuid }).ToArray();
                var m = _contentManager.New<BlockModelPart>("BlockModel");
                m.Recipients = emails.FlattenStringArray();
                m.BmFileName = bmFileName;
                m.FormatFileName = formatFileName;
                m.ProjectName = projectName;
                m.Alias = alias;
                m.UserID = userGuid;
                m.Notes = notes;
                m.Stage = stage;
                m.StageMetaID = stageMetaID;
                m.Processed = DateTime.UtcNow;
                _contentManager.Create(m, VersionOptions.Published);
                //_taskManager.ProcessModelAsync(m.ContentItem);
                _concurrentTasks.ExecuteAsyncTask(ProcessModel, m.ContentItem);                
            }
            else
            {
                Logger.Information(string.Format("Can't re-import file:{0} with the same settings within the currently selected time-frame", bmFileName));
            }
        }

        public void ProcessModel(ContentItem c)
        {
            var m = c.As<BlockModelPart>();
            List<string> domains = null;

            string targetFolder;
            bool attmemptModelLoad;

            var bmFileName = ExtractBlockModelFromZip(m.BmFileName, out targetFolder, out attmemptModelLoad);

            ModelImportStatus mos = DoNewModelImport(bmFileName, m.FormatFileName, m.ProjectName, m.Alias, m.UserID, ref domains, targetFolder, attmemptModelLoad, m.Notes, m.Stage, m.StageMetaID);
            mos.importTextFileName = bmFileName +" (from "+ m.BmFileName +")";
            mos.targetModelName = m.Alias;

            string msg = "";
            string subject = "";
            if (mos.finalErrorCode > 0) {
                subject = "Model Import Failed";
                msg += string.Format("Error importing block model:\n{0}\n\n", m.Alias, m.BmFileName);
                 
            }
            else {
                subject = "Model Import Succeeded";
                msg += string.Format("Successfully imported block model:\n{0}\n\n", m.Alias, m.BmFileName);
            }
            msg += mos.GenerateStringMessage();

            msg += string.Format("Time Elapsed: {0}\n\n", m.Processed.HasValue ? (DateTime.UtcNow - m.Processed.Value).ToString(@"hh\h\:mm\m\:ss\s\:fff\m\s") : "Unknown");
            
            Logger.Information(msg);

            _users.EmailUsers(m.Recipients.SplitStringArray(), subject, msg);

        }

        /// <summary>
        /// Import the block model.  This is ultimatly carried out throught he import library which must be 
        /// passed the streams for the import file (and defintiion possibly).
        /// </summary>
        /// <param name="bmFileName"></param>
        /// <param name="formatFileName"></param>
        /// <param name="projectID"></param>
        /// <param name="alias"></param>
        /// <param name="authorGuid"></param>
        /// <param name="res2"></param>
        /// <param name="domains"></param>
        /// <param name="targetFolder"></param>
        /// <param name="attmemptModelLoad"></param>
        /// <param name="notes"></param>
        /// <param name="stage"></param>
        /// <returns></returns>
        private ModelImportStatus  DoNewModelImport(string bmFileName, string formatFileName, string projectID, string alias, Guid authorGuid, ref List<string> domains, string targetFolder, bool attmemptModelLoad, string notes, string stage, Guid stageMetaID)
        {
            ModelImportStatus mos = new ModelImportStatus();
            mos.finalErrorCode = ModelImportStatus.OK;
            double _originX = -1;
            double _originY = -1;
            double _originZ = -1;
            BaseImportTools bit = new BaseImportTools();
            if (attmemptModelLoad)
            {
                //IStorageFile formatFile = _storageProvider.GetFile(formatFileName);
                IStorageFile bmFile = _storageProvider.GetFile(bmFileName);
                Stream bmFileStream = bmFile.OpenRead();
                Stream bmTempFileStream = bmFile.OpenRead();
                // try and automatically detect the origin coordinates from the file itself - a datamine file 
                // always carries the origin coords in XMORIG, YMORIG, ZMORIG
                
                NKD.Import.FormatSpecification.ImportDataMap idm = null;
                try
                {                   
                    StreamReader sr = new StreamReader(bmTempFileStream);
                    string headerLine = "";
                    string firstDataLine = "";
                    if (sr != null)
                    {
                        headerLine = sr.ReadLine();
                        firstDataLine = sr.ReadLine();
                        bit.ParseDataLinesForOrigins(headerLine, firstDataLine, out _originX, out _originY, out _originZ);
                        // auto generate a format defintion based on Goldfields typical input column data
                        idm = bit.AutoGenerateFormatDefinition(headerLine);
                    }
                    sr.Close();

                }
                catch (Exception ex) {
                    mos.AddWarningMessage("Unable to auto detect origin and other format information from the file\n\n" + ex.ToString());
                }
                var opts = new TransactionOptions();
                opts.IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted;
                using (new TransactionScope(TransactionScopeOption.Suppress, opts))
                {
                    try
                    {
                        Guid blockModelGUID = Guid.NewGuid();
                        mos.modelID = blockModelGUID;
                        domains = bit.PerformBMImport(mos, blockModelGUID, bmFileStream, null, idm, _originX, _originY, _originZ, null, 1000, projectID, alias, authorGuid, _users.ApplicationConnectionString);
                        List<Tuple<string, string>> doms = new List<Tuple<string, string>>();
                        string domainColumnName = "Domain";
                        foreach (string ss in domains)
                        {
                            doms.Add(new Tuple<string, string>(domainColumnName, ss));
                        }
                        UpdateDomains(doms, blockModelGUID);
                        AddModelNotes(notes, blockModelGUID);
                        UpdateStage(blockModelGUID, stageMetaID, stage);
                    }
                    catch (Exception ex)
                    {
                        mos.finalErrorCode = ModelImportStatus.ERROR_WRITING_TO_DB;
                        mos.AddErrorMessage("Error importing block model:\n" + ex.ToString());
                    }
                }
                
                //TODO call into import library with the stream object for the import

                bmFileStream.Close();
                _storageProvider.DeleteFile(bmFileName);
                _storageProvider.DeleteFolder(targetFolder);
            }
            return mos;
        }

        /// <summary>
        /// Extract the block model file from the nominated Zip file.  A temp folder is created to store the file
        /// and the path to that file is generated for use when importing.
        /// </summary>
        /// <param name="bmFileName"></param>
        /// <param name="targetFolder"></param>
        /// <param name="attmemptModelLoad"></param>
        /// <returns></returns>
        private string ExtractBlockModelFromZip(string bmFileName, out string targetFolder, out bool attmemptModelLoad)
        {
            IStorageFile bmZipFile = _storageProvider.GetFile(bmFileName);
            targetFolder = bmFileName + "extracted";
            List<string> filesInZip = new List<string>();
            attmemptModelLoad = false;
            if (bmFileName.EndsWith(".zip"))
            {
                Stream bmZipFileStream = bmZipFile.OpenRead();
                // extract file to a temp location
                using (var fileInflater = ZipFile.Read(bmZipFileStream))
                {
                    foreach (ZipEntry entry in fileInflater)
                    {
                        if (entry == null) { continue; }

                        if (!entry.IsDirectory && !string.IsNullOrEmpty(entry.FileName))
                        {

                            // skip disallowed files
                            //if (FileAllowed(entry.FileName, false))
                            // {
                            string fullFileName = _storageProvider.Combine(targetFolder, entry.FileName);
                            filesInZip.Add(fullFileName);
                            using (var stream = entry.OpenReader())
                            {
                                // the call will return false if the file already exists
                                if (!_storageProvider.TrySaveStream(fullFileName, stream))
                                {

                                    // try to delete the file and save again
                                    try
                                    {
                                        _storageProvider.DeleteFile(fullFileName);
                                        _storageProvider.TrySaveStream(fullFileName, stream);
                                    }
                                    catch (ArgumentException)
                                    {
                                        // ignore the exception as the file doesn't exist
                                    }
                                }
                            }
                            //}
                        }
                    }
                }
                bmZipFileStream.Close();
                if (filesInZip.Count() > 0)
                {
                    bmFileName = filesInZip.First();
                    attmemptModelLoad = true;
                }
            }
            return bmFileName;
        }

        

       
        public IEnumerable<BlockModel> GetModels()
        {
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);
                return d.BlockModels.ToArray();
            }
        }

        public IEnumerable<BlockModel> GetModelsCurrent()
        {
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);
                var b = from m in d.BlockModels join p in d.ModelStatusViews on m.BlockModelID equals p.BlockModelID select m;
                return b.ToArray();
            }
        }


        public string GetBlockModelInfo(Guid bm) {
            string res = "";
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                try
                {

                    BaseImportTools bit = new BaseImportTools();
                    var d = new NKDC(_users.ApplicationConnectionString, null);
                    //d.Connection.ConnectionString = global::System.Configuration.ConfigurationManager.ConnectionStrings["NKDConnectionString"].ConnectionString;
                    List<BlockModel> bl = d.BlockModels.ToList();
                    foreach (BlockModel aModel in bl)
                    {
                        res += aModel.Alias;
                    }
                }
                catch (Exception ex) {
                    res += "Error " + ex.ToString();
                }
            }
            return res;
        }

        public void UpdateModelParameter(BlockModelParameterViewModel m)
        {
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);
                var x = from p in d.Parameters where p.ParameterID == m.ParameterID select p;
                var o = x.First();
                o.UnitID = m.UnitID;
                d.SaveChanges();
            }
        }

        public string GetModelAlias(Guid modelID)
        {
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);
                return (from o in d.BlockModels where o.BlockModelID == modelID select o.Alias).FirstOrDefault();
            }
        }

        public BlockModelAppendViewModel GetBlockModelToAppend(Guid modelID)
        {
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);
                var b = (d.BlockModels.OrderByDescending(x => x.Version).FirstOrDefault(x => x.BlockModelID == modelID));
                var m = new BlockModelAppendViewModel
                {
                    BlockModelAlias = b.Alias,
                    Version = (b.Version + 1),
                    BlockModelID = modelID,
                    FileNames = this.GetUpdatedModelList()
                };
                return m;
            }
        }

        public IEnumerable<Tuple<Parameter, BlockModelMetadata>> GetModelParameters(Guid modelID)
        {
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);
                var o = from m in d.BlockModelMetadatas where m.BlockModelID == modelID
                        join p in d.Parameters on m.ParameterID equals p.ParameterID
                        select new { m, p }
                        ; 
                return o.OrderBy(f=>f.p.Description).ToArray().Select(f=>new Tuple<Parameter,BlockModelMetadata>(f.p, f.m)); 
            }
        }

        public IEnumerable<Tuple<string, string>> GetModelDomains(Guid modelID)
        {
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                //Slow Method
                //var d = new NKDC(_users.ApplicationConnectionString, null);
                //return d.GetUniqueDomains(modelID).Select(x => new Tuple<string,string>("Domain", x.Domain)).OrderBy(x => x).ToList();
                
                //Fast Method
                return _privateServices.GetFirstMetadata<List<Tuple<string, string>>>(modelID, f => f.MetaDataType == "Domains");
            }
        }

        public void CheckModels()
        {
            //TODO:
            // Removes old files, removes old models, checks for new files (future - automatically add model?)
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);
                d.X_SP_DropOldModels();
            }
        }

        public void DeleteModel(Guid modelID, Guid deletedByGuid)
        {
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);
                var b = (d.BlockModels.OrderBy(x => x.Version).FirstOrDefault(x => x.BlockModelID == modelID));
                b.VersionUpdated = DateTime.UtcNow;
                b.VersionDeletedBy = deletedByGuid;
                d.SaveChanges();
            }

        }

        public async Task<IReport> CompareModelsAsync(BlockModelCompareViewModel models)
        {
            models.ReportResult = CompareModelsResult;
            return await Task<IReport>.Run(() => CompareModels(models));
        }

        public IReport CompareModels(BlockModelCompareViewModel models)
        {
            models.ReportResult = CompareModelsResult;
            return AllReports.CreateModel(models);
        }

        public decimal GetMultiplierForField(Guid convertFromFieldGuid, string targetStandardUnitName)
        {
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);

                //////Get Curves
                var cmd = d.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[X_SP_GetConversionFactorForField]";
                cmd.CommandType = CommandType.StoredProcedure;

                var parm1 = cmd.CreateParameter();
                parm1.ParameterName = "@field_guid";
                parm1.DbType = DbType.Guid;
                parm1.Value = convertFromFieldGuid;
                cmd.Parameters.Add(parm1);

                var parm2 = cmd.CreateParameter();
                parm2.ParameterName = "@target";
                parm2.DbType = DbType.String;
                parm2.Value = targetStandardUnitName;
                cmd.Parameters.Add(parm2);

                var output = new System.Data.SqlClient.SqlParameter();
                output.ParameterName = "@multiplier";
                output.DbType = DbType.Decimal;
                output.Direction = ParameterDirection.Output;
                output.Precision = 38;
                output.Scale = 20;
                cmd.Parameters.Add(output);

                try
                {
                    //Let's actually run the queries
                    d.Connection.Open();
                    var reader = cmd.ExecuteReader();
                    return (decimal)output.Value;
                }
                catch
                {
                    return 0;
                }
                finally
                {
                    d.Connection.Close();
                }
            }
        }


        public DataSet CompareModelsResult(object model)
        {
            var m = (BlockModelCompareViewModel)model;

            DataSet ds = new DataSet("CompareModelsResult");

            var y = m.FilterString.Deserialize<dynamic>();
            var filter = new List<Tuple<string, string, string, string, string>>();
            foreach (object[] k in y)
            {
                filter.Add(new Tuple<string, string, string, string, string>(
                    k[0] as string, //1 Model# (1 or 2)
                    k[1] as string, //2 Boolean Comparator (AND, OR)
                    k[2] as string, //3 Guid (FieldID)
                    k[3] as string, //4 Math Comparator (>=,<=,=)
                    k[4] as string  //5 Value (0.05)
                    ));
            }
            string filter1 = string.Join(";", (from o in filter
                             where o.Item1 == "1"
                             select
                                 o.Item2.CleanTokenForSQL() + "," +
                                 o.Item3.CleanTokenForSQL() + "," +
                                 o.Item4.CleanTokenForSQL() + "," +
                                 o.Item5.CleanTokenForSQL()).ToArray());
            filter1 = string.IsNullOrEmpty(filter1) ? filter1 : filter1 + ";";
            string filter2 = string.Join(";", (from o in filter
                            where o.Item1 == "2"
                            select
                                o.Item2.CleanTokenForSQL() + "," +
                                o.Item3.CleanTokenForSQL() + "," +
                                o.Item4.CleanTokenForSQL() + "," +
                                o.Item5.CleanTokenForSQL()).ToArray());
            filter2 = string.IsNullOrEmpty(filter2) ? filter2 : filter2 + ";";

            string domains1 = m.SelectedDomainsModel1Compact != null ? m.SelectedDomainsModel1Compact.CleanTokenForSQL() + ";" : "";
            string domains2 = m.SelectedDomainsModel2Compact !=null ? m.SelectedDomainsModel2Compact.CleanTokenForSQL() + ";": "";
            
            //Test DataSet
            var p = new DataTable("Test1Text");
            var x = new string[] { "asdasd", "asdasd", "asss", "asddasd", "reerrr" };
            var s = from i in x select new { Test = i };
            p.Columns.Add("Test");
            s.CopyToDataTable(p);
            ds.Tables.Add(p);

            //Test Dataset
            double[][] z = new double[][] { new[] { 0.5, 0.7 }, new[] { 40.5, 60.7 } };
            var t = from i in z select new { Argument = i[0], Value = i[1] };
            var g = new DataTable("Test2Tuple");
            g.Columns.Add("Argument", typeof(double));
            g.Columns.Add("Value", typeof(double));
            t.CopyToDataTable(g);
            ds.Tables.Add(g);

            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);

                //////Get Curves
                var cmd = d.Connection.CreateCommand();
                cmd.CommandText = "[dbo].[X_SP_GradeTonnage]";
                cmd.CommandType = CommandType.StoredProcedure;

                var parm1 = cmd.CreateParameter();
                parm1.ParameterName = "@gt_guid";
                parm1.DbType = DbType.Guid;
                parm1.Value = m.GradeTonnageFieldID;
                cmd.Parameters.Add(parm1);

                var parm2 = cmd.CreateParameter();
                parm2.ParameterName = "@bm1_guid";
                parm2.DbType = DbType.Guid;
                parm2.Value = m.Model1;
                cmd.Parameters.Add(parm2);

                var parm3 = cmd.CreateParameter();
                parm3.ParameterName = "@bm2_guid";
                parm3.DbType = DbType.Guid;
                parm3.Value = m.Model2;
                cmd.Parameters.Add(parm3);

                var parm4 = cmd.CreateParameter();
                parm4.ParameterName = "@gt_increment";
                parm4.DbType = DbType.Decimal;
                parm4.Value = m.GradeTonnageIncrement;
                cmd.Parameters.Add(parm4);

                var parm5 = cmd.CreateParameter();
                parm5.ParameterName = "@filter1";
                parm5.DbType = DbType.String;
                parm5.Value = filter1;
                cmd.Parameters.Add(parm5);

                var parm6 = cmd.CreateParameter();
                parm6.ParameterName = "@filter2";
                parm6.DbType = DbType.String;
                parm6.Value = filter2;
                cmd.Parameters.Add(parm6);

                var parm7 = cmd.CreateParameter();
                parm7.ParameterName = "@domains1";
                parm7.DbType = DbType.String;
                parm7.Value = domains1;
                cmd.Parameters.Add(parm7);

                var parm8 = cmd.CreateParameter();
                parm8.ParameterName = "@domains2";
                parm8.DbType = DbType.String;
                parm8.Value = domains2;
                cmd.Parameters.Add(parm8);

                var output = cmd.CreateParameter();
                output.ParameterName = "@filterString";
                output.DbType = DbType.String;
                output.Size = 4000;
                output.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(output);

                /////Get Table

                var cmdc = d.Connection.CreateCommand();
                cmdc.CommandText = "[dbo].[X_SP_GradeTonnageComparison]";
                cmdc.CommandType = CommandType.StoredProcedure;

                var parmc1 = cmdc.CreateParameter();
                parmc1.ParameterName = "@gt_guid";
                parmc1.DbType = DbType.Guid;
                parmc1.Value = m.GradeTonnageFieldID;
                cmdc.Parameters.Add(parmc1);

                var parmc2 = cmdc.CreateParameter();
                parmc2.ParameterName = "@bm1_guid";
                parmc2.DbType = DbType.Guid;
                parmc2.Value = m.Model1;
                cmdc.Parameters.Add(parmc2);

                var parmc3 = cmdc.CreateParameter();
                parmc3.ParameterName = "@bm2_guid";
                parmc3.DbType = DbType.Guid;
                parmc3.Value = m.Model2;
                cmdc.Parameters.Add(parmc3);

                var parmc5 = cmdc.CreateParameter();
                parmc5.ParameterName = "@filter1";
                parmc5.DbType = DbType.String;
                parmc5.Value = filter1;
                cmdc.Parameters.Add(parmc5);

                var parmc6 = cmdc.CreateParameter();
                parmc6.ParameterName = "@filter2";
                parmc6.DbType = DbType.String;
                parmc6.Value = filter2;
                cmdc.Parameters.Add(parmc6);

                var parmc7 = cmdc.CreateParameter();
                parmc7.ParameterName = "@domains1";
                parmc7.DbType = DbType.String;
                parmc7.Value = domains1;
                cmdc.Parameters.Add(parmc7);

                var parmc8 = cmdc.CreateParameter();
                parmc8.ParameterName = "@domains2";
                parmc8.DbType = DbType.String;
                parmc8.Value = domains2;
                cmdc.Parameters.Add(parmc8);

                var outputc = cmdc.CreateParameter();
                outputc.ParameterName = "@filterString";
                outputc.DbType = DbType.String;
                outputc.Size = 4000;
                outputc.Direction = ParameterDirection.Output;
                cmdc.Parameters.Add(outputc);

                //Slicer
                var cmds = d.Connection.CreateCommand();
                cmds.CommandText = "[dbo].[X_SP_SlicingTool]";
                cmds.CommandType = CommandType.StoredProcedure;

                var parms1 = cmds.CreateParameter();
                parms1.ParameterName = "@st_guid";
                parms1.DbType = DbType.Guid;
                parms1.Value = m.GradeTonnageFieldID; //TODO: might need to separate this out in the UI one day (we are assuming XYZ values are xmax, ymax, zmax respectively now)
                cmds.Parameters.Add(parms1);

                var parms2 = cmds.CreateParameter();
                parms2.ParameterName = "@bm1_guid";
                parms2.DbType = DbType.Guid;
                parms2.Value = m.Model1;
                cmds.Parameters.Add(parms2);

                var parms3 = cmds.CreateParameter();
                parms3.ParameterName = "@bm2_guid";
                parms3.DbType = DbType.Guid;
                parms3.Value = m.Model2;
                cmds.Parameters.Add(parms3);

                var parms5 = cmds.CreateParameter();
                parms5.ParameterName = "@filter1";
                parms5.DbType = DbType.String;
                parms5.Value = filter1;
                cmds.Parameters.Add(parms5);

                var parms6 = cmds.CreateParameter();
                parms6.ParameterName = "@filter2";
                parms6.DbType = DbType.String;
                parms6.Value = filter2;
                cmds.Parameters.Add(parms6);

                var parms7 = cmds.CreateParameter();
                parms7.ParameterName = "@domains1";
                parms7.DbType = DbType.String;
                parms7.Value = domains1;
                cmds.Parameters.Add(parms7);

                var parms8 = cmds.CreateParameter();
                parms8.ParameterName = "@domains2";
                parms8.DbType = DbType.String;
                parms8.Value = domains2;
                cmds.Parameters.Add(parms8);

                var outputs = cmds.CreateParameter();
                outputs.ParameterName = "@filterString";
                outputs.DbType = DbType.String;
                outputs.Size = 4000;
                outputs.Direction = ParameterDirection.Output;
                cmds.Parameters.Add(outputs);

                try
                {
                    var gt = new DataTable("gt");
                    var gtm = gt.Columns.Add("m", typeof(string));
                    var gtg = gt.Columns.Add("grade", typeof(decimal));
                    var gtt = gt.Columns.Add("tonnes", typeof(decimal));
                    var gttg = gt.Columns.Add("tonnage", typeof(decimal));

                    var gfs = new DataTable("gfs");
                    gfs.Columns.Add("FilterString", typeof(string));

                    var gfc = new DataTable("gfc");
                    gfc.Columns.Add("Resource Category", typeof(string));
                    gfc.Columns.Add("Model 1 Tonnes", typeof(decimal));
                    gfc.Columns.Add("Model 1 Grade", typeof(decimal));
                    gfc.Columns.Add("Model 2 Tonnes", typeof(decimal));
                    gfc.Columns.Add("Model 2 Grade", typeof(decimal));
                    gfc.Columns.Add("Absolute Difference", typeof(decimal));

                    var st = new DataTable("st");
                    st.Columns.Add("xyz", typeof(decimal));
                    st.Columns.Add("m", typeof(decimal));
                    st.Columns.Add("slice", typeof(decimal));
                    st.Columns.Add("samples", typeof(decimal));
                    st.Columns.Add("grade", typeof(decimal));

                    //Let's actually run the queries
                    d.Connection.Open();
                                     

                    if (m.GradeTonnageIncrement != 0)
                    {
                        cmd.CommandTimeout = DBHelper.DefaultTimeout;
                        var reader = cmd.ExecuteReader();
                        gt.Load(reader, LoadOption.OverwriteChanges);
                        decimal cumulative1 = 0, cumulative2 = 0;
                        decimal gtFieldMultiplier = GetMultiplierForField(m.GradeTonnageFieldID, "%");
                        foreach (DataRow r in gt.Rows)
                        {
                            decimal tonnes;
                            decimal grade;
                            if (!decimal.TryParse(string.Format("{0}", r[gtg]), out grade) && grade >= 0)
                                continue;
                            if (!decimal.TryParse(string.Format("{0}", r[gtt]), out tonnes) && tonnes >= 0)
                                continue;
                            //Cumulative needs to be in grade ascending order
                            if ((string)r[gtm] == "1")
                            {
                                //cumulative1 += ((decimal)r[gtg] * (decimal)r[gtt] * gtFieldMultiplier);
                                cumulative1 += (tonnes);
                                r[gttg] = cumulative1;
                            }
                            else
                            {
                                cumulative2 += (tonnes);
                                r[gttg] = cumulative2;
                            }
                            r[gtg] =  grade * gtFieldMultiplier;
                        }

                        //reader.NextResult(); // Only 1 Resultset
                        //var gt2 = new DataTable("gt2");
                        //gt2.Load(reader, LoadOption.OverwriteChanges);
                        //ds.Tables.Add(gt2);
                    }

                    {
                        cmds.CommandTimeout = DBHelper.DefaultTimeout;
                        var reader = cmds.ExecuteReader();
                        st.Load(reader, LoadOption.OverwriteChanges);
                    }

                    {
                        cmdc.CommandTimeout = DBHelper.DefaultTimeout;
                        var reader = cmdc.ExecuteReader();
                        gfc.Load(reader, LoadOption.OverwriteChanges);
                    }

                    gfs.Rows.Add(outputc.Value as string); //filterString                    

                    ds.Tables.Add(st);
                    ds.Tables.Add(gt);
                    ds.Tables.Add(gfs);
                    ds.Tables.Add(gfc);
                }
                finally
                {
                    d.Connection.Close();
                }
                return ds;
            }
        }


        public void ApproveModel(Guid modelID, Guid approverID, string note)
        {
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);
                var o = d.BlockModels.Where(f => f.BlockModelID == modelID).Single();
                o.ApproverContactID = approverID;
                if (o.AuthorContactID.HasValue)
                    _users.EmailUsers(_users.GetUserEmails(new Guid[] { o.AuthorContactID.Value }), "Model approved", note);
                var n = new BlockModelMetadata();
                n.BlockModelMetadataID = Guid.NewGuid();
                n.BlockModelID = modelID;
                n.ParameterID = _privateServices.NKD_GUID_LOG;
                var oc = new Occurrence();
                oc.ID = Guid.NewGuid();
                oc.ContactID = o.ApproverContactID.Value;
                oc.Occurred = DateTime.UtcNow;
                oc.Status = (uint)Occurrence.StatusCode.OK;
                n.BlockModelMetadataText = oc.ToJson();
                d.BlockModelMetadatas.AddObject(n);
                d.SaveChanges();
            }

        }

        public void DenyModel(Guid modelID, Guid denierID, string error)
        {
            using (new TransactionScope(TransactionScopeOption.Suppress))
            {
                var d = new NKDC(_users.ApplicationConnectionString, null);
                var o = d.BlockModels.Where(f => f.BlockModelID == modelID && f.AuthorContactID != null).Select(f => (Guid)f.AuthorContactID).ToArray();
                _users.EmailUsers(_users.GetUserEmails(o), "Model not accepted", error);
                var n = new BlockModelMetadata();
                n.BlockModelMetadataID = Guid.NewGuid();
                n.BlockModelID = modelID;
                n.ParameterID = _privateServices.NKD_GUID_LOG;
                var oc = new Occurrence();
                oc.ID = Guid.NewGuid();
                oc.ContactID = denierID;
                oc.Occurred = DateTime.UtcNow;
                oc.Status = (uint)Occurrence.StatusCode.Notified;
                n.BlockModelMetadataText = oc.ToJson();
                d.BlockModelMetadatas.AddObject(n);
                d.SaveChanges();
            }
        }

        ////////////////
        //Nick Code
        ///

        /// <summary>
        /// Set meta data for domains that are cached during model import
        /// </summary>
        /// <param name="domains"></param>
        /// <param name="blockModelID"></param>
        public void UpdateDomains(List<Tuple<string, string>> domains, Guid blockModelID)
        {

            if (domains != null)
            {
                string metaDataType = "Domains";
                string tableType = "X_BlockModel";
                string cont = ObjectHelper.ToJson(domains);
                SetMetaDataItem(blockModelID, metaDataType, tableType, cont);
            }
        }

        public void UpdateDomains(List<string> domains, Guid blockModelID)
        {
            if (domains != null)
            {
                string metaDataType = "Domains";
                string tableType = "X_BlockModel";
                string cont = ObjectHelper.ToJson(domains);
                SetMetaDataItem(blockModelID, metaDataType, tableType, cont);
            }
        }


        /// <summary>
        /// Add notes into the meta data associated with a model
        /// </summary>
        /// <param name="notes"></param>
        /// <param name="blockModelID"></param>
        public void AddModelNotes(string notes, Guid blockModelID)
        {
            if (notes != null)
            {
                string cont = ObjectHelper.ToJson(notes);
                string metaDataType = "Note";
                string tableType = "X_BlockModel";
                SetMetaDataItem(blockModelID, metaDataType, tableType, cont);
            }

        }

        /// <summary>
        /// Set an item of meta data with the given types and cvalues
        /// </summary>
        /// <param name="blockModelID"></param>
        /// <param name="metaDataType"></param>
        /// <param name="tableType"></param>
        /// <param name="cont"></param>
        public void SetMetaDataItem(Guid blockModelID, string metaDataType, string tableType, string cont)
        {
            try
            {
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    var d = new NKDC(_users.ApplicationConnectionString, null);

                    MetaData dt = new MetaData();
                    dt.MetaDataID = Guid.NewGuid();
                    dt.MetaDataType = metaDataType;
                    dt.ContentToIndex = cont;

                    MetaDataRelation rel = new MetaDataRelation();
                    rel.MetaDataRelationID = Guid.NewGuid();
                    rel.MetaDataID = dt.MetaDataID;
                    rel.TableType = tableType;
                    rel.ReferenceID = blockModelID;

                    d.MetaDatas.AddObject(dt);
                    d.SaveChanges();
                    d.MetaDataRelations.AddObject(rel);
                    d.SaveChanges();
                }
            }
            catch { }

        }


        public void UpdateStage(Guid blockModelGUID, Guid stageMetaID, string stage)
        {
            if (stage != null && stage.Length > 0)
            {
                try
                {
                    using (new TransactionScope(TransactionScopeOption.Suppress))
                    {
                        var d = new NKDC(_users.ApplicationConnectionString, null);
                        Parameter dt = new Parameter();
                        dt.ParameterID = stageMetaID;
                        dt.ParameterName = "Stage";
                        dt.ParameterType = "Metadata";

                        BlockModelMetadata rel = new BlockModelMetadata();
                        rel.BlockModelID = blockModelGUID;
                        rel.BlockModelMetadataID = Guid.NewGuid();
                        rel.ParameterID = stageMetaID;
                        rel.IsColumnData = false;
                        rel.BlockModelMetadataText = stage;

                        d.Parameters.AddObject(dt);
                        d.SaveChanges();
                        d.BlockModelMetadatas.AddObject(rel);
                        d.SaveChanges();
                    }
                }
                catch
                {
                }
            }

        }

    }
}
