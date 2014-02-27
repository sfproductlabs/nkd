using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Core.Scheduling.Models;
using Orchard.Data;
using Orchard.Logging;
using Orchard.Services;
using Orchard.Tasks;
using Orchard.Tasks.Scheduling;
using System.Transactions;
using NHibernate;
using Orchard.Environment.Configuration;
using Orchard.Environment;
using Orchard.Environment.State;
using Orchard.Environment.Descriptor;

namespace Orchard.Core.Scheduling.Services {
    [UsedImplicitly]
    public class ScheduledTaskExecutor : IBackgroundTask {
        private readonly IClock _clock;
        private readonly IRepository<ScheduledTaskRecord> _repository;
        private readonly IEnumerable<IScheduledTaskHandler> _handlers;
        private readonly IContentManager _contentManager;
        private readonly ISessionLocator _sessionLocator;
        private readonly IOrchardServices _orchardServices;
        private readonly ITransactionManager _transactionManager;

        public ScheduledTaskExecutor(
            IClock clock,
            IRepository<ScheduledTaskRecord> repository,
            IEnumerable<IScheduledTaskHandler> handlers,
            ISessionLocator sessionLocator,
            ITransactionManager transactionManager, 
            ShellSettings shellSettings, IWorkContextAccessor workContextAccessor, IRunningShellTable runningShellTable,
             IProcessingEngine processingEngine,           
            IShellDescriptorManager shellDescriptorManager,
            IOrchardServices orchardServices,
            IContentManager contentManager) {
            _clock = clock;
            _orchardServices = orchardServices;
            _repository = repository;
            _transactionManager = transactionManager;
            _handlers = handlers;
            _contentManager = contentManager;
            _sessionLocator = sessionLocator;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void Sweep() {


            try
            {
                while (true)
                {
                    ScheduledTaskRecord taskToRun = new ScheduledTaskRecord();
                    using (var scope = new TransactionScope(TransactionScopeOption.Suppress, new TransactionOptions() { IsolationLevel = IsolationLevel.Serializable }))
                    {
                        var taskRecord = _repository.Fetch(x => x.ScheduledUtc <= _clock.UtcNow).FirstOrDefault();
                        // another server or thread has performed this work before us
                        if (taskRecord == null)
                        {
                            break;
                        }
                        _repository.Copy(taskRecord, taskToRun);
                        _repository.Delete(taskRecord);
                        _repository.Flush();
                        scope.Complete();
                    }

                    //using (ISession _session = _sessionLocator.For(typeof(ScheduledTaskRecord)))
                    //{
                    //    using (var tx = _session.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted))
                    //    {
                    using (var scope = new TransactionScope(TransactionScopeOption.Required))
                    {
                        var context = new ScheduledTaskContext
                        {
                            Task = new Task(_contentManager, taskToRun)
                        };

                        // dispatch to standard or custom handlers
                        foreach (var handler in _handlers)
                        {
                            handler.Process(context);
                        }
                        scope.Complete();
                    }
                
                    _contentManager.Flush();
                    _contentManager.Clear();

                }
            }
            catch (Exception ex)
            {
                Logger.Warning(ex, "Unable to process scheduled task.");

                //TODO: handle exception to rollback dedicated xact, and re-delete task record. 
                // does this also need some retry logic?
                //Maybe put in taskToRun back in if retry parameter included?
            }
            
        }
    }
}
