using System.Collections.Generic;
using Gallery.Core.Domain;

namespace Orchard.Gallery.PackageSynchronization {
    public interface IPackageLogEntryService : IDependency {
        IEnumerable<PackageLogEntry> GetUnprocessedLogEntries();
    }
}