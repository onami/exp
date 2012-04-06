using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Security.Cryptography;

namespace experiments
{
    public class Configuration
    {
        public string server = "http://rfidserver";
        [XmlAttribute]
        public bool isRegistered = false;
        public string login;
        public string pass;

        [NonSerialized]
        private readonly Random _rng = new Random();

        public string GenerateDeviceKey()
        {
           return GetRandomString(40); 
        }

        string GetRandomString(int size)
        {
            const string _chars = "abcdefghijklmnopqstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            char[] buffer = new char[size];

            for(int i = 0; i < size; i++)
            {
                buffer[i] = _chars[_rng.Next(_chars.Length)];
            }

            return new string(buffer);
        }

        public void serialize(string path = "config.xml")
        {
            var serializer = new XmlSerializer(typeof(Configuration));
            File.Delete(path);
            TextWriter writer = new StreamWriter(path);
            serializer.Serialize(writer, this);
            writer.Close();
        }

        public static Configuration deserialize(string path = "config.xml")
        {
            try
            {
                //TODO::using
                XmlSerializer serializer = new XmlSerializer(typeof(Configuration));
                FileStream fs = new FileStream(path, FileMode.Open);
                var conf = (Configuration)serializer.Deserialize(fs);
                fs.Close();
                return conf;
            }

            catch (FileNotFoundException e)
            {
                var conf = new Configuration();
                conf.login = conf.GenerateDeviceKey();
                conf.serialize();
                return null;
            }
        }
    }
}