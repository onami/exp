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
        public int id;
        public string time;
        public string location = String.Empty;
        public DeliveryStatus deliveryStatus;
        public ReadingStatus readingStatus;
        public List<string> tags;

        public RfidSession(DeliveryStatus deliveryStatus = DeliveryStatus.Unshipped, ReadingStatus readingStatus = ReadingStatus.Normal)
        {
            this.deliveryStatus = deliveryStatus;
            this.readingStatus = readingStatus;
            tags = new List<string>();
        }
    }
}
