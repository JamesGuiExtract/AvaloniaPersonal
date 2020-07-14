using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Extract.FileActionManager.FileProcessors
{
    public partial class PaginationShortcutsForm : Form
    {
        private Dictionary<string, string> ShortCuts = new Dictionary<string, string>() {
            {"Tab", "Data Entry Panel:" + 
                "\n    Tabbing will navigate the Data Entry Panel fields." + "" +
                "\n    If you tab through all of the fields, it will cycle through the pages." + 
                "\n    After cycling through the pages, it will mark the document to be commited and return you to the main view." +
                "\nMain View:" +
                "\n    Hitting tab will open the document's pages for viewing. Hitting tab again will open its Data Entry Panel"},
            {"Shift Tab", "Data Entry Panel:"+
                "\n    Will navigate the Data Entry Panel fields in reverse." + 
                "\n    If you reach the top of the Data Entry Panel, it will return you to the main view." +
                "\nMainView:" +
                "\n    Navigates to the previous document."},
            {"Insert", "Begins a new document on the selected page." },
            {"Delete", "Exclude page from pending output document." },
            {"Space", "Mark the selected document for submission (check the box)." },
            {"Arrow Keys", "Can be used to navigate documents in the main view, and some Data Entry Panel Fields." },
            {"Page Up", "Selects the previous page, or collapses the document."},
            {"Page Down", "Selects the next page, or opens up a document to view its pages." },
            {"CTRL + C", "Copies the selected page(s) or text." },
            {"CTRL + X", "Cuts the selected page(s) or text." },
            {"CTRL + V", "Pastes the page(s)/text in the clipboard." },
            {"CTRL + Z", "Will undo the last text based operation." },
            {"CTRL + P", "Opens the print dialog." },
            {"CTRL + R", "Rotates the selected document 90 degrees clockwise." },
            {"CTRL + SHIFT + R", "Rotates the selected document 90 counter clockwise." },
            {@"CTRL + ""+"" / F7", "Zoom in." },
            {@"CTRL + ""-"" / F8", "Zoom out." },
            {"ALT + W", "Fits the page to width." },
            {"ALT + P", "Fits the window to page." },
            {"ALT + Right Arrow", "Navigate to the next zoom level (if it exists)." },
            {"ALT + Left Arrow", "Returns to the previous zoom level." },
            {"ALT + A", "Will select the pan tool." },
            {"ALT + Z", "Will select the zoom tool." },
            {"CTRL (hold)", "Allows you to select multiple pages to move between documents." },
            {"SHIFT (hold)", "Allows you to select all the pages between the range you shift select." },
            {"Enter/F2", "Opens the Data Entry Panel for the selected document." },
            {"Shift + Delete", "Unmarks a page for deletion." },
            {"F10", "While in the Data Entry Panel, this will highlight all data in the image." },
            {"Escape", "Data Entry Panel:\n    This will close the data entry panel.\nMain View\n    This will collapse the current document if un-collapsed." },
        };

        public PaginationShortcutsForm()
        {
            InitializeComponent();
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.Controls.Add(new Label() { Text = "Key To Press", AutoSize = true, Font = new Font(Font, FontStyle.Bold) }, 0, 0);
            this.tableLayoutPanel1.Controls.Add(new Label() { Text = "Description Of Shortcut", AutoSize = true, Font = new Font(Font, FontStyle.Bold) }, 1, 0);
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
