using Extract;
using Extract.Imaging.Forms;
using Extract.Rules;
using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace IDShieldOffice.Test
{
    public partial class TestIDShieldOffice
    {
        #region Debit Credit

        /// <summary>
        /// The minimum area that needs to be covered to be considered a match for a clue.
        /// </summary>
        static readonly Rectangle[] _DEBIT_CREDIT_CLUES = new Rectangle[]
        {
            new Rectangle(162,  578, 194, 24),
            new Rectangle(161,  621, 145, 24),
            new Rectangle(161,  662, 306, 30),
            new Rectangle(163,  704, 150, 24),
            new Rectangle(162,  788, 419, 24),
            new Rectangle(162,  830, 324, 30),
            new Rectangle(163,  872, 273, 24),
            new Rectangle(161,  956, 489, 24),
            new Rectangle(161,  998, 509, 30),
            new Rectangle(163, 1040, 380, 24),
            new Rectangle(163, 1082, 353, 24),
            new Rectangle(163, 1124, 242, 24),
            new Rectangle(161, 1209,  68, 24),
            new Rectangle(163, 1250, 163, 24),
            new Rectangle(163, 1292, 125, 24),
            new Rectangle(161, 1333, 257, 30),
            new Rectangle(162, 1376, 161, 24),
            new Rectangle(163, 1418, 151, 24),
            new Rectangle(162, 1460, 174, 30)
        };

        /// <summary>
        /// The minimum area that needs to be covered to be considered a match for a redaction.
        /// </summary>
        static readonly Rectangle[] _DEBIT_CREDIT_MATCHES = new Rectangle[]
        {
            new Rectangle(163,  285, 319, 24),
            new Rectangle(163,  327, 367, 24),
            new Rectangle(163,  369, 289, 24),
            new Rectangle(163,  411, 321, 24),
            new Rectangle(365,  579, 288, 24),
            new Rectangle(327,  621, 309, 24),
            new Rectangle(487,  663, 214, 24),
            new Rectangle(330,  705, 297, 24),
            new Rectangle(598,  789, 287, 24),
            new Rectangle(507,  831, 213, 24),
            new Rectangle(457,  873, 272, 24),
            new Rectangle(669,  957, 286, 24),
            new Rectangle(689,  999, 292, 24),
            new Rectangle(562, 1041, 236, 24),
            new Rectangle(535, 1083, 150, 24),
            new Rectangle(424, 1125, 303, 24),
            new Rectangle(247, 1209, 270, 24),
            new Rectangle(347, 1251, 364, 24),
            new Rectangle(305, 1293, 297, 24),
            new Rectangle(438, 1335, 195, 24),
            new Rectangle(345, 1377, 213, 24),
            new Rectangle(336, 1419, 253, 24),
            new Rectangle(358, 1461, 300, 24)
        };

        /// <summary>
        /// Test the ability to find credit and debit card numbers.
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("Data type rule")]
        public void Automated_FindDebitCreditCardNumbers()
        {
            // Step 1: Open the Image

            // Show the ID Shield Office form
            _idShieldOfficeForm.Show();

            // Open the test image
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;
            imageViewer.OpenImage(_testImages.GetFile(_DEBIT_CREDIT), false);

            // Ensure the image openned
            Assert.That(imageViewer.IsImageAvailable, "Could not open test image.");

            // Step 2: Setup the data type finder form

            // Click the data type finder button
            ToolStripButton button = FormMethods.GetFormComponent<ToolStripButton>(_idShieldOfficeForm,
                "Data type finder");
            button.PerformClick();

            // Ensure the data type rule is displayed
            RuleForm ruleForm = _idShieldOfficeForm.DataTypeRuleForm;
            Assert.That(ruleForm != null && ruleForm.Visible, "Cannot display rule form.");

            // Check the Account numbers - Credit and debit card numbers checkbox.
            CheckBox checkBox = 
                FormMethods.GetFormComponent<CheckBox>(ruleForm,
                "Account numbers - Credit and debit card numbers");
            checkBox.Checked = true;

            // Step 3: Redact items individually
            
            // Click the Find next button.
            Button findNext = FormMethods.GetFormComponent<Button>(ruleForm, "Find next");
            findNext.PerformClick();

            // Confirm that all the proper clues are found
            AssertMatches<Clue>(imageViewer, _DEBIT_CREDIT_CLUES);

            // Confirm that the first occurrence 1234-1234-1234-1234 is highlighted.
            CompositeHighlightLayerObject searchResult = GetSearchResult(imageViewer);
            AssertMatch(searchResult, _DEBIT_CREDIT_MATCHES[0]);

            // Click the Redact button.
            Button redact = FormMethods.GetFormComponent<Button>(ruleForm, "Redact");
            redact.PerformClick();

            // Confirm that next occurrence 1234 - 1234 - 1234 - 1234 is highlighted.
            searchResult = GetSearchResult(imageViewer);
            AssertMatch(searchResult, _DEBIT_CREDIT_MATCHES[1]);

            // Click the Redact button.
            redact.PerformClick();

            // Confirm that next occurrence 1234-123456-12345 is highlighted.
            searchResult = GetSearchResult(imageViewer);
            AssertMatch(searchResult, _DEBIT_CREDIT_MATCHES[2]);

            // Step 4: Skip to the first clue

            // Skip the next two and make sure we are on a clue
            findNext.PerformClick();
            findNext.PerformClick();
            searchResult = GetSearchResult(imageViewer);
            AssertMatch(searchResult, _DEBIT_CREDIT_CLUES[0]);

            // The redact button should be disabled
            Assert.That(!redact.Enabled);

            // Step 5: Redact all

            // Click the Redact all button.
            Button redactAll = FormMethods.GetFormComponent<Button>(ruleForm, "Redact all");
            redactAll.PerformClick();

            // Confirm that all the proper items are redacted
            AssertMatches<Redaction>(imageViewer, _DEBIT_CREDIT_MATCHES);
        }

        #endregion Debit Credit

        #region Savings Checking

        /// <summary>
        /// The minimum area that needs to be covered to be considered a match for clues.
        /// </summary>
        static readonly Rectangle[] _SAVINGS_CHECKING_CLUES = new Rectangle[]
        {
            new Rectangle(157, 254, 366, 30),
            new Rectangle(157, 296, 216, 30),
            new Rectangle(157, 339, 213, 24),
            new Rectangle(157, 381, 115, 24),
            new Rectangle(158, 464, 388, 30),
            new Rectangle(158, 507, 179, 24),
            new Rectangle(158, 549,  63, 24),
            new Rectangle(159, 632, 327, 24),
            new Rectangle(159, 674, 120, 24),
            new Rectangle(159, 716, 168, 24)
        };

        /// <summary>
        /// The minimum area that needs to be covered to be considered a match for redactions.
        /// </summary>
        static readonly Rectangle[] _SAVINGS_CHECKING_MATCHES = new Rectangle[]
        {
            new Rectangle(542, 255, 308, 24),
            new Rectangle(394, 297, 291, 24),
            new Rectangle(387, 339, 263, 24),
            new Rectangle(293, 381, 196, 24),
            new Rectangle(563, 465, 269, 24),
            new Rectangle(358, 507, 304, 24),
            new Rectangle(239, 549, 214, 24),
            new Rectangle(505, 633, 252, 24),
            new Rectangle(299, 675, 178, 24),
            new Rectangle(346, 717, 261, 24),
        };

        /// <summary>
        /// Test the ability to find savings and checking card numbers.
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("Data type rule")]
        public void Automated_FindSavingsCheckingNumbers()
        {
            // Step 1: Open the Image

            // Show the ID Shield Office form
            _idShieldOfficeForm.Show();

            // Open the test image
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;
            imageViewer.OpenImage(_testImages.GetFile(_SAVINGS_CHECKING), false);

            // Ensure the image openned
            Assert.That(imageViewer.IsImageAvailable, "Could not open test image.");

            // Step 2: Setup the data type finder form

            // Click the data type finder button
            ToolStripButton button = FormMethods.GetFormComponent<ToolStripButton>(
                _idShieldOfficeForm, "Data type finder");
            button.PerformClick();

            // Ensure the data type rule is displayed
            RuleForm ruleForm = _idShieldOfficeForm.DataTypeRuleForm;
            Assert.That(ruleForm != null && ruleForm.Visible, "Cannot display rule form.");

            // Check the Account numbers - Savings, checking, and bank account numbers checkbox.
            CheckBox checkBox =
                FormMethods.GetFormComponent<CheckBox>(ruleForm,
                "Account numbers - Savings, checking, and bank account numbers");
            checkBox.Checked = true;

            // Step 3: Find the clues

            // Click the Find next button.
            Button findNext = FormMethods.GetFormComponent<Button>(ruleForm, "Find next");
            findNext.PerformClick();

            // Confirm that all the proper clues are found
            AssertMatches<Clue>(imageViewer, _SAVINGS_CHECKING_CLUES);

            // Confirm that the first clue is highlighted.
            CompositeHighlightLayerObject searchResult = GetSearchResult(imageViewer);
            AssertMatch(searchResult, _SAVINGS_CHECKING_CLUES[0]);

            // The redact button should be disabled
            Button redact = FormMethods.GetFormComponent<Button>(ruleForm, "Redact");
            Assert.That(!redact.Enabled);

            // Step 4: Redact individually

            // Click the Find next button.
            findNext.PerformClick();

            // Confirm that the first redaction is highlighted.
            searchResult = GetSearchResult(imageViewer);
            AssertMatch(searchResult, _SAVINGS_CHECKING_MATCHES[0]);

            // Click the Redact button.
            redact.PerformClick();

            // Confirm that the first clue is highlighted.
            searchResult = GetSearchResult(imageViewer);
            AssertMatch(searchResult, _SAVINGS_CHECKING_CLUES[1]);
            Assert.That(!redact.Enabled);

            // Step 5: Redact all

            // Click the Redact all button.
            Button redactAll = FormMethods.GetFormComponent<Button>(ruleForm, "Redact all");
            redactAll.PerformClick();

            // Confirm that all the proper items are redacted
            AssertMatches<Redaction>(imageViewer, _SAVINGS_CHECKING_MATCHES);
        }

        #endregion Savings Checking

        #region Other Accounts

        /// <summary>
        /// The minimum area that needs to be covered to be considered a match for clues.
        /// </summary>
        static readonly Rectangle[] _OTHER_ACCOUNTS_CLUES = new Rectangle[]
        {
            new Rectangle(235, 419, 248, 24),
            new Rectangle(235, 462, 112, 24),
            new Rectangle(235, 501, 148, 32),
            new Rectangle(235, 543,  81, 32)
        };

        /// <summary>
        /// The minimum area that needs to be covered to be considered a match for redactions.
        /// </summary>
        static readonly Rectangle[] _OTHER_ACCOUNTS_MATCHES = new Rectangle[]
        {
            new Rectangle(502, 420, 236, 24),
            new Rectangle(366, 462, 161, 24),
            new Rectangle(404, 504, 247, 24),
            new Rectangle(335, 546, 230, 24)
        };

        /// <summary>
        /// Test the ability to find other account numbers.
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("Data type rule")]
        public void Automated_FindOtherAccountNumbers()
        {
            // Step 1: Open the Image

            // Show the ID Shield Office form
            _idShieldOfficeForm.Show();

            // Open the test image
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;
            imageViewer.OpenImage(_testImages.GetFile(_ACCOUNT_NUMBER), false);

            // Ensure the image openned
            Assert.That(imageViewer.IsImageAvailable, "Could not open test image.");

            // Step 2: Setup the data type finder form

            // Click the data type finder button
            ToolStripButton button = FormMethods.GetFormComponent<ToolStripButton>(
                _idShieldOfficeForm, "Data type finder");
            button.PerformClick();

            // Ensure the data type rule is displayed
            RuleForm ruleForm = _idShieldOfficeForm.DataTypeRuleForm;
            Assert.That(ruleForm != null && ruleForm.Visible, "Cannot display rule form.");

            // Check the Account numbers - Other account numbers checkbox.
            CheckBox checkBox =
                FormMethods.GetFormComponent<CheckBox>(ruleForm,
                "Account numbers - Other account numbers");
            checkBox.Checked = true;

            // Step 3: Find the clues

            // Click the Find next button.
            Button findNext = FormMethods.GetFormComponent<Button>(ruleForm, "Find next");
            findNext.PerformClick();

            // Confirm that all the proper clues are found
            AssertMatches<Clue>(imageViewer, _OTHER_ACCOUNTS_CLUES);

            // Confirm that the first clue is highlighted.
            CompositeHighlightLayerObject searchResult = GetSearchResult(imageViewer);
            AssertMatch(searchResult, _OTHER_ACCOUNTS_CLUES[0]);

            // The redact button should be disabled
            Button redact = FormMethods.GetFormComponent<Button>(ruleForm, "Redact");
            Assert.That(!redact.Enabled);

            // Step 4: Redact individually

            // Click the Find next button.
            findNext.PerformClick();

            // Confirm that the first redaction is highlighted.
            searchResult = GetSearchResult(imageViewer);
            AssertMatch(searchResult, _OTHER_ACCOUNTS_MATCHES[0]);

            // Click the Redact button.
            redact.PerformClick();

            // Confirm that the first clue is highlighted.
            searchResult = GetSearchResult(imageViewer);
            AssertMatch(searchResult, _OTHER_ACCOUNTS_CLUES[1]);
            Assert.That(!redact.Enabled);

            // Step 5: Redact all

            // Click the Redact all button.
            Button redactAll = FormMethods.GetFormComponent<Button>(ruleForm, "Redact all");
            redactAll.PerformClick();

            // Confirm that all the proper items are redacted
            AssertMatches<Redaction>(imageViewer, _OTHER_ACCOUNTS_MATCHES);
        }

        #endregion Other Accounts

        #region Driver's License

        /// <summary>
        /// The minimum area that needs to be covered to be considered a match for clues.
        /// </summary>
        static readonly Rectangle[] _DRIVERS_LICENSE_CLUES = new Rectangle[]
        {
            new Rectangle(163,  249, 354, 26),
            new Rectangle(163,  291, 303, 26),
            new Rectangle(163,  333, 275, 26),
            new Rectangle(163,  375, 252, 26),
            new Rectangle(163,  417, 206, 26),
            new Rectangle(163,  459, 285, 26),
            new Rectangle(163,  502, 163, 24),
            new Rectangle(163,  545, 112, 24),
            new Rectangle(163,  587,  84, 24),
            new Rectangle(163,  629,  53, 24),
            new Rectangle(163,  671,  56, 24),
            new Rectangle(163,  713,  90, 24),
            new Rectangle(215,  795, 342, 26),
            new Rectangle(163,  879, 225, 26),
            new Rectangle(163,  921, 156, 26),
            new Rectangle(163,  965,  35, 24),
            new Rectangle(163, 1007,  60, 24)
        };

        /// <summary>
        /// The minimum area that needs to be covered to be considered a match for redactions.
        /// </summary>
        static readonly Rectangle[] _DRIVERS_LICENSE_MATCHES = new Rectangle[]
        {
            new Rectangle(536,  251, 182, 24),
            new Rectangle(485,  293, 340, 24),
            new Rectangle(457,  335, 368, 24),
            new Rectangle(434,  377, 371, 24),
            new Rectangle(388,  419, 215, 24),
            new Rectangle(465,  461, 223, 24),
            new Rectangle(345,  503, 290, 24),
            new Rectangle(296,  545, 261, 24),
            new Rectangle(266,  587, 388, 24),
            new Rectangle(235,  629, 196, 24),
            new Rectangle(239,  671, 195, 24),
            new Rectangle(283,  713, 228, 24),
            new Rectangle(595,  797, 198, 24),
            new Rectangle(409,  881, 195, 24),
            new Rectangle(340,  923, 286, 24),
            new Rectangle(218,  965, 296, 24),
            new Rectangle(243, 1007, 218, 24)
        };

        /// <summary>
        /// Test the ability to find driver's license numbers.
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("Data type rule")]
        public void Automated_FindDriversLicenseNumbers()
        {
            // Step 1: Open the Image

            // Show the ID Shield Office form
            _idShieldOfficeForm.Show();

            // Open the test image
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;
            imageViewer.OpenImage(_testImages.GetFile(_DRIVERS_LICENSE), false);

            // Ensure the image openned
            Assert.That(imageViewer.IsImageAvailable, "Could not open test image.");

            // Step 2: Setup the data type finder form

            // Click the data type finder button
            ToolStripButton button = FormMethods.GetFormComponent<ToolStripButton>(
                _idShieldOfficeForm, "Data type finder");
            button.PerformClick();

            // Ensure the data type rule is displayed
            RuleForm ruleForm = _idShieldOfficeForm.DataTypeRuleForm;
            Assert.That(ruleForm != null && ruleForm.Visible, "Cannot display rule form.");

            // Check the Driver's license numbers checkbox.
            CheckBox checkBox =
                FormMethods.GetFormComponent<CheckBox>(ruleForm, "Driver's license numbers");
            checkBox.Checked = true;

            // Step 3: Find the clues

            // Click the Find next button.
            Button findNext = FormMethods.GetFormComponent<Button>(ruleForm, "Find next");
            findNext.PerformClick();

            // Confirm that all the proper clues are found
            AssertMatches<Clue>(imageViewer, _DRIVERS_LICENSE_CLUES);

            // Confirm that the first clue is highlighted.
            CompositeHighlightLayerObject searchResult = GetSearchResult(imageViewer);
            AssertMatch(searchResult, _DRIVERS_LICENSE_CLUES[0]);

            // The redact button should be disabled
            Button redact = FormMethods.GetFormComponent<Button>(ruleForm, "Redact");
            Assert.That(!redact.Enabled);

            // Step 4: Redact individually

            // Click the Find next button.
            findNext.PerformClick();

            // Confirm that the first redaction is highlighted.
            searchResult = GetSearchResult(imageViewer);
            AssertMatch(searchResult, _DRIVERS_LICENSE_MATCHES[0]);

            // Click the Redact button.
            redact.PerformClick();

            // Confirm that the next clue is highlighted.
            searchResult = GetSearchResult(imageViewer);
            AssertMatch(searchResult, _DRIVERS_LICENSE_CLUES[1]);
            Assert.That(!redact.Enabled);

            // Step 5: Redact all

            // Click the Redact all button.
            Button redactAll = FormMethods.GetFormComponent<Button>(ruleForm, "Redact all");
            redactAll.PerformClick();

            // Confirm that all the proper items are redacted
            AssertMatches<Redaction>(imageViewer, _DRIVERS_LICENSE_MATCHES);
        }

        #endregion Driver's License

        #region Email Addresses

        /// <summary>
        /// The minimum area that needs to be covered to be considered a match for clues.
        /// </summary>
        static readonly Rectangle[] _EMAIL_ADDRESS_CLUES = new Rectangle[]
        {
            new Rectangle(273, 401,  76, 24),
            new Rectangle(273, 443,  87, 24),
            new Rectangle(273, 485, 103, 24),
            new Rectangle(273, 569, 203, 24),
            new Rectangle(272, 611, 212, 24),
            new Rectangle(272, 653, 228, 24)
        };

        /// <summary>
        /// The minimum area that needs to be covered to be considered a match for redactions.
        /// </summary>
        static readonly Rectangle[] _EMAIL_ADDRESS_MATCHES = new Rectangle[]
        {
            new Rectangle(272,  821, 480, 30),
            new Rectangle(272,  862, 511, 32),
            new Rectangle(271,  905, 549, 30),
            new Rectangle(271,  947, 290, 30),
            new Rectangle(947, 1030, 357, 32)
        };

        /// <summary>
        /// Test the ability to find email addresses.
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("Data type rule")]
        public void Automated_FindEmailAddresses()
        {
            // Step 1: Open the Image

            // Show the ID Shield Office form
            _idShieldOfficeForm.Show();

            // Open the test image
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;
            imageViewer.OpenImage(_testImages.GetFile(_EMAIL), false);

            // Ensure the image openned
            Assert.That(imageViewer.IsImageAvailable, "Could not open test image.");

            // Step 2: Setup the data type finder form

            // Click the data type finder button
            ToolStripButton button = FormMethods.GetFormComponent<ToolStripButton>(
                _idShieldOfficeForm, "Data type finder");
            button.PerformClick();

            // Ensure the data type rule is displayed
            RuleForm ruleForm = _idShieldOfficeForm.DataTypeRuleForm;
            Assert.That(ruleForm != null && ruleForm.Visible, "Cannot display rule form.");

            // Check the E-mail addresses checkbox.
            CheckBox checkBox =
                FormMethods.GetFormComponent<CheckBox>(ruleForm, "E-mail addresses");
            checkBox.Checked = true;

            // Step 3: Find the clues

            // Click the Find next button.
            Button findNext = FormMethods.GetFormComponent<Button>(ruleForm, "Find next");
            findNext.PerformClick();

            // Confirm that all the proper clues are found
            AssertMatches<Clue>(imageViewer, _EMAIL_ADDRESS_CLUES);

            // Confirm that the first clue is highlighted.
            CompositeHighlightLayerObject searchResult = GetSearchResult(imageViewer);
            AssertMatch(searchResult, _EMAIL_ADDRESS_CLUES[0]);

            // The redact button should be disabled
            Button redact = FormMethods.GetFormComponent<Button>(ruleForm, "Redact");
            Assert.That(!redact.Enabled);

            // Step 4: Redact all

            // Click the Redact all button.
            Button redactAll = FormMethods.GetFormComponent<Button>(ruleForm, "Redact all");
            redactAll.PerformClick();

            // Confirm that all the proper items are redacted
            AssertMatches<Redaction>(imageViewer, _EMAIL_ADDRESS_MATCHES);
        }

        #endregion Email Addresses

        #region Tax ID

        /// <summary>
        /// The minimum area that needs to be covered to be considered a match for clues.
        /// </summary>
        static readonly Rectangle[] _TAX_ID_CLUES = new Rectangle[]
        {
            new Rectangle(249,  476, 603, 30),
            new Rectangle(249,  513, 248, 24),
            new Rectangle(249,  550, 607, 30),
            new Rectangle(249,  587, 359, 30),
            new Rectangle(248,  661, 479, 30),
            new Rectangle(248,  698, 294, 24),
            new Rectangle(248,  735, 146, 24),
            new Rectangle(249,  809, 482, 30),
            new Rectangle(249,  846, 234, 30),
            new Rectangle(248,  920,  49, 24),
            new Rectangle(248,  957,  85, 24),
            new Rectangle(248,  994,  47, 24),
            new Rectangle(249, 1031,  49, 24),
            new Rectangle(249, 1068,  85, 24),
            new Rectangle(249, 1105,  47, 24),
            new Rectangle(249, 1142,  69, 24),
            new Rectangle(249, 1179,  96, 24),
            new Rectangle(249, 1216,  67, 24),
            new Rectangle(248, 1808, 146, 24),
            new Rectangle(248, 1845, 146, 24)
        };

        /// <summary>
        /// The minimum area that needs to be covered to be considered a match for redactions.
        /// </summary>
        static readonly Rectangle[] _TAX_ID_MATCHES = new Rectangle[]
        {
            new Rectangle(250,  254, 167, 24),
            new Rectangle(248,  291, 191, 24),
            new Rectangle(259,  328, 172, 24),
            new Rectangle(871,  476, 161, 24),
            new Rectangle(517,  513, 160, 24),
            new Rectangle(875,  550, 161, 24),
            new Rectangle(627,  587, 161, 24),
            new Rectangle(746,  661, 158, 24),
            new Rectangle(560,  698, 162, 24),
            new Rectangle(413,  735, 161, 24),
            new Rectangle(752,  809, 160, 24),
            new Rectangle(504,  846, 159, 24),
            new Rectangle(319,  920, 160, 24),
            new Rectangle(355,  957, 159, 24),
            new Rectangle(313,  994, 161, 24),
            new Rectangle(318, 1031, 162, 24),
            new Rectangle(354, 1068, 161, 24),
            new Rectangle(314, 1105, 162, 24),
            new Rectangle(340, 1142, 159, 24),
            new Rectangle(367, 1179, 159, 24),
            new Rectangle(334, 1216, 158, 24)
        };

        /// <summary>
        /// Test the ability to find federal tax identification numbers.
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("Data type rule")]
        public void Automated_FindTaxIdNumbers()
        {
            // Step 1: Open the Image

            // Show the ID Shield Office form
            _idShieldOfficeForm.Show();

            // Open the test image
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;
            imageViewer.OpenImage(_testImages.GetFile(_TAX_ID), false);

            // Ensure the image openned
            Assert.That(imageViewer.IsImageAvailable, "Could not open test image.");

            // Step 2: Setup the data type finder form

            // Click the data type finder button
            ToolStripButton button = FormMethods.GetFormComponent<ToolStripButton>(
                _idShieldOfficeForm, "Data type finder");
            button.PerformClick();

            // Ensure the data type rule is displayed
            RuleForm ruleForm = _idShieldOfficeForm.DataTypeRuleForm;
            Assert.That(ruleForm != null && ruleForm.Visible, "Cannot display rule form.");

            // Check the Federal tax identification numbers checkbox.
            CheckBox checkBox =
                FormMethods.GetFormComponent<CheckBox>(ruleForm, "Federal tax identification numbers");
            checkBox.Checked = true;

            // Step 3: Redact items individually

            // Click the Find next button.
            Button findNext = FormMethods.GetFormComponent<Button>(ruleForm, "Find next");
            findNext.PerformClick();

            // Confirm that all the proper clues are found
            AssertMatches<Clue>(imageViewer, _TAX_ID_CLUES);

            // Confirm that the first redaction is highlighted.
            CompositeHighlightLayerObject searchResult = GetSearchResult(imageViewer);
            AssertMatch(searchResult, _TAX_ID_MATCHES[0]);

            // Click the Redact button.
            Button redact = FormMethods.GetFormComponent<Button>(ruleForm, "Redact");
            redact.PerformClick();

            // Confirm that next redaction is highlighted.
            searchResult = GetSearchResult(imageViewer);
            AssertMatch(searchResult, _TAX_ID_MATCHES[1]);

            // Click the Redact button.
            redact.PerformClick();

            // Confirm that next redaction is highlighted.
            searchResult = GetSearchResult(imageViewer);
            AssertMatch(searchResult, _TAX_ID_MATCHES[2]);

            // Step 4: Check the first clue

            // Click the redact button
            redact.PerformClick();

            // Make sure we are on a clue
            searchResult = GetSearchResult(imageViewer);
            AssertMatch(searchResult, _TAX_ID_CLUES[0]);

            // The redact button should be disabled
            Assert.That(!redact.Enabled);

            // Step 5: Redact all

            // Click the Redact all button.
            Button redactAll = FormMethods.GetFormComponent<Button>(ruleForm, "Redact all");
            redactAll.PerformClick();

            // Confirm that all the proper items are redacted
            AssertMatches<Redaction>(imageViewer, _TAX_ID_MATCHES);
        }

        #endregion Tax ID

        #region Social Security Numbers

        /// <summary>
        /// The minimum area that needs to be covered to be considered a match for clues.
        /// </summary>
        static readonly Rectangle[] _SSN_CLUES = new Rectangle[]
        {
            new Rectangle(139, 1531, 341, 30),
            new Rectangle(139, 1573, 290, 30),
            new Rectangle(139, 1615, 262, 30),
            new Rectangle(139, 1657, 239, 30),
            new Rectangle(139, 1700,  55, 24),
            new Rectangle(139, 1742,  83, 24),
            new Rectangle(139, 1784,  52, 24),
            new Rectangle(139, 1826,  73, 24),
            new Rectangle(139, 1868,  89, 24),
            new Rectangle(139, 1910,  76, 24),
            new Rectangle(139, 1951, 162, 24),
            new Rectangle(139, 1994, 101, 24),
            new Rectangle(140, 2042,  88, 18),
            new Rectangle(139, 2498,  55, 24),
            new Rectangle(139, 2540,  76, 24)
        };

        /// <summary>
        /// The minimum area that needs to be covered to be considered a match for redactions.
        /// </summary>
        static readonly Rectangle[] _SSN_MATCHES = new Rectangle[]
        {
            new Rectangle(140,  272, 183, 24),
            new Rectangle(140,  314, 214, 24),
            new Rectangle(139,  356, 189, 24),
            new Rectangle(139,  398, 221, 24),
            new Rectangle(139,  440, 178, 24),
            new Rectangle(139,  482, 211, 24),
            new Rectangle(152,  524, 181, 24),
            new Rectangle(141,  608, 179, 24),
            new Rectangle(141,  650, 211, 24),
            new Rectangle(141,  692, 186, 24),
            new Rectangle(141,  734, 218, 24),
            new Rectangle(140,  902, 118, 66),
            new Rectangle(139, 1028, 130, 66),
            new Rectangle(141, 1154, 109, 66),
            new Rectangle(139, 1280, 100, 66),
            new Rectangle(489, 1532, 159, 24),
            new Rectangle(450, 1574, 160, 24),
            new Rectangle(420, 1616, 162, 24),
            new Rectangle(395, 1658, 162, 24),
            new Rectangle(215, 1700, 160, 24),
            new Rectangle(242, 1742, 160, 24),
            new Rectangle(210, 1784, 159, 24),
            new Rectangle(232, 1826, 162, 24),
            new Rectangle(257, 1868, 161, 24),
            new Rectangle(236, 1910, 160, 24),
            new Rectangle(318, 1952, 161, 24),
            new Rectangle(259, 1994, 162, 24),
            new Rectangle(250, 2036, 159, 24)
        };

        /// <summary>
        /// Test the ability to find social security numbers.
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("Data type rule")]
        public void Automated_FindSocialSecurityNumbers()
        {
            // Step 1: Open the Image

            // Show the ID Shield Office form
            _idShieldOfficeForm.Show();

            // Open the test image
            ImageViewer imageViewer = _idShieldOfficeForm.ImageViewer;
            imageViewer.OpenImage(_testImages.GetFile(_SOCIAL_SECURITY_NUMBER), false);

            // Ensure the image openned
            Assert.That(imageViewer.IsImageAvailable, "Could not open test image.");

            // Step 2: Setup the data type finder form

            // Click the data type finder button
            ToolStripButton button = FormMethods.GetFormComponent<ToolStripButton>(
                _idShieldOfficeForm, "Data type finder");
            button.PerformClick();

            // Ensure the data type rule is displayed
            RuleForm ruleForm = _idShieldOfficeForm.DataTypeRuleForm;
            Assert.That(ruleForm != null && ruleForm.Visible, "Cannot display rule form.");

            // Check the Social security numbers checkbox.
            CheckBox checkBox =
                FormMethods.GetFormComponent<CheckBox>(ruleForm, "Social security numbers");
            checkBox.Checked = true;

            // Step 3: Redact individually

            // Click the Find next button.
            Button findNext = FormMethods.GetFormComponent<Button>(ruleForm, "Find next");
            findNext.PerformClick();

            // Confirm that all the proper clues are found
            AssertMatches<Clue>(imageViewer, _SSN_CLUES);

            // Confirm that the first redaction is highlighted.
            CompositeHighlightLayerObject searchResult = GetSearchResult(imageViewer);
            AssertMatch(searchResult, _SSN_MATCHES[0]);

            // Click the Redact button.
            Button redact = FormMethods.GetFormComponent<Button>(ruleForm, "Redact");
            redact.PerformClick();

            // Confirm that next redaction is highlighted.
            searchResult = GetSearchResult(imageViewer);
            AssertMatch(searchResult, _SSN_MATCHES[1]);

            //// Step 4: Redact all

            // Click the Redact all button.
            Button redactAll = FormMethods.GetFormComponent<Button>(ruleForm, "Redact all");
            redactAll.PerformClick();

            // Confirm that all the proper items are redacted
            AssertMatches<Redaction>(imageViewer, _SSN_MATCHES);
        }

        #endregion Social Security Numbers

        #region Helper Methods

        /// <summary>
        /// Asserts that all the layer objects of the specified type appropriately cover the 
        /// specified areas.
        /// </summary>
        /// <typeparam name="TLayerObject">The type of the layer objects to check.</typeparam>
        /// <param name="imageViewer">The image viewer that contains the layer objects.</param>
        /// <param name="areas">The sensitive areas that need to be covered.</param>
        static void AssertMatches<TLayerObject>(ImageViewer imageViewer, Rectangle[] areas)
            where TLayerObject : CompositeHighlightLayerObject
        {
            // Get all the layer objects of the required type
            List<LayerObject> layerObjects =
                imageViewer.GetLayeredObjects(null, ArgumentRequirement.Any,
                new Type[] { typeof(TLayerObject) }, ArgumentRequirement.Any,
                null, ArgumentRequirement.Any);

            // Ensure the correct number of objects were found
            Assert.That(areas.Length == layerObjects.Count, "Unexpected number of objects found.");

            // Assert that each layer object matches the expected area
            layerObjects.Sort();
            for (int i = 0; i < layerObjects.Count; i++)
            {
                AssertMatch((CompositeHighlightLayerObject)layerObjects[i], areas[i]);
            }
        }

        /// <summary>
        /// Asserts that specified layer object contains the specified area without over-redacting.
        /// </summary>
        /// <param name="layerObject">The layer object to check.</param>
        /// <param name="area">The area the layer object should cover.</param>
        static void AssertMatch(CompositeHighlightLayerObject layerObject, Rectangle area)
        {
            // Get the bounds of the layer object
            Rectangle bounds = layerObject.GetBounds();

            // Assert that the layer object covers the sensitive area
            // Note: The layer object must completely cover the sensitive data to be a match.
            Assert.That(bounds.Contains(area), "Sensitive data not redacted.");

            // Ensure the bounds are reasonable.
            // Note: The layer object must be within five pixels of the data to be a match.
            area.Inflate(5, 5);
            Assert.That(area.Contains(bounds), "Overredaction.");
        }

        /// <summary>
        /// Gets the search result from the specified image viewer.
        /// </summary>
        /// <param name="imageViewer">The image viewer from which to retrieve search results.
        /// </param>
        /// <returns>The search result.</returns>
        static CompositeHighlightLayerObject GetSearchResult(ImageViewer imageViewer)
        {
            // Get all search results, there should be only one at a time
            List<LayerObject> searchResults = imageViewer.GetLayeredObjectsOnPage(
                imageViewer.PageNumber, new string[] { RuleForm.SearchResultTag },
                ArgumentRequirement.Any, new Type[] { typeof(CompositeHighlightLayerObject) },
                ArgumentRequirement.Any, null, ArgumentRequirement.Any);
            Assert.That(searchResults.Count == 1, "Unexpected number of search results.");

            // Return the found search result
            return (CompositeHighlightLayerObject)searchResults[0];
        }

        #endregion Helper Methods
    }
}
