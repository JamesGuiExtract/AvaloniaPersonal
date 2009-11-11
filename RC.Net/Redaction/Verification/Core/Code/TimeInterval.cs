using System;

namespace Extract.Redaction
{
    /// <summary>
    /// Represents a start time and end time.
    /// </summary>
    public class TimeInterval
    {
        #region TimeInterval Fields

        /// <summary>
        /// The date and time the <see cref="TimeInterval"/> started.
        /// </summary>
        readonly DateTime _start;

        /// <summary>
        /// The amount of time in seconds elapsed since the <see cref="_start"/>.
        /// </summary>
        readonly double _elapsedSeconds;

        #endregion TimeInterval Fields

        #region TimeInterval Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeInterval"/> class.
        /// </summary>
        public TimeInterval(DateTime startTime, double elapsedSeconds)
        {
            _start = startTime;
            _elapsedSeconds = elapsedSeconds;
        }

        #endregion TimeInterval Constructors

        #region TimeInterval Properties

        /// <summary>
        /// Gets the date and time the <see cref="TimeInterval"/> started.
        /// </summary>
        /// <value>The date and time the <see cref="TimeInterval"/> started.</value>
        public DateTime Start
        {
            get
            {
                return _start;
            }
        }

        /// <summary>
        /// Gets the amount of time in seconds elapsed since the <see cref="Start"/>.
        /// </summary>  
        /// <value>The amount of time in seconds elapsed since the <see cref="Start"/>.</value>
        public double ElapsedSeconds
        {
            get
            {
                return _elapsedSeconds;
            }
        }

        #endregion TimeInterval Properties
    }
}
