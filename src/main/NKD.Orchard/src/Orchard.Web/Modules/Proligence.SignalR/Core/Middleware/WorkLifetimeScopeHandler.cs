using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Routing;
using Orchard;
using Orchard.Data;

namespace Proligence.SignalR.Core.Middleware
{
    public class WorkLifetimeScopeHandler
    {
        private readonly Func<IDictionary<string, object>, Task> _next;

        public WorkLifetimeScopeHandler(Func<IDictionary<string, object>, Task> next)
        {
            _next = next;
        }

        public Task Invoke(IDictionary<string, object> env)
        {
            var requestContext = (RequestContext)env["System.Web.Routing.RequestContext"];
            var workContextAccessor = (IWorkContextAccessor)requestContext.RouteData.DataTokens["IWorkContextAccessor"];

            using (var scope = workContextAccessor.CreateWorkContextScope(requestContext.HttpContext))
            {
                var tm = scope.Resolve<ITransactionManager>();
                tm.Demand();
                try
                {
                    var task = _next(env);
                    return task;
                }
                catch
                {
                    tm.Cancel();
                    throw;
                }
            }
        }
    }
}