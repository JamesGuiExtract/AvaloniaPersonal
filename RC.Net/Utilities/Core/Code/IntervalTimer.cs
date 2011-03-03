using Extract.Licensing;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Extract.Utilities
{
    /// <summary>
    /// Represents a timer that measures a <see cref="TimeInterval"/>.
    /// </summary>
    public class IntervalTimer
    {
        #region Fields

        static readonly string _OBJECT_NAME = typeof(IntervalTimer).ToString();

        /// <summary>
        /// The last time that the <see cref="Start"/> method was called.
        /// </summary>
        DateTime _startTime;

        /// <summary>
        /// Measures the seconds elapsed since <see cref="_startTime"/>.
        /// </summary>
        Stopwatch _stopWatch;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this <see cref="IntervalTimer"/> is running.
        /// </summary>
        /// <value><see langword="true"/> if running; otherwise, <see langword="false"/>.</value>
        public bool Running
        {
            get;
            private set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Initializes a new <see cref="IntervalTimer"/> instance and starts measuring the 
        /// elapsed <see cref="TimeInterval"/>.
        /// </summary>
        /// <returns>A <see cref="IntervalTimer"/> that has just begun measuring the 
        /// <see cref="TimeInterval"/>.</returns>
        // "New" is not a suffix in this method.
        [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
        public static IntervalTimer StartNew()
        {
            // Validate licensing
            LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                "ELI30039", _OBJECT_NAME);

            IntervalTimer timer = new IntervalTimer();
            timer.Start();
            return timer;
        }

        /// <summary>
        /// Starts or restarts the <see cref="TimeInterval"/> to measure.
        /// </summary>
        public void Start()
        {
            try
            {
                _startTime = DateTime.Now;
                _stopWatch = Stopwatch.StartNew();

                Running = true;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29905", ex);
            }
        }

        /// <summary>
        /// Stops measuring the elapsed <see cref="TimeInterval"/>.
        /// </summary>
        /// <returns>The <see cref="TimeInterval"/> that elapsed since the last time 
        /// <see cref="Start"/> was called.</returns>
        public TimeInterval Stop()
        {
            try
            {
                Running = false;

                _stopWatch.Stop();
                double elapsedSeconds = _stopWatch.ElapsedMilliseconds / 1000.0;

                return new TimeInterval(_startTime, elapsedSeconds);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29906", ex);
            }
        }

        #endregion Methods
    }
}
