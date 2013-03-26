using System;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// 
    /// </summary>
    internal partial class PaginationSeparator : PaginationControl
    {
        #region Constants

        /// <summary>
        /// 
        /// </summary>
        internal static readonly int _SEPARATOR_WIDTH = 11;

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationSeparator"/> class.
        /// </summary>
        public PaginationSeparator()
            : base()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35496");
            }
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.SizeChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnSizeChanged(EventArgs e)
        {
            try
            {
                base.OnSizeChanged(e);

                if (Width != _SEPARATOR_WIDTH)
                {
                    Width = _SEPARATOR_WIDTH;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35497");
            }
        }

        #endregion Overrides
    }
}
