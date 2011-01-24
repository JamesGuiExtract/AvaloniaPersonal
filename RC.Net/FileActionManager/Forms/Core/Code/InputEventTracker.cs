using Extract.Licensing;
using Extract.Utilities.Forms;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Forms
{
    /// <summary>
    /// A class for tracking input events (KeyPress, MouseClick, MouseScroll) on a particular
    /// control or set of controls.
    /// </summary>
    public class InputEventTracker : MessageFilterBase
    {
        #region Internal Class

        /// <summary>
        /// Internal class used to manage the active second count for the update database call.
        /// </summary>
        class ActiveMinuteData
        {
            /// <summary>
            /// The count of active seconds in the past active minute.
            /// </summary>
            int _activeSecondCount;

            /// <summary>
            /// The time stamp for the active minute.
            /// </summary>
            readonly DateTime _timeStamp;

            /// <overloads>
            /// Initializes a new instance of the <see cref="ActiveMinuteData"/> class.
            /// </overloads>
            /// <summary>
            /// Initializes a new instance of the <see cref="ActiveMinuteData"/> class.
            /// </summary>
            /// <param name="activeSecondCount">The count of active seconds in the current minute.
            /// </param>
            public ActiveMinuteData(int activeSecondCount)
            {
                // Store the active second count
                _activeSecondCount = activeSecondCount;

                // Store the current timestamp
                _timeStamp = DateTime.Now;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ActiveMinuteData"/> class
            /// with the values from a second <see cref="ActiveMinuteData"/> class.
            /// </summary>
            /// <param name="activeData">The class to copy the data from.</param>
            public ActiveMinuteData(ActiveMinuteData activeData)
            {
                _activeSecondCount = activeData.ActiveSecondCount;
                _timeStamp = activeData.TimeStamp;
            }

            /// <summary>
            /// Gets the active second count.
            /// </summary>
            public int ActiveSecondCount
            {
                get
                {
                    return _activeSecondCount;
                }
                set
                {
                    _activeSecondCount = value;
                }
            }

            /// <summary>
            /// Gets the time stamp for this active minute.
            /// </summary>
            public DateTime TimeStamp
            {
                get
                {
                    return _timeStamp;
                }
            }
        }

        #endregion Internal Class

        #region Constants

        /// <summary>
        /// The name of the <see cref="InputEventTracker"/> class. Used for licensing.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(InputEventTracker).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The count of input events
        /// </summary>
        volatile int _inputCount;

        /// <summary>
        /// The active minute data for the current minute
        /// </summary>
        volatile ActiveMinuteData _currentMinuteData;

        /// <summary>
        /// Cached active minute data that is set by the active minute thread and
        /// used by the update database thread
        /// </summary>
        volatile ActiveMinuteData _cachedMinuteData;

        /// <summary>
        /// Event to indicate threads should end.
        /// </summary>
        ManualResetEvent _endThreads = new ManualResetEvent(false);

        /// <summary>
        /// Event to indicate time to update the database.
        /// </summary>
        AutoResetEvent _updateDatabase = new AutoResetEvent(false);

        /// <summary>
        /// Thread which handles monitoring active seconds.
        /// </summary>
        Thread _activeSecondThread;

        /// <summary>
        /// Thread which handles monitoring active minutes.
        /// </summary>
        Thread _activeMinuteThread;

        /// <summary>
        /// Thread which handles updating the database with active minute data.
        /// </summary>
        Thread _updateDatabaseThread;

        /// <summary>
        /// The file processing DB manager that will be used to update the database
        /// </summary>
        FileProcessingDB _database;

        /// <summary>
        /// The action ID for this event tracker.
        /// </summary>
        readonly int _actionId;

        /// <summary>
        /// The current processes ID
        /// </summary>
        readonly int _processId;

        /// <summary>
        /// Whether the input events should be tracked in the database or not.
        /// </summary>
        readonly bool _trackEvents;

        /// <summary>
        /// Mutex for getting/setting the current minute data
        /// </summary>
        readonly static object _lock = new object();

        #endregion Fields

        #region Constructors

        /// <overloads>
        /// Initializes a new instance of the <see cref="InputEventTracker"/> class.
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="InputEventTracker"/> class.
        /// </summary>
        [CLSCompliant(false)]
        public InputEventTracker(FileProcessingDB database, int actionId)
            : this(database, actionId, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InputEventTracker"/> class.
        /// </summary>
        [CLSCompliant(false)]
        public InputEventTracker(FileProcessingDB database, int actionId, params Control[] controls)
            : base(controls)
        {
            try
            {
                // Ensure the FAM DB object is not null
                ExtractException.Assert("ELI28946", "File processing DB cannot be null.",
                    database != null);

                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects,
                    "ELI28959", _OBJECT_NAME);

                // Set the fam DB object
                _database = database;

                // The action ID to record events for
                _actionId = actionId;

                using (Process process = Process.GetCurrentProcess())
                {
                    _processId = process.Id;
                }

                // Check whether event tracking is enabled
                _trackEvents = _database.GetDBInfoSetting("EnableInputEventTracking", true)
                    .Equals("1", StringComparison.OrdinalIgnoreCase);

                if (_trackEvents)
                {
                    // Create the threads and set the threading model to multi-threaded apartment
                    _activeSecondThread = new Thread(ActiveSecondTimer);
                    _activeSecondThread.SetApartmentState(ApartmentState.MTA);
                    _activeMinuteThread = new Thread(ActiveMinuteTimer);
                    _activeMinuteThread.SetApartmentState(ApartmentState.MTA);
                    _updateDatabaseThread = new Thread(UpdateDatabase);
                    _updateDatabaseThread.SetApartmentState(ApartmentState.MTA);

                    // Start the threads
                    _activeSecondThread.Start();
                    _activeMinuteThread.Start();
                    _updateDatabaseThread.Start();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28947", ex);
            }
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Adds the event handlers for tracking input events to the specified <see cref="Control"/>
        /// </summary>
        /// <param name="control">The <see cref="Control"/> to watch input events for.</param>
        public void RegisterControl(Control control)
        {
            try
            {
                if (Controls.Contains(control))
                {
                    throw new ExtractException("ELI28948", "The specified control is already being tracked.");
                }

                Controls.Add(control);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28949", ex);
                ee.AddDebugData("Control", control, false);
            }
        }

        /// <summary>
        /// Removes the event handlers for tracking input events to the specified <see cref="Control"/>
        /// </summary>
        /// <param name="control">The <see cref="Control"/> to no longer monitor for input events.</param>
        public void UnregisterControl(Control control)
        {
            try
            {
                int index = Controls.IndexOf(control);
                if (index == -1)
                {
                    throw new ExtractException("ELI28950", "The specified control is not being tracked.");
                }

                Controls.RemoveAt(index);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28951", ex);
                ee.AddDebugData("Control", control, false);
            }
        }

        /// <summary>
        /// External method for notifying the input event counter that an input event
        /// has occurred.
        /// <para><b>Note:</b></para>
        /// This method should only be used in the case where a class has another
        /// <see cref="IMessageFilter.PreFilterMessage"/> running that handles the message
        /// and returns <see langword="true"/>, which would cause this class
        /// to never see the input event message.  In this case the external class should
        /// call this method explicitly to record an input event.
        /// </summary>
        public void NotifyOfInputEvent()
        {
            _inputCount++;
        }

        /// <summary>
        /// Inserts a new record into the InputEvent table with the specified active minute data.
        /// </summary>
        /// <param name="data">The data to use when creating the IputEvent entry.</param>
        void RecordActiveMinuteData(ActiveMinuteData data)
        {
            // Update the InputEvent table
            _database.RecordInputEvent(data.TimeStamp.ToString("g", DateTimeFormatInfo.InvariantInfo),
                _actionId, data.ActiveSecondCount, _processId);
        }

        #endregion Methods

        #region Thread Methods

        /// <summary>
        /// Thread function that wakes every second to check for input in the past second
        /// and then updates the active second counter.
        /// </summary>
        void ActiveSecondTimer()
        {
            try
            {
                int sleepTime = 1000 - DateTime.Now.Millisecond;
                while (!_endThreads.WaitOne(sleepTime))
                {
                    // Get the current tickcount
                    int tickCount = Environment.TickCount;
                    if (_inputCount > 0)
                    {
                        lock (_lock)
                        {
                            if (_currentMinuteData == null)
                            {
                                _currentMinuteData = new ActiveMinuteData(1);
                            }
                            else
                            {
                                _currentMinuteData.ActiveSecondCount++;
                            }
                        }

                        _inputCount = 0;
                    }

                    int elapsedTicks = Environment.TickCount - tickCount;

                    // Handle tick count rollover issue (rolls over at ~ 50 days uptime)
                    if (elapsedTicks < 0)
                    {
                        sleepTime = 1000;
                    }
                    else if (elapsedTicks > 1000)
                    {
                        // We have spent over a second in the loop, set the sleep time to 0
                        sleepTime = 0;
                    }
                    else
                    {
                        // Sleep time is 1000 - milliseconds elapsed in the loop
                        sleepTime = 1000 - elapsedTicks;
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // Do nothing if the thread is aborted
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI28953", ex);
            }
        }

        /// <summary>
        /// Thread function that wakes every minute to see if there have been active seconds
        /// in the past minute and sets the updateDatabase event if there has been activity.
        /// </summary>
        void ActiveMinuteTimer()
        {
            try
            {
                // Wait until the next minute boundary or the end threads event is signaled
                while (!_endThreads.WaitOne(1000 * (60 - DateTime.Now.Second)))
                {
                    if (_currentMinuteData != null)
                    {
                        // Mutex around current minute data
                        lock (_lock)
                        {
                            _cachedMinuteData = _currentMinuteData;
                            _currentMinuteData = null;
                        }

                        _updateDatabase.Set();
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // Do nothing if the thread is aborted
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI28955", ex);
            }
        }

        /// <summary>
        /// Thread function which will update the database with the active minute data.
        /// </summary>
        void UpdateDatabase()
        {
            try
            {
                WaitHandle[] waitHandles = new WaitHandle[] { _updateDatabase, _endThreads };
                while (WaitHandle.WaitAny(waitHandles) != 1)
                {
                    try
                    {
                        ActiveMinuteData data = new ActiveMinuteData(_cachedMinuteData);

                        // Record the active minute data
                        RecordActiveMinuteData(data);
                    }
                    catch (Exception ex)
                    {
                        // Just log the exception that occurred and allow the thread to continue
                        ExtractException.Log("ELI29128", ex);
                    }
                }

                // Check for any left over active second data
                lock (_lock)
                {
                    if (_currentMinuteData != null)
                    {
                        // Get the active minute data
                        ActiveMinuteData data = _currentMinuteData;
                        _currentMinuteData = null;

                        // Record the active minute data
                        RecordActiveMinuteData(data);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                // Do nothing if the thread is aborted
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI28956", ex);
            }
        }

        #endregion Thread Methods

        #region MessageFilterBase Overrides

        /// <summary>
        /// Called from <see cref="IMessageFilter.PreFilterMessage"/>.  This method will examine
        /// the <paramref name="message"/> and increment the inputCount if the message
        /// is an input message 
        /// </summary>
        /// <param name="message">The message to be dispatched. You cannot modify this message.</param>
        /// <returns>
        /// <see langword="false"/> to allow the message to continue to the next filter or control.
        /// </returns>
        protected override bool HandleMessage(Message message)
        {
            try
            {
                // Only check the message if event tracking is on
                if (_trackEvents)
                {
                    switch (message.Msg)
                    {
                        case WindowsMessage.KeyDown:
                        case WindowsMessage.SystemKeyDown:
                        case WindowsMessage.LeftButtonDown:
                        case WindowsMessage.RightButtonDown:
                        case WindowsMessage.MiddleButtonDown:
                        case WindowsMessage.MouseWheel:
                        case WindowsMessage.NonClientLeftButtonDown:
                        case WindowsMessage.NonClientRightButtonDown:
                        case WindowsMessage.NonClientMiddleButtonDown:
                            _inputCount++;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI28952", ex);
            }

            return false;
        }

        /// <overloads>Releases resources used by the <see cref="MessageFilterBase"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="MessageFilterBase"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>		
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                // Dispose of managed objects

                // End the threads and close the end thread event handle
                if (_endThreads != null)
                {
                    // Set the end threads event and wait for the threads to exit
                    _endThreads.Set();
                    if (_activeSecondThread != null)
                    {
                        _activeSecondThread.Join();
                        _activeSecondThread = null;
                    }
                    if (_activeMinuteThread != null)
                    {
                        _activeMinuteThread.Join();
                        _activeMinuteThread = null;
                    }
                    if (_updateDatabaseThread != null)
                    {
                        _updateDatabaseThread.Join();
                        _updateDatabaseThread = null;
                    }

                    _endThreads.Close();
                    _endThreads = null;
                }

                // Close the update database event handle
                if (_updateDatabase != null)
                {
                    _updateDatabase.Close();
                    _updateDatabase = null;
                }

                // Set the FAMDB COM object to NULL
                if (_database != null)
                {
                    _database = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion MessageFilterBase Overrides
    }
}
