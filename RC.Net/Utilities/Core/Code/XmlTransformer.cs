using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Xsl;

namespace Extract.Utilities
{
    [CLSCompliant(false)]
    public class XmlTransformer
    {
        readonly XslCompiledTransform transform;

        /// <summary>
        /// The text encoding to be used for the output of the transform
        /// </summary>
        public Encoding OutputEncoding => transform.OutputSettings.Encoding;


        public XmlTransformer(string styleSheet)
        {
            try
            {
                transform = new XslCompiledTransform();
                transform.Load(new XmlTextReader(new StringReader(styleSheet)));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI50294");
            }
        }

        public XmlTransformer(string styleSheetPath, IPathTags pathTags)
            : this(LoadStyleSheet(styleSheetPath, pathTags))
        {
        }

        static string LoadStyleSheet(string styleSheetPath, IPathTags pathTags)
        {
            try
            {
                styleSheetPath = pathTags.Expand(styleSheetPath);
                return File.ReadAllText(styleSheetPath);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI50273");
            }
        }

        /// <summary>
        /// Attempt to read the XML encoding attribute
        /// </summary>
        /// <param name="input">The XML stream to read</param>
        /// <returns>Whether an encoding was read successfully</returns>
        static bool TryGetEncodingFromXmlAttribute(Stream input, out Encoding encoding)
        {
            var position = input.Position;
            try
            {
                var settings = new XmlReaderSettings
                {
                    ConformanceLevel = ConformanceLevel.Fragment,
                    CloseInput = false
                };

                using var reader = XmlReader.Create(input, settings);
                if (reader.Read())
                {
                    var encodingName = reader.GetAttribute("encoding");
                    if (!String.IsNullOrEmpty(encodingName))
                    {
                        encoding = Encoding.GetEncoding(encodingName);
                        return true;
                    }
                }

                encoding = Encoding.Default;
                return false;
            }
            finally
            {
                input.Position = position;
            }
        }

        /// <summary>
        /// Create a reader after determining the input encoding
        /// </summary>
        /// <param name="input">The xml stream to be read</param>
        /// <param name="defaultEncoding">Encoding to use if the encoding can't be detected from the input</param>
        static StreamReader GetStreamReader(Stream input, Encoding defaultEncoding)
        {
            var inputEncoding = defaultEncoding;

            if (TryGetEncodingFromXmlAttribute(input, out var encoding))
            {
                inputEncoding = encoding;
            }

            return new StreamReader(input,
                encoding: inputEncoding,
                detectEncodingFromByteOrderMarks: true,
                bufferSize: 1024,
                leaveOpen: true);
        }

        /// <summary>
        /// Transform an XML document
        /// </summary>
        /// <param name="input">The stream containing the XML document to be transformed</param>
        /// <param name="output">The stream to write the results to</param>
        public void TransformXml(Stream input, Stream output)
        {
            try
            {
                var inputSettings = new XmlReaderSettings();
                using var reader = GetStreamReader(input, defaultEncoding: Encoding.GetEncoding("Windows-1252"));
                using var xmlReader = XmlReader.Create(reader);
                transform.Transform(xmlReader, null, output);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI50272");
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public static class StyleSheets
        {
            /// <summary>
            /// An XSLT stylesheet that sorts XML alphabetically by node name, @Name, @FieldName
            /// and indents the output.
            /// </summary>
            /// <remarks>
            /// The preserve-space for FullText ensures that the value of the FullText node will
            /// be preserved when it is only whitespace.
            /// In my testing this stylesheet fails to indent XML properly if they don't use a FullText node.
            /// For this reason it would be slightly better to use a different stylesheet with neither indent="yes"
            /// nor &lt;:strip-space elements="*"/&gt; for XML that doesn't nest text in FullText nodes.
            /// </remarks>
            public static string AlphaSortName => @"
                <xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">
                    <xsl:output method=""xml"" omit-xml-declaration=""yes"" indent=""yes"" encoding=""windows-1252""/>
                    <xsl:strip-space elements=""*""/>
                    <xsl:preserve-space elements=""FullText""/>
                    <xsl:template match=""@*|node()"">
                        <xsl:copy>
                            <xsl:apply-templates select=""@*""/>
                            <xsl:apply-templates select=""node()"">
                                <xsl:sort select=""name()""/>
                                <xsl:sort select=""@Name""/>
                                <xsl:sort select=""@FieldName""/>
                            </xsl:apply-templates>
                        </xsl:copy>
                    </xsl:template>
                </xsl:stylesheet>";

            /// <summary>
            /// Same as <see cref="AlphaSortName"/> but with FullText nodes ordered before all other nodes
            /// </summary>
            public static string AlphaSortNameFullTextFirst => @"
                <xsl:stylesheet xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" version=""1.0"">
                    <xsl:output method=""xml"" omit-xml-declaration=""yes"" indent=""yes"" encoding=""windows-1252""/>
                    <xsl:strip-space elements=""*""/>
                    <xsl:preserve-space elements=""FullText""/>
                    <xsl:template match=""@*|node()"">
                        <xsl:copy>
                            <xsl:apply-templates select=""@*""/>
                            <xsl:apply-templates select=""FullText""/>
                            <xsl:apply-templates select=""node()[name() != 'FullText']"">
                                <xsl:sort select=""name()""/>
                                <xsl:sort select=""@Name""/>
                                <xsl:sort select=""@FieldName""/>
                            </xsl:apply-templates>
                        </xsl:copy>
                    </xsl:template>
                </xsl:stylesheet>";
        }
    }
}
