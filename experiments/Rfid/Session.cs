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
        [CLSCompliant(false)]
        public DeliveryStatus deliveryStatus = DeliveryStatus.Unshipped;
        [CLSCompliant(false)]
        public ReadingStatus readingStatus = ReadingStatus.Normal;
        [CLSCompliant(false)]
        public SessionMode sessionMode = SessionMode.Reading;
        public List<string> tags;


        public RfidSession()
        {
            tags = new List<string>();
        }
    }
}
