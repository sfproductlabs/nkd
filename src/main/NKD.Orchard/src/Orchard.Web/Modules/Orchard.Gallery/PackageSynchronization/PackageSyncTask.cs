using JetBrains.Annotations;
using Orchard.Tasks;

namespace Orchard.Gallery.PackageSynchronization {
    [UsedImplicitly]
    public class PackageSyncTask : IBackgroundTask {

        private readonly IPackageSynchronizer _packageSynchronizer;

        public PackageSyncTask(IPackageSynchronizer packageSynchronizer) {
            _packageSynchronizer = packageSynchronizer;
        }

        public void Sweep() {
            _packageSynchronizer.Synchronize();
        }
    }
}