using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Extract.Utilities
{
    /// <summary>
    /// Contains a collection of utility methods to aid thread related code.
    /// </summary>
    public static class ThreadingMethods
    {
        #region Fields

        /// <summary>
        /// Mutex object used to protect access to the mutex creation.
        /// </summary>
        static object _lock = new object();

        #endregion Fields

        #region Methods

        /// <summary>
        /// Gets a system global mutex for the specified mutex name.
        /// This method will set the access rights to <see cref="MutexRights.Synchronize"/>
        /// and <see cref="MutexRights.Modify"/> for the SID = "Everyone".
        /// <para><b>Note:</b></para>
        /// If <paramref name="mutexName"/> is not prefaced with the string
        /// "Global\", then the string will have "Global\" prepended to it
        /// before the mutex is created.
        /// </summary>
        /// <param name="mutexName">The named mutex to get (or create).</param>
        /// <returns>A <see cref="Mutex"/> for the specified name.</returns>
        public static Mutex GetGlobalNamedMutex(string mutexName)
        {
            // Get the global named mutex for the "Everyone" group with
            // the default permissions
            return GetGlobalNamedMutex(mutexName,
                new SecurityIdentifier(WellKnownSidType.WorldSid, null));
        }

        /// <summary>
        /// Gets a system global mutex for the specified mutex name.
        /// This method will set the access rights to <see cref="MutexRights.Synchronize"/>
        /// and <see cref="MutexRights.Modify"/> for the specified SID.
        /// <para><b>Note:</b></para>
        /// If <paramref name="mutexName"/> is not prefaced with the string
        /// "Global\", then the string will have "Global\" prepended to it
        /// before the mutex is created.
        /// </summary>
        /// <param name="mutexName">The named mutex to get (or create).</param>
        /// <returns>A <see cref="Mutex"/> for the specified name.</returns>
        /// <param name="sid">The <see cref="SecurityIdentifier"/> to set the access rights
        /// for.</param>
        public static Mutex GetGlobalNamedMutex(string mutexName, SecurityIdentifier sid)
        {
            // Get the global named mutex for the specified SID with
            // the default rights of FullControl [FlexIDSCore #4261]
            return GetGlobalNamedMutex(mutexName, sid, MutexRights.FullControl);
        }

        /// <summary>
        /// Gets a system global mutex for the specified mutex name.
        /// This method will set the specified access rights for the specified SID.
        /// <para><b>Note:</b></para>
        /// If <paramref name="mutexName"/> is not prefaced with the string
        /// "Global\", then the string will have "Global\" prepended to it
        /// before the mutex is created.
        /// </summary>
        /// <param name="mutexName">The named mutex to get (or create).</param>
        /// <param name="sid">The <see cref="SecurityIdentifier"/> to set the access rights
        /// for.</param>
        /// <param name="rights">The rights to assign to the mutex.</param>
        /// <returns>A <see cref="Mutex"/> for the specified name.</returns>
        // Although this method is only using the base type values of the SecurityIdentifier,
        // since we are specificially setting SecurityIdentifier access permissions to
        // a globally named mutex it is better to force the user to provide a SecurityIdentifier.
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static Mutex GetGlobalNamedMutex(string mutexName, SecurityIdentifier sid,
            MutexRights rights)
        {
            try
            {
                ExtractException.Assert("ELI29989", "No mutex name specified.",
                    !string.IsNullOrEmpty(mutexName));

                lock (_lock)
                {
                    if (!mutexName.StartsWith("Global\\", StringComparison.Ordinal))
                    {
                        mutexName = "Global\\" + mutexName;
                    }

                    Mutex mutex = null;
                    bool exists = true;
                    try
                    {
                        mutex = Mutex.OpenExisting(mutexName);
                    }
                    catch (WaitHandleCannotBeOpenedException)
                    {
                        exists = false;
                    }

                    if (!exists)
                    {
                        // Create an access rule to allow the specified rights for
                        // the specified SID
                        MutexAccessRule rule = new MutexAccessRule(sid, rights,
                            AccessControlType.Allow);

                        // Add the access rule
                        MutexSecurity security = new MutexSecurity();
                        security.AddAccessRule(rule);

                        // Create the mutex since it did not exist. Create it with the specified
                        // access rule.
                        bool createdNew = false;
                        mutex = new Mutex(false, mutexName, out createdNew, security);
                    }

                    return mutex;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29987", ex);
            }
        }

        /// <summary>
        /// Runs the specified <see paramref="action"/> asynchronously within a try/catch handler
        /// that will display or log any exceptions.
        /// </summary>
        /// <param name="eliCode">The ELI code to associate with any exception.</param>
        /// <param name="action">The <see cref="Action"/> to be executed.</param>
        /// <param name="displayExceptions"><see langword="true"/> to display any exception caught;
        /// <see langword="false"/> to log instead.</param>
        public static void RunInBackgroundThread(string eliCode, Action action,
            bool displayExceptions = true)
        {
            try
            {
                // Per this thread:
                // http://social.msdn.microsoft.com/Forums/en-US/parallelextensions/thread/7b3a42e5-4ebf-405a-8ee6-bcd2f0214f85/
                // Since nothing is waiting on this task, there is no harm in not displosing of the
                // task.
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        if (displayExceptions)
                        {
                            ex.ExtractDisplay(eliCode);
                        }
                        else
                        {
                            ex.ExtractLog(eliCode);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35413");
            }
        }

        #endregion Methods
    }
}
