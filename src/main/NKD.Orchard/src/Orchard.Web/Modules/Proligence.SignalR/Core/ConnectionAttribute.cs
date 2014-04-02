using System;

namespace Proligence.SignalR.Core
{
    /// <summary>
    /// Allows connection name and URL customization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ConnectionAttribute : Attribute
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public ConnectionAttribute(string name, string url = "")
        {
            if(string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Connection name cannot be empty.", "name");

            Name = name;
            Url = url;
        }
    }
}