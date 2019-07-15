using Extract.AttributeFinder.Rules;
using Extract.Testing.Utilities;
using NUnit.Framework;
using System.Collections.ObjectModel;
using System.Linq;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

namespace Extract.AttributeFinder.Test
{
    /// <summary>
    /// Summary description for MoveCopyAttributeTest
    /// </summary>
    [TestFixture]
    [Category("MoveCopyAttribute")]
    [Category("Automated")]
    public class TestMoveCopyAttribute
    {
        internal class AttributeDefinition
        {
            public string Name { get; set; }
            public string Value { get; set; }

            public Collection<AttributeDefinition> SubAttributes { get; set; }
        }

        #region Test Data

        internal static readonly Collection<AttributeDefinition> AttributeSet = new Collection<AttributeDefinition>
        {
            new AttributeDefinition
            {
                Name ="A", Value = "A0", SubAttributes = new Collection<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        Name = "B", Value = "B0", SubAttributes = new Collection<AttributeDefinition>
                        {
                            new AttributeDefinition{Name = "C", Value = "C0",  SubAttributes = new Collection<AttributeDefinition>() },
                            new AttributeDefinition{Name = "C", Value = "C1", SubAttributes = new Collection<AttributeDefinition>() }
                        }
                    },
                    new AttributeDefinition
                    {
                        Name = "B", Value =  "B1", SubAttributes = new Collection<AttributeDefinition>()
                    }
                }
            },
            new AttributeDefinition
            {
                Name = "A", Value = "A1", SubAttributes = new Collection<AttributeDefinition>()
            },
            new AttributeDefinition
            {
                Name ="A", Value = "A2", SubAttributes = new Collection<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        Name = "B", Value = "B2", SubAttributes = new Collection<AttributeDefinition>
                        {
                            new AttributeDefinition{Name = "C", Value = "C2",  SubAttributes = new Collection<AttributeDefinition>() },
                            new AttributeDefinition{Name = "C", Value = "C3", SubAttributes = new Collection<AttributeDefinition>() }
                        }
                    },
                    new AttributeDefinition
                    {
                        Name = "B", Value =  "B3", SubAttributes = new Collection<AttributeDefinition>()
                    }
                }
            }
        };

        internal static readonly Collection<AttributeDefinition> ExpectedAttributeSet1 = new Collection<AttributeDefinition>
        {
            new AttributeDefinition
            {
                Name ="A", Value = "A0", SubAttributes = new Collection<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        Name = "B", Value = "B0", SubAttributes = new Collection<AttributeDefinition>
                        {
                            new AttributeDefinition{Name = "C", Value = "C0",  SubAttributes = new Collection<AttributeDefinition>() },
                            new AttributeDefinition{Name = "C", Value = "C1", SubAttributes = new Collection<AttributeDefinition>() }
                        }
                    },
                    new AttributeDefinition
                    {
                        Name = "B", Value =  "B1", SubAttributes = new Collection<AttributeDefinition>()
                    }
                }
            },
            new AttributeDefinition
            {
                Name = "B", Value = "B0", SubAttributes = new Collection<AttributeDefinition>
                {
                    new AttributeDefinition{Name = "C", Value = "C0",  SubAttributes = new Collection<AttributeDefinition>() },
                    new AttributeDefinition{Name = "C", Value = "C1", SubAttributes = new Collection<AttributeDefinition>() }
                }
            },
            new AttributeDefinition{Name = "C", Value = "C0",  SubAttributes = new Collection<AttributeDefinition>() },
            new AttributeDefinition{Name = "C", Value = "C1", SubAttributes = new Collection<AttributeDefinition>() },
            new AttributeDefinition
            {
                Name = "B", Value =  "B1", SubAttributes = new Collection<AttributeDefinition>()
            },

            new AttributeDefinition{ Name = "A", Value = "A1", SubAttributes = new Collection<AttributeDefinition>() },
            new AttributeDefinition
            {
                Name ="A", Value = "A2", SubAttributes = new Collection<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        Name = "B", Value = "B2", SubAttributes = new Collection<AttributeDefinition>
                        {
                            new AttributeDefinition{Name = "C", Value = "C2",  SubAttributes = new Collection<AttributeDefinition>() },
                            new AttributeDefinition{Name = "C", Value = "C3", SubAttributes = new Collection<AttributeDefinition>() }
                        }
                    },
                    new AttributeDefinition
                    {
                        Name = "B", Value =  "B3", SubAttributes = new Collection<AttributeDefinition>()
                    }
                }
            },
            new AttributeDefinition
            {
                Name = "B", Value = "B2", SubAttributes = new Collection<AttributeDefinition>
                {
                    new AttributeDefinition{Name = "C", Value = "C2",  SubAttributes = new Collection<AttributeDefinition>() },
                    new AttributeDefinition{Name = "C", Value = "C3", SubAttributes = new Collection<AttributeDefinition>() }
                }
            },
            new AttributeDefinition{ Name = "C", Value = "C2",  SubAttributes = new Collection<AttributeDefinition>() },
            new AttributeDefinition{ Name = "C", Value = "C3", SubAttributes = new Collection<AttributeDefinition>() },
            new AttributeDefinition
            {
                Name = "B", Value =  "B3", SubAttributes = new Collection<AttributeDefinition>()
            }

        };

        internal static readonly Collection<AttributeDefinition> ExpectedAttributeSet2 = new Collection<AttributeDefinition>
        {
            new AttributeDefinition
            {
                Name ="A", Value = "A0", SubAttributes = new Collection<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        Name = "B", Value = "B0", SubAttributes = new Collection<AttributeDefinition>
                        {
                            new AttributeDefinition{Name = "C", Value = "C0",  SubAttributes = new Collection<AttributeDefinition>() },
                            new AttributeDefinition{Name = "C", Value = "C1", SubAttributes = new Collection<AttributeDefinition>() }
                        }
                    },
                    new AttributeDefinition
                    {
                        Name = "B", Value =  "B1", SubAttributes = new Collection<AttributeDefinition>()
                    },
                    new AttributeDefinition{Name = "C", Value = "C0",  SubAttributes = new Collection<AttributeDefinition>() },
                    new AttributeDefinition{Name = "C", Value = "C1", SubAttributes = new Collection<AttributeDefinition>() }
                }
            },
            new AttributeDefinition
            {
                Name = "A", Value = "A1", SubAttributes = new Collection<AttributeDefinition>()
            },
            new AttributeDefinition
            {
                Name ="A", Value = "A2", SubAttributes = new Collection<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        Name = "B", Value = "B2", SubAttributes = new Collection<AttributeDefinition>
                        {
                            new AttributeDefinition{Name = "C", Value = "C2",  SubAttributes = new Collection<AttributeDefinition>() },
                            new AttributeDefinition{Name = "C", Value = "C3", SubAttributes = new Collection<AttributeDefinition>() }
                        }
                    },
                    new AttributeDefinition
                    {
                        Name = "B", Value =  "B3", SubAttributes = new Collection<AttributeDefinition>()
                    },
                    new AttributeDefinition{Name = "C", Value = "C2",  SubAttributes = new Collection<AttributeDefinition>() },
                    new AttributeDefinition{Name = "C", Value = "C3", SubAttributes = new Collection<AttributeDefinition>() }
                }
            }

        };

        internal static readonly Collection<AttributeDefinition> ExpectedAttributeSet3 = new Collection<AttributeDefinition>
        {
            new AttributeDefinition
            {
                Name ="A", Value = "A0", SubAttributes = new Collection<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        Name = "B", Value = "B0", SubAttributes = new Collection<AttributeDefinition>()
                    },
                    new AttributeDefinition
                    {
                        Name = "B", Value =  "B1", SubAttributes = new Collection<AttributeDefinition>()
                    },
                    new AttributeDefinition{Name = "C", Value = "C0",  SubAttributes = new Collection<AttributeDefinition>() },
                    new AttributeDefinition{Name = "C", Value = "C1", SubAttributes = new Collection<AttributeDefinition>() }
                }
            },
            new AttributeDefinition
            {
                Name = "A", Value = "A3", SubAttributes = new Collection<AttributeDefinition>()
            },
                        new AttributeDefinition
            {
                Name ="A", Value = "A2", SubAttributes = new Collection<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        Name = "B", Value = "B2", SubAttributes = new Collection<AttributeDefinition>()
                    },
                    new AttributeDefinition
                    {
                        Name = "B", Value =  "B3", SubAttributes = new Collection<AttributeDefinition>()
                    },
                    new AttributeDefinition{Name = "C", Value = "C2",  SubAttributes = new Collection<AttributeDefinition>() },
                    new AttributeDefinition{Name = "C", Value = "C3", SubAttributes = new Collection<AttributeDefinition>() }
                }
            }
        };

        internal static readonly Collection<AttributeDefinition> ExpectedAttributeSet4 = new Collection<AttributeDefinition>
        {
            new AttributeDefinition
            {
                Name ="A", Value = "A0", SubAttributes = new Collection<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        Name = "B", Value = "B0", SubAttributes = new Collection<AttributeDefinition>
                        {
                            new AttributeDefinition{Name = "C", Value = "C0",  SubAttributes = new Collection<AttributeDefinition>() },
                            new AttributeDefinition{Name = "C", Value = "C1", SubAttributes = new Collection<AttributeDefinition>() }
                        }
                    },
                    new AttributeDefinition
                    {
                        Name = "B", Value =  "B1", SubAttributes = new Collection<AttributeDefinition>()
                    }
                }
            },
            new AttributeDefinition
            {
                Name = "A", Value = "A1", SubAttributes = new Collection<AttributeDefinition>()
            },
            new AttributeDefinition
            {
                Name ="A", Value = "A2", SubAttributes = new Collection<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        Name = "B", Value = "B2", SubAttributes = new Collection<AttributeDefinition>
                        {
                            new AttributeDefinition{Name = "C", Value = "C2",  SubAttributes = new Collection<AttributeDefinition>() },
                            new AttributeDefinition{Name = "C", Value = "C3", SubAttributes = new Collection<AttributeDefinition>() }
                        }
                    },
                    new AttributeDefinition
                    {
                        Name = "B", Value =  "B3", SubAttributes = new Collection<AttributeDefinition>()
                    }
                }
            },
            new AttributeDefinition
            {
                Name = "B", Value = "B0", SubAttributes = new Collection<AttributeDefinition>
                {
                    new AttributeDefinition{Name = "C", Value = "C0",  SubAttributes = new Collection<AttributeDefinition>() },
                    new AttributeDefinition{Name = "C", Value = "C1", SubAttributes = new Collection<AttributeDefinition>() }
                }
            },
            new AttributeDefinition
            {
                Name = "B", Value =  "B1", SubAttributes = new Collection<AttributeDefinition>()
            },
            new AttributeDefinition
            {
                Name = "B", Value = "B2", SubAttributes = new Collection<AttributeDefinition>
                {
                    new AttributeDefinition{Name = "C", Value = "C2",  SubAttributes = new Collection<AttributeDefinition>() },
                    new AttributeDefinition{Name = "C", Value = "C3", SubAttributes = new Collection<AttributeDefinition>() }
                }
            },
            new AttributeDefinition
            {
                Name = "B", Value =  "B3", SubAttributes = new Collection<AttributeDefinition>()
            }
        };

        internal static readonly Collection<AttributeDefinition> ExpectedAttributeSet5 = new Collection<AttributeDefinition>
        {
            new AttributeDefinition
            {
                Name ="A", Value = "A0", SubAttributes = new Collection<AttributeDefinition>()
            },
            new AttributeDefinition
            {
                Name = "A", Value = "A1", SubAttributes = new Collection<AttributeDefinition>()
            },
            new AttributeDefinition
            {
                Name ="A", Value = "A2", SubAttributes = new Collection<AttributeDefinition>()
            },
            new AttributeDefinition
            {
                Name = "B", Value = "B0", SubAttributes = new Collection<AttributeDefinition>
                {
                    new AttributeDefinition{Name = "C", Value = "C0",  SubAttributes = new Collection<AttributeDefinition>() },
                    new AttributeDefinition{Name = "C", Value = "C1", SubAttributes = new Collection<AttributeDefinition>() }
                }
            },
            new AttributeDefinition
            {
                Name = "B", Value =  "B1", SubAttributes = new Collection<AttributeDefinition>()
            },
            new AttributeDefinition
            {
                Name = "B", Value = "B2", SubAttributes = new Collection<AttributeDefinition>
                {
                    new AttributeDefinition{Name = "C", Value = "C2",  SubAttributes = new Collection<AttributeDefinition>() },
                    new AttributeDefinition{Name = "C", Value = "C3", SubAttributes = new Collection<AttributeDefinition>() }
                }
            },
            new AttributeDefinition
            {
                Name = "B", Value =  "B3", SubAttributes = new Collection<AttributeDefinition>()
            }
        };

        internal static readonly Collection<AttributeDefinition> ExpectedAttributeSet6 = new Collection<AttributeDefinition>
        {
            new AttributeDefinition
            {
                Name ="A", Value = "A0", SubAttributes = new Collection<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        Name = "B", Value = "B0", SubAttributes = new Collection<AttributeDefinition>
                        {
                            new AttributeDefinition{Name = "C", Value = "C0",  SubAttributes = new Collection<AttributeDefinition>() },
                            new AttributeDefinition{Name = "C", Value = "C1", SubAttributes = new Collection<AttributeDefinition>() }
                        }
                    },
                    new AttributeDefinition
                    {
                        Name = "B", Value =  "B1", SubAttributes = new Collection<AttributeDefinition>()
                    }
                }
            },
            new AttributeDefinition
            {
                Name = "A", Value = "A1", SubAttributes = new Collection<AttributeDefinition>()
            },
            new AttributeDefinition
            {
                Name ="A", Value = "A2", SubAttributes = new Collection<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        Name = "B", Value = "B2", SubAttributes = new Collection<AttributeDefinition>
                        {
                            new AttributeDefinition{Name = "C", Value = "C2",  SubAttributes = new Collection<AttributeDefinition>() },
                            new AttributeDefinition{Name = "C", Value = "C3", SubAttributes = new Collection<AttributeDefinition>() }
                        }
                    },
                    new AttributeDefinition
                    {
                        Name = "B", Value =  "B3", SubAttributes = new Collection<AttributeDefinition>()
                    }
                }
            },
            new AttributeDefinition
            {
                Name = "D", Value = "D0", SubAttributes = new Collection<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        Name = "B", Value = "B0", SubAttributes = new Collection<AttributeDefinition>
                        {
                            new AttributeDefinition{Name = "C", Value = "C0",  SubAttributes = new Collection<AttributeDefinition>() },
                            new AttributeDefinition{Name = "C", Value = "C1", SubAttributes = new Collection<AttributeDefinition>() }
                        }
                    },
                    new AttributeDefinition
                    {
                        Name = "B", Value =  "B1", SubAttributes = new Collection<AttributeDefinition>()
                    },
                    new AttributeDefinition
                    {
                        Name = "B", Value = "B2", SubAttributes = new Collection<AttributeDefinition>
                        {
                            new AttributeDefinition{Name = "C", Value = "C2",  SubAttributes = new Collection<AttributeDefinition>() },
                            new AttributeDefinition{Name = "C", Value = "C3", SubAttributes = new Collection<AttributeDefinition>() }
                        }
                    },
                    new AttributeDefinition
                    {
                        Name = "B", Value =  "B3", SubAttributes = new Collection<AttributeDefinition>()
                    }
                }
            }
        };

        #endregion

        #region Constructors

        public TestMoveCopyAttribute()
        {
        }

        #endregion

        #region Test Initialization

        /// <summary>
        /// Initializes the test fixture
        /// </summary>
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
        }

        #endregion

        #region Tests

        [Test]
        [Category("Automated")]
        [Category("OutputHandler")]
        [Category("MoveCopyAttributes")]
        public static void TestMoveAllToRoot()
        {
            var moveAttribute = new MoveCopyAttributes();
            moveAttribute.SourceAttributeTreeXPath = "//*";
            moveAttribute.DestinationAttributeTreeXPath = "/";
            moveAttribute.CopyAttributes = false;

            var attributes = CreateAttributesVector(AttributeSet);
            var expected = CreateAttributesVector(ExpectedAttributeSet1);
            TestCopyMoveSettings(moveAttribute, expected, attributes);
        }

        [Test]
        [Category("Automated")]
        [Category("OutputHandler")]
        [Category("MoveCopyAttributes")]
        public static void TestMoveDestinationSameAsSource()
        {
            var moveAttribute = new MoveCopyAttributes();
            moveAttribute.SourceAttributeTreeXPath = "//A";
            moveAttribute.DestinationAttributeTreeXPath = "//A";
            moveAttribute.CopyAttributes = false;

            var attributes = CreateAttributesVector(AttributeSet);

            Assert.Throws<ExtractException>(() =>
            {
                moveAttribute.ProcessOutput(attributes, null, null);
            },
            "Exception should be thrown if the source contains destination.");
        }

        [Test]
        [Category("Automated")]
        [Category("OutputHandler")]
        [Category("MoveCopyAttributes")]
        public static void TestCopyAllToRoot()
        {
            var moveAttribute = new MoveCopyAttributes();
            moveAttribute.SourceAttributeTreeXPath = "//*";
            moveAttribute.DestinationAttributeTreeXPath = "/";
            moveAttribute.CopyAttributes = true;

            var attributes = CreateAttributesVector(AttributeSet);
            var expected = CreateAttributesVector(AttributeSet);
            expected.Append(CreateAttributesVector(ExpectedAttributeSet1));
            TestCopyMoveSettings(moveAttribute, expected, attributes);
        }

        [Test]
        [Category("Automated")]
        [Category("OutputHandler")]
        [Category("MoveCopyAttributes")]
        public static void TestCopyDestinationSameAsSource()
        {
            var moveAttribute = new MoveCopyAttributes();
            moveAttribute.SourceAttributeTreeXPath = "//A";
            moveAttribute.DestinationAttributeTreeXPath = "//A";
            moveAttribute.CopyAttributes = true;

            var attributes = CreateAttributesVector(AttributeSet);

            Assert.Throws<ExtractException>(() =>
            {
                moveAttribute.ProcessOutput(attributes, null, null);
            },
            "Exception should be thrown if the source contains destination.");
        }


        [Test]
        [Category("Automated")]
        [Category("OutputHandler")]
        [Category("MoveCopyAttributes")]
        public static void TestCopySourceToDestination()
        {
            var moveAttribute = new MoveCopyAttributes();
            moveAttribute.SourceAttributeTreeXPath = "//A/B/C";
            moveAttribute.DestinationAttributeTreeXPath = "//A";
            moveAttribute.CopyAttributes = true;

            var attributes = CreateAttributesVector(AttributeSet);
            var expected = CreateAttributesVector(ExpectedAttributeSet2);

            TestCopyMoveSettings(moveAttribute, expected, attributes);
        }

        [Test]
        [Category("Automated")]
        [Category("OutputHandler")]
        [Category("MoveCopyAttributes")]
        public static void TestMoveSourceToDestination()
        {
            var moveAttribute = new MoveCopyAttributes();
            moveAttribute.SourceAttributeTreeXPath = "//A/B/C";
            moveAttribute.DestinationAttributeTreeXPath = "//A";
            moveAttribute.CopyAttributes = false;

            var attributes = CreateAttributesVector(AttributeSet);
            var expected = CreateAttributesVector(ExpectedAttributeSet3);

            TestCopyMoveSettings(moveAttribute, expected, attributes);
        }

        [Test]
        [Category("Automated")]
        [Category("OutputHandler")]
        [Category("MoveCopyAttributes")]
        public static void TestCopyTreeSourceToDestination()
        {
            var moveAttribute = new MoveCopyAttributes();
            moveAttribute.SourceAttributeTreeXPath = "//*/B";
            moveAttribute.DestinationAttributeTreeXPath = "/";
            moveAttribute.CopyAttributes = true;

            var attributes = CreateAttributesVector(AttributeSet);
            var expected = CreateAttributesVector(ExpectedAttributeSet4);

            TestCopyMoveSettings(moveAttribute, expected, attributes);
        }

        [Test]
        [Category("Automated")]
        [Category("OutputHandler")]
        [Category("MoveCopyAttributes")]
        public static void TestMoveTreeSourceToDestination()
        {
            var moveAttribute = new MoveCopyAttributes();
            moveAttribute.SourceAttributeTreeXPath = "//*/B";
            moveAttribute.DestinationAttributeTreeXPath = "/";
            moveAttribute.CopyAttributes = false;

            var attributes = CreateAttributesVector(AttributeSet);
            var expected = CreateAttributesVector(ExpectedAttributeSet5);

            TestCopyMoveSettings(moveAttribute, expected, attributes);
        }

        [Test]
        [Category("Automated")]
        [Category("OutputHandler")]
        [Category("MoveCopyAttributes")]
        public static void TestMoveTreeSourceToDestinationNotSameTree()
        {
            var moveAttribute = new MoveCopyAttributes();
            moveAttribute.SourceAttributeTreeXPath = "//A/*";
            moveAttribute.DestinationAttributeTreeXPath = "//D";
            moveAttribute.CopyAttributes = true;

            var attributes = CreateAttributesVector(AttributeSet);
            var attributeCreator = new AttributeCreator("C:\\Dummy.tif");
            attributes.PushBack(attributeCreator.Create("D", "D0"));

            var expected = CreateAttributesVector(ExpectedAttributeSet6);

            TestCopyMoveSettings(moveAttribute, expected, attributes);
        }

        #endregion

        #region Helper Methods

        static void TestCopyMoveSettings(MoveCopyAttributes moveAttribute, IUnknownVector expected, IUnknownVector attributes)
        {
            moveAttribute.ProcessOutput(attributes, null, null);

            Assert.IsTrue(IsEqual(expected, attributes));
        }

        static bool IsEqual(IAttribute a, IAttribute b)
        {
            Assert.AreEqual(a.Name, b.Name, "Attribute names should be the same.");

            bool equal = a.Name == b.Name;

            AttributeComparer comparer = new AttributeComparer();
            equal = equal && comparer.Compare(a, b) == 0;

            equal = equal && IsEqual(a.SubAttributes, b.SubAttributes);

            return equal;
        }

        static bool IsEqual(IUnknownVector a, IUnknownVector b)
        {
            Assert.AreEqual(a.Size(), b.Size(), "Results vectors should be the same size.");
            bool equal = a.Size() == b.Size();
            for (int i = 0; equal && i < a.Size(); i++)
            {
                var attributeA = a.At(i) as IAttribute;
                var attributeB = b.At(i) as IAttribute;
                equal = IsEqual(attributeA, attributeB);
            }
            return equal;
        }

        static IUnknownVector CreateAttributesVector(Collection<AttributeDefinition> collection)
        {
            var attributes = new IUnknownVector();
            var attributeCreator = new AttributeCreator("C:\\Dummy.tif");

            foreach (var ad in collection)
            {
                Attribute a = attributeCreator.Create(ad.Name, ad.Value);
                a.SubAttributes = CreateAttributesVector(ad.SubAttributes);
                attributes.PushBack(a);
            }

            return attributes;
        }

        #endregion
    }
}
