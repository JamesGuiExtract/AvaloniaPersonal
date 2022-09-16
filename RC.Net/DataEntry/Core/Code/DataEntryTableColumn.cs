using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Extract.DataEntry
{
    /// <summary>
    /// An extension of <see cref="DataGridViewColumn"/> that allows for Extract Systems data entry
    /// specific properties and behavior.
    /// </summary>
    public class DataEntryTableColumn : DataEntryTableColumnBase
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        protected override string ObjectName { get; } = typeof(DataEntryTableColumn).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// Specifies whether the cells in the column should be edited with a non-editable combo box.
        /// </summary>
        bool _useComboBoxCells;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Specifies whether the cells in the column should be edited with a non-editable combo box.
        /// </summary>
        /// <value><see langword="true"/> if a <see cref="DataEntryComboBoxCell"/> should be used to
        /// edit values in this column, <see langword="false"/> if values should be edited with a
        /// <see cref="DataEntryTextBoxCell"/>.</value>
        /// <returns><see langword="true"/> if a <see cref="DataEntryComboBoxCell"/> are used to
        /// edit values in this column, <see langword="false"/> if values are edited with a
        /// <see cref="DataEntryTextBoxCell"/>.</returns>
        [Category("Data Entry Table Column")]
        [DefaultValue(false)]
        public bool UseComboBoxCells
        {
            get
            {
                return _useComboBoxCells;
            }

            set
            {
                try
                {
                    if (_useComboBoxCells != value)
                    {
                        _useComboBoxCells = value;

                        UpdateCellTemplate();
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26513", ex);
                }
            }
        }

        #endregion Properties

        #region Constructors

        public DataEntryTableColumn() : base()
        {
            UpdateCellTemplate();
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Creates an exact copy of the <see cref="DataEntryTextBoxColumn"/> instance.
        /// </summary>
        /// <returns>An exact copy of the <see cref="DataEntryTextBoxColumn"/> instance.</returns>
        public override object Clone()
        {
            try
            {
                DataEntryTableColumn column = (DataEntryTableColumn)base.Clone();

                // Copy DataEntryTextBoxColumn specific properties
                column.UseComboBoxCells = UseComboBoxCells;

                return column;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24221", ex);
            }
        }
        /// <summary>
        /// Updates the cell template to reflect the current _useComboBoxCells and validation
        /// settings.
        /// </summary>
        protected override void UpdateCellTemplate()
        {
            // Use a combo box template or text box template depending on the value of 
            // _useComboBoxCells.
            if (_useComboBoxCells)
            {
                DataEntryComboBoxCell cellTemplate = new DataEntryComboBoxCell();
                cellTemplate.ValidatorTemplate = (DataEntryValidator)ValidatorTemplate.Clone();
                cellTemplate.RemoveNewLineChars = RemoveNewLineChars;

                string[] autoCompleteValues = ValidatorTemplate.GetAutoCompleteValues();
                if (autoCompleteValues != null)
                {
                    cellTemplate.Items.AddRange(autoCompleteValues);
                }

                base.CellTemplate = cellTemplate;
            }
            else
            {
                DataEntryTextBoxCell cellTemplate = new DataEntryTextBoxCell();
                cellTemplate.ValidatorTemplate = (DataEntryValidator)ValidatorTemplate.Clone();
                cellTemplate.RemoveNewLineChars = RemoveNewLineChars;

                base.CellTemplate = cellTemplate;
            }
        }

        /// <inheritdoc/>
        public override BackgroundFieldModel GetBackgroundFieldModel()
        {
            try
            {
                return new BackgroundFieldModel()
                {
                    Name = AttributeName,
                    AutoUpdateQuery = AutoUpdateQuery,
                    ValidationQuery = ValidationQuery,
                    IsViewable = Visible && (DataGridView?.Visible ?? false),
                    PersistAttribute = PersistAttribute,
                    ValidationErrorMessage = ValidationErrorMessage,
                    ValidationPattern = ValidationPattern,
                    ValidationCorrectsCase = ValidationCorrectsCase
                };
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45506");
            }
        }

        #endregion Overrides
    }
}
