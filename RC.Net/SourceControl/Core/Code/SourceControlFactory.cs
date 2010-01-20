using System;
using System.Collections.Generic;
using System.Text;

namespace Extract.SourceControl
{
    /// <summary>
    /// Represents a way to construct <see cref="ISourceControl"/> objects.
    /// </summary>
    public static class SourceControlFactory
    {
        /// <summary>
        /// Creates a source control database using the specified login settings.
        /// </summary>
        /// <param name="settings">The login settings to use.</param>
        /// <param name="engineeringRoot">The root of the engineering tree.  If
        /// <see langword="null"/> will use a default value of "$/Engineering".</param>
        /// <returns>A source control database using <paramref name="settings"/>.</returns>
        public static ISourceControl Create(LogOnSettings settings, string engineeringRoot)
        {
            return new VaultSourceControl(settings, engineeringRoot);
        }
    }
}
