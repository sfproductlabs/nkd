using System.Collections.Generic;
using System.Linq;
using Contrib.Taxonomies.Models;
using Contrib.Taxonomies.Services;
using JetBrains.Annotations;
using Orchard.Gallery.Interfaces;

namespace Orchard.Gallery.Impl {
    [UsedImplicitly]
    public class CategoryGetter : ICategoryGetter {
        private readonly ITaxonomyService _taxonomyService;

        public CategoryGetter(ITaxonomyService taxonomyService) {
            _taxonomyService = taxonomyService;
        }

        public List<string> GetCategoriesWithAssociatedContentItems(TermPart packageTypeTerm) {
            var categoriesList = new List<string>();
            //TODO:Check
            var categories = _taxonomyService.GetChildren(packageTypeTerm)
                .Where(ct => _taxonomyService.GetContentItems(ct).Any())
                .Select(c => c.Name).OrderBy(c => c);
            if (categories.Count() > 0)
            {
                categoriesList.Add("All Categories");
                categoriesList.AddRange(categories);
            }
            return categoriesList;
        }
    }
}