using Extract.Database;
using Extract.Database.Sqlite;
using Extract.Testing.Utilities;
using Extract.Utilities.ContextTags.SqliteModels.Version3;
using LinqToDB;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Extract.Utilities.ContextTags.Test
{
    [TestFixture]
    [Category("ContextTagProvider")]
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class TestContextTagProvider
    {
        TemporaryFile _currentCustomTagsDBFile;
        ContextTagsSqliteDatabaseManager _manager;
        ContextTagProvider _contextTagProvider;

        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
        }

        [SetUp]
        public void PerTestSetup()
        {
            // Create the database in a random subfolder of the temp dir
            _currentCustomTagsDBFile = new(null, "CustomTags.sqlite", null, false);
            File.Delete(_currentCustomTagsDBFile.FileName);
            _manager = new ContextTagsSqliteDatabaseManager(_currentCustomTagsDBFile.FileName);
            _manager.CreateDatabase();

            // Add a context and two tags
            using var db = new CustomTagsDB(SqliteMethods.BuildConnectionOptions(_currentCustomTagsDBFile.FileName));

            var contextPath = Path.GetDirectoryName(_currentCustomTagsDBFile.FileName);
            var context1 = new Context { Name = "Context1", FPSFileDir = contextPath };
            context1.ID = (long)db.InsertWithIdentity(context1);

            var tag1 = new CustomTag { Name = "Tag1" };
            tag1.ID = (long)db.InsertWithIdentity(tag1);

            var tag2 = new CustomTag { Name = "Tag2" };
            tag2.ID = (long)db.InsertWithIdentity(tag2);

            // Set default values
            AddTagValue(db, context1, tag1, "", "C1Default1");
            AddTagValue(db, context1, tag2, "", "C1Default2");

            // Override both values for workflow 1 and one value for workflow 2
            AddTagValue(db, context1, tag1, "WF1", "C1WF1Value1");
            AddTagValue(db, context1, tag2, "WF1", "C1WF1Value2");
            AddTagValue(db, context1, tag1, "WF2", "C1WF2Value1");

            // Add a second context and set one value
            var context2 = new Context { Name = "Context2", FPSFileDir = @"C:\FSPFiles" };
            context2.ID = (long)db.InsertWithIdentity(context2);
            AddTagValue(db, context2, tag2, "WF1", "C2WF1Value2");

            // Init the provider
            _contextTagProvider = new();
            _contextTagProvider.ContextPath = contextPath;
        }

        [TearDown]
        public void Teardown()
        {
            _currentCustomTagsDBFile.Dispose();
        }

        [Test, Category("Automated")]
        public void GetTagNames()
        {
            var expectedTagNames = new[] { "Tag1", "Tag2", "Edit custom tags..."};
            var tagNames = _contextTagProvider.GetTagNames().ToIEnumerable<string>().ToList();

            CollectionAssert.AreEquivalent(expectedTagNames, tagNames);
        }

        /// Get default value of a tag
        [Test, Category("Automated")]
        public void GetTagValue_NoWorkflow()
        {
            Assert.AreEqual("C1Default2", _contextTagProvider.GetTagValue("Tag2", ""));
        }

        /// Get default value of a tag when not overridden by specified workflow
        [Test, Category("Automated")]
        public void GetTagValue_Default()
        {
            Assert.AreEqual("C1Default2", _contextTagProvider.GetTagValue("Tag2", "WF2"));
        }

        /// Get workflow-specific value of a tag
        [Test, Category("Automated")]
        public void GetTagValue_WorkflowOverride()
        {
            Assert.AreEqual("C1WF1Value2", _contextTagProvider.GetTagValue("Tag2", "WF1"));
        }

        /// Confirm that 'undefined' means empty value for a tag
        [Test, Category("Automated")]
        public void GetUndefinedTags_EmptyValues()
        {
            CollectionAssert.IsEmpty(_contextTagProvider.GetUndefinedTags("WF1").ToIEnumerable<string>());
            CollectionAssert.IsEmpty(_contextTagProvider.GetUndefinedTags("WF2").ToIEnumerable<string>());

            using var db = new CustomTagsDB(SqliteMethods.BuildConnectionOptions(_currentCustomTagsDBFile.FileName));

            // Set a tag1 to be empty for WF1
            var tagValue = db.TagValues.Where(t =>
                t.Context.Name == "Context1" &&
                t.Tag.Name == "Tag1" &&
                t.Workflow == "WF1").First();
            tagValue.Value = "";
            db.Update(tagValue);

            // Set tag2 to be empty for the default
            tagValue = db.TagValues.Where(t =>
                t.Context.Name == "Context1" &&
                t.Tag.Name == "Tag2" &&
                t.Workflow.Length == 0).First();
            tagValue.Value = "";
            db.Update(tagValue);

            // Reload info from the database
            _contextTagProvider.RefreshTags();

            // No tags undefined for WF1
            // Now tag1 is undefined for WF1
            var undefined = _contextTagProvider.GetUndefinedTags("WF1").ToIEnumerable<string>().ToList();
            CollectionAssert.AreEquivalent(new[] { "Tag1" }, undefined);

            // Tag2 is undefined for WF2
            undefined = _contextTagProvider.GetUndefinedTags("WF2").ToIEnumerable<string>().ToList();
            CollectionAssert.AreEquivalent(new[] { "Tag2" }, undefined);
        }

        /// Confirm that 'undefined' means no value available for a tag; 'defined' means either default value or workflow override is available
        /// NOTE: This is currently broken because tags that are missing are not reported. Only tags with empty or null values are reported as undefined
        /// This behavior was true in 11.7 and has probably been broken for a while. See https://extract.atlassian.net/browse/ISSUE-17668
        [Test, Category("Automated"), Category("Broken")]
        public void Broken_GetUndefinedTags_MissingValues()
        {
            CollectionAssert.IsEmpty(_contextTagProvider.GetUndefinedTags("WF1").ToIEnumerable<string>());
            CollectionAssert.IsEmpty(_contextTagProvider.GetUndefinedTags("WF2").ToIEnumerable<string>());

            // Create a new context folder
            using TemporaryFile copyOfCustomTagsDBFile = new(null, "CustomTags.sqlite", null, false);
            File.Copy(_currentCustomTagsDBFile.FileName, copyOfCustomTagsDBFile.FileName, true);
            string contextPath = Path.GetDirectoryName(copyOfCustomTagsDBFile.FileName);

            // Update the context that has undefined tags to use the new path
            using var db = new CustomTagsDB(SqliteMethods.BuildConnectionOptions(copyOfCustomTagsDBFile.FileName));
            var context2 = db.Contexts.Where(c => c.Name == "Context2").First();
            context2.FPSFileDir = contextPath;
            db.Update(context2);

            // Point the provider at the new context
            _contextTagProvider.ContextPath = contextPath;

            // Now one tag is undefined for WF1
            var undefined = _contextTagProvider.GetUndefinedTags("WF1").ToIEnumerable<string>().ToList();
            CollectionAssert.AreEquivalent(new[] { "Tag1" }, undefined);

            // Both tags are undefined for WF2
            undefined = _contextTagProvider.GetUndefinedTags("WF2").ToIEnumerable<string>().ToList();
            CollectionAssert.AreEquivalent(new[] { "Tag1", "Tag2" }, undefined);
        }

        /// Confirm that stale values are recalculated after a call to RefreshTags()
        [Test, Category("Automated")]
        public void RefreshTags()
        {
            using var db = new CustomTagsDB(SqliteMethods.BuildConnectionOptions(_currentCustomTagsDBFile.FileName));

            // Set value of tag1
            var tagValue = db.TagValues.Where(t =>
                t.Context.Name == "Context1" &&
                t.Tag.Name == "Tag1" &&
                t.Workflow == "WF1").First();
            tagValue.Value = "NewValue!";
            db.Update(tagValue);

            // Provider still has stale info
            var oldValue = _contextTagProvider.GetTagValue("Tag1", "WF1");
            Assert.AreEqual("C1WF1Value1", oldValue);

            // Reload info from the database
            _contextTagProvider.RefreshTags();
            var newValue = _contextTagProvider.GetTagValue("Tag1", "WF1");
            Assert.AreEqual("NewValue!", newValue);
        }

        /// All workflows that appear in the TagValue table are returned
        [Test, Category("Automated")]
        public void GetWorkflowsThatHaveValues()
        {
            var workflows = _contextTagProvider.GetWorkflowsThatHaveValues().ToIEnumerable<string>().ToList();
            CollectionAssert.AreEquivalent(new[] { "", "WF1", "WF2" }, workflows);
        }

        /// Test that correct values are returned for each workflow (essentially the same test as appears in TestContextTagDatabaseManager)
        [TestCase("", TestName = "GetTagValuePairsForWorkflow_Default")]
        [TestCase("WF1", TestName = "GetTagValuePairsForWorkflow_WF1")]
        [TestCase("WF2", TestName = "GetTagValuePairsForWorkflow_WF2")]
        [Category("Automated")]
        public void GetTagValuePairsForWorkflow(string workflow)
        {
            Dictionary<string, Dictionary<string, string>> expectedContextTagValues = new()
            {
                { "", new Dictionary<string, string> { {"Tag1", "C1Default1"}, {"Tag2", "C1Default2"}, { "Edit custom tags...", "" } } },
                { "WF1", new Dictionary<string, string> { {"Tag1", "C1WF1Value1"}, {"Tag2", "C1WF1Value2"} } },
                { "WF2", new Dictionary<string, string> { {"Tag1", "C1WF2Value1"}, {"Tag2", "C1Default2"} } },
            };

            var tagValuePairs = _contextTagProvider.GetTagValuePairsForWorkflow(workflow).ComToDictionary();

            CollectionAssert.AreEqual(expectedContextTagValues[workflow], tagValuePairs);
        }

        /// Confirm that no update is required for a new database
        [Test, Category("Automated")]
        public void IsUpdateRequired_IsFalseForNewDatabase()
        {
            Assert.IsFalse(_contextTagProvider.IsUpdateRequired(_contextTagProvider.ContextPath));
        }

        /// Confirm update is required for an old database version
        [Test, Category("Automated")]
        public void IsUpdateRequired_IsTrueWhenVersionIsOld()
        {
            // Change the schema version
            using var db = new CustomTagsDB(SqliteMethods.BuildConnectionOptions(_currentCustomTagsDBFile.FileName));
            db.Update(new Settings { Name = CustomTagsDBSettings.ContextTagsDBSchemaVersionKey, Value = "1" });

            Assert.IsTrue(_contextTagProvider.IsUpdateRequired(_contextTagProvider.ContextPath));
        }

        /// Confirm that an exception is thrown when attempting an invalid upgrade from current to current
        [Test, Category("Automated")]
        public void UpdateContextTagsDB_ThrowsExceptionWhenSchemaIsCurrent()
        {
            var ex = Assert.Throws<ExtractException>(() => _contextTagProvider.UpdateContextTagsDB(_contextTagProvider.ContextPath));
            var uex = ExtractException.FromStringizedByteStream("ELI51832", ex.Message);
            var innerExn = uex.InnerException;
            StringAssert.AreEqualIgnoringCase("Database version must be less than the current schema version", innerExn.Message);
        }

        /// The ContextTagProvider no longer keeps the database open so CloseDatabase does nothing
        [Test, Category("Automated")]
        public void CloseDatabase()
        {
            Assert.DoesNotThrow(_contextTagProvider.CloseDatabase);
        }

        #region Private Methods

        // Simplify adding a tag value
        private static void AddTagValue(
            CustomTagsDB db,
            Context context,
            CustomTag tag,
            string workflow,
            string value)
        {
            db.Insert(new TagValue
            {
                ContextID = (int)context.ID,
                TagID = (int)tag.ID,
                Workflow = workflow,
                Value = value
            });
        }
        #endregion Private Methods
    }
}