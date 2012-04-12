using System;
using System.Diagnostics;
using System.Threading;

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
            var client = new RfidWebClient(conf);
            client.Auth();
            if ((int)client.StatusCode == 403)
            {
                Console.WriteLine("Auth Status: Fail.");
                Console.ReadKey();
                return;
            }

            for (int i = 0; i < 10; i++)
            {
                var session = new RfidSession();
                var reader = new RfidTagReader();
                reader.GetTags(0, 30, ref session.data, out session.time_stamp);
                session.coords = "60 40";

                //foreach (string currentTag in session.data)
                //{
                //    Console.WriteLine("{0}", currentTag);
                //}
                if (session.data.Count != 0)
                {
                    client.SendRfidReport(session);
                }

                else
                {
                    Console.WriteLine("Zero.");
                }

                if ((int)client.StatusCode == 200)
                {
                    Console.WriteLine("Report: Success! {0}", client.ResponseMsg);
                }
                else
                {
                    Console.WriteLine("Error: {0}", client.ResponseMsg);
                }
            }

            Console.ReadKey();
            Thread.Sleep(500);
        }
    }
}
