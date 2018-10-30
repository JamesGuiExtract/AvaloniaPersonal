using Extract.Interop;
using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using UCLID_FILEPROCESSORSLib;

namespace Extract.FileActionManager.FileProcessors.Test
{
    /// <summary>
    /// Provides unit test cases for the <see cref="ValidateXmlTask"/>.
    /// </summary>
    [TestFixture]
    [Category("DisplayUIPropertyForTasks")]
    public class TestDisplayUIPropertyForTasks
    {
        #region Constants

        /// <summary>
        /// This value should be the total of all the file processors when all items are licensed.
        /// This should be updated if new File processors are created
        /// </summary>
        const int _NUMBER_OF_FILE_PROCESSORS = 39;

        /// <summary>
        /// This list contains the text that will only be contained in descriptions of tasks
        /// that are Display a UI.
        /// </summary>
        static readonly List<string> _UI_SELECTION_LIST = new List<string> { "View", "Verify", "Paginate" };

        #endregion

        #region Overhead Methods

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestFixtureTearDown]
        public static void FinalCleanup()
        {

        }

        #endregion Overhead Methods

        #region Unit Tests

        /// <summary>
        /// Tests that all the file processing tasks return appropriate values for DisplayUI
        /// 
        /// </summary>
        [Test, Category("DisplayUI")]
        public static void TestDisplayUIProperty()
        {
            CategoryManager categoryManager = new CategoryManager();
            var fileProcessorsProgIDs = categoryManager.GetDescriptionToProgIDMap1(ExtractCategories.FileProcessorsName);

            // This is testing that all the file processor tasks we have are licensed, this number will need to be changed if
            // other file processing tasks added
            Assert.AreEqual(_NUMBER_OF_FILE_PROCESSORS, fileProcessorsProgIDs.Size, "Checking that all file processing tasks are registered and licensed");

            var processorsProgIDs = fileProcessorsProgIDs.ComToDictionary();

            // Test the Tasks that have a UI           
            var uiTasks = processorsProgIDs.Where(t => _UI_SELECTION_LIST.Any(w => t.Key.Contains(w)));
            foreach (var k in uiTasks)
            {
                string progID = fileProcessorsProgIDs.GetValue(k.Key);
                Type t = Type.GetTypeFromProgID(progID);
                IFileProcessingTask task = Activator.CreateInstance(t) as IFileProcessingTask;

                Assert.IsNotNull(task, "Unable to create " + progID);
                Assert.That(task.DisplaysUI, "UI task has incorrect DisplaysUI value " + progID);
            }

            var nonUITasks = processorsProgIDs.Except(uiTasks);
            foreach (var k in nonUITasks)
            {
                string progID = fileProcessorsProgIDs.GetValue(k.Key);
                Type t = Type.GetTypeFromProgID(progID);
                IFileProcessingTask task = Activator.CreateInstance(t) as IFileProcessingTask;

                Assert.IsNotNull(task, "Unable to create " + progID);
                Assert.That(!task.DisplaysUI, "Non UI task has incorrect DisplaysUI value " + progID);
            }
        }

        /// <summary>
        /// Tests that the conditional task checks its true and false tasks when calling the DisplayUI property
        /// </summary>
        [Test, Category("DisplayUI")]
        public static void TestDisplayUIPropertyForConditionalTask()
        {
            ConditionalTask conditionalTask = new ConditionalTask();
            IFileProcessingTask task = conditionalTask as IFileProcessingTask;

            // Create an ObjectWithDiscription that has a UI object in it
            ObjectWithDescription UIObjectWithDescription = new ObjectWithDescription
            {
                Enabled = true,

                // Use the pagination task as the UI object to test
                Object = new PaginationTask()
            };

            // Create an ObjectWithdiscription that has a non UI object in it
            ObjectWithDescription NonUIObjectWithDiscription = new ObjectWithDescription
            {
                Enabled = true,

                // Use the copy move delete file processor as non UI object to test
                Object = new CopyMoveDeleteFileProcessor()
            };

            // Test true tasks with UI object
            conditionalTask.TasksForConditionTrue.PushBack(UIObjectWithDescription);

            Assert.That(!task.DisplaysUI, "Conditional task with UI object in True tasks should always return false.");

            // Test true tasks with non UI object
            conditionalTask.TasksForConditionTrue.Clear();
            conditionalTask.TasksForConditionTrue.PushBack(NonUIObjectWithDiscription);

            Assert.That(!task.DisplaysUI, "Conditional task with non UI object in True tasks");

            // Test false tasks with UI object
            conditionalTask.TasksForConditionTrue.Clear();
            conditionalTask.TasksForConditionFalse.Clear();
            conditionalTask.TasksForConditionFalse.PushBack(UIObjectWithDescription);

            Assert.That(!task.DisplaysUI, "Conditional task with UI object in False tasks should always return false");

            // Test false tasks with non UI object
            conditionalTask.TasksForConditionFalse.Clear();
            conditionalTask.TasksForConditionFalse.PushBack(NonUIObjectWithDiscription);

            Assert.That(!task.DisplaysUI, "Conditional task with non UI object in False tasks");
        }

        #endregion
    }
}