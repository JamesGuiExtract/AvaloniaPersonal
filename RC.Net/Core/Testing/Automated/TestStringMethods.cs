using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace Extract.Test
{
    /// <summary>
    /// Class for testing the <see cref="StringMethods"/> class.
    /// </summary>
    [TestFixture]
    [Category("StringMethods")]
    public class TestStringMethods
    {
        /// <summary>
        /// Tests converting an array of bytes into a Hex string.
        /// </summary>
        [Test, Category("Automated")]
        public static void ConvertBytesToHexString()
        {
            byte[] bytes = new byte[] { 255, 0, 255, 0 };
            string hex = StringMethods.ConvertBytesToHexString(bytes);
            Assert.That(hex.Equals("FF00FF00", StringComparison.Ordinal));
        }

        /// <summary>
        /// Tests converting an empty array of bytes into a Hex string.
        /// </summary>
        [Test, Category("Automated")]
        public static void ConvertBytesToHexStringEmptyArray()
        {
            byte[] bytes = new byte[] { };
            string hex = StringMethods.ConvertBytesToHexString(bytes);
            Assert.That(string.IsNullOrEmpty(hex));
        }

        /// <summary>
        /// Tests converting a Hex string into a string of bytes.
        /// </summary>
        [Test, Category("Automated")]
        public static void ConvertHexStringToBytes()
        {
            string hex = "FF00FF00";
            byte[] bytes = StringMethods.ConvertHexStringToBytes(hex);
            Assert.That(bytes[0] == 255
                        && bytes[1] == 0
                        && bytes[0] == bytes[2]
                        && bytes[1] == bytes[3]);
        }

        /// <summary>
        /// Tests the roundtrip conversion from a hex string to bytes and back to a hex string.
        /// </summary>
        [Test, Category("Automated")]
        public static void ConvertHexToBytesAndBackToHex()
        {
            string hex = "FFA0E70455429D75300E4242424242B69E3B42";
            byte[] bytes = StringMethods.ConvertHexStringToBytes(hex);
            Assert.That(hex.Equals(StringMethods.ConvertBytesToHexString(bytes)));
        }

        /// <summary>
        /// Tests that the convert to hex string function will throw an exception with
        /// an odd length string.
        /// </summary>
        [Test, Category("Automated")]
        public static void ConvertHexStringLengthException()
        {
            string hex = "FFEE0A42F";
            Assert.Throws<ExtractException>(delegate
            {
                StringMethods.ConvertHexStringToBytes(hex);
            });
        }

        /// <summary>
        /// Tests that the convert to hex string function will throw an exception with
        /// when the hex string contains invalid characters.
        /// </summary>
        [Test, Category("Automated")]
        public static void ConvertHexStringInvalidCharacterException()
        {
            string hex = "FFGG";
            Assert.Throws<ExtractException>(delegate
            {
                StringMethods.ConvertHexStringToBytes(hex);
            });
        }

        /// <summary>
        /// Tests that the convert bytes to hex string function will throw an exception with
        /// a <see langword="null"/> array.
        /// </summary>
        [Test, Category("Automated")]
        public static void ConvertBytesNullException()
        {
            Assert.Throws<ExtractException>(delegate
            {
                StringMethods.ConvertBytesToHexString(null);
            });
        }

        /// <summary>
        /// Tests that the FindIndexOfAny function correctly finds the index of a single word.
        /// </summary>
        // Testing the default behavior in this class and so we do not want to
        // specify the comparison type
        [SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison",
            MessageId="Extract.StringMethods.FindIndexOfAny(System.String,System.Collections.Generic.IList`1<System.String>)")]
        [Test, Category("Automated")]
        public static void FindIndexOfAnySingleItem()
        {
            string[] items = new string[] { "Hello" };
            string helloWorld = "Hello World!";

            int index = StringMethods.FindIndexOfAny(helloWorld, items);
            Assert.That(index == 0);
        }

        /// <summary>
        /// Tests that the FindIndexOfAny function correctly finds the first index of
        /// in the value to search when there are multiple words to search for.
        /// </summary>
        // Testing the default behavior in this class and so we do not want to
        // specify the comparison type
        [SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison",
            MessageId="Extract.StringMethods.FindIndexOfAny(System.String,System.Collections.Generic.IList`1<System.String>)")]
        [Test, Category("Automated")]
        public static void FindIndexOfAnyMultipleItems()
        {
            string[] items = new string[] { "World", "Hello", "!" };
            string helloWorld = "Hello World!";
            int index = StringMethods.FindIndexOfAny(helloWorld, items);
            Assert.That(index == 0);
        }

        /// <summary>
        /// Tests that the FindIndexOfAny function does not find an item if it is not in
        /// the value to search.
        /// </summary>
        // Testing the default behavior in this class and so we do not want to
        // specify the comparison type
        [SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison",
            MessageId="Extract.StringMethods.FindIndexOfAny(System.String,System.Collections.Generic.IList`1<System.String>)")]
        [Test, Category("Automated")]
        public static void FindIndexOfAnyNotFoundSingleItem()
        {
            string[] items = new string[] { "GoodBye" };
            string helloWorld = "Hello World!";

            int index = StringMethods.FindIndexOfAny(helloWorld, items);
            Assert.That(index == -1);
        }

        /// <summary>
        /// Tests that the FindIndexOfAny function does not find an item if it is not in
        /// the value to search with multiple values to search.
        /// </summary>
        // Testing the default behavior in this class and so we do not want to
        // specify the comparison type
        [SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison",
            MessageId="Extract.StringMethods.FindIndexOfAny(System.String,System.Collections.Generic.IList`1<System.String>)")]
        [Test, Category("Automated")]
        public static void FindIndexOfAnyNotFoundMultipleItems()
        {
            string[] items = new string[] { "GoodBye", "Are", "You", "Sure", "Is This Here" };
            string helloWorld = "Hello World!";

            int index = StringMethods.FindIndexOfAny(helloWorld, items);
            Assert.That(index == -1);
        }

        /// <summary>
        /// Tests that the FindIndexOfAny function correctly finds a value ignoring
        /// the case of the words being searched when a case insensitive string
        /// comparison is specified.
        /// </summary>
        [Test, Category("Automated")]
        public static void FindIndexOfAnyCaseInsensitive()
        {
            string[] items = new string[] { "HELLO" };
            string helloWorld = "Hello World!";

            int index = StringMethods.FindIndexOfAny(helloWorld, items,
                StringComparison.CurrentCultureIgnoreCase);
            Assert.That(index == 0);
        }

        /// <summary>
        /// Tests that the FindIndexOfAny function correctly finds the correct
        /// index when the last item in the list of strings to search for is the
        /// earliest item in the string to search.
        /// </summary>
        // Testing the default behavior in this class and so we do not want to
        // specify the comparison type
        [SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison",
            MessageId="Extract.StringMethods.FindIndexOfAny(System.String,System.Collections.Generic.IList`1<System.String>)")]
        [Test, Category("Automated")]
        public static void FindIndexOfAnyMultipleFound()
        {
            string[] items = new string[] { "!", "World", "ello", "H" };
            string helloWorld = "Hello World!";

            int index = StringMethods.FindIndexOfAny(helloWorld, items);
            Assert.That(index == 0);
        }

        /// <summary>
        /// Tests that the FindIndexOfAny function correctly finds the first
        /// instance of the searched for string when if occurs multiple times in the string.
        /// </summary>
        // Testing the default behavior in this class and so we do not want to
        // specify the comparison type
        [SuppressMessage("Microsoft.Globalization", "CA1307:SpecifyStringComparison",
            MessageId="Extract.StringMethods.FindIndexOfAny(System.String,System.Collections.Generic.IList`1<System.String>)")]
        [Test, Category("Automated")]
        public static void FindIndexOfAnyFindsFirst()
        {
            string[] items = new string[] { "!" };
            string helloWorld = "Hello! World!";

            int index = StringMethods.FindIndexOfAny(helloWorld, items);
            Assert.That(index == 5);
        }
    }
}
