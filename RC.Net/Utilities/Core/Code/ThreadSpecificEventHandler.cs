using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace Extract.Utilities
{
    /// <summary>
    /// A class that allows for event handlers for static events to only be invoked on the thread on
    /// which they were added.
    /// <para><b>Note</b></para>
    /// This should only be used in conjunction with static events, not events that associated with
    /// specific <see cref="T:EventHandler&lt;T&gt;.TargetObject"/>
    /// </summary>
    /// <typeparam name="T">The type of <see cref="EventArgs"/> the events use.</typeparam>
    [SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    // As this class is intended to be used only for static events, there should be no opportunity
    // to dispose of these instances.
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    public class ThreadSpecificEventHandler<T> where T : EventArgs
    {
        #region Constants

        /// <summary>
        /// The object name used in licensing calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(ThreadSpecificEventHandler<T>).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// Keeps track of the registered event handlers for this thread.
        /// </summary>
        ThreadLocal<HashSet<WeakReference>> _eventHandlers =
            new ThreadLocal<HashSet<WeakReference>>(() => new HashSet<WeakReference>(new EventComparer()));
        
        /// <summary>
        /// This class works by having this one real event handler which, in turn, manually
        /// executes all registered <see cref="_eventHandlers"/>. This prevents performance issues
        /// for large numbers of registered event handlers.
        /// https://extract.atlassian.net/browse/ISSUE-13900
        /// </summary>
        EventHandler<T> _threadEventHandler;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadSpecificEventHandler&lt;T&gt;"/>
        /// class.
        /// </summary>
        public ThreadSpecificEventHandler()
        {
            try
            {
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.ExtractCoreObjects, "ELI38409", _OBJECT_NAME);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38410");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the <see cref="T:EventHander&lt;T&gt;"/> for the current thread.
        /// </summary>
        /// <returns>The <see cref="T:EventHander&lt;T&gt;"/> for the target object or
        /// <see langword="null"/> if there is no event registered for the object.</returns>
        public EventHandler<T> ThreadEventHandler
        {
            get
            {
                try
                {
                    return _threadEventHandler;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38414");
                }
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Registers the <see paramref="eventHandler"/> on the current thread.
        /// </summary>
        /// <param name="eventHandler">The <see cref="EventHandler&lt;T&gt;"/> to register.</param>
        public void AddEventHandler(EventHandler<T> eventHandler)
        {
            try
            {
                if (_threadEventHandler == null)
                {
                    _threadEventHandler += HandleThreadEvent;
                }

                _eventHandlers.Value.Add(new WeakReference(eventHandler));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38411");
            }
        }

        /// <summary>
        /// Removes registration of the <see paramref="eventHandler"/> on the current thread.
        /// </summary>
        /// <param name="eventHandler">The <see cref="EventHandler&lt;T&gt;"/> to unregister.
        /// </param>
        public void RemoveEventHander(EventHandler<T> eventHandler)
        {
            try
            {
                _eventHandlers.Value.Remove(new WeakReference(eventHandler));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38412");
            }
        }

        #endregion Methods

        #region Private Members

        /// <summary>
        /// Handles the event for this thread so that the event can be dispatched to all
        /// subscribed <see cref="_eventHandlers"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The event args.</param>
        void HandleThreadEvent(object sender, T eventArgs)
        {
            foreach (WeakReference reference in _eventHandlers.Value.ToList())
            {
                EventHandler<T> eventHandler = (EventHandler<T>)reference.Target;
                if (eventHandler != null)
                {
                    eventHandler(sender, eventArgs);
                }
            }
        }

        #endregion Private Members

        /// <summary>
        /// An IEqualityComparer for event handlers that allows for efficient hashing for use in a
        /// dictionary or hash set.
        /// </summary>
        class EventComparer : IEqualityComparer<WeakReference>
        {
            /// <summary>
            /// Determines whether the specified objects are equal.
            /// </summary>
            /// <param name="x">The first event handler to compare.</param>
            /// <param name="y">The second event handler to compare.</param>
            /// <returns>
            /// true if the specified objects are equal; otherwise, false.
            /// </returns>
            public bool Equals(WeakReference x, WeakReference y)
            {
                return x.Target == y.Target;
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <param name="x">The x.</param>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data
            /// structures like a hash table. 
            /// </returns>
            public int GetHashCode(WeakReference x)
            {
                var eventHandler = (EventHandler<T>)x.Target;
                if (eventHandler != null)
                {
                    return HashCode.Start.Hash(eventHandler.Method).Hash(eventHandler.Target);
                }

                return 0;
            }
        }
    }
}
