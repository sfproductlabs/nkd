using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Gallery.Core.Domain;
using JetBrains.Annotations;
using Microsoft.Http;
using Orchard.ContentManagement;
using Orchard.Gallery.Extensions;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;
using Orchard.Localization;
using Orchard.UI.Notify;

namespace Orchard.Gallery.Impl {
    [UsedImplicitly]
    public class GalleryPackageService : IGalleryPackageService {
        private readonly IOrchardServices _orchardServices;
        private readonly IServiceUriBuilder _serviceUriBuilder;
        private readonly IPackageService _packageService;
        private readonly IUserkeyService _userKeyService;

        public Localizer T { get; set; }

        private readonly Lazy<string> _serviceRoot;

        public GalleryPackageService(IOrchardServices orchardServices, IServiceUriBuilder serviceUriBuilder, IPackageService packageService,
            IUserkeyService userKeyService) {
            _serviceUriBuilder = serviceUriBuilder;
            _packageService = packageService;
            _orchardServices = orchardServices;
            _userKeyService = userKeyService;

            T = NullLocalizer.Instance;

            _serviceRoot = new Lazy<string>(() =>_orchardServices.WorkContext.CurrentSite.As<GallerySettingsPart>().ServiceRoot);
        }

        public Package CreatePackage(Func<HttpClient, HttpResponseMessage> createPackagePostMethod) {
            string serviceRoot = _orchardServices.WorkContext.CurrentSite.As<GallerySettingsPart>().ServiceRoot;
            using (var client = new HttpClient(serviceRoot)) {
                using (HttpResponseMessage response = createPackagePostMethod(client)) {
                    if (response.StatusCode != HttpStatusCode.OK) {
                        _orchardServices.Notifier.Error(T(response.ReadContentAsStringWithoutQuotes()));
                        return null;
                    }
                    return response.Content.ReadAsJsonDataContract<Package>();
                }
            }
        }

        public Package GetPackage(string packageId, string packageVersion) {
            using (var client = new HttpClient(_serviceRoot.Value)) {
                string uri = _serviceUriBuilder.BuildServiceUri(ServiceConstants.PackageServiceName, packageId, packageVersion);
                using (HttpResponseMessage response = client.Get(uri)) {
                    if (response.StatusCode != HttpStatusCode.OK) {
                        _orchardServices.Notifier.Error(T(response.ReadContentAsStringWithoutQuotes()));
                        return null;
                    }
                    return response.Content.ReadAsJsonDataContract<Package>();
                }
            }
        }

        public void UpdatePackage(Package packageToUpdate) {
            using (var client = new HttpClient(_serviceRoot.Value)) {
                HttpContent content = HttpContentExtensions.CreateDataContract(packageToUpdate);
                string uri = _serviceUriBuilder.BuildServiceUri(ServiceConstants.PackageServiceName, packageToUpdate.Id,
                    packageToUpdate.Version);
                using (HttpResponseMessage response = client.Put(uri, content))
                {
                    if (response.StatusCode != HttpStatusCode.OK) {
                        _orchardServices.Notifier.Error(T(response.ReadContentAsStringWithoutQuotes()));
                    }
                }
            }
        }

        public IEnumerable<Package> GetUnfinishedPackages(IEnumerable<string> packageIDs, Guid accessKey) {
            using (var client = new HttpClient(_serviceRoot.Value)) {
                string uri = string.Format("{0}/{1}", ServiceConstants.PackageServiceName, accessKey);
                HttpContent content = HttpContentExtensions.CreateDataContract(packageIDs);
                using (HttpResponseMessage response = client.Post(uri, content)) {
                    if (response.StatusCode != HttpStatusCode.OK) {
                        _orchardServices.Notifier.Error(T(response.ReadContentAsStringWithoutQuotes()));
                        return new List<Package>();
                    }
                    return response.Content.ReadAsDataContract<IEnumerable<Package>>();
                }
            }
        }

        public IEnumerable<PackageLogEntry> GetNewPackageLogs(int lastPackageLogId) {
            using (var client = new HttpClient(_serviceRoot.Value)) {
                string logUri = String.Format("{0}/{1}", ServiceConstants.PackageLogService, lastPackageLogId);
                using (HttpResponseMessage response = client.Get(logUri)) {
                    if (response.StatusCode == HttpStatusCode.OK) {
                        return response.Content.ReadAsJsonDataContract<List<PackageLogEntry>>();
                    }
                    return new List<PackageLogEntry>();
                }
            }
        }

        public void DeletePackage(string packageId, string packageVersion) {
            string deleteUri = _serviceUriBuilder.BuildServiceUri(ServiceConstants.PackageServiceName, packageId, packageVersion);
            DeletePackage(packageId, packageVersion, deleteUri);
        }

        public void DeleteUnfinishedPackages(string packageId) {
            Guid adminUserKey = _userKeyService.GetSuperUserKey().AccessKey;
            IEnumerable<Package> unfinishedPackages = GetUnfinishedPackages(new[] { packageId }, adminUserKey);
            foreach (var unfinishedPackage in unfinishedPackages) {
                string deleteUri = _serviceUriBuilder.BuildServiceUri(ServiceConstants.PackageServiceName, packageId, unfinishedPackage.Version, adminUserKey);
                DeletePackage(packageId, unfinishedPackage.Version, deleteUri);
            }
        }

        private void DeletePackage(string packageId, string packageVersion, string deleteUri) {
            string serviceRoot = _orchardServices.WorkContext.CurrentSite.As<GallerySettingsPart>().ServiceRoot;
            using (var client = new HttpClient(serviceRoot)) {
                using (HttpResponseMessage response = client.Delete(deleteUri)) {
                    if (response.StatusCode != HttpStatusCode.OK) {
                        _orchardServices.Notifier.Error(T(response.ReadContentAsStringWithoutQuotes()));
                    } else {
                        PackagePart partToDelete = _packageService.Get(p => p.PackageID == packageId && p.PackageVersion == packageVersion, true)
                            .SingleOrDefault();
                        if (partToDelete != null) {
                            _packageService.Delete(packageId, packageVersion);
                            _packageService.ResetRecommendedVersionForPackage(partToDelete);
                        }
                    }
                }
            }
            _orchardServices.Notifier.Information(T(string.Format("Package {0}, version {1}, has been deleted.", packageId, packageVersion)));
        }
    }
}