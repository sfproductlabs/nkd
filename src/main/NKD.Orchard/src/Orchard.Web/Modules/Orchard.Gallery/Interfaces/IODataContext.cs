using System.Data.Services.Client;
using System.Linq;
using Orchard.Gallery.GalleryServer;

namespace Orchard.Gallery.Interfaces {
    public interface IODataContext : IDependency {
        IQueryable<PublishedPackage> Packages { get; }
        IQueryable<PublishedScreenshot> Screenshots { get; }
        string GetDownloadUrl(PublishedPackage package);
    }
}