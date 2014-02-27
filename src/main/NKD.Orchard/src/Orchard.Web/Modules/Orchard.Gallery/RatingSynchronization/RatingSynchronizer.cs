using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using Contrib.Voting.Functions;
using Contrib.Voting.Models;
using Gallery.Core.Domain;
using JetBrains.Annotations;
using Microsoft.Http;
using Orchard.ContentManagement;
using Orchard.Data;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;
using Orchard.Gallery.PackageSynchronization;
using Orchard.Gallery.Services;
using Orchard.Logging;
using Orchard.Services;

namespace Orchard.Gallery.RatingSynchronization {
    [UsedImplicitly]
    public class RatingSynchronizer : ThreadSafeActionBase, IRatingSynchronizer {
        private readonly IRepository<ResultRecord> _resultRecordRepository;
        private readonly IPackageService _packageService;
        private readonly IOrchardServices _orchardServices;
        private readonly ITypeCaster _typeCaster;
        private readonly IClock _clock;
        private readonly INonceCache _nonceCache;

        public ILogger Logger { get; set; }

        private readonly Lazy<string> _serviceRoot;

        public RatingSynchronizer(IRepository<ResultRecord> resultRecordRepository, IPackageService packageService, IOrchardServices orchardServices,
            ITypeCaster typeCaster, IClock clock, INonceCache nonceCache) {
            _resultRecordRepository = resultRecordRepository;
            _nonceCache = nonceCache;
            _packageService = packageService;
            _orchardServices = orchardServices;
            _typeCaster = typeCaster;
            _clock = clock;

            Logger = NullLogger.Instance;

            _serviceRoot = new Lazy<string>(() => _orchardServices.WorkContext.CurrentSite.As<GallerySettingsPart>().ServiceRoot);
        }

        protected override void ExecuteThreadSafeAction() {
            DateTime? lastRatingSyncTimeSetting = _orchardServices.WorkContext.CurrentSite.As<GallerySettingsPart>().LastRatingSyncTime;
            DateTime lastRatingSyncTime = lastRatingSyncTimeSetting.HasValue ? lastRatingSyncTimeSetting.Value : new DateTime(1900, 1, 1);

            IEnumerable<ResultRecord> resultRecords = _resultRecordRepository.Fetch(rr => rr.ContentType == "Package" && rr.FunctionName == new Average().Name
                && rr.CreatedUtc > lastRatingSyncTime);
            DateTime utcNow = _clock.UtcNow;
            var packageRatingAggregates = new List<PackageVersionRatings>();
            foreach (var resultRecord in resultRecords) {
                ResultRecord closureResultRecord = resultRecord;
                var package = _packageService.Get(p => p.ContentItemRecord.Id == closureResultRecord.ContentItemRecord.Id).SingleOrDefault();
                if (package != null) {
                    var packageRatingAggregate = new PackageVersionRatings
                    {
                        PackageId = package.PackageID,
                        PackageVersion = package.PackageVersion,
                        RatingAverage = closureResultRecord.Value,
                        RatingCount = closureResultRecord.Count
                    };
                    packageRatingAggregates.Add(packageRatingAggregate);
                }
            }

            if (packageRatingAggregates.Any()) {
                using (var client = new HttpClient(_serviceRoot.Value)) {
                    HttpContent content = HttpContentExtensions.CreateDataContract(packageRatingAggregates);
                    _nonceCache.Nonce = Guid.NewGuid().ToString();
                    string uri = string.Format("{0}/{1}/{2}", ServiceConstants.PackageServiceName, "UpdatePackageRatings", _nonceCache.Nonce);
                    using (HttpResponseMessage response = client.Put(uri, content)) {
                        if (response.StatusCode == HttpStatusCode.OK) {
                            _typeCaster.CastTo<GallerySettingsPart>(_orchardServices.WorkContext.CurrentSite).LastRatingSyncTime = utcNow;
                        }
                        else {
                            Logger.Error("Call to UpdatePackageRatings on Gallery.Server failed.");
                        }
                    }
                }
            }
        }
    }
}