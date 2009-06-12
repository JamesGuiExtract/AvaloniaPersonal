using Leadtools.Codecs;
using System;
namespace Extract.Imaging.Forms
{
    partial class ImageViewer
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <overloads>Releases resources used by the <see cref="ImageViewer"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="ImageViewer"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if(components != null)
                {
                    components.Dispose();
                }
                if (_codecs != null)
                {
                    _codecs.Dispose();
                    _codecs = null;
                }
                if (_layerObjects != null)
                {
                    _layerObjects.Dispose();
                    _layerObjects = null;
                }
                if (_annotations != null)
                {
                    _annotations.Dispose();
                    _annotations = null;
                }
                if (_trackingData != null)
                {
                    _trackingData.Dispose();
                    _trackingData = null;
                }
                if (_transform != null)
                {
                    _transform.Dispose();
                    _transform = null;
                }
                if (_printDialog != null)
                {
                    _printDialog.Dispose();
                    _printDialog = null;
                }
                if (_printDocument != null)
                {
                    _printDocument.Dispose();
                    _printDocument = null;
                }
                if (_currentOpenFile != null)
                {
                    _currentOpenFile.Dispose();
                    _currentOpenFile = null;
                }

                RasterCodecs.Shutdown();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }

        #endregion
    }
}
