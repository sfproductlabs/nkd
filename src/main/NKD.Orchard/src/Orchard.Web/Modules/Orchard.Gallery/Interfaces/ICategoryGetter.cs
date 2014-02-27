using System.Collections.Generic;
using Contrib.Taxonomies.Models;

namespace Orchard.Gallery.Interfaces {
    public interface ICategoryGetter : IDependency {
        List<string> GetCategoriesWithAssociatedContentItems(TermPart packageTypeTerm);
    }
}