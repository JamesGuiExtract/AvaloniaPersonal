using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.DataEntry
{
    /// <summary>
    /// Represents a revertible change that involves an <see cref="IAttribute"/> in the data entry
    /// framework.
    /// </summary>
    internal abstract class DataEntryAttributeMemento : IUndoMemento
    {
        #region Fields

        /// <summary>
        /// The <see cref="IAttribute"/> involved in the change.
        /// </summary>
        IAttribute _affectedAttribute;

        /// <summary>
        /// A set of <see cref="IAttribute"/>s that need to be refreshed at the end of the undo
        /// operation this memento is a part of.
        /// </summary>
        HashSet<IAttribute> _attributesToRefresh = new HashSet<IAttribute>();

        /// <summary>
        /// Indicates whether the entire <see cref="IDataEntryControl"/> associated with this
        /// <see cref="IAttribute"/> needs to be refreshed after the undo operation this memento
        /// is a part of.
        /// </summary>
        bool _refreshEntireControl;

        /// <summary>
        /// Indicates whether this change is the first in the current operation for _owningControl
        /// (meaning it will be the last to be un-done).
        /// </summary>
        bool _firstMementoForControl = true;

        /// <summary>
        /// The <see cref="IDataEntryControl"/> associated with the _affectedAttribute.
        /// </summary>
        readonly IDataEntryControl _owningControl;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DataEntryAttributeMemento"/> class.
        /// </summary>
        /// <param name="affectedAttribute">The affected attribute.</param>
        protected DataEntryAttributeMemento(IAttribute affectedAttribute)
        {
            _affectedAttribute = affectedAttribute;
            _attributesToRefresh.Add(affectedAttribute);
            _owningControl = AttributeStatusInfo.GetOwningControl(affectedAttribute);
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// The <see cref="IAttribute"/> involved in the change.
        /// </summary>
        /// <value>The <see cref="IAttribute"/> involved in the change.</value>
        protected IAttribute AffectedAttribute
        {
            get
            {
                return _affectedAttribute;
            }

            set
            {
                _affectedAttribute = value;
            }
        }

        /// <summary>
        /// A set of <see cref="IAttribute"/>s that need to be refreshed at the end of the undo
        /// operation this memento is a part of.
        /// </summary>
        /// <value>The <see cref="IAttribute"/>s to refresh.</value>
        protected HashSet<IAttribute> AttributesToRefresh
        {
            get
            {
                return _attributesToRefresh;
            }
        }

        /// <summary>
        /// Indicates whether the entire <see cref="IDataEntryControl"/> associated with this
        /// <see cref="IAttribute"/> needs to be refreshed after the undo operation this memento
        /// is a part of.
        /// </summary>
        /// <value>
        /// <see langword="true"/> to refresh the entire control; <see langword="false"/> to
        /// refresh only the <see cref="IAttribute"/>s in <see cref="AttributesToRefresh"/>.
        /// </value>
        protected bool RefreshEntireControl
        {
            get
            {
                return _refreshEntireControl;
            }

            set
            {
                _refreshEntireControl = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this change is the first in the current
        /// operation for _owningControl (meaning it will be the last to be un-done).
        /// </summary>
        /// <value>
        /// <see langword="true"/> if it is the first <see cref="DataEntryAttributeMemento"/> for
        /// <see cref="OwningControl"/>; otherwise, <see langword="false"/>.
        /// </value>
        protected bool FirstMementoForControl
        {
            get
            {
                return _firstMementoForControl;
            }

            set
            {
                _firstMementoForControl = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="IDataEntryControl"/> associated with the AffectedAttribute.
        /// </summary>
        /// <value>The <see cref="IDataEntryControl"/> associated with the AffectedAttribute.
        /// </value>
        protected IDataEntryControl OwningControl
        {
            get
            {
                return _owningControl;
            }
        }

        /// <summary>
        /// Gets the <see cref="UndoMementoSignificance"/> of the change.
        /// </summary>
        /// <value>The <see cref="UndoMementoSignificance"/> of the change.</value>
        public virtual UndoMementoSignificance Significance
        {
            get
            {
                // Be default, any DataEntryAttributeMementos will be considered worthy of being an
                // independent operation.
                return UndoMementoSignificance.Substantial;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Determines whether a new memento provides information not contained or replaced by 
        /// the information in this memento.
        /// </summary>
        /// <param name="memento">The <see cref="IUndoMemento"/> proposed to be added to the current
        /// operation.</param>
        /// <returns>
        /// <see langword="true"/> if this memento supersedes <see paramref="memento"/> and
        /// it should therefore be disregarded; <see langword="false"/> to allow the memento to be
        /// added.
        /// </returns>
        public virtual bool Supersedes(IUndoMemento memento)
        {
            try
            {
                // Determines which memento is the first DataEntryAttributeMemento associated with
                // this control and determine to what extent the control needs to be refreshed.
                // This allows the control to be refreshed in Undo after all changes have been made
                // to the control's attributes.
                var dataEntryAttributeMemento = memento as DataEntryAttributeMemento;
                if (dataEntryAttributeMemento != null &&
                    dataEntryAttributeMemento.OwningControl == OwningControl)
                {
                    dataEntryAttributeMemento.FirstMementoForControl = false;

                    RefreshEntireControl |= dataEntryAttributeMemento.RefreshEntireControl;
                    if (!RefreshEntireControl)
                    {
                        AttributesToRefresh.Add(dataEntryAttributeMemento.AffectedAttribute);
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31015", ex);
            }
        }

        /// <summary>
        /// Reverts the change represented by this memento.
        /// </summary>
        public virtual void Undo()
        {
            try
            {
                // If this is the first memento for this control (and thus the last change to be
                // un-done), refresh the control as appropriate.
                if (FirstMementoForControl)
                {
                    if (RefreshEntireControl)
                    {
                        try
                        {
                            // Disable validation while the control is being refreshed to avoid
                            // unnecessary validation state changing until the refresh is complete.
                            AttributeStatusInfo.EnableValidationTriggers(false);

                            OwningControl.RefreshAttributes();
                        }
                        finally
                        {
                            AttributeStatusInfo.EnableValidationTriggers(true);
                        }
                    }
                    else
                    {
                        OwningControl.RefreshAttributes(true, AttributesToRefresh.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31016", ex);
            }
        }

        #endregion Methods
    }

    /// <summary>
    /// Represents a revertible creation of an <see cref="IAttribute"/> in the data entry framework.
    /// </summary>
    internal class DataEntryAddedAttributeMemento : DataEntryAttributeMemento
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataEntryAddedAttributeMemento"/> class.
        /// </summary>
        /// <param name="affectedAttribute">The affected attribute.</param>
        public DataEntryAddedAttributeMemento(IAttribute affectedAttribute)
            : base(affectedAttribute)
        {
            RefreshEntireControl = true;
        }

        /// <summary>
        /// Reverts the change represented by this memento.
        /// </summary>
        public override void Undo()
        {
            AttributeStatusInfo.DeleteAttribute(AffectedAttribute);

            base.Undo();
        }
    }

    /// <summary>
    /// Represents a revertible status change of an <see cref="IAttribute"/> in the data entry
    /// framework.
    /// </summary>
    internal class DataEntryAttributeStatusChangeMemento : DataEntryAttributeMemento
    {
        /// <summary>
        /// Indicates whether the AffectedAttribute had been viewed prior to the change.
        /// </summary>
        bool _hasBeenViewed;

        /// <summary>
        /// Indicates whether the AffectedAttribute had been accepted prior to the change.
        /// </summary>
        bool _isAccepted;

        /// <summary>
        /// Indicates whether the AffectedAttribute had been displaying hints prior
        /// to the change.
        /// </summary>
        bool _hintEnabled;

        /// <summary>
        /// Indicates the <see cref="HintType"/> of the AffectedAttribute prior to the change.
        /// </summary>
        HintType _hintType;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataEntryAttributeStatusChangeMemento"/>
        /// class.
        /// </summary>
        /// <param name="affectedAttribute">The affected attribute.</param>
        public DataEntryAttributeStatusChangeMemento(IAttribute affectedAttribute)
            : base(affectedAttribute)
        {
            _hasBeenViewed = AttributeStatusInfo.HasBeenViewed(affectedAttribute, false);
            _isAccepted = AttributeStatusInfo.IsAccepted(affectedAttribute);
            _hintEnabled = AttributeStatusInfo.HintEnabled(affectedAttribute);
            _hintType = AttributeStatusInfo.GetHintType(affectedAttribute);

            AttributesToRefresh.Add(affectedAttribute);
        }

        /// <summary>
        /// Gets the <see cref="UndoMementoSignificance"/> of the change.
        /// </summary>
        /// <value>The <see cref="UndoMementoSignificance"/> of the change.</value>
        public override UndoMementoSignificance Significance
        {
            get
            {
                // The only status change that will occur independently from a value modification
                // is the viewed status... allow consecutive viewed status changes to be lumped
                // together as a single operation.
                return UndoMementoSignificance.Minor;
            }
        }

        /// <summary>
        /// Determines whether a new memento provides information not contained or replaced by
        /// the information in this memento.
        /// </summary>
        /// <param name="memento">The <see cref="IUndoMemento"/> proposed to be added to the current
        /// operation.</param>
        /// <returns>
        /// <see langword="true"/> if this memento supersedes <see paramref="memento"/> and
        /// it should therefore be disregarded; <see langword="false"/> to allow the memento to be
        /// added.
        /// </returns>
        public override bool Supersedes(IUndoMemento memento)
        {
            try
            {
                var attributeStatusChangeMemento = memento as DataEntryAttributeStatusChangeMemento;
                if (attributeStatusChangeMemento != null &&
                    AffectedAttribute == attributeStatusChangeMemento.AffectedAttribute)
                {
                    return true;
                }

                return base.Supersedes(memento);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31017", ex);
            }
        }

        /// <summary>
        /// Reverts the change represented by this memento.
        /// </summary>
        public override void Undo()
        {
            try
            {
                AttributeStatusInfo.MarkAsViewed(AffectedAttribute, _hasBeenViewed);
                AttributeStatusInfo.AcceptValue(AffectedAttribute, _isAccepted);
                AttributeStatusInfo.EnableHint(AffectedAttribute, _hintEnabled);
                AttributeStatusInfo.SetHintType(AffectedAttribute, _hintType);

                base.Undo();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31019", ex);
            }
        }
    }

    /// <summary>
    /// Represents a change in the value of an <see cref="IAttribute"/> in the data entry framework.
    /// </summary>
    internal class DataEntryModifiedAttributeMemento : DataEntryAttributeMemento
    {
        /// <summary>
        /// The original value of the <see cref="IAttribute"/>.
        /// </summary>
        SpatialString _originalValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataEntryModifiedAttributeMemento"/> class.
        /// </summary>
        /// <param name="affectedAttribute">The affected attribute.</param>
        public DataEntryModifiedAttributeMemento(IAttribute affectedAttribute)
            : base(affectedAttribute)
        {
            // Make a copy of the original value.
            ICopyableObject copySource = (ICopyableObject)affectedAttribute.Value;
            _originalValue = (SpatialString)copySource.Clone();

            AttributesToRefresh.Add(affectedAttribute);
        }

        /// <summary>
        /// Gets the original value of the <see cref="IAttribute"/>.
        /// </summary>
        /// <value>The original value of the <see cref="IAttribute"/>.</value>
        public SpatialString OriginalValue
        {
            get
            {
                return _originalValue;
            }
        }

        /// <summary>
        /// Determines whether a new memento provides information not contained or replaced by
        /// the information in this memento.
        /// </summary>
        /// <param name="memento">The <see cref="IUndoMemento"/> proposed to be added to the current
        /// operation.</param>
        /// <returns>
        /// <see langword="true"/> if this memento supersedes <see paramref="memento"/> and
        /// it should therefore be disregarded; <see langword="false"/> to allow the memento to be
        /// added.
        /// </returns>
        public override bool Supersedes(IUndoMemento memento)
        {
            try
            {
                var modifiedAttributeMemento = memento as DataEntryModifiedAttributeMemento;
                if (modifiedAttributeMemento != null &&
                    AffectedAttribute == modifiedAttributeMemento.AffectedAttribute)
                {
                    return true;
                }

                return base.Supersedes(memento);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31021", ex);
            }
        }

        /// <summary>
        /// Reverts the change represented by this memento.
        /// </summary>
        public override void Undo()
        {
            AttributeStatusInfo.SetValue(AffectedAttribute, _originalValue, false, false);

            base.Undo();
        }
    }

    /// <summary>
    /// Represents the deletion of an <see cref="IAttribute"/> in the data entry framework.
    /// </summary>
    internal class DataEntryDeletedAttributeMemento : DataEntryAttributeMemento, IDisposable
    {
        /// <summary>
        /// The <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s to which the
        /// AffectedAttribute belonged prior to the deletion.
        /// </summary>
        readonly IUnknownVector _parentCollection;

        /// <summary>
        /// The index the AffectedAttribute was at in <see cref="_parentCollection"/> prior to
        /// deletion.
        /// </summary>
        readonly int _index;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataEntryDeletedAttributeMemento"/> class.
        /// </summary>
        /// <param name="affectedAttribute">The affected attribute.</param>
        /// <param name="parentCollection">he <see cref="IUnknownVector"/> of
        /// <see cref="IAttribute"/>s to which the <see paramref="affectedAttribute"/> belonged prior
        /// to the deletion.</param>
        /// <param name="index">The index the <see paramref="affectedAttribute"/> was at in
        /// <see paramref="parentCollection"/> prior to deletion.</param>
        public DataEntryDeletedAttributeMemento(IAttribute affectedAttribute,
            IUnknownVector parentCollection, int index)
            : base(affectedAttribute)
        {
            _parentCollection = parentCollection;
            _index = index;
            RefreshEntireControl = true;
        }

        /// <summary>
        /// Reverts the change represented by this memento.
        /// </summary>
        public override void Undo()
        {
            try
            {
                _parentCollection.Insert(_index, AffectedAttribute);
                AttributeStatusInfo.Initialize(AffectedAttribute, _parentCollection, OwningControl);

                base.Undo();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31020", ex);
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="DataEntryDeletedAttributeMemento"/> by
        /// calling FinalReleaseComObject on the affected <see cref="IAttribute"/>.
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
                ExtractException.Display("ELI31001", ex);
            }
        }

        /// <overloads>Releases all resources used by the
        /// <see cref="DataEntryDeletedAttributeMemento"/> by calling FinalReleaseComObject on the
        /// affected <see cref="IAttribute"/>.</overloads>
        /// <summary>
        /// Releases all resources used by the <see cref="DataEntryDeletedAttributeMemento"/> by
        /// calling FinalReleaseComObject on the affected <see cref="IAttribute"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed objects
                if (AffectedAttribute != null)
                {
                    // [DataEntry:693]
                    // Since the attribute will no longer be accessed by the DataEntry, it needs to be
                    // released with FinalReleaseComObject to prevent handle leaks.
                    Marshal.FinalReleaseComObject(AffectedAttribute);
                    AffectedAttribute = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members
    }

    /// <summary>
    /// Represents a selection state change within the data entry framework.
    /// </summary>
    internal class DataEntrySelectionMemento : IUndoMemento
    {
        /// <summary>
        /// The <see cref="DataEntryControlHost"/> affected by the selection change.
        /// </summary>
        DataEntryControlHost _dataEntryControlHost;

        /// <summary>
        /// The <see cref="SelectionState"/> prior to the selection change.
        /// </summary>
        SelectionState _selectionState;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataEntrySelectionMemento"/> class.
        /// </summary>
        /// <param name="dataEntryControlHost">The <see cref="DataEntryControlHost"/> affected by
        /// the selection change.</param>
        /// <param name="selectionState">The <see cref="SelectionState"/> prior to the selection
        /// change.</param>
        public DataEntrySelectionMemento(DataEntryControlHost dataEntryControlHost,
            SelectionState selectionState)
        {
            _dataEntryControlHost = dataEntryControlHost;
            _selectionState = selectionState;
        }

        /// <summary>
        /// Gets the <see cref="UndoMementoSignificance"/> of the change.
        /// </summary>
        /// <value>The <see cref="UndoMementoSignificance"/> of the change.</value>
        public UndoMementoSignificance Significance
        {
            get
            {
                // Selection changes are only needed when they relate to data changes so that
                // selection can be reverted to what it was just prior to the data change.
                return UndoMementoSignificance.Supporting;
            }
        }

        /// <summary>
        /// Determines whether a new memento provides information not contained or replaced by
        /// the information in this memento.
        /// </summary>
        /// <param name="memento">The <see cref="IUndoMemento"/> proposed to be added to the current
        /// operation.</param>
        /// <returns>
        /// <see langword="true"/> if this memento supersedes <see paramref="memento"/> and
        /// it should therefore be disregarded; <see langword="false"/> to allow the memento to be
        /// added.
        /// </returns>
        public bool Supersedes(IUndoMemento memento)
        {
            try
            {
                var dataEntrySelectionMemento = memento as DataEntrySelectionMemento;
                if (dataEntrySelectionMemento != null &&
                    _selectionState.DataControl == dataEntrySelectionMemento._selectionState.DataControl)
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31022", ex);
            }
        }

        /// <summary>
        /// Reverts the change represented by this memento.
        /// </summary>
        public void Undo()
        {
            try
            {
                // Apply the reverted selection state via the message pump so that this is the last
                // thing that occurs as part of the undo procedure. Until all other data has been
                // reverted, the reverted selection state may not be valid.
                ((Control)_selectionState.DataControl).BeginInvoke((MethodInvoker)(() =>
                    _selectionState.DataControl.ApplySelection(_selectionState)));
                _dataEntryControlHost.BeginInvoke((MethodInvoker)(() =>
                    _dataEntryControlHost.ApplySelection(_selectionState)));
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31012", ex);
            }
        }
    }

    /// <summary>
    /// Represents a change in the active <see cref="IDataEntryControl"/>.
    /// </summary>
    internal class DataEntryActiveControlMemento : IUndoMemento
    {
        /// <summary>
        /// The <see cref="IDataEntryControl"/> that was active prior to the change.
        /// </summary>
        IDataEntryControl _dataControl;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataEntryActiveControlMemento"/> class.
        /// </summary>
        /// <param name="dataControl">The <see cref="IDataEntryControl"/> that was active prior to
        /// the change.</param>
        public DataEntryActiveControlMemento(IDataEntryControl dataControl)
        {
            _dataControl = dataControl;
        }

        /// <summary>
        /// Gets the <see cref="UndoMementoSignificance"/> of the change.
        /// </summary>
        /// <value>The <see cref="UndoMementoSignificance"/> of the change.</value>
        public UndoMementoSignificance Significance
        {
            get
            {
                // Active control changes are only needed when they relate to data changes so that
                // the active control can be reverted to what it was just prior to the data change.
                return UndoMementoSignificance.Supporting;
            }
        }

        /// <summary>
        /// Determines whether a new memento provides information not contained or replaced by
        /// the information in this memento.
        /// </summary>
        /// <param name="memento">The <see cref="IUndoMemento"/> proposed to be added to the current
        /// operation.</param>
        /// <returns>
        /// <see langword="true"/> if this memento supersedes <see paramref="memento"/> and
        /// it should therefore be disregarded; <see langword="false"/> to allow the memento to be
        /// added.
        /// </returns>
        public bool Supersedes(IUndoMemento memento)
        {
            return (memento is DataEntryActiveControlMemento);
        }

        /// <summary>
        /// Reverts the change represented by this memento.
        /// </summary>
        public void Undo()
        {
            try
            {
                ((Control)_dataControl).Focus();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31013", ex);
            }
        }
    }
}
