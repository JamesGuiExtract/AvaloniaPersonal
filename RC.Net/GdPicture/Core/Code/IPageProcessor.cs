using Extract.GoogleCloud.Dto;

namespace Extract.GdPicture
{
    public interface IPageProcessor
    {
        /// <summary>
        /// Search for MICR on the currently selected page of the specified image ID
        /// </summary>
        /// <param name="imageID">The ID of the image loaded into GdPicture</param>
        /// <param name="documentContext">Information about the current document and utility functions</param>
        Dto.TextAnnotation? ProcessPage(int imageID, IDocumentContext documentContext);
    }
}
