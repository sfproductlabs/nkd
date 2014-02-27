using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard;
using Orchard.Tasks.Scheduling;
using Orchard.Logging;
using Orchard.ContentManagement;
using NKD.Models;
using NKD.Helpers;
namespace NKD.Services
{
    public class AppendModelScheduledTaskHandler : IScheduledTaskHandler
    {
        public const string TASK_TYPE_APPEND_MODEL = "AppendModelScheduled";
        private readonly IBlockModelService _blockModelService;

        public ILogger Logger { get; set; }

        public AppendModelScheduledTaskHandler(IBlockModelService blockModelService)
        {
            _blockModelService = blockModelService;           
            Logger = NullLogger.Instance;
        }

        public void Process(ScheduledTaskContext context)
        {
            if (context.Task.TaskType == TASK_TYPE_APPEND_MODEL && context.Task.ContentItem != null)
            {
                try
                {
                    _blockModelService.AppendModel(context.Task.ContentItem);
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