using System.Threading;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Orchard.Environment.Extensions;

namespace Proligence.SignalR.Samples.DrawingPad
{
    [OrchardFeature("Proligence.SignalR.Core.Samples")]
    [HubName("DrawingPad")]
    public class DrawingPad : Hub
    {
        public class Point
        {
            public int X { get; set; }
            public int Y { get; set; }
        }
        public class Line
        {
            public bool Remote { get; set; }
            public Point From { get; set; }
            public Point To { get; set; }
            public string Color { get; set; }
        }

        private static long _id;
        // defines some colors
        private readonly static string[] colors = new[]{
            "red", "green", "blue", "orange", "navy", "silver", "black", "lime"
        };

        public void Join()
        {
            Clients.Caller.color = colors[Interlocked.Increment(ref _id) % colors.Length];
        }

        // A user has drawed a line ...
        [HubMethodName("Draw")]
        public void Draw(Line data)
        {
            Clients.Others.draw(data);
        }
    }
}