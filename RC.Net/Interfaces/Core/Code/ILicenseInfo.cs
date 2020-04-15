using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;

// This assembly is reserved for the definition of interfaces and helper classes for those
// interfaces. To ensure these interfaces are accessible from all projects without circular
// dependency issues and to allow the assemblies definitions to be used in both 32 and 64 bit code,
// This assembly should have no dependencies on any other Extract projects.
namespace Extract.Interfaces
{
    /// <summary>
    /// Provides active license status for Extract software.
    /// </summary>
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    [Guid("57C53FC5-E9C7-40B9-B16B-7FA9BD0C1E78")]
    [CLSCompliant(false)]
    public interface ILicenseInfo
    {
        /// <summary>
        /// Gets all component codes that have valid active licenses (whether temporary or permanent)
        /// </summary>
        /// <param name="expirationDates">If specified, can be used to retrieve the expiration date corresponding
        /// to each of the returned component codes with a date of 0 representing a permanently licensed package.</param>
        [DispId(1)]
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#")]
        uint[] GetLicensedComponents(ref DateTime[] expirationDates);

        /// <summary>
        /// Gets all license package names that have valid active licenses (whether temporary or permanent)
        /// License status is based on all components referenced in the packages.dat file compiled with the
        /// release having a proper license. Package names are indicated based on the the status of included
        /// components regardless of whether the name was specifically selected in the license generator.
        /// </summary>
        /// <param name="expirationDates">If specified, can be used to retrieve the expiration date corresponding
        /// to each of the returned package names with a date of 0 representing a permanently licensed package.</param>
        [DispId(2)]
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#")]
        string[] GetLicensedPackageNames(ref DateTime[] expirationDates);

        /// <summary>
        /// Gets all license package names that have valid active licenses (whether temporary or permanent)
        /// License status is based on all components referenced in the packages.dat file compiled with the
        /// release having a proper license. Package names are indicated based on the the status of included
        /// components regardless of whether the name was specifically selected in the license generator.
        /// </summary>
        [DispId(3)]
        string[] GetLicensedPackageNamesWithExpiration();
    }
}
