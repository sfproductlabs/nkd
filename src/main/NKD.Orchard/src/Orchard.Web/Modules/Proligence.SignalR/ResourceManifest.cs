using Orchard.UI.Resources;

namespace Proligence.SignalR {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var manifest = builder.Add();
            manifest.DefineScript("jQuery_SignalR").SetUrl("jquery.signalr.min.js", "jquery.signalr.js").SetDependencies("jQuery");
            manifest.DefineScript("jQuery_SignalR_Hubs").SetUrl("~/signalr/hubs").SetDependencies("jQuery_SignalR");
        }
    }
}
