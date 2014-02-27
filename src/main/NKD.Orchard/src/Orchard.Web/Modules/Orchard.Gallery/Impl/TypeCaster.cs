using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Gallery.Interfaces;

namespace Orchard.Gallery.Impl {
    [UsedImplicitly]
    public class TypeCaster : ITypeCaster {
        public TDestinationType CastTo<TDestinationType>(IContent content)
            where TDestinationType : IContent {
            return content.As<TDestinationType>();
        }
    }
}