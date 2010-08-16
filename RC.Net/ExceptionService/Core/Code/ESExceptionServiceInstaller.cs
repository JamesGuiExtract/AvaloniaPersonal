using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;


namespace Extract.ExceptionService
{
    /// <summary>
    /// Service installer class for the Extract exception logging service.
    /// </summary>
    [RunInstaller(true)]
    public partial class ESExceptionServiceInstaller : System.Configuration.Install.Installer
    {
        /// <summary>
        /// Initializes a new instance of the<see cref="ESExceptionServiceInstaller"/> class.
        /// </summary>
        public ESExceptionServiceInstaller()
        {
            InitializeComponent();
        }
    }
}
