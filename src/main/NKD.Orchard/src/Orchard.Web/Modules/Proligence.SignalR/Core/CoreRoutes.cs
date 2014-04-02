using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using System.Web.SessionState;
using JetBrains.Annotations;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Host.SystemWeb;
using Orchard.Environment.Extensions.Models;
using Orchard.Logging;
using Orchard.Mvc.Routes;
using Owin;
using Proligence.SignalR.Core.Middleware;

namespace Proligence.SignalR.Core
{
    [UsedImplicitly]
    public class CoreRoutes : IRouteProvider
    {
        private readonly ITypeHarvester _harvester;
        private readonly IOrchardHubConfiguration _orchardHubConfiguration;

        public CoreRoutes(ITypeHarvester harvester, IOrchardHubConfiguration orchardHubConfiguration)
        {
            _harvester = harvester;
            _orchardHubConfiguration = orchardHubConfiguration;
        }

        public ILogger Logger { get; set; }

        public void GetRoutes(ICollection<RouteDescriptor> routes)
        {
            foreach (RouteDescriptor routeDescriptor in GetRoutes())
            {
                routes.Add(routeDescriptor);
            }
        }

        public IEnumerable<RouteDescriptor> GetRoutes()
        {
            yield return new RouteDescriptor
            {
                Route = new RouteCollection().MapOwinPath("signalr.hubs", "/signalr", map =>
                {
                    map.Use(typeof(WorkLifetimeScopeHandler));
                    map.MapHubs(string.Empty, _orchardHubConfiguration.ConnectionConfiguration);                
                }),
                SessionState = SessionStateBehavior.Disabled,
                Priority = int.MaxValue
            };

            foreach (var tuple in _harvester.Get<PersistentConnection>())
            {
                var attrs = tuple.Item1.GetCustomAttributes(typeof(ConnectionAttribute), false);

                var typeName = tuple.Item1.Name.ToLowerInvariant();
                var connectionName = typeName.Contains("connection")
                                            ? typeName.Substring(0, typeName.IndexOf("connection", System.StringComparison.Ordinal))
                                            : typeName;
                string connectionUrl = connectionName;

                if (attrs.Any())
                {
                    var attrName = ((ConnectionAttribute)attrs[0]).Name;
                    var attrUrl = ((ConnectionAttribute)attrs[0]).Url;

                    connectionName = !string.IsNullOrWhiteSpace(attrName) ? attrName : connectionName;
                    connectionUrl = connectionName;

                    connectionUrl = !string.IsNullOrWhiteSpace(attrUrl) ? attrUrl : connectionUrl;
                }

                Tuple<Type, Feature> tuple1 = tuple;
                yield return new RouteDescriptor
                {
                    Route = new RouteCollection().MapOwinPath(connectionName, "/" + connectionUrl.TrimStart('/'), map =>
                    {
                        map.Use(typeof(WorkLifetimeScopeHandler));
                        map.MapConnection(string.Empty, tuple1.Item1, _orchardHubConfiguration.ConnectionConfiguration);
                    }),
                    SessionState = SessionStateBehavior.Disabled,
                    Priority = int.MaxValue - 1
                };
            }
        }

    }
}