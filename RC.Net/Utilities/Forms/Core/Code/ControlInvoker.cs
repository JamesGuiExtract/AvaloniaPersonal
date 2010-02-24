using System;
using System.Windows.Forms;
using Extract.Licensing;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// Represents a way to invoke methods on a <see cref="Control"/> and properly handle 
    /// exceptions.
    /// </summary>
    /// <see href="http://connect.microsoft.com/VisualStudio/feedback/details/266184/bug-in-control-threadmethodentry-invokemarshaledcallbacks"/>
    public class ControlInvoker
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(ControlInvoker).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="Control"/> on which methods will be invoked.
        /// </summary>
        readonly Control _control;

        /// <summary>
        /// <see langword="true"/> if a <see cref="Control.Invoke(Delegate)"/> call has been 
        /// made and exceptions should be stored and thrown later; <see langword="false"/> if the 
        /// calling thread is the current thread and exceptions should be thrown directly.
        /// </summary>
        bool _storeException;

        /// <summary>
        /// The exception being stored so that it may be thrown later from the calling thread.
        /// </summary>
        ExtractException _exception;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlInvoker"/> class.
        /// </summary>
        public ControlInvoker(Control control)
        {
            LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI29815",
                _OBJECT_NAME);

            ExtractException.Assert("ELI29814", "Control to invoke must be specified.",
                control != null);

            _control = control;
        }

        #endregion Constructors

        #region Methods

        /// <overloads>Executes a delegate on the thread that owns the control's underlying window 
        /// handle.</overloads>
        /// <summary>
        /// Executes the specified delegate, on the thread that owns the control's underlying 
        /// window handle, with the specified list of arguments.
        /// </summary>
        /// <param name="method">A delegate to a method that takes no parameters.</param>
        public void Invoke(Delegate method)
        {
            Invoke(method, null);
        }

        /// <summary>
        /// Executes the specified delegate, on the thread that owns the control's underlying 
        /// window handle, with the specified list of arguments.
        /// </summary>
        /// <param name="method">A delegate to a method that takes parameters of the same number 
        /// and type that are contained in the <paramref name="args"/> parameter.</param>
        /// <param name="args">An array of objects to pass as arguments to the specified method. 
        /// This parameter can be <see langword="null"/> if the method takes no arguments.</param>
        public void Invoke(Delegate method, params object[] args)
        {
            // Invoke the method, storing exceptions internal to the method
            try
            {
                _storeException = true;

                // Invoke the method
                _control.Invoke(method, args);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26590", ex);
            }
            finally
            {
                _storeException = false;
            }

            // Check if any exceptions were stored. If so, throw them.
            try
            {
                if (_exception != null)
                {
                    throw _exception;
                }
            }
            catch
            {
                _exception = null;
                throw;
            }
        }

        /// <summary>
        /// Throws or stores an exception depending on the calling thread.
        /// </summary>
        /// <param name="ex">The exception to throw or store.</param>
        public void HandleException(ExtractException ex)
        {
            if (_storeException)
            {
                _exception = ex;
            }
            else
            {
                throw ex;
            }
        }

        #endregion Methods
    }
}
