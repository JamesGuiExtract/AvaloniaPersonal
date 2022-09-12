using Extract.ErrorHandling;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Extract.ErrorHandling.Test
{
    [TestFixture]
    public class ExceptionDataTests
    {
        [Test]
        public void Indexer_ExpectedBehavior()
        {
            // Arrange
            var exceptionData = new ExceptionData();

            // Act
            exceptionData["Test1"] = "Test1 Value1";
            exceptionData["Test1"] = "Test1 Value2";
            exceptionData["Test2"] = "Value2";

            var test1Data = exceptionData["Test1"] as List<object>;
            var test2Data = exceptionData["Test2"] as List<object>;

            // Assert
            Assert.AreEqual(3, exceptionData.Count); ;
            
            CollectionAssert.AreEqual(test1Data, new List<object>() { "Test1 Value1", "Test1 Value2" });

            CollectionAssert.AreEqual(test2Data, new List<object>() { "Value2" });
        }

        [Test]
        public void Keys_ExpectedBehavior()
        {
            // Arrange
            var exceptionData = new ExceptionData
            {
                ["Test1"] = "Test1 Value1",
                ["Test1"] = "Test1 Value2",
                ["Test2"] = "Value2"
            };

            // Act
            var result = exceptionData.Keys;

            // Assert
            Assert.AreEqual(2, result.Count);
            var keylist = result.Cast<object>().ToList();

            CollectionAssert.AreEqual(keylist, new List<object>() { "Test1", "Test2" });
        }

        [Test]
        public void Values_ExpectedBehavior()
        {
            // Arrange
            var exceptionData = new ExceptionData
            {
                ["Test1"] = "Test1 Value1",
                ["Test1"] = "Test1 Value2",
                ["Test2"] = "Value2"
            };

            // Act
            var result = exceptionData.Values;

            var test1Data = exceptionData["Test1"] as List<object>;
            var test2Data = exceptionData["Test2"] as List<object>;
            List<List<object>> expectedResult = new List<List<object>>() { test1Data, test2Data };
            var resultList = result.Cast<List<object>>().ToList();

            // Assert
            Assert.AreEqual(2, result.Count);
            CollectionAssert.AreEqual(expectedResult, resultList);
        }

        [Test]
        public void Add_Key_Value()
        {
            // Arrange
            var exceptionData = new ExceptionData();
            object key = "Test";
            object value = 10;
            var expectedReturn = new List<object>() { 10 };

            // Act
            exceptionData.Add(key, value);

            // Assert
            CollectionAssert.AreEqual(expectedReturn, (List<object>)exceptionData[key]);
        }

        [Test]
        public void Add_DictionaryEntry()
        {
            // Arrange
            var exceptionData = new ExceptionData();
            object key = "Test";
            object value = 10;
            DictionaryEntry item = new(key, value);
            var expectedReturn = new List<object>() { 10 };

            // Act
            exceptionData.Add(item);

            // Assert
            CollectionAssert.AreEqual(expectedReturn, (List<object>)exceptionData[key]);
        }

        [Test]
        public void Clear_ExpectedBehavior()
        {
            // Arrange
            var exceptionData = new ExceptionData();
            object key = "Test";
            object value = 10;
            exceptionData.Add(key, value);

            // Act
            exceptionData.Clear();

            // Assert
            Assert.AreEqual(0, exceptionData.Count);
        }

        [Test]
        public void Contains_ExpectedBehavior()
        {
            // Arrange
            var exceptionData = new ExceptionData();

            // Act
            exceptionData.Add(new DictionaryEntry("Test1", "Value1"));
            exceptionData.Add(new DictionaryEntry("Test2", "Value2"));

            // Assert
            Assert.IsTrue(exceptionData.Contains("Test1"));
            Assert.IsFalse(exceptionData.Contains("NotIn"));
        }

        [Test]
        public void CopyTo_DictionaryEntry_Array()
        {
            // Arrange
            var exceptionData = new ExceptionData
            {
                new DictionaryEntry("Test1", "Value1"),
                new DictionaryEntry("Test2", "Value2")
            };

            DictionaryEntry[] array = new DictionaryEntry[exceptionData.Count];
            int arrayIndex = 0;

            // Act
            exceptionData.CopyTo(array, arrayIndex);

            // Assert
            CollectionAssert.AreEqual(exceptionData, array);
        }

        [Test]
        public void Remove_Key_ExpectedBehavior()
        {
            // Arrange
            var exceptionData = new ExceptionData
            {
                new DictionaryEntry("Test1", "Value1"),
                new DictionaryEntry("Test2", "Value2")
            };
            object key = "Test1";

            // Act
            exceptionData.Remove(key);

            // Assert
            Assert.AreEqual(1, exceptionData.Count);
            Assert.IsFalse(exceptionData.Contains(key));
        }

        [Test]
        public void Remove_KeyValuePair_ExpectedBehavior()
        {
            // Arrange
            var exceptionData = new ExceptionData
            {
                new DictionaryEntry("Test1", "Value1"),
                new DictionaryEntry("Test2", "Value2")
            };
            KeyValuePair<object, object> item = new ("Test2", "Value2");

            // Act
            var result = exceptionData.Remove(item);

            // Assert
            Assert.AreEqual(1, exceptionData.Count);
            Assert.IsFalse(exceptionData.Contains("Test2"));
        }

        [Test]
        public void CopyTo_ObjectArray_NotSupported()
        {
            // Arrange
            var exceptionData = new ExceptionData
            {
                new DictionaryEntry("Test1", "Value1"),
                new DictionaryEntry("Test2", "Value2")
            };
            Array array = new object[exceptionData.Count];
            int index = 0;

            // Act

            // Assert
            Assert.Throws<NotImplementedException>(()
                => exceptionData.CopyTo(array, index));
        }

        [Test]
        public void GetObjectData_ExpectedBehavior()
        {
            // Arrange
            var exceptionData = new ExceptionData
            {
                new DictionaryEntry("Test1", "Value1"),
                new DictionaryEntry("Test2", "Value2")
            };
            SerializationInfo info = new(typeof(ExceptionData), new FormatterConverter());
            StreamingContext context = new (StreamingContextStates.All);

            // Act
            exceptionData.GetObjectData(info, context);

            // Assert
            var entries = info.GetValue("Entries", typeof(List<DictionaryEntry>)) as List<DictionaryEntry>;

            CollectionAssert.AreEqual(entries, exceptionData.OfType<DictionaryEntry>());
        }

        [Test]
        public void GetFlattenedData_ExpectedBehavior()
        {
            // Arrange
            var exceptionData = new ExceptionData
            {
                new DictionaryEntry("Test1", "Value1"),
                new DictionaryEntry("Test2", "Value2")
            };

            // Act
            var result = exceptionData.GetFlattenedData();

            // Assert
            CollectionAssert.AreEqual(result, exceptionData.OfType<DictionaryEntry>());
        }
    }
}
