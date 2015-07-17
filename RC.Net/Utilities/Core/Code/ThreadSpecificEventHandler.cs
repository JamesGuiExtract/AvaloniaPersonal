using Extract.Licensing;
using System;
using System.Collections.Concurrent;
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
        /// Keeps track of the event handlers registered per thread, per target object.
        /// </summary>
        ConcurrentDictionary<int, EventHandler<T>> _eventHandlers =
            new ConcurrentDictionary<int, EventHandler<T>>();

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
                    int threadId = Thread.CurrentThread.ManagedThreadId;

                    EventHandler<T> existingEventHandler = null;
                    _eventHandlers.TryGetValue(threadId, out existingEventHandler);

                    return existingEventHandler;
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
                int threadId = Thread.CurrentThread.ManagedThreadId;

                EventHandler<T> existingEventHandler;
                _eventHandlers.TryGetValue(threadId, out existingEventHandler);

                // It is okay if existingEventHandler does not exist (is null). The concatenation
                // operator will initialize it.
                existingEventHandler += eventHandler;

                // The += operator creates a new instance rather than updating the original
                // instance. The dictionary needs to be updated with the new value.
                _eventHandlers[threadId] = existingEventHandler;
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
                int threadId = Thread.CurrentThread.ManagedThreadId;

                EventHandler<T> existingEventHandler = null;
                if (_eventHandlers.TryGetValue(threadId, out existingEventHandler))
                {
                    existingEventHandler -= eventHandler;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38412");
            }
        }

        #endregion Methods
    }
}
