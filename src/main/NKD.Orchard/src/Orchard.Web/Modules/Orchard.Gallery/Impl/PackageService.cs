using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;
using Orchard.Indexing;
using Orchard.Tasks.Indexing;
using Orchard.Autoroute.Models;

namespace Orchard.Gallery.Impl {
    [UsedImplicitly]
    public class PackageService : IPackageService {
        private readonly IContentManager _contentManager;
        private readonly IPackageMediaDirectoryHelper _packageMediaDirectoryHelper;
        private readonly IIndexingTaskManager _indexingTaskManager;
        private readonly IIndexNotifierHandler _indexNotifierHandler;

        public PackageService(IContentManager contentManager, IPackageMediaDirectoryHelper packageMediaDirectoryHelper,
            IIndexingTaskManager indexingTaskManager, IIndexNotifierHandler indexNotifierHandler) {
            _contentManager = contentManager;
            _indexNotifierHandler = indexNotifierHandler;
            _indexingTaskManager = indexingTaskManager;
            _packageMediaDirectoryHelper = packageMediaDirectoryHelper;
        }

        public PackagePart Get(string slug) {
            //TODO:CHECK
            return _contentManager.Query<PackagePart, PackagePartRecord>()
                .Join<AutoroutePartRecord>().Where(rr => rr.DisplayAlias == slug)
                .Slice(1).FirstOrDefault();
        }

        public PackagePart Get(string packageId, string packageVersion, bool includeAllVersions) {
            IContentQuery<PackagePart, PackagePartRecord> contentQuery = includeAllVersions
                ? _contentManager.Query<PackagePart, PackagePartRecord>(VersionOptions.AllVersions)
                : _contentManager.Query<PackagePart, PackagePartRecord>();
            return contentQuery
                .Where(pp => pp.PackageID == packageId && pp.PackageVersion == packageVersion)
                .Slice(1).FirstOrDefault();
        }

        public IEnumerable<PackagePart> Get(Expression<Func<PackagePartRecord, bool>> filter, bool includeAllVersions) {
            if (filter == null) {
                filter = p => true;
            }
            IContentQuery<PackagePart, PackagePartRecord> contentQuery = includeAllVersions
                ? _contentManager.Query<PackagePart, PackagePartRecord>(VersionOptions.AllVersions)
                : _contentManager.Query<PackagePart, PackagePartRecord>();
            return contentQuery.Where(filter).List();
        }

        public IEnumerable<PackagePart> Get(int startingIndex, int pageSize, Expression<Func<PackagePartRecord,bool>> filter) {
            if (filter == null) {
                filter = p => true;
            }
            return _contentManager.Query<PackagePart, PackagePartRecord>().Where(filter).Slice(startingIndex, pageSize);
        }

        public IEnumerable<PackagePart> GetById(string packageId, bool includeAllVersions) {
            IContentQuery<PackagePart, PackagePartRecord> contentQuery = includeAllVersions
                ? _contentManager.Query<PackagePart, PackagePartRecord>(VersionOptions.AllVersions)
                : _contentManager.Query<PackagePart, PackagePartRecord>();
            return contentQuery.Where(pp => pp.PackageID == packageId).List();
        }

        public void Delete(string packageId, string packageVersion) {
            string packageScreenshotDirectory = _packageMediaDirectoryHelper.GetAbsolutePathToPackageMediaDirectory(packageId, packageVersion);
            if (Directory.Exists(packageScreenshotDirectory)) {
                try {
                    Directory.Delete(packageScreenshotDirectory, true);
                }
                catch (IOException) { }
            }
            PackagePart package = Get(packageId, packageVersion, true);
            if (package != null) {
                _contentManager.Remove(package.ContentItem);
            }
        }

        public int CountOfPackages(Expression<Func<PackagePartRecord, bool>> filter, bool includeAllVersions) {
            if (filter == null) {
                filter = p => true;
            }
            IContentQuery<PackagePart, PackagePartRecord> contentQuery = includeAllVersions ?
                _contentManager.Query<PackagePart, PackagePartRecord>(VersionOptions.AllVersions) :
                _contentManager.Query<PackagePart, PackagePartRecord>();
            return contentQuery.Where(filter).Count();
        }

        public bool PackageIdExists(string packageId) {
            return _contentManager.Query<PackagePart, PackagePartRecord>().Where(p => p.PackageID == packageId).Count() > 0;
        }

        public bool PackageExists(string packageId, string packageVersion, VersionOptions versionOptions) {
            var contentQuery = versionOptions != null ? _contentManager.Query<PackagePart, PackagePartRecord>(versionOptions)
                : _contentManager.Query<PackagePart, PackagePartRecord>();
            return contentQuery.Where(p => p.PackageID == packageId && p.PackageVersion == packageVersion).List().Any();
        }

        public void ResetRecommendedVersionForPackage(PackagePart packagePart) {
            if (!packagePart.IsRecommendedVersion) {
                return;
            }
            packagePart.IsRecommendedVersion = false;
            IEnumerable<PackagePart> packageParts = GetById(packagePart.PackageID, false);
            if (!packageParts.Any()) {
                return;
            }
            var highestVersion = packageParts.Select(p => new { Package = p, Version = new Version(p.PackageVersion) })
                .OrderByDescending(v => v.Version).First().Package;
            highestVersion.IsRecommendedVersion = true;
            _indexingTaskManager.CreateUpdateIndexTask(highestVersion.ContentItem);
            _indexNotifierHandler.UpdateIndex("Search");
        }
    }
}