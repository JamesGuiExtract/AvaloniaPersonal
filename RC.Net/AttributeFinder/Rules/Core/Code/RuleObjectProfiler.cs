using Microsoft.Win32;
using System;
using UCLID_AFUTILSLib;
using UCLID_COMUTILSLib;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// Allows for profiling of .Net rule objects
    /// </summary>
    internal class RuleObjectProfiler : IDisposable
    {
        #region Fields

        /// <summary>
        /// Protects access to static members in the constructor.
        /// </summary>
        static object _lock = new object();

        /// <summary>
        /// The <see cref="AFUtility"/> to make the profiling calls against.
        /// </summary>
        static AFUtility _afUtility = new AFUtility();

        /// <summary>
        /// Indicates whether to profile (as determined by the ProfileRules registry key).
        /// </summary>
        static bool? _profilingActive;

        /// <summary>
        /// The handle for the current profiling call.
        /// </summary>
        int? _handle;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleObjectProfiler"/> class.
        /// This starts timing a call to a rule object.
        /// </summary>
        /// <param name="name">The name assigned to the object in the ruleset if applicable.</param>
        /// <param name="type">The type of the rule object (specify only if ruleObject does not
        /// implement <see cref="ICategorizedComponent"/>.</param>
        /// <param name="ruleObject">The rule object being called.</param>
        /// <param name="subID">The sub ID. Set this value to indicate uniqueness of a call if
        /// multiple calls are made to the same rule object.</param>
        public RuleObjectProfiler(string name, string type, object ruleObject, int subID)
        {
            try
            {
                // Check whether profiling is active per the registry key.
                if (_profilingActive == null)
                {
                    lock (_lock)
                    {
                        if (_profilingActive == null)
                        {
                            using RegistryKey maybeRegistryKey = Registry.CurrentUser.OpenSubKey(
                                @"Software\Extract Systems\AttributeFinder\Settings");

                            if (maybeRegistryKey is RegistryKey registryKey
                                && registryKey.GetValue("ProfileRules") is string registryKeyValue)
                            {
                                _profilingActive = registryKeyValue == "1";
                            }
                            else
                            {
                                _profilingActive = false;
                            }
                        }
                    }
                }

                if (_profilingActive.Value)
                {
                    _handle = _afUtility.StartProfilingRule(
                        name, type, (IIdentifiableObject)ruleObject, subID);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33752");
            }
        }

        #endregion Constructors

        #region IDisposable Members

        /// <overloads>
        /// Releases resources used by the <see cref="RuleObjectProfiler"/>
        /// </overloads>
        /// <summary>
        /// Releases resources used by the <see cref="RuleObjectProfiler"/>
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases resources used by the <see cref="RuleObjectProfiler"/>
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                // If we have a profiling handle, stop the timer for the rule object call.
                if (_handle.HasValue)
                {
                    _afUtility.StopProfilingRule(_handle.Value);
                    _handle = null;
                }
            }
        }

        #endregion IDisposable Members
    }
}
