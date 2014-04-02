using System.Threading;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Orchard;
using Orchard.Environment.Extensions;

namespace Proligence.SignalR.Samples.DrawingPad
{
    [OrchardFeature("Proligence.SignalR.Core.Samples")]
    [HubName("DrawingPad")]
    public class DrawingPad : Hub
    {
        private readonly IOrchardServices services;

        public DrawingPad(IOrchardServices services)
        {
            this.services = services;
        }

        public class Point
        {
            public double X { get; set; }
            public double Y { get; set; }
        }

        public class Line
        {
            public Point From { get; set; }
            public Point To { get; set; }
            public string Color { get; set; }
        }

        private static long _id;
        // defines some colors
        private readonly static string[] colors = new string[]{
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
            // ... propagate it to all users
            Clients.Others.draw(data);
        }
    }
}