using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rfid
{
    public class RfidSession
    {
        public enum SessionStatus { Normal = 1, Interrupted = 2 };
        public string location;
        public string time;
        public SessionStatus status;
        public List<string> tags;

        public RfidSession(SessionStatus status = SessionStatus.Normal)
        {
            this.status = status;
            tags = new List<string>();
        }
    }
}
