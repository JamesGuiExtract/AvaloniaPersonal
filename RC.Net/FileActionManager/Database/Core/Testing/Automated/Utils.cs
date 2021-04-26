using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Database.Test
{
    #region FileProcessingDB Wrappers

    /// <summary>
    /// Wrapper for a FileProcessingDB with a single workflow that takes care of initialization and cleanup
    /// </summary>
    internal class OneWorkflow<T> : IDisposable
    {
        public readonly string action1 = "Action1";
        public readonly string action2 = "Action2";

        public readonly FileProcessingDB fpDB = null;

        readonly string testDBName;
        readonly FAMTestDBManager<T> testDBManager;

        public OneWorkflow(FAMTestDBManager<T> testDBManager, string testDBName, bool enableLoadBalancing)
        {
            this.testDBManager = testDBManager;
            this.testDBName = testDBName;

            // Setup DB
            fpDB = testDBManager.GetNewDatabase(testDBName);
            fpDB.SetDBInfoSetting("EnableLoadBalancing", enableLoadBalancing ? "1" : "0", true, false);
            fpDB.DefineNewAction(action1);
            fpDB.DefineNewAction(action2);

            int workflowID = fpDB.AddWorkflow("Workflow1", EWorkflowType.kUndefined, action1, action2);
            Assert.AreEqual(1, workflowID);

            fpDB.ActiveWorkflow = "Workflow1";

            // Start a session
            fpDB.RecordFAMSessionStart("Test.fps", action1, true, true);
            fpDB.RegisterActiveFAM();
        }

        public int addFakeFile(int fileNumber, bool setAsSkipped, EFilePriority priority = EFilePriority.kPriorityNormal)
        {
            var fileName = Path.Combine(Path.GetTempPath(), fileNumber.ToString("N3", CultureInfo.InvariantCulture) + ".tif");
            int fileID = fpDB.AddFileNoQueue(fileName, 0, 0, priority, 1);
            if (setAsSkipped)
            {
                fpDB.SetFileStatusToSkipped(fileID, action1, false, false);
                fpDB.SetFileStatusToSkipped(fileID, action2, false, false);
            }
            else
            {
                fpDB.SetFileStatusToPending(fileID, action1, false);
                fpDB.SetFileStatusToPending(fileID, action2, false);
            }

            return fileID;
        }

        public void Dispose()
        {
            if (fpDB != null)
            {
                try
                {
                    // Prevent 'files were reverted' log
                    fpDB.SetStatusForAllFiles(action1, EActionStatus.kActionUnattempted);
                    fpDB.SetStatusForAllFiles(action2, EActionStatus.kActionUnattempted);
                    fpDB.UnregisterActiveFAM();
                    fpDB.RecordFAMSessionStop();
                }
                catch { }
            }
            testDBManager.RemoveDatabase(testDBName);
        }
    }

    /// <summary>
    /// Wrapper for a FileProcessingDB with two workflows that takes care of initialization and cleanup
    /// </summary>
    internal class TwoWorkflows<T> : IDisposable
    {
        public readonly string action1 = "Action1";
        public readonly string action2 = "Action2";

        public readonly FileProcessingDB wf1 = null;
        public readonly FileProcessingDB wf2 = null;
        public readonly FileProcessingDB wfAll = null;

        readonly FileProcessingDB[] fpDBs = null;
        readonly string testDBName;
        readonly FAMTestDBManager<T> testDBManager;

        public TwoWorkflows(FAMTestDBManager<T> testDBManager, string testDBName, bool enableLoadBalancing)
        {
            this.testDBManager = testDBManager;
            this.testDBName = testDBName;

            // Setup DB
            wf1 = testDBManager.GetNewDatabase(testDBName);
            wf1.SetDBInfoSetting("EnableLoadBalancing", enableLoadBalancing ? "1" : "0", true, false);
            wf1.DefineNewAction(action1);
            wf1.DefineNewAction(action2);

            int workflow1 = wf1.AddWorkflow("Workflow1", EWorkflowType.kUndefined, action1, action2);
            Assert.AreEqual(1, workflow1);
            int workflow2 = wf1.AddWorkflow("Workflow2", EWorkflowType.kUndefined, action1, action2);
            Assert.AreEqual(2, workflow2);

            // Configure a separate object for each workflow configuration needed
            wf1.ActiveWorkflow = "Workflow1";
            wf2 = new FileProcessingDBClass
            {
                DatabaseServer = wf1.DatabaseServer,
                DatabaseName = wf1.DatabaseName,
                ActiveWorkflow = "Workflow2"
            };
            wfAll = new FileProcessingDBClass
            {
                DatabaseServer = wf1.DatabaseServer,
                DatabaseName = wf1.DatabaseName,
                ActiveWorkflow = ""
            };

            // Start a session for each DB object
            fpDBs = new[] { wf1, wf2, wfAll };
            foreach (var fpDB in fpDBs)
            {
                fpDB.RecordFAMSessionStart("Test.fps", action1, true, true);
                fpDB.RegisterActiveFAM();
            }
        }

        public int addFakeFile(int fileNumber, bool setAsSkipped, EFilePriority priority = EFilePriority.kPriorityNormal)
        {
            var fileName = Path.Combine(Path.GetTempPath(), fileNumber.ToString("N3", CultureInfo.InvariantCulture) + ".tif");
            int fileID = wf1.AddFileNoQueue(fileName, 0, 0, priority, 1);
            if (setAsSkipped)
            {
                wf1.SetFileStatusToSkipped(fileID, action1, false, false);
                wf1.SetFileStatusToSkipped(fileID, action2, false, false);
                wf2.SetFileStatusToSkipped(fileID, action1, false, false);
                wf2.SetFileStatusToSkipped(fileID, action2, false, false);
            }
            else
            {
                wf1.SetFileStatusToPending(fileID, action1, false);
                wf1.SetFileStatusToPending(fileID, action2, false);
                wf2.SetFileStatusToPending(fileID, action1, false);
                wf2.SetFileStatusToPending(fileID, action2, false);
            }

            return fileID;
        }

        public void startNewSession(FileProcessingDB fpDB)
        {
            fpDB.UnregisterActiveFAM();
            fpDB.RecordFAMSessionStop();
            fpDB.RecordFAMSessionStart("Test.fps", action1, true, true);
            fpDB.RegisterActiveFAM();
        }

        public void Dispose()
        {
            foreach (var fpDB in fpDBs)
            {
                if (fpDB != null)
                {
                    try
                    {
                        // Prevent 'files were reverted' log
                        fpDB.SetStatusForAllFiles(action1, EActionStatus.kActionUnattempted);
                        fpDB.SetStatusForAllFiles(action2, EActionStatus.kActionUnattempted);
                        fpDB.UnregisterActiveFAM();
                        fpDB.RecordFAMSessionStop();
                    }
                    catch { }
                }
            }
            testDBManager.RemoveDatabase(testDBName);
        }

        public int getTotalProcessing()
        {
            var action1Stats = wfAll.GetStatsAllWorkflows(action1, false);
            var action2Stats = wfAll.GetStatsAllWorkflows(action2, false);
            int totalFilesInDB = action1Stats.NumDocuments + action2Stats.NumDocuments;
            int notProcessing = 0;
            notProcessing += action1Stats.NumDocumentsSkipped;
            notProcessing += action2Stats.NumDocumentsSkipped;
            notProcessing += action1Stats.NumDocumentsPending;
            notProcessing += action2Stats.NumDocumentsPending;
            notProcessing += action1Stats.NumDocumentsComplete;
            notProcessing += action2Stats.NumDocumentsComplete;
            notProcessing += action1Stats.NumDocumentsFailed;
            notProcessing += action2Stats.NumDocumentsFailed;
            return totalFilesInDB - notProcessing;
        }
    }

    #endregion FileProcessingDB Wrappers

    #region Statistics Helper Classes

    // FileIDs for each status
    internal class ActionStatus
    {
        public int[] P = Array.Empty<int>();
        public int[] R = Array.Empty<int>();
        public int[] S = Array.Empty<int>();
        public int[] C = Array.Empty<int>();
        public int[] F = Array.Empty<int>();

        public ActionStatus() { }

        ActionStatus(int[][] statuses)
        {
            P = statuses[0];
            R = statuses[1];
            S = statuses[2];
            C = statuses[3];
            F = statuses[4];
        }

        int[][] statuses => new int[][] { P, R, S, C, F };

        // Create a copy that has specific file IDs filtered out
        public ActionStatus CopyWithFileFilter(Func<int, bool> fileFilter)
        {
            return new ActionStatus(statuses.Select(status => status.Where(fileFilter).ToArray()).ToArray());
        }
    }

    // Total counts for each status
    internal class ActionStatusCounts : IEquatable<ActionStatusCounts>
    {
        public int P;
        public int R;
        public int S;
        public int C;
        public int F;

        public ActionStatusCounts() { }

        ActionStatusCounts(int[] counts)
        {
            P = counts[0];
            R = counts[1];
            S = counts[2];
            C = counts[3];
            F = counts[4];
        }

        int[] counts => new int[] { P, R, S, C, F };

        public bool Equals(ActionStatusCounts other)
        {
            return
                P == other.P &&
                R == other.R &&
                S == other.S &&
                C == other.C &&
                F == other.F;
        }

        public static ActionStatusCounts operator +(ActionStatusCounts a, ActionStatusCounts b)
        {
            return new ActionStatusCounts(a.counts.Zip(b.counts, (a, b) => a + b).ToArray());
        }

        public override string ToString()
        {
            return UtilityMethods.FormatInvariant($"P={P},R={R},S={S},C={C},F={F}");
        }
    }

    internal static class ActionStatusExtensions
    {
        static ActionStatus[][] FilterFileFromWorkflow(ActionStatus[][] statuses, int workflowToDeleteFrom, bool keepFilesInOtherWorkflows, Func<int, bool> fileFilter)
        {
            return statuses
                .Select((workflow, i) => (i + 1) == workflowToDeleteFrom
                    ? workflow.Select(a => a.CopyWithFileFilter(fileFilter)).ToArray()
                    : keepFilesInOtherWorkflows
                        ? workflow
                        : workflow.Select(a => new ActionStatus()).ToArray())
                .ToArray();
        }

        public static ActionStatus[][] RemoveFileFromWorkflow(this ActionStatus[][] statuses, int workflowToDeleteFrom, int fileToDelete)
        {
            return FilterFileFromWorkflow(statuses, workflowToDeleteFrom, true, fileID => fileID != fileToDelete);
        }

        public static ActionStatus[][] KeepOnlySpecifiedFile(this ActionStatus[][] statuses, int workflowToDeleteFrom, int fileToKeep)
        {
            return FilterFileFromWorkflow(statuses, workflowToDeleteFrom, false, fileID => fileID == fileToKeep);
        }

        public static ActionStatusCounts ComputeCountsFromIDs(this ActionStatus action)
        {
            return new ActionStatusCounts
            {
                P = action.P.Length,
                R = action.R.Length,
                S = action.S.Length,
                C = action.C.Length,
                F = action.F.Length
            };
        }

        public static ActionStatusCounts ComputeCountsFromIDs(this ActionStatus[] workflows)
        {
            return workflows.Select(ComputeCountsFromIDs).Aggregate((acc, x) => acc + x);
        }

        public static ActionStatusCounts[] ComputeCountsFromIDsByAction(this ActionStatus[] actions)
        {
            return actions.Select(ComputeCountsFromIDs).ToArray();
        }

        // Require each array to have the same length (each 'workflow' has every action represented)
        public static ActionStatusCounts[] ComputeCountsFromIDsByAction(this ActionStatus[][] workflows)
        {
            int numWorkflows = workflows.Length;
            int numActions = workflows[0].Length;

            foreach (var workflow in workflows)
            {
                Assert.AreEqual(numActions, workflow.Length);
            }

            // Transpose the matrix to sum by action
            ActionStatus[][] actions = new ActionStatus[numActions][];
            for (int i = 0; i < numActions; i++)
            {
                actions[i] = new ActionStatus[numWorkflows];
                for (int j = 0; j < numWorkflows; j++)
                {
                    actions[i][j] = workflows[j][i];
                }
            }
            return actions.Select(ComputeCountsFromIDs).ToArray();
        }

        public static ActionStatusCounts ComputeCountsFromActionStatistics(this ActionStatistics stats)
        {
            var p = stats.NumDocumentsPending;
            var s = stats.NumDocumentsSkipped;
            var c = stats.NumDocumentsComplete;
            var f = stats.NumDocumentsFailed;
            var r = stats.NumDocuments - p - s - c - f;
            return new ActionStatusCounts { P = p, R = r, S = s, C = c, F = f };
        }

        public static ActionStatusCounts[] ComputeCountsFromActionStatisticsByAction(this IEnumerable<ActionStatistics> actions)
        {
            return actions.Select(ComputeCountsFromActionStatistics).ToArray();
        }
    }

    internal class StatisticsAsserter
    {
        readonly FileProcessingDB fpDB;
        readonly bool invisibleStats;
        readonly string[] actions;

        public StatisticsAsserter(FileProcessingDB fpDB, bool invisibleStats, params string[] actions)
        {
            this.fpDB = fpDB;
            this.invisibleStats = invisibleStats;
            this.actions = actions;
        }

        // Compare actual with expected file statuses
        public void AssertStats(params ActionStatus[] expectedStatuses)
        {
            // Get stats for action
            ActionStatistics getStats(string action)
            {
                if (invisibleStats)
                {
                    return fpDB.GetInvisibleFileStatsAllWorkflows(action, false);
                }
                else
                {
                    return fpDB.GetVisibleFileStatsAllWorkflows(action, false);
                }
            }

            ActionStatusCounts[] expectedCountsByAction = expectedStatuses.ComputeCountsFromIDsByAction();

            var actualStatuses = actions.Select(getStats).ToArray();
            var actualCountsByAction = actualStatuses.ComputeCountsFromActionStatisticsByAction();
            CollectionAssert.AreEqual(expectedCountsByAction, actualCountsByAction);
        }
    }

    #endregion Statistics Helper Classes
}
