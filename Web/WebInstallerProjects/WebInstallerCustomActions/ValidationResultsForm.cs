using Extract;
using Extract.Imaging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using WebInstallerCustomActions.Properties;

namespace WebInstallerCustomActions
{
    public partial class ValidationResultsForm : Form   
    {
        /// <summary>
        /// The validation result categories that can be indicated by this form
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public enum ValidationResult
        {
            Error,
            Warning,
            Valid
        }

        public ValidationResultsForm()
        {
            try
            {
                InitializeComponent();
                this._validationTable.AutoScroll= true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51528");
            }
        }

        public void AddHeading(string message)
        {
            try
            {
                var row = _validationTable.RowCount;
                _validationTable.RowCount++;
                _validationTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
                var label = new Label()
                {
                    Text = message,
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                    Font = new Font(Font, FontStyle.Bold),
                    AutoSize = true
                };
                _validationTable.Controls.Add(label, 0, row);
                _validationTable.SetColumnSpan(label, 2);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI51525");
            }
        }

        public void AddError(string message)
        {
            AddRow(_errorImage, message);
        }

        public void AddWarning(string message)
        {
            AddRow(_warningImage, message);
        }

        public void AddValid(string message)
        {
            AddRow(_validImage, message);
        }

        public void AddMessage(ValidationResult validationResult, string message)
        {
            try
            {
                switch (validationResult)
                {
                    case ValidationResult.Error: AddError(message); break;
                    case ValidationResult.Warning: AddWarning(message); break;
                    case ValidationResult.Valid: AddValid(message); break;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI51526");
            }
        }

        void AddRow(Image icon, string message)
        {
            try
            {
                var row = _validationTable.RowCount;
                _validationTable.RowCount++;
                _validationTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 18));
                _validationTable.Controls.Add(new PictureBox() { Image = icon }, 0, row);
                _validationTable.Controls.Add(new Label()
                {
                    Text = message,
                    TextAlign = ContentAlignment.MiddleLeft,
                    AutoSize = true
                }, 1, row);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI51527");
            }
        }

        Image _validImage = ImageMethods.ResizeHighQuality(Resources.Valid, 12, 12);
        Image _errorImage = ImageMethods.ResizeHighQuality(Resources.Error, 12, 12);
        Image _warningImage = ImageMethods.ResizeHighQuality(SystemIcons.Warning.ToBitmap(), 12, 12);
    }
}
