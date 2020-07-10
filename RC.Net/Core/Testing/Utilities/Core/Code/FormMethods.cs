using Extract.Utilities.Forms;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics.CodeAnalysis;

namespace Extract.Testing.Utilities
{
    /// <summary>
    /// A static class containing testing helper methods for retrieving controls from a form
    /// and displaying modeless instruction dialogs for interactive tests.
    /// </summary>
    public static class FormMethods
    {
        /// <overloads>Retrieves a component from the specified <see cref="Control"/>.</overloads>
        /// <summary>
        /// Retrieves the first component contained within a component of the 
        /// <see cref="Form"/> that is of the type <typeref name="TComponent"/>. 
        /// </summary>
        /// <typeparam name="TComponent">The type of the component to retrieve.</typeparam>
        /// <param name="control">The control within which to find the component.</param>
        /// <returns>The first component of <see cref="Form"/> that is of the type
        /// <typeref name="TComponent"/>. </returns>
        // FXCop does not like non-inferrable generics since it makes the library more "difficult"
        // to use.  Since this is just a helper class for use with our internal NUnit testing
        // we do not need to be concerned with how "difficult" it is to use this library.
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static TComponent GetFormComponent<TComponent>(Control control)
            where TComponent : Component
        {
            return GetFormComponent<TComponent>(control, null);
        }

        /// <summary>
        /// Retrieves the first component contained within a component of the
        /// <paramref name="control"/> that is of the type <typeref name="TComponent"/> and 
        /// with Text property of <paramref name="itemText"/>. 
        /// </summary>
        /// <typeparam name="TComponent">The type of the component to retrieve.</typeparam>
        /// <param name="control">The control within which to find the component.</param>
        /// <param name="itemText">The Text property of the component to retrieve.</param>
        /// <returns>The first component of <paramref name="control"/> that is of the type
        /// <typeref name="TComponent"/> with Text property of <paramref name="itemText"/>. 
        /// </returns>
        // FXCop does not like non-inferrable generics since it makes the library more "difficult"
        // to use.  Since this is just a helper class for use with our internal NUnit testing
        // we do not need to be concerned with how "difficult" it is to use this library.
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static TComponent GetFormComponent<TComponent>(Control control, string itemText)
            where TComponent : Component
        {
            try
            {
                // Check whether this control is the specified form component
                TComponent component = control as TComponent;
                if (component != null && (string.IsNullOrEmpty(itemText) || control.Name == itemText
                    || control.Text == itemText))
                {
                    return component;
                }

                // Check whether this is a toolstrip control
                ToolStrip toolStrip = control as ToolStrip;
                if (toolStrip != null)
                {
                    // Iterate through each tool strip item
                    foreach (ToolStripItem toolStripItem in toolStrip.Items)
                    {
                        // Check whether this toolstrip is the specified form component
                        component = toolStripItem as TComponent;
                        if (component != null && (itemText == null || toolStripItem.Text == itemText))
                        {
                            return component;
                        }
                    }
                }

                // Check whether this is a menustrip control
                MenuStrip menuStrip = control as MenuStrip;
                if (menuStrip != null)
                {
                    // Iterate through each tool strip item
                    foreach (ToolStripItem toolStripItem in menuStrip.Items)
                    {
                        // Recursively search for the component within this item and its children
                        component = GetFormComponent<TComponent>(toolStripItem, itemText);
                        if (component != null)
                        {
                            return component;
                        }
                    }
                }

                // Check each sub control of this control
                Control.ControlCollection controls = control.Controls;
                foreach (Control subcontrol in controls)
                {
                    // Recursively search for the component within this item and its children
                    component = GetFormComponent<TComponent>(subcontrol, itemText);
                    if (component != null)
                    {
                        return component;
                    }
                }

                // Recursively search for the component within the 
                // context menu strip associated with this control.
                if (control.ContextMenuStrip != null)
                {
                    component = GetFormComponent<TComponent>(control.ContextMenuStrip, itemText);
                }

                return itemText == null || control.Text == itemText ? component : null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32922");
            }
        }

