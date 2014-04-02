using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.AspNet.SignalR.Hubs;

namespace Proligence.SignalR.Core.Hubs
{
    [UsedImplicitly]
    public class DefaultHubDescriptorProvider : IHubDescriptorProvider
    {
        private readonly ITypeHarvester _harvester;
        private readonly Lazy<IDictionary<string, HubDescriptor>> _hubs;

        public DefaultHubDescriptorProvider(ITypeHarvester harvester)
        {
            _harvester = harvester;
            _hubs = new Lazy<IDictionary<string, HubDescriptor>>(BuildHubsCache);
        }

        public IList<HubDescriptor> GetHubs()
        {
            return _hubs.Value
                .Select(kv => kv.Value)
                .Distinct()
                .ToList();
        }

        public bool TryGetHub(string hubName, out HubDescriptor descriptor)
        {
            return _hubs.Value.TryGetValue(hubName, out descriptor);
        }

        protected IDictionary<string, HubDescriptor> BuildHubsCache()
        {
            var types = _harvester.Get(IsHubType).Select(tt => tt.Item1);

            // Building cache entries for each descriptor
            // Each descriptor is stored in dictionary under a key
            // that is it's name or the name provided by an attribute
            var cacheEntries = types
                .Select(type => new HubDescriptor
                {
                    NameSpecified = (GetHubAttributeName(type) != null),
                    Name = GetHubName(type),
                    HubType = type
                })
                .ToDictionary(hub => hub.Name,
                              hub => hub,
                              StringComparer.OrdinalIgnoreCase);

            return cacheEntries;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "If we throw then it's not a hub type")]
        private static bool IsHubType(Type type)
        {
            try
            {
                return typeof(IHub).IsAssignableFrom(type) &&
                       !type.IsAbstract &&
                       (type.Attributes.HasFlag(TypeAttributes.Public) ||
                        type.Attributes.HasFlag(TypeAttributes.NestedPublic));
            }
            catch
            {
                return false;
            }
        }

        private static string GetHubAttributeName(Type type)
        {
            return ReflectionHelper.GetAttributeValue<HubNameAttribute, string>(type, attr => attr.HubName);
        }

        internal static string GetHubName(Type type)
        {
            return GetHubAttributeName(type) ?? type.Name;
        }
    }
}