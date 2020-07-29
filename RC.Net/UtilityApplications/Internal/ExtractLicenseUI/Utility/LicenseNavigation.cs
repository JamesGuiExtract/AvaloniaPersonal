using System;

namespace ExtractLicenseUI.Utility
{
    [Flags]
    public enum LicenseNavigationOptions
    {
        None = 0,
        CloneLicense = 1,
        NewLicense = 2,
        ViewLicense = 3,
        EditLicense = 4,
    }
}
