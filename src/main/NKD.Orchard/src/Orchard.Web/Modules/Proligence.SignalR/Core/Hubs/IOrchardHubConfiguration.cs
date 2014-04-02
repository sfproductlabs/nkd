using Microsoft.AspNet.SignalR;
using Orchard;

namespace Proligence.SignalR.Core.Hubs {
    public interface IOrchardHubConfiguration : IDependency {
        HubConfiguration ConnectionConfiguration { get; }
    }
}