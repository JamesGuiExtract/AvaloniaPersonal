using Extract.Licensing;
using Extract.Utilities;
using StatisticsReporter.Properties;
using System;
using System.Globalization;
using System.Linq;

namespace StatisticsReporter
{
    class Program
    {
        static void Main(/*string[] args*/)
        {
            // Validate the license
            LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
            LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexIDShieldCoreObjects,
                                             "ELI41500", 
                                             "Statistics Reporter");
        }
    }
}
