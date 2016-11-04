using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        /// significant enough to warrant a separate operation. Minor mementos will continue to be
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
        /// Adds a reference to object <see paramref="reference"/> that must be released before
        /// this memento can be disposed.
        /// </summary>
        /// <param name="reference">The object referencing this instance.</param>
        void AddDisposeReference(object reference);

        /// <summary>
        /// Removes a reference to object <see paramref="reference"/> allowing it to be disposed if
        /// there are no other references remaining.
        /// </summary>
        /// <param name="reference">The object no longer referencing this instance.</param>
        /// <returns><c>True</c> if this is a disposable instance and <see paramref="reference"/>
        /// was the last instance referencing this instance; otherwise, <c>False</c>.</returns>
        bool RemoveDisposeReference(object reference);

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
    /// Manages the tracking of data changes for an application and the undoing of changes on
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

        #region UndoState

        /// <summary>
        /// A particular state undo and redo operations; used for <see cref="GetState"/> and <see cref="RestoreState"/>.
        /// </summary>
        class UndoState : IDisposable
        {
            /// <summary>
            /// The undo operations in the state.
            /// </summary>
            public Stack<Stack<IUndoMemento>> UndoStack;

            /// <summary>
            /// The redo operations in the state.
            /// </summary>
            public Stack<Stack<IUndoMemento>> RedoStack;

            #region IDisposable Members

            /// <summary>
            /// Releases all resources used by the <see cref="UndoState"/>.
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
                    ExtractException.Display("ELI41489", ex);
                }
            }

            /// <summary>
            /// Releases all resources used by the <see cref="UndoState"/>.
            /// </summary>
            /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
            /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
            protected virtual void Dispose(bool disposing)
            {
                if (disposing && UndoStack != null && RedoStack != null)
                {
                    foreach (var operation in UndoStack.Concat(RedoStack))
                    {
                        foreach (var memento in operation)
                        {
                            if (memento.RemoveDisposeReference(this))
                            {
                                ((IDisposable)memento).Dispose();
                            }
                        }
                    }

                    UndoStack = null;
                    RedoStack = null;
                }

                // Dispose of unmanaged resources
            }

            #endregion IDisposable Members
        }

        #endregion UndoState

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
        List<IUndoMemento> _mementosToDispose = new List<IUndoMemento>();

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
        /// regardless of <see cref="UndoMementoSignificance"/> or whether
        /// <see cref="StartNewOperation"/> is called. This provides a mechanism to ensure
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
        /// regardless of <see cref="UndoMementoSignificance"/> or whether
        /// <see cref="StartNewOperation"/> is called. This provides a mechanism to ensure
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
                                PushOperation(_currentOperation, _undoStack);
                                _currentOperation = new Stack<IUndoMemento>();
                                _extendCurrentOperation = false;

                                if (_undoStack.Count == 1)
                                {
                                    OnUndoAvailabilityChanged();
                                }
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
                                        _currentOperation.Where(m => m is IDisposable));
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
                return (_undoStack.Count > 0);
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
                memento.AddDisposeReference(this);

                // If we are not in an undo or redo operation, any non-supporting memento should
                // cause all available redo operations to be cleared.
                if (!_inUndoOperation && !_inRedoOperation && _redoStack.Count > 0 &&
                    memento.Significance == UndoMementoSignificance.Substantial)
                {
                    ClearRedoOperations();
                }

                // If in an undo operation, all mementos received should be collected for an
                // opposite redo operation.
                if (_inUndoOperation)
                {
                    if (!_currentOperation.Any(m => m.Supersedes(memento)))
                    {
                        _currentOperation.Push(memento);

                        return;
                    }
                }
                // If in a redo operation, all mementos received should be collected for an
                // opposite undo operation.
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
                        PushOperation(_currentOperation, _undoStack);
                        _currentOperation = new Stack<IUndoMemento>();

                        if (_undoStack.Count == 1)
                        {
                            OnUndoAvailabilityChanged();
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

                            return;
                        }
                    }
                }

                // If the memento was not recorded but is disposable, add it to _mementosToDispose. 
                if (memento is IDisposable)
                {
                    _mementosToDispose.Add(memento);
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
        /// Requests that the current operation be extended beyond the next substantial data change
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

                PushOperation(_currentOperation, _undoStack);
                _currentOperation = new Stack<IUndoMemento>();

                undoOperation = _undoStack.Pop();

                // Perform the undo
                if (undoOperation != null)
                {
                    foreach (IUndoMemento memento in undoOperation)
                    {
                        memento.Undo();
                    }

                    _mementosToDispose.AddRange(
                        undoOperation.Where(m => m is IDisposable));

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
                        // By undoing mementos that were done during an undo operation, we are redoing.
                        memento.Undo();
                    }

                    _mementosToDispose.AddRange(redoOperation.Where(m => m is IDisposable));

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
                    PushOperation(_currentOperation, _undoStack);
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

                _mementosToDispose.AddRange(_currentOperation.Where(m => m is IDisposable));
                foreach (Stack<IUndoMemento> operation in _undoStack)
                {
                    _mementosToDispose.AddRange(operation.Where(m => m is IDisposable));
                }
                foreach (Stack<IUndoMemento> operation in _redoStack)
                {
                    _mementosToDispose.AddRange(operation.Where(m => m is IDisposable));
                }

                foreach (var memento in _mementosToDispose)
                {
                    if (memento.RemoveDisposeReference(this))
                    {
                        ((IDisposable)memento).Dispose();
                    }
                }

                _mementosToDispose.Clear();
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

        /// <summary>
        /// Pushes the specified <see cref="operation"/> onto the specified <see cref="undoStack"/>.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="undoStack">The undo stack.</param>
        static void PushOperation(Stack<IUndoMemento> operation,
            Stack<Stack<IUndoMemento>> undoStack)
        {
            // If operation is empty don't add it to the stack
            // https://extract.atlassian.net/browse/ISSUE-14221
            if (operation.Count == 0)
            {
                return;
            }
            // If either the last undo operation, or the operation getting pushed don't contain any substantial
            // mementos, merge the two rather than adding a new undo operation.
            else if (undoStack.Count > 0 &&
                (!undoStack.Peek().Any(memento => memento.Significance == UndoMementoSignificance.Substantial) ||
                !operation.Any(memento => memento.Significance == UndoMementoSignificance.Substantial)))
            {
                undoStack.Peek().Push(operation);
            }
            else
            {
                undoStack.Push(operation);
            }
        }

        /// <summary>
        /// Gets the current undo/redo operations as a state object that can be restored later.
        /// </summary>
        /// <returns>An <c>object</c> representing the current undo and redo operations.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public IDisposable GetState()
        {
            try
            {
                var state = new UndoState()
                {
                    UndoStack = new Stack<Stack<IUndoMemento>>(_undoStack.Reverse()),
                    RedoStack = new Stack<Stack<IUndoMemento>>(_redoStack.Reverse())
                };

                // Make a copy of the _currentOperation so that further changes don't get reflected
                // in the returned state.
                var currentOperation = new Stack<IUndoMemento>(_currentOperation.Reverse());
                PushOperation(currentOperation, state.UndoStack);

                foreach (var operation in state.UndoStack.Concat(state.RedoStack))
                {
                    foreach (var memento in operation)
                    {
                        memento.AddDisposeReference(state);
                    }
                }

                return state;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41478");
            }
        }

        /// <summary>
        /// Restores availability of a previously retrieved state of undo and redo operations.
        /// </summary>
        /// <param name="state">An <c>object</c> representing the undo and redo to make available.</param>
        public void RestoreState(object state)
        {
            try
            {
                UndoState undoState = state as UndoState;
                ExtractException.Assert("ELI41479", "Invalid state", undoState != null);

                bool undoAvailabilityChange = UndoOperationAvailable;
                bool redoAvailabilityChange = RedoOperationAvailable;

                ClearHistory();
                _undoStack = new Stack<Stack<IUndoMemento>>(undoState.UndoStack.Reverse());
                _redoStack = new Stack<Stack<IUndoMemento>>(undoState.RedoStack.Reverse());
                
                if (undoAvailabilityChange != UndoOperationAvailable)
                {
                    OnUndoAvailabilityChanged();
                }
                if (redoAvailabilityChange != RedoOperationAvailable)
                {
                    OnRedoAvailabilityChanged();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41480");
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
                    _mementosToDispose.AddRange(operation.Where(m => m is IDisposable));
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
