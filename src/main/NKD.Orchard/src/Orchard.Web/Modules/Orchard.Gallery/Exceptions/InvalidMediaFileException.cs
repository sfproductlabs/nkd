using System;

namespace Orchard.Gallery.Exceptions {
    public class InvalidMediaFileException : Exception {
        public InvalidMediaFileException(string message)
            : base(message) { }
    }
}