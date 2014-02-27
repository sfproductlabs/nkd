using Orchard.ContentManagement;

namespace Orchard.Gallery.Interfaces {
    public interface ITypeCaster : IDependency {
        TDestinationType CastTo<TDestinationType>(IContent content)
            where TDestinationType : IContent;
    }
}