using JetBrains.Annotations;

namespace Orchard.Gallery.RatingSynchronization {
    [UsedImplicitly]
    public class NonceCache : INonceCache {
        private static string _nonce;

        public string Nonce { get { return _nonce; } set { _nonce = value; } }
    }
}