using System;
using System.Data.Services.Client;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Gallery.GalleryServer;
using Orchard.Gallery.Interfaces;
using Orchard.Gallery.Models;
using Orchard.Settings;
using Orchard.ContentManagement;

namespace Orchard.Gallery.Impl {
    [UsedImplicitly]
    public class ODataContext : IODataContext {
        private readonly Lazy<GalleryFeedContext> _context;

        public ODataContext(ISiteService siteService) {
            _context = new Lazy<GalleryFeedContext>(() => new GalleryFeedContext(new Uri(siteService.GetSiteSettings().As<GallerySettingsPart>().FeedUrl)) {
                IgnoreResourceNotFoundException = true
            });
        }

        public IQueryable<PublishedPackage> Packages { get { return _context.Value.Packages; } }
        public IQueryable<PublishedScreenshot> Screenshots { get { return _context.Value.Screenshots; } }

        public string GetDownloadUrl(PublishedPackage package) {
            return _context.Value.GetReadStreamUri(package).ToString();
        }
    }
}