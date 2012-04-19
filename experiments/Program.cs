using System;
using System.Diagnostics;
using System.Threading;

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

            //for (int j = 0; j < 10; j++)
            //{
            //    var session = new RfidSession();
            //    for (int i = 0; i < 1; i++)
            //    {
            //        reader.GetTags(0, 10, ref session.tags, out session.time);
            //        Console.WriteLine("Count: {0}", session.tags.Count);
            //        Thread.Sleep(50);
            //    }
            //    collector.Write(session);
            //}

            var webClient = new RfidWebClient(Configuration.deserialize());
            webClient.Auth();
            Console.WriteLine("Auth Time: {0}", webClient.ResponseMsg);
            var sessions = collector.GetUnshippedTags();
            if (sessions.Count != 0)
            {
                webClient.SendRfidReports(sessions);
                collector.SetDeliveryStatus(sessions);
            }

            Console.WriteLine("End\t{0}\t", watch.Elapsed);
            collector.Close();
            Console.ReadKey();
        }
    }
}