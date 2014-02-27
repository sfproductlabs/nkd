using System;
using System.Web.Script.Serialization;
using Microsoft.Http;

namespace Orchard.Gallery.Extensions
{
    public static class HttpClientHelpers
    {
        private const string CONTENT_TYPE = "application/json; charset=utf-8";
        private static readonly Lazy<JavaScriptSerializer> _serializer = new Lazy<JavaScriptSerializer>();

        public static HttpResponseMessage PostJson<T>(this HttpClient httpClient, string uri, T obj)
        {
            return httpClient.Post(uri, HttpContent.Create(_serializer.Value.Serialize(obj), CONTENT_TYPE));
        }
    }
}