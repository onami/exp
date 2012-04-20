using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace rfid
{
    public class RfidSession
    {
        public enum DeliveryStatus : int { Unshipped = 0, Shipped = 1 };
        public enum ReadingStatus : int { Normal = 0, InterruptedByTimer = 1 };
        public enum ReadingMode : int { Reading = 0, Registration = 1, Restoration = 2, Recycling = 3 };

        [JsonIgnoreAttribute]
        public int id;
        public string time;
        public string location = String.Empty;
        [JsonIgnoreAttribute]
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
