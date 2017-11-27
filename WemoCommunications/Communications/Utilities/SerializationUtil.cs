using System.Xml.Linq;
using System.Xml.Serialization;

namespace Communications.Utilities
{
    public class SerializationUtil
    {
        public static T Deserialize<T>(XDocument doc)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            using (var reader = doc.Root.CreateReader())
            {
                return (T)xmlSerializer.Deserialize(reader);
            }
        }

        public static T Deserialize<T>(XElement element)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            using (var reader = element.CreateReader())
            {
                return (T)xmlSerializer.Deserialize(reader);
            }
        }

        public static XDocument Serialize<T>(T value)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            var doc = new XDocument();
            using (var writer = doc.CreateWriter())
            {
                xmlSerializer.Serialize(writer, value);
            }

            return doc;
        }
    }

}