        /// <summary>
        /// Retrieves the first component contained within a component of the 
        /// <paramref name="toolStripItem"/> that is of the type <typeref name="TComponent"/> 
        /// and with Text property of <paramref name="itemText"/>. 
        /// </summary>
        /// <typeparam name="TComponent">The type of the component to retrieve.</typeparam>
        /// <param name="toolStripItem">The tool strip item within which to find the component.
        /// <param name="itemText">The Text property of the component to retrieve.</param>
        /// </param>
        /// <returns>The first component of <paramref name="toolStripItem"/> that is of the type
        /// <typeref name="TComponent"/> with Text property of <paramref name="itemText"/>. 
        /// </returns>
        // FXCop does not like non-inferrable generics since it makes the library more "difficult"
        // to use.  Since this is just a helper class for use with our internal NUnit testing
        // we do not need to be concerned with how "difficult" it is to use this library.
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public static TComponent GetFormComponent<TComponent>(ToolStripItem toolStripItem, string itemText)
            where TComponent : Component
        {
            try
            {
                // Check whether this control is the specified component
                TComponent component = toolStripItem as TComponent;
                if (component != null && (itemText == null || toolStripItem.Text == itemText))
                {
                    return component;
                }

                // Check whether this is a tool strip menu item
                ToolStripMenuItem menuItem = toolStripItem as ToolStripMenuItem;
                if (menuItem != null)
                {
                    // Iterate through each child tool strip item
                    foreach (ToolStripItem childItem in menuItem.DropDownItems)
                    {
                        // Recursively search for the component within this item and its children
                        component = GetFormComponent<TComponent>(childItem, itemText);
                        if (component != null)
                        {
                            return component;
                        }
                    }
                }

                // The component was not found
                return null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32923");
            }
        }

        #region Methods

        /// <summary>
        /// Will show a modeless message box containing a list of instructions for performing
        /// the current interactive test.
        /// <para><b>Note:</b></para>
        /// This method will not return until the dialog box has closed.
        /// </summary>
        /// <param name="instructions">An array of <see cref="string"/> objects containing
        /// the instructions for performing the current test.</param>
        public static void ShowModelessInstructionsAndWait(string[] instructions)
        {
            try
            {
                // Create a StringBuilder to hold the message to display
                StringBuilder sb = new StringBuilder("Here are your instructions\n\n");

                // Build the instruction list
                int i = 1;
                foreach (string instruction in instructions)
                {
                    sb.Append(i.ToString(CultureInfo.CurrentCulture));
                    sb.Append(". ");
                    sb.Append(instruction);
                    sb.Append("\n");
                    i++;
                }

                // Add a polite ending
                sb.Append("\nThank you.");

                // Build and display the modeless dialog box
                using (CustomizableMessageBox messageBox = new CustomizableMessageBox())
                {
                    // Build the modeless dialog
                    messageBox.AddStandardButtons(MessageBoxButtons.OK);
                    messageBox.Caption = "Test instructions";
                    messageBox.StandardIcon = MessageBoxIcon.Information;
                    messageBox.Text = sb.ToString();

                    // Show the modeless dialog
                    messageBox.ShowModeless();

                    // Loop until the dialog has closed
                    while (messageBox.IsVisible)
                    {
                        // Process all messages in the queue
                        Application.DoEvents();
                        System.Threading.Thread.Sleep(100);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32924");
            }
        }

        /// <summary>
        /// Peforms a click of the specified <see cref="ToolStripItem"/> in a way that ensures all
        /// processing that may be triggered by an internal
        /// <see cref="Control.BeginInvoke(Delegate)"/> call happens before this call exists.
        /// </summary>
        /// <param name="control">The <see cref="Control"/> that should be used to invoke the click.
        /// </param>
        /// <param name="toolStripItem">The <see cref="ToolStripItem"/> to be clicked.</param>
        public static void PerformClick(this Control control, ToolStripItem toolStripItem)
        {
            try
            {
                control.Invoke((MethodInvoker)(() =>
                {
                    toolStripItem.PerformClick();
                    Application.DoEvents();
                }));
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36184");
            }
        }

        #endregion Methods
    }
}
