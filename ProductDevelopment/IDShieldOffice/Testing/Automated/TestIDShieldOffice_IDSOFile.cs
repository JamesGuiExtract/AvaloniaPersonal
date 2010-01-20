using Extract;
using System.Drawing;
using System.Drawing.Imaging;
using Extract.Imaging;
using Extract.Imaging.Forms;
using Extract.Rules;
using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

namespace IDShieldOffice.Test
{
    public partial class TestIDShieldOffice
    {
        #region IDSO File Constants

        private static readonly string _IDSO_FILE_CURRENT_VERSION = "1";

        #endregion IDSO File Constants

        #region IDSO File Helper Methods

        /// <summary>
        /// Gets the <see cref="ModificationHistory"/> object for the current
        /// <see cref="IDShieldOfficeForm"/>.
        /// </summary>
        /// <returns>The <see cref="ModificationHistory"/> for the current IDSO Form.</returns>
        private ModificationHistory GetModificationHistory()
        {
            // Get the modification history object
            ModificationHistory modificationHistory = _idShieldOfficeForm.Modification;

            // Ensure we got the modification history
            ExtractException.Assert("ELI23391", "Could not get the modification history.",
                modificationHistory != null);

            return modificationHistory;
        }

        /// <summary>
        /// Runs the SSN finder rule (using the find form) on the currently open image.
        /// If <paramref name="redactAll"/> is <see langword="true"/> then will perform
        /// a redact all; otherwise will just perform a find.
        /// </summary>
        private void RunSsnFinder(bool redactAll)
        {
            // Find the ssn data type
            string ssnDataTypeName = "";
            foreach (KeyValuePair<string, DataType> pair in DataTypeRule.ValidDataTypes)
            {
                if (pair.Key.Contains("Social"))
                {
                    ssnDataTypeName = pair.Key;
                    break;
                }
            }

            // Create a new RuleForm
            using (RuleForm ruleForm = new RuleForm("Data Type Finder",
                new DataTypeRule(new string[] { ssnDataTypeName }), _idShieldOfficeForm.ImageViewer, 
                _idShieldOfficeForm, _idShieldOfficeForm))
            {

                // Show the rule form
                ruleForm.Show();

                // Get the appropriate button (find next or redact all)
                Button button;
                if (redactAll)
                {
                    // Get the redact all button
                    button = FormMethods.GetFormComponent<Button>(ruleForm, "Redact all");
                }
                else
                {
                    // Get the find next button
                    button = FormMethods.GetFormComponent<Button>(ruleForm, "Find next");
                }

                // Wait for OCR completion
                while (!_idShieldOfficeForm.OcrManager.OcrFinished)
                {
                    System.Windows.Forms.Application.DoEvents();
                    System.Threading.Thread.Sleep(100);
                }

                // Click button and close the find form
                button.PerformClick();
                ruleForm.Close();
            }
        }

        /// <summary>
        /// Performs a redact all with the bracketed text finder on the currently open image with
        /// the specified parameters.
        /// </summary>
        /// <param name="squareBrackets">If <see langword="true"/> then will redact
        /// pairs of square brackets; if <see langword="false"/> will not match
        /// square brackets.</param>
        /// <param name="curvedBrackets">If <see langword="true"/> then will redact
        /// pairs of curved brackets; if <see langword="false"/> will not match
        /// curved brackets.</param>
        /// <param name="curlyBrackets">If <see langword="true"/> then will redact
        /// pairs of curly brackets; if <see langword="false"/> will not match
        /// curly brackets.</param>
        private void RunBracketedTextFinder(bool squareBrackets,
            bool curvedBrackets, bool curlyBrackets)
        {
            // Create a new RuleForm
            using (RuleForm ruleForm = new RuleForm("Bracketed Text Finder",
                new BracketedTextRule(squareBrackets, curvedBrackets, curlyBrackets),
                _idShieldOfficeForm.ImageViewer, _idShieldOfficeForm, _idShieldOfficeForm))
            {

                // Show the rule form
                ruleForm.Show();

                // Get the redact all button
                Button redactAll = FormMethods.GetFormComponent<Button>(ruleForm, "Redact all");

                // Wait for OCR completion
                while (!_idShieldOfficeForm.OcrManager.OcrFinished)
                {
                    System.Windows.Forms.Application.DoEvents();
                    System.Threading.Thread.Sleep(100);
                }

                // Click button and close the find form
                redactAll.PerformClick();
                ruleForm.Close();
            }
        }

        #endregion IDSO File Helper Methods

        #region IDSO File Default Contents

        /// <summary>
        /// Tests that the first element in the IDSO XML string is correct.
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesHasProperTopLevelElement()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that the first node is the IDSO file tag
            Assert.That(document.CreateNavigator().Select(@"/IDShieldOfficeData").Count > 0);
        }

