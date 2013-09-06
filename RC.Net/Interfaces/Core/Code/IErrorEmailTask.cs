using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

// This assembly is reserved for the definition of interfaces and helper classes for those
// interfaces. To ensure these interfaces are accessible from all projects without circular
// dependency issues and to allow the assemblies definitions to be used in both 32 and 64 bit code,
// This assembly should have no dependencies on any other Extract projects.
namespace Extract.Interfaces
{
    /// <summary>
    /// Defines an object that can be configured to send an email in response to an error that is
    /// encountered.
    /// </summary>
    [ComVisible(true)]
    [Guid("35A93ACE-76FF-4AA2-975B-EA2C6900BC95")]
    public interface IErrorEmailTask
    {
        /// <summary>
        /// The email addresses(es) of the primary recipients of the email.
        /// </summary>
        string Recipient
        {
            get;
            set;
        }

        /// <summary>
        /// The ExtractException that triggered the error (in stringized form).
        /// </summary>
        string StringizedException
        {
            get;
            set;
        }
        
        /// <summary>
        /// Allows configuration of this instance.
        /// </summary>
        bool ConfigureErrorEmail();

        /// <summary>
        /// Applies the default settings for the email.
        /// </summary>
        void ApplyDefaultErrorEmailSettings();
    }
}
