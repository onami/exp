using System;
using System.Xml.Serialization;
using System.IO;

namespace DL6970
{
    public class Configuration
    {
        public string Server;
        public string DeviceKey;
        public string Location;

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
                var conf = new Configuration();
                conf.Serialize();
                return null;
            }
        }
    }
}