using System;
using System.Windows.Forms;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// A <see cref="PaginationControl"/> that can be traversed with keyboard navigation controls
    /// and that will dynamically update its padding to allow room for a preceding
    /// <see cref="PaginationSeparator"/> to be inserted without having to be moved.
    /// </summary>
    internal partial class NavigablePaginationControl : PaginationControl
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigablePaginationControl"/> class.
        /// </summary>
        public NavigablePaginationControl()
            : base()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35620");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the <see cref="PageLayoutControl"/> this instance is currently loaded into.
        /// </summary>
        public PageLayoutControl PageLayoutControl
        {
            get
            {
                try
                {
                    for (Control control = Parent; Parent != null; control = control.Parent)
                    {
                        var pageLayoutControl = control as PageLayoutControl;
                        if (pageLayoutControl != null)
                        {
                            return pageLayoutControl;
                        }
                    }

                    return null;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI39658");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is highlighted.
        /// </summary>
        /// <value><see langword="true"/> if this instance is highlighted; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public virtual bool Highlighted
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the normal <see cref="Padding"/> that should be used by this instance.
        /// </summary>
        /// <value>The normal <see cref="Padding"/> that should be used by this instance.
        /// </value>
        public virtual Padding NormalPadding
        {
            get
            {
                return DefaultPadding;
            }
        }

        #endregion Properties
    }
}
