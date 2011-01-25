using Extract.Licensing;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Extract.DataEntry.DEP.Generic
{
    /// <summary>
    /// An extension of <see cref="FlowLayoutPanel"/> intended to make control sizing more
    /// intuitive. <see cref="FlowLayoutPanel"/> ignores certain anchor settings which may prevent
    /// controls from being automatically sized based on its parent.
    /// <see cref="AnchoredFlowLayoutPanel"/> makes use of all <see cref="Control.Anchor"/>
    /// settings to allow for auto-sizing controls.
    /// </summary>
    public partial class AnchoredFlowLayoutPanel : FlowLayoutPanel
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(AnchoredFlowLayoutPanel).ToString();

        #endregion Constants

        #region Column

        /// <summary>
        /// Describes an implicit column created by the <see cref="FlowLayoutPanel"/>. Each column
        /// is comprised of one or more <see cref="Control"/>s arranged in the direction of
        /// <see cref="FlowLayoutPanel.FlowDirection"/>.
        /// </summary>
        class Column
        {
            /// <summary>
            /// The column's controls.
            /// </summary>
            public List<Control> Controls = new List<Control>();

            /// <summary>
            /// <see langref="true"/> if the column is to be auto-sized, <see langref="false"/> if
            /// all controls are to maintain their present sizes.
            /// </summary>
            public bool AutoSize;

            /// <summary>
            /// The minimum number of pixels in thickness a column must be where thickness is
            /// horzontal when FlowDirection is vertical and vertical when FlowDirection is
            /// horizontal.
            /// </summary>
            public int MinThickness;

            /// <summary>
            /// The margin to use around the edges of the column.
            /// </summary>
            public Padding Margin = new Padding(0);
        }

        #endregion Column

        #region Structs

        /// <summary>
        /// A collection of layout settings for a control.
        /// </summary>
        struct ControlLayoutSettings
        {
            /// <summary>
            /// The minimum size of the control.
            /// </summary>
            public Size MinimumSize;

            /// <summary>
            /// The margins for the the control.
            /// </summary>
            public Padding Margin;
        }

        #endregion Structs

        #region Fields

        /// <summary>
        /// Specifies how the implicit columns should align themselves with borders of the
        /// <see cref="AnchoredFlowLayoutPanel"/>.
        /// </summary>
        AnchorStyles _internalAnchor =
            AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;

        /// <summary>
        /// The padding value that has been assigned to this control. May be temporarily overridden
        /// to achieve desired layout results.
        /// </summary>
        Padding _originalPadding;

        /// <summary>
        /// The layout values that have been assigned for all contained controls. May be
        /// temporarily overridden to achieve desired layout results.
        /// </summary>
        Dictionary<Control, ControlLayoutSettings> _originalLayoutSettings = 
            new Dictionary<Control, ControlLayoutSettings>();

        /// <summary>
        /// Indicates whether the control has been created. Modified layout code will not occur
        /// until the control has been created.
        /// </summary>
        bool _created = false;

        #endregion Fields

        #region Contructors

        /// <summary>
        /// Initializes a new <see cref="AnchoredFlowLayoutPanel"/> instance.
        /// </summary>
        public AnchoredFlowLayoutPanel()
            : base()
        {
            try
            {
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.DataEntryCoreComponents, "ELI30535", _OBJECT_NAME);

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30532", ex);
            }
        }

        #endregion Contructors

        #region Properties

        /// <summary>
        /// Specifies how the implicit columns should align themselves with borders of the
        /// <see cref="AnchoredFlowLayoutPanel"/>.
        /// </summary>
        [Category("Layout")]
        [DefaultValue(AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom)]
        public AnchorStyles InternalAnchor
        {
            get
            {
                return _internalAnchor;
            }

            set
            {
                try
                {
                    if (_internalAnchor != value)
                    {
                        _internalAnchor = value;

                        PerformLayout();
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI30534", ex);
                }
            }
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Raises the <see cref="Control.Layout"/> event.
        /// </summary>
        /// <param name="levent">A <see cref="LayoutEventArgs"/> that contains the event data.
        /// </param>
        protected override void OnLayout(LayoutEventArgs levent)
        {
            try
            {
                // If created, the layout will be modified. Record parameter values that may be
                // overridden and lock control updates so the base.OnLayout results aren't flashed
                // before the final layout results are ready.
                if (_created)
                {
                    _originalPadding = Padding;

                    foreach (Control control in Controls)
                    {
                        ControlLayoutSettings layoutSettings = new ControlLayoutSettings();
                        layoutSettings.MinimumSize = control.MinimumSize;
                        layoutSettings.Margin = control.Margin;
                        _originalLayoutSettings[control] = layoutSettings;
                    }

                    FormsMethods.LockControlUpdate(this, true, true);
                }

                // Allow the base class to perform the initial layout.
                base.OnLayout(levent);

                // Using the original layout as a base, adjust layout parameters as necessary so
                // that a second call to base.OnLayout achieves the desired layout.
                if (_created)
                {
                    AdjustLayoutParameters();

                    base.OnLayout(levent);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30531", ex);
            }
            finally
            {
                if (_created)
                {
                    // Unlock control updates to allow the panel and its controls to be drawn again.
                    FormsMethods.LockControlUpdate(this, false, true);

                    try
                    {
                        // Prevent restoration of layout parameters from triggering additional
                        // layouts.
                        SuspendLayout();

                        // Restore overriden layout parameters.
                        Padding = _originalPadding;

                        foreach (Control control in Controls)
                        {
                            ControlLayoutSettings layoutSettings = _originalLayoutSettings[control];
                            control.MinimumSize = layoutSettings.MinimumSize;
                            control.Margin = layoutSettings.Margin;
                        }
                    }
                    finally
                    {
                        ResumeLayout();
                    }
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.CreateControl"/> method.
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            _created = true;
        }

        #endregion Overrides

        #region Private Members

        /// <summary>
        /// Indicates whether <see cref="FlowLayoutPanel.FlowDirection"/> is horizontal.
        /// </summary>
        /// <see langword="true"/> if <see cref="FlowLayoutPanel.FlowDirection"/> is
        /// LeftToRight or RightToLeft, <see langword="false"/> otherwise.
        bool Horizontal
        {
            get
            {
                return (FlowDirection == FlowDirection.LeftToRight ||
                        FlowDirection == FlowDirection.RightToLeft);
            }
        }

        /// <summary>
        /// Returns the thickness of the provided <see paramref="size"/> value where thickness is
        /// defined as the dimension opposite of the <see cref="FlowLayoutPanel.FlowDirection"/>.
        /// </summary>
        /// <param name="size"></param>
        /// <returns>The thickness.</returns>
        int GetThickness(Size size)
        {
            return (Horizontal ? size.Height : size.Width);
        }

        /// <summary>
        /// Indicates whether the specified <see cref="AnchorStyles"/> settings should result in
        /// auto-sizing in the specified dimension.
        /// </summary>
        /// <param name="anchor">The <see cref="AnchorStyles"/> settings to test for auto-sizing.
        /// </param>
        /// <param name="horizontal"><see langword="true"/> if <paramref name="anchor"/> should be
        /// tested for horizontal sizing, <see langword="false"/> otherwise.</param>
        /// <returns></returns>
        static bool IsAutoSize(AnchorStyles anchor, bool horizontal)
        {
            AnchorStyles autoSizeFlags = (horizontal ? (AnchorStyles.Left | AnchorStyles.Right) :
                (AnchorStyles.Top | AnchorStyles.Bottom));

            return ((anchor & autoSizeFlags) == autoSizeFlags);
        }


        /// <summary>
        /// Gets the position of <see paramref="control"/> where position is defined as the edge
        /// of the control first encountered using the <see cref="FlowLayoutPanel.FlowDirection"/>.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> for which the position is needed.
        /// </param>
        /// <returns>The position of the <see cref="Control"/>.</returns>
        int GetPosition(Control control)
        {
            switch (FlowDirection)
            {
                case FlowDirection.LeftToRight: return control.Left;
                case FlowDirection.TopDown: return control.Top;
                case FlowDirection.RightToLeft: return control.Right;
                case FlowDirection.BottomUp: return control.Bottom;
            }

            return 0;
        }

        /// <summary>
        /// Adjusts layout parameters of the form and of child controls as necessary to cause an
        /// additional layout to create the desired layout.
        /// </summary>
        void AdjustLayoutParameters()
        {
            try
            {
                // Numerous changes to control layout parameters may occur in order to cause the
                // base class to layout the form in the expected fashion. Don't allow any of these
                // changes to trigger another layout until we are ready.
                SuspendLayout();

                // Identify the columns created by the initial base.OnLayout call.
                List<Column> columns = GetColumns();
                List<Column> autoSizeColumns = new List<Column>();

                // Determine how much thickness is available on the control for the columns to make
                // use of.
                int remainingThickness = GetThickness(Size);

                // Subtract the layout control's padding.
                remainingThickness -=
                    (Horizontal ? _originalPadding.Vertical : _originalPadding.Horizontal);

                foreach (Column column in columns)
                {
                    // Subtract the column margins
                    remainingThickness -=
                        Horizontal ? column.Margin.Vertical : column.Margin.Horizontal;

                    if (column.AutoSize)
                    {
                        autoSizeColumns.Add(column);
                    }
                    else
                    {
                        // Subtract the size of the fixed size columns
                        remainingThickness -= column.MinThickness;
                    }
                }

                // If there are any columns to be auto-sized, size the columns to take up the
                // remainingThickness on the form.
                if (autoSizeColumns.Count > 0)
                {
                    AutoSizeColumns(autoSizeColumns, remainingThickness);
                }
                // Otherwise, use padding to shift the controls so they take up the
                // remainingThickness on the form.
                else if (Horizontal &&
                        (_internalAnchor & AnchorStyles.Bottom) != 0 &&
                        (_internalAnchor & AnchorStyles.Top) == 0)
                {
                    Padding newPadding = Padding;
                    newPadding.Top = _originalPadding.Top + remainingThickness;
                    Padding = newPadding;
                }
                else if (!Horizontal &&
                        (_internalAnchor & AnchorStyles.Right) != 0 &&
                        (_internalAnchor & AnchorStyles.Left) == 0)
                {
                    Padding newPadding = Padding;
                    newPadding.Left = _originalPadding.Left + remainingThickness;
                    Padding = newPadding;
                }
            }
            catch (Exception ex)
            {
                ExtractException.AsExtractException("ELI30533", ex);
            }
            finally
            {
                // All layout parameters have been adjusted-- we can now allow layout
                // to be performed based on the new parameters
                ResumeLayout();
            }
        }

        /// <summary>
        /// Adjusts the thickness of the provided columns such that they use all of the
        /// <see paramref="remainingThickness"/> on the form.
        /// </summary>
        /// <param name="columns">The <see cref="Column"/>s to be auto-sized.</param>
        /// <param name="remainingThickness">The number of pixels available for the provided
        /// <see cref="Column"/>s to occupy.</param>
        void AutoSizeColumns(List<Column> columns, int remainingThickness)
        {
            int autoColumnThickness = 0;

            List<Column> autoSizeColumns = new List<Column>(columns);

            do
            {
                // Calculate the size for auto-size columns
                autoColumnThickness = remainingThickness / autoSizeColumns.Count;

                // Account for any auto-size columns whose minimum size is bigger than
                // autoColumnThickness. No longer treat these columns as auto-sized.
                List<Column> oversizedColumns = new List<Column>();
                foreach (Column column in autoSizeColumns)
                {
                    if (column.MinThickness > autoColumnThickness)
                    {
                        oversizedColumns.Add(column);
                    }
                }

                if (oversizedColumns.Count == 0)
                {
                    break;
                }

                foreach (Column column in oversizedColumns)
                {
                    autoSizeColumns.Remove(column);

                    // Adjust remainingThickness and autoColumnThickness to account for the
                    // oversized columns.
                    remainingThickness -= column.MinThickness;
                }

                // Repeat loop to ensure additional columns don't qualify as auto-sized using the
                // new autoColumnThickness value.
            }
            while (remainingThickness > 0);

            // For each column, adjust control properties so that the column assumes
            // autoColumnThickness and each control is positioned in the column correctly.
            foreach (Column column in columns)
            {
                int columnThickness = Math.Max(column.MinThickness, autoColumnThickness);

                // Identify the control to use to set the column thickness (other autosize controls
                // in the column will match this control in thickness.
                Control masterControl = null;
                foreach (Control control in column.Controls)
                {
                    if (IsAutoSize(control.Anchor, !Horizontal))
                    {
                        masterControl = control;
                        break;
                    }
                }

                if (Horizontal)
                {
                    masterControl.MinimumSize =
                        new Size(masterControl.MinimumSize.Width, columnThickness);
                }
                else
                {
                    masterControl.MinimumSize =
                        new Size(columnThickness, masterControl.MinimumSize.Height);
                }

                // If the column has only one control and that control is autosized in the thickness
                // dimension, size it to use the full thickness of the column.
                if (column.Controls.Count == 1 &&
                    IsAutoSize(masterControl.Anchor, Horizontal))
                {
                    if (Horizontal)
                    {
                        int maxWidth = Size.Width - Padding.Horizontal
                            - masterControl.Margin.Horizontal;
                        if (Size.Width > maxWidth && MinimumSize.Width < maxWidth)
                        {
                            masterControl.Width = maxWidth;
                        }
                    }
                    else
                    {
                        int maxHeight = Size.Height - Padding.Vertical
                            - masterControl.Margin.Vertical;
                        if (Size.Height > maxHeight && MinimumSize.Height < maxHeight)
                        {
                            masterControl.Height = maxHeight;
                        }
                    }
                }

                // Unify the margins between the columns to ensure the calculation of
                // autoColumnThickness is accurate.
                foreach (Control control in column.Controls)
                {
                    if (Horizontal)
                    {
                        control.Margin =
                            new Padding(control.Margin.Left, column.Margin.Top,
                                        control.Margin.Right, column.Margin.Bottom);
                    }
                    else
                    {
                        control.Margin =
                            new Padding(column.Margin.Left, control.Margin.Top,
                                        column.Margin.Right, control.Margin.Bottom);
                    }
                }
            }
        }

        /// <summary>
        /// Identifies the columns created by base.OnLayout.
        /// </summary>
        /// <returns>A list of the <see cref="Column"/>s.</returns>
        List<Column> GetColumns()
        {
            List<Column> columns = new List<Column>();
            Column currentColumn = null;
            int currentPosition = 0;

            foreach (Control control in Controls)
            {
                // Don't consider any hidden controls.
                if (!control.Visible)
                {
                    continue;
                }

                int position = GetPosition(control);

                // If a column hasn't yet been created or the positon of the control is even or
                // before that of the last control in the FlowDirection, this is a new column.
                if (currentColumn == null || position <= currentPosition)
                {
                    if (currentColumn != null)
                    {
                        currentColumn = null;
                    }

                    currentColumn = new Column();
                    columns.Add(currentColumn);
                }
                currentColumn.Controls.Add(control);

                currentPosition = position;

                // Determine whether to auto-size the column.
                bool isAutoSize = IsAutoSize(_internalAnchor, !Horizontal) &&
                                  IsAutoSize(control.Anchor, !Horizontal);
                currentColumn.AutoSize |= isAutoSize;

                // The thickness of the column is the minimum size for auto-size columns.
                int thickness =
                    (isAutoSize ? GetThickness(control.MinimumSize) : GetThickness(control.Size));
                if (thickness > currentColumn.MinThickness)
                {
                    currentColumn.MinThickness = thickness;
                }

                // The margin for the column will be that of the control with the largest margins.
                currentColumn.Margin =
                    new Padding(Math.Max(currentColumn.Margin.Left, control.Margin.Left),
                                Math.Max(currentColumn.Margin.Top, control.Margin.Top),
                                Math.Max(currentColumn.Margin.Right, control.Margin.Right),
                                Math.Max(currentColumn.Margin.Bottom, control.Margin.Bottom));
            }

            return columns;
        }

        #endregion Private Members
    }
}
