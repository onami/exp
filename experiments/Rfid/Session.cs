using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rfid
{
    public class RfidSession
    {
        public string coords;
        public string location;
        public string time;
        public List<string> tags;

        public RfidSession()
        {
            tags = new List<string>();
        }
    }
}
