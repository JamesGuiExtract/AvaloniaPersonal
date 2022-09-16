using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Extract.DataEntry
{
    /// <summary>
    /// An extension of <see cref="DataGridViewColumn"/> that allows for Extract Systems data entry
    /// specific properties and behavior.
    /// </summary>
    public class DataEntryCheckBoxColumn : DataEntryTableColumnBase, ICheckBoxObject
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        protected override string ObjectName { get; } = typeof(DataEntryCheckBoxColumn).ToString();

        #endregion Constants

        private DataEntryCheckBoxCell DataEntryCellTemplate => (DataEntryCheckBoxCell)base.CellTemplate;

        #region ICheckBoxObject

        /// <inheritdoc/>
        [Category("Data Entry CheckBox Column")]
        [DefaultValue("True")]
        public string CheckedValue
        {
            get => (string)DataEntryCellTemplate.TrueValue;
            set => DataEntryCellTemplate.TrueValue = value;
        }

        /// <inheritdoc/>
        [Category("Data Entry CheckBox Column")]
        [DefaultValue("False")]
        public string UncheckedValue
        {
            get => (string)DataEntryCellTemplate.FalseValue;
            set => DataEntryCellTemplate.FalseValue = value;
        }

        /// <inheritdoc/>
        [Category("Data Entry CheckBox Column")]
        [DefaultValue(false)]
        public bool DefaultCheckedState
        {
            get => DataEntryCellTemplate.DefaultCheckedState;
            set => DataEntryCellTemplate.DefaultCheckedState = value;
        }

        #endregion ICheckBoxObject

        #region Overrides

        #region Constructors

        public DataEntryCheckBoxColumn() : base()
        {
            base.CellTemplate = new DataEntryCheckBoxCell();
        }

        #endregion Constructors

        /// <summary>
        /// Creates an exact copy of the <see cref="DataEntryCheckBoxColumn"/> instance.
        /// </summary>
        /// <returns>An exact copy of the <see cref="DataEntryCheckBoxColumn"/> instance.</returns>
        public override object Clone()
        {
            try
            {
                DataEntryCheckBoxColumn column = (DataEntryCheckBoxColumn)base.Clone();

                // Copy DataEntryCheckBoxColumn specific properties
                column.CheckedValue = CheckedValue;
                column.UncheckedValue = UncheckedValue;
                column.DefaultCheckedState = DefaultCheckedState;

                return column;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI53631", ex);
            }
        }

        /// <inheritdoc/>
        protected override void UpdateCellTemplate()
        {
            base.CellTemplate = new DataEntryCheckBoxCell()
            {
                ValidatorTemplate = (DataEntryValidator)ValidatorTemplate.Clone(),
                CheckedValue = CheckedValue,
                UncheckedValue = UncheckedValue,
                DefaultCheckedState = DefaultCheckedState
            };
        }

        /// <inheritdoc/>
        public override BackgroundFieldModel GetBackgroundFieldModel()
        {
            try
            {
                return new DataEntryCheckBoxBackgroundFieldModel()
                {
                    Name = AttributeName,
                    AutoUpdateQuery = AutoUpdateQuery,
                    ValidationQuery = ValidationQuery,
                    IsViewable = Visible && (DataGridView?.Visible ?? false),
                    PersistAttribute = PersistAttribute,
                    ValidationErrorMessage = ValidationErrorMessage,
                    ValidationPattern = ValidationPattern,
                    ValidationCorrectsCase = ValidationCorrectsCase,
                    CheckedValue = CheckedValue,
                    UncheckedValue = UncheckedValue,
                    DefaultCheckedState = DefaultCheckedState
                };
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53623");
            }
        }

        #endregion Overrides
    }
}
