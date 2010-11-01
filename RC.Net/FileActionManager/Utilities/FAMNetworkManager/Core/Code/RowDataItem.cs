using System;

namespace Extract.FileActionManager.Utilities
{
    /// <summary>
    /// Class representing the data item that will be held in each
    /// <see cref="Extract.Utilities.Forms.BetterDataGridViewRow{T}"/>
    /// </summary>
    class RowDataItem : IDisposable
    {
        #region Fields

        /// <summary>
        /// The controller associated with this row.
        /// </summary>
        ServiceMachineController _controller;

        /// <summary>
        /// The exception currently associated with this row (may be
        /// <see langword="null"/>
        /// </summary>
        ExtractException _exception;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RowDataItem"/> class.
        /// </summary>
        /// <param name="controller">The controller.</param>
        public RowDataItem(ServiceMachineController controller)
            : this(controller, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RowDataItem"/> class.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="ee">The ee.</param>
        public RowDataItem(ServiceMachineController controller, ExtractException ee)
        {
            _controller = controller;
            _exception = ee;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the controller.
        /// </summary>
        /// <value>The controller.</value>
        public ServiceMachineController Controller
        {
            get
            {
                return _controller;
            }
        }

        /// <summary>
        /// Gets or sets the exception associated with this row.
        /// </summary>
        /// <value>The exception.</value>
        public ExtractException Exception
        {
            get
            {
                return _exception;
            }
            set
            {
                _exception = value;
            }
        }

        #endregion Properties

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_controller != null)
                {
                    _controller.Dispose();
                    _controller = null;
                }
            }
        }

        #endregion
    }
}
