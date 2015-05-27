﻿using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Extract.Utilities
{
    /// <summary>
    /// Indicates the significance of any given change in the software. This determines how mementos
    /// are grouped into operations by the <see cref="UndoManager"/>.
    /// </summary>
    public enum UndoMementoSignificance
    {
        /// <summary>
        /// Indicates changes that are only meaningful in the context of other changes. Does not
        /// represent any change to actual data. These mementos may not be tracked if they occur
        /// immediately after document load or an Undo operation. May be used, for example,
        /// to allow control state/selection to be set as they were just prior to an operation in
        /// which data changed.
        /// </summary>
        Supporting = 0,

        /// <summary>
        /// Indicates changes to data which must be tracked, but that are not on their own
        /// significant enough to warrant a seperate operation. Minor mementos will continue to be
        /// appended to the current "operation" until the next substantial memento is encountered.
        /// </summary>
        Minor = 1,

        /// <summary>
        /// Indicates changes to data which is significant enough on its own to be considered an
        /// independent operation.
        /// </summary>
        Substantial = 2
    }

    /// <summary>
    /// Represents a change or edit made by a user as well as the logic to undo the change.
    /// </summary>
    public interface IUndoMemento
    {
        /// <summary>
        /// Indicates the significance of any given change in the software. This determines how
        /// mementos are grouped into operations by the <see cref="UndoManager"/>.
        /// </summary>
        UndoMementoSignificance Significance
        {
            get;
        }

        /// <summary>
        /// As new <see cref="IUndoMemento"/>s are added to the operation in which the current
        /// memento exists, this method will be called with the new memento as
        /// <see paramref="memento"/>. If the new memento provides no information not contained
        /// or replaced by the information in this memento, the new memento will be disregarded.
        /// <para><b>Note</b></para>
        /// Since this method will always be called between all pairs of <see cref="IUndoMemento"/>s
        /// in an operation, it provides a way for <see cref="IUndoMemento"/> implementations to
        /// make adjustments based on other <see cref="IUndoMemento"/>s in the operation.
        /// </summary>
        /// <param name="memento">The <see cref="IUndoMemento"/> proposed to be added to the current
        /// operation.</param>
        /// <returns><see langword="true"/> if this memento supersedes <see paramref="memento"/> and
        /// it should therefore be disregarded; <see langword="false"/> to allow the memento to be
        /// added.</returns>
        bool Supersedes(IUndoMemento memento);

        /// <summary>
        /// Reverts the change represented by this memento.
        /// </summary>
        void Undo();
    }

    /// <summary>
    /// Manages the tracking of data changes for an application and the undo-ing of changes on
    /// demand. Whatever uses the UndoManager is responsible for calling AddMemento for every
    /// change that is made in the application. Changes will be grouped into operations via either
    /// the <see cref="StartNewOperation"/> method or via use of the
    /// <see cref="OperationInProgress"/> property. <see cref="Undo"/> will roll back changes one
    /// operation at a time.
    /// </summary>
    public class UndoManager : IDisposable
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(UndoManager).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="IUndoMemento"/>s that represent any on-going operation.
        /// </summary>
        Stack<IUndoMemento> _currentOperation = new Stack<IUndoMemento>();

        /// <summary>
        /// Represents all operations performed prior to any current operation.
        /// </summary>
        Stack<Stack<IUndoMemento>> _undoStack = new Stack<Stack<IUndoMemento>>();

        /// <summary>
        /// Represents all operations that have been undone (while not having any major or minor
        /// changes since).
        /// </summary>
        Stack<Stack<IUndoMemento>> _redoStack = new Stack<Stack<IUndoMemento>>();

        /// <summary>
        /// Keeps track of mementos that need to be disposed of the next time history is cleared.
        /// </summary>
        List<IDisposable> _mementosToDispose = new List<IDisposable>();

        /// <summary>
        /// Indicates whether an operation is currently being reverted.
        /// </summary>
        bool _inUndoOperation;

        /// <summary>
        /// Indicates whether an operation is currently being redone.
        /// </summary>
        bool _inRedoOperation;

        /// <summary>
        /// Indicates whether the <see cref="UndoManager"/> should currently be tracking added
        /// <see cref="IUndoMemento"/>s.
        /// </summary>
        bool _trackOperations;

        /// <summary>
        /// Indicates any added <see cref="IUndoMemento"/>s should be added to the current operation
        /// regarless of <see cref="UndoMementoSignificance"/> or whether
        /// <see cref="StartNewOperation"/> is called. This provideds a mechanism to ensure
        /// protracted/complex operations are not inappropriately divided.
        /// </summary>
        bool _operationInProgress;

        /// <summary>
        /// Indicates that <see cref="StartNewOperation"/> has been called and that a new operation
        /// should be started when the next <see cref="IUndoMemento"/> of substantial significance
        /// is added.
        /// </summary>
        bool _newOperationPending;

        /// <summary>
        /// Indicates whether the current operation should be extended.
        /// </summary>
        bool _extendCurrentOperation;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UndoManager"/> class.
        /// </summary>
        public UndoManager()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.ExtractCoreObjects, "ELI30999", _OBJECT_NAME);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31000", ex);
            }
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Indicates that the availability of an operation for the <see cref="Undo"/> method to
        /// revert has changed.
        /// </summary>
        public event EventHandler<EventArgs> UndoAvailabilityChanged;

        /// <summary>
        /// Indicates that the availability of an operation for the <see cref="Redo"/> method to
        /// redo has changed.
        /// </summary>
        public event EventHandler<EventArgs> RedoAvailabilityChanged;

        /// <summary>
        /// Indicates when an <see cref="Undo"/> or <see cref="Redo"/> operation has completed.
        /// </summary>
        public event EventHandler<EventArgs> OperationEnded;

        #endregion Events

        #region Properties

        /// <summary>
        /// Indicates whether the <see cref="UndoManager"/> should be tracking added
        /// <see cref="IUndoMemento"/>s.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to track changes; otherwise, <see langword="false"/>.
        /// </value>
        public bool TrackOperations
        {
            get
            {
                return _trackOperations;
            }

            set
            {
                _trackOperations = value;
            }
        }

        /// <summary>
        /// Indicates any added <see cref="IUndoMemento"/>s should be added to the current operation
        /// regarless of <see cref="UndoMementoSignificance"/> or whether
        /// <see cref="StartNewOperation"/> is called. This provideds a mechanism to ensure
        /// protracted/complex operations are not inappropriately divided.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to add all new <see cref="IUndoMemento"/>s to the current
        /// operation; otherwise, <see langword="false"/>.
        /// </value>
        public bool OperationInProgress
        {
            get
            {
                return _operationInProgress;
            }

            set
            {
                try
                {
                    if (_operationInProgress != value)
                    {
                        if (_currentOperation.Count > 0)
                        {
                            if (value)
                            {
                                // If starting a new operation, stop and record the current one.
                                _undoStack.Push(_currentOperation);
                                _currentOperation = new Stack<IUndoMemento>();
                                _extendCurrentOperation = false;
                            }
                            // If extending the current operation, leave the _currentOperation
                            // stack as is until the next call to StartNewOperation or until
                            // OperationInProgress is set back to true.
                            else if (!_extendCurrentOperation)
                            {
                                if (_currentOperation
                                    .Any(m => m.Significance >= UndoMementoSignificance.Minor))
                                {
                                    // If ending an operation, if at least one memento was of at
                                    // least minor significance, consider a new operation to be
                                    // pending.
                                    _newOperationPending = true;
                                }
                                else
                                {
                                    // If ending an operation but none of the operations were
                                    // significant, discard them.
                                    _mementosToDispose.AddRange(
                                        _currentOperation.OfType<IDisposable>());
                                    _currentOperation.Clear();
                                }
                            }
                        }

                        _operationInProgress = value;
                    }
                }
                catch (Exception ex)
                {
                    ExtractException.AsExtractException("ELI31003", ex);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether an operation is available to revert.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if an operation is available to revert otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool UndoOperationAvailable
        {
            get
            {
                return (_currentOperation.Count > 0 || _undoStack.Count > 0);
            }
        }


        /// <summary>
        /// Gets a value indicating whether an operation is available to redo.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if an operation is available to redo otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool RedoOperationAvailable
        {
            get
            {
                return (_redoStack.Count > 0);
            }
        }

        /// <summary>
        /// Gets a value indicating whether an operation is currently being reverted.
        /// </summary>
        /// <value><see langword="true"/> if in an undo operation; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool InUndoOperation
        {
            get
            {
                return _inUndoOperation;
            }
        }

        /// <summary>
        /// Gets a value indicating whether an operation is currently being redone.
        /// </summary>
        /// <value><see langword="true"/> if in an redo operation; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool InRedoOperation
        {
            get
            {
                return _inRedoOperation;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Requests that the <see cref="UndoManager"/> record the change represented by the 
        /// <see paramref="memento"/> so that the change can be un-done at a later time.
        /// </summary>
        /// <param name="memento">The <see cref="IUndoMemento"/> to be potentially un-done later.
        /// </param>
        public void AddMemento(IUndoMemento memento)
        {
            try
            {
                // If we are not in an undo or redo operation, any non-supporting memento should
                // cause all available redo operations to be cleared.
                if (!_inUndoOperation && !_inRedoOperation && _redoStack.Count > 0 &&
                    memento.Significance != UndoMementoSignificance.Supporting)
                {
                    ClearRedoOperations();
                }

                // If in an undo operation, all mementos received should be collected for an
                // opposite redo operataion.
                if (_inUndoOperation)
                {
                    if (!_currentOperation.Any(m => m.Supersedes(memento)))
                    {
                        _currentOperation.Push(memento);

                        return;
                    }
                }
                // If in a redo operation, all mementos received should be collected for an
                // opposite undo operataion.
                else if (_inRedoOperation)
                {
                    if (!_currentOperation.Any(m => m.Supersedes(memento)))
                    {
                        _currentOperation.Push(memento);

                        return;
                    }
                }
                // Otherwise, collect the memento based upon _trackOperations and the memento itself.
                else if (_trackOperations)
                {
                    // If a new operations has been requested and the incoming memento is a
                    // substantial change, break off the current operation and start a new one.
                    if (!_inRedoOperation && _newOperationPending && !_operationInProgress &&
                        memento.Significance == UndoMementoSignificance.Substantial)
                    {
                        if (_currentOperation.Count > 0)
                        {
                            _undoStack.Push(_currentOperation);
                            _currentOperation = new Stack<IUndoMemento>();
                        }

                        _newOperationPending = false;
                    }

                    // If there is an on-going operation or the memento is significant enough to
                    // start a new one, record it now.
                    if (_operationInProgress || _currentOperation.Count > 0 ||
                        memento.Significance >= UndoMementoSignificance.Minor)
                    {
                        if (!_currentOperation.Any(m => m.Supersedes(memento)))
                        {
                            _currentOperation.Push(memento);

                            if (_currentOperation.Count == 1 && _undoStack.Count == 0)
                            {
                                OnUndoAvailabilityChanged();
                            }

                            return;
                        }
                    }
                }

                // If the memento was not recorded but is disposable, add it to _mementosToDispose. 
                IDisposable disposableMemento = memento as IDisposable;
                if (disposableMemento != null)
                {
                    _mementosToDispose.Add(disposableMemento);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31004", ex);
            }
        }

        /// <summary>
        /// Requests that the next <see cref="UndoMementoSignificance.Substantial"/> change marks
        /// the beginning of a new operation. <see cref="UndoMementoSignificance.Supporting"/>
        /// mementos or mementos that are added while <see cref="OperationInProgress"/> is
        /// <see langword="true"/> will continue to be added to the current operation.
        /// </summary>
        public void StartNewOperation()
        {
            _extendCurrentOperation = false;
            _newOperationPending = true;
        }

        /// <summary>
        /// Requests that the current operation be exetended beyond the next substantial data change
        /// even if <see cref="StartNewOperation"/> had been called or
        /// <see cref="OperationInProgress"/> is returned to <see langword="false"/> from
        /// <see langword="true"/>. The operation will end the next time
        /// <see cref="StartNewOperation"/> is called or <see cref="OperationInProgress"/> is set
        /// back to <see langword="true"/>.
        /// </summary>
        public void ExtendCurrentOperation()
        {
            try
            {
                if (_currentOperation.Count > 0)
                {
                    _newOperationPending = false;
                    _extendCurrentOperation = true;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31092", ex);
            }
        }

        /// <summary>
        /// Reverts the changes from the last recorded operation.
        /// </summary>
        /// <param name="endImmediately"><see langword="true"/> if the undo operation should
        /// immediately (synchronously); <see langword="false"/> otherwise. If any undone mementos
        /// may result in new mementos being added asynchronously, specify <see langword="false"/>
        /// and call <see cref="EndUndo"/> once all such mementos have been added.</param>
        public void Undo(bool endImmediately)
        {
            try
            {
                ExtractException.Assert("ELI34451",
                    "Cannot perform an undo operation while in an undo operation", !_inUndoOperation);
                ExtractException.Assert("ELI34454",
                    "Cannot perform an undo operation while in a redo operation", !_inRedoOperation);

                _inUndoOperation = true;
                _newOperationPending = false;

                Stack<IUndoMemento> undoOperation = null;

                if (_currentOperation.Count > 0)
                {
                    // Undo the current operation
                    undoOperation = _currentOperation;

                    // Start a new operation.
                    _currentOperation = new Stack<IUndoMemento>();
                }
                else if (_undoStack.Count > 0)
                {
                    // There is no current operation; undo the next operation on the stack.
                    undoOperation = _undoStack.Pop();
                }

                // Perform the undo
                if (undoOperation != null)
                {
                    foreach (IUndoMemento memento in undoOperation)
                    {
                        memento.Undo();
                    }

                    _mementosToDispose.AddRange(undoOperation.OfType<IDisposable>());

                    // End the undo operation if specified.
                    if (endImmediately)
                    {
                        EndUndo();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31006", ex);
            }
        }

        /// <summary>
        /// Ends the active undo operation (any mementos received after this point will not be
        /// considered part of the undo operation).
        /// </summary>
        public void EndUndo()
        {
            try
            {
                ExtractException.Assert("ELI34442",
                    "Cannot end undo operation; no undo operation in progress.", _inUndoOperation);

                _inUndoOperation = false;

                if (_currentOperation.Any(memento =>
                    memento.Significance >= UndoMementoSignificance.Minor))
                {
                    // Add all mementos recorded during the undo operation as a redo operation.
                    _redoStack.Push(_currentOperation);
                }

                OnOperationEnded();

                if (_redoStack.Count == 1)
                {
                    OnRedoAvailabilityChanged();
                }

                // Clear all mementos added during the undo.
                _currentOperation = new Stack<IUndoMemento>();

                if (!UndoOperationAvailable)
                {
                    OnUndoAvailabilityChanged();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34441");
            }
        }

        /// <summary>
        /// Re-does the changes from the last operation undone.
        /// </summary>
        /// <param name="endImmediately"><see langword="true"/> if the redo operation should
        /// immediately (synchronously); <see langword="false"/> otherwise. If any redone mementos
        /// may result in new mementos being added asynchronously, specify <see langword="false"/>
        /// and call <see cref="EndRedo"/> once all such mementos have been added.</param>
        public void Redo(bool endImmediately)
        {
            try
            {
                ExtractException.Assert("ELI34452",
                    "Cannot perform a redo operation while in an undo operation", !_inUndoOperation);
                ExtractException.Assert("ELI34453",
                    "Cannot perform a redo operation while in a redo operation", !_inRedoOperation);

                _inRedoOperation = true;

                ExtractException.Assert("ELI34432",
                    "Redo operation not allowed when data has changed since last undo.",
                    _operationInProgress == false);

                Stack<IUndoMemento> redoOperation = _redoStack.Pop();

                // Perform the redo
                if (redoOperation != null)
                {
                    foreach (IUndoMemento memento in redoOperation)
                    {
                        // By undo-ing mementos that were done during an undo operation, we are redo-ing.
                        memento.Undo();
                    }

                    _mementosToDispose.AddRange(redoOperation.OfType<IDisposable>());

                    // End the redo operation if specified.
                    if (endImmediately)
                    {
                        EndRedo();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI34433", ex);
            }
        }

        /// <summary>
        /// Ends the active redo operation (any mementos received after this point will not be
        /// considered part of the redo operation).
        /// </summary>
        public void EndRedo()
        {
            try
            {
                ExtractException.Assert("ELI34443",
                    "Cannot end redo operation; no redo operation in progress.", _inRedoOperation);

                _inRedoOperation = false;

                if (_currentOperation.Any(memento =>
                    memento.Significance >= UndoMementoSignificance.Minor))
                {
                    // Add all mementos recorded during the redo operation as an undo operation.
                    _undoStack.Push(_currentOperation);
                }

                OnOperationEnded();

                if (_undoStack.Count == 1)
                {
                    OnUndoAvailabilityChanged();
                }

                // Clear all mementos added during the redo.
                _currentOperation = new Stack<IUndoMemento>();

                if (!RedoOperationAvailable)
                {
                    OnRedoAvailabilityChanged();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38265");
            }
        }

        /// <summary>
        /// Clears all recorded <see cref="IUndoMemento"/>s and disposes of any that implement
        /// <see cref="IDisposable"/>.
        /// </summary>
        public void ClearHistory()
        {
            try
            {
                _newOperationPending = false;

                bool undoAvailabilityChange = UndoOperationAvailable;
                bool redoAvailabilityChange = RedoOperationAvailable;

                _mementosToDispose.AddRange(_currentOperation.OfType<IDisposable>());
                foreach (Stack<IUndoMemento> operation in _undoStack)
                {
                    _mementosToDispose.AddRange(operation.OfType<IDisposable>());
                }
                foreach (Stack<IUndoMemento> operation in _redoStack)
                {
                    _mementosToDispose.AddRange(operation.OfType<IDisposable>());
                }

                CollectionMethods.ClearAndDispose(_mementosToDispose);

                _currentOperation.Clear();
                _undoStack.Clear();
                _redoStack.Clear();

                if (undoAvailabilityChange)
                {
                    OnUndoAvailabilityChanged();
                }
                if (redoAvailabilityChange)
                {
                    OnRedoAvailabilityChanged();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31007", ex);
            }
        }

        #endregion Methods

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="UndoManager"/>
        /// </summary>
        public void Dispose()
        {
            try
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31026", ex);
            }
        }

        /// <overloads>Releases all resources used by the <see cref="UndoManager"/>.</overloads>
        /// <summary>
        /// Releases all resources used by the <see cref="UndoManager"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed objects
                ClearHistory();
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members

        #region Private Members

        /// <summary>
        /// Raises the <see cref="UndoAvailabilityChanged"/> event.
        /// </summary>
        void OnUndoAvailabilityChanged()
        {
            var eventHandler = UndoAvailabilityChanged;
            if (eventHandler != null)
            {
                UndoAvailabilityChanged(this, new EventArgs());
            }
        }

        /// <summary>
        /// Raises the <see cref="RedoAvailabilityChanged"/> event.
        /// </summary>
        void OnRedoAvailabilityChanged()
        {
            var eventHandler = RedoAvailabilityChanged;
            if (eventHandler != null)
            {
                RedoAvailabilityChanged(this, new EventArgs());
            }
        }

        /// <summary>
        /// Raises the <see cref="OperationEnded"/> event.
        /// </summary>
        void OnOperationEnded()
        {
            var eventHandler = OperationEnded;
            if (eventHandler != null)
            {
                eventHandler(this, new EventArgs());
            }
        }

        /// <summary>
        /// Clears all available redo operations.
        /// </summary>
        void ClearRedoOperations()
        {
            try
            {
                _newOperationPending = false;

                bool redoAvailabilityChange = RedoOperationAvailable;

                foreach (Stack<IUndoMemento> operation in _redoStack)
                {
                    _mementosToDispose.AddRange(operation.OfType<IDisposable>());
                }

                _redoStack.Clear();

                if (redoAvailabilityChange)
                {
                    OnRedoAvailabilityChanged();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI34430", ex);
            }
        }

        #endregion Private Members
    }
}
