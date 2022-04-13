using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Database.Test
{
    public enum TestUser
    {
        NoUser = 0,
        CurrentUser = 1,
        AnotherUser = 2
    }

    internal static class UserQueueTestUtils
    {
        public static
            (TestDatabase<TestFAMFileProcessing> fpDB,
            FileProcessingDB workflow,
            FileProcessingDB session,
            string action)
            SetupTest(FAMTestDBManager<TestFAMFileProcessing> testDbManager, string testDBName, int workflowCount, bool allWorkflows, TestUser user)
        {
            var fpDB = new TestDatabase<TestFAMFileProcessing>(testDbManager, testDBName,
                workflowCount, actionCount: 1, enableLoadBalancing: true);
            fpDB.FileProcessingDB.ExecuteCommandQuery(
                "INSERT INTO [FAMUser] ([UserName], [FullUserName]) VALUES ('User2','User Two')");

            var workflow = fpDB.Workflows[0];
            var session = allWorkflows ? fpDB.wfAll : fpDB.Workflows[0];
            var action = workflow.GetActiveActionName();
            Assert.AreEqual(1, fpDB.AddFakeFile(1, setAsSkipped: false));

            workflow.SetStatusForFileForUser(1,
                action, workflow.GetWorkflowID(),
                GetName(user),
                EActionStatus.kActionPending,
                vbQueueChangeIfProcessing: false,
                vbAllowQueuedStatusOverride: false,
                out var _);

            return (fpDB, workflow, session, action);
        }

        /// <summary>
        /// Returns the Username associated with the ID's passed
        /// </summary>
        /// <param name="user">Id of user to return</param>
        /// <returns></returns>
        public static string GetName(TestUser? user)
        {
            return user switch
            {
                null => "",
                TestUser.NoUser => "",
                TestUser.CurrentUser => Environment.UserName,
                TestUser.AnotherUser => "User2",
                _ => throw new ArgumentException("Invalid TestUser")
            };
        }

        /// <summary>
        /// Helper for TestGetFilesToProcessAdvanced/TestUserSpecificQueue to confirm if files queued
        /// for the specified user should be returned by GetFilesToProcess.
        /// </summary>
        public static bool QualifiesForQueue(this TestUser user, bool limitToUserQueue, bool includeFilesQueuedForOthers)
        {
            // Intentionally phrased this logic in a different way than in GetFilesToProcess to better confirm the logic.
            if (limitToUserQueue)
            {
                return user != TestUser.NoUser
                    && (includeFilesQueuedForOthers || user == TestUser.CurrentUser);
            }
            else // !limitToUserQueue
            {
                return includeFilesQueuedForOthers || user == TestUser.NoUser || user == TestUser.CurrentUser;
            }
        }

        public static void CheckFinalState(this FileProcessingDB workflow, string action, TestUser user, TestUser? overrideForUser, bool completeFile)
        {
            var expectedStatus = completeFile
                ? overrideForUser switch
                {
                    null => EActionStatus.kActionCompleted,
                    TestUser.NoUser => EActionStatus.kActionPending,
                    TestUser.CurrentUser => EActionStatus.kActionPending,
                    TestUser.AnotherUser => EActionStatus.kActionPending,
                    _ => throw new ArgumentException("Invalid queueForUser")
                }
                : EActionStatus.kActionPending;

            Assert.AreEqual(expectedStatus, workflow.GetFileStatus(1, action, false));

            TestUser? expectedUser = completeFile
                ? overrideForUser ?? user
                : user;
            using var results = workflow.GetQueryResults(
                "SELECT [UserID] FROM [FileActionStatus] WHERE [FileID] = 1");

            Assert.AreEqual(1, results.Rows.Count);
            if (expectedUser == null || expectedUser == TestUser.NoUser)
            {
                Assert.AreEqual(DBNull.Value, results.Rows[0].ItemArray[0]);
            }
            else
            {
                Assert.AreNotEqual(DBNull.Value, results.Rows[0].ItemArray[0]);
                Assert.AreEqual(expectedUser, (TestUser)results.Rows[0].ItemArray[0]);
            }
        }
    }
}
