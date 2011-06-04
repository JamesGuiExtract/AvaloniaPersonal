using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.Imaging
{
    /// <summary>
    /// Provides a convenient way to access the same <see cref="SpatialString"/> across both an STA
    /// UI thread a background MTA thread(s) by converting to/from a stringized version when
    /// necessary.
    /// </summary>
    public class ThreadSafeSpatialString
    {
        /// <summary>
        /// A <see cref="Control"/> in the UI thread where this instance may be used. This instance
        /// may also be used in any other thread that is MTA but cannot be used in an STA thread
        /// other than the one hosting this control.
        /// </summary>
        Control _uiControl;

        /// <summary>
        /// An array of the stringized spatial strings that make up this instance (there may be more
        /// than one if this instance was created as a join of multiple SpatialStrings).
        /// </summary>
        string[] _ocrData;

        /// <summary>
        /// A cached <see cref="SpatialString"/> instance that is able to be used in the UI thread.
        /// </summary>
        SpatialString _uiThreadSpatialString;

        /// <summary>
        /// A cached <see cref="SpatialString"/> instance that is able to be used in a background
        /// MTA thread.
        /// </summary>
        volatile SpatialString _backgroundSpatialString;

        /// <summary>
        /// A <see cref="MiscUtils"/> to be used for serializing/deserializing OCR data in
        /// background worker threads.
        /// </summary>
        MiscUtils _uiThreadMiscUtils;

        /// <summary>
        /// A <see cref="MiscUtils"/> to be used for serializing/deserializing OCR data in the UI
        /// thread.
        /// </summary>
        volatile MiscUtils _backgroundMiscUtils;

        /// <summary>
        /// For synchronizing access while constructing a SpatialString value for the calling
        /// thread.
        /// </summary>
        object _lock = new object();

        /// <overloads>
        /// Initializes a new instance of the <see cref="ThreadSafeSpatialString"/> class.
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadSafeSpatialString"/> class.
        /// </summary>
        /// <param name="uiControl">A <see cref="Control"/> in the UI thread where this instance
        /// may be used. This instance may also be used in any other thread that is MTA but cannot
        /// be used in an STA thread other than the one hosting this control.</param>
        /// <param name="ocrData">A stringized <see cref="SpatialString"/> instance.</param>
        public ThreadSafeSpatialString(Control uiControl, string ocrData)
        {
            try 
	        {
                _uiControl = uiControl;
		        _ocrData = new string[] { ocrData };
	        }
	        catch (Exception ex)
	        {
		        throw ex.AsExtract("ELI32639");
	        }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadSafeSpatialString"/> class.
        /// </summary>
        /// <param name="uiControl">A <see cref="Control"/> in the UI thread where this instance
        /// may be used. This instance may also be used in any other thread that is MTA but cannot
        /// be used in an STA thread other than the one hosting this control.</param>
        /// <param name="ocrData">An enumeration of <see cref="ThreadSafeSpatialString"/> instances
        /// to be combined to create one <see cref="ThreadSafeSpatialString"/> instance. (These may
        /// be pages of a document, for example.</param>
        public ThreadSafeSpatialString(Control uiControl, IEnumerable<ThreadSafeSpatialString> ocrData)
        {
            try 
	        {
                _uiControl = uiControl;

                ExtractException.Assert("ELI32640",
                    "Cannot create string in STA thread other than the UI thread.",
                    !_uiControl.InvokeRequired ||
                    Thread.CurrentThread.GetApartmentState() == ApartmentState.MTA);

                // If all ocrData instance already have stringized data, it will be most
                // efficient to simply store this data, then use it to construce SpatialStrings when
                // needed.
                if (ocrData.All(data => data._ocrData != null))
                {
                    _ocrData = ocrData
                        .SelectMany(data => data._ocrData)
                        .ToArray();
                }
                // Otherwise we need to build a unified spatial string.
                else
                {
                    SpatialString unifiedSpatialString = new SpatialString();
                    foreach (SpatialString spatialString in ocrData)
                    {
                        unifiedSpatialString.Append(spatialString);
                    }

                    // Cache the unifiedSpatialString for the appropriate threading model. 
                    if (_uiControl.InvokeRequired)
                    {
                        _backgroundSpatialString = unifiedSpatialString;
                    }
                    else
                    {
                        _uiThreadSpatialString = unifiedSpatialString;
                    }
                }
	        }
	        catch (Exception ex)
	        {
		        throw ex.AsExtract("ELI32641");
	        }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadSafeSpatialString"/> class.
        /// </summary>
        /// <param name="uiControl">A <see cref="Control"/> in the UI thread where this instance
        /// may be used. This instance may also be used in any other thread that is MTA but cannot
        /// be used in an STA thread other than the one hosting this control.</param>
        /// <param name="spatialString">The <see cref="SpatialString"/> for which multi-threaded
        /// access is required.</param>
        [CLSCompliant(false)]
        public ThreadSafeSpatialString(Control uiControl, SpatialString spatialString)
        {
            try
            {
                _uiControl = uiControl;
                if (_uiControl.InvokeRequired)
                {
                    ExtractException.Assert("ELI32642",
                        "Cannot create a string in a STA thread other than the UI thread.",
                        Thread.CurrentThread.GetApartmentState() == ApartmentState.MTA);

                    _backgroundSpatialString = spatialString;
                }
                else
                {
                    _uiThreadSpatialString = spatialString;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32643");
            }
        }

        /// <summary>
        /// Gets a <see cref="SpatialString"/> instance usable in the calling thread.
        /// </summary>
        [CLSCompliant(false)]
        public SpatialString SpatialString
        {
            get
            {
                try
                {
                    if (_uiControl.InvokeRequired)
                    {
                        ExtractException.Assert("ELI32644",
                            "Cannot access string in STA thread other than the UI thread.",
                            Thread.CurrentThread.GetApartmentState() == ApartmentState.MTA);

                        if (_backgroundSpatialString == null)
                        {
                            // Construct a version of the SpatialString that can be used in a
                            // background thread.
                            lock (_lock)
                            {
                                if (_backgroundSpatialString == null)
                                {
                                    _backgroundSpatialString = BuildSpatialString();
                                }
                            }
                        }

                        return _backgroundSpatialString;
                    }
                    else
                    {
                        if (_uiThreadSpatialString == null)
                        {
                            // Construct a version of the SpatialString that can be used in the
                            // UI thread.
                            lock (_lock)
                            {
                                if (_uiThreadSpatialString == null)
                                {
                                    _uiThreadSpatialString = BuildSpatialString();
                                }
                            }
                        }

                        return _uiThreadSpatialString;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI32645");
                }
            }
        }

        /// <summary>
        /// Contructs a <see cref="SpatialString"/> instance usable by the calling thread.
        /// </summary>
        /// <returns>A <see cref="SpatialString"/> instance usable by the calling thread.</returns>
        SpatialString BuildSpatialString()
        {
            // We need the stringized data in order to create a new SpatialString instance for the
            // calling thread. Obtain it from the cached SpatialString created for the other
            // threading if necessary.
            if (_ocrData == null)
            {
                _ocrData = GetOCRData();
            }

            // Construct a SpatialString for each element in _ocrData and append them together to
            // obtain a unified result.
            SpatialString spatialString = new SpatialString();
            foreach (string ocrData in _ocrData.Where(data => !string.IsNullOrEmpty(data)))
            {
                spatialString.Append(
                        (SpatialString)MiscUtils.GetObjectFromStringizedByteStream(ocrData));
            }

            return spatialString;
        }

        /// <summary>
        /// Gets stringized <see cref="SpatialString"/> data using data from a previously cached
        /// <see cref="SpatialString"/> instance.
        /// </summary>
        /// <returns>The stringized SpatialString data.</returns>
        string[] GetOCRData()
        {
            string[] ocrData = null;

            // If we have a cached instance for the UI thread, use it to obtain the stringized
            // version.
            if (_uiThreadSpatialString != null)
            {
                // Must be done in the UI thread.
                FormsMethods.ExecuteInUIThread(_uiControl, () =>
                {
                    ocrData = new string[] 
                        {
                            MiscUtils.GetObjectAsStringizedByteStream(_uiThreadSpatialString)
                        };
                });
            }
            // If we have a cached instance for background MTA threads, use it to obtain the
            // stringized version.
            else if (_backgroundSpatialString != null)
            {
                ExtractException.Assert("ELI32646", "", !_uiControl.InvokeRequired);

                // Must be done in an MTA thread.
                Exception backgroundException = null;
                Thread backgroundThread = new Thread(new ThreadStart(() =>
                {
                    try
                    {
                        ocrData = new string[] 
                                {
                                    MiscUtils.GetObjectAsStringizedByteStream(_backgroundSpatialString)
                                };
                    }
                    catch (Exception ex)
                    {
                        backgroundException = ex;
                    }
                }));
                backgroundThread.Start();
                backgroundThread.Join();

                if (backgroundException != null)
                {
                    throw backgroundException;
                }
            }

            if (ocrData == null)
            {
                throw new ExtractException("ELI32647", "Failed to build OCR data");
            }

            return ocrData;
        }

        /// <summary>
        /// Gets a <see cref="MiscUtils"/> instance to user for stringizing and loading
        /// <see cref="SpatialString"/> instances. One of two instances will be returned
        /// depending on the thread caller is running in: either one that was was created in and
        /// is usable in the STA UI thread, or one that was created and is usable in a background
        /// MTA thread.
        /// </summary>
        MiscUtils MiscUtils
        {
            get
            {
                // If in a background MTA thread, get the background instance.
                if (_uiControl.InvokeRequired)
                {
                    if (_backgroundMiscUtils == null)
                    {
                        _backgroundMiscUtils = new MiscUtils();
                    }

                    return _backgroundMiscUtils;
                }
                // If in the UI STA thread, get the UI thread instance.
                else
                {
                    if (_uiThreadMiscUtils == null)
                    {
                        _uiThreadMiscUtils = new MiscUtils();
                    }

                    return _uiThreadMiscUtils;
                }
            }
        }
    }
}
