using System.Collections.Generic;

namespace Orchard.Gallery.ViewModels {
    public class ContactOwnersViewModel {
        public string PackageId { get; set; }
        public string UrlReferrer { get; set; }
        public IEnumerable<string> Owners { get; set; }
        public bool IsEnabled { get; set; }
    }
}