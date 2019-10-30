using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.AttributeFinder
{
    [CLSCompliant(false)]
    public static class SpatialStringMethods
    {
        /// <summary>
        /// Creates a spatial string from letters
        /// </summary>
        /// <param name="letters">Letters that define the string</param>
        /// <param name="sourceDocName">The source image name</param>
        /// <param name="pageInfos">The page info for at least each page of the source image that is referenced by the letters</param>
        public static SpatialString CreateFromLetters(IEnumerable<LetterStruct> letters, string sourceDocName, LongToObjectMap pageInfos)
        {
            try
            {
                var letterBuffer = letters.ToArray();
                var uss = new SpatialStringClass();
                unsafe
                {
                    fixed (LetterStruct* ptr = letterBuffer)
                    {
                        uss.CreateFromLetterArray(letterBuffer.Length, (IntPtr)ptr, sourceDocName, pageInfos);
                    }
                }
                uss.ReportMemoryUsage();
                return uss;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49502");
            }
        }

        /// <summary>
        /// Enumerate the spatial page info map of a <see cref="SpatialString"/>
        /// </summary>
        /// <param name="spatialString">The map to enumerate</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static IEnumerable<KeyValuePair<int, SpatialPageInfo>> EnumerateSpatialPageInfos(this ISpatialString spatialString)
        {
            return spatialString.SpatialPageInfos.ToIEnumerable<SpatialPageInfo>();
        }
    }
}
