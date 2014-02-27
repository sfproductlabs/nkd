using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard;
using Orchard.Tasks.Scheduling;
using Orchard.Logging;
using Orchard.ContentManagement;
using NKD.Models;

namespace NKD.Services
{
    public class ProcessModelScheduledTaskHandler : IScheduledTaskHandler
    {
        public const string TASK_TYPE_PROCESS_MODEL = "ProcessModelScheduled";
        private readonly IBlockModelService _blockModelService;

        public ILogger Logger { get; set; }

        public ProcessModelScheduledTaskHandler(IBlockModelService blockModelService)
        {
            _blockModelService = blockModelService;           
            Logger = NullLogger.Instance;
        }

        public void Process(ScheduledTaskContext context)
        {
            if (context.Task.TaskType == TASK_TYPE_PROCESS_MODEL && context.Task.ContentItem != null)
            {
                try
                {                    
                    _blockModelService.ProcessModel(context.Task.ContentItem);
                }
                catch (Exception e)
                {
                    this.Logger.Error(e, e.Message);
                }
                finally
                {

                }
            }
        }

    }
}