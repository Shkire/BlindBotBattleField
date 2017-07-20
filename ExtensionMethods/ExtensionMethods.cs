using BasicClasses.Common;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace ExtensionMethods
{
    public static class ExtensionMethods
    {
        public static string SerializeObject<T>(this T toSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());

            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, toSerialize);
                return textWriter.ToString();
            }
        }

        public static T DeserializeObject<T>(this string toDeserialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

            using (TextReader reader = new StringReader(toDeserialize))
            {
                return (T)xmlSerializer.Deserialize(reader);
            }
        }

        public static List<DictionaryEntry<K, V>> ToSerializable<K,V>(this Dictionary<K, V> original)
        {
            List<DictionaryEntry<K, V>> res = new List<DictionaryEntry<K, V>>();
            foreach (K key in original.Keys)
            {
                res.Add(new DictionaryEntry<K, V>(key, original[key]));
            }
            return res;
        }

        public static Dictionary<K, V> ToDictionary<K, V>(this List<DictionaryEntry<K, V>> original)
        {
            Dictionary<K, V> res = new Dictionary<K, V>();
            foreach (DictionaryEntry<K, V> entry in original)
            {
                res.Add(entry.key, entry.value);
            }
            return res;
        }
    }
}
