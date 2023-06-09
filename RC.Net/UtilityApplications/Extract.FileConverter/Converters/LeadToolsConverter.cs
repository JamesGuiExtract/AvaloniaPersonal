﻿using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Extract.FileConverter
{
    public sealed class LeadtoolsConverter : IConverter
    {
        public LeadtoolsModel LeadtoolsModel { get; set; } = new LeadtoolsModel();

        public bool IsEnabled { get; set; }

        [JsonIgnore]
        public string ConverterName => "LeadTools";

        [JsonIgnore]
        public Collection<DestinationFileFormat> SupportedDestinationFormats => new Collection<DestinationFileFormat>() { DestinationFileFormat.Pdf, DestinationFileFormat.Tif };

        [JsonIgnore]
        public bool HasDataError => LeadtoolsModel.HasDataError;

        /// <summary>
        /// Converts the input file to a PDF, and sets the name to the output file name.
        /// </summary>
        /// <param name="inputFile"></param>
        public void Convert(string inputFile, DestinationFileFormat destinationFileFormat)
        {
            try
            {
                string arguments = "\"" + inputFile + "\"" + " "
                               + "\"" + inputFile + "." + destinationFileFormat.EnumValue() + "\""
                               + " /" + destinationFileFormat.EnumValue();
                if (LeadtoolsModel.Retain.Equals(true))
                {
                    arguments += " /retain";
                }
                if (LeadtoolsModel.PerspectiveID != -1)
                {
                    arguments += " /vp " + LeadtoolsModel.PerspectiveID.ToString(CultureInfo.InvariantCulture);
                }
                if (!string.IsNullOrEmpty(LeadtoolsModel.RemovePages))
                {
                    arguments += " /RemovePages " + LeadtoolsModel.RemovePages;
                }

                Utilities.SystemMethods.RunExtractExecutable(@$"{Utilities.FileSystemMethods.CommonComponentsPath}\ImageFormatConverter.exe", arguments);
            }
            catch (Exception ee)
            {
                throw ee.AsExtract("ELI51718");
            }
        }

        ///<inheritdoc cref="IConverter"/>
        public IConverter Clone()
        {
            return new LeadtoolsConverter()
            {
                IsEnabled = IsEnabled,
                LeadtoolsModel = LeadtoolsModel.Clone()
            };
        }
    }
}
