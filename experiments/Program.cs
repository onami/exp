using System;
using System.Net;
using System.Threading;
using DL6970.Rfid;
using System.Collections;

namespace DL6970
{
    class Daemon
    {
        private readonly RfidTagsCollector collector;
        private readonly RfidWebClient webClient;
        private readonly DL6970Reader reader;

        public bool IsSending { get; private set; }

        public Daemon(RfidTagsCollector collector, RfidWebClient webClient, DL6970Reader reader)
        {
            this.collector = collector;
            this.webClient = webClient;
            this.reader = reader;
            try
            {
                webClient.Auth();
                Send(); ;
               // new Thread(this.Send).Start();
            }
            catch (WebException) { Console.WriteLine("Fail."); }
        }

        public void Send()
        {
            while (IsSending == true) { Thread.Sleep(100); }

            IsSending = true;

            var sessions = collector.GetUnshippedTags();
            if (sessions.Count == 0)
            {
                IsSending = false;
                return;
            }

            webClient.SendRfidReports(sessions);
            lock (collector)
            {
                collector.SetDeliveryStatus(sessions);
            }

            if (webClient.Status == RfidWebClient.ResponseStatus.Ok)
            {
                Console.WriteLine("Sent {0}", sessions.Count);

            }

            IsSending = false;
        }

        public void Read()
        {
            var session = new RfidSession { sessionMode = RfidSession.SessionMode.Reading };

            reader.GetTags(0, 20, ref session.tags, out session.time);

            Console.WriteLine("Count: {0}", session.tags.Count);

            lock (collector)
            {
                collector.Write(session);
            }

         //   new Thread(this.Send).Start();
            return;
        }
    }

    class Program
    {
        static void Main()
        {
            try
            {
                var collector = new RfidTagsCollector("rfid.db");
                var reader = new DL6970Reader();
                var webClient = new RfidWebClient(Configuration.Deserialize());
                var daemon = new Daemon(collector, webClient, reader);
                daemon.Read();
                daemon.Read();
                daemon.Read();
                daemon.Send();
                //new Thread(daemon.Read).Start();
                reader.CloseConnection();
            }
            catch (NullReferenceException) { }

            Console.ReadKey();
        }
    }
}