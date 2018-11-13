using Extract;
using Extract.Licensing;
using System;
using System.IO;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;
using UCLID_SSOCRLib;

namespace CreateHtmlFromImage
{
    class CreateHtmlFromImage
    {
        public static void Process(string imagePath, bool html40, string parametersFile)
        {
            try
            {
            // Create the OCR COM object
            ScansoftOCRClass ssocr = new ScansoftOCRClass();

            // Initlialize the private license
            ssocr.InitPrivateLicense(
                LicenseUtilities.GetMapLabelValue(new MapLabel()));

            // Load the OCR parameters from the file
            IOCRParameters ocrParameters = null;
            if (!string.IsNullOrEmpty(parametersFile))
            {
                ILoadOCRParameters loadOCRParameters = new RuleSetClass();
                loadOCRParameters.LoadOCRParameters(parametersFile);
                ocrParameters = ((IHasOCRParameters)loadOCRParameters).OCRParameters;
            }

            var outputDir = Path.Combine(Path.GetDirectoryName(imagePath), html40 ? "4.0" : "3.2");
            var outputFile = Path.Combine(outputDir, Path.ChangeExtension(Path.GetFileName(imagePath), "html"));
            ssocr.CreateOutputImage(
                imagePath,
                html40 ? "Converters.Text.Html40" : "Converters.Text.Html32",
                outputFile,
                ocrParameters);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46463");
            }
        }
    }
}
