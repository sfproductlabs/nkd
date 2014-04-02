using Orchard.UI.Resources;

namespace Proligence.SignalR {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineScript("jQuery_SignalR").SetUrl("jquery.signalR-2.0.2.min.js", "jquery.signalR-2.0.2.js").SetVersion("2.0.2").SetDependencies("jQuery");
            manifest.DefineScript("jQuery_SignalR_Hubs").SetUrl("~/signalr/hubs").SetVersion("2.0.2").SetDependencies("jQuery_SignalR");
        }
    }
}
