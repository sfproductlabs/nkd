using JetBrains.Annotations;
using Microsoft.AspNet.SignalR;

namespace Proligence.SignalR.Core
{
    [UsedImplicitly]
    public class OrchardHubConfiguration : IOrchardHubConfiguration {
        private readonly IDependencyResolver _dependencyResolver;

        public OrchardHubConfiguration(IDependencyResolver dependencyResolver) {
            _dependencyResolver = dependencyResolver;
        }

        public HubConfiguration ConnectionConfiguration {
            get { return new HubConfiguration { Resolver = _dependencyResolver }; }
        }
    }
}