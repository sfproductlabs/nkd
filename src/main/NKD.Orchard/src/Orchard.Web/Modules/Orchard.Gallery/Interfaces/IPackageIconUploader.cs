using System.Web;

namespace Orchard.Gallery.Interfaces {
    public interface IPackageIconUploader : IDependency {
        string UploadPackageIcon(HttpPostedFileBase iconFile, string packageId, string packageVersion);
    }
}