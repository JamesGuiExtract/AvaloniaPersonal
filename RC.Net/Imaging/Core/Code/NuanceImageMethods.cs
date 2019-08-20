using Nuance.OmniPage.CSDK.ArgTypes;
using Nuance.OmniPage.CSDK.CAPI;
using Nuance.OmniPage.CSDK.Objects;
using System;
using System.Collections.Generic;

namespace Extract.Imaging
{
    [CLSCompliant(false)]
    public static class NuanceImageMethods
    {
        static NuanceImageMethods()
        {
            try
            {
                Engine.SetLicenseKey(null, "9d478fe171d5");
                Engine.Init("Extract Systems", "Extract Systems");
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46861");
            }
        }

        /// <summary>
        /// Gets the page count.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public static int GetPageCount(string fileName)
        {
            IntPtr fileHandle = IntPtr.Zero;
            try
            {
                int pageCount = 0;

                RecAPI.kRecOpenImgFile(fileName, out fileHandle, FILEOPENMODE.IMGF_READ, IMF_FORMAT.FF_SIZE)
                    .ThrowOnError("ELI46862", "Unable to open image",
                        new KeyValuePair<string, string>("Image path", fileName));
                RecAPI.kRecGetImgFilePageCount(fileHandle, out pageCount)
                    .ThrowOnError("ELI46863", "Unable to obtain page count",
                        new KeyValuePair<string, string>("Image path", fileName));

                ExtractException.Assert("ELI46858", "Unable to retrieve page count.", pageCount > 0);

                return pageCount;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46857");
            }
            finally
            {
                if (fileHandle != IntPtr.Zero)
                {
                    try
                    {
                        RecAPI.kRecCloseImgFile(fileHandle).ThrowOnError("ELI47243", "Unable to close image file");
                    }
                    catch (Exception ex)
                    {
                        ex.ExtractLog("ELI47244");
                    }
                }
            }
        }

        /// <summary>
        /// Throws if fails.
        /// </summary>
        /// <param name="recApiMethod">The record API method.</param>
        /// <param name="eli">The eli.</param>
        /// <param name="message">The message.</param>
        /// <param name="debugData">The debug data.</param>
        public static void ThrowOnError(this RECERR rc, string eli, string message, params KeyValuePair<string, string>[] debugData)
        {
            try
            {
                if (rc != RECERR.REC_OK)
                {
                    RecAPI.kRecGetLastError(out int errExt, out string errStr);
                    RecAPI.kRecGetErrorUIText(rc, errExt, errStr, out string errUIText);
                    var uex = new ExtractException(eli, message);
                    uex.AddDebugData("Error code", rc, false);
                    uex.AddDebugData("Extended error code", errExt, false);
                    uex.AddDebugData("Error text", errUIText, false);
                    foreach (var kv in debugData)
                        uex.AddDebugData(kv.Key, kv.Value, false);
                    throw uex;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46856");
            }
        }
    }
}
