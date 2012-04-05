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
            //webpageReader = new StreamReader(webRequest.GetResponse().GetResponseStream());
            //webpageContent = webpageReader.ReadToEnd();
            //var client = new RfidClient("csharp", "f0c951d22714b96f8b53e895df8fe0a8f8afc6f4");
            //client.Signup();
            var conf = Configuration.deserialize();
            if (conf == null) Console.WriteLine("confix.xml hasn't been found");
            //conf.login = "csharp";
            //conf.pass = "f0c951d22714b96f8b53e895df8fe0a8f8afc6f4";
            //conf.isRegistered = false;



            //client.Auth();
            //Console.WriteLine("{0}", (int)client.StatusCode);
            Console.ReadKey();
        }
    }
}
