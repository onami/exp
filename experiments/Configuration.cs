using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace experiments
{
    public class Configuration
    {
        public string server = "http://rfidserver";
        [XmlAttribute]
        public bool isRegistered = false;
        public string login;
        public string pass;

        void generateDeviceKey()
        {

        }

        void serialize(string path = "config.xml")
        {
            var serializer = new XmlSerializer(typeof(Configuration));
            TextWriter writer = new StreamWriter(path);
            serializer.Serialize(writer, this);
            writer.Close();
        }

        public static Configuration deserialize(string path = "config.xml")
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Configuration));
                FileStream fs = new FileStream(path, FileMode.Open);
                return (Configuration)serializer.Deserialize(fs);
            }

            catch (FileNotFoundException e)
            {
                return null;
            }
        }
    }
}