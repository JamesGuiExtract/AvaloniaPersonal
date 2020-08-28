using Extract.Testing.Utilities;
using NUnit.Framework;
using System.Drawing;

namespace Extract.Utilities.Test
{
    /// <summary>
    /// Class for testing the <see cref="TestUtilityMethods"/> class.
    /// </summary>
    [TestFixture, Category("TestUtilityMethods")]
    class TestUtilityExtensionMethods
    {
        /// <summary>
        /// Initializes the test fixture for testing these methods
        /// </summary>
        [OneTimeSetUp]
        public static void Initialize()
        {
            GeneralMethods.TestSetup();
        }

        /// <summary>
        /// Performs post test execution cleanup.
        /// </summary>
        [OneTimeTearDown]
        public static void Cleanup()
        {
        }

        [Test, Category("TestUtilityMethods")]
        public static void TestSetPropertiesViaType()
        {
            var bar1 = new Bar();
            bar1.IntValue = 1;
            bar1.DoubleValue = 1.2;
            bar1.StringValue = "Three";
            bar1.ColorValue = Color.Blue;

            var bar2 = new Bar();

            bar2.SetPropertyValue("IntValue", 1);
            Assert.AreEqual(bar1.IntValue, bar2.IntValue);

            bar2.SetPropertyValue("DoubleValue", 1.2d);
            Assert.AreEqual(bar1.DoubleValue, bar2.DoubleValue);

            bar2.SetPropertyValue("StringValue", "Three");
            Assert.AreEqual(bar1.StringValue, bar2.StringValue);

            bar2.SetPropertyValue("ColorValue", Color.Blue);
            Assert.AreEqual(bar1.ColorValue, bar2.ColorValue);
        }

        [Test, Category("TestUtilityMethods")]
        public static void TestSetPropertiesViaString()
        {
            var bar1 = new Bar();
            bar1.IntValue = 1;
            bar1.DoubleValue = 1.2;
            bar1.StringValue = "Three";
            bar1.ColorValue = Color.Blue;

            var bar2 = new Bar();

            bar2.SetPropertyValue("IntValue", "1");
            Assert.AreEqual(bar1.IntValue, bar2.IntValue);

            bar2.SetPropertyValue("DoubleValue", "1.2");
            Assert.AreEqual(bar1.DoubleValue, bar2.DoubleValue);

            bar2.SetPropertyValue("StringValue", "Three");
            Assert.AreEqual(bar1.StringValue, bar2.StringValue);

            bar2.SetPropertyValue("ColorValue", "Blue");
            Assert.AreEqual(bar1.ColorValue, bar2.ColorValue);
        }

        [Test, Category("TestUtilityMethods")]
        public static void TestSetNestedPropertiesViaType()
        {
            var foo1 = new Foo();
            foo1.Bar = new Bar();
            foo1.Bar.IntValue = 1;
            foo1.Bar.DoubleValue = 1.2;
            foo1.Bar.StringValue = "Three";
            foo1.Bar.ColorValue = Color.Blue;

            var foo2 = new Foo();
            foo2.SetPropertyValue("Bar", new Bar());

            foo2.SetPropertyValue("Bar.IntValue", 1);
            Assert.AreEqual(foo1.Bar.IntValue, foo2.Bar.IntValue);

            foo2.SetPropertyValue("Bar.DoubleValue", 1.2d);
            Assert.AreEqual(foo1.Bar.DoubleValue, foo2.Bar.DoubleValue);

            foo2.SetPropertyValue("Bar.StringValue", "Three");
            Assert.AreEqual(foo1.Bar.StringValue, foo2.Bar.StringValue);

            foo2.SetPropertyValue("Bar.ColorValue", Color.Blue);
            Assert.AreEqual(foo1.Bar.ColorValue, foo2.Bar.ColorValue);
        }

