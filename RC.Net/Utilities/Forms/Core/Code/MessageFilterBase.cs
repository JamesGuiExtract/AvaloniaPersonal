using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Security.Permissions;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// A class containing the data for the <see cref="Message"/> that was handled by the
    /// <see cref="IMessageFilter.PreFilterMessage"/> function.
    /// </summary>
    public class MessageHandledEventArgs : EventArgs
    {
        /// <summary>
        /// The message associated with the event.
        /// </summary>
        private readonly Message _message;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHandledEventArgs"/> class.
        /// </summary>
        /// <param name="message">The <see cref="Message"/> that was handled.</param>
        public MessageHandledEventArgs(Message message)
        {
            _message = message;
        }

        /// <summary>
        /// Gets the message that was handled in the <see cref="IMessageFilter.PreFilterMessage"/>
        /// function.
        /// </summary>
        public Message Message
        {
            get
            {
                return _message;
            }
        }
    }

    /// <summary>
    /// A class for handling PreFilterMessage events for specific controls.
    /// </summary>
    public abstract class MessageFilterBase : IMessageFilter, IDisposable
    {
        #region Fields

        /// <summary>
        /// The collection of controls being observed.
        /// </summary>
        ObservableCollection<Control> _controls = new ObservableCollection<Control>();

        /// <summary>
        /// <see langword="true"/> if the <see cref="MessageFilterBase"/> has been disposed;
        /// <see langword="false"/> otherwise. Defaults to <see langword="true"/> so that if 
        /// the <see cref="MessageFilterBase"/> isn't fully constructed and hasn't been added 
        /// to the application's message filter, no attempt is made to remove it in the finalizer.
        /// </summary>
        bool _disposed = true;

        #endregion Fields

        #region Events

        /// <summary>
        /// Indicates that the <see cref="IMessageFilter.PreFilterMessage"/> method has
        /// handled the <see cref="Message"/> and will return true.
        /// </summary>
        public event EventHandler<MessageHandledEventArgs> MessageHandled;

        #endregion Event

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageFilterBase"/> class.
        /// </summary>
        /// <param name="controls">The array of controls to monitor.</param>
        protected MessageFilterBase(params Control[] controls)
        {
            if (controls != null)
            {
                _controls = new ObservableCollection<Control>(controls);

                Application.AddMessageFilter(this);
            }

            _controls.CollectionChanged += HandleControlCollectionChanged;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the collection of controls that are being listened to.
        /// </summary>
        protected Collection<Control> Controls
        {
            get
            {
                return _controls;
            }
        }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="ObservableCollection{T}.CollectionChanged"/> event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleControlCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            // A control was added to the collection, check if we have
                            // added the message filter yet
                            if (_disposed)
                            {
                                Application.AddMessageFilter(this);
                                _disposed = false;
                            }
                        }
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        {
                            // A control was removed from the collection, check if there
                            // are any controls left, of no controls remove the message
                            // filter.
                            if (_controls.Count == 0)
                            {
                                Application.RemoveMessageFilter(this);
                                _disposed = true;
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28944", ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="MessageHandled"/> event.
        /// </summary>
        /// <param name="e">The data associated with the event.</param>
        void OnMessageHandled(MessageHandledEventArgs e)
        {
            try
            {
                if (MessageHandled != null)
                {
                    MessageHandled(this, e);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI29127", ex);
            }
        }

        #endregion Event Handlers

        #region Methods

        /// <summary>
        /// Called from <see cref="PreFilterMessage"/>.  This method should examine
        /// the <see cref="Message"/> and return <see langword="true"/> to indicate the 
        /// <see cref="Message"/> has been handled and should not be sent on to the
        /// control or <see langword="false"/> if the message has not been handled and
        /// should be passed on to the control.
        /// </summary>
        /// <param name="message">The message to be dispatched. You cannot modify this message.</param>
        /// <returns>
        /// <see langword="true"/> to filter the message and stop it from being dispatched; 
        /// <see langword="false"/> to allow the message to continue to the next filter or control.
        /// </returns>
        protected abstract bool HandleMessage(Message message);

        #endregion Methods

        #region IMessageFilter Members

        /// <summary>
        /// Filters out a message before it is dispatched. This method will call
        /// <see cref="HandleMessage"/> and return the result.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> to filter the message and stop it from being dispatched; 
        /// <see langword="false"/> to allow the message to continue to the next filter or control.
        /// </returns>
        /// <param name="m">The message to be dispatched. You cannot modify this message.</param>
        public bool PreFilterMessage(ref Message m)
        {
            try
            {
                // Check if the message has been handled
                if (HandleMessage(m))
                {
                    // Raise the message handled event.
                    OnMessageHandled(new MessageHandledEventArgs(m));
                    //return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI28945", ex);

                return false;
            }
        }

        #endregion IMessageFilter Members

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="MessageFilterBase"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="MessageFilterBase"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="MessageFilterBase"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>		
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed objects
                if (!_disposed)
                {
                    Application.RemoveMessageFilter(this);
                    _disposed = true;
                }

                if (_controls != null)
                {
                    // Remove the collection changed handler and clear the collection
                    _controls.CollectionChanged -= HandleControlCollectionChanged;
                    _controls.Clear();
                    _controls = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members
    }
}
