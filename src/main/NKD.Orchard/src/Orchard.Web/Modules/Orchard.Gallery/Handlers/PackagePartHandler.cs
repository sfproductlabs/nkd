using System;
using System.Collections.Generic;
using Contrib.Taxonomies.Models;
using Contrib.Taxonomies.Services;
using Contrib.Voting.Functions;
using Contrib.Voting.Models;
using JetBrains.Annotations;
using Lucene.Net.QueryParsers;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Gallery.Models;
using Orchard.Tags.Models;

namespace Orchard.Gallery.Handlers {
    [UsedImplicitly]
    public class PackagePartHandler : ContentHandler {
        public PackagePartHandler(IRepository<PackagePartRecord> packageRepository, IRepository<ResultRecord> resultRecordRepository,
            ITaxonomyService taxonomyService) {
            Filters.Add(StorageFilter.For(packageRepository));

            OnIndexing<PackagePart>((context, packagePart) => {
                IEnumerable<TermPart> termsForContentItem = taxonomyService.GetTermsForContentItem(packagePart.ContentItem.Id);
                var tags = packagePart.As<TagsPart>().CurrentTags;

                var resultRecord = resultRecordRepository
                    .Get(rr => rr.ContentItemRecord.Id == packagePart.ContentItem.Id && rr.FunctionName == new Average().Name);
                double packageRating = resultRecord != null ? resultRecord.Value : 0;

                context.DocumentIndex
                    .Add("package-id", packagePart.PackageID).Analyze().Store()
                    .Add("package-id", packagePart.PackageID.Replace('.', ' ')).Analyze()
                    .Add("package-title", packagePart.Title).Store()
                    .Add("package-title-parts", packagePart.Title).Analyze()
                    .Add("package-version", packagePart.PackageVersion).Store()
                    .Add("package-summary", packagePart.Summary).Analyze()
                    .Add("package-description", packagePart.Description).Analyze()
                    .Add("package-download-count", packagePart.TotalDownloadCount.ToString("D8"))
                    .Add("package-authors", QueryParser.Escape(packagePart.Authors.ToLower())).Store()
                    .Add("package-authors-parts", packagePart.Authors).Analyze()
                    .Add("package-rating", packageRating.ToString())
                    .Add("is-recommended-version", packagePart.IsRecommendedVersion);

                if (packagePart.Created.HasValue) {
                    context.DocumentIndex.Add("package-created", packagePart.Created.Value).Store();
                }

                foreach (var term in termsForContentItem) {
                    context.DocumentIndex.Add("term-id", term.Id).Store();
                }
            });

            OnRemoved<PackagePart>((contentContext, part) => packageRepository.Delete(part.Record));
        }
    }
}