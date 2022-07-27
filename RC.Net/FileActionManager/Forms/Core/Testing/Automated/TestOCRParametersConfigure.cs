using Extract.Testing.Utilities;
using Extract.Utilities;
using Nuance.OmniPage.CSDK;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;
using UCLID_SSOCR2Lib;

namespace Extract.FileActionManager.Forms.Test
{
    /// <summary>
    /// Class for testing <see cref="OCRParametersConfigure"/>
    /// </summary>
    [TestFixture]
    [Category("TestOCRParametersConfigure")]
    public class TestOCRParametersConfigure
    {
        #region Fields

        static IHasOCRParameters _hasParameters;

        #endregion Fields

        #region TestSetup

        /// <summary>
        /// Performs initialization needed for the entire test run.
        /// </summary>
        [OneTimeSetUp]
        public static void Initialize()
        {
            GeneralMethods.TestSetup();

            _hasParameters = new SpatialStringClass();
        }

        #endregion TestSetup

        #region Tests

        /// <summary>
        /// Tests the settings after passing null to SetOCRParameters so that settings are from the registry
        /// </summary>
        /// <remarks>
        /// The assertion may fail if non-default registry settings are used
        /// </remarks>
        [Test, Category("Automated")]
        public static void TestNone()
        {
            string expectedSettings = Regex.Replace(
                @";PART
                Kernel.Chr.CodePage = Windows ANSI
                Kernel.Chr.Rejected = 94
                Kernel.Imf.PDF.Resolution = 300
                Kernel.Img.Max.Pix.X = 32000
                Kernel.Img.Max.Pix.Y = 32000
                Kernel.OcrMgr.PreferAccurateEngine = TRUE

                AssignSpatialInfoToSpaceCharacters = 0
                EnableDespeckleMode = 1
                ForceDespeckle = 0
                IgnoreParagraphFlag = 0
                LimitToBasicLatinCharacters = 1
                MaxOcrPageFailureNumber = 10
                MaxOcrPageFailurePercentage = 25
                OrderZones = 1
                OutputMultipleSpaceCharacterSequences = 0
                OutputOneSpaceCharacterPerCount = 0
                OutputTabCharactersForTabSpaceType = 0
                PrimaryDecompositionMethod = 0
                RequireOnePageSuccess = 0
                SkipPageOnFailure = 0
                TimeoutLength = 120000
                TreatZonesAsParagraphs = 0
                OCRFindType = 0
                ReturnUnrecognizedCharacters = 0
                LocateZonesInSpecifiedZone = 0
                IgnoreAreaOutsideSpecifiedZone = 0", @"(?m)^ +", "");

            using (var tmpFile = new TemporaryFile(false))
            {
                var ocrEngine = new ScansoftOCR2Class();
                ocrEngine.SetOCRParameters(null, true);
                ocrEngine.WriteOCRSettingsToFile(tmpFile.FileName, false, true);

                string settings = ReadSettings(tmpFile.FileName);
                Assert.AreEqual(expectedSettings, settings, "Can fail because of non-default registry settings");

                // Set to new defaults and then set back from the registry
                using (var config = new OCRParametersConfigure())
                {
                    SetDefault(config, false);
                }
                ocrEngine.SetOCRParameters(_hasParameters.OCRParameters, true);
                ocrEngine.SetOCRParameters(null, true);
                ocrEngine.WriteOCRSettingsToFile(tmpFile.FileName, false, true);
                settings = ReadSettings(tmpFile.FileName);
                Assert.AreEqual(expectedSettings, settings);
            }
        }

        /// <summary>
        /// Tests that the classic default settings are correctly set
        /// </summary>
        /// <remarks>
        /// The only classic settings that are respected are the 'recognition' settings.
        /// </remarks>
        [Test, Category("Automated")]
        public static void TestUIClassicDefaults()
        {
            string expectedSettings = Regex.Replace(
                @";PART
                Kernel.Chr.CodePage = Windows ANSI
                Kernel.Chr.Rejected = 94
                Kernel.Imf.PDF.Resolution = 300
                Kernel.Img.Max.Pix.X = 32000
                Kernel.Img.Max.Pix.Y = 32000
                Kernel.OcrMgr.PreferAccurateEngine = TRUE

                AssignSpatialInfoToSpaceCharacters = 0
                EnableDespeckleMode = 1
                ForceDespeckle = 0
                IgnoreParagraphFlag = 0
                LimitToBasicLatinCharacters = 1
                MaxOcrPageFailureNumber = 10
                MaxOcrPageFailurePercentage = 25
                OrderZones = 1
                OutputMultipleSpaceCharacterSequences = 0
                OutputOneSpaceCharacterPerCount = 0
                OutputTabCharactersForTabSpaceType = 0
                PrimaryDecompositionMethod = 0
                RequireOnePageSuccess = 0
                SkipPageOnFailure = 0
                TimeoutLength = 120000
                TreatZonesAsParagraphs = 0
                OCRFindType = 0
                ReturnUnrecognizedCharacters = 0
                LocateZonesInSpecifiedZone = 0
                IgnoreAreaOutsideSpecifiedZone = 0", @"(?m)^ +", "");

            using (var config = new OCRParametersConfigure())
            {
                SetClassic(config);
            }
            using (var tmpFile = new TemporaryFile(false))
            {
                var ocrEngine = new ScansoftOCR2Class();
                ocrEngine.SetOCRParameters(_hasParameters.OCRParameters, true);
                ocrEngine.WriteOCRSettingsToFile(tmpFile.FileName, false, true);

                string settings = ReadSettings(tmpFile.FileName);
                Assert.AreEqual(expectedSettings, settings);
            }
        }

        /// <summary>
        /// Tests that the default settings are correctly set
        /// </summary>
        [Test, Category("Automated")]
        public static void TestUIDefaults()
        {
            string expectedSettings = Regex.Replace(
                @";PART
                    Kernel.Chr.CodePage = Windows ANSI
                    Kernel.Chr.Rejected = 94
                    Kernel.Imf.PDF.Resolution = 300
                    Kernel.Img.Max.Pix.X = 32000
                    Kernel.Img.Max.Pix.Y = 32000
                    Kernel.OcrMgr.PreferAccurateEngine = TRUE

                    AssignSpatialInfoToSpaceCharacters = 1
                    EnableDespeckleMode = 1
                    ForceDespeckle = 0
                    IgnoreParagraphFlag = 1
                    LimitToBasicLatinCharacters = 0
                    MaxOcrPageFailureNumber = 4294967295
                    MaxOcrPageFailurePercentage = 100
                    OrderZones = 0
                    OutputMultipleSpaceCharacterSequences = 1
                    OutputOneSpaceCharacterPerCount = 1
                    OutputTabCharactersForTabSpaceType = 0
                    PrimaryDecompositionMethod = 0
                    RequireOnePageSuccess = 1
                    SkipPageOnFailure = 1
                    TimeoutLength = 240000
                    TreatZonesAsParagraphs = 1
                    OCRFindType = 0
                    ReturnUnrecognizedCharacters = 0
                    LocateZonesInSpecifiedZone = 1
                    IgnoreAreaOutsideSpecifiedZone = 1", @"(?m)^ +", "");

            // Set default settings by opening and closing the dialog with the OK button
            using (var config = new OCRParametersConfigure())
            {
                SetDefault(config, false);
            }
            using (var tmpFile = new TemporaryFile(false))
            {
                var ocrEngine = new ScansoftOCR2Class();
                ocrEngine.SetOCRParameters(_hasParameters.OCRParameters, true);
                ocrEngine.WriteOCRSettingsToFile(tmpFile.FileName, false, true);

                string settings = ReadSettings(tmpFile.FileName);
                Assert.AreEqual(expectedSettings, settings);
            }

            // Set default settings by opening, clicking the Classic and Default buttons and closing the dialog with the OK button
            using (var config = new OCRParametersConfigure())
            {
                SetDefault(config, true);
            }
            using (var tmpFile = new TemporaryFile(false))
            {
                var ocrEngine = new ScansoftOCR2Class();
                ocrEngine.SetOCRParameters(_hasParameters.OCRParameters, true);
                ocrEngine.WriteOCRSettingsToFile(tmpFile.FileName, false, true);

                string settings = ReadSettings(tmpFile.FileName);
                Assert.AreEqual(expectedSettings, settings);
            }
        }

        /// <summary>
        /// Tests that languages can be set
        /// </summary>
        [Test, Category("Automated")]
        public static void TestLanguages()
        {
            string expectedSettings = Regex.Replace(
                @";PART
                    Kernel.Chr.CodePage = Windows ANSI
                    Kernel.Chr.Rejected = 94
                    Kernel.Imf.PDF.Resolution = 300
                    Kernel.Img.Max.Pix.X = 32000
                    Kernel.Img.Max.Pix.Y = 32000
                    Kernel.Languages = LANG_ENG,LANG_SPA
                    Kernel.OcrMgr.PreferAccurateEngine = TRUE

                    AssignSpatialInfoToSpaceCharacters = 1
                    EnableDespeckleMode = 1
                    ForceDespeckle = 0
                    IgnoreParagraphFlag = 1
                    LimitToBasicLatinCharacters = 0
                    MaxOcrPageFailureNumber = 4294967295
                    MaxOcrPageFailurePercentage = 100
                    OrderZones = 0
                    OutputMultipleSpaceCharacterSequences = 1
                    OutputOneSpaceCharacterPerCount = 1
                    OutputTabCharactersForTabSpaceType = 0
                    PrimaryDecompositionMethod = 0
                    RequireOnePageSuccess = 1
                    SkipPageOnFailure = 1
                    TimeoutLength = 240000
                    TreatZonesAsParagraphs = 1
                    OCRFindType = 0
                    ReturnUnrecognizedCharacters = 0
                    LocateZonesInSpecifiedZone = 1
                    IgnoreAreaOutsideSpecifiedZone = 1", @"(?m)^ +", "");

            // Set default settings by opening and closing the dialog with the OK button
            using (var config = new OCRParametersConfigure())
            {
                SetDefault(config, false);
            }
            var parameters = (VariantVector)_hasParameters.OCRParameters;
            parameters.PushBack(new VariantPair
            {
                VariantKey = EOCRParameter.kLanguage,
                VariantValue = LANGUAGES.LANG_ENG
            });
            parameters.PushBack(new VariantPair
            {
                VariantKey = EOCRParameter.kLanguage,
                VariantValue = LANGUAGES.LANG_SPA
            });
            using (var tmpFile = new TemporaryFile(false))
            {
                var ocrEngine = new ScansoftOCR2Class();
                ocrEngine.SetOCRParameters(_hasParameters.OCRParameters, true);
                ocrEngine.WriteOCRSettingsToFile(tmpFile.FileName, false, true);

                string settings = ReadSettings(tmpFile.FileName);
                Assert.AreEqual(expectedSettings, settings);
            }
        }

        /// <summary>
        /// Tests that an int value for an unrecognized 'enum' setting and for a
        /// string setting can be loaded/saved out of the UI
        /// </summary>
        [Test, Category("Automated")]
        public static void TestInt()
        {
            string expectedSettings = Regex.Replace(
                @";PART
                    Kernel.Chr.CodePage = Windows ANSI
                    Kernel.Chr.Rejected = 32
                    Kernel.Decomp.ForceSingleColumn = TRUE
                    Kernel.Imf.PDF.Resolution = 300
                    Kernel.Img.Max.Pix.X = 32000
                    Kernel.Img.Max.Pix.Y = 32000

                    AssignSpatialInfoToSpaceCharacters = 1
                    EnableDespeckleMode = 1
                    ForceDespeckle = 0
                    IgnoreParagraphFlag = 1
                    LimitToBasicLatinCharacters = 0
                    MaxOcrPageFailureNumber = 4294967295
                    MaxOcrPageFailurePercentage = 100
                    OrderZones = 0
                    OutputMultipleSpaceCharacterSequences = 1
                    OutputOneSpaceCharacterPerCount = 1
                    OutputTabCharactersForTabSpaceType = 0
                    PrimaryDecompositionMethod = 0
                    RequireOnePageSuccess = 1
                    SkipPageOnFailure = 1
                    TimeoutLength = 240000
                    TreatZonesAsParagraphs = 1
                    OCRFindType = 0
                    ReturnUnrecognizedCharacters = 0
                    LocateZonesInSpecifiedZone = 1
                    IgnoreAreaOutsideSpecifiedZone = 1", @"(?m)^ +", "");

            using (var config = new OCRParametersConfigure())
            {
                // Set default settings by opening and closing the dialog with the OK button
                SetDefault(config, false);

                var parameters = (VariantVector)_hasParameters.OCRParameters;

                // Override a built-in int value
                parameters.PushBack(new VariantPair
                {
                    VariantKey = "Kernel.Chr.Rejected",
                    VariantValue = 32
                });

                // Override a supported true 'BOOl' value with false
                // (this is the Nuance default so it will now be omitted from the output)
                parameters.PushBack(new VariantPair
                {
                    VariantKey = "Kernel.OcrMgr.PreferAccurateEngine",
                    VariantValue = 0
                });

                // Override a built-in false 'BOOl' value with true
                parameters.PushBack(new VariantPair
                {
                    VariantKey = "Kernel.Decomp.ForceSingleColumn",
                    VariantValue = 1
                });

                // Add a custom int value
                parameters.PushBack(new VariantPair
                {
                    VariantKey = 1000,
                    VariantValue = 3
                });


                // Open/close the configuration again to ensure the values are persisted
                OpenAndCloseConfig(config);

                // Confirm that the custom value still exists
                Assert.That(parameters
                    .ToIEnumerable<VariantPair>()
                    .FirstOrDefault(pair => pair.VariantKey is int setting
                    && setting == 1000
                    && pair.VariantValue is int value
                    && value == 3) != null);
            }

            // Confirm that the settings are applied (except the custom int setting which should be ignored)
            using (var tmpFile = new TemporaryFile(false))
            {
                var ocrEngine = new ScansoftOCR2Class();
                ocrEngine.SetOCRParameters(_hasParameters.OCRParameters, true);
                ocrEngine.WriteOCRSettingsToFile(tmpFile.FileName, false, true);

                string settings = ReadSettings(tmpFile.FileName);
                Assert.AreEqual(expectedSettings, settings);
            }
        }

        /// <summary>
        /// Tests that a decimal value for a string setting can be loaded/saved out
        /// of the UI even if the decimal portion is zero
        /// </summary>
        [Test, Category("Automated")]
        public static void TestDecimal()
        {
            string expectedSettings = Regex.Replace(
                @";PART
                    Kernel.Chr.CodePage = Windows ANSI
                    Kernel.Chr.Rejected = 94
                    Kernel.Imf.PDF.Resolution = 300
                    Kernel.Img.Max.Pix.X = 32000
                    Kernel.Img.Max.Pix.Y = 32000
                    Kernel.OcrMgr.PreferAccurateEngine = TRUE
                    Kernel.OcrMgr.StrictPrecision = 1

                    AssignSpatialInfoToSpaceCharacters = 1
                    EnableDespeckleMode = 1
                    ForceDespeckle = 0
                    IgnoreParagraphFlag = 1
                    LimitToBasicLatinCharacters = 0
                    MaxOcrPageFailureNumber = 4294967295
                    MaxOcrPageFailurePercentage = 100
                    OrderZones = 0
                    OutputMultipleSpaceCharacterSequences = 1
                    OutputOneSpaceCharacterPerCount = 1
                    OutputTabCharactersForTabSpaceType = 0
                    PrimaryDecompositionMethod = 0
                    RequireOnePageSuccess = 1
                    SkipPageOnFailure = 1
                    TimeoutLength = 240000
                    TreatZonesAsParagraphs = 1
                    OCRFindType = 0
                    ReturnUnrecognizedCharacters = 0
                    LocateZonesInSpecifiedZone = 1
                    IgnoreAreaOutsideSpecifiedZone = 1", @"(?m)^ +", "");

            using (var config = new OCRParametersConfigure())
            {
                // Set default settings by opening and closing the dialog with the OK button
                SetDefault(config, false);

                // Add a custom double value
                var parameters = (VariantVector)_hasParameters.OCRParameters;
                parameters.PushBack(new VariantPair
                {
                    VariantKey = "Kernel.OcrMgr.StrictPrecision",
                    VariantValue = 1.0
                });

                // Open/close the configuration again to ensure the decimal is applied
                OpenAndCloseConfig(config);

                // Confirm that the value is still a double after being transformed to/from a string
                Assert.That(parameters
                    .ToIEnumerable<VariantPair>()
                    .FirstOrDefault(pair => pair.VariantKey is string setting
                    && setting == "Kernel.OcrMgr.StrictPrecision"
                    && pair.VariantValue is double) != null);
            }

            // Confirm that the setting was applied
            using (var tmpFile = new TemporaryFile(false))
            {
                var ocrEngine = new ScansoftOCR2Class();
                ocrEngine.SetOCRParameters(_hasParameters.OCRParameters, true);
                ocrEngine.WriteOCRSettingsToFile(tmpFile.FileName, false, true);

                string settings = ReadSettings(tmpFile.FileName);
                Assert.AreEqual(expectedSettings, settings);
            }
        }

        /// <summary>
        /// Tests that a string value for a string setting can be loaded/saved out of the UI
        /// </summary>
        [Test, Category("Automated")]
        public static void TestString()
        {
            string expectedSettings = Regex.Replace(
                @";PART
                    Kernel.Chr.CodePage = Windows ANSI
                    Kernel.Chr.Rejected = 94
                    Kernel.Imf.PDF.Resolution = 300
                    Kernel.Img.Max.Pix.X = 32000
                    Kernel.Img.Max.Pix.Y = 32000
                    Kernel.OcrMgr.PreferAccurateEngine = TRUE
                    Kernel.OcrMgr.Spell.UserDictionary.FileName = ""D:\\Dictionary.txt""

                    AssignSpatialInfoToSpaceCharacters = 1
                    EnableDespeckleMode = 1
                    ForceDespeckle = 0
                    IgnoreParagraphFlag = 1
                    LimitToBasicLatinCharacters = 0
                    MaxOcrPageFailureNumber = 4294967295
                    MaxOcrPageFailurePercentage = 100
                    OrderZones = 0
                    OutputMultipleSpaceCharacterSequences = 1
                    OutputOneSpaceCharacterPerCount = 1
                    OutputTabCharactersForTabSpaceType = 0
                    PrimaryDecompositionMethod = 0
                    RequireOnePageSuccess = 1
                    SkipPageOnFailure = 1
                    TimeoutLength = 240000
                    TreatZonesAsParagraphs = 1
                    OCRFindType = 0
                    ReturnUnrecognizedCharacters = 0
                    LocateZonesInSpecifiedZone = 1
                    IgnoreAreaOutsideSpecifiedZone = 1", @"(?m)^ +", "");

            using (var config = new OCRParametersConfigure())
            {
                // Set default settings by opening and closing the dialog with the OK button
                SetDefault(config, false);

                // Set a string value
                var parameters = (VariantVector)_hasParameters.OCRParameters;
                parameters.PushBack(new VariantPair
                {
                    VariantKey = "Kernel.OcrMgr.Spell.UserDictionary.FileName",
                    VariantValue = "\"D:\\Dictionary.txt\""
                });

                // Open/close the configuration again to test persistence
                OpenAndCloseConfig(config);
            }

            // Confirm that the setting was applied
            using (var tmpFile = new TemporaryFile(false))
            {
                var ocrEngine = new ScansoftOCR2Class();
                ocrEngine.SetOCRParameters(_hasParameters.OCRParameters, true);
                ocrEngine.WriteOCRSettingsToFile(tmpFile.FileName, false, true);

                string settings = ReadSettings(tmpFile.FileName);
                Assert.AreEqual(expectedSettings, settings);
            }
        }
        #endregion Tests

        #region Helper Methods

        static void SetClassic(OCRParametersConfigure config)
        {
            // Run the UI and click the Classic and OK buttons
            Task t = Task.Factory.StartNew(() =>
            {
                int processID = Process.GetCurrentProcess().Id;
                var winHandle = IntPtr.Zero;
                for (int i = 0; i < 10 && winHandle == IntPtr.Zero; i++)
                {
                    Thread.Sleep(50);
                    winHandle = NativeMethods.FindWindowWithText(winHandle, "Configure OCR properties", processID);
                }
                Assert.AreNotEqual(IntPtr.Zero, winHandle);

                var buttonHandle = NativeMethods.FindWindowWithText(winHandle, "Classic");
                Assert.AreNotEqual(IntPtr.Zero, buttonHandle);
                NativeMethods.ClickButton(buttonHandle);

                buttonHandle = NativeMethods.FindWindowWithText(winHandle, "OK");
                Assert.AreNotEqual(IntPtr.Zero, buttonHandle);
                NativeMethods.ClickButton(buttonHandle);
            }, new CancellationToken());

            _hasParameters.OCRParameters = (IOCRParameters)new VariantVector();
            config.ConfigureOCRParameters(_hasParameters, false, 0);

            t.Wait();
        }

        static void SetDefault(OCRParametersConfigure config, bool explicitly)
        {
            // Run the UI and click the Default and OK buttons
            Task t = Task.Factory.StartNew(() =>
            {
                int processID = Process.GetCurrentProcess().Id;
                var winHandle = IntPtr.Zero;
                for (int i = 0; i < 10 && winHandle == IntPtr.Zero; i++)
                {
                    Thread.Sleep(50);
                    winHandle = NativeMethods.FindWindowWithText(winHandle, "Configure OCR properties", processID);
                    Assert.AreNotEqual(IntPtr.Zero, winHandle);
                }
                Assert.AreNotEqual(IntPtr.Zero, winHandle);

                var buttonHandle = IntPtr.Zero;
                if (explicitly)
                {
                    // First set 'classic' values so that there's something to change
                    buttonHandle = NativeMethods.FindWindowWithText(winHandle, "Classic");
                    Assert.AreNotEqual(IntPtr.Zero, buttonHandle);
                    NativeMethods.ClickButton(buttonHandle);

                    buttonHandle = NativeMethods.FindWindowWithText(winHandle, "Default");
                    Assert.AreNotEqual(IntPtr.Zero, buttonHandle);
                    NativeMethods.ClickButton(buttonHandle);
                }

                buttonHandle = NativeMethods.FindWindowWithText(winHandle, "OK");
                Assert.AreNotEqual(IntPtr.Zero, buttonHandle);
                NativeMethods.ClickButton(buttonHandle);
            }, new CancellationToken());

            _hasParameters.OCRParameters = (IOCRParameters)new VariantVector();
            config.ConfigureOCRParameters(_hasParameters, false, 0);

            t.Wait();
        }

        static void OpenAndCloseConfig(OCRParametersConfigure config)
        {
            // Run the UI and click the OK button
            Task t = Task.Factory.StartNew(() =>
            {
                int processID = Process.GetCurrentProcess().Id;
                var winHandle = IntPtr.Zero;
                for (int i = 0; i < 10 && winHandle == IntPtr.Zero; i++)
                {
                    Thread.Sleep(50);
                    winHandle = NativeMethods.FindWindowWithText(winHandle, "Configure OCR properties", processID);
                    Assert.AreNotEqual(IntPtr.Zero, winHandle);
                }
                Assert.AreNotEqual(IntPtr.Zero, winHandle);

                var buttonHandle = NativeMethods.FindWindowWithText(winHandle, "OK");
                Assert.AreNotEqual(IntPtr.Zero, buttonHandle);
                NativeMethods.ClickButton(buttonHandle);
            }, new CancellationToken());

            config.ConfigureOCRParameters(_hasParameters, false, 0);

            t.Wait();
        }

        private static string ReadSettings(string fileName)
        {
            string settings = File.ReadAllText(fileName);

            // Remove APIPlus settings (present if InitPlus was called)
            if (settings.Contains("APIPlus.ProcessPagesEx.PromptPath"))
            {
                settings = Regex.Replace(settings,
                    @"^(APIPlus\.ProcessPagesEx\.PromptPath|Converters\.).*[\r\n]+",
                    "",
                    RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture);
            }

            return settings;
        }

        #endregion Helper Methods
    }
}