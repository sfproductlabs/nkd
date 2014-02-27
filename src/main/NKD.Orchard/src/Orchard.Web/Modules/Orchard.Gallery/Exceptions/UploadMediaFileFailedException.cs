using System;

namespace Orchard.Gallery.Exceptions {
    public class UploadMediaFileFailedException : Exception {
        public UploadMediaFileFailedException(string fileName)
            : base(string.Format("The file '{0}' could not be uploaded to the Media folder.", fileName))
        { }
    }
}