using Extract.Testing.Utilities;
using Extract.Utilities.Test.Properties;
using Microsoft.Win32;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using UCLID_COMUTILSLib;

namespace Extract.Utilities.Test
{
    /// <summary>
    /// Class for testing the ConfigSettings class
    /// </summary>
    [TestFixture]
    [Category("ExtractSettings")]
    public class TestExtractSettings
    {
        #region Overhead Methods

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
        }

        #endregion Overhead Methods

        /// <summary>
        /// Tests the static vs dynamic behavior against the same config file.
        /// </summary>
        [Test]
        public static void ConfigFileStaticVSDynamic()
        {
            string configFileName = Path.Combine(Path.GetTempPath(), "StaticVSDynamic.config");

            try
            {
                // Ensure there is no previously existing config file.
                if (File.Exists(configFileName))
                {
                    File.Delete(configFileName);
                }

                // Initialize a dynamic ConfigSettings instance and ensure the proper default value.
                ConfigSettings<Settings> dynamicConfig =
                    new ConfigSettings<Settings>(configFileName, true, true);
                Assert.That(dynamicConfig.Settings.TestInt == 42,
                    "Dynamic ConfigSettings did not initialize properly");

                // Specifying a new value should update the config file.
                dynamicConfig.Settings.TestInt = 0;

                // A static ConfigSettings instance initialized at this point should default to the
                // value currently saved in the config file.
                ConfigSettings<Settings> staticConfig =
                    new ConfigSettings<Settings>(configFileName, false, false);
                Assert.That(staticConfig.Settings.TestInt == 0,
                    "Dynamic ConfigSettings did not update value.");

                // A new value specified in the static instance should not affect the value in the
                // dynamic instance.
                staticConfig.Settings.TestInt = 1;
                Assert.That(dynamicConfig.Settings.TestInt == 0,
                    "Static ConfigSettings modified value without save.");

                // However, saving the static config file should update the dynamic instance.
                staticConfig.Save();
                Assert.That(dynamicConfig.Settings.TestInt == 1,
                    "Static ConfigSettings save failed to modify value.");

                // Finally, changing the setting in the dynamic config file should not alter the
                // static instance.
                dynamicConfig.Settings.TestInt = 2;
                Assert.That(staticConfig.Settings.TestInt == 1,
                    "Static ConfigSettings updated dynamically.");
            }
            finally
            {
                if (File.Exists(configFileName))
                {
                    File.Delete(configFileName);
                }
            }
        }

        /// <summary>
        /// Tests whether config files abide by the createIfMissing constructor parameter as true.
        /// </summary>
        [Test]
        public static void ConfigFileAutomaticFileCreation()
        {
            string configFileName =
                Path.Combine(Path.GetTempPath(), "AutomaticFileCreation.config");

            try
            {
                // Ensure there is no previously existing config file.
                if (File.Exists(configFileName))
                {
                    File.Delete(configFileName);
                }

                // Test that a config file is automatically generated.
                ConfigSettings<Settings> autoFileCreationConfig =
                    new ConfigSettings<Settings>(configFileName, true, true);
                Assert.That(File.Exists(configFileName),
                    "ConfigSettings did not auto-create config file.");

                // Modify a test setting, and ensure that a config file initialized without
                // auto-creation properly reads the existing file.
                autoFileCreationConfig.Settings.TestInt = 1;
                ConfigSettings<Settings> noFileCreationConfig =
                    new ConfigSettings<Settings>(configFileName, true, false);

                Assert.That(noFileCreationConfig.Settings.TestInt == 1,
                    "ConfigSettings without auto-creation did not read from existing file");
            }
            finally
            {
                if (File.Exists(configFileName))
                {
                    File.Delete(configFileName);
                }
            }
        }

