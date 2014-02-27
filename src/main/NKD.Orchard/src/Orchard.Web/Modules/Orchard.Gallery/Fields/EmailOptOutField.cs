using Orchard.ContentManagement;

namespace Orchard.Gallery.Fields {
    public class EmailOptOutField : ContentField {
        private const string FIELD_NAME = "UserOptsOutOfEmails";

        public bool OptOut {
            get { return Storage.Get<bool>(FIELD_NAME); }
            set { Storage.Set(FIELD_NAME, value);}
        }
    }
}