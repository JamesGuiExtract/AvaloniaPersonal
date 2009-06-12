using System;
using System.Collections.Generic;
using System.Text;

namespace Extract
{
    /// <summary>
    /// Enumeration indicating how to treat a requirement list that is passed as a
    /// function argument.
    /// </summary>
    public enum ArgumentRequirement
    {
        /// <summary>
        /// Require that at least one of the requirements in the list is met.
        /// </summary>
        Any,

        /// <summary>
        /// Require that all requirements in the list are met.
        /// </summary>
        All
    }
}
