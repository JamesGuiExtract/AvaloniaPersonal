using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Extract.Reporting
{
    /// <summary>
    /// A form for entering report parameters that will dynamically add the appropriate
    /// controls to the forms area.
    /// </summary>
    public partial class ParameterEntryForm : Form
    {
        #region Constants

        /// <summary>
        /// The maximum height for the form.
        /// </summary>
        private static readonly int _MAX_FORM_HEIGHT = 700;

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME = typeof(ParameterEntryForm).ToString();

        #endregion Constants

        #region Constructors

        /// <overloads>Initializes a new <see cref="ParameterEntryForm"/> class.</overloads>
        /// <summary>
        /// Initializes a new <see cref="ParameterEntryForm"/> class.
        /// </summary>
        public ParameterEntryForm() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="ParameterEntryForm"/> class.
        /// </summary>
        /// <param name="parameters">A collection of parameters to add entry controls for.</param>
        public ParameterEntryForm(IEnumerable<IExtractReportParameter> parameters)
        {
            try
            {
                // Load licenses if in design mode
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23821",
					_OBJECT_NAME);

                // Display wait cursor while the form is configured
                using (Extract.Utilities.Forms.TemporaryWaitCursor waitCursor
                    = new Extract.Utilities.Forms.TemporaryWaitCursor())
                {
                    InitializeComponent();

                    // Ensure the parameters collection is not null
                    if (parameters != null)
                    {
                        AddParameterControls(parameters);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23744", ex);
            }
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Adds entry controls for each of the parameters in the specified collection.
        /// </summary>
        /// <param name="parameters">The collection of parameters to add controls
        /// for.  Must not be <see langword="null"/>.</param>
        private void AddParameterControls(IEnumerable<IExtractReportParameter> parameters)
        {
            ExtractException.Assert("ELI23745", "Parameter collection cannot be null!",
                parameters != null);

            // Get the location for the first control
            Point startLocation = _topPanel.DisplayRectangle.Location;
            startLocation.Offset(_topPanel.Margin.Left, _topPanel.Margin.Top);

            // Set the next location to the start location
            Point nextControlLocation = startLocation;

            // Loop through all of the objects and add associated controls to the panel
            foreach (IExtractReportParameter parameter in parameters)
            {
                // Check for each parameter type and create associated controls
                TextParameter textParameter = parameter as TextParameter;
                if (textParameter != null)
                {
                    TextParameterControl control = new TextParameterControl(textParameter);
                    AddControlToPanel(control, ref nextControlLocation);
                    continue;
                }

                NumberParameter numberParameter = parameter as NumberParameter;
                if (numberParameter != null)
                {
                    NumberParameterControl control = new NumberParameterControl(numberParameter);
                    AddControlToPanel(control, ref nextControlLocation);
                    continue;
                }

                DateParameter dateParameter = parameter as DateParameter;
                if (dateParameter != null)
                {
                    DateParameterControl control = new DateParameterControl(dateParameter);
                    AddControlToPanel(control, ref nextControlLocation);
                    continue;
                }

                DateRangeParameter dateRangeParameter = parameter as DateRangeParameter;
                if (dateRangeParameter != null)
                {
                    DateRangeParameterControl control =
                        new DateRangeParameterControl(dateRangeParameter);
                    AddControlToPanel(control, ref nextControlLocation);
                    continue;
                }

                ValueListParameter valueListParameter = parameter as ValueListParameter;
                if (valueListParameter != null)
                {
                    Control valueListControl;
                    if (valueListParameter.MultipleSelect)
                    {
                        valueListControl = new MultipleSelectValueListParameterControl(valueListParameter);
                    }
                    else
                    {
                        valueListControl =
                            new ValueListParameterControl(valueListParameter);
                    }
                    AddControlToPanel(valueListControl, ref nextControlLocation);
                    continue;
                }

                // If code reached this point then the parameter type is unrecognized,
                // throw an exception
                ExtractException ee = new ExtractException("ELI23746",
                    "Unrecognized parameter type!");
                ee.AddDebugData("Parameter name", parameter.ParameterName, false);
                ee.AddDebugData("Parameter Type", parameter.GetType().ToString(), false);
                throw ee;
            }

            // Compute new height
            int newHeight = this.Height + (nextControlLocation.Y - startLocation.Y);

            // Adjust the size of the form accordingly
            if (newHeight < _MAX_FORM_HEIGHT)
            {
                this.Height = newHeight;
            }
            else
            {
                // If reached max height then the scroll bar will show up, need to make room
                // for it
                this.Width = this.Width + SystemInformation.VerticalScrollBarWidth;
                this.Height = _MAX_FORM_HEIGHT;
            }
        }

        /// <summary>
        /// Adds the specified control to the top panel.  Also updates the
        /// <paramref name="nextControlLocation"/> to point to Location for next
        /// control.
        /// </summary>
        /// <param name="control">The control to add.</param>
        /// <param name="nextControlLocation">The location to add the control to.</param>
        private void AddControlToPanel(Control control, ref Point nextControlLocation)
        {
            ExtractException.Assert("ELI23747", "Control cannot be null!", control != null);

            // Set the controls location and and add it to the top panel
            control.Location = nextControlLocation;
            control.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            _topPanel.Controls.Add(control);

            // Update the next control location
            nextControlLocation.Offset(0, control.Height + control.Margin.Bottom);
        }

        #endregion Methods

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event for the form.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        private void HandleOkClicked(object sender, EventArgs e)
        {
            try
            {
                // Iterate through each control looking for IExtractParameterControls
                foreach (Control control in _topPanel.Controls)
                {
                    IExtractParameterControl parameterControl = control as IExtractParameterControl;
                    if (parameterControl != null)
                    {
                        try
                        {
                            parameterControl.Apply();
                        }
                        catch
                        {
                            // If an exception occurred on this control, give it focus
                            control.Focus();
                            throw;
                        }
                    }
                }

                // Set the dialog result to OK (this will cause the dialog to close)
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI23822", ex);
            }
        }

        #endregion Event Handlers
    }
}