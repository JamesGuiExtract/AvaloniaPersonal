using Extract.FileConverter.Converters;
using NUnit.Framework;
using System.IO;
using System.Reflection;

namespace Extract.FileConverter.Test
{
    /// <summary>
    /// Provides test cases for the <see cref="DataEntryQuery"/> class.
    /// </summary>
    [TestFixture]
    [Category("TestLaunchArguments")]
    public class TestLaunchArguments
    {
        [Test, Category("Automated")]
        public static void ConvertDocumentToPdf()
        {
            string fileName = Path.GetTempFileName() + ".docx";
            try
            {
                WriteResourceToFile("Extract.FileConverter.Test.TestDocuments.VPNInstructions.docx", fileName);
                var converter = new WordToPdfConverter();
                converter.Convert(fileName);
                Assert.IsTrue(File.Exists(fileName.Replace(".docx", ".pdfx")));
            }
            finally
            {
                File.Delete(fileName);
                File.Delete(fileName.Replace(".docx", ".pdf"));
            }
        }

        public static void WriteResourceToFile(string resourceName, string fileName)
        {
            using var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            using var file = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            resource.CopyTo(file);
        }
    }
}