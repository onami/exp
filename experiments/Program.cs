using System;
using System.Net;
using System.Threading;
using DL6970.Rfid;
using System.Collections;

namespace DL6970
{
    class Daemon
    {
        private readonly RfidTagsCollector collector = new RfidTagsCollector("rfid.db");
        private readonly RfidWebClient webClient = new RfidWebClient(Configuration.Deserialize());
        private readonly DL6970Reader reader = new DL6970Reader();

        public bool IsSending { get; private set; }

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

            return;
        }

        public void Shutdown()
        {
            reader.CloseConnection();
        }
    }

    class Program
    {
        static void Main()
        {
            try
            {                
                var daemon = new Daemon();
               // daemon.Read();
                daemon.Send();
                daemon.Shutdown();

                //new Thread(daemon.Read).Start();

            }
            catch (NullReferenceException) { }

            Console.ReadKey();
        }
    }
}