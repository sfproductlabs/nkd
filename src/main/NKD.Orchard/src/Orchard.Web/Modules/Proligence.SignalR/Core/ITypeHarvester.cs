using System;
using System.Collections.Generic;
using Orchard;
using Orchard.Environment.Extensions.Models;

namespace Proligence.SignalR.Core
{
    /// <summary>
    /// Retrieves types matching a specified criteria, available in an Orchard instance.
    /// </summary>
    public interface ITypeHarvester : ISingletonDependency
    {
        IEnumerable<Tuple<Type, Feature>> Get(Func<Type, bool> predicate);
        IEnumerable<Tuple<Type, Feature>> Get(Type type);
        IEnumerable<Tuple<Type, Feature>> Get<T>();
    }
}