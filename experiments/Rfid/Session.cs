using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rfid
{
    public class RfidSession
    {
        public enum DeliveryStatus : int { Unshipped = 0, Shipped = 1 };
        public enum ReadingStatus : int { Normal = 1, InterruptedByTimer = 2 };
        public enum ReadingMode : int { Reading = 1, Registration = 2, Restoration = 3, Recycling = 4 };

        [NonSerialized]
        public int id;
        public string time;
        public string location = String.Empty;
        [NonSerialized]
        public DeliveryStatus deliveryStatus = DeliveryStatus.Unshipped;
        public ReadingStatus readingStatus = ReadingStatus.Normal;
        public ReadingMode readingMode = ReadingMode.Reading;
        public List<string> tags;

        public RfidSession()
        {
            tags = new List<string>();
        }
    }
}
