using System.Net;

namespace Proligence.SignalR.Samples.Chat.ContentProviders
{
    public interface IContentProvider
    {
        string GetContent(HttpWebResponse response);
    }
}