        /// <summary>
        /// Tests that the version number in the IDSO XML string is correct.
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesHasProperVersionTag()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that the IDSO node contains the correct version number
            Assert.That(document.CreateNavigator().Select(
                @"//IDShieldOfficeData[@Version='" + _IDSO_FILE_CURRENT_VERSION + @"']").Count > 0);
        }
        
        /// <summary>
        /// Tests that the IDSO xml stream contains a historical objects node.
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesHasHistoricalObjectsNode()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that there is a historical objects node
            Assert.That(document.CreateNavigator().Select(@"//HistoricalObjects").Count > 0);
        }

        /// <summary>
        /// Tests that the IDSO xml stream contains a current objects node.
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesHasCurrentObjectsNode()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that there is a current objects node
            Assert.That(document.CreateNavigator().Select(@"//CurrentObjects").Count > 0);
        }

        /// <summary>
        /// Tests that the IDSO xml stream contains a sessions node.
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesHasSessionsNode()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that there is a sessions node
            Assert.That(document.CreateNavigator().Select(@"//Sessions").Count > 0);
        }

        /// <summary>
        /// Tests that the IDSO xml stream contains a sessions node that is not empty
        /// (the session node should never be empty).
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesSessionsIsNotEmpty()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Get the sessions node
            XPathNodeIterator nodeIterator = document.CreateNavigator().Select(@"//Sessions[1]");

            // Navigate to the sessions node
            while (nodeIterator.Current.Name != "Sessions")
            {
                ExtractException.Assert("ELI23453", "Could not find sessions node!",
                    nodeIterator.MoveNext());
            }

            // Check that the sessions node is not empty
            Assert.That(!nodeIterator.Current.IsEmptyElement);
        }

        /// <summary>
        /// Tests that the IDSO xml stream sessions node contains a session node.
        /// </summary>
        public void Automated_IdsoPropertiesSessionsNodeHasSessionNode()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that there is a session node
            Assert.That(document.CreateNavigator().Select(@"//Sessions//Session").Count > 0);
        }

        /// <summary>
        /// Tests that the IDSO xml stream contains a sessions node with an Id attribute.
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesSessionNodeHasId()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that the session node contains an Id attribute
            Assert.That(document.CreateNavigator().Select(@"//Sessions//Session[@Id]").Count > 0);
        }

        /// <summary>
        /// Tests that the IDSO xml stream contains a sessions node that is not empty
        /// (the session node should never be empty).
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesSessionIsNotEmpty()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Get the first session node
            XPathNodeIterator nodeIterator = document.CreateNavigator().Select(
                @"//Sessions//Session[1]");

            // Check that the node is not empty
            Assert.That(!nodeIterator.Current.IsEmptyElement);
        }

        /// <summary>
        /// Tests that the IDSO xml stream contains a sessions node that contains a session info node.
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesSessionNodeHasSessionInfo()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that there is a session info node
            Assert.That(document.CreateNavigator().Select(@"//Session//SessionInfo").Count > 0);
        }

        /// <summary>
        /// Tests that the IDSO xml stream contains a sessions node that contains an
        /// objects added node.
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesSessionNodeHasObjectsAddedNode()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that there is an objects added node
            Assert.That(document.CreateNavigator().Select(@"//Session//ObjectsAdded").Count > 0);
        }

        /// <summary>
        /// Tests that the IDSO xml stream contains a sessions node that contains an
        /// objects modified node.
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesSessionNodeHasObjectsModifiedNode()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that there is an objects modified node
            Assert.That(document.CreateNavigator().Select(@"//Session//ObjectsModified").Count > 0);
        }

        /// <summary>
        /// Tests that the IDSO xml stream contains a sessions node that contains an
        /// objects deleted node.
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesSessionNodeHasObjectsDeletedNode()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that there is an objects deleted node
            Assert.That(document.CreateNavigator().Select(@"//Session//ObjectsDeleted").Count > 0);
        }

        /// <summary>
        /// Tests that the IDSO xml stream contains a session info node that contains
        /// a user node.
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesSessionInfoHasUserNode()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that there is a user node
            Assert.That(document.CreateNavigator().Select(@"//SessionInfo//User").Count > 0);
        }

        /// <summary>
        /// Tests that the IDSO xml stream contains a session info node that contains
        /// a computer node.
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesSessionInfoHasComputerNode()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that there is a computer node
            Assert.That(document.CreateNavigator().Select(@"//SessionInfo//Computer").Count > 0);
        }

        /// <summary>
        /// Tests that the IDSO xml stream contains a session info node that contains
        /// a start time node.
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesSessionInfoHasStartTimeNode()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that there is a start time node
            Assert.That(document.CreateNavigator().Select(@"//SessionInfo//StartTime").Count > 0);
        }

        /// <summary>
        /// Tests that the IDSO xml stream contains a session info node that contains
        /// a end time node.
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesSessionInfoHasEndTimeNode()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that there is an end time node
            Assert.That(document.CreateNavigator().Select(@"//SessionInfo//EndTime").Count > 0);
        }


        #endregion IDSO File Default Contents

        #region IDSO File Data File Contents For Clues

        /// <summary>
        /// Tests that when a data type rule find is performed that the clues that are added
        /// also appear in the IDSO file.
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesCurrentObjectsContainsClues()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Ensure there are no layer objects on the image already
            ExtractException.Assert("ELI23400", "Image contains layer objects already!",
                _idShieldOfficeForm.ImageViewer.LayerObjects.Count == 0);

            // Run the ssn finder
            RunSsnFinder(false);

            // Ensure there were some clues added to the image
            ExtractException.Assert("ELI23401", "No clues where added!",
                _idShieldOfficeForm.ImageViewer.GetLayeredObjects(null, ArgumentRequirement.Any,
                new Type[] { typeof(Clue) }, ArgumentRequirement.Any,
                null, ArgumentRequirement.Any).Count > 0);

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that there are at least 2 clues
            Assert.That(document.CreateNavigator().Select(@"//CurrentObjects//Clue").Count > 1);
        }

        /// <summary>
        /// Tests that the clues node contains a page node.
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesCurrentObjectsClueContainsPage()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Ensure there are no layer objects on the image already
            ExtractException.Assert("ELI23402", "Image contains layer objects already!",
                _idShieldOfficeForm.ImageViewer.LayerObjects.Count == 0);

            // Run the ssn finder
            RunSsnFinder(false);

            // Ensure there were some clues added to the image
            ExtractException.Assert("ELI23403", "No clues where added!",
                _idShieldOfficeForm.ImageViewer.GetLayeredObjects(null, ArgumentRequirement.Any,
                new Type[] { typeof(Clue) }, ArgumentRequirement.Any,
                null, ArgumentRequirement.Any).Count > 0);

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that there is a page node
            Assert.That(document.CreateNavigator().Select(@"//Clue//Page").Count > 0);
        }

        /// <summary>
        /// Tests that the clues node contains a comment node.
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesCurrentObjectsClueContainsComment()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Ensure there are no layer objects on the image already
            ExtractException.Assert("ELI23404", "Image contains layer objects already!",
                _idShieldOfficeForm.ImageViewer.LayerObjects.Count == 0);

            // Run the ssn finder
            RunSsnFinder(false);

            // Ensure there were some clues added to the image
            ExtractException.Assert("ELI23405", "No clues where added!",
                _idShieldOfficeForm.ImageViewer.GetLayeredObjects(null, ArgumentRequirement.Any,
                new Type[] { typeof(Clue) }, ArgumentRequirement.Any,
                null, ArgumentRequirement.Any).Count > 0);

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that there is a comment node
            Assert.That(document.CreateNavigator().Select(@"//Clue//Comment").Count > 0);
        }

        /// <summary>
        /// Tests that the clues node contains a tags node.
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesCurrentObjectsClueContainsTags()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Ensure there are no layer objects on the image already
            ExtractException.Assert("ELI23406", "Image contains layer objects already!",
                _idShieldOfficeForm.ImageViewer.LayerObjects.Count == 0);

            // Run the ssn finder
            RunSsnFinder(false);

            // Ensure there were some clues added to the image
            ExtractException.Assert("ELI23407", "No clues where added!",
                _idShieldOfficeForm.ImageViewer.GetLayeredObjects(null, ArgumentRequirement.Any,
                new Type[] { typeof(Clue) }, ArgumentRequirement.Any,
                null, ArgumentRequirement.Any).Count > 0);

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that there is a tags node
            Assert.That(document.CreateNavigator().Select(@"//Clue//Tags").Count > 0);
        }

        /// <summary>
        /// Tests that the clues node contains a zone node.
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesCurrentObjectsClueContainsZone()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Ensure there are no layer objects on the image already
            ExtractException.Assert("ELI23408", "Image contains layer objects already!",
                _idShieldOfficeForm.ImageViewer.LayerObjects.Count == 0);

            // Run the ssn finder
            RunSsnFinder(false);

            // Ensure there were some clues added to the image
            ExtractException.Assert("ELI23409", "No clues where added!",
                _idShieldOfficeForm.ImageViewer.GetLayeredObjects(null, ArgumentRequirement.Any,
                new Type[] { typeof(Clue) }, ArgumentRequirement.Any,
                null, ArgumentRequirement.Any).Count > 0);

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that there is a zone node
            Assert.That(document.CreateNavigator().Select(@"//Clue//Zone").Count > 0);
        }

        /// <summary>
        /// Tests that the clue's zone node contains a start node.
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesCurrentObjectsClueZoneContainsStart()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Ensure there are no layer objects on the image already
            ExtractException.Assert("ELI23410", "Image contains layer objects already!",
                _idShieldOfficeForm.ImageViewer.LayerObjects.Count == 0);

            // Run the ssn finder
            RunSsnFinder(false);

            // Ensure there were some clues added to the image
            ExtractException.Assert("ELI23411", "No clues where added!",
                _idShieldOfficeForm.ImageViewer.GetLayeredObjects(null, ArgumentRequirement.Any,
                new Type[] { typeof(Clue) }, ArgumentRequirement.Any,
                null, ArgumentRequirement.Any).Count > 0);

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that there is a start node
            Assert.That(document.CreateNavigator().Select(@"//Clue//Zone//Start").Count > 0);
        }

        /// <summary>
        /// Tests that the clue's zone node contains a start node that contains
        /// an X and Y attribute.
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesCurrentObjectsClueZoneContainsStartWithXAndY()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Ensure there are no layer objects on the image already
            ExtractException.Assert("ELI23413", "Image contains layer objects already!",
                _idShieldOfficeForm.ImageViewer.LayerObjects.Count == 0);

            // Run the ssn finder
            RunSsnFinder(false);

            // Ensure there were some clues added to the image
            ExtractException.Assert("ELI23414", "No clues where added!",
                _idShieldOfficeForm.ImageViewer.GetLayeredObjects(null, ArgumentRequirement.Any,
                new Type[] { typeof(Clue) }, ArgumentRequirement.Any,
                null, ArgumentRequirement.Any).Count > 0);

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that the start node contains an X and Y attribute
            Assert.That(document.CreateNavigator().Select(@"//Clue//Zone//Start[@X and @Y]").Count > 0);
        }

        /// <summary>
        /// Tests that the clue's zone node contains a end node.
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesCurrentObjectsClueZoneContainsEnd()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Ensure there are no layer objects on the image already
            ExtractException.Assert("ELI23416", "Image contains layer objects already!",
                _idShieldOfficeForm.ImageViewer.LayerObjects.Count == 0);

            // Run the ssn finder
            RunSsnFinder(false);

            // Ensure there were some clues added to the image
            ExtractException.Assert("ELI23417", "No clues where added!",
                _idShieldOfficeForm.ImageViewer.GetLayeredObjects(null, ArgumentRequirement.Any,
                new Type[] { typeof(Clue) }, ArgumentRequirement.Any,
                null, ArgumentRequirement.Any).Count > 0);

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that there is an end node
            Assert.That(document.CreateNavigator().Select(@"//Clue//Zone//End").Count > 0);
        }

        /// <summary>
        /// Tests that the clue's zone node contains a end node that contains
        /// an X and Y attribute.
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesCurrentObjectsClueZoneContainsEndWithXAndY()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Ensure there are no layer objects on the image already
            ExtractException.Assert("ELI23419", "Image contains layer objects already!",
                _idShieldOfficeForm.ImageViewer.LayerObjects.Count == 0);

            // Run the ssn finder
            RunSsnFinder(false);

            // Ensure there were some clues added to the image
            ExtractException.Assert("ELI23420", "No clues where added!",
                _idShieldOfficeForm.ImageViewer.GetLayeredObjects(null, ArgumentRequirement.Any,
                new Type[] { typeof(Clue) }, ArgumentRequirement.Any,
                null, ArgumentRequirement.Any).Count > 0);

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that the end node contains an X and Y attribute
            Assert.That(document.CreateNavigator().Select(@"//Clue//Zone//End[@X and @Y]").Count > 0);
        }

        /// <summary>
        /// Tests that the clue's zone node contains a height node.
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesCurrentObjectsClueZoneContainsHeight()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Ensure there are no layer objects on the image already
            ExtractException.Assert("ELI23422", "Image contains layer objects already!",
                _idShieldOfficeForm.ImageViewer.LayerObjects.Count == 0);

            // Run the ssn finder
            RunSsnFinder(false);

            // Ensure there were some clues added to the image
            ExtractException.Assert("ELI23423", "No clues where added!",
                _idShieldOfficeForm.ImageViewer.GetLayeredObjects(null, ArgumentRequirement.Any,
                new Type[] { typeof(Clue) }, ArgumentRequirement.Any,
                null, ArgumentRequirement.Any).Count > 0);

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that there is a height node
            Assert.That(document.CreateNavigator().Select(@"//Clue//Zone//Height").Count > 0);
        }

        /// <summary>
        /// Tests that the clues node contains a color node.
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesCurrentObjectsClueContainsColor()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Ensure there are no layer objects on the image already
            ExtractException.Assert("ELI23425", "Image contains layer objects already!",
                _idShieldOfficeForm.ImageViewer.LayerObjects.Count == 0);

            // Run the ssn finder
            RunSsnFinder(false);

            // Ensure there were some clues added to the image
            ExtractException.Assert("ELI23426", "No clues where added!",
                _idShieldOfficeForm.ImageViewer.GetLayeredObjects(null, ArgumentRequirement.Any,
                new Type[] { typeof(Clue) }, ArgumentRequirement.Any,
                null, ArgumentRequirement.Any).Count > 0);

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that there is a color node
            Assert.That(document.CreateNavigator().Select(@"//Clue//Color").Count > 0);
        }

        /// <summary>
        /// Tests that the clue's color node contains red, green and blue.
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesCurrentObjectsClueColorContainsRedGreenBlue()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Ensure there are no layer objects on the image already
            ExtractException.Assert("ELI23427", "Image contains layer objects already!",
                _idShieldOfficeForm.ImageViewer.LayerObjects.Count == 0);

            // Run the ssn finder
            RunSsnFinder(false);

            // Ensure there were some clues added to the image
            ExtractException.Assert("ELI23428", "No clues where added!",
                _idShieldOfficeForm.ImageViewer.GetLayeredObjects(null, ArgumentRequirement.Any,
                new Type[] { typeof(Clue) }, ArgumentRequirement.Any,
                null, ArgumentRequirement.Any).Count > 0);

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that Color node contains red, green and blue
            Assert.That(document.CreateNavigator().Select(
                @"//Clue//Color[@Red and @Green and @Blue]").Count > 0);
        }

        /// <summary>
        /// Tests that the objects added node contains clues.
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesObjectsAddedContainsClues()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Ensure there are no layer objects on the image already
            ExtractException.Assert("ELI23430", "Image contains layer objects already!",
                _idShieldOfficeForm.ImageViewer.LayerObjects.Count == 0);

            // Run the ssn finder
            RunSsnFinder(false);

            // Ensure there were some clues added to the image
            ExtractException.Assert("ELI23431", "No clues where added!",
                _idShieldOfficeForm.ImageViewer.GetLayeredObjects(null, ArgumentRequirement.Any,
                new Type[] { typeof(Clue) }, ArgumentRequirement.Any,
                null, ArgumentRequirement.Any).Count > 0);

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that there are multiple clues in the ObjectsAdded section
            Assert.That(document.CreateNavigator().Select(
                @"//ObjectsAdded//Object[@Type='Clue']").Count > 1);
        }

        /// <summary>
        /// Tests that the the current objects node still contains clues even if they are
        /// not visible
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesStillContainsCluesWhenNotVisible()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Ensure there are no layer objects on the image already
            ExtractException.Assert("ELI23436", "Image contains layer objects already!",
                _idShieldOfficeForm.ImageViewer.LayerObjects.Count == 0);

            // Run the ssn finder
            RunSsnFinder(false);

            // Ensure there were some clues added to the image
            ExtractException.Assert("ELI23437", "No clues where added!",
                _idShieldOfficeForm.ImageViewer.GetLayeredObjects(null, ArgumentRequirement.Any,
                new Type[] { typeof(Clue) }, ArgumentRequirement.Any,
                null, ArgumentRequirement.Any).Count > 0);

            // Ensure that the clues are currently visible
            ExtractException.Assert("ELI23438", "Clues are not visible!",
                _idShieldOfficeForm.ImageViewer.GetLayeredObjects(null, ArgumentRequirement.Any,
                new Type[] { typeof(Clue) }, ArgumentRequirement.Any,
                null, ArgumentRequirement.Any)[0].Visible);

            // Hide the clues
            _idShieldOfficeForm.ImageViewer.SetVisibleStateForSpecifiedLayerObjects(null,
                new Type[] { typeof(Clue) }, false);

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that there are still Clue objects in the current objects
            Assert.That(document.CreateNavigator().Select(@"//CurrentObjects//Clue").Count > 0);
        }

        // TODO: Add test for Text node for clue when DNRCAU #293 is fixed

        #endregion IDSO File Data File Contents For Clues

        #region IDSO File Data File Contents For Bates Number

        /// <summary>
        /// Tests that the Bates number object has the appropriate tag
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesBatesNumberContainsAppropriateTag()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Apply Bates numbers to the image
            _idShieldOfficeForm.ApplyBatesNumbers();

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that all Bates numbers have the appropriate tag
            Assert.That(document.CreateNavigator().Select(
                @"//CurrentObjects//TextLayerObject//Tags[Tag='Bates']").Count > 0);
        }

        /// <summary>
        /// Tests that the Bates number object has the appropriate comment
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesBatesNumberContainsAppropriateComment()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Apply Bates numbers to the image
            _idShieldOfficeForm.ApplyBatesNumbers();

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Get a node iterator for all bates number nodes
            XPathNodeIterator nodeIterator = document.CreateNavigator().Select(
                @"//CurrentObjects//TextLayerObject//Tags[Tag='Bates']");

            // Check that all Bates numbers have the appropriate comment
            Assert.That(nodeIterator.Current.Select(
                @"//CurrentObjects//TextLayerObject[Comment='Bates number']").Count == nodeIterator.Count);
        }

        /// <summary>
        /// Tests that the first Bates number object has a next object Id node.
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesFirstBatesNumberDoesContainNextObjectId()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Apply Bates numbers to the image
            _idShieldOfficeForm.ApplyBatesNumbers();

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that the first Bates number has a next object Id
            Assert.That(document.CreateNavigator().Select(
                @"//CurrentObjects//TextLayerObject[Page='1' and Tags/Tag='Bates']/NextObjectId").Count == 1);
        }

        /// <summary>
        /// Tests that the first Bates number object does not have a previous object Id node.
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesFirstBatesNumberDoesNotContainPreviousObjectId()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Apply Bates numbers to the image
            _idShieldOfficeForm.ApplyBatesNumbers();

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that the first Bates number does not have a previous object Id
            Assert.That(document.CreateNavigator().Select(
                @"//CurrentObjects//TextLayerObject[Page='1' and Tags/Tag='Bates']/PreviousObjectId").Count == 0);
        }

        /// <summary>
        /// Tests that the second Bates number object has a next object Id node.
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesSecondBatesNumberDoesContainNextObjectId()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Apply Bates numbers to the image
            _idShieldOfficeForm.ApplyBatesNumbers();

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that the second Bates number has a next object Id
            Assert.That(document.CreateNavigator().Select(
                @"//CurrentObjects//TextLayerObject[Page='2' and Tags/Tag='Bates']/NextObjectId").Count == 1);
        }

        /// <summary>
        /// Tests that the second Bates number object has a previous object Id node.
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesSecondBatesNumberDoesContainPreviousObjectId()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Apply Bates numbers to the image
            _idShieldOfficeForm.ApplyBatesNumbers();

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that the second Bates number has a previous object Id
            Assert.That(document.CreateNavigator().Select(
                @"//CurrentObjects//TextLayerObject[Page='2' and Tags/Tag='Bates']/PreviousObjectId").Count == 1);
        }

        /// <summary>
        /// Tests that the third Bates number object does not have a next object Id node.
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesThirdBatesNumberDoesNotContainNextObjectId()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Apply Bates numbers to the image
            _idShieldOfficeForm.ApplyBatesNumbers();

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that the third Bates number does not have a next object Id
            Assert.That(document.CreateNavigator().Select(
                @"//CurrentObjects//TextLayerObject[Page='3' and Tags/Tag='Bates']/NextObjectId").Count == 0);
        }

        /// <summary>
        /// Tests that the third Bates number object has a previous object Id node.
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesThirdBatesNumberDoesContainPreviousObjectId()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Apply Bates numbers to the image
            _idShieldOfficeForm.ApplyBatesNumbers();

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that the third Bates number has a previous object Id
            Assert.That(document.CreateNavigator().Select(
                @"//CurrentObjects//TextLayerObject[Page='3' and Tags/Tag='Bates']/PreviousObjectId").Count == 1);
        }

        /// <summary>
        /// Tests that the Bates number object has a non-empty font node
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesBatesNumberContainsFont()
        {
            // Get the current count of layer objects
            int layerObjectCount = _idShieldOfficeForm.ImageViewer.LayerObjects.Count;

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Apply Bates numbers to the image
            _idShieldOfficeForm.ApplyBatesNumbers();

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that each Bates number added has a Font node
            Assert.That(document.CreateNavigator().Select(
                @"//CurrentObjects//TextLayerObject[Tags/Tag='Bates']/Font[@Family and @Style and @Size]").Count == 
                _idShieldOfficeForm.ImageViewer.LayerObjects.Count - layerObjectCount);
        }

        /// <summary>
        /// Tests that the Bates number object has a text node
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesBatesNumberContainsText()
        {
            // Get the current count of layer objects
            int layerObjectCount = _idShieldOfficeForm.ImageViewer.LayerObjects.Count;

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Apply Bates numbers to the image
            _idShieldOfficeForm.ApplyBatesNumbers();

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that each Bates number added has a Text node
            Assert.That(document.CreateNavigator().Select(
                @"//CurrentObjects//TextLayerObject[Tags/Tag='Bates']/Text").Count == 
                _idShieldOfficeForm.ImageViewer.LayerObjects.Count - layerObjectCount);
        }

        /// <summary>
        /// Tests that the Bates number object has a non-empty text node
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesBatesNumberTextIsNotEmpty()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Apply Bates numbers to the image
            _idShieldOfficeForm.ApplyBatesNumbers();

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Get the text node
            XPathNodeIterator nodeIterator = document.CreateNavigator().Select(
                @"//CurrentObjects//TextLayerObject[1 and Tags/Tag='Bates']/Text");

            // Navigate to the text node
            while (nodeIterator.Current.Name != "Text")
            {
                ExtractException.Assert("ELI23458", "Could not find text node!",
                    nodeIterator.MoveNext());
            }

            // Check that the text node is not empty
            Assert.That(!nodeIterator.Current.IsEmptyElement);
        }

        /// <summary>
        /// Tests that the Bates number object has a page node
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesBatesNumberContainsPage()
        {
            // Get the current count of layer objects
            int layerObjectCount = _idShieldOfficeForm.ImageViewer.LayerObjects.Count;

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Apply Bates numbers to the image
            _idShieldOfficeForm.ApplyBatesNumbers();

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that each Bates number added has a Text node
            Assert.That(document.CreateNavigator().Select(
                @"//CurrentObjects//TextLayerObject[Tags/Tag='Bates']/Page").Count == 
                _idShieldOfficeForm.ImageViewer.LayerObjects.Count - layerObjectCount);
        }

        /// <summary>
        /// Tests that the Bates number object has a non-empty page node
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesBatesNumberPageIsNotEmpty()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Apply Bates numbers to the image
            _idShieldOfficeForm.ApplyBatesNumbers();

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Get the page node
            XPathNodeIterator nodeIterator = document.CreateNavigator().Select(
                @"//CurrentObjects//TextLayerObject[1 and Tags/Tag='Bates']/Page");

            // Navigate to the page node
            while (nodeIterator.Current.Name != "Page")
            {
                ExtractException.Assert("ELI23459", "Could not find page node!",
                    nodeIterator.MoveNext());
            }

            // Check that the page node is not empty
            Assert.That(!nodeIterator.Current.IsEmptyElement);
        }

        /// <summary>
        /// Tests that the Bates number object has an anchor position node
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesBatesNumberContainsAnchorPosition()
        {
            // Get the current count of layer objects
            int layerObjectCount = _idShieldOfficeForm.ImageViewer.LayerObjects.Count;

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Apply Bates numbers to the image
            _idShieldOfficeForm.ApplyBatesNumbers();

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that each Bates number added has an anchor position node
            Assert.That(document.CreateNavigator().Select(
                @"//CurrentObjects//TextLayerObject[Tags/Tag='Bates']/AnchorPosition[@X and @Y]").Count == 
                _idShieldOfficeForm.ImageViewer.LayerObjects.Count - layerObjectCount);
        }

        /// <summary>
        /// Tests that the Bates number object has a content alignment node
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesBatesNumberContainsContentAlignment()
        {
            // Get the current count of layer objects
            int layerObjectCount = _idShieldOfficeForm.ImageViewer.LayerObjects.Count;

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Apply Bates numbers to the image
            _idShieldOfficeForm.ApplyBatesNumbers();

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that each Bates number added has a content alignment node
            Assert.That(document.CreateNavigator().Select(
                @"//CurrentObjects//TextLayerObject[Tags/Tag='Bates']/ContentAlignment").Count == 
                _idShieldOfficeForm.ImageViewer.LayerObjects.Count - layerObjectCount);
        }

        /// <summary>
        /// Tests that the Bates number object has a non-empty content alignment node
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesBatesNumberContentAlignmentIsNotEmpty()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Apply Bates numbers to the image
            _idShieldOfficeForm.ApplyBatesNumbers();

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Get the content alignment node
            XPathNodeIterator nodeIterator = document.CreateNavigator().Select(
                @"//CurrentObjects//TextLayerObject[1 and Tags/Tag='Bates']/ContentAlignment");

            // Navigate to the content alignment node
            while (nodeIterator.Current.Name != "ContentAlignment")
            {
                ExtractException.Assert("ELI23460", "Could not find ContentAlignment node!",
                    nodeIterator.MoveNext());
            }

            // Check that the content alignment node is not empty
            Assert.That(!nodeIterator.Current.IsEmptyElement);
        }

        /// <summary>
        /// Tests that the session node contains an object added for each bates number
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesSessionNodeContainsObjectAddedForEachBatesNumber()
        {
            // Get the current count of layer objects
            int layerObjectCount = _idShieldOfficeForm.ImageViewer.LayerObjects.Count;

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Apply Bates numbers to the image
            _idShieldOfficeForm.ApplyBatesNumbers();

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that each Bates number added has an objects added node
            Assert.That(document.CreateNavigator().Select(
                @"//Sessions/Session[@Id='1']/ObjectsAdded/Object[@Type='TextLayerObject']").Count ==
                _idShieldOfficeForm.ImageViewer.LayerObjects.Count - layerObjectCount);
        }

        /// <summary>
        /// Tests that the session node contains an object deleted for each bates number deleted
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesSessionNodeContainsObjectDeletedForEachBatesNumber()
        {
            try
            {
                // Open the test image
                OpenTestImage(_idShieldOfficeForm.ImageViewer);

                // Apply Bates numbers to the image
                _idShieldOfficeForm.ApplyBatesNumbers();

                // Save the changes to the idso file
                _idShieldOfficeForm.SaveImage("", OutputFormat.Idso);

                // Close the image
                _idShieldOfficeForm.ImageViewer.CloseImage();

                // Open the image again
                OpenTestImage(_idShieldOfficeForm.ImageViewer);

                // Get the bates numbers
                LayerObject[] batesNumbers = _idShieldOfficeForm.ImageViewer.GetLayeredObjects(
                    new string[] { "Bates" }, ArgumentRequirement.Any).ToArray();

                // Get the count of bates numbers
                int batesCount = batesNumbers.Length;

                // Delete all the Bates numbers
                _idShieldOfficeForm.ImageViewer.LayerObjects.Remove(batesNumbers);

                // Get the XML document from the modification history
                XmlDocument document = new XmlDocument();
                document.LoadXml(GetModificationHistory().ToXmlString());

                // Check that each Bates number deleted has an objects deleted entry
                Assert.That(document.CreateNavigator().Select(
                    @"//Sessions/Session[@Id='2']/ObjectsDeleted/Object[@Type='TextLayerObject']").Count ==
                    batesCount);
            }
            finally
            {
                // Clean up the idso file that was created
                string idsoFile = _idShieldOfficeForm.ImageViewer.ImageFile + ".idso";
                if (File.Exists(idsoFile))
                {
                    try
                    {
                        File.Delete(idsoFile);
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Tests that the session node contains an object modified for each bates number modified
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesSessionNodeContainsObjectModifiedForEachBatesNumber()
        {
            try
            {
                // Open the test image
                OpenTestImage(_idShieldOfficeForm.ImageViewer);

                // Apply Bates numbers to the image
                _idShieldOfficeForm.ApplyBatesNumbers();

                // Save the changes to the idso file
                _idShieldOfficeForm.SaveImage("", OutputFormat.Idso);

                // Close the image
                _idShieldOfficeForm.ImageViewer.CloseImage();

                // Open the image again
                OpenTestImage(_idShieldOfficeForm.ImageViewer);

                // Get the bates numbers
                LayerObject[] batesNumbers = _idShieldOfficeForm.ImageViewer.GetLayeredObjects(
                    new string[] { "Bates" }, ArgumentRequirement.Any).ToArray();

                // Get the count of bates numbers
                int batesCount = batesNumbers.Length;

                foreach (LayerObject layerObject in batesNumbers)
                {
                    layerObject.Offset(1, 1);
                }

                // Get the XML document from the modification history
                XmlDocument document = new XmlDocument();
                document.LoadXml(GetModificationHistory().ToXmlString());

                // Check that each Bates number modified has an objects modified entry
                Assert.That(document.CreateNavigator().Select(
                    @"//Sessions/Session[@Id='2']/ObjectsModified/Object[@Type='TextLayerObject']").Count ==
                    batesCount);
            }
            finally
            {
                // Clean up the idso file that was created
                string idsoFile = _idShieldOfficeForm.ImageViewer.ImageFile + ".idso";
                if (File.Exists(idsoFile))
                {
                    try
                    {
                        File.Delete(idsoFile);
                    }
                    catch
                    {
                    }
                }
            }
        }

        #endregion IDSO File Data File Contents For Bates Number

        #region IDSO File Data File Contents For Redaction

        /// <summary>
        /// Tests that the Redaction object has the appropriate nodes
        /// </summary>
        /// <param name="xmlPath">The XPath query for the node.</param>
        [TestCase(@"//CurrentObjects//Redaction[@Id]",
            TestName="Automated_IdsoPropertiesRedactionNodeContainsIdAttribute",
            Description="Test that the redaction object node contains an Id attribute")]
        [TestCase(@"//CurrentObjects//Redaction/Page",
            TestName="Automated_IdsoPropertiesRedactionContainsPage",
            Description="Test that the redaction object node contains a page node")]
        [TestCase(@"//CurrentObjects//Redaction/Comment",
            TestName="Automated_IdsoPropertiesRedactionContainsComment",
            Description="Test that the redaction object node contains a comment node")]
        [TestCase(@"//CurrentObjects//Redaction/Tags",
            TestName="Automated_IdsoPropertiesRedactionContainsTags",
            Description="Test that the redaction object node contains a tags node")]
        [TestCase(@"//CurrentObjects//Redaction/Zone",
            TestName="Automated_IdsoPropertiesRedactionContainsZone",
            Description="Test that the redaction object node contains a zone node")]
        [TestCase(@"//CurrentObjects//Redaction/Zone/Start[@X and @Y]",
            TestName="Automated_IdsoPropertiesRedactionZoneContainsStart",
            Description="Test that the redaction zone node contains a start node")]
        [TestCase(@"//CurrentObjects//Redaction/Zone/End[@X and @Y]",
            TestName="Automated_IdsoPropertiesRedactionZoneContainsEnd",
            Description="Test that the redaction zone node contains an end node")]
        [TestCase(@"//CurrentObjects//Redaction/Zone/Height",
            TestName="Automated_IdsoPropertiesRedactionZoneContainsHeight",
            Description="Test that the redaction zone node contains a height node")]
        [TestCase(@"//CurrentObjects//Redaction/Color",
            TestName="Automated_IdsoPropertiesRedactionContainsColor",
            Description="Test that the redaction object node contains a color node")]
        [Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesRedactionNodes(string xmlPath)
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Perform a redact all using the SSN finder
            RunSsnFinder(true);

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that the appropriate nodes are found
            Assert.That(document.CreateNavigator().Select(xmlPath).Count > 0);
        }

        /// <summary>
        /// Tests that the specified redaction nodes are not empty.
        /// </summary>
        /// <param name="xmlPath">The XPath query for the node.</param>
        /// <param name="nodeName">The name of the node to check.</param>
        [TestCase(@"//CurrentObjects//Redaction/Page", "Page",
            TestName="Automated_IdsoPropertiesRedactionPageIsNotEmpty",
            Description="Test that the redaction object node contains a non-empty page node")]
        [TestCase(@"//CurrentObjects//Redaction/Comment", "Comment",
            TestName="Automated_IdsoPropertiesRedactionCommentIsNotEmpty",
            Description="Test that the redaction object node contains a non-empty comment node")]
        [TestCase(@"//CurrentObjects//Redaction/Zone", "Zone",
            TestName="Automated_IdsoPropertiesRedactionZoneIsNotEmpty",
            Description="Test that the redaction object node contains a non-empty zone node")]
        [TestCase(@"//CurrentObjects//Redaction/Zone/Height", "Height",
            TestName="Automated_IdsoPropertiesRedactionZoneHeightIsNotEmpty",
            Description="Test that the redaction zone node contains a non-empty height node")]
        [TestCase(@"//CurrentObjects//Redaction/Color", "Color",
            TestName="Automated_IdsoPropertiesRedactionColorIsNotEmpty",
            Description="Test that the redaction object node contains a non-empty color node")]
        [Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesRedactionNodesNotEmpty(string xmlPath, string nodeName)
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Perform a redact all using the SSN finder
            RunSsnFinder(true);

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Get the node specified in the xmlPath
            XPathNodeIterator nodeIterator = document.CreateNavigator().Select(xmlPath);

            // Navigate to the appropriate node
            while (nodeIterator.Current.Name != nodeName)
            {
                ExtractException.Assert("ELI23461", "Could not find " + nodeName + " node!",
                    nodeIterator.MoveNext());
            }

            // Check that the node is not empty
            Assert.That(!nodeIterator.Current.IsEmptyElement);
        }

        /// <summary>
        /// Tests that the redaction object does not have a text node
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesRedactionHasNoTextNode()
        {
            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Perform a redact all using the SSN finder
            RunSsnFinder(true);

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that no redaction contains a text node
            Assert.That(document.CreateNavigator().Select(
                @"//CurrentObjects//Redaction/Text").Count == 0);
        }

        /// <summary>
        /// Tests that the session node contains an object added for each bates number
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesSessionNodeContainsObjectAddedForEachRedaction()
        {
            // Get the current count of layer objects
            int layerObjectCount = _idShieldOfficeForm.ImageViewer.LayerObjects.Count;

            // Open the test image
            OpenTestImage(_idShieldOfficeForm.ImageViewer);

            // Redact square brackets with bracketed text finder
            RunBracketedTextFinder(true, false, false);

            // Get the XML document from the modification history
            XmlDocument document = new XmlDocument();
            document.LoadXml(GetModificationHistory().ToXmlString());

            // Check that each Redaction added has an objects added node
            Assert.That(document.CreateNavigator().Select(
                @"//Sessions/Session[@Id='1']/ObjectsAdded/Object[@Type='Redaction']").Count ==
                _idShieldOfficeForm.ImageViewer.LayerObjects.Count - layerObjectCount);
        }

        /// <summary>
        /// Tests that the session node contains an object deleted for each redaction deleted
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesSessionNodeContainsObjectDeletedForEachRedaction()
        {
            try
            {
                // Open the test image
                OpenTestImage(_idShieldOfficeForm.ImageViewer);

                // Redact square brackets with bracketed text finder
                RunBracketedTextFinder(true, false, false);

                // Save the changes to the idso file
                _idShieldOfficeForm.SaveImage("", OutputFormat.Idso);

                // Close the image
                _idShieldOfficeForm.ImageViewer.CloseImage();

                // Open the image again
                OpenTestImage(_idShieldOfficeForm.ImageViewer);

                // Get the redactions
                LayerObject[] redactions = _idShieldOfficeForm.ImageViewer.GetLayeredObjects(
                    null, ArgumentRequirement.Any,
                    new Type[] { typeof(Redaction) }, ArgumentRequirement.Any,
                    null, ArgumentRequirement.Any).ToArray();

                // Get the count of redactions
                int redactionCount = redactions.Length;

                // Delete all the redactions
                _idShieldOfficeForm.ImageViewer.LayerObjects.Remove(redactions);

                // Get the XML document from the modification history
                XmlDocument document = new XmlDocument();
                document.LoadXml(GetModificationHistory().ToXmlString());

                // Check that each redaction deleted has an objects deleted entry
                Assert.That(document.CreateNavigator().Select(
                    @"//Sessions/Session[@Id='2']/ObjectsDeleted/Object[@Type='Redaction']").Count ==
                    redactionCount);
            }
            finally
            {
                // Clean up the idso file that was created
                string idsoFile = _idShieldOfficeForm.ImageViewer.ImageFile + ".idso";
                if (File.Exists(idsoFile))
                {
                    try
                    {
                        File.Delete(idsoFile);
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Tests that the session node contains an object modified for each redaction modified
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesSessionNodeContainsObjectModifiedForEachRedaction()
        {
            try
            {
                // Open the test image
                OpenTestImage(_idShieldOfficeForm.ImageViewer);

                // Redact square brackets with bracketed text finder
                RunBracketedTextFinder(true, false, false);

                // Save the changes to the idso file
                _idShieldOfficeForm.SaveImage("", OutputFormat.Idso);

                // Close the image
                _idShieldOfficeForm.ImageViewer.CloseImage();

                // Open the image again
                OpenTestImage(_idShieldOfficeForm.ImageViewer);

                // Get the redactions
                LayerObject[] redactions = _idShieldOfficeForm.ImageViewer.GetLayeredObjects(
                    null, ArgumentRequirement.Any,
                    new Type[] { typeof(Redaction) }, ArgumentRequirement.Any,
                    null, ArgumentRequirement.Any).ToArray();

                // Get the count of redactions
                int redactionCount = redactions.Length;

                // Move each redaction slightly
                foreach (LayerObject layerObject in redactions)
                {
                    layerObject.Offset(1, 1);
                }

                // Get the XML document from the modification history
                XmlDocument document = new XmlDocument();
                document.LoadXml(GetModificationHistory().ToXmlString());

                // Check that each redaction modified has an objects modified entry
                Assert.That(document.CreateNavigator().Select(
                    @"//Sessions/Session[@Id='2']/ObjectsModified/Object[@Type='Redaction']").Count ==
                    redactionCount);
            }
            finally
            {
                // Clean up the idso file that was created
                string idsoFile = _idShieldOfficeForm.ImageViewer.ImageFile + ".idso";
                if (File.Exists(idsoFile))
                {
                    try
                    {
                        File.Delete(idsoFile);
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Tests that the redaction nodes do not contain sensitive data.
        /// (Actually checks that the sensitive data is not contained within
        /// the IDSO file at all).
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesRedactionObjectDoesNotContainSensitiveData()
        {
            // Get a temporary bitmap file
            string tempImageFile = FileSystemMethods.GetTemporaryFileName("bmp");
            Bitmap image = null;
            try
            {
                // Create the strings of sensitive text
                string[] sensitiveText = new string[] { "333 - 22 - 4444",
                    "[This text should not be found in the IDSO file]"};
                // Create a new image
                image = new Bitmap(2000, 2500, PixelFormat.Format24bppRgb);

                // Paint the image white and draw the appropriate text on it
                using (Graphics graphics = Graphics.FromImage(image))
                {
                    // Paint the image white
                    GraphicsUnit pixel = GraphicsUnit.Pixel;
                    graphics.FillRectangle(Brushes.White,
                        Rectangle.Round(image.GetBounds(ref pixel)));

                    // Draw the sensitive text on the image
                    using (Font font = new Font(FontFamily.GenericMonospace, 32.0F))
                    {
                        PointF location = new PointF(20, 20);
                        foreach (string text in sensitiveText)
                        {
                            graphics.DrawString(text, font, Brushes.Black, location);
                            location.Y = location.Y + 300;
                        }
                    }
                }

                // Save the image to the temp file
                image.Save(tempImageFile);

                // Open the temp image in the image viewer
                _idShieldOfficeForm.ImageViewer.OpenImage(tempImageFile, false);

                // Run the ssn finder and the bracketed text finder on the image
                RunBracketedTextFinder(true, false, false);
                RunSsnFinder(true);

                // Ensure that redactions were added
                ExtractException.Assert("ELI23466", "No redactions added!",
                    _idShieldOfficeForm.ImageViewer.GetLayeredObjects(null, ArgumentRequirement.Any,
                    new Type[] { typeof(Redaction) }, ArgumentRequirement.Any,
                    null, ArgumentRequirement.Any).Count > 0);

                // Ensure the sensitive text is not found in the IDSO file
                string xml = GetModificationHistory().ToXmlString();

                bool noSensitiveData = true;
                foreach (string text in sensitiveText)
                {
                    noSensitiveData = noSensitiveData && !xml.Contains(text);
                }

                // Check that no sensitive data was found in the xml string
                Assert.That(noSensitiveData);
            }
            finally
            {
                // Dispose of the image
                if (image != null)
                {
                    image.Dispose();
                    image = null;
                }

                // If there is still an image open in the image viewer, close it
                if (_idShieldOfficeForm.ImageViewer.IsImageAvailable)
                {
                    // Save the image to clear the dirty flag
                    _idShieldOfficeForm.SaveImage("", OutputFormat.Idso);

                    // Delete the IDSO file that was just created
                    if (File.Exists(_idShieldOfficeForm.ImageViewer.ImageFile + ".idso"))
                    {
                        try
                        {
                            File.Delete(_idShieldOfficeForm.ImageViewer.ImageFile + ".idso");
                        }
                        catch
                        {
                        }
                    }

                    // Close the image
                    _idShieldOfficeForm.ImageViewer.CloseImage();
                }

                // If the temporary file exists, delete it
                if (File.Exists(tempImageFile))
                {
                    try
                    {
                        File.Delete(tempImageFile);
                    }
                    catch
                    {
                    }
                }
            }
        }

        #endregion IDSO File Data File Contents For Redaction

        #region IDSO File Data File Contents For Historical Objects

        /// <summary>
        /// Tests that the historical object node contains an entry for each redaction deleted
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesHistoricalObjectsNodeContainsRedactionForEachRedactionDeleted()
        {
            try
            {
                // Open the test image
                OpenTestImage(_idShieldOfficeForm.ImageViewer);

                // Redact square brackets with bracketed text finder
                RunBracketedTextFinder(true, false, false);

                // Save the changes to the idso file
                _idShieldOfficeForm.SaveImage("", OutputFormat.Idso);

                // Close the image
                _idShieldOfficeForm.ImageViewer.CloseImage();

                // Open the image again
                OpenTestImage(_idShieldOfficeForm.ImageViewer);

                // Get the redactions
                LayerObject[] redactions = _idShieldOfficeForm.ImageViewer.GetLayeredObjects(
                    null, ArgumentRequirement.Any,
                    new Type[] { typeof(Redaction) }, ArgumentRequirement.Any,
                    null, ArgumentRequirement.Any).ToArray();

                // Get the count of redactions
                int redactionCount = redactions.Length;

                // Delete all the redactions
                _idShieldOfficeForm.ImageViewer.LayerObjects.Remove(redactions);

                // Get the XML document from the modification history
                XmlDocument document = new XmlDocument();
                document.LoadXml(GetModificationHistory().ToXmlString());

                // Check that each redaction deleted has an historical objects entry
                Assert.That(document.CreateNavigator().Select(
                    @"//HistoricalObjects/Redaction").Count ==
                    redactionCount);
            }
            finally
            {
                // Clean up the idso file that was created
                string idsoFile = _idShieldOfficeForm.ImageViewer.ImageFile + ".idso";
                if (File.Exists(idsoFile))
                {
                    try
                    {
                        File.Delete(idsoFile);
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Tests that the historical object node contains an entry for each redaction moved
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesHistoricalObjectsNodeContainsRedactionForEachRedactionMoved()
        {
            try
            {
                // Open the test image
                OpenTestImage(_idShieldOfficeForm.ImageViewer);

                // Redact square brackets with bracketed text finder
                RunBracketedTextFinder(true, false, false);

                // Save the changes to the idso file
                _idShieldOfficeForm.SaveImage("", OutputFormat.Idso);

                // Close the image
                _idShieldOfficeForm.ImageViewer.CloseImage();

                // Open the image again
                OpenTestImage(_idShieldOfficeForm.ImageViewer);

                // Get the redactions
                LayerObject[] redactions = _idShieldOfficeForm.ImageViewer.GetLayeredObjects(
                    null, ArgumentRequirement.Any,
                    new Type[] { typeof(Redaction) }, ArgumentRequirement.Any,
                    null, ArgumentRequirement.Any).ToArray();

                // Get the count of redactions
                int redactionCount = redactions.Length;

                // Move each redaction
                foreach (LayerObject layerObject in redactions)
                {
                    layerObject.Offset(1, 1);
                }

                // Get the XML document from the modification history
                XmlDocument document = new XmlDocument();
                document.LoadXml(GetModificationHistory().ToXmlString());

                // Check that each redaction moved has an historical objects entry
                Assert.That(document.CreateNavigator().Select(
                    @"//HistoricalObjects/Redaction").Count ==
                    redactionCount);
            }
            finally
            {
                // Clean up the idso file that was created
                string idsoFile = _idShieldOfficeForm.ImageViewer.ImageFile + ".idso";
                if (File.Exists(idsoFile))
                {
                    try
                    {
                        File.Delete(idsoFile);
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Tests that the historical object node contains an entry for each Bates number deleted
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesHistoricalObjectsNodeContainsBatesNumberForEachBatesNumberDeleted()
        {
            try
            {
                // Open the test image
                OpenTestImage(_idShieldOfficeForm.ImageViewer);

                // Apply Bates numbers
                _idShieldOfficeForm.ApplyBatesNumbers();

                // Save the changes to the idso file
                _idShieldOfficeForm.SaveImage("", OutputFormat.Idso);

                // Close the image
                _idShieldOfficeForm.ImageViewer.CloseImage();

                // Open the image again
                OpenTestImage(_idShieldOfficeForm.ImageViewer);

                // Get the Bates numbers
                LayerObject[] batesNumbers = _idShieldOfficeForm.ImageViewer.GetLayeredObjects(
                    new string[] { "Bates" } , ArgumentRequirement.Any,
                    new Type[] { typeof(TextLayerObject) }, ArgumentRequirement.Any,
                    null, ArgumentRequirement.Any).ToArray();

                // Get the count of Bates numbers
                int batesCount = batesNumbers.Length;

                // Delete all the Bates numbers
                _idShieldOfficeForm.ImageViewer.LayerObjects.Remove(batesNumbers);

                // Get the XML document from the modification history
                XmlDocument document = new XmlDocument();
                document.LoadXml(GetModificationHistory().ToXmlString());

                // Check that each Bates numbers deleted has a historical objects entry
                Assert.That(document.CreateNavigator().Select(
                    @"//HistoricalObjects/TextLayerObject").Count ==
                    batesCount);
            }
            finally
            {
                // Clean up the idso file that was created
                string idsoFile = _idShieldOfficeForm.ImageViewer.ImageFile + ".idso";
                if (File.Exists(idsoFile))
                {
                    try
                    {
                        File.Delete(idsoFile);
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Tests that the historical object node contains an entry for each Bates number moved
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesHistoricalObjectsNodeContainsBatesNumberForEachBatesNumberMoved()
        {
            try
            {
                // Open the test image
                OpenTestImage(_idShieldOfficeForm.ImageViewer);

                // Apply Bates numbers
                _idShieldOfficeForm.ApplyBatesNumbers();

                // Save the changes to the idso file
                _idShieldOfficeForm.SaveImage("", OutputFormat.Idso);

                // Close the image
                _idShieldOfficeForm.ImageViewer.CloseImage();

                // Open the image again
                OpenTestImage(_idShieldOfficeForm.ImageViewer);

                // Get the Bates numbers
                LayerObject[] batesNumbers = _idShieldOfficeForm.ImageViewer.GetLayeredObjects(
                    new string[] { "Bates" } , ArgumentRequirement.Any,
                    new Type[] { typeof(TextLayerObject) }, ArgumentRequirement.Any,
                    null, ArgumentRequirement.Any).ToArray();

                // Get the count of Bates numbers
                int batesCount = batesNumbers.Length;

                // Move each Bates number
                foreach (LayerObject layerObject in batesNumbers)
                {
                    layerObject.Offset(1, 1);
                }

                // Get the XML document from the modification history
                XmlDocument document = new XmlDocument();
                document.LoadXml(GetModificationHistory().ToXmlString());

                // Check that each Bates number moved has an historical objects entry
                Assert.That(document.CreateNavigator().Select(
                    @"//HistoricalObjects/TextLayerObject").Count ==
                    batesCount);
            }
            finally
            {
                // Clean up the idso file that was created
                string idsoFile = _idShieldOfficeForm.ImageViewer.ImageFile + ".idso";
                if (File.Exists(idsoFile))
                {
                    try
                    {
                        File.Delete(idsoFile);
                    }
                    catch
                    {
                    }
                }
            }
        }

        #endregion IDSO File Data File Contents For Historical Objects

        #region IDSO File Data File Contents For Multiple Sessions

        /// <summary>
        /// Tests that the second session node contains the appropriate nodes.
        /// </summary>
        [TestCase(@"//Sessions/Session[@Id='2']/ObjectsDeleted/Object[@Type='Redaction']", 0,
            TestName="Automated_IdsoPropertiesSecondSessionObjectsDeletedContainsDeletedRedactions",
            Description="Test that the second sessions objects deleted node contains redactions")]
        [TestCase(@"//Sessions/Session[@Id='2']/ObjectsModified/Object[@Type='TextLayerObject']", 1,
            TestName="Automated_IdsoPropertiesSecondSessionObjectsModifiedContainsMovedBatesNumbers",
            Description="Test that the second sessions objects modified node contains Bates numbers")]
        [TestCase(@"//Sessions/Session[@Id='2']/ObjectsAdded/Object[@Type='Clue']", 2,
            TestName="Automated_IdsoPropertiesSecondSessionObjectsAddedContainsClues",
            Description="Test that the second sessions objects added node contains clues")]
        [Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesSecondSessionNodeContainsObjectsNodes(
            string xmlPath, int typeToCount)
        {
            try
            {
                //-----------------------------------------------------------------------
                // First session - Add bates numbers and redactions
                //-----------------------------------------------------------------------

                // Open the test image
                OpenTestImage(_idShieldOfficeForm.ImageViewer);

                // Apply Bates numbers to the image
                _idShieldOfficeForm.ApplyBatesNumbers();

                // Redact square brackets with bracketed text finder
                RunBracketedTextFinder(true, false, false);

                // Save the changes to the idso file
                _idShieldOfficeForm.SaveImage("", OutputFormat.Idso);

                // Close the image
                _idShieldOfficeForm.ImageViewer.CloseImage();

                //-----------------------------------------------------------------------
                // Second session - Add clues, move bates numbers, and delete redactions
                //-----------------------------------------------------------------------

                // Open the image again
                OpenTestImage(_idShieldOfficeForm.ImageViewer);

                // Run the SSN finder to add clues
                RunSsnFinder(false);

                // Get the redactions, bates numbers and clues from the image viewer
                List<Redaction> redactions = new List<Redaction>();
                List<TextLayerObject> batesNumbers = new List<TextLayerObject>();
                List<Clue> clues = new List<Clue>();
                foreach(LayerObject layerObject in _idShieldOfficeForm.ImageViewer.LayerObjects)
                {
                    // Check if the object is a redaction
                    Redaction redaction = layerObject as Redaction;
                    if (redaction != null)
                    {
                        redactions.Add(redaction);
                        continue;
                    }

                    // Check if the object is a bates number
                    TextLayerObject batesNumber = layerObject as TextLayerObject;
                    if (batesNumber != null)
                    {
                        if (batesNumber.Tags.Contains("Bates"))
                        {
                            batesNumbers.Add(batesNumber);
                            continue;
                        }
                    }

                    // Check if the object is a clue
                    Clue clue = layerObject as Clue;
                    if (clue != null)
                    {
                        clues.Add(clue);
                        continue;
                    }
                }

                // Get the appropriate count
                int count = 0;
                switch (typeToCount)
                {
                    case 0:
                        count = redactions.Count;
                        break;

                    case 1:
                        count = batesNumbers.Count;
                        break;

                    case 2:
                        count = clues.Count;
                        break;

                    default:
                        ExtractException.ThrowLogicException("ELI23462");
                        break;
                }

                // Move the bates numbers
                foreach(TextLayerObject batesNumber in batesNumbers)
                {
                    batesNumber.Offset(-5, 5);
                }

                // Delete the redactions
                _idShieldOfficeForm.ImageViewer.LayerObjects.Remove(redactions.ToArray());

                // Get the XML document from the modification history
                XmlDocument document = new XmlDocument();
                document.LoadXml(GetModificationHistory().ToXmlString());

                // Check that the specified node has the appropriate number of entries
                Assert.That(document.CreateNavigator().Select(xmlPath).Count ==
                    count);
            }
            finally
            {
                // Clean up the idso file that was created
                string idsoFile = _idShieldOfficeForm.ImageViewer.ImageFile + ".idso";
                if (File.Exists(idsoFile))
                {
                    try
                    {
                        File.Delete(idsoFile);
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Tests that the session nodes contain the appropriate user data.
        /// </summary>
        /// <param name="xmlPath">The xml node to grab.</param>
        /// <param name="nodeName">The name of the particular node to check.</param>
        [TestCase(@"//Sessions/Session[@Id]/SessionInfo/User", "User", 0,
            TestName="Automated_IdsoPropertiesSecondSessionNodeContainsAppropriateUserName",
            Description="Test that each session node contains the appropriate user name")]
        [TestCase(@"//Sessions/Session[@Id]/SessionInfo/Computer",
            "Computer", 1,
            TestName="Automated_IdsoPropertiesSecondSessionNodeContainsAppropriateComputerName",
            Description="Test that each session node contains the appropriate computer name")]
        [Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesSecondSessionNodeContainsAppropriateUserData(string xmlPath,
            string nodeName, int expectedValuePicker)
        {
            try
            {
                //-----------------------------------------------------------------------
                // First session - Add bates numbers and redactions
                //-----------------------------------------------------------------------

                // Open the test image
                OpenTestImage(_idShieldOfficeForm.ImageViewer);

                // Apply Bates numbers to the image
                _idShieldOfficeForm.ApplyBatesNumbers();

                // Redact square brackets with bracketed text finder
                RunBracketedTextFinder(true, false, false);

                // Save the changes to the idso file
                _idShieldOfficeForm.SaveImage("", OutputFormat.Idso);

                // Close the image
                _idShieldOfficeForm.ImageViewer.CloseImage();

                //-----------------------------------------------------------------------
                // Second session - Add clues
                //-----------------------------------------------------------------------

                // Open the image again
                OpenTestImage(_idShieldOfficeForm.ImageViewer);

                // Run the SSN finder to add clues
                RunSsnFinder(false);

                // Save the changes to the idso file
                _idShieldOfficeForm.SaveImage("", OutputFormat.Idso);

                // Close the image
                _idShieldOfficeForm.ImageViewer.CloseImage();

                //-----------------------------------------------------------------------
                // Third session - Move bates numbers and delete redactions
                //-----------------------------------------------------------------------

                // Open the image again
                OpenTestImage(_idShieldOfficeForm.ImageViewer);

                // Get the redactions and move the bates numbers
                List<Redaction> redactions = new List<Redaction>();
                foreach(LayerObject layerObject in _idShieldOfficeForm.ImageViewer.LayerObjects)
                {
                    // Check if the object is a redaction
                    Redaction redaction = layerObject as Redaction;
                    if (redaction != null)
                    {
                        redactions.Add(redaction);
                        continue;
                    }

                    // Check if the object is a bates number
                    TextLayerObject batesNumber = layerObject as TextLayerObject;
                    if (batesNumber != null)
                    {
                        if (batesNumber.Tags.Contains("Bates"))
                        {
                            batesNumber.Offset(-5, 5);
                            continue;
                        }
                    }
                }

                // Delete the redactions
                _idShieldOfficeForm.ImageViewer.LayerObjects.Remove(redactions.ToArray());

                // Get the XML document from the modification history
                XmlDocument document = new XmlDocument();
                document.LoadXml(GetModificationHistory().ToXmlString());

                // Get the user node
                XPathNodeIterator nodeIterator = document.CreateNavigator().Select(xmlPath);

                // Check that there are three user nodes (there should be three sessions)
                ExtractException.Assert("ELI24214", "Incorrect number of nodes!",
                    nodeIterator.Count == 3);

                string expectedString = "";
                switch (expectedValuePicker)
                {
                    case 0:
                        expectedString = Environment.UserName;
                        break;

                    case 1:
                        expectedString = Environment.MachineName;
                        break;

                    default:
                        ExtractException.ThrowLogicException("ELI23465");
                        break;
                }

                bool containsCorrectText = true;
                for (int i = 0; containsCorrectText && i < nodeIterator.Count; i++)
                {
                    // Move to the first user
                    nodeIterator.MoveNext();

                    containsCorrectText =
                        nodeIterator.Current.Name == nodeName
                        && nodeIterator.Current.Value == expectedString;
                }

                // Check that each session node contains the appropriate text
                Assert.That(containsCorrectText);
            }
            finally
            {
                // Clean up the idso file that was created
                string idsoFile = _idShieldOfficeForm.ImageViewer.ImageFile + ".idso";
                if (File.Exists(idsoFile))
                {
                    try
                    {
                        File.Delete(idsoFile);
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Tests that the session nodes contain the appropriate time data.
        /// </summary>
        /// <param name="xmlPath">The xml node to grab.</param>
        /// <param name="nodeName">The name of the particular node to check.</param>
        [TestCase(@"//Sessions/Session[@Id]/SessionInfo/StartTime", "StartTime",
            TestName="Automated_IdsoPropertiesSecondSessionNodeContainsAppropriateStartTime",
            Description="Test that each session node contains the appropriate start time")]
        [TestCase(@"//Sessions/Session[@Id]/SessionInfo/EndTime", "EndTime",
            TestName="Automated_IdsoPropertiesSecondSessionNodeContainsAppropriateEndTime",
            Description="Test that each session node contains the appropriate end time")]
        [Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoPropertiesSecondSessionNodeContainsAppropriateTimeData(string xmlPath,
            string nodeName)
        {
            try
            {
                //-----------------------------------------------------------------------
                // First session - Add bates numbers and redactions
                //-----------------------------------------------------------------------

                // Open the test image
                OpenTestImage(_idShieldOfficeForm.ImageViewer);

                // Apply Bates numbers to the image
                _idShieldOfficeForm.ApplyBatesNumbers();

                // Redact square brackets with bracketed text finder
                RunBracketedTextFinder(true, false, false);

                // Save the changes to the idso file
                _idShieldOfficeForm.SaveImage("", OutputFormat.Idso);

                // Close the image
                _idShieldOfficeForm.ImageViewer.CloseImage();

                //-----------------------------------------------------------------------
                // Second session - Add clues
                //-----------------------------------------------------------------------

                // Open the image again
                OpenTestImage(_idShieldOfficeForm.ImageViewer);

                // Run the SSN finder to add clues
                RunSsnFinder(false);

                // Save the changes to the idso file
                _idShieldOfficeForm.SaveImage("", OutputFormat.Idso);

                // Close the image
                _idShieldOfficeForm.ImageViewer.CloseImage();

                //-----------------------------------------------------------------------
                // Third session - Move bates numbers and delete redactions
                //-----------------------------------------------------------------------

                // Open the image again
                OpenTestImage(_idShieldOfficeForm.ImageViewer);

                // Sleep for a second (this ensures a different end time for this session)
                System.Threading.Thread.Sleep(1000);

                // Get the redactions and move the bates numbers
                List<Redaction> redactions = new List<Redaction>();
                foreach(LayerObject layerObject in _idShieldOfficeForm.ImageViewer.LayerObjects)
                {
                    // Check if the object is a redaction
                    Redaction redaction = layerObject as Redaction;
                    if (redaction != null)
                    {
                        redactions.Add(redaction);
                        continue;
                    }

                    // Check if the object is a bates number
                    TextLayerObject batesNumber = layerObject as TextLayerObject;
                    if (batesNumber != null)
                    {
                        if (batesNumber.Tags.Contains("Bates"))
                        {
                            batesNumber.Offset(-5, 5);
                            continue;
                        }
                    }
                }

                // Delete the redactions
                _idShieldOfficeForm.ImageViewer.LayerObjects.Remove(redactions.ToArray());

                // Get the XML document from the modification history
                XmlDocument document = new XmlDocument();
                document.LoadXml(GetModificationHistory().ToXmlString());

                // Get the user node
                XPathNodeIterator nodeIterator = document.CreateNavigator().Select(xmlPath);

                // Check that there are three user nodes (there should be three sessions)
                ExtractException.Assert("ELI23463", "Incorrect number of nodes!",
                    nodeIterator.Count == 3);

                DateTime[] times = new DateTime[nodeIterator.Count];
                for (int i = 0; i < nodeIterator.Count; i++)
                {
                    // Move to the first user
                    nodeIterator.MoveNext();

                    // Check that it is the appropriate node
                    ExtractException.Assert("ELI23464", "Incorrect node found!",
                        nodeIterator.Current.Name == nodeName);

                    // Get the time from the node
                    times[i] = DateTime.Parse(nodeIterator.Current.Value,
                        CultureInfo.InvariantCulture);
                }

                // Check that the times are ordered
                bool timesAreAppropriate = true;
                for (int i = 0; timesAreAppropriate && i < times.Length - 1; i++)
                {
                    timesAreAppropriate = times[i] < times[i + 1];
                }

                // Check that each session node contains the appropriate time
                Assert.That(timesAreAppropriate);
            }
            finally
            {
                // Clean up the idso file that was created
                string idsoFile = _idShieldOfficeForm.ImageViewer.ImageFile + ".idso";
                if (File.Exists(idsoFile))
                {
                    try
                    {
                        File.Delete(idsoFile);
                    }
                    catch
                    {
                    }
                }
            }
        }

        #endregion IDSO File Data File Contents For Multiple Sessions

        #region IDSO File Data File Contents - General

        /// <summary>
        /// Tests that the IDSO file contents are encrypted.
        /// </summary>
        [Test, Category("Interactive"), Category("IDSO File")]
        public void Interactive_IdsoPropertiesFileIsEncrypted()
        {
            try
            {
                //-----------------------------------------------------------------------
                // First session - Add bates numbers and redactions
                //-----------------------------------------------------------------------

                // Open the test image
                OpenTestImage(_idShieldOfficeForm.ImageViewer);

                // Apply Bates numbers to the image
                _idShieldOfficeForm.ApplyBatesNumbers();

                // Redact square brackets with bracketed text finder
                RunBracketedTextFinder(true, false, false);

                // Save the changes to the idso file
                _idShieldOfficeForm.SaveImage("", OutputFormat.Idso);

                // Close the image
                _idShieldOfficeForm.ImageViewer.CloseImage();

                //-----------------------------------------------------------------------
                // Second session - Add clues
                //-----------------------------------------------------------------------

                // Open the image again
                OpenTestImage(_idShieldOfficeForm.ImageViewer);

                // Run the SSN finder to add clues
                RunSsnFinder(false);

                // Save the changes to the idso file
                _idShieldOfficeForm.SaveImage("", OutputFormat.Idso);

                // Close the image
                _idShieldOfficeForm.ImageViewer.CloseImage();

                //-----------------------------------------------------------------------
                // Third session - Move bates numbers and delete redactions
                //-----------------------------------------------------------------------

                // Open the image again
                OpenTestImage(_idShieldOfficeForm.ImageViewer);

                // Get the redactions and move the bates numbers
                List<Redaction> redactions = new List<Redaction>();
                foreach(LayerObject layerObject in _idShieldOfficeForm.ImageViewer.LayerObjects)
                {
                    // Check if the object is a redaction
                    Redaction redaction = layerObject as Redaction;
                    if (redaction != null)
                    {
                        redactions.Add(redaction);
                        continue;
                    }

                    // Check if the object is a bates number
                    TextLayerObject batesNumber = layerObject as TextLayerObject;
                    if (batesNumber != null)
                    {
                        if (batesNumber.Tags.Contains("Bates"))
                        {
                            batesNumber.Offset(-5, 5);
                            continue;
                        }
                    }
                }

                // Delete the redactions
                _idShieldOfficeForm.ImageViewer.LayerObjects.Remove(redactions.ToArray());

                // Save the changes to the idso file
                _idShieldOfficeForm.SaveImage("", OutputFormat.Idso);

                MessageBox.Show("Opening IDSO file in Notepad."
                    + Environment.NewLine + Environment.NewLine
                    + "Please check if the data is human readable text and close notepad.",
                    "Instructions", MessageBoxButtons.OK, MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1, 0);

                // Open the IDSO file in notepad
                Process p = new Process();
                p.StartInfo.FileName = "notepad";
                p.StartInfo.Arguments = _idShieldOfficeForm.ImageViewer.ImageFile + ".idso";
                p.Start();
                p.WaitForExit();

                Assert.That(MessageBox.Show("Was the data human readable?", "Was Data Readable",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2, 0) == DialogResult.No); 
            }
            finally
            {
                // Clean up the idso file that was created
                string idsoFile = _idShieldOfficeForm.ImageViewer.ImageFile + ".idso";
                if (File.Exists(idsoFile))
                {
                    try
                    {
                        File.Delete(idsoFile);
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Tests that the IDSO file contents are load correctly after being saved.
        /// </summary>
        [Test, Category("Automated"), Category("IDSO File")]
        public void Automated_IdsoFileCorrectlyLoadsRedactions()
        {
            try
            {
                // Get the image viewer
                ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

                // Open the test image
                OpenTestImage(imageViewer);

                // Get the idso file name
                string idsoFileName = imageViewer.ImageFile + ".idso";

                // Add two redactions to the image viewer
                Rectangle rectangle1 = new Rectangle(200, 100, 300, 200);
                Rectangle rectangle2 = new Rectangle(200, 400, 300, 200);
                imageViewer.LayerObjects.Add(new Redaction(imageViewer, 1, null, "Manual",
                    new RasterZone[] { new RasterZone(rectangle1, 1) },
                    RedactionColor.Black));
                imageViewer.LayerObjects.Add(new Redaction(imageViewer, 1, null, "Manual",
                    new RasterZone[] { new RasterZone(rectangle2, 1) },
                    RedactionColor.White));

                // Save the changes to the idso file
                _idShieldOfficeForm.SaveImage("", OutputFormat.Idso);

                // Close the image
                imageViewer.CloseImage();

                // Now open the IDSO file
                imageViewer.OpenImage(idsoFileName, false);

                // Get the redactions from the image viewer
                List<Redaction> redactions = new List<Redaction>();
                foreach (LayerObject layerObject in imageViewer.GetLayeredObjects(
                    null, ArgumentRequirement.Any,
                    new Type[] { typeof(Redaction) }, ArgumentRequirement.Any,
                    null, ArgumentRequirement.Any))
                {
                    Redaction redaction = layerObject as Redaction;
                    if (redaction != null)
                    {
                        redactions.Add(redaction);
                    }
                }

                // Sort the redactions
                redactions.Sort();

                // Check that there are the correct number of redactions in the correct
                // location with the correct color
                bool redactionsAreCorrect = redactions.Count == 2
                    && redactions[0].GetBounds() == rectangle1
                    && redactions[1].GetBounds() == rectangle2
                    && redactions[0].FillColor == RedactionColor.Black
                    && redactions[1].FillColor == RedactionColor.White;

                Assert.That(redactionsAreCorrect);
            }
            finally
            {
                // Clean up the idso file that was created
                string idsoFile = _idShieldOfficeForm.ImageViewer.ImageFile + ".idso";
                if (File.Exists(idsoFile))
                {
                    try
                    {
                        File.Delete(idsoFile);
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Tests that the IDSO file contents are load correctly after being saved
        /// and also tests that the redaction did not get burned into the image.
        /// </summary>
        [Test, Category("Interactive"), Category("IDSO File")]
        public void Interactive_IdsoFileCorrectlySavesAndLoadsBlackRedactions()
        {
            try
            {
                // Get the image viewer
                ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

                // Open the test image
                OpenTestImage(imageViewer);

                // Get the idso file name
                string idsoFileName = imageViewer.ImageFile + ".idso";

                // Add a redaction to the image viewer
                Rectangle rectangle = new Rectangle(369, 299, 341, 61);
                imageViewer.LayerObjects.Add(new Redaction(imageViewer, 1, null, "Manual",
                    new RasterZone[] { new RasterZone(rectangle, 1) },
                    RedactionColor.Black));

                // Save the changes to the idso file
                _idShieldOfficeForm.SaveImage("", OutputFormat.Idso);

                // Close the image
                imageViewer.CloseImage();

                // Now open the IDSO file
                imageViewer.OpenImage(idsoFileName, false);

                // Get the redactions from the image viewer
                List<Redaction> redactions = new List<Redaction>();
                foreach (LayerObject layerObject in imageViewer.GetLayeredObjects(
                    null, ArgumentRequirement.Any,
                    new Type[] { typeof(Redaction) }, ArgumentRequirement.Any,
                    null, ArgumentRequirement.Any))
                {
                    Redaction redaction = layerObject as Redaction;
                    if (redaction != null)
                    {
                        redactions.Add(redaction);
                    }
                }

                // Show the IDSO form
                _idShieldOfficeForm.Show();

                // Zoom in a few times
                imageViewer.ZoomToRectangle(new Rectangle(500, 500, 50, 300));

                imageViewer.CenterOnLayerObjects(redactions[0]);

                Assert.That(MessageBox.Show(
                    "Is the redaction rendered a Dark Gray and can the text be viewed through it?",
                    "Is Redaction Dark Gray", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
            }
            finally
            {
                // Clean up the idso file that was created
                string idsoFile = _idShieldOfficeForm.ImageViewer.ImageFile + ".idso";
                if (File.Exists(idsoFile))
                {
                    try
                    {
                        File.Delete(idsoFile);
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Tests that the IDSO file contents are load correctly after being saved
        /// and also tests that the redaction did not get burned into the image.
        /// </summary>
        [Test, Category("Interactive"), Category("IDSO File")]
        public void Interactive_IdsoFileCorrectlySavesAndLoadsWhiteRedactions()
        {
            try
            {
                // Get the image viewer
                ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

                // Open the test image
                OpenTestImage(imageViewer);

                // Get the idso file name
                string idsoFileName = imageViewer.ImageFile + ".idso";

                // Add a redaction to the image viewer
                Rectangle rectangle = new Rectangle(369, 299, 341, 61);
                imageViewer.LayerObjects.Add(new Redaction(imageViewer, 1, null, "Manual",
                    new RasterZone[] { new RasterZone(rectangle, 1) },
                    RedactionColor.White));

                // Save the changes to the idso file
                _idShieldOfficeForm.SaveImage("", OutputFormat.Idso);

                // Close the image
                imageViewer.CloseImage();

                // Now open the IDSO file
                imageViewer.OpenImage(idsoFileName, false);

                // Get the redactions from the image viewer
                List<Redaction> redactions = new List<Redaction>();
                foreach (LayerObject layerObject in imageViewer.GetLayeredObjects(
                    null, ArgumentRequirement.Any,
                    new Type[] { typeof(Redaction) }, ArgumentRequirement.Any,
                    null, ArgumentRequirement.Any))
                {
                    Redaction redaction = layerObject as Redaction;
                    if (redaction != null)
                    {
                        redactions.Add(redaction);
                    }
                }

                // Show the IDSO form
                _idShieldOfficeForm.Show();

                // Zoom in a few times
                imageViewer.ZoomToRectangle(new Rectangle(500, 500, 50, 300));

                imageViewer.CenterOnLayerObjects(redactions[0]);

                Assert.That(MessageBox.Show(
                    "Is the redaction rendered a Light Gray and can the text be viewed through it?",
                    "Is Redaction Light Gray", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
            }
            finally
            {
                // Clean up the idso file that was created
                string idsoFile = _idShieldOfficeForm.ImageViewer.ImageFile + ".idso";
                if (File.Exists(idsoFile))
                {
                    try
                    {
                        File.Delete(idsoFile);
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Tests that the IDSO file contents are load correctly after being saved
        /// and also tests that the redaction did not get burned into the image.
        /// </summary>
        [Test, Category("Interactive"), Category("IDSO File")]
        public void Interactive_IdsoFileCannotBeModifiedWithTextEditor()
        {
            try
            {
                // Get the image viewer
                ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;

                // Open the test image
                OpenTestImage(imageViewer);

                // Get the idso file name
                string idsoFileName = imageViewer.ImageFile + ".idso";

                // Add a redaction to the image viewer
                Rectangle rectangle = new Rectangle(369, 299, 341, 61);
                imageViewer.LayerObjects.Add(new Redaction(imageViewer, 1, null, "Manual",
                    new RasterZone[] { new RasterZone(rectangle, 1) },
                    RedactionColor.White));

                // Save the changes to the idso file
                _idShieldOfficeForm.SaveImage("", OutputFormat.Idso);

                // Close the image
                imageViewer.CloseImage();

                MessageBox.Show("Opening IDSO file in Notepad."
                    + Environment.NewLine + Environment.NewLine
                    + "Please modify the file, save it, and close notepad."
                    + " You should then see an error message displayed. Please read the message"
                    + " and close it.",
                    "Instructions", MessageBoxButtons.OK, MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1, 0);

                // Open the IDSO file in notepad
                Process p = new Process();
                p.StartInfo.FileName = "notepad";
                p.StartInfo.Arguments = idsoFileName;
                p.Start();
                p.WaitForExit();

                // Show the IDSO form
                _idShieldOfficeForm.Show();

                // Now open the IDSO file
                imageViewer.OpenImage(idsoFileName, false);

                // Check that the error was displayed
                Assert.That(MessageBox.Show("Was an error message containing the words "
                    + "'Unrecognized version number' displayed?",
                    "Is Error Displayed", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes);
            }
            finally
            {
                // Clean up the idso file that was created
                string idsoFile = _idShieldOfficeForm.ImageViewer.ImageFile + ".idso";
                if (File.Exists(idsoFile))
                {
                    try
                    {
                        File.Delete(idsoFile);
                    }
                    catch
                    {
                    }
                }
            }
        }
        #endregion IDSO File Data File Contents - General
    }
}
