using Accord.MachineLearning.VectorMachines;
using Accord.Math;
using AForge.Neuro;
using Extract.AttributeFinder;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using ZstdNet;

namespace LearningMachineTrainer
{
    /// <summary>
    /// Different accuracy measures used to score a classifier
    /// </summary>
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/Extract.AttributeFinder")]
    public enum MachineScoreType
    {
        [EnumMember]
        OverallAgreementOrF1 = 0,
        [EnumMember]
        Precision = 1,
        [EnumMember]
        Recall = 2,
    }

    [Obfuscation(Feature = "renaming", Exclude = true)]
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/Extract.AttributeFinder")]
    public class LearningMachineDataEncoderModel : ILearningMachineDataEncoderModel
    {
        [DataMember(Name = "_answerCodeToNameList")]
        public List<string> AnswerCodeToName { get; set; }

        public Dictionary<string, int> AnswerNameToCode { get; set; } = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        [DataMember(Name = "_negativeClassName")]
        public string NegativeClassName { get; set; }

        /// <summary>
        /// Called when deserialized
        /// </summary>
        /// <param name="context">The context.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            AnswerNameToCode = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < AnswerCodeToName.Count; i++)
            {
                AnswerNameToCode[AnswerCodeToName[i]] = i;
            }
        }
    }

    [Obfuscation(Feature = "renaming", Exclude = true)]
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/Extract.AttributeFinder")]
    public class LearningMachineModel : ILearningMachineModel
    {
        [DataMember(Name = "_version")]
        public int Version { get; set; }

        [DataMember(Name = "_classifier")]
        public IClassifierModel Classifier { get; set; }

        [DataMember(Name = "_encoder")]
        public ILearningMachineDataEncoderModel Encoder { get; set; }

        public List<string> AnswerCodeToName { get => Encoder.AnswerCodeToName; set => Encoder.AnswerCodeToName = value; }

        [DataMember(Name = "_trainingLog")]
        public string TrainingLog { get; set; }

        [DataMember(Name = "_accuracyData")]
        public (SerializableConfusionMatrix train, SerializableConfusionMatrix test)? AccuracyData { get; set; }

        [DataMember(Name = "_randomNumberSeed")]
        public int RandomNumberSeed { get; set; }

        public Dictionary<string, int> AnswerNameToCode => Encoder.AnswerNameToCode;

        public string NegativeClassName => Encoder.NegativeClassName;

        [DataMember(Name = "_useUnknownCategory")]
        public bool UseUnknownCategory { get; set; }

        [DataMember(Name = "_unknownCategoryCutoff")]
        public double UnknownCategoryCutoff { get; set; }

        [DataMember(Name = "_translateUnknownCategory")]
        public bool TranslateUnknownCategory { get; set; }

        [DataMember(Name = "_translateUnknownCategoryTo")]
        public string TranslateUnknownCategoryTo { get; set; }

        public static ILearningMachineModel LoadClassifier(string path)
        {
            using (var decompressor = new Decompressor())
            {
                var bytes = File.ReadAllBytes(path);
                byte[] uncompressedBytes = null;
                if (Encoding.ASCII.GetString(bytes.Take(16).ToArray())
                    .Equals("<LearningMachine", StringComparison.Ordinal))
                {
                    uncompressedBytes = bytes;
                }
                else
                {
                    uncompressedBytes = decompressor.Unwrap(bytes);
                }
                using (var stream = new MemoryStream(uncompressedBytes))
                {
                    var subStream = TranslateToThisAssembly(stream);
                    var serializer = new NetDataContractSerializer();
                    serializer.AssemblyFormat = FormatterAssemblyStyle.Simple;
                    var classifier = (ILearningMachineModel)serializer.Deserialize(subStream);

                    return classifier;
                }
            }
        }

        /// <summary>
        /// Called when deserialized
        /// </summary>
        /// <param name="context">The context.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (Version < 6)
            {
                throw new Exception("This application requires version 6 or greater LearningMachine");
            }
        }

        public static void SaveClassifierIntoLearningMachine(ILearningMachineModel model, string path)
        {
            var doc = new XmlDocument();

            using (var decompressor = new Decompressor())
            {
                var bytes = File.ReadAllBytes(path);
                byte[] uncompressedBytes = null;
                if (Encoding.ASCII.GetString(bytes.Take(16).ToArray())
                    .Equals("<LearningMachine", StringComparison.Ordinal))
                {
                    uncompressedBytes = bytes;
                }
                else
                {
                    uncompressedBytes = decompressor.Unwrap(bytes);
                }
                using (var stream = new MemoryStream(uncompressedBytes))
                {
                    doc.Load(stream);
                    var nsmgr = new XmlNamespaceManager(doc.NameTable);
                    nsmgr.AddNamespace("af", "http://schemas.datacontract.org/2004/07/Extract.AttributeFinder");
                    nsmgr.AddNamespace("z", "http://schemas.microsoft.com/2003/10/Serialization/");

                    void ReplaceChildren(XmlElement destination, object source, bool onlyMatching)
                    {
                        // Calculate max serialization id (need to avoid collisions)
                        XElement e = XElement.Load(new XmlNodeReader(doc));
                        XNamespace ser = e.GetNamespaceOfPrefix("z");
                        var maxId = e.DescendantNodesAndSelf()
                            .OfType<XElement>()
                            .Where(l => l.Attribute(ser + "Id") != null)
                            .Max(l => (int)l.Attribute(ser + "Id"));

                        // Save source to xml streem
                        var serializer = new NetDataContractSerializer();
                        using (var newStream = new MemoryStream())
                        {
                            serializer.Serialize(newStream, source);
                            newStream.Position = 0;

                            // Load it as Xml and copy the children
                            var newDoc = new XmlDocument();
                            newDoc.Load(newStream);

                            // Update the serialization IDs and Refs to be greater than any existing IDs
                            newDoc = UpdateIDs(newDoc, maxId);

                            if (onlyMatching)
                            {
                                var children = newDoc.SelectNodes("/*/*[not(starts-with(@z:Type, 'LearningMachineTrainer'))]", nsmgr);
                                foreach (XmlNode child in children)
                                {
                                    var destinationChild = destination.SelectSingleNode("af:"+child.LocalName, nsmgr);
                                    if (destinationChild != null)
                                    {
                                        var clone = doc.ImportNode(child, true);
                                        destination.ReplaceChild(clone, destinationChild);
                                    }
                                }
                            }
                            else
                            {
                                // Clear the destination of children and add the source children instead
                                destination.IsEmpty = true;
                                foreach (XmlNode child in newDoc.FirstChild.ChildNodes)
                                {
                                    var clone = doc.ImportNode(child, true);
                                    destination.AppendChild(clone);
                                }
                            }
                        }
                    }
                    var lm = (XmlElement)doc.SelectSingleNode("/*", nsmgr);
                    ReplaceChildren(lm, model, true);

                    var classifier = (XmlElement)doc.SelectSingleNode("/*/af:_classifier", nsmgr);
                    ReplaceChildren(classifier, model.Classifier, false);

                    var encoder = (XmlElement)doc.SelectSingleNode("/*/af:_encoder", nsmgr);
                    ReplaceChildren(encoder, model.Encoder, true);
                }
            }

            // Save machine to destination
            string tempFile = null;
            try
            {
                XmlWriterSettings settings = new XmlWriterSettings
                {
                    Indent = true,

                    // This is needed in order to preserve \r\n newline sequence for the training log
                    NewLineHandling = NewLineHandling.Entitize
                };

                // Save to a temporary file
                tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".tmp");
                using (var fstream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var stream = new MemoryStream())
                using (var options = new CompressionOptions(9))
                using (var compressor = new Compressor(options))
                using (var writer = XmlWriter.Create(stream, settings))
                {
                    doc.WriteTo(writer);
                    writer.Flush();
                    var bytes = compressor.Wrap(stream.ToArray());
                    fstream.Write(bytes, 0, bytes.Length);
                }

                // Once the save process is complete, copy the file into the real destination.
                File.Copy(tempFile, path, true);
            }
            finally
            {
                if (tempFile != null)
                {
                    File.Delete(tempFile);
                }
            }
        }

        static Stream TranslateToThisAssembly(Stream input)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(input);
            var styleSheet =
            @"<xsl:stylesheet version=""1.0"" xmlns:xsl=""http://www.w3.org/1999/XSL/Transform""
                   xmlns:z=""http://schemas.microsoft.com/2003/10/Serialization/""
                   xmlns:af=""http://schemas.datacontract.org/2004/07/Extract.AttributeFinder"">
                   <xsl:output method=""xml"" omit-xml-declaration=""yes"" indent=""no""/>

                   <!-- identity transform -->
                   <xsl:template match=""@*|node()"">
                       <xsl:copy>
                           <xsl:apply-templates select=""@*|node()""/>
                       </xsl:copy>
                   </xsl:template>

                   <!-- override for z:Assembly attributes -->
                   <xsl:template match=""@z:Assembly[starts-with(., 'Extract.AttributeFinder')]"">
                     <xsl:attribute name=""{name()}"">
                       <xsl:value-of select=""'LearningMachineTrainer'""/>
                     </xsl:attribute>
                   </xsl:template>

                   <xsl:template match=""@z:Type[.='Extract.AttributeFinder.LearningMachine']"">
                     <xsl:attribute name=""{name()}"">
                       <xsl:value-of select=""'LearningMachineTrainer.LearningMachineModel'""/>
                     </xsl:attribute>
                   </xsl:template>


                   <!-- selectively copy _encoder -->

                   <xsl:template match=""/*/af:_encoder"">
                     <xsl:element name=""{local-name()}"" namespace=""http://schemas.datacontract.org/2004/07/Extract.AttributeFinder"">
                         <xsl:attribute name=""z:Id"">
                           <xsl:value-of select=""@z:Id""/>
                         </xsl:attribute>
                         <xsl:attribute name=""z:Assembly"">
                           <xsl:value-of select=""'LearningMachineTrainer'""/>
                         </xsl:attribute>
                         <xsl:attribute name=""z:Type"">
                           <xsl:value-of select=""'LearningMachineTrainer.LearningMachineDataEncoderModel'""/>
                         </xsl:attribute>
                        <xsl:apply-templates select=""af:_negativeClassName|af:_answerCodeToNameList""/>
                     </xsl:element>
                   </xsl:template>


                   <!-- change types of the classifier objects -->

                   <xsl:template match=""@z:Type[.='Extract.AttributeFinder.MulticlassSupportVectorMachineClassifier']"">
                     <xsl:attribute name=""{name()}"">
                       <xsl:value-of select=""'LearningMachineTrainer.MulticlassSupportVectorMachineModel'""/>
                     </xsl:attribute>
                   </xsl:template>

                   <xsl:template match=""@z:Type[.='Extract.AttributeFinder.MultilabelSupportVectorMachineClassifier']"">
                     <xsl:attribute name=""{name()}"">
                       <xsl:value-of select=""'LearningMachineTrainer.MultilabelSupportVectorMachineModel'""/>
                     </xsl:attribute>
                   </xsl:template>

                   <xsl:template match=""@z:Type[.='Extract.AttributeFinder.NeuralNetworkClassifier']"">
                     <xsl:attribute name=""{name()}"">
                       <xsl:value-of select=""'LearningMachineTrainer.NeuralNetModel'""/>
                     </xsl:attribute>
                   </xsl:template>

            </xsl:stylesheet>";

            var xslTrans = new XslCompiledTransform();
            var output = new MemoryStream();
            xslTrans.Load(new XmlTextReader(new StringReader(styleSheet)));
            xslTrans.Transform(xmlDoc, null, output);
            output.Position = 0;

            return output;
        }

        static XmlDocument UpdateIDs(XmlDocument input, int startId)
        {
            string offset = startId.ToString(CultureInfo.InvariantCulture);
            var styleSheet =
            @"<xsl:stylesheet version=""1.0"" xmlns:xsl=""http://www.w3.org/1999/XSL/Transform""
                   xmlns:z=""http://schemas.microsoft.com/2003/10/Serialization/"">
                   <xsl:output method=""xml"" omit-xml-declaration=""yes"" indent=""yes""/>

                   <!-- identity transform -->
                   <xsl:template match=""@*|node()"">
                     <xsl:copy>
                       <xsl:apply-templates select=""@*|node()""/>
                     </xsl:copy>
                   </xsl:template>

                   <!-- Add offset to all Id and Ref attributes -->
                   <xsl:template match=""@z:Id|@z:Ref"">
                     <xsl:attribute name=""{name()}"">
                       <xsl:value-of select=""current() + " + offset + @"""/>
                     </xsl:attribute>
                   </xsl:template>
            </xsl:stylesheet>";

            var xslTrans = new XslCompiledTransform();
            var output = new MemoryStream();
            xslTrans.Load(new XmlTextReader(new StringReader(styleSheet)));
            xslTrans.Transform(input, null, output);
            output.Position = 0;
            var doc = new XmlDocument();
            doc.Load(output);

            return doc;
        }
    }

    [Obfuscation(Feature = "renaming", Exclude = true)]
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/Extract.AttributeFinder")]
    public class NeuralNetModel : INeuralNetModel
    {
        [DataMember(Name = "_featureMean")]
        public double[] FeatureMean { get; set; }

        [DataMember(Name = "_featureScaleFactor")]
        public double[] FeatureScaleFactor { get; set; }

        [DataMember(Name = "_featureVectorLength")]
        public int FeatureVectorLength { get; set; }

        [DataMember(Name = "_hiddenLayers")]
        int[] _hiddenLayers;

        public IEnumerable<int> HiddenLayers
        {
            get => _hiddenLayers;
            set => _hiddenLayers = value.ToArray();
        }

        [DataMember(Name = "_maxTrainingIterations")]
        public int MaxTrainingIterations { get; set; }

        [DataMember(Name = "_numberOfCandidateNetworksToBuild")]
        public int NumberOfCandidateNetworksToBuild { get; set; }

        [DataMember(Name = "_sigmoidAlpha")]
        public double SigmoidAlpha { get; set; }

        [DataMember(Name = "_useCrossValidationSets")]
        public bool UseCrossValidationSets { get; set; }

        [DataMember(Name = "_numberOfClasses")]
        public int NumberOfClasses { get; set; }

        [DataMember(Name = "_isTrained")]
        public bool IsTrained { get; set; }

        [DataMember(Name = "_lastTrainedOn")]
        public DateTime LastTrainedOn { get; set; }

        [DataMember(Name = "_classifier")]
        public ActivationNetwork Classifier { get; set; }

        [DataMember(Name = "_negativeToPositiveWeightRatio")]
        public double? NegativeToPositiveWeightRatio { get; set; }
    }

    [Obfuscation(Feature = "renaming", Exclude = true)]
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/Extract.AttributeFinder")]
    class SupportVectorMachineModel : ISupportVectorMachineModel
    {
        [DataMember(Name = "FeatureMean")]
        public double[] FeatureMean { get; set; }

        [DataMember(Name = "FeatureScaleFactor")]
        public double[] FeatureScaleFactor { get; set; }

        [DataMember(Name = "_automaticallyChooseComplexityValue")]
        public bool AutomaticallyChooseComplexityValue { get; set; }

        [DataMember(Name = "_classifier")]
        public ISupportVectorMachine Classifier { get; set; }

        [DataMember(Name = "_complexity")]
        public double Complexity { get; set; }

        [DataMember(Name = "_conditionallyApplyWeightRatio")]
        public bool ConditionallyApplyWeightRatio { get; set; }

        [DataMember(Name = "_featureVectorLength")]
        public int FeatureVectorLength { get; set; }

        [DataMember(Name = "_isTrained")]
        public bool IsTrained { get; set; }

        [DataMember(Name = "_lastTrainedOn")]
        public DateTime LastTrainedOn { get; set; }

        [DataMember(Name = "_numberOfClasses")]
        public int NumberOfClasses { get; set; }

        [DataMember(Name = "_positiveToNegativeWeightRatio")]
        public double? PositiveToNegativeWeightRatio { get; set; }

        [DataMember(Name = "_scoreTypeToUseForComplexityChoosingAlgorithm")]
        public MachineScoreType ScoreTypeToUseForComplexityChoosingAlgorithm { get; set; }

        [DataMember(Name = "_smoCacheSize")]
        public int? TrainingAlgorithmCacheSize { get; set; }

        [DataMember(Name = "_version")]
        public int Version { get; set; }

        [DataMember(Name = "_calibrateMachineToProduceProbabilities")]
        public bool CalibrateMachineToProduceProbabilities { get; set; }

        [DataMember(Name = "_useClassProportionsForComplexityWeights")]
        public bool UseClassProportionsForComplexityWeights { get; set; }
    }

    [Obfuscation(Feature = "renaming", Exclude = true)]
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/Extract.AttributeFinder")]
    class MulticlassSupportVectorMachineModel : SupportVectorMachineModel, IMulticlassSupportVectorMachineModel
    {
    }

    [Obfuscation(Feature = "renaming", Exclude = true)]
    [DataContract(Namespace = "http://schemas.datacontract.org/2004/07/Extract.AttributeFinder")]
    class MultilabelSupportVectorMachineModel : SupportVectorMachineModel, IMultilabelSupportVectorMachineModel
    {
    }
}
