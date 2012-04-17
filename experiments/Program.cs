using System;
using System.Diagnostics;
using System.Threading;

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

            var session = new RfidSession() { location = "Abitech Ltd." };
            for (int i = 0; i < 2; i++)
            {
                reader.GetTags(0, 10, ref session.tags, out session.time);
                Console.WriteLine("Count: {0}", session.tags.Count);
                Thread.Sleep(50);
            }
            collector.Write(session);

            session = new RfidSession() { location = "Moon" };
            for (int i = 0; i < 2; i++)
            {
                reader.GetTags(0, 10, ref session.tags, out session.time);
                Console.WriteLine("Count: {0}", session.tags.Count);
                Thread.Sleep(50);
            }
            collector.Write(session);
            //for (int i = 0; i < 499; i++)
            //{
            //    collector.write("BBBB");
            //}

            watch.Stop();
            collector.Close();
            Console.WriteLine("Time elapsed: {0}", watch.Elapsed);
            Console.ReadKey();
        }
    }
}
