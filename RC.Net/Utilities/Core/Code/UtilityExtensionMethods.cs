using Extract.Licensing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using UCLID_COMUTILSLib;

namespace Extract.Utilities
{
    /// <summary>
    /// Helper class containing extension methods.
    /// </summary>
    public static class UtilityExtensionMethods
    {
        #region Constants

        /// <summary>
        /// Name of object used in license validation calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(UtilityExtensionMethods).ToString();

        /// <summary>
        /// The default buffer size to use when reading from streams.
        /// </summary>
        const int _DEFAULT_BUFFER = 8192;

        #endregion Constants

        #region Methods

        /// <summary>
        /// Converts a <see langword="string"/> to a <see langword="bool"/> where "0" and "1" are
        /// recognized as well as "true" and "false".
        /// </summary>
        /// <param name="value">The <see langword="string"/> to be converted.</param>
        /// <returns>The <see langword="bool"/> equivalent.</returns>
        public static bool ToBoolean(this string value)
        {
            try
            {
                if (value == "1")
                {
                    return true;
                }
                else if (value == "0")
                {
                    return false;
                }

                return bool.Parse(value);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36996");
            }
        }

        /// <summary>
        /// Converts to type.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static object ConvertToType(this string value, Type type)
        {
            try
            {
                if (type == typeof(bool))
                {
                    return value.ToBoolean();
                }
                else
                {
                    var converter = TypeDescriptor.GetConverter(type);
                    return converter.ConvertFromString(value);
                }
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI36923");
                ee.AddDebugData("Type", type.Name, false);
                ee.AddDebugData("Value", value, false);
                throw ee;
            }
        }

        /// <summary>
        /// Gets the specified <see paramref="propertyName"/> value of <see paramref="targetObject"/>.
        /// </summary>
        /// <param name="targetObject">The object for which the property value is sought</param>
        /// <param name="propertyName">The name of the property value to retrieve. Multiple property
        /// references may be chained by separating with a period. E.g., if the targetObject is a
        /// DataGridViewRow, an elementName of "DataGridView.VerticalScrollBar" could be used to refer
        /// to the scroll bar for the grid.</param>
        public static object GetProperty(this object targetObject, string propertyName)
        {
            try
            {
                PropertyInfo property = ResolveProperty(ref targetObject, propertyName);
                return property.GetValue(targetObject);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI50296");
            }
        }

        /// <summary>
        /// Sets the specified <see paramref="propertyName"/> value of <see paramref="targetObject"/>.
        /// </summary>
        /// <param name="targetObject">The object for which the property value is sought</param>
        /// <param name="propertyName">The name of the property value to retrieve. Multiple property
        /// references may be chained by separating with a period. E.g., if the targetObject is a
        /// DataGridViewRow, an elementName of "DataGridView.VerticalScrollBar" could be used to refer
        /// to the scroll bar for the grid.</param>
        /// <param name="value">The string representation of the value to apply. Must be convertable
        /// to the type of the specified property.</param>
        public static void SetPropertyValue(this object targetObject, string propertyName, string value)
        {
            try
            {
                PropertyInfo property = ResolveProperty(ref targetObject, propertyName);
                property.SetValue(targetObject, value.ConvertToType(property.PropertyType));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI50311");
            }
        }

        /// <summary>
        /// Sets the specified <see paramref="propertyName"/> value of <see paramref="targetObject"/>.
        /// </summary>
        /// <param name="targetObject">The object for which the property value is sought</param>
        /// <param name="propertyName">The name of the property value to retrieve. Multiple property
        /// references may be chained by separating with a period. E.g., if the targetObject is a
        /// DataGridViewRow, an elementName of "DataGridView.VerticalScrollBar" could be used to refer
        /// to the scroll bar for the grid.</param>
        /// <param name="value">The  value to apply. Must be of the same type of the specified property.</param>
        public static void SetPropertyValue<T>(this object targetObject, string propertyName, T value)
        {
            try
            {
                PropertyInfo property = ResolveProperty(ref targetObject, propertyName);
                ExtractException.Assert("ELI50313", "Unexpected type", property.PropertyType == typeof(T));

                property.SetValue(targetObject, value);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI50312");
            }
        }

