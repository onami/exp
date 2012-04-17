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

            var collector = new RfidTagsCollector("rfidDb.sqlite");
            var session = new RfidSession();
            var reader = new DL770Reader();

            reader.GetTags(0, 10, ref session.tags, out session.time);
            collector.Write(session.tags, "location");

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
