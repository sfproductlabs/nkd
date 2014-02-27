using JetBrains.Annotations;
using Orchard.Tasks;

namespace Orchard.Gallery.RatingSynchronization {
    [UsedImplicitly]
    public class RatingSyncTask : IBackgroundTask {

        private readonly IRatingSynchronizer _ratingSynchronizer;

        public RatingSyncTask(IRatingSynchronizer ratingSynchronizer) {
            _ratingSynchronizer = ratingSynchronizer;
        }

        public void Sweep() {
            _ratingSynchronizer.Synchronize();
        }
    }
}