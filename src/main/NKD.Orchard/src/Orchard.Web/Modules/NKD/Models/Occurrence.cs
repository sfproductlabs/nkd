using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NKD.Helpers;

namespace NKD.Models
{
    public class Occurrence
    {
        [Flags]
        public enum StatusCode : uint
        {
            OK = 0,
            Unknown = 1 << 0,
            Error = 1 << 1,
            Notifying = 1 << 2,
            Notified = 1 << 3,
            Failed = 0xFFFFFFFF
        }

        public Occurrence() { }

        private Guid id;
        public Guid ID
        {
            get { return id; }
            set { id = value; }
        }

        private Guid contactID;
        public Guid ContactID
        {
            get { return contactID; }
            set { contactID = value; }
        }


        private string note;
        /// <summary>
        /// Be careful of this (not many chars)
        /// </summary>
        public string Note 
        {
            get { return note; }
            set { note = value; }
        }


        private uint status = (uint)StatusCode.Unknown;
        public uint Status
        {
            get { return status; }
            set { status = value; }
        }

        public string StatusString
        {
            get { return EnumHelper.EnumToString((StatusCode)Status); }
        }
        
        private DateTime occurred;

        public DateTime Occurred
        {
            get { return occurred; }
            set { occurred = value; }
        }

     
    }
}