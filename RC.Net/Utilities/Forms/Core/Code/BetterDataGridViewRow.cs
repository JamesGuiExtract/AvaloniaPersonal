using System;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// A class that allows storing an item associated with the <see cref="DataGridViewRow"/>.
    /// <para><b>Note:</b></para>
    /// By default this class assumes ownership of the data item associated with it. If this
    /// class is the owner of the item and the item implements <see cref="IDisposable"/>
    /// then this class will dispose of the item when <see cref="Dispose"/> is called.
    /// </summary>
    /// <typeparam name="T">The type of object to store in the row.</typeparam>
    public partial class BetterDataGridViewRow<T> : DataGridViewRow
        where T : class
    {
        #region Fields

        /// <summary>
        /// Flag to indicate whether this row owns the data item or not.
        /// If <see langword="true"/> then if the data item is disposable
        /// it will be disposed when this class is disposed or if a new
        /// data item is set for the class.
        /// </summary>
        bool _ownDataItem;

        /// <summary>
        /// The data item associated with the row.
        /// </summary>
        T _dataItem;

        /// <summary>
        /// Mutex object to provide thread safe access to the data item.
        /// </summary>
        object _lock = new object();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BetterDataGridViewRow&lt;T&gt;"/> class.
        /// </summary>
        public BetterDataGridViewRow()
            : this(true, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BetterDataGridViewRow&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="ownDataItem">if set to <see langword="true"/> then
        /// the data item will be disposed when this class is disposed or when it is
        /// modified via the <see cref="DataItem"/> property.</param>
        public BetterDataGridViewRow(bool ownDataItem)
            : this(ownDataItem, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BetterDataGridViewRow&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="dataItem">The data item.</param>
        public BetterDataGridViewRow(T dataItem)
            : this(true, dataItem)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BetterDataGridViewRow&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="ownDataItem">if set to <see langword="true"/> then
        /// the data item will be disposed when this class is disposed or when it is
        /// modified via the <see cref="DataItem"/> property.</param>
        /// <param name="dataItem">The data item.</param>
        public BetterDataGridViewRow(bool ownDataItem, T dataItem) : base()
        {
            InitializeComponent();

            _ownDataItem = ownDataItem;
            _dataItem = dataItem;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this row owns the data item associated with it.
        /// If the row owns the data item then it will dispose of the item when the row is disposed.
        /// If it does not own the data item then the owner of the row is responsible for managing
        /// the lifetime of the data item.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the row owns the data item; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool RowOwnsDataItem
        {
            get
            {
                return _ownDataItem;
            }
            set
            {
                _ownDataItem = value;
            }
        }

        /// <summary>
        /// Gets or sets the data item.
        /// </summary>
        /// <value>The data item.</value>
        public T DataItem
        {
            get
            {
                lock (_lock)
                {
                    return _dataItem;
                }
            }
            set
            {
                try
                {
                    lock (_lock)
                    {
                        if (!object.ReferenceEquals(_dataItem, value))
                        {
                            if (_ownDataItem)
                            {
                                IDisposable disposable = _dataItem as IDisposable;
                                if (disposable != null)
                                {
                                    disposable.Dispose();
                                }
                            }

                            _dataItem = value;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI30979", ex);
                }
            }
        }

        #endregion Properties
    }
}