        /// <summary>
        /// Helper method for GetProperty/SetProperty. Resolves and returns a <see cref="PropertyInfo"/>
        /// representing the the specified <see cref="propertyName"/>.
        /// </summary>
        /// <param name="targetObject">The object for which the property is sought</param>
        /// <param name="propertyName">The name of the property value to retrieve. Multiple property
        /// references may be chained by separating with a period. In this case targetObject is updated
        /// to represent each successive object referenced until it represents the object to which the
        /// property directly pertains.
        static PropertyInfo ResolveProperty(ref object targetObject, string propertyName)
        {
            PropertyInfo property = null;
            foreach (string currentProperty in propertyName.Split('.'))
            {
                if (property != null)
                {
                    targetObject = property.GetValue(targetObject, null);
                }

                property = targetObject.GetType().GetProperty(currentProperty);
            }

            return property;
        }

        /// <summary>
        /// Creates a memoized version of a unary function
        /// </summary>
        /// <typeparam name="T1">The type of the function parameter</typeparam>
        /// <typeparam name="TResult">The return type of the function</typeparam>
        /// <param name="fun">The non-memoized unary function</param>
        /// <returns>A memoized version of the function</returns>
        public static Func<T1, TResult> Memoize<T1, TResult>(this Func<T1, TResult> fun, bool threadSafe = false)
        {
            try
            {
                if (threadSafe)
                {
                    var map = new ConcurrentDictionary<T1, TResult>();
                    return a => map.GetOrAdd(a, fun);
                }
                else
                {
                    var map = new Dictionary<T1, TResult>();
                    return a => map.GetOrAdd(a, fun);
                }
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI38961");
                throw ee;
            }
        }

        /// <summary>
        /// Creates a memoized version of a binary function
        /// </summary>
        /// <typeparam name="T1">The type of the first function parameter</typeparam>
        /// <typeparam name="T2">The type of the second function parameter</typeparam>
        /// <typeparam name="TResult">The return type of the function</typeparam>
        /// <param name="fun">The non-memoized binary function</param>
        /// <returns>A memoized version of the function</returns>
        public static Func<T1, T2, TResult> Memoize<T1, T2, TResult>
            (this Func<T1, T2, TResult> fun)
        {
            try
            {
                var map = new Dictionary<Tuple<T1, T2>, TResult>();
                return (a, b) => map.GetOrAdd(Tuple.Create(a, b), _ => fun(a, b));
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI38962");
                throw ee;
            }
        }

        /// <summary>
        /// Creates a memoized version of a ternary function
        /// </summary>
        /// <typeparam name="T1">The type of the first function parameter</typeparam>
        /// <typeparam name="T2">The type of the second function parameter</typeparam>
        /// <typeparam name="T3">The type of the third function parameter</typeparam>
        /// <typeparam name="TResult">The return type of the function</typeparam>
        /// <param name="fun">The non-memoized function</param>
        /// <returns>A memoized version of the function</returns>
        public static Func<T1, T2, T3, TResult> Memoize<T1, T2, T3, TResult>
            (this Func<T1, T2, T3, TResult> fun)
        {
            try
            {
                var map = new Dictionary<Tuple<T1, T2, T3>, TResult>();
                return (a, b, c) => map.GetOrAdd(Tuple.Create(a, b, c), _ => fun(a, b, c));
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI44869");
                throw ee;
            }
        }

