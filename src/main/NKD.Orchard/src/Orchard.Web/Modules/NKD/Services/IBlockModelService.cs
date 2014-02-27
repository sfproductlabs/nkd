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
using NKD.Module.BusinessObjects;

namespace NKD.Services
{
     [ServiceContract]
    public interface IBlockModelService : IDependency 
    {
         [OperationContract]
         IEnumerable<MediaFile> GetNewBlockModelFiles();

         [OperationContract]
         IEnumerable<MediaFile> GetUpdateFileNameList();

         [OperationContract]
         IEnumerable<MediaFile> GetNewFormatFiles();

         [OperationContract]
         BlockModelAppendViewModel GetBlockModelToAppend(Guid modelID);

         [OperationContract]
         IEnumerable<BlockModel> GetModels();

         [OperationContract]
         IEnumerable<BlockModel> GetModelsCurrent();

         [OperationContract]
         string GetModelAlias(Guid modelID);

         [OperationContract]
         IEnumerable<Tuple<Parameter, BlockModelMetadata>> GetModelParameters(Guid modelID);
         
         [OperationContract]
         string GetBlockModelInfo(Guid guid);

         [OperationContract]
         void ProcessModelAsync(string bmFileName, string formatFileName, string projectID, string alias, Guid userID, string notes, string stage, Guid stageMetaID, int allowImportAfterMinutes = 0);

         [OperationContract]
         void ProcessModel(ContentItem m);

         [OperationContract]
         void AppendModelAsync(Guid bmGuid, string bmFileName, string alias, string columnNameToImport, int columnIndexToImport, Guid userGuid, int allowImportAfterMinutes = 0);

         [OperationContract]
         void AppendModel(ContentItem m);

         [OperationContract]
         void DeleteModel(Guid modelID, Guid deletedByGuid);

         [OperationContract]
         List<string> GetImportFileColumnsAsList(Guid guid, string modelFileName, string modelAlias);

		 [OperationContract]
         IEnumerable<Tuple<string,string>> GetModelDomains(Guid modelID);

         /// <summary>
         /// Removes old files, removes old models, checks for new files (future - automatically add model?)
         /// </summary>
         /// <returns></returns>
         [OperationContract]
         void CheckModels();

         [OperationContract]
         Task<IReport> CompareModelsAsync(BlockModelCompareViewModel models);

         [OperationContract]
         IReport CompareModels(BlockModelCompareViewModel models);

         [OperationContract]
         void UpdateModelParameter(BlockModelParameterViewModel m);

         [OperationContract]
         void ApproveModel(Guid modelID, Guid approverID, string note);

         [OperationContract]
         void DenyModel(Guid modelID, Guid denierID, string error);
    }
}