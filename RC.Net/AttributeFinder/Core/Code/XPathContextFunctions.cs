using Extract.Imaging;
using Extract.Imaging.Utilities;
using Extract.Utilities;
using Leadtools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// Encapsulates custom functions to be used by <see cref="XPathContext"/>.
    /// </summary>
    class XPathContextFunctions : IXsltContextFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XPathContextFunctions"/> class.
        /// </summary>
        /// <param name="minArgs">The min args.</param>
        /// <param name="maxArgs">The max args.</param>
        /// <param name="returnType">Type of the return.</param>
        /// <param name="argTypes">The supplied XML Path Language (XPath) types for the function's
        /// argument list. This information can be used to discover the signature of the function
        /// which allows you to differentiate between overloaded functions.</param>
        /// <param name="functionName">The name of the function referenced by this instance.</param>
        /// <param name="context">The <see cref="XPathContext"/> to be used to resolve nodes to <see cref="UCLID_AFCORELib.IAttribute">attributes</see></param>
        public XPathContextFunctions(int minArgs, int maxArgs,
            XPathResultType returnType, XPathResultType[] argTypes, string functionName, XPathContext context)
        {
            try
            {
                Minargs = minArgs;
                Maxargs = maxArgs;
                ReturnType = returnType;
                ArgTypes = argTypes;
                FunctionName = functionName;
                Context = context;
            }
            catch (System.Exception ex)
            {
                throw ex.AsExtract("ELI39414");
            }
        }

        /// <summary>
        /// Gets the maximum number of arguments for the function. This enables the user to
        /// differentiate between overloaded functions.
        /// </summary>
        /// <returns>The maximum number of arguments for the function.</returns>
        public int Maxargs
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the minimum number of arguments for the function. This enables the user to
        /// differentiate between overloaded functions.
        /// </summary>
        /// <returns>The minimum number of arguments for the function.</returns>
        public int Minargs
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the <see cref="T:XPathResultType"/> representing the XPath type returned by the function.
        /// </summary>
        /// <returns>An <see cref="T:XPathResultType"/> representing the XPath type returned by the
        /// function.</returns>
        public XPathResultType ReturnType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the supplied XML Path Language (XPath) types for the function's argument list.
        /// This information can be used to discover the signature of the function which allows you
        /// to differentiate between overloaded functions.
        /// </summary>
        /// <returns>An array of <see cref="T:XPathResultType"/> representing the
        /// types for the function's argument list.</returns>
        public XPathResultType[] ArgTypes
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of the function referenced by this instance.
        /// </summary>
        /// <value>
        /// The name of the function referenced by this instance.
        /// </value>
        public string FunctionName
        {
            get;
            private set;
        }

        /// <summary>
        /// The <see cref="XPathContext"/> used to resolve nodes to <see cref="UCLID_AFCORELib.IAttribute">attributes</see>
        /// </summary>
        public XPathContext Context
        {
            get;
            private set;
        }

        /// <summary>
        /// Provides the method to invoke the function with the given arguments in the given context.
        /// </summary>
        /// <param name="xsltContext">The XSLT context for the function call.</param>
        /// <param name="args">The arguments of the function call. Each argument is an element in
        /// the array.</param>
        /// <param name="docContext">The context node for the function call.</param>
        /// <returns>
        /// An <see cref="T:System.Object"/> representing the return value of the function.
        /// </returns>
        public object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            try
            {
                if (FunctionName == XPathContext.LevenshteinFunction)
                {
                    // Translate node sets into strings. Return null if a node set is empty
                    var stringArgs = args.Select(a =>
                        {
                            var nodeIterator = a as XPathNodeIterator;
                            if (nodeIterator != null)
                            {
                                return nodeIterator.MoveNext()
                                    ? nodeIterator.Current.Value
                                    : null;
                            }
                            else if (a == null)
                            {
                                return null;
                            }
                            else
                            {
                                if (!typeof(string).IsInstanceOfType(a))
                                {
                                    var ue = new ExtractException("ELI39927",
                                        "Application trace: Unexpected argument to es:Levenshtein function");
                                    ue.AddDebugData("Type", a.GetType().ToString(), false);
                                    ue.Log();
                                }
                                return a.ToString();
                            }
                        }).ToArray();
                    if (stringArgs.Any(a => a == null))
                    {
                        return null;
                    }
                    return UtilityMethods.LevenshteinDistance(stringArgs[0], stringArgs[1]);
                }
                else if (FunctionName == XPathContext.BitmapFunction)
                {
                    if (args[0] is double width
                        && args[1] is double height
                        && args[2] is XPathNodeIterator nodeIterator)
                    {
                        if (nodeIterator.MoveNext())
                        {
                            var attr = (new XPathContext.XPathIterator(nodeIterator, Context)).CurrentAttribute;

                            if (args.Length == 3)
                            {
                                return InvokeBitmapFunction(Convert.ToInt32(width, CultureInfo.CurrentCulture),
                                                            Convert.ToInt32(height, CultureInfo.CurrentCulture),
                                                            attr);

                            }
                            else if (args.Length == 5
                                && args[3] is double minValue
                                && args[4] is double maxValue)
                            {
                                ExtractException.Assert("ELI45137", "Bad range", minValue < maxValue, "Min", minValue, "Max", maxValue);
                                return InvokeBitmapFunction(Convert.ToInt32(width, CultureInfo.CurrentCulture),
                                                            Convert.ToInt32(height, CultureInfo.CurrentCulture),
                                                            attr,
                                                            true,
                                                            minValue,
                                                            maxValue);
                            }
                            else
                            {
                                ThrowBadBitmapArgsException();
                            }
                        }
                    }
                    else
                    {
                        ThrowBadBitmapArgsException();
                    }

                    void ThrowBadBitmapArgsException()
                    {
                        var messages = new List<FormattableString>
                        {
                            $"Bad arguments. ",
                            $"Expected Double * Double * XPathSelectionIterator [ * Double * Double ], ",
                            $"received {args[0].GetType().Name} * {args[1].GetType().Name} * {args[2].GetType().Name}"
                        };
                        if (args.Length > 3)
                        {
                            messages.Add($" * {args[3].GetType().Name}");
                            if (args.Length > 4)
                            {
                                messages.Add($" * {args[4].GetType().Name}");
                            }
                        }
                        throw new ExtractException("ELI44669", UtilityMethods.FormatInvariant(messages.ToArray()));
                    }
                }

                return null;
            }
            catch (System.Exception ex)
            {
                throw ex.AsExtract("ELI39415");
            }
        }

        /// <summary>
        /// Invokes the es:Bitmap function to scale and encode an attribute's area as a bitmap
        /// </summary>
        /// <param name="width">The width of the resulting bitmap</param>
        /// <param name="height">The height of the resulting bitmap</param>
        /// <param name="attr">The attribute to be used to generate the bitmap</param>
        static string InvokeBitmapFunction(int width, int height, IAttribute attr)
        {
            return InvokeBitmapFunction(width, height, attr, false, 0, 0);
        }

        /// <summary>
        /// Invokes the es:Bitmap function to scale and encode an attribute's area as a bitmap
        /// </summary>
        /// <param name="width">The width of the resulting bitmap</param>
        /// <param name="height">The height of the resulting bitmap</param>
        /// <param name="attr">The attribute to be used to generate the bitmap</param>
        /// <param name="scaleIntensityRange">Whether to scale the output values to be in a new range</param>
        /// <param name="minValue">The minimum value of the output range</param>
        /// <param name="maxValue">The maximum value of the output range</param>
        static string InvokeBitmapFunction(int width, int height, IAttribute attr,
            bool scaleIntensityRange, double minValue, double maxValue)
        {
            try
            {
                // Attempt to unlock PDF support
                ExtractException ee = UnlockLeadtools.UnlockPdfSupport(returnExceptionIfUnlicensed: false);
                if (ee != null)
                {
                    throw ee;
                }

                RasterZone zone = null;
                var value = attr.Value;
                if (!value.HasSpatialInfo())
                {
                    return "";
                }

                double scaleFactor = 1;
                if (scaleIntensityRange)
                {
                    scaleFactor = (maxValue - minValue) / 255;
                }

                var zones = value.GetOriginalImageRasterZones();
                if (zones.Size() == 1)
                {
                    zone = new RasterZone((UCLID_RASTERANDOCRMGMTLib.RasterZone)zones.At(0));
                }
                else
                {
                    int pageNumber = value.GetFirstPageNumber();
                    var valueOnPage = value.GetSpecifiedPages(pageNumber, pageNumber);

                    LongRectangle rectangle = valueOnPage.GetOriginalImageBounds();
                    rectangle.GetBounds(out int left, out int top, out int right, out int bottom);
                    zone = new RasterZone(Rectangle.FromLTRB(left, top, right, bottom), pageNumber);
                }

                Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                var rect = new Rectangle(0, 0, width, height);

                using (var codecs = new ImageCodecs())
                using (var reader = codecs.CreateReader(value.SourceDocName))
                using (var probe = reader.CreatePixelProbe(zone.PageNumber))
                using (var g = Graphics.FromImage(bitmap))
                {
                    ZoneGeometry data = new ZoneGeometry(zone);
                    Bitmap sourceBitmap = data.GetZoneAsBitmap(probe);

                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.Clear(Color.White);
                    g.DrawImage(sourceBitmap, rect);
                }

                // Select the brightness of each pixel
                var bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);
                var numberOfBytes = bitmapData.Stride * height;
                var numberOfPixels = width * height;

                var ptr = bitmapData.Scan0;
                var bitmapBytes = new byte[numberOfBytes];
                System.Runtime.InteropServices.Marshal.Copy(ptr, bitmapBytes, 0, numberOfBytes);

                // Pixels are stored as sequential R, G, B
                var pixelValues = new byte[numberOfPixels];
                using (var r = new LeadtoolsGuard())
                {
                    for (int i = 0, j = 0; i < numberOfPixels; i++, j += 3)
                    {
                        pixelValues[i] = RasterHsvColor.FromRasterColor(
                            new RasterColor(bitmapBytes[j], bitmapBytes[j + 1], bitmapBytes[j + 2])).V;
                    }
                    bitmap.UnlockBits(bitmapData);
                }

                var pixelStrings = pixelValues.Select(b =>
                {
                    if (scaleIntensityRange)
                    {
                        return (b * scaleFactor + minValue).ToString(CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        return b.ToString(CultureInfo.InvariantCulture);
                    }
                });

                var formattedString = UtilityMethods.FormatInvariant($"Bitmap: {width} x {height} = ") +
                    string.Join(",", pixelStrings);

                return formattedString;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44671");
            }
        }
    }
}