        /// <summary>
        /// Creates a memoized version of a quaternary function
        /// </summary>
        /// <typeparam name="T1">The type of the first function parameter</typeparam>
        /// <typeparam name="T2">The type of the second function parameter</typeparam>
        /// <typeparam name="T3">The type of the third function parameter</typeparam>
        /// <typeparam name="T4">The type of the fourth function parameter</typeparam>
        /// <typeparam name="TResult">The return type of the function</typeparam>
        /// <param name="fun">The non-memoized function</param>
        /// <returns>A memoized version of the function</returns>
        public static Func<T1, T2, T3, T4, TResult> Memoize<T1, T2, T3, T4, TResult>
            (this Func<T1, T2, T3, T4, TResult> fun)
        {
            try
            {
                var map = new Dictionary<Tuple<T1, T2, T3, T4>, TResult>();
                return (a, b, c, d) => map.GetOrAdd(Tuple.Create(a, b, c, d), _ => fun(a, b, c, d));
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI46994");
                throw ee;
            }
        }

        /// <summary>
        /// Method to deconstruct a <see cref="KeyValuePair{TKey, TValue}"/>.
        /// This enables more concise iteration over a <see cref="Dictionary{TKey, TValue}"/>,
        /// e.g., foreach(var (key, value) in dictionary)
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> tuple,
            out TKey key, out TValue value)
        {
            key = tuple.Key;
            value = tuple.Value;
        }

        /// <summary>
        /// Attempts to get a public instance <see cref="PropertyInfo">property</see> from a string
        /// </summary>
        /// <param name="type">The type of the class containing the property</typeparam>
        /// <param name="name">The name of the property</param>
        /// <param name="ignoreCase">Whether to ignore case differences between the given name and the property name</param>
        /// <param name="property">The <see cref="PropertyInfo"/> of the property</param>
        /// <returns><c>true</c> if the property was found</returns>
        public static bool TryGetProperty(this Type type, string name, bool ignoreCase, out PropertyInfo property)
        {
            try
            {
                var flags = BindingFlags.Public | BindingFlags.Instance | (ignoreCase ? BindingFlags.IgnoreCase : 0);
                property = type.GetProperty(name, flags);
                return property != null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45100");
            }
        }

        /// <summary>
        /// Return batches of an enumerable
        /// https://www.make-awesome.com/2010/08/batch-or-partition-a-collection-with-linq/
        /// </summary>
        /// <typeparam name="T">The Type contained in the IEnumerable</typeparam>
        /// <param name="collection">The collection to return batches from</param>
        /// <param name="batchSize">The Size of each batch - last batch will just be the remaining</param>
        /// <returns>The current batch as a List</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static IEnumerable<List<T>> Batch<T>(this IEnumerable<T> collection, int batchSize)
        {
            List<T> nextBatch = new List<T>(batchSize);
            foreach(T item in collection)
            {
                nextBatch.Add(item);
                if (nextBatch.Count == batchSize)
                {
                    yield return nextBatch;
                    nextBatch = new List<T>(batchSize);
                }
            }
            if (nextBatch.Count > 0)
            {
                yield return nextBatch;
            }
        }

