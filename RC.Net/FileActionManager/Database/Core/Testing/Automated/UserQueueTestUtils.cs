using NUnit.Framework;
using System;
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
        public static MiseEnPlace SetupTest(
            FAMTestDBManager<TestFAMFileProcessing> testDbManager,
            string testDBName,
            int workflowCount,
            bool allWorkflows,
            TestUser user)
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

            return new MiseEnPlace(fpDB, workflow, session, action);
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
        public static bool QualifiesForQueue(this TestUser user, EQueueType queueType)
        {
            return queueType switch
            {
                EQueueType.kPendingSpecifiedUser or EQueueType.kSkippedSpecifiedUser => user == TestUser.CurrentUser,
                EQueueType.kPendingSpecifiedUserOrNoUser => user == TestUser.CurrentUser || user == TestUser.NoUser,
                _ => true
            };
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

    internal class MiseEnPlace : IDisposable
    {
        public TestDatabase<TestFAMFileProcessing> fpDB;
        public FileProcessingDB workflow;
        public FileProcessingDB session;
        public string action;

        public MiseEnPlace(TestDatabase<TestFAMFileProcessing> fpDB, FileProcessingDB workflow, FileProcessingDB session, string action)
        {
            this.fpDB = fpDB;
            this.workflow = workflow;
            this.session = session;
            this.action = action;
        }

        public void Deconstruct(out TestDatabase<TestFAMFileProcessing> fpDB, out FileProcessingDB workflow, out FileProcessingDB session, out string action)
        {
            fpDB = this.fpDB;
            workflow = this.workflow;
            session = this.session;
            action = this.action;
        }

        public void Dispose()
        {
            fpDB.Dispose();
        }
    }
}