        /// <summary>
        /// Tests whether config files abide by the createIfMissing constructor parameter as false.
        /// </summary>
        [Test]
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "noFileCreationConfig")]
        public static void ConfigFileNoAutomaticFileCreation()
        {
            string configFileName =
                Path.Combine(Path.GetTempPath(), "NoAutomaticFileCreation.config");

            try
            {
                bool exceptionThrown = false;

                try
                {
                    // Ensure there is no previously existing config file.
                    if (File.Exists(configFileName))
                    {
                        File.Delete(configFileName);
                    }

                    // Test that an exception is thrown if a ConfigSettings instance is initialized
                    // with no existing config file and createIfMissing set to false. 
                    ConfigSettings<Settings> noFileCreationConfig =
                        new ConfigSettings<Settings>(configFileName, true, false);
                }
                catch (ExtractException ee)
                {
                    for (ExtractException eeNext = ee; eeNext != null;
                            eeNext = eeNext.InnerException as ExtractException)
                    {
                        if (eeNext.EliCode.EndsWith("29688", StringComparison.Ordinal))
                        {
                            exceptionThrown = true;
                            break;
                        }
                    }

                    if (!exceptionThrown)
                    {
                        throw;
                    }
                }

                Assert.That(exceptionThrown);
            }
            finally
            {
                if (File.Exists(configFileName))
                {
                    File.Delete(configFileName);
                }
            }
        }

        /// <summary>
        /// Tests use of the XML attribute on a config file node.
        /// https://extract.atlassian.net/browse/ISSUE-12804
        /// </summary>
        [Test]
        public static void ConfigFileXml()
        {
             using (TemporaryFile configFile = new TemporaryFile(".config", false))
             {
                 File.Delete(configFile.FileName);

                 // Initialize a dynamic ConfigSettings instance and ensure the proper default value.
                 ConfigSettings<Settings> config = new ConfigSettings<Settings>(configFile.FileName, true, true);
                 config.Settings.TestString = "TEST";

                 string fileText = File.ReadAllText(configFile.FileName);
                 fileText = fileText.Replace("<value>TEST</value>", "<value XML=\"False\">&lt;root/&gt;</value>");
                 File.Delete(configFile.FileName);
                 File.WriteAllText(configFile.FileName, fileText);

                 Assert.That(config.Settings.TestString == "<root/>");
             }
        }

        /// <summary>
        /// Tests use of the XML attribute on a config file node.
        /// https://extract.atlassian.net/browse/ISSUE-12804
        /// </summary>
        [Test]
        public static void ConfigFileXml2()
        {
            using (TemporaryFile configFile = new TemporaryFile(".config", false))
            {
                File.Delete(configFile.FileName);

                // Initialize a dynamic ConfigSettings instance and ensure the proper default value.
                ConfigSettings<Settings> config = new ConfigSettings<Settings>(configFile.FileName, true, true);
                config.Settings.TestString = "TEST";

                string fileText = File.ReadAllText(configFile.FileName);
                fileText = fileText.Replace("<value>TEST</value>", "<value XML=\"True\">&lt;root/&gt;</value>");
                File.Delete(configFile.FileName);
                File.WriteAllText(configFile.FileName, fileText);

                Assert.That(config.Settings.TestString == "&lt;root/&gt;");
            }
        }

        /// <summary>
        /// Tests use of the expandTags attribute on a config file node
        /// https://extract.atlassian.net/browse/ISSUE-12804
        /// </summary>
        [Test]
        public static void ConfigFileExpandTags()
        {
            using (TemporaryFile configFile = new TemporaryFile(".config", false))
            {
                File.Delete(configFile.FileName);

                // Initialize a dynamic ConfigSettings instance and ensure the proper default value.
                ConfigSettings<Settings> config = new ConfigSettings<Settings>(configFile.FileName, true, true, 
                    (ITagUtility)new MiscUtils());
                config.Settings.TestString = "TEST";

                string fileText = File.ReadAllText(configFile.FileName);
                fileText = fileText.Replace("<value>TEST</value>", "<value expandTags=\"True\">$DirOf(C:\\Test\\Test.tif)</value>");
                File.Delete(configFile.FileName);
                File.WriteAllText(configFile.FileName, fileText);

                Assert.That(config.Settings.TestString == "C:\\Test");
            }
        }

        /// <summary>
        /// Tests use of the expandTags attribute on a config file node
        /// https://extract.atlassian.net/browse/ISSUE-12804
        /// </summary>
        [Test]
        public static void ConfigFileExpandTags2()
        {
            using (TemporaryFile configFile = new TemporaryFile(".config", false))
            {
                File.Delete(configFile.FileName);

                // Initialize a dynamic ConfigSettings instance and ensure the proper default value.
                ConfigSettings<Settings> config = new ConfigSettings<Settings>(configFile.FileName, true, true,
                    (ITagUtility)new MiscUtils());
                config.Settings.TestString = "TEST";

                string fileText = File.ReadAllText(configFile.FileName);
                fileText = fileText.Replace("<value>TEST</value>", "<value expandTags=\"False\">$DirOf(C:\\Test\\Test.tif)</value>");
                File.Delete(configFile.FileName);
                File.WriteAllText(configFile.FileName, fileText);

                Assert.That(config.Settings.TestString == "$DirOf(C:\\Test\\Test.tif)");
            }
        }

        /// <summary>
        /// Tests that expandTags and XML attributes can both be explicitly specified.
        /// https://extract.atlassian.net/browse/ISSUE-13196
        /// </summary>
        [Test]
        public static void ConfigFileXmlAndExpandTags()
        {
            using (TemporaryFile configFile = new TemporaryFile(".config", false))
            {
                File.Delete(configFile.FileName);

                // Initialize a dynamic ConfigSettings instance and ensure the proper default value.
                ConfigSettings<Settings> config = new ConfigSettings<Settings>(configFile.FileName, true, true,
                    (ITagUtility)new MiscUtils());
                config.Settings.TestString = "TEST";

                string fileText = File.ReadAllText(configFile.FileName);
                fileText = fileText.Replace("<value>TEST</value>", "<value XML=\"False\" expandTags=\"True\">$DirOf(C:\\Test\\Test.tif)</value>");
                File.Delete(configFile.FileName);
                File.WriteAllText(configFile.FileName, fileText);

                Assert.That(config.Settings.TestString == "C:\\Test");
            }
        }

        /// <summary>
        /// Tests that expandTags and XML attributes can both be explicitly specified.
        /// https://extract.atlassian.net/browse/ISSUE-13196
        /// </summary>
        [Test]
        public static void ConfigFileXmlAndExpandTags2()
        {
            using (TemporaryFile configFile = new TemporaryFile(".config", false))
            {
                File.Delete(configFile.FileName);

                // Initialize a dynamic ConfigSettings instance and ensure the proper default value.
                ConfigSettings<Settings> config = new ConfigSettings<Settings>(configFile.FileName, true, true,
                    (ITagUtility)new MiscUtils());
                config.Settings.TestString = "TEST";

                string fileText = File.ReadAllText(configFile.FileName);
                fileText = fileText.Replace("<value>TEST</value>", "<value XML=\"True\" expandTags=\"False\">&lt;root/&gt;</value>");
                File.Delete(configFile.FileName);
                File.WriteAllText(configFile.FileName, fileText);

                Assert.That(config.Settings.TestString == "<root/>");
            }
        }

        /// <summary>
        /// Tests that expandTags and XML attributes cannot both be true.
        /// https://extract.atlassian.net/browse/ISSUE-12804
        /// </summary>
        [Test]
        public static void ConfigFileXmlAndExpandTags3()
        {
            using (TemporaryFile configFile = new TemporaryFile(".config", false))
            {
                File.Delete(configFile.FileName);

                // Initialize a dynamic ConfigSettings instance and ensure the proper default value.
                ConfigSettings<Settings> config = new ConfigSettings<Settings>(configFile.FileName, true, true,
                    (ITagUtility)new MiscUtils());
                config.Settings.TestString = "TEST";

                string fileText = File.ReadAllText(configFile.FileName);
                fileText = fileText.Replace("<value>TEST</value>", "<value XML=\"True\" expandTags=\"True\">&lt;root/&gt;</value>");
                File.Delete(configFile.FileName);
                File.WriteAllText(configFile.FileName, fileText);

                bool exceptionThrown = false;
                try
                {
                    // Triggers TestString to be read from disk.
                    exceptionThrown = config.Settings.TestString == "BOGUS";
                    
                }
                catch
                {
                    exceptionThrown = true;
                }


                Assert.That(exceptionThrown);
            }
        }

        /// <summary>
        /// Tests the static vs dynamic behavior against the same registry key.
        /// </summary>
        [Test]
        public static void RegistryStaticVSDynamic()
        {
            string registryKeyName =
                    @"SOFTWARE\Extract Systems\ReusableComponents\Extract.Utilities.Test";

            try
            {
                // Ensure there is no previously existing registry key.
                Registry.CurrentUser.DeleteSubKey(registryKeyName, false);

                // Initialize a dynamic RegistrySettings instance and ensure the proper default value.
                RegistrySettings<Settings> dynamicConfig =
                    new RegistrySettings<Settings>(registryKeyName, true);
                Assert.That(dynamicConfig.Settings.TestInt == 42,
                    "Dynamic RegistrySettings did not initialize properly");

                // Specifying a new value should update the config file.
                dynamicConfig.Settings.TestInt = 0;

                // A static RegistrySettings instance initialized at this point should default to the
                // value currently saved in the config file.
                RegistrySettings<Settings> staticConfig =
                    new RegistrySettings<Settings>(registryKeyName, false);
                Assert.That(staticConfig.Settings.TestInt == 0,
                    "Dynamic RegistrySettings did not update value.");

                // A new value specified in the static instance should not affect the value in the
                // dynamic instance.
                staticConfig.Settings.TestInt = 1;
                Assert.That(dynamicConfig.Settings.TestInt == 0,
                    "Static RegistrySettings modified value without save.");

                // However, saving the static config file should update the dynamic instance.
                staticConfig.Save();
                Assert.That(dynamicConfig.Settings.TestInt == 1,
                    "Static RegistrySettings save failed to modify value.");

                // Finally, changing the setting in the dynamic config file should not alter the
                // static instance.
                dynamicConfig.Settings.TestInt = 2;
                Assert.That(staticConfig.Settings.TestInt == 1,
                    "Static RegistrySettings updated dynamically.");
            }
            finally
            {
                Registry.CurrentUser.DeleteSubKey(registryKeyName, false);
            }
        }
    }
}
