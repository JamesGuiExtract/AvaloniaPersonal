using Extract.Testing.Utilities;
using NUnit.Framework;
using System.Linq;
using System.Runtime.InteropServices;
using UCLID_COMUTILSLib;

namespace Extract.Utilities.Test
{
    /// <summary>
    /// Class for testing of COMUtils classes and methods
    /// </summary>
    [TestFixture, Category("ComUtils")]
    public class TestComUtils
    {
        #region TestSetup

        /// <summary>
        /// Initializes the test fixture for testing these methods
        /// </summary>
        [OneTimeSetUp]
        public static void Initialize()
        {
            GeneralMethods.TestSetup();
        }

        static StrToStrMap GetStrToStrMap(bool caseSensitive)
        {
            var map = new StrToStrMap();
            map.CaseSensitive = caseSensitive;

            map.Set("Apple", "fruit");
            map.Set("DOG", "animal");
            map.Set("car", "vehicle");

            return map;
        }

        #endregion TestSetup

        [Test, Category("StrToStrMap")]
        public static void StrToStrCaseSensitiveSetter()
        {
            var map = new StrToStrMap();
            Assert.IsTrue(map.CaseSensitive);
            map.Set("A", "1");
            Assert.Throws<COMException>(() => map.CaseSensitive = false);

            map = new StrToStrMap();
            map.CaseSensitive = false;
            Assert.IsFalse(map.CaseSensitive);
        }

        [Test, Category("StrToStrMap")]
        public static void StrToStrGetValueCaseSensitive()
        {
            var map = GetStrToStrMap(true);
            Assert.AreEqual("fruit", map.GetValue("Apple"));
            Assert.Throws<COMException>(() => map.GetValue("apple"));
        }

        [Test, Category("StrToStrMap")]
        public static void StrToStrGetValueCaseInsensitive()
        {
            var map = GetStrToStrMap(false);
            Assert.AreEqual("fruit", map.GetValue("Apple"));
            Assert.AreEqual("fruit", map.GetValue("apple"));
        }

        [Test, Category("StrToStrMap")]
        public static void StrToStrClear()
        {
            var map = GetStrToStrMap(true);
            Assert.AreEqual(3, map.Size);
            map.Clear();
            Assert.AreEqual(0, map.Size);
        }

        [Test, Category("StrToStrMap")]
        public static void StrToStrContainsCaseSensitive()
        {
            var map = GetStrToStrMap(true);
            Assert.IsTrue(map.Contains("Apple"));
            Assert.IsFalse(map.Contains("apple"));
            Assert.IsFalse(map.Contains("A"));
        }

        [Test, Category("StrToStrMap")]
        public static void StrToStrContainsCaseInsensitive()
        {
            var map = GetStrToStrMap(false);
            Assert.IsTrue(map.Contains("Apple"));
            Assert.IsTrue(map.Contains("apple"));
            Assert.IsFalse(map.Contains("A"));
        }

        [Test, Category("StrToStrMap")]
        public static void StrToStrRemoveItemCaseSensitive()
        {
            var map = GetStrToStrMap(true);
            map.RemoveItem("Apple");
            Assert.IsFalse(map.Contains("Apple"));
            map.RemoveItem("dog");
            Assert.IsFalse(map.Contains("dog"));
            Assert.IsTrue(map.Contains("DOG"));
        }

        [Test, Category("StrToStrMap")]
        public static void StrToStrRemoveItemCaseInsensitive()
        {
            var map = GetStrToStrMap(false);
            map.RemoveItem("Apple");
            Assert.IsFalse(map.Contains("Apple"));
            map.RemoveItem("dog");
            Assert.IsFalse(map.Contains("dog"));
            Assert.IsFalse(map.Contains("DOG"));
        }

        [Test, Category("StrToStrMap")]
        public static void StrToStrGetKeys()
        {
            var map = GetStrToStrMap(true);
            var keys = map.GetKeys().ToIEnumerable<string>();
            Assert.IsTrue(keys.SequenceEqual(new[] { "Apple", "DOG", "car" }));
        }

        [Test, Category("StrToStrMap")]
        public static void StrToStrGetAllKeyValuePairs()
        {
            var map = GetStrToStrMap(true);
            var pairs = map.GetAllKeyValuePairs()
                .ToIEnumerable<IStringPair>()
                .ToArray();

            Assert.AreEqual("Apple", pairs[0].StringKey);
            Assert.AreEqual("fruit", pairs[0].StringValue);
            Assert.AreEqual("DOG", pairs[1].StringKey);
            Assert.AreEqual("animal", pairs[1].StringValue);
            Assert.AreEqual("car", pairs[2].StringKey);
            Assert.AreEqual("vehicle", pairs[2].StringValue);
        }

