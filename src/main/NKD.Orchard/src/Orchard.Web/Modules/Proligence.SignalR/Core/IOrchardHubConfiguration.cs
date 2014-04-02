using Microsoft.AspNet.SignalR;
using Orchard;

namespace Proligence.SignalR.Core {
    public interface IOrchardHubConfiguration : IDependency {
        HubConfiguration ConnectionConfiguration { get; }
    }
}