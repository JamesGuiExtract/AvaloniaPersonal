using System;
using System.Drawing;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// A <see cref="ToolStripTextBox"/> that expands to take up any extra room in the
    /// <see cref="ToolStrip"/> that hosts it. Based on the code provided by Microsoft here:
    /// http://msdn.microsoft.com/en-us/library/ms404304.aspx
    /// </summary>
    public class ToolStripSpringTextBox : ToolStripTextBox
    {
        /// <summary>
        /// Retrieves the size of a rectangular area into which a control can be fitted.
        /// </summary>
        /// <param name="constrainingSize">The custom-sized area for a control.</param>
        /// <returns>
        /// An ordered pair of type <see cref="T:System.Drawing.Size"/> representing the width and
        /// height of a rectangle.
        /// </returns>
        public override Size GetPreferredSize(Size constrainingSize)
        {
            // Retrieve the preferred size from the base class
            Size size = base.GetPreferredSize(constrainingSize);

            try
            {
                // Use the default size if the text box is on the overflow menu 
                // or is on a vertical ToolStrip. 
                if (IsOnOverflow || Owner.Orientation == Orientation.Vertical)
                {
                    return DefaultSize;
                }

                // Declare a variable to store the total available width as  
                // it is calculated, starting with the display width of the  
                // owning ToolStrip.
                Int32 width = Owner.DisplayRectangle.Width;

                // Subtract the width of the overflow button if it is displayed.  
                if (Owner.OverflowButton.Visible)
                {
                    width = width - Owner.OverflowButton.Width -
                        Owner.OverflowButton.Margin.Horizontal;
                }

                // Declare a variable to maintain a count of ToolStripSpringTextBox  
                // items currently displayed in the owning ToolStrip. 
                Int32 springBoxCount = 0;

                foreach (ToolStripItem item in Owner.Items)
                {
                    // Ignore items on the overflow menu. 
                    if (item.IsOnOverflow) continue;

                    if (item is ToolStripSpringTextBox)
                    {
                        // For ToolStripSpringTextBox items, increment the count and  
                        // subtract the margin width from the total available width.
                        springBoxCount++;
                        width -= item.Margin.Horizontal;
                    }
                    else
                    {
                        // For all other items, subtract the full width from the total 
                        // available width.
                        width = width - item.Width - item.Margin.Horizontal;
                    }
                }

                // If there are multiple ToolStripSpringTextBox items in the owning 
                // ToolStrip, divide the total available width between them.  
                if (springBoxCount > 1) width /= springBoxCount;

                // If the available width is less than the default width, use the 
                // default width, forcing one or more items onto the overflow menu. 
                if (width < DefaultSize.Width) width = DefaultSize.Width;

                // Change the width to the calculated width. 
                size.Width = width;
                return size;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35557");
            }

            return base.GetPreferredSize(constrainingSize);
        }
    }
}
