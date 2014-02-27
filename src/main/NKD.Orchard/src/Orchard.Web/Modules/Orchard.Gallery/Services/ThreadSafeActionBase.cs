using System.Threading;

namespace Orchard.Gallery.Services {
    public abstract class ThreadSafeActionBase {
        private static readonly object syncLock = new object();

        protected virtual void CouldNotEnterLockAction() {}
        protected abstract void ExecuteThreadSafeAction();
        protected virtual void ExitLockAction() {}

        public void Synchronize() {
            if (!Monitor.TryEnter(syncLock)) {
                CouldNotEnterLockAction();
                return;
            }
            try {
                ExecuteThreadSafeAction();
            }
            finally {
                Monitor.Exit(syncLock);
                ExitLockAction();
            }
        }
    }
}