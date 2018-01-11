﻿using Extract.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Timers;

namespace Extract.Utilities
{
    /// <summary>
    /// Describes a scheduled event that may recur.
    /// </summary>
    [DataContract]  
    public class ScheduledEvent : IDisposable
    {
        #region Constants

        const int _CURRENT_VERSION = 1;

        #endregion Constants

        #region Fields

        /// <summary>
        /// Synchronizes access for multi-threaded processing.
        /// </summary>
        static object _lock = new object();

        /// <summary>
        /// The <see cref="Timer"/> used to trigger the next <see cref="EventStarted"/>
        /// </summary>
        Timer _timer;

        /// <summary>
        /// Indicates whether this timer is enabled (whether it will fire <see cref="EventStarted"/>).
        /// </summary>
        bool _enabled = true;

        /// <summary>
        /// Backing event for <see cref="<see cref="EventStarted"/>
        /// </summary>
        private event EventHandler<EventArgs> _eventStarted;

        #endregion Fields

        #region Constructors

        public ScheduledEvent()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45410");
            }
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Raised when any instance of the event starts.
        /// </summary>
        public event EventHandler<EventArgs> EventStarted
        {
            add
            {
                _eventStarted += value;
                SetTimer();
            }

            remove
            {
                _eventStarted -= value;
            }
        }

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets the version for this instance.
        /// </summary>
        [DataMember]
        public int Version { get; } = 1;

        /// <summary>
        /// Backing field for <see cref="Start"/>.
        /// </summary>
        DateTime _start = DateTime.Now;

