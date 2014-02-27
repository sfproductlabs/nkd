using System;

namespace Orchard.Gallery.Interfaces {
    public interface IServiceUriBuilder : IDependency {
        string BuildServiceUri(string serviceName, string packageId, string packageVersion);
        string BuildServiceUri(string packageServiceName, string packageId, string packageVersion, Guid userkey);
    }
}