using System;
using System.Collections.Generic;
using System.Text;

namespace Extract.ErrorHandling
{
    /// <summary>
    /// Base class for providing ExceptionEvent data 
    /// </summary>
    public class ExtractExceptionEventArgs : EventArgs
    {
        /// <summary>
        /// The exception that occurred during the OCR event.
        /// </summary>
        private ExtractException _exception;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractExceptionEventArgs"/> class.
        /// </summary>
        /// <param name="exception">The <see cref="ExtractException"/> that occurred.</param>
        public ExtractExceptionEventArgs(ExtractException exception)
        {
            try
            {
                _exception = exception;
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI22035",
                    "Failed to initialize ExceptionEventArgs!", ex);
            }
        }

        /// <summary>
        /// Gets the <see cref="ExtractException"/> that occurred.
        /// </summary>
        /// <returns>The <see cref="ExtractException"/> that occurred.</returns>
        public ExtractException Exception
        {
            get
            {
                return _exception;
            }
        }
    }
}
