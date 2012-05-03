using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DL6970.Rfid
{
    public class RfidSession
    {
        public enum DeliveryStatus { Unshipped = 0, Shipped = 1 };
        public enum ReadingStatus { Normal = 0, InterruptedByTimer = 1 };
        public enum SessionMode { Reading = 0, Registration = 1, Restoration = 2, Recycling = 3 };

        [JsonIgnore]
        public int id;
        public string time;
        public string location = String.Empty;
        [JsonIgnore]
        public DeliveryStatus deliveryStatus = DeliveryStatus.Unshipped;
        public ReadingStatus readingStatus = ReadingStatus.Normal;
        public SessionMode sessionMode = SessionMode.Reading;
        public List<string> tags;

        public RfidSession()
        {
            tags = new List<string>();
        }
    }
}
