using Microsoft.Http;

namespace Orchard.Gallery.Extensions {
    public static class HttpResponseMessageExtensions {
        public static string ReadContentAsStringWithoutQuotes(this HttpResponseMessage response) {
            return response.Content.ReadAsString().TrimStart('"').TrimEnd('"');
        }
    }
}