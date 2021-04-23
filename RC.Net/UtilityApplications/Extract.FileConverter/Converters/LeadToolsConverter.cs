using Extract.FileConverter.Converters.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Extract.FileConverter.Converters
{
    public class LeadtoolsConverter : IConverter
    {
        public LeadtoolsModel LeadtoolsModel { get; set; } = new LeadtoolsModel();

        public bool IsEnabled { get; set; }

        public string ConverterName { get => "LeadTools"; }

        public Collection<FileFormat> SupportedDestinationFormats => new Collection<FileFormat>() { FileFormat.Pdf, FileFormat.Tiff };

        private static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        /// <summary>
        /// Converts the input file to a PDF, and sets the name to the output file name.
        /// </summary>
        /// <param name="inputFile"></param>
        public void Convert(string inputFile, FileFormat destinationFileFormat)
        {
            string arguments = inputFile + " " 
                               + Path.ChangeExtension(inputFile, destinationFileFormat.AsString()) + " /" 
                               + destinationFileFormat.AsString();
            if(LeadtoolsModel.Retain.Equals(true))
            {
                arguments += " /retain";
            }
            if(LeadtoolsModel.PerspectiveID != -1)
            {
                arguments += " /vp " + LeadtoolsModel.PerspectiveID.ToString(CultureInfo.InvariantCulture);
            }
            if(!string.IsNullOrEmpty(LeadtoolsModel.RemovePages))
            {
                arguments += " /RemovePages " + LeadtoolsModel.RemovePages;
            }
            try
            {
                Utilities.SystemMethods.RunExtractExecutable(@$"{AssemblyDirectory}\ImageFormatConverter.exe", arguments);
            }
            catch(ExtractException ee)
            {
                //25307 means that the files must differ (IE trying to convert a tiff to a tiff). This behavior is fine
                if(!ee.EliCode.Equals("25307"))
                {
                    throw;
                }
            }
        }

        public IConverter Clone()
        {
            return new LeadtoolsConverter()
            {
                IsEnabled = this.IsEnabled,
                LeadtoolsModel = new LeadtoolsModel()
                {
                    PerspectiveID = this.LeadtoolsModel.PerspectiveID,
                    RemovePages = this.LeadtoolsModel.RemovePages,
                    Retain = this.LeadtoolsModel.Retain,
                } 
            };
        }
    }
}
