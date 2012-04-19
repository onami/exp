using System;
using System.Diagnostics;

namespace rfid
{
    class Program
    {
        static void Main()
        {
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            var watch = new Stopwatch();
            watch.Start();

            var collector = new RfidTagsCollector("rfid.db");
            var reader = new DL6970Reader();

            //var session = new RfidSession();
            //for (int i = 0; i < 2; i++)
            //{
            //    reader.GetTags(0, 10, ref session.tags, out session.time);
            //    Console.WriteLine("Count: {0}", session.tags.Count);
            //    Thread.Sleep(50);
            //}
            //collector.Write(session);

            var webClient = new RfidWebClient(Configuration.deserialize());
            var sessions = collector.GetUnshippedTags();
            if (sessions.Count != 0)
            {
                webClient.SendRfidReports(sessions);
                collector.SetDeliveryStatus(sessions);
            }

            watch.Stop();
            collector.Close();
            Console.WriteLine("Time elapsed: {0}", watch.Elapsed);
            Console.ReadKey();
        }
    }
}
