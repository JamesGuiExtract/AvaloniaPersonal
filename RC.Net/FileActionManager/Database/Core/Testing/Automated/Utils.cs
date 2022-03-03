using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Database.Test
{
    #region FileProcessingDB Wrappers

    /// Wrapper for a FileProcessingDB with a single workflow that takes care of initialization and cleanup
    [CLSCompliant(false)]
    public sealed class NoWorkflows<T> : DisposableDatabaseBase<T>
    {
        readonly FileProcessingDB _fileProcessingDB;

        public override FileProcessingDB FileProcessingDB => _fileProcessingDB;
        public override FileProcessingDB[] Workflows => new[] { _fileProcessingDB };
        public override FileProcessingDB[] Sessions => new[] { _fileProcessingDB };
        public override string[] Actions { get; } = new[] { "Action1", "Action2" };

        public NoWorkflows(FAMTestDBManager<T> dbManager, string dbName, FileProcessingDB fileProcessingDB)
        {
            TestDBManager = dbManager;
            DbName = dbName;
            _fileProcessingDB = fileProcessingDB;

            foreach (string action in Actions)
                _fileProcessingDB.DefineNewAction(action);

            // Start a session
            _fileProcessingDB.RecordFAMSessionStart("Test.fps", Actions[0], true, true);
            _fileProcessingDB.RegisterActiveFAM();
        }
    }

    /// <summary>
    /// Wrapper for a FileProcessingDB with a single workflow that takes care of initialization and cleanup
    /// </summary>
    [CLSCompliant(false)]
    public sealed class OneWorkflow<T> : DisposableDatabaseBase<T>
    {
        readonly bool _enableLoadBalancing;

        private readonly string action1 = "Action1";
        private readonly string action2 = "Action2";

        private readonly FileProcessingDB _fpDB = null;

        public override FileProcessingDB FileProcessingDB => _fpDB;
        public override FileProcessingDB[] Workflows => new[] { FileProcessingDB };
        public override FileProcessingDB[] Sessions => new[] { FileProcessingDB };
        public string Action1 => action1;
        public string Action2 => action2;
        public override string[] Actions => new[] { Action1, Action2 };

        public OneWorkflow(FAMTestDBManager<T> testDBManager, string testDBName, bool enableLoadBalancing)
        {
            TestDBManager = testDBManager;
            DbName = testDBName;
            _enableLoadBalancing = enableLoadBalancing;

            _fpDB = testDBManager.GetNewDatabase(testDBName);
            _fpDB.SetDBInfoSetting("EnableLoadBalancing", _enableLoadBalancing ? "1" : "0", true, false);

            foreach (string action in Actions)
                _fpDB.DefineNewAction(action);

            int workflowID = _fpDB.AddWorkflow("Workflow1", EWorkflowType.kUndefined, action1, action2);
            Assert.AreEqual(1, workflowID);

            _fpDB.ActiveWorkflow = "Workflow1";

            // Start a session
            _fpDB.RecordFAMSessionStart("Test.fps", action1, true, true);
            _fpDB.RegisterActiveFAM();
        }
    }

    /// <summary>
    /// Wrapper for a FileProcessingDB with two workflows that takes care of initialization and cleanup
    /// </summary>
    internal sealed class TwoWorkflows<T> : DisposableDatabaseBase<T>
    {
        readonly bool _enableLoadBalancing;

        public readonly string action1 = "Action1";
        public readonly string action2 = "Action2";

        public readonly FileProcessingDB wf1 = null;
        public FileProcessingDB wf2 = null;
        public FileProcessingDB wfAll = null;
        readonly FileProcessingDB[] fpDBs = null;

        public override FileProcessingDB FileProcessingDB => wf1;
        public override FileProcessingDB[] Workflows => new[] { wf1, wf2 };
        public override FileProcessingDB[] Sessions => new[] { wf1, wf2, wfAll };

        public override string[] Actions => new[] { action1, action2 };

        public TwoWorkflows(FAMTestDBManager<T> testDBManager, string testDBName, bool enableLoadBalancing)
        {
            TestDBManager = testDBManager;
            DbName = testDBName;
            _enableLoadBalancing = enableLoadBalancing;

            wf1 = testDBManager.GetNewDatabase(testDBName);
            wf1.SetDBInfoSetting("EnableLoadBalancing", _enableLoadBalancing ? "1" : "0", true, false);

            foreach (string action in Actions)
                wf1.DefineNewAction(action);

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

        public void startNewSession(FileProcessingDB fpDB)
        {
            fpDB.UnregisterActiveFAM();
            fpDB.RecordFAMSessionStop();
            fpDB.RecordFAMSessionStart("Test.fps", action1, true, true);
            fpDB.RegisterActiveFAM();
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
