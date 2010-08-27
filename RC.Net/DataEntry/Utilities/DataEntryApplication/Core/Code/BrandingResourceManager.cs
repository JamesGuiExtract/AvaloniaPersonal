using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Resources;
using EmbeddedResources = Extract.DataEntry.Utilities.DataEntryApplication.BrandingResources.Default;

namespace Extract.DataEntry.Utilities.DataEntryApplication
{
 
    /// <summary>
    /// Provides the resources for branding the DataEntryApplication as a particular product. The
    /// resources will come from the specified resource file, if provided, but the embedded default
    /// resources will be used if not available from a resource file.
    /// </summary>
    internal class BrandingResourceManager : IDisposable
    {
        #region Fields

        /// <summary>
        /// Used to load reasources from the specified resource file.
        /// </summary>
        ResourceManager _fileResources = null;

        /// <summary>
        /// Keeps track of resources that failed to load from the specified resource file.
        /// </summary>
        List<string> _loadFailures = new List<string>();

        /// <summary>
        /// The name that the DataEntryApplication will use.
        /// </summary>
        public string _applicationTitle;

        /// <summary>
        /// A description of the DataEntryApplication will use.
        /// </summary>
        public string _applicationDescription;

        /// <summary>
        /// The application's icon.
        /// </summary>
        public Icon _applicationIcon;

        /// <summary>
        /// The logo that should appear in the about box.
        /// </summary>
        public Image _aboutLogo;

        /// <summary>
        /// The location of the help file for the application.
        /// </summary>
        public string _helpFilePath;

        /// <summary>
        /// The version of the application.
        /// </summary>
        public string _version;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="BrandingResourceManager"/> instance using the specified
        /// resource file.
        /// </summary>
        /// <param name="brandingFilename"></param>
        public BrandingResourceManager(string brandingFilename)
        {
            try
            {
                // If a resource file has been specified, create a ResourceManager to read from it.
                if (!string.IsNullOrEmpty(brandingFilename))
                {
                    _fileResources = ResourceManager.CreateFileBasedResourceManager(
                        Path.GetFileNameWithoutExtension(brandingFilename),
                        Path.GetDirectoryName(brandingFilename), null);
                }

                _applicationTitle = GetString("ApplicationTitle");
                _applicationDescription = GetString("ApplicationDescription");
                _applicationIcon = (Icon)GetObject("ApplicationIcon");
                _aboutLogo = (Image)GetObject("AboutLogo");
                _helpFilePath = GetString("HelpFilePath");
                _version = GetString("Version");

                if (_loadFailures.Count > 0)
                {
                    ExtractException ee = new ExtractException("ELI30540",
                        "Some resources could not be loaded.");
                    foreach (string resourceName in _loadFailures)
                    {
                        ee.AddDebugData("Resource", resourceName, true);
                    }

                    ee.Log();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI30541", "Failed to load resources.", ex);
                ee.AddDebugData("Branding Filename", brandingFilename, false);
                throw ee;
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// The name that the DataEntryApplication will use.
        /// </summary>
        public string ApplicationTitle
        {
            get
            {
                return _applicationTitle;
            }
        }

        /// <summary>
        /// A description of the DataEntryApplication will use.
        /// </summary>
        public string ApplicationDescription
        {
            get
            {
                return _applicationDescription;
            }
        }

        /// <summary>
        /// The application's icon.
        /// </summary>
        public Icon ApplicationIcon
        {
            get
            {
                return _applicationIcon;
            }
        }

        /// <summary>
        /// The logo that should appear in the about box.
        /// </summary>
        public Image AboutLogo
        {
            get
            {
                return _aboutLogo;
            }
        }

        /// <summary>
        /// The location of the help file for the application.
        /// </summary>
        public string HelpFilePath
        {
            get
            {
                return _helpFilePath;
            }
        }

        /// <summary>
        /// The version of the application.
        /// </summary>
        public string Version
        {
            get
            {
                return _version;
            }
        }

        #endregion Properties

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="BrandingResourceManager"/>.
        /// </summary>
        /// 
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="BrandingResourceManager"/>.
        /// </overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="BrandingResourceManager"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _applicationIcon.Dispose();
                _applicationIcon = null;

                _aboutLogo.Dispose();
                _aboutLogo = null;
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members

        #region Private Members

        /// <summary>
        /// Attempts to read a <see langoword="string"/> property from the provided resource file
        /// (if available), or otherwise from embedded defaults.
        /// </summary>
        /// <param name="propertyName">The property to retrieve</param>
        /// <returns>The values of the <see langoword="string"/> resource.</returns>
        string GetString(string propertyName)
        {
            try
            {
                string value = null;

                // Attempt to load from the resource file, if available.
                try
                {
                    if (_fileResources != null)
                    {
                        value = _fileResources.GetString(propertyName);
                    }
                }
                catch { }

                // Otherwise, fall back on the embedded resource while making note of any resources
                // that failed to load.
                if (value == null)
                {
                    if (_fileResources != null)
                    {
                        _loadFailures.Add(propertyName);
                    }
                    value = EmbeddedResources.ResourceManager.GetString(propertyName);
                }

                return value;
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI30542", "Failed to load resource.", ex);
                ee.AddDebugData("Name", propertyName, true);
                throw ee;
            }
        }

        /// <summary>
        /// Attempts to read an <see langoword="object"/> property from the provided resource file
        /// (if available), or otherwise from embedded defaults.
        /// </summary>
        /// <param name="propertyName">The property to retrieve</param>
        /// <returns>The values of the <see langoword="object"/> resource.</returns>
        object GetObject(string propertyName)
        {
            try
            {
                object resource = null;

                // Attempt to load from the resource file, if available.
                try
                {
                    if (_fileResources != null)
                    {
                        resource = _fileResources.GetObject(propertyName);
                    }
                }
                catch { }

                // Otherwise, fall back on the embedded resource while making note of any resources
                // that failed to load.
                if (resource == null)
                {
                    if (_fileResources != null)
                    {
                        _loadFailures.Add(propertyName);
                    }
                    resource = EmbeddedResources.ResourceManager.GetObject(propertyName);
                }

                return resource;
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI30543", "Failed to load resource.", ex);
                ee.AddDebugData("Name", propertyName, false);
                throw ee;
            }
        }

        #endregion Private Members
    }
}
