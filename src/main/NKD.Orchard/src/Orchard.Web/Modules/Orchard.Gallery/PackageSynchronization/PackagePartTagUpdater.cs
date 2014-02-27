using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Orchard.Gallery.GalleryServer;
using Orchard.Gallery.Models;
using Orchard.Tags.Services;

namespace Orchard.Gallery.PackageSynchronization {
    [UsedImplicitly]
    public class PackagePartTagUpdater : IPackagePartTagUpdater {
        private readonly ITagService _tagService;

        public PackagePartTagUpdater(ITagService tagService) {
            _tagService = tagService;
        }

        public void UpdateTags(PublishedPackage publishedPackage, PackagePart packagePart) {
            if (publishedPackage.Tags != null)
            {
                string whitespaceProcessedTagString = new Regex(@"\s+").Replace(publishedPackage.Tags.Trim(), " ");
                _tagService.UpdateTagsForContentItem(packagePart.ContentItem, whitespaceProcessedTagString.Split(' '));
            }
        }

    }
}