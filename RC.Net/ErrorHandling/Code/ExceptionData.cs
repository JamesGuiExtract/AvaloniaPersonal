using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Extract.ErrorHandling
{
    [Serializable]
    public class ExceptionData : IDictionary, ISerializable
    {
        List<DictionaryEntry> entries = new List<DictionaryEntry>();

        private ExceptionData(SerializationInfo info, StreamingContext context)
        {
            entries = (List<DictionaryEntry>)info.GetValue("Entries", entries.GetType());
        }
        public ExceptionData()
        {
        }
        public object this[object key] {
            get => entries?.Where(e => e.Key.Equals(key))?.Select(e => e.Value)?.ToList();
            set => entries.Add(new DictionaryEntry(key, value)); 
        }

        public ICollection Keys => entries.Select(k => k.Key).Distinct().ToList();

        public ICollection Values => entries.GroupBy(k => k.Key).Select(g => g.Select(c => c.Value).ToList()).ToList();

        public int Count => entries.Count;

        public bool IsReadOnly => false;

        public bool IsFixedSize => false;

        public object SyncRoot => throw new NotImplementedException();

        public bool IsSynchronized => false;

        public void Add(object key, object value) => Add(new DictionaryEntry(key, value));

        public void Add(DictionaryEntry item) => entries.Add(item);

        public void Clear() => entries.Clear();

        public bool Contains(object key) => entries.Any(entry => entry.Key == key);

        public void CopyTo(DictionaryEntry[] array, int arrayIndex) 
            => entries.CopyTo(array, arrayIndex);

        public void Remove(object key) => entries.RemoveAll(item => item.Key == key);

        public bool Remove(KeyValuePair<object, object> item)
        {
            return 0 != entries.RemoveAll(entry => entry.Key == item.Key && entry.Value == item.Value);
        }

        private class ExceptionDataEnumerator : IDictionaryEnumerator
        {
            DictionaryEntry[] entries;
            int index = -1;

            public ExceptionDataEnumerator(ExceptionData data)
            {
                entries = new DictionaryEntry[data.Count];
                data.CopyTo(entries, 0);
            }

            public object Key { get { ValidateIndex(); return entries[index].Key; } }

            public object Value { get { ValidateIndex(); return entries[index].Value; } }

            public DictionaryEntry Entry { get { ValidateIndex(); return entries[index]; } }

            public object Current => Entry;

            public bool MoveNext()
            {
                if (index < entries.Length - 1) 
                { 
                    index++; 
                    return true; 
                }
                return false;
            }

            public void Reset()
            {
                index = -1;
            }

            private void ValidateIndex()
            {
                if (index < 0 || index >= entries.Length)
                    throw new InvalidOperationException("Enumerator is before or after the collection.");
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ExceptionDataEnumerator(this);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new ExceptionDataEnumerator(this);
        }

        public void CopyTo(Array array, int index) => throw new NotImplementedException();

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Entries", entries);
        }

        // use this for converting to bytestream or json format
        public IList<DictionaryEntry> GetFlattenedData()
        {
            return entries
                .Select(v => new DictionaryEntry(v.Key, v.Value))
                .ToList();
        }

        public class ExceptionDataJsonConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(ExceptionData);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var v = existingValue as ExceptionData;
                if (existingValue == null)
                {
                    return new ExceptionData()
                    {
                        entries = serializer.Deserialize<List<DictionaryEntry>>(reader)
                    };
                }
                v.entries = serializer.Deserialize<List<DictionaryEntry>>(reader);
                return existingValue;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if (value == null)
                {
                    serializer.Serialize(writer, null);
                    return;
                }

                var v = value as ExceptionData;

                serializer.Serialize(writer, v.entries);
            }
        }
    }

}