        /// <summary>
        /// Gets or sets the scheduled start time of this event.
        /// </summary>
        [DataMember]
        public DateTime Start
        {
            get
            {
                return _start;
            }

            set
            {
                try
                {
                    if (value != _start)
                    {
                        _start = value;
                        SetTimer();
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI45401");
                }
            }
        }

        /// <summary>
        /// Backing field for <see cref="End"/>.
        /// </summary>
        DateTime? _end;

        /// <summary>
        /// Gets or sets a time after which no recurrences should be fired or <c>null</c> if
        /// recurrences should be indefinite.
        /// </summary>
        [DataMember]
        public DateTime? End
        {
            get
            {
                return _end;
            }

            set
            {
                try
                {
                    if (value != _end)
                    {
                        _end = value;
                        SetTimer();
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI45402");
                }
            }
        }

        /// <summary>
        /// Backing field for <see cref="RecurrenceUnit"/>.
        /// </summary>
        DateTimeUnit? _recurrenceUnit = null;

        /// <summary>
        /// Gets or sets the <see cref="DateTimeUnit"/> over which the event should occur or
        /// <c>null</c> if this event does not recur.
        /// </summary>
        [DataMember]
        public DateTimeUnit? RecurrenceUnit
        {
            get
            {
                return _recurrenceUnit;
            }

            set
            {
                try
                {
                    if (value != _recurrenceUnit)
                    {
                        _recurrenceUnit = value;
                        SetTimer();
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI45403");
                }
            }

        }

        /// <summary>
        /// Gets or sets the duration for each instance of the event.
        /// </summary>
        [DataMember]
        public TimeSpan? Duration { get; set; }

        /// <summary>
        /// Backing field for <see cref="Exclusions"/>.
        /// </summary>
        [DataMember]
        [JsonProperty(PropertyName = "Exclusions")]
        List<ScheduledEvent> _exclusions = new List<ScheduledEvent>();

        /// <summary>
        /// Gets or sets time periods over which the event should be excluded from executing.
        /// </summary>
        public IReadOnlyCollection<ScheduledEvent> Exclusions
        {
            get
            {
                return _exclusions.AsReadOnly();
            }

            set
            {
                try
                {
                    if (!value.SequenceEqual(_exclusions))
                    {
                        _exclusions = new List<ScheduledEvent>(value);
                        SetTimer();
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI45404");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:Extract.Interfaces.DatabaseService" /> is enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                try
                {
                    if (value != _enabled)
                    {
                        _enabled = value;

                        SetTimer();
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI45405");
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="DateTime"/> of the next occurrence or <c>null</c> if there are no
        /// more occurrences.
        /// </summary>
        public DateTime? NextOccurrence
        {
            get
            {
                try
                {
                    DateTime? nextOccurrence = null;

                    if (RecurrenceUnit.HasValue)
                    {
                        var offset = Start - Start.Floor(RecurrenceUnit.Value);
                        nextOccurrence = (DateTime.Now - offset).Ceiling(RecurrenceUnit.Value) + offset;
                    }
                    else
                    {
                        nextOccurrence = Start;
                    }

                    return (nextOccurrence >= DateTime.Now &&
                            (End == null || nextOccurrence <= End))
                        ? nextOccurrence
                        : null;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI45407");
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="DateTime"/> of the last occurrence or <c>null</c> if there were no
        /// previous occurrences.
        /// </summary>
        public DateTime? LastOccurrence
        {
            get
            {
                try
                {
                    DateTime? lastOccurrence = null;
                    if (RecurrenceUnit.HasValue)
                    {
                        var offset = Start - Start.Floor(RecurrenceUnit.Value);
                        lastOccurrence = (DateTime.Now - offset).Floor(RecurrenceUnit.Value) + offset;
                    }
                    else
                    {
                        lastOccurrence = Start;
                    }

                    return (lastOccurrence <= DateTime.Now)
                        ? lastOccurrence
                        : null;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI45408");
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current time is within a time frame where an
        /// instance of the event is on-going.
        /// </summary>
        public bool InScheduledEvent
        {
            get
            {
                try
                {
                    var endOfLastOccurrence = LastOccurrence + Duration;
                    return (endOfLastOccurrence.HasValue &&
                        LastOccurrence.Value > DateTime.Now &&
                        (End == null || LastOccurrence.Value <= End));
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI45409");
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current time is within a time frame where the event
        /// is excluded from occurring.
        /// </summary>
        public bool InExcludedTime
        {
            get
            {
                return Exclusions.Any(ex => ex.InScheduledEvent);
            }
        }

        #endregion Properties

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="ScheduledEvent"/>. Also deletes
        /// the temporary file being managed by this class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="ScheduledEvent"/>. Also
        /// deletes the temporary file being managed by this class.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (_lock)
                {
                    // Dispose of managed resources
                    if (_timer != null)
                    {
                        _timer.Dispose();
                        _timer = null;
                    }
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members

        #region Private Members

        /// <summary>
        /// Called when deserialization is started
        /// </summary>
        /// <param name="context">The <see cref="StreamingContext"/>.</param>
        [OnDeserializing]
        void OnDeserializing(StreamingContext context)
        {
            if (Version > _CURRENT_VERSION)
            {
                ExtractException ee = new ExtractException("ELI45400", "Settings were saved with a newer version.");
                ee.AddDebugData("SavedVersion", Version, false);
                ee.AddDebugData("CurrentVersion", _CURRENT_VERSION, false);
                throw ee;
            }

            Enabled = false;
        }

        /// <summary>
        /// Called after this instance is deserialized.
        /// </summary>
        /// <param name="context">The <see cref="StreamingContext"/>.</param>
        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {
            Enabled = true;
        }

        /// <summary>
        /// Sets <see cref="_timer"/> to fire at the time of the next instance.
        /// </summary>
        void SetTimer()
        {
            lock (_lock)
            {
                _timer?.Dispose();
                _timer = null;

                if (!Enabled || _eventStarted == null)
                {
                    return;
                }

                var milliseconds = (NextOccurrence - DateTime.Now).Value.TotalMilliseconds;
                if (milliseconds > 0)
                {
                    _timer = new Timer(milliseconds);
                    _timer.Elapsed += (o, e) =>
                    {
                        try
                        {
                            lock (_lock)
                            {
                                if (Enabled && o == _timer && !InExcludedTime)
                                {
                                    // Avoid deadlocks if an event handler were to in turn attempt
                                    // to update or dispose of this instance
                                    Task.Run(() => _eventStarted?.Invoke(this, new EventArgs()));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.ExtractLog("ELI45406");
                        }
                    };
                    _timer.Start();
                }
            }
        }

        #endregion Private Members
    }
}