using Extract;
using Extract.AttributeFinder;
using Extract.Licensing;
using Extract.Redaction;
using Extract.Utilities;
using RedactionPredictor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace PredictionEvaluator
{
    partial class Program
    {
        static int Main(string[] argv)
        {
            int usage(bool error = false)
            {
                var message =
                "Evaluates a predictions VOA wrt a post-verification VOA.\r\n" +
                "Runs training on the attribute categorizer associated with the templates.\r\n" +
                "Creates or updates a template if needed.\r\n" +
                "Usage:\r\n" +
                "  PredictionEvaluator <templateLibrary> <imagePath> <predictionsVoaPath> <postVerificationVoaPath> [-x] [-t]" +
                "    templateLibrary is a zip or .etf file (created if missing) that holds the template zone files" +
                "    imagePath is the image to evaluate" +
                "    predictionsVoa is the post-rules VOA file" +
                "    postVerificationVoaPath is the expected data VOA file" +
                "    -x means to only consider expected attributes that have an exemption code applied to them" +
                "    -t means to train an attributeClassifier.lm file in the same dir as the templateLibrary for spatially correct attributes";
                UtilityMethods.ShowMessageBox(message, "PredictionEvaluator Usage", error);

                return error ? -1 : 0;
            }

            try
            {
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());

                List<string> args = new List<string>(argv.Length);
                bool requireExemptionCodeSubattribute = false;
                bool trainAttributeClassifier = false;
                for (int i = 0; i < argv.Length; i++)
                {
                    if (string.Equals(argv[i], "-x", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(argv[i], "/x", StringComparison.OrdinalIgnoreCase))
                    {
                        requireExemptionCodeSubattribute = true;
                    }
                    else if (string.Equals(argv[i], "-t", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(argv[i], "/t", StringComparison.OrdinalIgnoreCase))
                    {
                        trainAttributeClassifier = true;
                    }
                    else
                    {
                        args.Add(argv[i]);
                    }
                }

                if (args.Count < 4)
                {
                    Console.Error.WriteLine("Not enough args");
                    return usage(error: true);
                }

                var templateLibrary = Path.GetFullPath(args[0]);
                var imagePath = Path.GetFullPath(args[1]);
                var predictionsVoaPath = Path.GetFullPath(args[2]);
                var postVerificationVoaPath = Path.GetFullPath(args[3]);

                var afutil = new AFUtilityClass();
                var predictionsAttributes = afutil.GetAttributesFromFile(predictionsVoaPath).ToIEnumerable<IAttribute>();
                var postVerificationAttributes = afutil.GetAttributesFromFile(postVerificationVoaPath).ToIEnumerable<IAttribute>();

                foreach (var predictorTurnedOff in predictionsAttributes.Where(a => a.Name.StartsWith("_LM_")))
                {
                    predictorTurnedOff.Name = predictorTurnedOff.Name.Substring(4);
                }

                var filteredPredictions = predictionsAttributes.Where(a => a.Value.HasSpatialInfo() && !a.Name.StartsWith("_")).ToList();
                var filteredPostVerification = postVerificationAttributes
                    .Where(a => a.Value.HasSpatialInfo()
                        && !a.Name.StartsWith("_")
                        && (!requireExemptionCodeSubattribute || afutil.QueryAttributes(a.SubAttributes, "ExemptionCodes", false).Size() > 0))
                    .ToList();

                var misses = Subtract(filteredPostVerification, filteredPredictions, overlapThreshold: 0.1);
                // Don't use filtered list for extras calculation because there are more than just template finder attributes present in the predictions list
                var extras = Subtract(filteredPredictions, postVerificationAttributes, overlapThreshold: 0.1);
                var spatiallyCorrect = filteredPredictions.Where(a => !(misses.Contains(a) || extras.Contains(a))).ToList();

                // Train machine
                if (trainAttributeClassifier)
                {
                    TrainMachine(filteredPostVerification, spatiallyCorrect, extras, imagePath, afutil, Path.GetDirectoryName(templateLibrary));
                }

                var potentialPredictions = predictionsAttributes.Where(a => a.Value.HasSpatialInfo() && a.Name == "_FormField");
                var missesThatExistOnForm = Intersect(misses, potentialPredictions, 0.1);
                var completeMisses = misses.Where(a => !missesThatExistOnForm.Contains(a)).ToList();

                // Make new template
                if (completeMisses.Any())
                {
                    var missesGroupedByPage = completeMisses.GroupBy(a => a.Value.GetFirstPageNumber());
                    var allExpectedRedactionsLookupByPage = filteredPostVerification.ToLookup(a => a.Value.GetFirstPageNumber());
                    foreach (var group in missesGroupedByPage)
                    {
                        var pageNum = group.Key;
                        var expectedRedactions = allExpectedRedactionsLookupByPage[pageNum].ToIUnknownVector();
                        Templates.CreateTemplate(imagePath, pageNum, expectedRedactions, templateLibrary);
                    }
                }
                // Update existing template
                else
                {
                }
            
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI44686");
                Console.Error.WriteLine("Error occurred");
                return -1;
            }

            return 0;
        }

        private static void TrainMachine(IEnumerable<IAttribute> filteredPostVerification, IEnumerable<IAttribute> spatiallyCorrect,
            IEnumerable<IAttribute> extras, string imagePath, AFUtilityClass afutil, string templateDir)
        {
            // Update predictions with correct labels
            foreach (var a in spatiallyCorrect)
            {
                var expected = filteredPostVerification.FirstOrDefault(b => IsSpatialMatch(a.Value, b.Value, 0.1));
                if (expected != null)
                {
                    var expectedClass = expected.Type;
                    SetSubattribute(a, "AttributeType", expectedClass, "", afutil, imagePath);
                }
            }

            // Update predictions that were deleted with empty label
            foreach (var a in extras)
            {
                SetSubattribute(a, "AttributeType", "", "", afutil, imagePath);
            }

            var trainingData = spatiallyCorrect.Concat(extras);

            if (trainingData.Any())
            {
                var attributeClassifierPath = Path.Combine(templateDir, "attributeClassifier.lm");
                LearningMachine lm = null;
                if (File.Exists(attributeClassifierPath))
                {
                    lm = LearningMachine.Load(attributeClassifierPath);
                }
                else
                {
                    lm = new LearningMachine
                    {
                        InputConfig = new InputConfiguration { InputPath = "PLACEHOLDER", AttributesPath = "PLACEHOLDER" },
                        Classifier = new NeuralNetworkClassifier { HiddenLayers = new[] { 50 }, MaxTrainingIterations = 1 },
                        Encoder = new LearningMachineDataEncoder(LearningMachineUsage.AttributeCategorization, attributeFilter: "*@Feature", negativeClassName: "")
                    };
                }

                var ann = lm.Classifier as NeuralNetworkClassifier;
                if (ann != null)
                {
                    ann.MaxTrainingIterations = 1;
                }

                var spatialString = new SpatialStringClass();
                if (File.Exists(imagePath + ".uss"))
                {
                    spatialString.LoadFrom(imagePath + ".uss", false);
                }

                if (!lm.IsTrained)
                {
                    // Create examples for each redaction type to initialize the machine
                    var iniSettings = new InitializationSettings();
                    var types = iniSettings.GetRedactionTypes().Concat(Enumerable.Repeat("", 1));
                    var fakedTrainingData = trainingData.SelectMany(a =>
                        types.Select(t =>
                        {
                            var clone = (IAttribute) ((ICopyableObject)a).Clone();
                            SetSubattribute(clone, "AttributeType", t, "", afutil, imagePath);
                            SetSubattribute(clone, "TypeFromTemplate", t, "Feature", afutil, imagePath);

                            return clone;
                        })).ToList();
                    lm.IncrementallyTrainMachine(spatialString, fakedTrainingData.ToIUnknownVector(), null);

                    // Set training iterations to higher value for first training with real data 
                    if (ann != null)
                    {
                        ann.MaxTrainingIterations = 5;
                    }
                }

                foreach(var attribute in trainingData)
                {
                    // Create a TypeFromTemplate feature the attribute's type
                    var typeSpatialString = new SpatialStringClass();
                    typeSpatialString.CreateNonSpatialString(attribute.Type, imagePath);
                    var typeAttribute = new AttributeClass
                    {
                        Name = "TypeFromTemplate",
                        Type = "Feature",
                        Value = typeSpatialString
                    };
                    attribute.SubAttributes.PushBack(typeAttribute);
                }

                lm.IncrementallyTrainMachine(spatialString, trainingData.ToIUnknownVector(), null);
                try
                {
                    lm.Save(attributeClassifierPath);
                }
                catch (Exception ex)
                {
                    ex.AsExtract("ELI44725").Log();
                    Console.Error.WriteLine("Error saving classifier after training");
                }
            }
        }

        private static void SetSubattribute(IAttribute a, string subattributeName, string value, string type, AFUtility afutil, string sourceDocName)
        {
            var res = afutil.QueryAttributes(a.SubAttributes, subattributeName, true);
            var subAttribute = res.Size() > 0 ? (IAttribute)res.At(0) : null;
            if (subAttribute == null)
            {
                subAttribute = new AttributeClass { Name = subattributeName, Type = type };
            }
            subAttribute.Value.CreateNonSpatialString(value, sourceDocName);
            a.SubAttributes.PushBack(subAttribute);
        }

        private static IEnumerable<IAttribute> Subtract(IEnumerable<IAttribute> minuend, IEnumerable<IAttribute> subtrahend, double overlapThreshold)
        {
            return minuend.Where(e => !subtrahend.Any(f => IsSpatialMatch(e.Value, f.Value, overlapThreshold))).ToList();
        }

        private static IEnumerable<IAttribute> Intersect(IEnumerable<IAttribute> A, IEnumerable<IAttribute> B, double overlapThreshold)
        {
            return A.Where(a => B.Any(b => IsSpatialMatch(a.Value, b.Value, overlapThreshold))).ToList();
        }

        private static bool IsSpatialMatch(SpatialString exp, SpatialString found, double overlapThreshold)
        {
            if (!(exp.HasSpatialInfo() && found.HasSpatialInfo()))
                return false;
            else
            {
                var expZones = exp.GetOCRImageRasterZones().ToIEnumerable<RasterZone>();
                var foundZones = found.GetOCRImageRasterZones().ToIEnumerable<RasterZone>();
                return expZones.Any(e =>
                    foundZones.Any(f =>
                    {
                        var overlapArea = e.GetAreaOverlappingWith(f);
                        var expArea = e.Area;
                        var overlapRatio = overlapArea / expArea;
                        return overlapRatio >= overlapThreshold;
                    }));
            }
        }
    }
}
