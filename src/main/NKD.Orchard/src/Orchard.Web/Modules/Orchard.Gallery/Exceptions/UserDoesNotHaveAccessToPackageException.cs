using System;

namespace Orchard.Gallery.Exceptions {
    public class UserDoesNotHaveAccessToPackageException : Exception {
        public UserDoesNotHaveAccessToPackageException(int userId, string packageId)
            : base(string.Format("User with ID {0} does not have access to package '{1}'.", userId, packageId))
        { }
    }
}