        [Test, Category("TestUtilityMethods")]
        public static void TestSetNestedPropertiesViaString()
        {
            var foo1 = new Foo();
            foo1.Bar = new Bar();
            foo1.Bar.IntValue = 1;
            foo1.Bar.DoubleValue = 1.2;
            foo1.Bar.StringValue = "Three";
            foo1.Bar.ColorValue = Color.Blue;

            var foo2 = new Foo();
            foo2.SetPropertyValue("Bar", new Bar());

            foo2.SetPropertyValue("Bar.IntValue", "1");
            Assert.AreEqual(foo1.Bar.IntValue, foo2.Bar.IntValue);

            foo2.SetPropertyValue("Bar.DoubleValue", "1.2");
            Assert.AreEqual(foo1.Bar.DoubleValue, foo2.Bar.DoubleValue);

            foo2.SetPropertyValue("Bar.StringValue", "Three");
            Assert.AreEqual(foo1.Bar.StringValue, foo2.Bar.StringValue);

            foo2.SetPropertyValue("Bar.ColorValue", "Blue");
            Assert.AreEqual(foo1.Bar.ColorValue, foo2.Bar.ColorValue);
        }

        [Test, Category("TestUtilityMethods")]
        public static void TestSetPropertyReturnValues()
        {
            var foo = new Foo();
            var bar = new Bar();
            Assert.IsTrue(foo.SetPropertyValue("Bar", bar));        // null -> value1
            Assert.IsFalse(foo.SetPropertyValue("Bar", bar));       // value1 -> value1
            Assert.IsTrue(foo.SetPropertyValue("Bar", new Bar()));  // value1 -> value2
            Assert.IsTrue(foo.SetPropertyValue("Bar", null));       // value2 -> null

            Assert.IsTrue(bar.SetPropertyValue("IntValue", 1));     // 0 -> 1
            Assert.IsFalse(bar.SetPropertyValue("IntValue", "1"));  // 1 -> 1
            Assert.IsTrue(bar.SetPropertyValue("IntValue", "2"));   // 1 -> 2
            Assert.IsFalse(bar.SetPropertyValue("IntValue", 2));    // 2 -> 2

            foo.Bar = new Bar();
            Assert.IsTrue(foo.SetPropertyValue("Bar.ColorValue", Color.Blue));      // Empty -> Blue
            Assert.IsFalse(foo.SetPropertyValue("Bar.ColorValue", "Blue"));         // Blue -> Blue
            Assert.IsTrue(foo.SetPropertyValue("Bar.ColorValue", "Red"));           // Blue -> Red
            Assert.IsFalse(foo.SetPropertyValue("Bar.ColorValue", Color.Red));      // Red -> Red
            Assert.IsTrue(foo.SetPropertyValue("Bar.ColorValue", null));            // Red -> Empty
            Assert.IsFalse(foo.SetPropertyValue("Bar.ColorValue", Color.Empty));    // Empty -> Empty
        }

        [Test, Category("TestUtilityMethods")]
        public static void TestGetProperty()
        {
            var foo = new Foo();
            var bar = new Bar();
            bar.IntValue = 1;
            bar.DoubleValue = 1.2;
            bar.StringValue = "Three";
            bar.ColorValue = Color.Blue;

            Assert.AreEqual(null, foo.GetProperty("Bar"));
            foo.Bar = bar;
            Assert.AreEqual(bar, foo.GetProperty("Bar"));
            Assert.AreEqual(foo.Bar, foo.GetProperty("Bar"));
            Assert.AreEqual(1, foo.Bar.GetProperty("IntValue"));
            Assert.AreEqual(1.2d, foo.GetProperty("Bar.DoubleValue"));
            Assert.AreEqual(Color.Blue.Name, foo.GetProperty("Bar.ColorValue.Name"));
        }

        /// <summary>
        /// Helper class for utility method tests
        /// </summary>
        class Bar
        {
            public int IntValue { get; set; }
            public double DoubleValue { get; set; }
            public string StringValue { get; set; }
            public Color ColorValue { get; set; }
        }

        /// <summary>
        /// Helper class for utility method tests
        /// </summary>
        class Foo
        {
            public Bar Bar { get; set; }
        };
    }
}
