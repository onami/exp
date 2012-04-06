using System;
using System.Data.SQLite;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace experiments
{
    class Program
    {
        public void Test()
        {
            var collector = new DataCollector.DataCollector(500, "rfidTags", "rfidDb.sqlite");
            var watch = new Stopwatch();

            watch.Start();
            for (int i = 0; i < 499; i++)
            {
                collector.write("BBBB");
            }
            collector.close();
            watch.Stop();

            Console.WriteLine("Time elapsed: {0}", watch.Elapsed);
        }
        
        static void Main()
        {    
            var conf = Configuration.deserialize();
            if (conf == null)
            {
                Console.WriteLine("the configuration hasn't been found");
                return;
            }
            var client = new RfidClient(conf);
            client.Auth();
            //Console.WriteLine("{0}", (int)client.StatusCode);
            Console.ReadKey();
        }
    }
}