        /// <summary>
        /// Adds the contents of one Dictionary to another
        /// </summary>
        /// <typeparam name="TKeyType">The Key type</typeparam>
        /// <typeparam name="TValueType">The Value type</typeparam>
        /// <param name="source">The Source dictionary that will be added to</param>
        /// <param name="collection">The dictionary that is being added to the source</param>
        public static void AddRange<TKeyType, TValueType>(
            this Dictionary<TKeyType, TValueType> source, 
            Dictionary<TKeyType, TValueType> collection)
        {
            try
            {
                if (collection is null)
                {
                    throw new ArgumentNullException(nameof(collection),"Collection is null");
                }
                foreach (var item in collection)
                {
                    if (!source.ContainsKey(item.Key))
                    {
                        source.Add(item.Key, item.Value);
                    }
                    else
                    {
                        ExtractException ee = new ExtractException("ELI47069", "Key exists in source.");
                        ee.AddDebugData("Key", item.Key.ToString(), false);
                        ee.AddDebugData("ValueToAdd", item.Value.ToString(), false);
                        throw ee;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47068");
            }
        }

        /// <summary>
        /// Converts an array of type to a HashSet of that type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static HashSet<T> ToHashSet<T>(this T[] source)
        {
            try
            {
                var returnValue = new HashSet<T>();
                foreach (var s in source)
                {
                    returnValue.Add(s);
                }
                return returnValue;
            }
            catch(Exception ex)
            {
                throw ex.AsExtract("ELI47092");
            }
        }

        /// <summary>
        /// Enumerate a <see cref="LongToObjectMap"/>
        /// </summary>
        /// <typeparam name="TValue">The type of the objects in the map</typeparam>
        /// <param name="map">The map to enumerate</param>
        [CLSCompliant(false)]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static IEnumerable<KeyValuePair<int, TValue>> ToIEnumerable<TValue>(this LongToObjectMap map)
        {
            for (int i = 0, size = map.Size; i < size; i++)
            {
                map.GetKeyValue(i, out int key, out object obj);
                yield return new KeyValuePair<int, TValue>(key, (TValue)obj);
            }
        }

        /// <summary>
        /// Creates a <see cref="LongToObjectMap"/> from enumeration of int-to-object pairs
        /// </summary>
        /// <typeparam name="TValue">The type of the objects in the pairs</typeparam>
        /// <param name="integersToObjects">The key-value pairs for the map</param>
        [CLSCompliant(false)]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static LongToObjectMap ToLongToObjectMap<TValue>(this IEnumerable<KeyValuePair<int, TValue>> integersToObjects)
        {
            try
            {
                var map = new LongToObjectMapClass();
                foreach (var intToObject in integersToObjects)
                {
                    map.Set(intToObject.Key, intToObject.Value);
                }
                return map;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI50293");
            }
        }

        /// <summary>
        /// Returns true if the type of the object is a numeric type
        /// </summary>
        /// <param name="value">The object to check the type of</param>
        /// <returns>Returns true of the type of the object is a numeric type, false if not</returns>
        public static bool IsNumericType(this object value)
        {
            try
            {
                switch (value)
                {
                    case Decimal _:
                    case Single _:
                    case Double _:
                    case Byte _:
                    case SByte _:
                    case Int16 _:
                    case Int32 _:
                    case Int64 _:
                    case UInt16 _:
                    case UInt32 _:
                    case UInt64 _:
                        return true;
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI50292");
            }
        }

        #endregion Methods

        #region Helper Methods

        /// <summary>
        /// Validates the license.
        /// </summary>
        /// <param name="eliCode">The eli code to associate with the license validation check.</param>
        static void ValidateLicense(string eliCode)
        {
            LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                eliCode, _OBJECT_NAME);
        }

        #endregion Helper Methods

        #region Serialization Methods

        /// <summary>
        /// Serializes the specified object to a hexadecimal string.
        /// </summary>
        /// <param name="data">The data to serialize.</param>
        /// <returns>A hexadecimal string.</returns>
        public static string ToSerializedHexString(this ISerializable data)
        {
            try
            {
                if (data == null)
                {
                    throw new ArgumentNullException("data");
                }

                ValidateLicense("ELI31807");

                using (var stream = new MemoryStream())
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, data);
                    return stream.ToArray().ToHexString();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31808");
            }
        }

        /// <summary>
        /// Deserializes the data from a hexadecimal string.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize from the string.</typeparam>
        /// <param name="serializedHexValue">The serialized hexadecimal string.</param>
        /// <returns>A new <typeparamref name="T"/> that is equivalent to the serialized
        /// string representation.</returns>
        public static T DeserializeFromHexString<T>(this string serializedHexValue) where T : ISerializable
        {
            try
            {
                if (string.IsNullOrEmpty(serializedHexValue))
                {
                    throw new ArgumentException("Serialized data string must not be null or empty.",
                        "serializedHexValue");
                }

                var bytes = serializedHexValue.ToByteArray();
                using (var stream = new MemoryStream(bytes))
                {
                    stream.Position = 0;
                    var formatter = new BinaryFormatter();
                    var data = (T)formatter.Deserialize(stream);
                    return data;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31810");
            }
        }

        #endregion Serialization Methods

        #region Stream Methods

        /// <summary>
        /// Reads from stream with a default buffer size of 32768.
        /// <para><b>Note:</b></para>
        /// Caller is responsible to open and close all streams.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        public static void ReadFromStream(this Stream source, Stream destination)
        {
            source.ReadFromStream(destination, _DEFAULT_BUFFER);
        }

        /// <summary>
        /// Reads from stream.
        /// <para><b>Note:</b></para>
        /// Caller is responsible to open and close all streams.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        /// <param name="bufferSize">Size of the buffer.</param>
        public static void ReadFromStream(this Stream source, Stream destination, int bufferSize)
        {
            try
            {
                if (source == null)
                {
                    throw new ArgumentNullException("source");
                }
                if (destination == null)
                {
                    throw new ArgumentNullException("destination");
                }
                if (bufferSize <= 0)
                {
                    throw new ArgumentOutOfRangeException("bufferSize", bufferSize,
                        "Must be > 0");
                }

                byte[] buffer = new byte[bufferSize];
                int bytesRead = 0;
                do
                {
                    bytesRead = source.Read(buffer, 0, bufferSize);
                    destination.Write(buffer, 0, bytesRead);
                } while (bytesRead != 0);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31811");
            }
        }

        /// <summary>
        /// Converts the specified stream into an array of bytes.
        /// <para><b>Note:</b></para>
        /// Caller is responsible to close the provided stream.
        /// </summary>
        /// <param name="source">The stream to read from.</param>
        /// <returns>An array of bytes.</returns>
        public static byte[] ToByteArray(this Stream source)
        {
            try
            {
                if (source == null)
                {
                    throw new ArgumentNullException("source");
                }

                using (var dest = new MemoryStream())
                {
                    source.ReadFromStream(dest);
                    return dest.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31812");
            }
        }

        /// <summary>
        /// Returns a list filtered with a regular expression
        /// </summary>
        /// <typeparam name="T">The type contained in the list</typeparam>
        /// <param name="list">List of items to filter</param>
        /// <param name="regularExpression">Regular expression to use for filtering</param>
        /// <param name="stringToFilter">Provides what to test the regularExpression against"/>"/></param>
        /// <returns>Filtered IEnumerable</returns>
        public static IEnumerable<T> FilterWithRegex<T>(this IEnumerable<T> list, string regularExpression, Func<T,string> stringToFilter)
        {
            try
            {
                Regex filterSearch = new Regex(regularExpression);
                return list.Where(p => filterSearch.IsMatch(stringToFilter(p)));
            }
            catch ( Exception ex)
            {
                throw ex.AsExtract("ELI49936");
            }
        }

        /// <summary>
        /// Returns a list that is Filtered by an includeFilter and ExcludeFilter
        /// </summary>
        /// <typeparam name="T">The type contained in the list</typeparam>
        /// <param name="list">List of items to filter</param>
        /// <param name="includeRegex">Regular expression to use to filter items to include in list</param>
        /// <param name="excludeRegex">Regular expression to use to filter items to exclude from list</param>
        /// <param name="stringToFilter">Provides what to test the regular expressions against</param>
        /// <returns>List filtered to include items that match includeRegEx but exclude excludeRegEx</returns>
        public static IEnumerable<T> FilterWithRegex<T>(this IEnumerable<T> list, string includeRegex, string excludeRegex, Func<T,string> stringToFilter)
        {
            try
            {
                return list
                    .FilterWithRegex(includeRegex, stringToFilter)
                    .Except(list.FilterWithRegex(excludeRegex, stringToFilter));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49937");
            }
        }

        #endregion Stream Methods
    }
}
