using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System.IO;
using System.Linq;

namespace Extract.DataEntry.Test
{
    [TestFixture]
    [Category("DataEntryCheckBox")]
    public class TestBackgroundFieldModels
    {
        static TestFileManager<TestBackgroundFieldModels> _testFiles;

        // Performs initialization needed for the entire test run
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
            _testFiles = new TestFileManager<TestBackgroundFieldModels>();
        }

        // Performs tear down needed after the entire test run
        [OneTimeTearDown]
        public static void Teardown()
        {
            _testFiles?.Dispose();
            _testFiles = null;
        }

        // Confirm that the class name and properties are serialized correctly for DataEntryCheckBoxes
        [Test, Category("Automated")]
        public static void DataEntryCheckBox_BackgroundFieldModel_Generation()
        {
            // Arrange
            var expectedModelJson = File.ReadAllText(_testFiles.GetFile("Resources.DataEntryCheckBoxBackgroundFieldModelList.json"));

            var dataEntryCheckBoxes = new[]
            {
                new DataEntryCheckBox() { Name = "YesNoDefaultYes", CheckedValue = "Yes", UncheckedValue = "No", DefaultCheckedState = true },
                new DataEntryCheckBox() { Name = "YesNoDefaultNo", CheckedValue = "Yes", UncheckedValue = "No", DefaultCheckedState = false },
                new DataEntryCheckBox() { Name = "TrueFalseDefaultTrue", CheckedValue = "True", UncheckedValue = "False", DefaultCheckedState = true },
                new DataEntryCheckBox() { Name = "TrueFalseDefaultFalse", CheckedValue = "True", UncheckedValue = "False", DefaultCheckedState = false }
            };

            // Set other props to be non-default for some of the controls to confirm that they are properly set in the background model
            for (int i = 1; i < dataEntryCheckBoxes.Length; i+=2)
            {
                var checkBox = dataEntryCheckBoxes[i];
                checkBox.AttributeName = "non-def AN";
                checkBox.AutoUpdateQuery = "non-def AUQ";
                checkBox.ValidationQuery = "non-def VQ";
                checkBox.Visible = false;
                checkBox.PersistAttribute = false;
                checkBox.ValidationErrorMessage = "non-def VEM";
            }

            // Act
            BackgroundModel model = new()
            {
                Fields = dataEntryCheckBoxes.Select(checkBox => checkBox.GetBackgroundFieldModel()).ToList()
            };

            // Assert
            var actualModelJson = model.ToJson();
            Assert.AreEqual(expectedModelJson, actualModelJson);
        }

        // Confirm that the class name and properties are serialized correctly for DataEntryTableColumns
        // isParentVisible confirms that the visibility property of the parent table affects the column model's IsViewable property
        [Test, Category("Automated")]
        public static void DataEntryTableColumn_BackgroundFieldModel_Generation([Values] bool isParentVisible)
        {
            // Arrange
            var expectedModelJson = File.ReadAllText(_testFiles.GetFile(UtilityMethods.FormatInvariant(
                $"Resources.DataEntryTableColumnBackgroundFieldModelList_{isParentVisible}.json")));

            DataEntryTableColumn[] dataEntryColumns = new[]
            {
                new DataEntryTableColumn() { Name = "TextBoxDefault", UseComboBoxCells = false },
                new DataEntryTableColumn() { Name = "TextBoxNonDefault", UseComboBoxCells = false },
                new DataEntryTableColumn() { Name = "ComboBoxDefault", UseComboBoxCells = true },
                new DataEntryTableColumn() { Name = "ComboBoxNonDefault", UseComboBoxCells = true }
            };

            // Set other props to be non-default for some of the controls to confirm that they are properly set in the background model
            for (int i = 1; i < dataEntryColumns.Length; i+=2)
            {
                var column = dataEntryColumns[i];
                column.AttributeName = "non-def AN";
                column.AutoUpdateQuery = "non-def AUQ";
                column.ValidationQuery = "non-def VQ";
                column.Visible = false;
                column.PersistAttribute = false;
                column.ValidationErrorMessage = "non-def VEM";
            }

            // Create the parent control of the column
            DataEntryTable dataEntryTable = new();
            dataEntryTable.Columns.AddRange(dataEntryColumns);

            // Set the Visible property of the parent control to confirm that this is propagated to the children when the model is generated
            dataEntryTable.Visible = isParentVisible;

            // There should still be some columns that are marked visible and some that are not
            Assume.That(dataEntryColumns.Any(column => column.Visible));
            Assume.That(dataEntryColumns.Any(column => !column.Visible));

            // Act
            BackgroundModel model = new()
            {
                Fields = dataEntryColumns.Select(checkBox => checkBox.GetBackgroundFieldModel()).ToList()
            };

            // Assert
            var actualModelJson = model.ToJson();
            Assert.AreEqual(expectedModelJson, actualModelJson);
        }

