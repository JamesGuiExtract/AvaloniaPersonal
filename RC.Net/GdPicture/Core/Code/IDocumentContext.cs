using GdPicture14;

namespace Extract.GdPicture
{
    public interface IDocumentContext
    {
        /// <summary>
        /// Container for APIs and utilities
        /// </summary>
        GdPictureUtility GdPictureUtility { get; }

        /// <summary>
        /// The current page number being processed
        /// </summary>
        int CurrentPageNumber { get; }

        /// <summary>
        /// Throw an exception with debug data about the current state if the status is not OK
        /// </summary>
        void ThrowIfStatusNotOK(GdPictureStatus status, string eliCode, string message);
    }
}
