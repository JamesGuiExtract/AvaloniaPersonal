using System;
using System.Windows.Forms;
using UCLID_AFCORELib;

namespace Extract.DataEntry
{
    /// <summary>
    /// Members required by the data entry framework for extensions of
    /// <see cref="DataGridViewCell"/>.
    /// </summary>
    public interface IDataEntryTableCell
    {
        /// <summary>
        /// Gets or sets the <see cref="IAttribute"/> whose value is associated with this cell.
        /// </summary>
        /// <value>The <see cref="IAttribute"/> whose value is associated with this cell.</value>
        /// <returns>The <see cref="IAttribute"/> whose value is associated with this cell.
        /// </returns>
        IAttribute Attribute { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DataEntryValidator"/> to be used to validate a
        /// cell's data.
        /// </summary>
        /// <value>The <see cref="DataEntryValidator"/> to be used to validate a cell's
        /// data.</value>
        /// <returns>The <see cref="DataEntryValidator"/> used to validate a cell's data.
        /// </returns>
        DataEntryValidator Validator { get; set; }

        /// <summary>
        /// Provides access to the object as an <see cref="DataGridViewCell"/>.
        /// </summary>
        /// <returns>The object as an <see cref="DataGridViewCell"/>.</returns>
        DataGridViewCell AsDataGridViewCell { get; }

        /// <summary>
        /// Raised when the spatial info associated with the cell's <see cref="IAttribute"/> has
        /// changed.
        /// </summary>
        event EventHandler<CellSpatialInfoChangedEventArgs> CellSpatialInfoChanged;
    }
}