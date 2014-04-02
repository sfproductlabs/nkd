using System.Threading.Tasks;
using System.Web.Routing;
using Microsoft.Owin;
using Orchard;
using Orchard.Data;

namespace Proligence.SignalR.Core.Middleware
{
    public class WorkContextScopeMiddleware : OwinMiddleware
    {
        public WorkContextScopeMiddleware(OwinMiddleware next)
            : base(next)
        {
        }

        public override Task Invoke(IOwinContext context)
        {
            var requestContext = (RequestContext)context.Request.Environment["System.Web.Routing.RequestContext"];
            var workContextAccessor = (IWorkContextAccessor)requestContext.RouteData.DataTokens["IWorkContextAccessor"];
            var scope = workContextAccessor.GetContext(requestContext.HttpContext);

            if (scope != null)
            {
                var tm = scope.Resolve<ITransactionManager>();
                tm.Demand();

                return Next.Invoke(context).ContinueWith(t =>
                {
                    if (t.IsFaulted || t.IsCanceled || t.Exception != null)
                    {
                        tm.Cancel();
                    }
                });
            }

            return Next.Invoke(context);
        }
    }
}