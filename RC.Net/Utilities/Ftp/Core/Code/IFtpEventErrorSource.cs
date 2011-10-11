using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Extract.Utilities.Ftp
{
    /// <summary>
    /// Defines an object that provides notification when FTP operation errors occur.
    /// </summary>
    public interface IFtpEventErrorSource
    {
        /// <summary>
        /// Raised when an error occurs during an FTP operation.
        /// </summary>
        event EventHandler<ExtractExceptionEventArgs> FtpError;
    }
}