        // Confirm that the class name and properties are serialized correctly for DataEntryCheckBoxColumns
        // isParentVisible confirms that the visibility property of the parent table affects the column model's IsViewable property
        [Test, Category("Automated")]
        public static void DataEntryCheckBoxColumn_BackgroundFieldModel_Generation([Values] bool isParentVisible)
        {
            // Arrange
            var expectedModelJson = File.ReadAllText(_testFiles.GetFile(UtilityMethods.FormatInvariant(
                $"Resources.DataEntryCheckBoxColumnBackgroundFieldModelList_{isParentVisible}.json")));

            DataEntryCheckBoxColumn[] dataEntryCheckBoxColumns = new[]
            {
                new DataEntryCheckBoxColumn() { Name = "YesNoDefaultYes", CheckedValue = "Yes", UncheckedValue = "No", DefaultCheckedState = true },
                new DataEntryCheckBoxColumn() { Name = "YesNoDefaultNo", CheckedValue = "Yes", UncheckedValue = "No", DefaultCheckedState = false },
                new DataEntryCheckBoxColumn() { Name = "TrueFalseDefaultTrue", CheckedValue = "True", UncheckedValue = "False", DefaultCheckedState = true },
                new DataEntryCheckBoxColumn() { Name = "TrueFalseDefaultFalse", CheckedValue = "True", UncheckedValue = "False", DefaultCheckedState = false }
            };

            // Set other props to be non-default for some of the controls to confirm that they are properly set in the background model
            for (int i = 1; i < dataEntryCheckBoxColumns.Length; i+=2)
            {
                var column = dataEntryCheckBoxColumns[i];
                column.AttributeName = "non-def AN";
                column.AutoUpdateQuery = "non-def AUQ";
                column.ValidationQuery = "non-def VQ";
                column.Visible = false;
                column.PersistAttribute = false;
                column.ValidationErrorMessage = "non-def VEM";
            }

            DataEntryTable dataEntryTable = new();
            dataEntryTable.Columns.AddRange(dataEntryCheckBoxColumns);

            // Set the Visible property of the parent control to confirm that this is propagated to the children when the model is generated
            dataEntryTable.Visible = isParentVisible;

            // There should still be some columns that are marked visible and some that are not
            Assume.That(dataEntryCheckBoxColumns.Any(column => column.Visible));
            Assume.That(dataEntryCheckBoxColumns.Any(column => !column.Visible));

            // Act
            BackgroundModel model = new()
            {
                Fields = dataEntryCheckBoxColumns.Select(checkBox => checkBox.GetBackgroundFieldModel()).ToList()
            };

            // Assert
            var actualModelJson = model.ToJson();
            Assert.AreEqual(expectedModelJson, actualModelJson);
        }

        // Confirm that NormalizeValue(string) works correctly for DataEntryCheckBoxBackgroundFieldModel
        [Category("Automated")]
        [TestCase("Yes", false, ExpectedResult = "Yes")]
        [TestCase("No", false, ExpectedResult = "No")]
        [TestCase("Other", false, ExpectedResult = "No")]
        [TestCase("", false, ExpectedResult = "No")]
        [TestCase(null, false, ExpectedResult = "No")]
        [TestCase("Yes", true, ExpectedResult = "Yes")]
        [TestCase("No", true, ExpectedResult = "No")]
        [TestCase("Other", true, ExpectedResult = "Yes")]
        [TestCase("", true, ExpectedResult = "Yes")]
        [TestCase(null, true, ExpectedResult = "Yes")]
        public static string DataEntryCheckBoxBackgroundFieldModel_NormalizeValue(string inputValue, bool defaultToChecked)
        {
            // Arrange
            DataEntryCheckBoxBackgroundFieldModel model = new()
            {
                CheckedValue = "Yes",
                UncheckedValue = "No",
                DefaultCheckedState = defaultToChecked
            };

            // Act
            string result = model.NormalizeValue(inputValue);

            // Assert
            return result;
        }
    }
}
