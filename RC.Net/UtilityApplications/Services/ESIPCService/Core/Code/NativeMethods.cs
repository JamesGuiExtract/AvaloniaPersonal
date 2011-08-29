using Extract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Extract.UtilityApplications.Services
{
    /// <summary>
    /// Operations requiring platform invoke for the ESIPCService.
    /// </summary>
    static class NativeMethods
    {
        #region Structs

        /// <summary>
        /// A double word value that defines standard, specific, and generic rights. These rights
        /// are used in access control entries (ACEs) and are the primary means of specifying the
        /// requested or granted access to an object.
        /// </summary>
        [Flags]
        enum ACCESS_MASK : uint
        {
            DELETE = 0x00010000,
            READ_CONTROL = 0x00020000,
            WRITE_DAC = 0x00040000,
            WRITE_OWNER = 0x00080000,
            SYNCHRONIZE = 0x00100000,

            STANDARD_RIGHTS_REQUIRED = 0x000f0000,

            STANDARD_RIGHTS_READ = 0x00020000,
            STANDARD_RIGHTS_WRITE = 0x00020000,
            STANDARD_RIGHTS_EXECUTE = 0x00020000,

            STANDARD_RIGHTS_ALL = 0x001f0000,

            SPECIFIC_RIGHTS_ALL = 0x0000ffff,

            ACCESS_SYSTEM_SECURITY = 0x01000000,

            MAXIMUM_ALLOWED = 0x02000000,

            GENERIC_READ = 0x80000000,
            GENERIC_WRITE = 0x40000000,
            GENERIC_EXECUTE = 0x20000000,
            GENERIC_ALL = 0x10000000,

            DESKTOP_READOBJECTS = 0x00000001,
            DESKTOP_CREATEWINDOW = 0x00000002,
            DESKTOP_CREATEMENU = 0x00000004,
            DESKTOP_HOOKCONTROL = 0x00000008,
            DESKTOP_JOURNALRECORD = 0x00000010,
            DESKTOP_JOURNALPLAYBACK = 0x00000020,
            DESKTOP_ENUMERATE = 0x00000040,
            DESKTOP_WRITEOBJECTS = 0x00000080,
            DESKTOP_SWITCHDESKTOP = 0x00000100,

            WINSTA_ENUMDESKTOPS = 0x00000001,
            WINSTA_READATTRIBUTES = 0x00000002,
            WINSTA_ACCESSCLIPBOARD = 0x00000004,
            WINSTA_CREATEDESKTOP = 0x00000008,
            WINSTA_WRITEATTRIBUTES = 0x00000010,
            WINSTA_ACCESSGLOBALATOMS = 0x00000020,
            WINSTA_EXITWINDOWS = 0x00000040,
            WINSTA_ENUMERATE = 0x00000100,
            WINSTA_READSCREEN = 0x00000200,

            WINSTA_ALL_ACCESS = 0x0000037f
        }

        /// <summary>
        /// A double word value that defines rights for the service control manager.
        /// </summary>
        [Flags]
        public enum SCM_ACCESS : uint
        {
            /// <summary>
            /// Required to connect to the service control manager.
            /// </summary>
            SC_MANAGER_CONNECT = 0x00001,

            /// <summary>
            /// Required to call the CreateService function to create a service
            /// object and add it to the database.
            /// </summary>
            SC_MANAGER_CREATE_SERVICE = 0x00002,

            /// <summary>
            /// Required to call the EnumServicesStatusEx function to list the 
            /// services that are in the database.
            /// </summary>
            SC_MANAGER_ENUMERATE_SERVICE = 0x00004,

            /// <summary>
            /// Required to call the LockServiceDatabase function to acquire a 
            /// lock on the database.
            /// </summary>
            SC_MANAGER_LOCK = 0x00008,

            /// <summary>
            /// Required to call the QueryServiceLockStatus function to retrieve 
            /// the lock status information for the database.
            /// </summary>
            SC_MANAGER_QUERY_LOCK_STATUS = 0x00010,

            /// <summary>
            /// Required to call the NotifyBootConfigStatus function.
            /// </summary>
            SC_MANAGER_MODIFY_BOOT_CONFIG = 0x00020,

            /// <summary>
            /// Includes STANDARD_RIGHTS_REQUIRED, in addition to all access 
            /// rights in this table.
            /// </summary>
            SC_MANAGER_ALL_ACCESS = ACCESS_MASK.STANDARD_RIGHTS_REQUIRED |
                SC_MANAGER_CONNECT |
                SC_MANAGER_CREATE_SERVICE |
                SC_MANAGER_ENUMERATE_SERVICE |
                SC_MANAGER_LOCK |
                SC_MANAGER_QUERY_LOCK_STATUS |
                SC_MANAGER_MODIFY_BOOT_CONFIG,

            GENERIC_READ = ACCESS_MASK.STANDARD_RIGHTS_READ |
                SC_MANAGER_ENUMERATE_SERVICE |
                SC_MANAGER_QUERY_LOCK_STATUS,

            GENERIC_WRITE = ACCESS_MASK.STANDARD_RIGHTS_WRITE |
                SC_MANAGER_CREATE_SERVICE |
                SC_MANAGER_MODIFY_BOOT_CONFIG,

            GENERIC_EXECUTE = ACCESS_MASK.STANDARD_RIGHTS_EXECUTE |
                SC_MANAGER_CONNECT | SC_MANAGER_LOCK,

            GENERIC_ALL = SC_MANAGER_ALL_ACCESS,
        }

        /// <summary>
        /// Access to the service. Before granting the requested access, the
        /// system checks the access token of the calling process. 
        /// </summary>
        [Flags]
        public enum SERVICE_ACCESS : uint
        {
            /// <summary>
            /// Required to call the QueryServiceConfig and 
            /// QueryServiceConfig2 functions to query the service configuration.
            /// </summary>
            SERVICE_QUERY_CONFIG = 0x00001,

            /// <summary>
            /// Required to call the ChangeServiceConfig or ChangeServiceConfig2 function 
            /// to change the service configuration. Because this grants the caller 
            /// the right to change the executable file that the system runs, 
            /// it should be granted only to administrators.
            /// </summary>
            SERVICE_CHANGE_CONFIG = 0x00002,

            /// <summary>
            /// Required to call the QueryServiceStatusEx function to ask the service 
            /// control manager about the status of the service.
            /// </summary>
            SERVICE_QUERY_STATUS = 0x00004,

            /// <summary>
            /// Required to call the EnumDependentServices function to enumerate all 
            /// the services dependent on the service.
            /// </summary>
            SERVICE_ENUMERATE_DEPENDENTS = 0x00008,

            /// <summary>
            /// Required to call the StartService function to start the service.
            /// </summary>
            SERVICE_START = 0x00010,

            /// <summary>
            ///     Required to call the ControlService function to stop the service.
            /// </summary>
            SERVICE_STOP = 0x00020,

            /// <summary>
            /// Required to call the ControlService function to pause or continue 
            /// the service.
            /// </summary>
            SERVICE_PAUSE_CONTINUE = 0x00040,

            /// <summary>
            /// Required to call the EnumDependentServices function to enumerate all
            /// the services dependent on the service.
            /// </summary>
            SERVICE_INTERROGATE = 0x00080,

            /// <summary>
            /// Required to call the ControlService function to specify a user-defined
            /// control code.
            /// </summary>
            SERVICE_USER_DEFINED_CONTROL = 0x00100,

            /// <summary>
            /// Includes STANDARD_RIGHTS_REQUIRED in addition to all access rights in this table.
            /// </summary>
            SERVICE_ALL_ACCESS = (ACCESS_MASK.STANDARD_RIGHTS_REQUIRED |
                SERVICE_QUERY_CONFIG |
                SERVICE_CHANGE_CONFIG |
                SERVICE_QUERY_STATUS |
                SERVICE_ENUMERATE_DEPENDENTS |
                SERVICE_START |
                SERVICE_STOP |
                SERVICE_PAUSE_CONTINUE |
                SERVICE_INTERROGATE |
                SERVICE_USER_DEFINED_CONTROL),

            GENERIC_READ = ACCESS_MASK.STANDARD_RIGHTS_READ |
                SERVICE_QUERY_CONFIG |
                SERVICE_QUERY_STATUS |
                SERVICE_INTERROGATE |
                SERVICE_ENUMERATE_DEPENDENTS,

            GENERIC_WRITE = ACCESS_MASK.STANDARD_RIGHTS_WRITE |
                SERVICE_CHANGE_CONFIG,

            GENERIC_EXECUTE = ACCESS_MASK.STANDARD_RIGHTS_EXECUTE |
                SERVICE_START |
                SERVICE_STOP |
                SERVICE_PAUSE_CONTINUE |
                SERVICE_USER_DEFINED_CONTROL,

            /// <summary>
            /// Required to call the QueryServiceObjectSecurity or 
            /// SetServiceObjectSecurity function to access the SACL. The proper
            /// way to obtain this access is to enable the SE_SECURITY_NAME 
            /// privilege in the caller's current access token, open the handle 
            /// for ACCESS_SYSTEM_SECURITY access, and then disable the privilege.
            /// </summary>
            ACCESS_SYSTEM_SECURITY = ACCESS_MASK.ACCESS_SYSTEM_SECURITY,

            /// <summary>
            /// Required to call the DeleteService function to delete the service.
            /// </summary>
            DELETE = ACCESS_MASK.DELETE,

            /// <summary>
            /// Required to call the QueryServiceObjectSecurity function to query
            /// the security descriptor of the service object.
            /// </summary>
            READ_CONTROL = ACCESS_MASK.READ_CONTROL,

            /// <summary>
            /// Required to call the SetServiceObjectSecurity function to modify
            /// the Dacl member of the service object's security descriptor.
            /// </summary>
            WRITE_DAC = ACCESS_MASK.WRITE_DAC,

            /// <summary>
            /// Required to call the SetServiceObjectSecurity function to modify 
            /// the Owner and Group members of the service object's security 
            /// descriptor.
            /// </summary>
            WRITE_OWNER = ACCESS_MASK.WRITE_OWNER,
        }

        #endregion Structs

        #region P/Invokes

        /// <summary>
        /// Establishes a connection to the service control manager on the specified computer and
        /// opens the specified service control manager database.
        /// </summary>
        /// <param name="machineName">The name of the target computer. If the pointer is
        /// <see langword="null"/> or points to an empty string, the function connects to the
        /// service control manager on the local computer.</param>
        /// <param name="databaseName">The name of the service control manager database. This
        /// parameter should be set to SERVICES_ACTIVE_DATABASE. If it is <see langword="null"/>,
        /// the SERVICES_ACTIVE_DATABASE database is opened by default.</param>
        /// <param name="dwAccess">The access to the service control manager. The
        /// <see cref="SCM_ACCESS.SC_MANAGER_CONNECT"/> access right is implicitly specified by
        /// calling this function.</param>
        /// <returns>If the function succeeds, the return value is a handle to the specified service
        /// control manager database. If the function fails, the return value is
        /// <see cref="IntPtr.Zero"/>. To get extended error information, call
        /// <see cref="Marshal.GetLastWin32Error"/>.
        /// </returns>
        [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern IntPtr OpenSCManager(string machineName, string databaseName, uint dwAccess);


        /// <summary>
        /// Closes a handle to a service control manager or service object.
        /// </summary>
        /// <param name="hSCObject">A handle to the service control manager object or the service
        /// object to close. Handles to service control manager objects are returned by the
        /// <see cref="OpenSCManager"/> function, and handles to service objects are returned by
        /// either the <see cref="OpenService"/> or CreateService function.</param>
        /// <returns>If the function succeeds, the return value is <see langword="true"/>. If the
        /// function fails, the return value is <see langword="false"/>. To get extended error
        /// information, call <see cref="Marshal.GetLastWin32Error"/>.
        /// </returns>
        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseServiceHandle(IntPtr hSCObject);

        /// <summary>
        /// Opens an existing service.
        /// </summary>
        /// <param name="hSCManager">A handle to the service control manager database. The
        /// <see cref="OpenSCManager"/> function returns this handle. This handle must have the
        /// <see cref="SCM_ACCESS.SC_MANAGER_ENUMERATE_SERVICE"/> access right.</param>
        /// <param name="lpServiceName">The name of the service to be opened. This is the name
        /// specified by the lpServiceName parameter of the CreateService function when the service
        /// object was created, not the service display name that is shown by user interface
        /// applications to identify the service. Service name comparisons are always case
        /// insensitive.</param>
        /// <param name="dwDesiredAccess">The access to the service. Before granting the requested
        /// access, the system checks the access token of the calling process against the
        /// discretionary access-control list of the security descriptor associated with the service
        /// object.</param>
        /// <returns>If the function succeeds, the return value is a handle to the service. If the
        /// function fails, the return value is <see cref="IntPtr.Zero"/>. To get extended error
        /// information, call <see cref="Marshal.GetLastWin32Error"/>.</returns>
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName, uint dwDesiredAccess);

        /// <summary>
        /// Retrieves a copy of the security descriptor associated with a service object. 
        /// </summary>
        /// <param name="serviceHandle">A handle to the service control manager or the service.
        /// Handles to the service control manager are returned by the <see cref="OpenSCManager"/>
        /// function, and handles to a service are returned by either the <see cref="OpenService"/>
        /// or CreateService function. The handle must have the
        /// <see cref="ACCESS_MASK.READ_CONTROL"/> access right.</param>
        /// <param name="secInfo">A set of bit flags that indicate the type of security information
        /// to retrieve. This parameter can be a combination of the SECURITY_INFORMATION bit flags,
        /// with the exception that this function does not support the LABEL_SECURITY_INFORMATION
        /// value.</param>
        /// <param name="lpSecDesrBuf">A pointer to a buffer that receives a copy of the security
        /// descriptor of the specified service object. The calling process must have the appropriate
        /// access to view the specified aspects of the security descriptor of the object. The
        /// SECURITY_DESCRIPTOR structure is returned in self-relative format.</param>
        /// <param name="bufSize">The size of the buffer pointed to by the
        /// <see paramref="lpSecurityDescriptor"/> parameter, in bytes. The largest size allowed is
        /// 8 kilobytes.</param>
        /// <param name="bufSizeNeeded">A pointer to a variable that receives the number of bytes
        /// needed to return the requested security descriptor information, if the function fails.
        /// </param>
        /// <returns>If the function succeeds, the return value is <see langword="true"/>. If the
        /// function fails, the return value is <see langword="false"/>. To get extended error
        /// information, call <see cref="Marshal.GetLastWin32Error"/>.
        /// </returns>
        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool QueryServiceObjectSecurity(IntPtr serviceHandle, SecurityInfos secInfo,
            byte[] lpSecDesrBuf, uint bufSize, out uint bufSizeNeeded);

        /// <summary>
        /// Sets the security descriptor of a service object.
        /// </summary>
        /// <param name="serviceHandle">A handle to the service. This handle is returned by the
        /// <see cref="OpenService"/> or CreateService function. The access required for this handle
        /// depends on the security information specified in the
        /// <see paramref="dwSecurityInformation"/> parameter.</param>
        /// <param name="secInfos">A <see cref="SecurityInfos"/> value specifying the components of
        /// the security descriptor to set.</param>
        /// <param name="lpSecDesrBuf">A pointer to a SECURITY_DESCRIPTOR structure that contains
        /// the new security information.</param>
        /// <returns>If the function succeeds, the return value is <see langword="true"/>. If the
        /// function fails, the return value is <see langword="false"/>. To get extended error
        /// information, call <see cref="Marshal.GetLastWin32Error"/>.
        /// </returns>
        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetServiceObjectSecurity(IntPtr serviceHandle, SecurityInfos secInfos,
            byte[] lpSecDesrBuf);

        #endregion P/Invokes

        #region Methods

        /// <summary>
        /// Grants all users access to start and stop the ESIPCService.
        /// Based on the sample code here:
        /// http://pinvoke.net/default.aspx/advapi32/QueryServiceObjectSecurity.html
        /// </summary>
        public static void SetServicePermissions()
        {
            IntPtr serviceManagerHandle = IntPtr.Zero;
            IntPtr serviceHandle = IntPtr.Zero;

            try
            {
                serviceManagerHandle = OpenSCManager(null, null, (uint)SCM_ACCESS.SC_MANAGER_ALL_ACCESS);
                ExtractException.Win32Assert("ELI33449", "Failed to open service manager.", serviceManagerHandle != IntPtr.Zero);

                serviceHandle = OpenService(serviceManagerHandle, "ESIPCService",
                    (uint)(SERVICE_ACCESS.READ_CONTROL | SERVICE_ACCESS.WRITE_DAC));
                ExtractException.Win32Assert("ELI33450", "Failed to open service.", serviceManagerHandle != IntPtr.Zero);

                // First call to QueryServiceObjectSecurity is to get the buffer size.
                byte[] psd = new byte[0];
                uint bufSizeNeeded;
                bool success = QueryServiceObjectSecurity(
                    serviceHandle, SecurityInfos.DiscretionaryAcl, psd, 0, out bufSizeNeeded);
                
                if (!success)
                {
                    // Failure with InsufficientBuffer is expected the first time to get the
                    // required buffer sized.
                    ExtractException.Win32Assert("ELI33451", "Failed to retrieve service permissions.",
                        Marshal.GetLastWin32Error() == (int)Win32ErrorCode.InsufficientBuffer);
                    psd = new byte[bufSizeNeeded];
                    success = QueryServiceObjectSecurity(
                        serviceHandle, SecurityInfos.DiscretionaryAcl, psd, bufSizeNeeded, out bufSizeNeeded);
                }
                ExtractException.Win32Assert("ELI33452", "Failed to retrieve service permissions.", success);

                // Get security descriptor via raw into DACL form so ACE ordering checks are done for us.
                RawSecurityDescriptor rsd = new RawSecurityDescriptor(psd, 0);
                RawAcl racl = rsd.DiscretionaryAcl;
                DiscretionaryAcl dacl = new DiscretionaryAcl(false, false, racl);
                
                // We're going to add access for all users that have logged onto the local machine.
                SecurityIdentifier usersSid = new SecurityIdentifier(WellKnownSidType.LocalSid, null);

                // Grant access to start and stop the service.
                dacl.SetAccess(AccessControlType.Allow, usersSid,
                    (int)(SERVICE_ACCESS.SERVICE_START | SERVICE_ACCESS.SERVICE_STOP),
                    InheritanceFlags.None, PropagationFlags.None);

                // Convert discretionary ACL back to raw form
                byte[] rawdacl = new byte[dacl.BinaryLength];
                dacl.GetBinaryForm(rawdacl, 0);
                rsd.DiscretionaryAcl = new RawAcl(rawdacl, 0);

                // Set raw security descriptor on service again
                byte[] rawsd = new byte[rsd.BinaryLength];
                rsd.GetBinaryForm(rawsd, 0);
                success = SetServiceObjectSecurity(serviceHandle, SecurityInfos.DiscretionaryAcl, rawsd);
                ExtractException.Win32Assert("ELI33453", "Failed to apply service permissions.", success);                
            }
            catch (Exception ex)
            {
                ExtractException ee =
                    new ExtractException("ELI33454", "Failed to initialize ESIPCService permissions.", ex);
                throw ee;
            }
            finally
            {
                if (serviceManagerHandle != IntPtr.Zero)
                {
                    CloseServiceHandle(serviceManagerHandle);
                }
                if (serviceHandle != IntPtr.Zero)
                {
                    CloseServiceHandle(serviceHandle);
                }
            }
        }

        #endregion Methods
    }
}
