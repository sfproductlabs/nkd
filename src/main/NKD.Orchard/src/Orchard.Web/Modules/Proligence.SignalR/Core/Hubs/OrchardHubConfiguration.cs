using JetBrains.Annotations;
using Microsoft.AspNet.SignalR;

namespace Proligence.SignalR.Core.Hubs
{
    [UsedImplicitly]
    public class OrchardHubConfiguration : IOrchardHubConfiguration {
        private readonly IDependencyResolver _dependencyResolver;

        public OrchardHubConfiguration(IDependencyResolver dependencyResolver) {
            _dependencyResolver = dependencyResolver;
        }

        // TODO: This should be editable via site or per-connection settings
        public HubConfiguration ConnectionConfiguration {
            get { 
                return new HubConfiguration
                {
                    EnableJavaScriptProxies = true,
                    EnableDetailedErrors = true,
                    Resolver = _dependencyResolver
                }; 
            }
        }
    }
}