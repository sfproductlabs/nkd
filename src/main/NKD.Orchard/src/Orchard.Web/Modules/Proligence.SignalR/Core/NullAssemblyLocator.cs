using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNet.SignalR.Hubs;

namespace Proligence.SignalR.Core
{
    /// <summary>
    /// Fake assembly locator.
    /// </summary>
    public class NullAssemblyLocator : IAssemblyLocator
    {
        public IList<Assembly> GetAssemblies()
        {
            return new List<Assembly>();
        }
    }
}