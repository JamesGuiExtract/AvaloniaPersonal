using System;
using System.Runtime.InteropServices;

namespace Extract.Interfaces
{
    /// <summary>
    /// Used to create secure objects.
    /// </summary>
    [ComVisible(true)]
    [Guid("D863B253-9CEA-47CC-9F69-05B890055CF6")]
    public interface ISecureObjectCreator
    {
        /// <summary>
        /// Gets the ID of this instance.
        /// </summary>
        int InstanceID
        {
            get;
        }

        /// <summary>
        /// Gets an instance of the COM class indicated by <see paramref="progId"/>.
        /// </summary>
        /// <param name="progId">The ProgID of the COM class to instantiate.</param>
        /// <returns>An instance of the COM class indicated by <see paramref="progId"/>.</returns>
        object GetObject(string progId);
    }
}
