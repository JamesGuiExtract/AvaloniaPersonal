using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Xml;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Represents the saved user interface state of a <see cref="Form"/>.
    /// </summary>
    public class FormMemento : IControlMemento<Form>
    {
        #region Fields

        /// <summary>
        /// The size and location of the form in screen pixels when it is in its normal 
        /// (unmaximized) state.
        /// </summary>
        readonly Rectangle _bounds;

        /// <summary>
        /// The form's window state.
        /// </summary>
        readonly FormWindowState _state;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FormMemento"/> class.
        /// </summary>
        /// <param name="control">The form whose state should be saved.</param>
        public FormMemento(Form control)
            : this(GetBounds(control), control.WindowState)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormMemento"/> class.
        /// </summary>
        FormMemento(Rectangle bounds, FormWindowState state)
        {
            _bounds = bounds;
            _state = state == FormWindowState.Minimized ? FormWindowState.Normal : state;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Gets the bounds of the specified form when it is in its normal (unmaximized) state.
        /// </summary>
        /// <param name="form">The form from which the bounds should be retrieved.</param>
        /// <returns>The bounds of the <paramref name="form"/> when it is in its normal 
        /// (unmaximized) state.</returns>
        static Rectangle GetBounds(Form form)
        {
            return form.WindowState == FormWindowState.Normal ? form.Bounds : form.RestoreBounds;
        }
       
        /// <overloads>
        /// Gets the bounds from the specified object.
        /// </overloads>
        /// <summary>
        /// Gets the bounds from the specified XML element.
        /// </summary>
        /// <param name="element">The element from which bounds should be retrieved.</param>
        /// <returns>The bounds from the specified XML <paramref name="element"/>.</returns>
        static Rectangle GetBounds(XmlElement element)
        {
            int x = GetInt32AttributeByName(element, "X");
            int y = GetInt32AttributeByName(element, "Y");
            int width = GetInt32AttributeByName(element, "Width");
            int height = GetInt32AttributeByName(element, "Height");

            return new Rectangle(x, y, width, height);
        }

        /// <summary>
        /// Creates an <see cref="FormMemento"/> from the specified XML element.
        /// </summary>
        /// <param name="element">The XML element from which the <see cref="FormMemento"/> 
        /// should be created.</param>
        /// <returns>An <see cref="FormMemento"/> from the specified XML 
        /// <paramref name="element"/>.</returns>
        // The concrete type is required in this case
        [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes",
            MessageId = "System.Xml.XmlNode")]
        public static FormMemento FromXmlElement(XmlElement element)
        {
            try
            {
                if (element.Name != "Form")
                {
                    throw new ExtractException("ELI28954",
                        "Invalid XML.");
                }

                Rectangle bounds = GetBounds(element);
                FormWindowState state = GetFormWindowState(element);

                return new FormMemento(bounds, state);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28961", ex);
            }
        }

        /// <summary>
        /// The XML attribute with the specified name as an <see cref="Int32"/> value.
        /// </summary>
        /// <param name="element">The XML element with the named attribute.</param>
        /// <param name="name">The name of the XML attribute to retrieve.</param>
        /// <returns>The XML attribute with the specified <paramref name="name"/> as an 
        /// <see cref="Int32"/> value.</returns>
        static int GetInt32AttributeByName(XmlElement element, string name)
        {
            return int.Parse(element.GetAttribute(name), CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Gets the window state (e.g. Maximized) from the specified XML element.
        /// </summary>
        /// <param name="element">The element from which to retrieve the window state.</param>
        /// <returns>The window state (e.g. Maximized) from the XML <paramref name="element"/>.
        /// </returns>
        static FormWindowState GetFormWindowState(XmlElement element)
        {
            string state = element.GetAttribute("State");

            return (FormWindowState)Enum.Parse(typeof(FormWindowState), state);
        }

        /// <summary>
        /// Determines whether the specified rectangle intersects with the screen.
        /// </summary>
        /// <param name="rectangle">The rectangle to test in screen coordinates.</param>
        /// <returns><see langword="true"/> if any part of the <paramref name="rectangle"/> would 
        /// appear on the screen; <see langword="false"/> if the <paramref name="rectangle"/> is 
        /// completely off-screen.</returns>
        static bool IntersectsWithScreen(Rectangle rectangle)
        {
            try
            {
                foreach (Screen screen in Screen.AllScreens)
                {
                    if (screen.Bounds.IntersectsWith(rectangle))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28962", ex);
            }
        }

        /// <summary>
        /// Assigns an attribute with the specified name and value to the specified element.
        /// </summary>
        /// <param name="element">The XML element to which the attribute is assigned.</param>
        /// <param name="name">The name of the attribute to assign.</param>
        /// <param name="value">The value of the attribute to assign.</param>
        static void SetAttribute(XmlElement element, string name, IConvertible value)
        {
            element.SetAttribute(name, value.ToString(CultureInfo.InvariantCulture));
        }

        #endregion Methods

        #region IControlMemento<Form> Members

        /// <summary>
        /// Restores the state of the user interface of the specified control.
        /// </summary>
        /// <param name="control">The control whose state will be restored.</param>
        public void Restore(Form control)
        {
            try
            {
                control.StartPosition = FormStartPosition.Manual;

                // Check if the form is on-screen
                if (IntersectsWithScreen(_bounds))
                {
                    control.Bounds = _bounds;
                }
                else
                {
                    // The form is off-screen, move it on screen
                    control.Location = Point.Empty;
                    control.Size = _bounds.Size;
                }

                control.WindowState = _state;
            }
            catch (Exception ex)
            {
                ExtractException.AsExtractException("ELI28963", ex);
            }
        }

        #endregion IControlMemento<Form> Members

        #region IXmlConvertible

        /// <summary>
        /// Creates an XML element that represents the instance of this object.
        /// </summary>
        /// <param name="document">The XML document to use when creating the XML element.</param>
        /// <returns>An XML element the represents the instance of this object.</returns>
        [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes",
            MessageId="System.Xml.XmlNode")]
        public XmlElement ToXmlElement(XmlDocument document)
        {
            try
            {
                XmlElement element = document.CreateElement("Form");
                SetAttribute(element, "X", _bounds.X);
                SetAttribute(element, "Y", _bounds.Y);
                SetAttribute(element, "Width", _bounds.Width);
                SetAttribute(element, "Height", _bounds.Height);
                SetAttribute(element, "State", _state);

                return element;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28964", ex);
            }
        }
        
        #endregion IXmlConvertible
    }
}