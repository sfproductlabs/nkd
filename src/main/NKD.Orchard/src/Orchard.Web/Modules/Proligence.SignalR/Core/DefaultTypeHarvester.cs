using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.Environment.Extensions;
using Orchard.Environment.Extensions.Models;
using Orchard.Environment.ShellBuilders.Models;

namespace Proligence.SignalR.Core
{
    /// <summary>
    /// Harvests types exposed by enabled features.
    /// </summary>
    public class DefaultTypeHarvester : ITypeHarvester
    {
        private readonly IExtensionManager _extensions;
        private readonly ShellBlueprint _shell;

        public DefaultTypeHarvester(IExtensionManager extensions, ShellBlueprint shell)
        {
            _extensions = extensions;
            _shell = shell;
        }

        public IEnumerable<Tuple<Type, Feature>> Get(Func<Type, bool> predicate)
        {
            return _extensions
                .LoadFeatures(_extensions.EnabledFeatures(_shell.Descriptor))
                .SelectMany(feature => feature.ExportedTypes
                    .Where(predicate)
                    .Select(c => new Tuple<Type, Feature>(c, feature)));
        }

        public IEnumerable<Tuple<Type, Feature>> Get(Type type)
        {
            return Get(type.IsAssignableFrom);
        }

        public IEnumerable<Tuple<Type, Feature>> Get<T>()
        {
            return Get(typeof(T));
        }
    }
}