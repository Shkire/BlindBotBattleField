using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicClasses.Common
{
    [Serializable]
    public class ServerResponseInfo<T>
    {
        public T info { get; set; }
    }

    [Serializable]
    public class ServerResponseInfo<T, E> : ServerResponseInfo<T>
    {
        public E exception { get; set; }
    }

    [Serializable]
    public class ServerResponseInfo<T, E, TT> : ServerResponseInfo<T, E>
    {
        public TT additionalInfo { get; set; }
    }

    [Serializable]
    public class DictionaryEntry<K, V>
    {
        public K key { get; set; }
        public V value { get; set; }

        public DictionaryEntry()
        { }

        public DictionaryEntry(K i_key, V i_value)
        {
            key = i_key;
            value = i_value;
        }
    }
}