        [Test, Category("StrToStrMap")]
        public static void StrToStrGetKeyValue()
        {
            var map = GetStrToStrMap(true);

            map.GetKeyValue(0, out string key, out string value);
            Assert.AreEqual("Apple", key);
            Assert.AreEqual("fruit", value);

            map.GetKeyValue(1, out key, out value);
            Assert.AreEqual("DOG", key);
            Assert.AreEqual("animal", value);

            map.GetKeyValue(2, out key, out value);
            Assert.AreEqual("car", key);
            Assert.AreEqual("vehicle", value);
        }

        [Test, Category("StrToStrMap")]
        public static void StrToStrRenameKeyCaseSensitive()
        {
            var map = GetStrToStrMap(true);
            map.RenameKey("Apple", "apple");
            Assert.AreEqual(3, map.Size);
            Assert.IsFalse(map.Contains("Apple"));
            Assert.IsTrue(map.Contains("apple"));
        }

        [Test, Category("StrToStrMap")]
        public static void StrToStrRenameKeyCaseInsensitive()
        {
            var map = GetStrToStrMap(false);
            map.RenameKey("apple", "Pear");
            Assert.AreEqual(3, map.Size);
            Assert.IsFalse(map.Contains("Apple"));
            Assert.IsTrue(map.Contains("pear"));
        }

        [Test, Category("StrToStrMap")]
        public static void StrToStrMergeAppendCaseSensitive()
        {
            var map = GetStrToStrMap(true);
            var map2 = GetStrToStrMap(true);
            map2.Set("Apple", "food");
            map2.Set("Car", "automobile");
            map.Merge(map2, EMergeMethod.kAppend);
            Assert.AreEqual(5, map.Size);
            Assert.AreEqual("food", map.GetValue("Apple_1"));
        }

        [Test, Category("StrToStrMap")]
        public static void StrToStrMergeKeepOriginalCaseSensitive()
        {
            var map = GetStrToStrMap(true);
            var map2 = GetStrToStrMap(true);
            map2.Set("Apple", "food");
            map2.Set("Car", "automobile");
            map.Merge(map2, EMergeMethod.kKeepOriginal);
            Assert.AreEqual(4, map.Size);
            Assert.AreEqual("fruit", map.GetValue("Apple"));
        }

        [Test, Category("StrToStrMap")]
        public static void StrToStrMergeOverwriteCaseSensitive()
        {
            var map = GetStrToStrMap(true);
            var map2 = GetStrToStrMap(true);
            map2.Set("Apple", "food");
            map2.Set("Car", "automobile");
            map.Merge(map2, EMergeMethod.kOverwriteOriginal);
            Assert.AreEqual(4, map.Size);
            Assert.AreEqual("food", map.GetValue("Apple"));
        }

        [Test, Category("StrToStrMap")]
        public static void StrToStrMergeAppendCaseInsensitive()
        {
            var map = GetStrToStrMap(false);
            var map2 = GetStrToStrMap(false);
            map2.Set("Apple", "food");
            map2.Set("Car", "automobile");
            map.Merge(map2, EMergeMethod.kAppend);
            Assert.AreEqual(5, map.Size);
            Assert.AreEqual("food", map.GetValue("Apple_1"));
            Assert.AreEqual("automobile", map.GetValue("Car_1"));
        }

        [Test, Category("StrToStrMap")]
        public static void StrToStrMergeKeepOriginalCaseInsensitive()
        {
            var map = GetStrToStrMap(false);
            var map2 = GetStrToStrMap(false);
            map2.Set("Apple", "food");
            map2.Set("Car", "automobile");
            map.Merge(map2, EMergeMethod.kKeepOriginal);
            Assert.AreEqual(map.Size, 3);
            Assert.AreEqual("fruit", map.GetValue("Apple"));
            Assert.AreEqual("vehicle", map.GetValue("Car"));
        }

        [Test, Category("StrToStrMap")]
        public static void StrToStrMergeOverwriteCaseInsensitive()
        {
            var map = GetStrToStrMap(false);
            var map2 = GetStrToStrMap(false);
            map2.Set("Apple", "food");
            map2.Set("Car", "automobile");
            map.Merge(map2, EMergeMethod.kOverwriteOriginal);
            Assert.AreEqual(3, map.Size);
            Assert.AreEqual("food", map.GetValue("Apple"));
            Assert.AreEqual("automobile", map.GetValue("Car"));
        }

        [Test, Category("StrToStrMap")]
        public static void StrToStrCopyFrom()
        {
            var map1 = GetStrToStrMap(false);
            var map2 = GetStrToStrMap(true);

            map1.Set("House", "building");
            map1.RemoveItem("dog");
            ((ICopyableObject)map2).CopyFrom(map1);

            Assert.IsFalse(map2.CaseSensitive);
            Assert.AreEqual(3, map2.Size);
            Assert.AreEqual("building", map2.GetValue("house"));
            Assert.IsFalse(map2.Contains("DOG"));
        }

        #region TestMethods


        #endregion TestMethods
    }
}
