using Extract.Licensing;
using System;
using System.ComponentModel;

namespace Extract.Utilities.ContextTags
{
    /// <summary>
    /// A <see cref="PropertyDescriptor"/> for describing the fields of
    /// <see cref="ContextTagsEditorViewRow"/>.
    /// </summary>
    public class ContextTagsEditorViewPropertyDescriptor : PropertyDescriptor
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(ContextTagsEditorViewPropertyDescriptor).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The Func used to retrieve the property's value.
        /// </summary>
        Func<ContextTagsEditorViewRow, object> _getFunc;

        /// <summary>
        /// The Action used to update the property's value.
        /// </summary>
        Action<ContextTagsEditorViewRow, object> _setAction;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextTagsEditorViewPropertyDescriptor"/>
        /// class.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="getFunc">The Func used to retrieve the property's value.</param>
        /// <param name="setAction">The Action used to update the property's value.</param>
        public ContextTagsEditorViewPropertyDescriptor(string name,
            Func<ContextTagsEditorViewRow, object> getFunc,
            Action<ContextTagsEditorViewRow, object> setAction)
            : base(name, null)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI37983",
                    _OBJECT_NAME);

                ExtractException.Assert("ELI37984", "Null argument exception", getFunc != null);
                ExtractException.Assert("ELI37985", "Null argument exception", setAction != null);

                _getFunc = getFunc;
                _setAction = setAction;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37986");
            }
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Gets the current value of the property on a component.
        /// </summary>
        /// <param name="component">The component with the property for which to retrieve the value.
        /// </param>
        /// <returns>
        /// The value of a property for a given component.
        /// </returns>
        public override object GetValue(object component)
        {
            try
            {
                return (_getFunc == null) ? null : _getFunc((ContextTagsEditorViewRow)component);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37987");
            }
        }

        /// <summary>
        /// Sets the value of the component to a different value.
        /// </summary>
        /// <param name="component">The component with the property value that is to be set.</param>
        /// <param name="value">The new value.</param>
        public override void SetValue(object component, object value)
        {
            try
            {
                if (_setAction != null)
                {
                    _setAction((ContextTagsEditorViewRow)component, value);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37988");
            }
        }

        /// <summary>
        /// Resets the value for this property of the component to the default value.
        /// </summary>
        /// <param name="component">The component with the property value that is to be reset to the
        /// default value.</param>
        public override void ResetValue(object component)
        {
            // Nothing to do (CanResetValue == false)
        }

        /// <summary>
        /// Whether resetting an object changes its  value.
        /// </summary>
        /// <param name="component">The component to test for reset capability.</param>
        /// <returns>
        /// <see langword="true"/> if resetting the component changes its value; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public override bool CanResetValue(object component)
        {
            return false;
        }

        /// <summary>
        /// Determines a value indicating whether the value of this property needs to be persisted.
        /// </summary>
        /// <param name="component">The component with the property to be examined for persistence.</param>
        /// <returns>
        /// <see langword="true"/> if the property should be persisted; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        /// <summary>
        /// Gets the type of the property.
        /// </summary>
        /// <returns>A <see cref="T:System.Type"/> that represents the type of the property.</returns>
        public override Type PropertyType
        {
            get
            {
                // ContextTagsEditorViewPropertyDescriptor properties are always strings.
                return typeof(string);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this property is read-only.
        /// </summary>
        /// <returns><see langword="true"/> if the property is read-only; otherwise, 
        /// <see langword="false"/>.</returns>
        public override bool IsReadOnly
        {
            get 
            { 
                return false; 
            }
        }

        /// <summary>
        /// Gets the type of the component this property is bound to.
        /// </summary>
        /// <returns>A <see cref="T:System.Type"/> that represents the type of component this
        /// property is bound to. When the <see cref="M:GetValue(System.Object)"/> or 
        /// <see cref="M:SetValue(System.Object,System.Object)"/> methods are invoked, the object
        /// specified might be an instance of this type.</returns>
        public override Type ComponentType
        {
            get 
            { 
                return typeof(ContextTagsEditorViewRow); 
            }
        }

        #endregion Overrides
    }
}
