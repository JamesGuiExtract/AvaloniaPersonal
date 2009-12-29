using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Defines a method to load the state of the user interface of a control.
    /// </summary>
    /// <typeparam name="TControl">The type of control whose state can be restored.</typeparam>
    public interface IControlMemento<TControl> : IXmlConvertible
        where TControl : Control
    {
        /// <summary>
        /// Restores the state of the user interface of the specified control.
        /// </summary>
        /// <param name="control">The control whose state will be restored.</param>
        void Restore(TControl control);
    }
}