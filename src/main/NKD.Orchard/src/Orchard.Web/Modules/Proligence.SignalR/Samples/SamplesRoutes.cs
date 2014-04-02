using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Environment.Extensions;
using Orchard.Mvc.Routes;

namespace Proligence.SignalR.Samples
{
    [OrchardFeature("Proligence.SignalR.Core.Samples")]
    public class SamplesRoutes : IRouteProvider
    {
        #region IRouteProvider Members

        public void GetRoutes(ICollection<RouteDescriptor> routes)
        {
            foreach (RouteDescriptor routeDescriptor in this.GetRoutes())
            {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes()
        {
            return new[] {
                new RouteDescriptor {
                    Priority = 12,
                    Route = new Route(
                        "Samples/{action}",
                        new RouteValueDictionary {
                            {"area", "Proligence.SignalR"},
                            {"controller", "Samples"},
                            {"action", "Raw"},
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "Proligence.SignalR"}
                        },
                        new MvcRouteHandler())
                }
            };
        }

        #endregion
    }
}