﻿using AttributeDbMgrComponentsLib;
using DocumentAPI;
using Extract.FileActionManager.Database.Test;
using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

using ApiUtils = DocumentAPI.Utils;
using DocumentAPI.Models;

namespace Extract.Web.DocumentAPI.Test
{
    /// <summary>
    /// test for DocumentAttributeSet, returned by Web API Document.GetResultSet(string fileId)
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("WebAPI")]
    public class TestDocumentAttributeSet
    {
        /// <summary>
        /// DTO to simplify writing the test
        /// </summary>
        class FileInfo
        {
            /// <summary>
            /// the resource-embedded xml file name to test DocumentAtributeSet against
            /// </summary>
            public string XmlFile { get; set; }

            /// <summary>
            /// The database name to fetch the DocumentAttributeSet from. Note that this
            /// value is used to set the web server Document API database name.
            /// </summary>
            public string DatabaseName { get; set; }

            /// <summary>
            /// The AttributeSetName to specify to retrieve the DocumentAttributeSet
            /// Note that this value is used to set the Web API Document API AttributeSetName.
            /// </summary>
            public string AttributeSetName { get; set; }
        }

        #region Constants

        /// <summary>
        /// Names for the temporary databases that are extracted from the resource folder and
        /// attached to the local database server, as needed for tests.
        /// </summary>
        static readonly string DbLabDE = "Demo_LabDE_Temp";
        static readonly string DbFlexIndex = "Demo_FlexIndex_Temp";
        static readonly string DbIDShield = "Demo_IDShield_Temp";

        /// <summary>
        /// These dictionaries use fileId as the key for a FileInfo object that describes the
        /// parameters necessary to get an associated DocumentAttributeSet.
        /// </summary>
        static Dictionary<int, FileInfo> LabDEFileIdToFileInfo = new Dictionary<int, FileInfo>
        {
            {1, new FileInfo {XmlFile = "Resources.A418.tif.restored.xml", DatabaseName = DbLabDE, AttributeSetName = "DataFoundByRules" } },
            {12, new FileInfo {XmlFile = "Resources.K151.tif.restored.xml", DatabaseName = DbLabDE, AttributeSetName = "DataFoundByRules" } }
        };

        static Dictionary<int, FileInfo> IDShieldFileIdToFileInfo = new Dictionary<int, FileInfo>
        {
            {2, new FileInfo {XmlFile = "Resources.TestImage002.tif.restored.xml", DatabaseName = DbIDShield, AttributeSetName = "Attr" } },
            {3, new FileInfo {XmlFile = "Resources.TestImage003.tif.restored.xml", DatabaseName = DbIDShield, AttributeSetName = "Attr" } }
        };

        static Dictionary<int, FileInfo> FlexIndexFileIdToFileInfo = new Dictionary<int, FileInfo>
        {
            {1, new FileInfo {XmlFile = "Resources.Example01.tif.restored.xml", DatabaseName = DbFlexIndex, AttributeSetName = "Attr"} },
            {3, new FileInfo {XmlFile = "Resources.Example03.tif.restored.xml", DatabaseName = DbFlexIndex, AttributeSetName = "Attr"} }
        };

        // Index for FulllText node, always the zeroeth child node of parent nodes.
        const int FullTextIndex = 0;

        // Index for AverageCharConfidence attribute in FullText node
        const int AvgCharConf = 0;

        // Indexes for SpatialLineZone attributes
        const int StartX = 0;
        const int StartY = 1;
        const int EndX = 2;
        const int EndY = 3;

        // Indexes for SpatialLineBounds attributes
        const int Top = 0;
        const int Left = 1;
        const int Bottom = 2;
        const int Right = 3;

        #endregion Constants

        #region Fields

        /// <summary>
        /// test file manager, used to extract test (XML) files from the resource.
        /// </summary>
        static TestFileManager<TestDocumentAttributeSet> _testXmlFiles;

        /// <summary>
        /// test DB Manager, used to extract a database backup file from the resource, and the attach/detach it
        /// to the local database server. 
        /// </summary>
        static FAMTestDBManager<TestDocumentAttributeSet> _testDbManager;

        static string _currentDatabaseName;

        #endregion Fields

        #region Setup and Teardown
        /// <summary>
        /// setup for nunit test
        /// </summary>
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testDbManager = new FAMTestDBManager<TestDocumentAttributeSet>();
            _testXmlFiles = new TestFileManager<TestDocumentAttributeSet>();
        }

        /// <summary>
        /// cleanup after all tests are finished
        /// </summary>
        [TestFixtureTearDown]
        public static void FinalCleanup()
        {
            if (_testDbManager != null)
            {
                _testDbManager.Dispose();
                _testDbManager = null;
            }

            if (_testXmlFiles != null)
            {
                _testXmlFiles.Dispose();
                _testXmlFiles = null;
            }
        }

        #endregion Setup and Teardown

        /// <summary>
        /// test LabDE documents
        /// </summary>
        #region Public Test Functions

        [Test, Category("Automated")]
        public static void TestManyLineAttribute()
        {
            try
            {
                var voa = new IUnknownVector();
                var ss = new SpatialString();
                string tempUssFileName = _testXmlFiles.GetFile("Resources.Image1.pdf.uss");
                Assert.That(File.Exists(tempUssFileName));

                ss.LoadFrom(tempUssFileName, bSetDirtyFlagToTrue: false);
                var attr = new AttributeClass { Name = "Manual", Value = ss };
                voa.PushBack(attr);

                var mapper = new AttributeMapper(voa, UCLID_FILEPROCESSINGLib.EWorkflowType.kExtraction);
                var docAttrr = mapper.MapAttributesToDocumentAttributeSet();
                Assert.AreEqual(0, docAttrr.Attributes.Where(a =>
                    a.SpatialPosition.Pages.Count != a.SpatialPosition.Pages.Distinct().Count()).Count());
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception reported: {0}", ex.Message);
                throw;
            }
        }

        [Test, Category("Automated")]
        public static void TestLabDE_DocumentAttributeSets()
        {
            try
            {
                _testDbManager.GetDatabase("Resources.Demo_LabDE.bak", DbLabDE);
                _currentDatabaseName = DbLabDE;

                foreach (var kvpFileInfo in LabDEFileIdToFileInfo)
                {
                    var fileId = kvpFileInfo.Key;
                    var fi = kvpFileInfo.Value;

                    var worked = TestFile(fileId, fi.DatabaseName, fi.XmlFile, fi.AttributeSetName);
                    Assert.IsTrue(worked);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail("Exception: {0}, in: {1}", ex.Message, Utils.GetMethodName());                
            }
            finally
            {                
                _testDbManager.RemoveDatabase(DbLabDE);
            }
        }

        /// <summary>
        /// test IDShield documents
        /// </summary>
        [Test, Category("Automated")]
        public static void TestIDShield_DocumentAttributeSets()
        {
            try
            {
                _testDbManager.GetDatabase("Resources.Demo_IDShield.bak", DbIDShield);
                _currentDatabaseName = DbIDShield;

                foreach (var kvpFileInfo in IDShieldFileIdToFileInfo)
                {
                    var fileId = kvpFileInfo.Key;
                    var fi = kvpFileInfo.Value;

                    var worked = TestFile(fileId, fi.DatabaseName, fi.XmlFile, fi.AttributeSetName);
                    Assert.IsTrue(worked);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail("Exception: {0}, in: {1}", ex.Message, Utils.GetMethodName());
            }
            finally
            {
                _testDbManager.RemoveDatabase(DbIDShield);
            }
        }

        /// <summary>
        /// test FlexIndex documents
        /// </summary>
        [Test, Category("Automated")]
        public static void TestFlexIndex_DocumentAttributeSets()
        {
            try
            {
                _testDbManager.GetDatabase("Resources.Demo_FlexIndex.bak", DbFlexIndex);
                _currentDatabaseName = DbFlexIndex;

                foreach (var kvpFileInfo in FlexIndexFileIdToFileInfo)
                {
                    var fileId = kvpFileInfo.Key;
                    var fi = kvpFileInfo.Value;

                    var worked = TestFile(fileId, fi.DatabaseName, fi.XmlFile, fi.AttributeSetName);
                    Assert.IsTrue(worked);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail("Exception: {0}, in: {1}", ex.Message, Utils.GetMethodName());
            }
            finally
            {
                _testDbManager.RemoveDatabase(DbFlexIndex);
            }
        }

        #endregion Public Test Functions

        #region Private functions

        static DocumentAttributeSet GetDocumentResultSet(int fileId)
        {
            UCLID_FILEPROCESSINGLib.FileProcessingDB db = null;

            try
            {
                var c = Utils.SetDefaultApiContext(_currentDatabaseName);
                var inf = FileApiMgr.GetInterface(c);
                inf.InUse = false;
                db = inf.Interface;

                using (var data = new DocumentData(ApiUtils.CurrentApiContext))
                {
                    Assert.IsTrue(data != null, "null DocumentData reference");
                    return data.GetDocumentResultSet(fileId.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex.Message}, in method: {Utils.GetMethodName()}");
                throw;
            }
            finally
            {
                db.CloseAllDBConnections();
            }
        }


        static void AssertSame(string xmlName, string xmlValue, string attrName, string attrValue)
        {
            Assert.IsTrue(attrValue.IsEquivalent(xmlValue),
                            "{0} value: {1}, is not equivalent to: {2} value: {3}",
                            xmlName,
                            xmlValue,
                            attrName,
                            attrValue);
        }

        static void AssertNodeName(XmlNode node, string name)
        {
            string nodeName = node.Name;
            Assert.IsTrue(nodeName.IsEquivalent(name),
                            "Node name test failed: expected: {0}, node is acutally named: {1}",
                            name,
                            nodeName);
        }

        static void TestSpatialLineBounds(XmlNode spatialLineBounds, SpatialLineBounds bounds, int i)
        {
            try
            {
                //< SpatialLineBounds Top = "92" Left = "1858" Bottom = "3136" Right = "2283" />
                var attributes = spatialLineBounds.Attributes;
                Assert.IsTrue(attributes != null, "null spatialLinebounds attrubtes, index: {0}", i);

                var top = attributes[Top];
                AssertSame(top.Name, top.Value, $"LineInfo[{i}].SpatialLineBounds.Top", bounds.Top.ToString());

                var left = attributes[Left];
                AssertSame(left.Name, left.Value, $"LineInfo[{i}].SpatialLineBounds.Left", bounds.Left.ToString());

                var bottom = attributes[Bottom];
                AssertSame(bottom.Name, bottom.Value, $"LineInfo[{i}].SpatialLineBounds.Bottom", bounds.Botton.ToString());

                var right = attributes[Right];
                AssertSame(right.Name, right.Value, $"LineInfo[{i}].SpatialLineBounds.Right", bounds.Right.ToString());
            }
            catch (Exception ex)
            {
                Assert.Fail($"Exception: {ex.Message}, in method: {Utils.GetMethodName()}");
            }
        }

        static string GetParentNodeName(XmlNode parentNode)
        {
            var name = parentNode.Name;
            if (name.IsEquivalent("HCData") ||
                name.IsEquivalent("MCData") ||
                name.IsEquivalent("LCData") ||
                name.IsEquivalent("Manual") )
            {
                return "Data";
            }

            return name;
        }

        /*
        <LabInfo>
            <FullText AverageCharConfidence="80">N/A</FullText>
            <SpatialLine PageNumber="1">
                <LineText AverageCharConfidence="70">N/</LineText>
                <SpatialLineZone StartX="89" StartY="1255" EndX="1619" EndY="1278" Height="2167"/>
                <SpatialLineBounds Top="172" Left="72" Bottom="2360" Right="1635"/>
            </SpatialLine>
            <SpatialLine PageNumber="2">
                <LineText AverageCharConfidence="100">A</LineText>
                <SpatialLineZone StartX="1909" StartY="3160" EndX="1923" EndY="3160" Height="25"/>
                <SpatialLineBounds Top="3148" Left="1909" Bottom="3172" Right="1923"/>
            </SpatialLine>
        */
        static XmlNode TestParentNode(XmlNode parentNode, DocumentAttribute docAttr)
        {
            try
            {
                // Test Name first - this is unusual because the xml attribute name is the tag name of the root attribute,
                // and the value of the ES_attribute is in the 0th child element Fulltext as the value.
                // Get AverageCharConfidence
                //<LabInfo>
                //    <FullText AverageCharConfidence = "86">N/A</FullText>
                var childNode = parentNode.ChildNodes[FullTextIndex];
                AssertNodeName(childNode, "FullText");

                string nodeName = GetParentNodeName(parentNode);
                AssertSame(nodeName, childNode.InnerText, docAttr.Name, docAttr.Value);

                // e.g. LabDE.PhysicianInfo has a child element FullText that doesn't have a AverageCharacterConfidence
                // attribute.
                if (childNode.Attributes.Count > 0)
                {
                    string xmlCharConfidence = childNode.Attributes[AvgCharConf].Value;
                    AssertSame(childNode.Name, xmlCharConfidence, "averageCharacterConfidence", docAttr.AverageCharacterConfidence.ToString());
                }

                return childNode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex.Message}, in method: {Utils.GetMethodName()}");
                throw;
            }
        }

        /// <summary>
        /// verify that the spatial page line number matches, if it exists
        /// </summary>
        /// <param name="spatialLineNode"></param>
        /// <param name="pageNumber"></param>
        /// <param name="index"></param>
        static void TestSpatialLinePageNumber(XmlNode spatialLineNode, int? pageNumber, int index)
        {
            try
            {
                if (pageNumber == null)
                {
                    if (spatialLineNode.Attributes != null)
                    {
                        Assert.IsTrue(spatialLineNode.Attributes["PageNumber"] == null, 
                                      "pageNumber is null but xml spatialLineNode has a PageNumber attribute");
                    }

                    return;
                }

                var xmlPageNumber = spatialLineNode.Attributes[0];
                AssertSame(spatialLineNode.Name,
                           xmlPageNumber.Value,
                           $"docAttr.SpatialPosition.Pages[{index}]",
                           pageNumber.ToString());
            }
            catch (Exception ex)
            {
                Assert.Fail($"Exception: {ex.Message}, in method: {Utils.GetMethodName()}");
            }
        }

        static void TestLineText(XmlNode lineTextNode, string text, int index)
        {
            try
            {
                AssertSame(lineTextNode.Name,
                           lineTextNode.InnerText,
                           $"docAttr.SpatialPosition.LineInfo[{index}].SpatialLineZone.Text",
                           text);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Exception: {ex.Message}, in method: {Utils.GetMethodName()}");
            }
        }

        //<SpatialLineZone StartX = "1878" StartY="1612" EndX="2264" EndY="1617" Height="3041"/>
        static void TestSpatialLineZone(XmlNode spatialLineZoneNode, SpatialLineZone zone, int i)
        {
            try
            {
                //< SpatialLineZone StartX = "1878" StartY = "1612" EndX = "2264" EndY = "1617" Height = "3041" />
                var attributes = spatialLineZoneNode.Attributes;
                Assert.IsTrue(attributes != null, "null spatialLineZoneNode attributes, index: {0}", i);

                var startX = attributes[StartX];
                AssertSame(startX.Name, startX.Value, $"LineInfo[{i}].SpatialLineZone.StartX", zone.StartX.ToString());

                var startY = attributes[StartY];
                AssertSame(startY.Name, startY.Value, $"LineInfo[{i}].SpatialLineZone.StartY", zone.StartY.ToString());

                var endX = attributes[EndX];
                AssertSame(endX.Name, endX.Value, $"LineInfo[{i}].SpatialLineZone.EndX", zone.EndX.ToString());

                var endY = attributes[EndY];
                AssertSame(endY.Name, endY.Value, $"LineInfo[{i}].SpatialLineZone.EndY", zone.EndY.ToString());
            }
            catch (Exception ex)
            {
                Assert.Fail($"Exception: {ex.Message}, in method: {Utils.GetMethodName()}");
            }
        }

        static void CompareParentNodeToAttribute(XmlNode parentNode, DocumentAttribute docAttr, bool topLevel = true)
        {
            try
            {
                if (topLevel)
                {
                    Debug.WriteLine($"Node: {parentNode.Name}");
                }
                else
                {
                    Debug.WriteLine($"\tNode: {parentNode.Name}");
                }

                var fullTextChildNode = TestParentNode(parentNode, docAttr);
                if (fullTextChildNode == null)
                {
                    return;     // only get here on an exception, don't throw any more...
                }

                var lastNode = fullTextChildNode;

                if ((bool)docAttr.HasPositionInfo)
                {
                    XmlNode nextNode = null;

                    for (int i = 0; i < docAttr.SpatialPosition.LineInfo.Count; ++i)
                    {
                        // process SpatialLine, and then it's nested child element LineText,
                        // and then the SpatialLineZone and SpatialLinebounds elements.
                        // if there node siblings, then continue the iteration with 
                        // the next pair(s) of SpatialLineZone and SpatialLineBounds 
                        // elements, otherwise restart the sequence testing.
                        if (nextNode == null || nextNode.NextSibling == null)
                        {

                            var spatialLineNode = lastNode.NextSibling;
                            AssertNodeName(spatialLineNode, "SpatialLine");

                            // last node at the nesting level of the spatialLine node - not the sub elements
                            lastNode = spatialLineNode;

                            TestSpatialLinePageNumber(spatialLineNode, docAttr.SpatialPosition.LineInfo[i].SpatialLineZone.PageNumber, i);

                            if (spatialLineNode.ChildNodes.Count == 0)
                            {
                                continue;
                            }

                            nextNode = spatialLineNode.ChildNodes[0];
                            AssertNodeName(nextNode, "LineText");

                            TestLineText(nextNode, docAttr.SpatialPosition.LineInfo[i].SpatialLineZone.Text, i);
                        }

                        nextNode = nextNode.NextSibling;
                        AssertNodeName(nextNode, "SpatialLineZone");

                        TestSpatialLineZone(nextNode, docAttr.SpatialPosition.LineInfo[i].SpatialLineZone, i);

                        nextNode = nextNode.NextSibling;
                        AssertNodeName(nextNode, "SpatialLineBounds");

                        TestSpatialLineBounds(nextNode, docAttr.SpatialPosition.LineInfo[i].SpatialLineBounds, i);
                    }
                }

                // Now process all the childAttribute items.
                for (int i = 0; i < docAttr.ChildAttributes.Count; ++i)
                {
                    var childDocAttr = docAttr.ChildAttributes[i];

                    Assert.IsTrue(lastNode != null, "last node is empty, child attribute index: {0}", i);
                    lastNode = lastNode.NextSibling;

                    CompareParentNodeToAttribute(lastNode, childDocAttr, topLevel: false);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail($"Exception: {ex.Message}, in method: {Utils.GetMethodName()}");
            }
        }

        static bool CompareXmlToDocumentAttributeSet(string xmlFile, DocumentAttributeSet documentAttributes)
        {
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFile);
                var root = xmlDoc.FirstChild;

                int rootCount = root.ChildNodes.Count;
                Assert.IsTrue(rootCount == documentAttributes.Attributes.Count,
                                "xml attributes count: {0}, != documentAttributes count: {1}",
                                rootCount,
                                documentAttributes.Attributes.Count);

                for (int i = 0; i < rootCount; ++i)
                {
                    var attr = documentAttributes.Attributes[i];
                    var parentNodes = root.ChildNodes[i];
                    CompareParentNodeToAttribute(parentNodes, attr);
                }
            }
            catch (Exception ex)
            {
                Assert.Fail($"Exception: {ex.Message}, in method: {Utils.GetMethodName()}");
            }

            return true;
        }

        static bool TestFile(int fileId, string dbName, string xmlFile, string attributeSetName)
        {
            try
            {
                DocumentAttributeSet das = GetDocumentResultSet(fileId);
                Assert.IsTrue(das.Attributes != null, "Didn't get attributes");

                string testXmlFile = _testXmlFiles.GetFile(xmlFile);
                Assert.That(File.Exists(testXmlFile));

                return CompareXmlToDocumentAttributeSet(testXmlFile, das);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex.Message}, in method: {Utils.GetMethodName()}");
                return false;
            }
        }

        #endregion Private functions
    }
}
