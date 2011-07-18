using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Extract.Testing.Utilities
{
    /// <summary>
    /// A testing helper class designed to count events that are fired.  Can be used to count
    /// up to four separate events at one time.
    /// </summary>
    public class EventCounters
    {
        #region Fields

        /// <summary>
        /// Counts the number of times an event occurs.
        /// </summary>
        private int _eventCounter;

        /// <summary>
        /// Counts the number of times a second event occurs.
        /// </summary>
        private int _eventCounter2;

        /// <summary>
        /// Counts the number of times a third event occurs.
        /// </summary>
        private int _eventCounter3;
        
        /// <summary>
        /// Counts the number of times a fourth event occurs.
        /// </summary>
        private int _eventCounter4;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EventCounters" /> class.
        /// </summary>
        public EventCounters()
        {
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets/sets the value of the first event counter.
        /// </summary>
        /// <returns>The value of the first event counter.</returns>
        /// <value>The value of the first event counter.</value>
        public int EventCounter
        {
            get
            {
                return _eventCounter;
            }
            set
            {
                _eventCounter = value;
            }
        }

        /// <summary>
        /// Gets/sets the value of the second event counter.
        /// </summary>
        /// <returns>The value of the second event counter.</returns>
        /// <value>The value of the second event counter.</value>
        public int EventCounter2
        {
            get
            {
                return _eventCounter2;
            }
            set
            {
                _eventCounter2 = value;
            }
        }

        /// <summary>
        /// Gets/sets the value of the third event counter.
        /// </summary>
        /// <returns>The value of the third event counter.</returns>
        /// <value>The value of the third event counter.</value>
        public int EventCounter3
        {
            get
            {
                return _eventCounter3;
            }
            set
            {
                _eventCounter3 = value;
            }
        }

        /// <summary>
        /// Gets/sets the value of the fourth event counter.
        /// </summary>
        /// <returns>The value of the fourth event counter.</returns>
        /// <value>The value of the fourth event counter.</value>
        public int EventCounter4
        {
            get
            {
                return _eventCounter4;
            }
            set
            {
                _eventCounter4 = value;
            }
        }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// An event handler that counts the number of occurrences of a generic event.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        // This method has been reviewed and does not contain any dangerous functionality
        // that would compromise the security of the software.
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        public void CountEvent<TEventArgs>(object sender, TEventArgs e)
            where TEventArgs : EventArgs
        {
            _eventCounter++;
        }

        /// <summary>
        /// An event handler that counts the number of occurrences of a second generic event.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        // This method has been reviewed and does not contain any dangerous functionality
        // that would compromise the security of the software.
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        public void CountEvent2<TEventArgs>(object sender, TEventArgs e)
            where TEventArgs : EventArgs
        {
            _eventCounter2++;
        }

        /// <summary>
        /// An event handler that counts the number of occurrences of a third generic event.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        // This method has been reviewed and does not contain any dangerous functionality
        // that would compromise the security of the software.
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        public void CountEvent3<TEventArgs>(object sender, TEventArgs e)
            where TEventArgs : EventArgs
        {
            _eventCounter3++;
        }

        /// <summary>
        /// An event handler that counts the number of occurrences of a fourth generic event.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        // This method has been reviewed and does not contain any dangerous functionality
        // that would compromise the security of the software.
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        public void CountEvent4<TEventArgs>(object sender, TEventArgs e)
            where TEventArgs : EventArgs
        {
            _eventCounter4++;
        }

        #endregion Event Handlers

        #region Methods

        /// <summary>
        /// Sets all four event counters back to 0
        /// </summary>
        public void ResetEventCounters()
        {
            _eventCounter = 0;
            _eventCounter2 = 0;
            _eventCounter3 = 0;
            _eventCounter4 = 0;
        }

        #endregion Methods
    }
}
