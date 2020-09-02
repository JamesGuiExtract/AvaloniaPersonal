using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Extract.FileActionManager.FileProcessors
{
    public partial class PaginationShortcutsForm : Form
    {
        private Dictionary<string, string> ShortCuts = new Dictionary<string, string>() {
            {"Tab", "Advance forward through all data fields and pages in each document." +
                "\nAs navigation of each document is completed, collapse and select it for submission."},
            {"Shift + Tab", "Move backward through any open document's fields and pages, then to previous documents." },
            {"Insert", "Make the selected page the first page of a new document." },
            {"Delete", "Exclude page from being output." },
            {"Shift + Delete", "Include a page currently marked to be excluded from output." },
            {"Space", "Select the document for submission (check the box)." },
            {"Arrow Keys", "With a specific page selected, navigate to other document pages" +
                "\nWith a document selected, up/down to navigate to previous/next document " +
                "\nand left/right to collapse/expand the document pages"},
            {"Page Up", "Select the previous page."},
            {"Page Down", "Select the next page." },
            {"CTRL + C", "Copy the selected page(s) or text to the clipboard." },
            {"CTRL + X", "Cut the selected page(s) or text to the clipboard." },
            {"CTRL + V", "Paste the page(s)/text from the clipboard." },
            {"CTRL + Z", "Undo the last data panel operation." },
            {"CTRL + Y", "Redo the last data panel operation undone." },
            {"CTRL + P", "Open the print dialog." },
            {"CTRL + .", "Rotate the page displayed in the image viewer 90 degrees clockwise." },
            {"CTRL + SHIFT + .", "Rotate all selected page thumbnails 90 degrees clockwise." },
            {"CTRL + ,", "Rotate the page displayed in the image viewer 90 degrees counterclockwise." },
            {"CTRL + SHIFT + ,", "Rotate all selected page thumbnails 90 degrees counterclockwise." },
            {@"CTRL + ""+"" / F7", "Zoom in." },
            {@"CTRL + ""-"" / F8", "Zoom out." },
            {"ALT + W", "Fit the page to width." },
            {"ALT + P", "Fit the page to the window." },
            {"ALT + A", "Select the pan tool." },
            {"ALT + Z", "Select the zoom tool." },
            {"CTRL (hold)", "Select multiple individual pages or document." },
            {"SHIFT (hold)", "Select all the pages/documents between and including previous selection." },
            {"Enter/F2", "Open the data panel for the selected document." },
            {"F3", "Advance to the next validation error." },
            {"F10", "While in a data panel, highlight all data in the image." },
            {"Escape", "Cancel current edit or close the open data panel or collapse the selected document's pages." }
        };

        public PaginationShortcutsForm()
        {
            InitializeComponent();
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.Controls.Add(new Label() { Text = "Key", AutoSize = true, Font = new Font(Font, FontStyle.Bold) }, 0, 0);
            this.tableLayoutPanel1.Controls.Add(new Label() { Text = "Action", AutoSize = true, Font = new Font(Font, FontStyle.Bold) }, 1, 0);
            this.AddShortcutsToTable();
        }

        private void AddShortcutsToTable()
        {
            foreach (KeyValuePair<string,string> shortcut in this.ShortCuts)
            {
                this.tableLayoutPanel1.RowCount += 1;
                this.tableLayoutPanel1.Controls.Add(new Label() { Text = shortcut.Key, AutoSize = true }, 0, this.tableLayoutPanel1.RowCount - 1);
                this.tableLayoutPanel1.Controls.Add(new Label() { Text = shortcut.Value, AutoSize = true }, 1, this.tableLayoutPanel1.RowCount - 1);
            }
        }
    }
}
