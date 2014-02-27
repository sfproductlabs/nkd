using JetBrains.Annotations;
using Orchard.Logging;
using Orchard.Tasks;
using Orchard;
using Orchard.Localization;
using Orchard.ContentManagement;
using Orchard.Services;
using NKD.Models;
using System.Linq;
using Orchard.Data;
using Orchard.ContentManagement.Handlers;
using Orchard.Caching;
using System;
using NKD.Helpers;

namespace NKD.Services {
    /// <summary>
    /// Regularly fires user sync events
    /// </summary>
    [UsedImplicitly]
    public class ModelCheckerBackgroundTask : IBackgroundTask
    {
        public const string NEXT_UPDATE_CACHE_KEY = "NKD.ModelCheckerBackgroundTask.NextUpdate.Changed";
        public const string MODEL_FILES_HASH_CACHE_KEY = "NKD.ModelCheckerBackgroundTask.ModelFiles.Changed";
        private readonly ICacheManager _cache;
        private readonly ISignals _signals;
        private readonly IBlockModelService _modelService;
        private readonly IProjectsService _projectService;
        private readonly IClock _clock;
        public IOrchardServices Services { get; set; }
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }


        public ModelCheckerBackgroundTask(IOrchardServices services, IBlockModelService modelService, IClock clock, ICacheManager cache, ISignals signals, IProjectsService projects)
        {
            _signals = signals;
            _cache = cache;
            _projectService = projects;
            Services = services;
            _modelService = modelService;
            _clock = clock;
            Logger = NullLogger.Instance;
        }

        private string getNewBlockModelFilesHash()
        {
            return string.Join(",", _modelService.GetNewBlockModelFiles().Select(f => f.FolderName + @"\" + f.Name).ToArray()).ComputeHash();
        }

        public void Sweep()
        {
            DateTime? nextUpdate = _cache.Get<string, DateTime?>(NEXT_UPDATE_CACHE_KEY, ctx =>
            {
                ctx.Monitor(_signals.When(NEXT_UPDATE_CACHE_KEY));
                return _clock.UtcNow.AddHours(6);
            });

            string oldFiles = _cache.Get<string, string>(MODEL_FILES_HASH_CACHE_KEY, ctx =>
            {                
                ctx.Monitor(_signals.When(MODEL_FILES_HASH_CACHE_KEY));
                return getNewBlockModelFilesHash();
            });

            if (_clock.UtcNow > nextUpdate.GetValueOrDefault())
            {
                _signals.Trigger(NEXT_UPDATE_CACHE_KEY);
                _modelService.CheckModels(); //Removes models over 3 months (set in SQL SP)
                string newfiles = getNewBlockModelFilesHash();
                if (oldFiles != newfiles)
                {
                    _signals.Trigger(MODEL_FILES_HASH_CACHE_KEY);
                    _projectService.EmailAllProjectOwners("New Model Uploaded", "A new model has been uploaded, and is ready for import into a project into NKD.");
                }
                Logger.Information(string.Format("Successfully updated old models (synchronisation completed).", DateTime.UtcNow.ToLongDateString()));
            }
        }
    }
}
