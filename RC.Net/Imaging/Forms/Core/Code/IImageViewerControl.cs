using System;
using System.Collections.Generic;
using System.Text;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Defines a method to establish a connection with the <see cref="ImageViewer"/> control.
    /// </summary>
    /// <remarks>Controls that perform actions on the image viewer control or receive events from 
    /// the image viewer control may implement this interface to establish a connection with the
    /// image viewer control. The image viewer control passes itself to controls implementing this
    /// interface when the image viewer's 
    /// <see cref="Extract.Imaging.Forms.ImageViewer.EstablishConnections(System.Windows.Forms.Control)"/> 
    /// method is called.
    /// </remarks>
    public interface IImageViewerControl
    {
        /// <summary>
        /// Gets or sets the image viewer with which to establish a connection.
        /// </summary>
        /// <value>The image viewer with which to establish a connection. <see langword="null"/> 
        /// indicates the connection should be disconnected from the current image viewer.</value>
        /// <returns>The image viewer with which a connection is established. 
        /// <see langword="null"/> if no image viewer is connected.</returns>
        /// <remarks>
        /// <para>The image viewer control passes itself to controls implementing this method when 
        /// the image viewer's 
        /// <see cref="Extract.Imaging.Forms.ImageViewer.EstablishConnections(System.Windows.Forms.Control)"/> 
        /// method is called.
        /// </para>
        /// <note type="implementnotes"><para>Classes implementing this method should:
        /// <list type="bullet">
        /// <item>store a reference to the image viewer.</item>
        /// <item>register themselves to receive <see cref="ImageViewer"/> events that they are 
        /// interested in.</item>
        /// <item>unregister themselves from any events for which they were previously registered, 
        /// before registering for the events of a new image viewer.</item>
        /// <item>disconnect from the image viewer when <see langword="null"/> is set.</item>
        /// </list></para></note>
        /// </remarks>
        /// <seealso cref="Extract.Imaging.Forms.ImageViewer.EstablishConnections(System.Windows.Forms.Control)"/>
        ImageViewer ImageViewer
        {
            get;
            set;
        }
    }
}
