using JetBrains.Annotations;
using Orchard.Tasks;

namespace Orchard.Gallery.ManagePackageIds {
    [UsedImplicitly]
    public class RegisteredPackageIdExpirerTask : IBackgroundTask {

        private readonly IExpiredPackageIdTaskRunner _expiredPackageIdTaskRunner;

        public RegisteredPackageIdExpirerTask(IExpiredPackageIdTaskRunner expiredPackageIdTaskRunner) {
            _expiredPackageIdTaskRunner = expiredPackageIdTaskRunner;
        }

        public void Sweep() {
            _expiredPackageIdTaskRunner.Synchronize();
        }
    }
}