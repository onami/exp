using System;
using System.Xml.Serialization;
using System.IO;

namespace DL6970
{
    public class Configuration
    {
        public string Server = "http://rfidserver";
        [XmlAttribute]
        public bool IsRegistered;
        public string Login;
        public string Pass;
        public string Location;

        [NonSerialized]
        private readonly Random _rng = new Random();

        public string GenerateDeviceKey()
        {
           return GetRandomString(40); 
        }

        string GetRandomString(int size)
        {
            const string chars = "abcdefghijklmnopqstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var buffer = new char[size];

            for(var i = 0; i < size; i++)
            {
                buffer[i] = chars[_rng.Next(chars.Length)];
            }

            return new string(buffer);
        }

        public void Serialize(string path = "config.xml")
        {
            var serializer = new XmlSerializer(typeof(Configuration));
            File.Delete(path);
            TextWriter writer = new StreamWriter(path);
            serializer.Serialize(writer, this);
            writer.Close();
        }

        public static Configuration Deserialize(string path = "config.xml")
        {
            try
            {
                var serializer = new XmlSerializer(typeof(Configuration));
                var fs = new FileStream(path, FileMode.Open);
                var conf = (Configuration)serializer.Deserialize(fs);
                fs.Close();
                return conf;
            }

            catch
            {
                //var conf = new Configuration();
                //conf.Login = conf.GenerateDeviceKey();
                //conf.Serialize();
                return null;
            }
        }
    }
}