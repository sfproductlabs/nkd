using System;

namespace Orchard.Gallery.Exceptions {
    public class AccessKeyDoesNotExistException : Exception {
        public AccessKeyDoesNotExistException(string accessKey)
            : base(string.Format("The access key '{0}' does not exist.", accessKey))
        { }
    }
}