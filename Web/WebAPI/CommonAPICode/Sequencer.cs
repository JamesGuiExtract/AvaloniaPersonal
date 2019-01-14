using Extract;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace WebAPI
{
    /// <summary>
    /// A class that blocks threads then releases the blocks in a pre-determined order and interval
    /// to ensure a proper sequence of the items passing through.
    /// </summary>
    public class Sequencer<T> where T : IComparable<T>
    {
        #region Fields

        /// <summary>
        /// Synchronizes access to <see cref="_queue"/>.
        /// </summary>
        object _lock = new object();

        /// <summary>
        /// The items whose release is to be sequenced.
        /// </summary>
        ObservableCollection<T> _queue = new ObservableCollection<T>();

        /// <summary>
        /// The minimum amount of time in milliseconds that should elapse between successive items
        /// being released.
        /// </summary>
        int _separationTime;

        /// <summary>
        /// Indicates whether items will be removed from the queue via the WaitForTurn call.
        /// </summary>
        bool _requireExplicitRemoval;

        /// <summary>
        /// Indicates if currently in the midst of a call to <see cref="Clear"/>.
        /// </summary>
        bool _clearing;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Sequencer&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="separationTime">The minimum amount of time in milliseconds that should
        /// elapse between successive items being released. </param>
        /// <param name="requireExplicitRemoval">If <c>true</c> items will not be removed from the
        /// queue via a WaitForTurn call; if <c>false</c> WaitForTurn will remove items from the queue.
        /// </param>
        public Sequencer(int separationTime, bool requireExplicitRemoval)
        {
            _separationTime = separationTime;
            _requireExplicitRemoval = requireExplicitRemoval;
        }

        #endregion Constructors
        
        #region Properties

        /// <summary>
        /// Gets the number of items currently in the queue.
        /// </summary>
        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _queue.Count;
                }
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Adds <see paramref="objectToQueue"/> to the queue of items to be sequenced.
        /// </summary>
        /// <param name="objectToQueue">The item to queue.</param>
        public void AddToQueue(T objectToQueue)
        {
            try
            {
                lock (_lock)
                {
                    _queue.Add(objectToQueue);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    // There was an situation that occurred in DocumentAPI testing where a null
                    // reference error was thrown from the CollectionChanged after objectToQueue was
                    // already in _queue. This caused everything thing else to pile up on the item
                    // the caller assumed wasn't queued.
                    lock (_lock)
                    {
                        _queue.Remove(objectToQueue);
                    }
                }
                catch (Exception ex2)
                {
                    ex2.ExtractLog("ELI46636");
                }

                throw ex.AsExtract("ELI39517");
            }
        }

        /// <summary>
        /// Determines whether the specified <see paramref="objectToCheckFor"/> is currently queued.
        /// </summary>
        /// <param name="objectToCheckFor">The object to check for.</param>
        /// <returns><see langword="true"/> if the specified <see paramref="objectToCheckFor"/> is
        /// currently queued; otherwise, <see langword="false"/>.</returns>
        public bool Contains(T objectToCheckFor)
        {
            try
            {
                lock (_lock)
                {
                    return _queue.Contains(objectToCheckFor);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39518");
            }
        }

        /// <summary>
        /// Blocks until all items queued prior to <see paramref="objectToWaitFor"/> have been
        /// released from the queue, then removes <see paramref="objectToWaitFor"/> from the queue.
        /// </summary>
        /// <param name="objectToWaitFor">The item to block on.</param>
        /// <returns><see langword="true"/> if the method returns after successfully blocking on
        /// <see paramref="objectToWaitFor"/>; or <see langword="false"/> immediately if the item
        /// was not queued.</returns>
        public bool WaitForTurn(T objectToWaitFor)
        {
            return WaitForTurn(objectToWaitFor, -1);
        }

        /// <summary>
        /// Blocks until all items queued prior to <see paramref="objectToWaitFor"/> have been
        /// released from the queue, then removes <see paramref="objectToWaitFor"/> from the queue.
        /// </summary>
        /// <param name="objectToWaitFor">The item to block on.</param>
        /// <param name="timeout">The maximum number of milliseconds to block before throwing an
        /// exception.</param>
        /// <returns><see langword="true"/> if the method returns after successfully blocking on
        /// <see paramref="objectToWaitFor"/>; or <see langword="false"/> immediately if the item
        /// was not queued.</returns>
        public bool WaitForTurn(T objectToWaitFor, int timeout)
        {
            try
            {
                bool foundItem = false;

                var waitTime = new Stopwatch();
                waitTime.Start();

                using (AutoResetEvent recheckEvent = new AutoResetEvent(false))
                {
                    // Handler to watch for any changes in the _queue it order to check if
                    // objectToWaitFor is next up.
                    NotifyCollectionChangedEventHandler collectionChangedHandler = (s, e) =>
                        {
                            if (e.Action == NotifyCollectionChangedAction.Remove)
                            {
                                recheckEvent.Set();
                            }
                        };

                    try
                    {
                        _queue.CollectionChanged += collectionChangedHandler;

                        // Block until the objectToWaitFor is up or the _queue is cleared.
                        while (!_clearing)
                        {
                            lock (_lock)
                            {
                                if (!_queue.Contains(objectToWaitFor))
                                {
                                    // Return false immediately if objectToWaitFor is not queued.
                                    break;
                                }
                                else if (objectToWaitFor.Equals(_queue[0]))
                                {
                                    foundItem = true;
                                    break;
                                }
                            }

                            // If a timeout has been specified, consider time elapsed in previous
                            // iterations of this loop.
                            int remainingTimeout = timeout;
                            if (timeout >= 0)
                            {
                                remainingTimeout = timeout - (int)waitTime.ElapsedMilliseconds;
                                if (remainingTimeout < 0)
                                {
                                    throw new ExtractException("ELI39519",
                                        "Timeout waiting for next object in sequence.");
                                }
                            }

                            if (!recheckEvent.WaitOne(remainingTimeout))
                            {
                                throw new ExtractException("ELI39520",
                                    "Timeout waiting for next object in sequence.");
                            }
                        }
                    }
                    finally
                    {
                        _queue.CollectionChanged -= collectionChangedHandler;
                    }
                }

                if (!_clearing && !_requireExplicitRemoval)
                {
                    // Remove the item from the queue immediately if no _separationTime is specified.
                    if (_separationTime <= 0)
                    {
                        lock (_lock)
                        {
                            _queue.Remove(objectToWaitFor);
                        }
                    }
                    // If a _separationTime is specified, unblock immediately, but do not dequeue
                    // the item until the separation time has passed.
                    else
                    {
                        Task.Factory.StartNew(() =>
                            {
                                Thread.Sleep(_separationTime);
                                lock (_lock)
                                {
                                    _queue.Remove(objectToWaitFor);
                                }
                            });
                    }
                }

                return foundItem;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39521");
            }
        }

        /// <summary>
        /// Clears the queue.
        /// </summary>
        public void Clear()
        {
            try
            {
                _clearing = true;

                lock (_lock)
                {
                    _queue.Clear();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39522");
            }
            finally
            {
                _clearing = false;
            }
        }

        /// <summary>
        /// Removes the specified <see paramref="objectToRemove"/> from the queue.
        /// </summary>
        /// <param name="objectToRemove">The item to remove from the queue.</param>
        public void Remove(T objectToRemove)
        {
            try
            {
                lock (_lock)
                {
                    _queue.Remove(objectToRemove);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39523");
            }
        }

        /// <summary>
        /// Returns the next value queued or the default value for <see typeref="T"/> if no value
        /// is queued.
        /// </summary>
        public T Peek()
        {
            try
            {
                lock (_lock)
                {
                    return (_queue.Count > 0) ? _queue[0] : default(T);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45645");
            }
        }

        #endregion Methods
    }
}
