using Extract.Utilities;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// Class to enable multi-threaded processing of single documents
    /// </summary>
    /// <seealso cref="UCLID_AFCORELib.IParallelRuleSet" />
    /// <seealso cref="System.IDisposable" />
    [ComVisible(true)]
    [Guid("ADBCFCB7-2DC2-4617-AF0D-0E0EF5AA28D4")]
    [CLSCompliant(false)]
    public class ParallelRuleSet : IParallelRuleSet, IDisposable
    {
        #region Static Fields

        /// <summary>
        /// Used to serialize/deserialize COM objects for sharing between threads
        /// </summary>
        static ThreadLocal<IMiscUtils> _miscUtils = new ThreadLocal<IMiscUtils>(() => new MiscUtilsClass());

        static readonly int _PROGRESS_ITEMS_PER_ATTRIBUTE = 2;

        #endregion

        #region IParallelRuleset


        /// <summary>
        /// Runs the specified attributes of the specified <see cref="RuleSet"/> using multiple
        /// threads if they are available (controlled by the named semaphore)
        /// </summary>
        /// <param name="pDocuments">The <see cref="AFDocument"/>s to be processed.</param>
        /// <param name="pRuleSet">The <see cref="RuleSet"/> to process the documents with</param>
        /// <param name="pAttributeNames">The names of the attribute to find (null for all defined attributes).</param>
        /// <param name="bstrParallelSemaphoreName">Name of the semaphore used to regulate thread use (enables the
        /// FAM thread limit specification to be respected).</param>
        /// <param name="pProgressStatus">The <see cref="ProgressStatus"/> object used to supply progress information back
        /// to the FAM, e.g.</param>
        /// <returns>An <see cref="IUnknownVector"/> of <see cref="IUnknownVector"/>s of <see cref="IAttribute"/>s,
        /// one collection per input document.</returns>
        public IUnknownVector RunAttributeFinders(IUnknownVector pDocuments, RuleSet pRuleSet,
            VariantVector pAttributeNames, string bstrParallelSemaphoreName, ProgressStatus pProgressStatus)
        {
            ThreadPoolThrottle throttle = null;
            ThreadLocal<RuleSet> ruleset = null;
            try
            {
                string[] attributeNames = pAttributeNames
                    ?.ToIEnumerable<string>()
                    .ToArray()
                    ??
                    pRuleSet.DefinedAttributeNames
                    .ToIEnumerable<string>()
                    .ToArray();

                // Serialize the ruleset if possible, else load from disk
                // for each thread that needs it. Serializing is not possible when
                // the file was loaded from an encrypted file. Loading from file
                // is not adequate if the file is saveable because it could be dirty (run from the editor)
                string rsdFileName = pRuleSet.FileName;
                string stringizedRuleset = null;
                if (pRuleSet.CanSave)
                {
                    try
                    {
                        stringizedRuleset = _miscUtils.Value.GetObjectAsStringizedByteStream(pRuleSet);
                    }
                    catch { }
                    // If a ruleset cannot be saved it will have to be loaded from disk.
                    // Reason it cannot be saved could be that the ruleset contains a,
                    // disabled, deprecated object (legacy AddressFinder, e.g.)
                }
                ruleset = new ThreadLocal<RuleSet>(() =>
                {
                    RuleSet rsd = null;
                    if (stringizedRuleset != null)
                    {
                        rsd = (RuleSet)_miscUtils.Value
                            .GetObjectFromStringizedByteStream(stringizedRuleset);
                        rsd.FileName = rsdFileName;
                    }
                    else
                    {
                        rsd = new RuleSet();
                        rsd.LoadFrom(rsdFileName, false);
                    }
                    return rsd;
                });

                var documents = pDocuments.ToIEnumerable<AFDocument>().ToList();
                int docCount = documents.Count;
                ExtractException.Assert("ELI42024", "Document collection is empty", docCount > 0);

                var parallelMode = documents[0].ParallelRunMode;
                bool greedy = parallelMode == EParallelRunMode.kGreedyParallelization;

                // Defer stringizing the document for if/when it is needed
                // but memoize so it doesn't happen multiple times in a loop
                var stringizedDoc = UtilityExtensionMethods.Memoize<AFDocument, string>
                    (d =>_miscUtils.Value.GetObjectAsStringizedByteStream(d));


                // Create thread manager to throttle thread usage. If no semaphore name was passed in then
                // either no parallelization is allowed (shouldn't really be here in that case but it could happen)
                // or the rules are not being run via the FAM (e.g., this execution is being driven by the RuleTester or TestHarness).
                if (parallelMode != EParallelRunMode.kNoParallelization
                    && !string.IsNullOrEmpty(bstrParallelSemaphoreName))
                {
                    Semaphore globalSemaphore = null;
                    if (Semaphore.TryOpenExisting(bstrParallelSemaphoreName, out globalSemaphore))
                    {
                        throttle = new ThreadPoolThrottle(globalSemaphore, greedy);
                    }
                }

                // This collection will be filled as rule running completes with tasks to be run to deserialize rules output
                var deserializeTasks = new BlockingCollection<Task<IUnknownVector>>();

                // Track the number of tasks that are scheduled to be added to the blocking collection so that we know when
                // to stop waiting
                int totalDeserializeTasks = 0;

                // Process the items
                var resultTasks = new Task<IUnknownVector>[docCount][];
                for (int i = 0; i < docCount; ++i)
                {
                    int docNum = i; // Create a copy of the index to close over
                    var doc = documents[docNum];
                    resultTasks[docNum] = new Task<IUnknownVector>[attributeNames.Length];

                    for(int j = 0; j < attributeNames.Length; ++j)
                    {
                        int attrNum = j; // Create a copy of the index to close over
                        var attributeName = attributeNames[attrNum];

                        // The thread manager needs to know which semaphore was used when it is releasing a semaphore
                        bool usedGlobalSemaphore = false;
                        bool runSynchronously = false;
                        bool lastItem = docNum == docCount + 1 && attrNum == attributeNames.Length + 1;

                        // Determine whether to run sync or async
                        // If this is the last item to process just run it directly
                        if (lastItem)
                        {
                            if (throttle != null)
                            {
                                usedGlobalSemaphore = throttle.Acquire(preferGlobal: false);
                            }
                            runSynchronously = true;
                        }
                        // If not being greedy acquire threads before starting a task
                        else if (throttle != null && !greedy)
                        {
                            usedGlobalSemaphore = throttle.Acquire(preferGlobal: true);

                            // Can't get another thread so just do work on this thread and then try again
                            if (!usedGlobalSemaphore)
                            {
                                runSynchronously = true;
                            }
                        }

                        // Run the task
                        if (runSynchronously)
                        {
                            try
                            {
                                if (pProgressStatus != null)
                                {
                                    string statusText = UtilityMethods.FormatInvariant($"Executing rules for field {attributeName} on logical document {docNum + 1}");
                                    pProgressStatus.StartNextItemGroup(statusText,
                                        _PROGRESS_ITEMS_PER_ATTRIBUTE);
                                }

                                var task = resultTasks[docNum][attrNum] =
                                    new Task<IUnknownVector>(() => pRuleSet.RunAttributeFinder(doc, attributeName, null));
                                task.RunSynchronously();
                                if (task.IsFaulted)
                                {
                                    var ag = task.Exception as AggregateException;
                                    var ue = (ag?.InnerException ?? task.Exception).AsExtract("ELI42033");
                                    ue.AddDebugData("Logical document number", docNum, true);
                                    ue.AddDebugData("Attribute name", attributeName, true);
                                    throw ue;
                                }
                            }
                            finally
                            {
                                throttle?.Release(usedGlobalSemaphore);
                            }
                        }
                        else
                        {
                            // Start the first task's progress immediately so that it doesn't appear that the file is stuck initializing
                            if (pProgressStatus != null && (docNum + attrNum == 0))
                            {
                                string statusText = UtilityMethods.FormatInvariant($"Executing rules for field {attributeName} on logical document {docNum + 1}");
                                pProgressStatus.StartNextItemGroup(statusText,
                                    _PROGRESS_ITEMS_PER_ATTRIBUTE);
                            }

                            StartTask(stringizedDoc(doc), ruleset, attributeName, throttle, needToAcquireSemaphore: greedy)
                                .ContinueWith(task =>
                                {
                                    var task2 = new Task<IUnknownVector>(() =>
                                    {
                                        // Start non-first task's progress in the task created by the continuation so that it doesn't appear that all the work is done.
                                        // (Can't run this in the main task because it could be executed on a different thread)
                                        if (pProgressStatus != null && (docNum + attrNum > 0))
                                        {
                                            string statusText = UtilityMethods.FormatInvariant($"Executing rules for field {attributeName} on logical document {docNum + 1}");
                                            pProgressStatus.StartNextItemGroup(statusText,
                                                _PROGRESS_ITEMS_PER_ATTRIBUTE);
                                        }

                                        if (task.IsFaulted)
                                        {
                                            var ag = task.Exception as AggregateException;
                                            var ue = (ag?.InnerException ?? task.Exception).AsExtract("ELI42056");
                                            ue.AddDebugData("Logical document number", docNum, true);
                                            ue.AddDebugData("Attribute name", attributeName, true);
                                            throw ue;
                                        }

                                        return (IUnknownVector)
                                            _miscUtils.Value.GetObjectFromStringizedByteStream(task.Result);
                                    });

                                    resultTasks[docNum][attrNum] = task2;
                                    deserializeTasks.Add(task2);
                                });
                            // Increment the number of tasks to expect
                            ++totalDeserializeTasks;
                        }
                    }
                }

                // Run all the deserializing tasks on this thread so that the COM objects are usable
                while (totalDeserializeTasks-- > 0)
                {
                    // This will block until a task is in the collection (is ready to be run)
                    var task = deserializeTasks.Take();
                    task.RunSynchronously();
                    if (task.IsFaulted)
                    {
                        var ag = task.Exception as AggregateException;
                        var ue = (ag?.InnerException ?? task.Exception).AsExtract("ELI42041");
                        throw ue;
                    }
                }

                // This will release all acquired global semaphore counts
                // (These should already be released, but just in case...)
                throttle?.ReleaseAllShared();

                // Turn tasks into VOAs
                var documentResults = new IUnknownVector();
                foreach (var documentTasks in resultTasks)
                {
                    IUnknownVector documentResult = documentTasks.First().Result;
                    foreach (var task in documentTasks.Skip(1))
                    {
                        documentResult.Append(task.Result);
                    }
                    documentResults.PushBack(documentResult);
                }

                return documentResults;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI41960", "Error in RunAttributeFinders");
            }
            finally
            {
                // This will release all acquired global semaphore counts
                throttle?.Dispose();

                ruleset?.Dispose();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Starts a rule running task on the thread-pool.
        /// </summary>
        /// <param name="stringizedDoc">The stringized <see cref="AFDocument"/> to process.</param>
        /// <param name="ruleset">The <see cref="ThreadLocal{RuleSet}"/> containing the attribute to run.</param>
        /// <param name="attributeName">Name of the attribute to run.</param>
        /// <param name="throttle">The <see cref="ThreadPoolThrottle"/> to use to acquire a semaphore.</param>
        /// <param name="needToAcquireSemaphore">if set to <c>true</c> the task needs to use <see paramref="throttle"/>
        /// to acquire a semaphore before doing work.</param>
        /// <returns>A <see cref="Task{IUnknownVector}"/> representing the async result</returns>
        private static Task<string> StartTask(string stringizedDoc, ThreadLocal<RuleSet> ruleset,
            string attributeName, ThreadPoolThrottle throttle, bool needToAcquireSemaphore)
        {
            return Task.Run(() =>
            {
                bool usedGlobalSemaphore = true;
                try
                {
                    if (throttle != null && needToAcquireSemaphore)
                    {
                        usedGlobalSemaphore = throttle.Acquire(preferGlobal: false);
                    }
                    var afDoc = (AFDocument)_miscUtils.Value
                        .GetObjectFromStringizedByteStream(stringizedDoc);
                    var result = ruleset.Value.RunAttributeFinder(afDoc, attributeName,
                        bstrAlternateComponentDataDir: null); // If alt FKB dir is already set in the afDoc then passing null here won't affect that setting
                    return _miscUtils.Value.GetObjectAsStringizedByteStream(result);
                }
                finally
                {
                    throttle?.Release(usedGlobalSemaphore);
                }
            });
        }

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _miscUtils.Dispose();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region ThreadPoolThrottle

        /// <summary>
        /// Limits use of the thread pool so that CPU resources are shared with other FAM processing threads
        /// </summary>
        /// <seealso cref="System.IDisposable" />
        class ThreadPoolThrottle : IDisposable
        {
            bool _greedy;
            int _sharedThreads = 0;
            int _waiting = 0;
            Semaphore _globalSemaphore = null;
            Semaphore _localSemaphore = null;
            Semaphore[] _semaphores = null;
            object _protectLogic = new object();

            /// <summary>
            /// Initializes a new instance of the <see cref="ThreadPoolThrottle"/> class.
            /// </summary>
            /// <param name="globalSemaphore">The global semaphore (e.g., created by the FAM).</param>
            /// <param name="greedy">if set to <c>true</c> then hold onto global semaphore counts until
            /// no tasks are waiting to be started.</param>
            public ThreadPoolThrottle(Semaphore globalSemaphore, bool greedy)
            {
                _globalSemaphore = globalSemaphore;
                _localSemaphore = new Semaphore(1, int.MaxValue);
                _semaphores = new Semaphore[] { _localSemaphore, globalSemaphore };
                _greedy = greedy;
            }

            /// <summary>
            /// Acquires a semaphore count, blocks until one is available
            /// </summary>
            /// <param name="preferGlobal">if set to <c>true</c> then preference will be given to
            /// acquiring a count on the shared semaphore.</param>
            /// <returns><c>true</c> if the global semaphore was decremented</returns>
            public bool Acquire(bool preferGlobal)
            {
                // Track whether any attempts to do work are waiting on a semaphore
                lock (_protectLogic)
                {
                    ++_waiting;
                }
                try
                {
                    // If preferGlobal check for an available count
                    bool usedGlobalSemaphore = preferGlobal && _globalSemaphore.WaitOne(millisecondsTimeout: 0);
                    if (usedGlobalSemaphore)
                    {
                        lock (_protectLogic)
                        {
                            ++_sharedThreads;
                        }
                    }
                    // Else if prefering the local semaphore or if attempt to acquire was not successful
                    // check for an available local count
                    else if (!_localSemaphore.WaitOne(millisecondsTimeout: 0))
                    {
                        // No semaphore counts were available so wait for either one
                        usedGlobalSemaphore = WaitHandle.WaitAny(_semaphores) == 1; // 1 is the index of the global semaphore

                        if (usedGlobalSemaphore)
                        {
                            lock (_protectLogic)
                            {
                                // Perhaps both semaphores became available at about the same time.
                                // If so, but the local semaphore is prefered then swap
                                if (!preferGlobal && _localSemaphore.WaitOne(millisecondsTimeout: 0))
                                {
                                    _globalSemaphore.Release();
                                    usedGlobalSemaphore = false;
                                }
                                // Otherwise just keep the global semaphore and increment the counter
                                else
                                {
                                    ++_sharedThreads;
                                }
                            }
                        }
                    }
                    return usedGlobalSemaphore;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI42066");
                }
                finally
                {
                    // Done waiting
                    lock (_protectLogic)
                    {
                        --_waiting;
                    }
                }
            }

            /// <summary>
            /// Increments one of the semaphores, according to various conditions
            /// </summary>
            /// <param name="usedGlobalSemaphore">if set to <c>true</c> then the associated work was run after decrementing the global semaphore.</param>
            public void Release(bool usedGlobalSemaphore)
            {
                try
                {
                    lock (_protectLogic)
                    {
                        // If mode is greedy and we are holding onto a shared thread, but there are no waiting tasks,
                        // then increment the shared semaphore because there will be no more use for it
                        if (_greedy && _waiting == 0 && _sharedThreads > 0)
                        {
                            _globalSemaphore.Release();
                            --_sharedThreads;
                        }
                        // If we are being greedy then increment the local semaphore, thus reserving the global count for subsequent use,
                        // regardless of whether the global semaphore or the local semaphore was decremented previously.
                        else if (!usedGlobalSemaphore || _greedy)
                        {
                            _localSemaphore.Release();
                        }
                        // Otherwise polite parallelization is being used and work was done after decrementing
                        // the shared, global semaphore.
                        else
                        {
                            _globalSemaphore.Release();
                            --_sharedThreads;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI42065");
                }
            }

            /// <summary>
            /// Give back all semaphore counts that this instance has acquired
            /// </summary>
            public void ReleaseAllShared()
            {
                try
                {
                    lock (_protectLogic)
                    {
                        while (_sharedThreads > 0)
                        {
                            _globalSemaphore.Release();
                            --_sharedThreads;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI42061");
                }
            }

            #region IDisposable Support
            private bool disposedValue = false; // To detect redundant calls

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        try
                        {
                            ReleaseAllShared();
                        }
                        catch { }

                        if (_globalSemaphore != null)
                        {
                            _globalSemaphore.Dispose();
                        }
                        if (_localSemaphore != null)
                        {
                            _localSemaphore.Dispose();
                        }
                    }

                    disposedValue = true;
                }
            }

            // This code added to correctly implement the disposable pattern.
            public void Dispose()
            {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            #endregion
        }

        #endregion
    }
}
