using System;
using System.Diagnostics;
using System.Threading;
using System.Net;

namespace rfid
{
    class Program
    {
        static void Main()
        {
            var watch = new Stopwatch();
            watch.Start();
            Console.WriteLine("Start\t{0}\t", watch.Elapsed);

            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            var collector = new RfidTagsCollector("rfid.db");
            var reader = new DL6970Reader();

            var session = new RfidSession() { readingMode = RfidSession.ReadingMode.Reading };

            while (true)
            {
                reader.GetTags(0, 20, ref session.tags, out session.time);
                Console.WriteLine("Count: {0}", session.tags.Count);
                Thread.Sleep(50);
                Console.WriteLine("Proceed? y/n");
                var info = Console.ReadKey();
                Console.WriteLine();
                if (info.KeyChar == 'n')
                    break;
            }

            collector.Write(session);

            var sessions = collector.GetUnshippedTags();
            if (sessions.Count != 0)
            {
                var webClient = new RfidWebClient(Configuration.deserialize());
                webClient.Auth();

                if (webClient.Status == RfidWebClient.ResponseStatus.OK)
                {
                    webClient.SendRfidReports(sessions);
                    collector.SetDeliveryStatus(sessions);
                }
            }

            Console.WriteLine("End\t{0}\t", watch.Elapsed);
            collector.Close();
            Console.ReadKey();
        }
    }
}