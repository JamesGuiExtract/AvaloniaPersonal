using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;

namespace Extract.Laserfiche
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }
    }
}