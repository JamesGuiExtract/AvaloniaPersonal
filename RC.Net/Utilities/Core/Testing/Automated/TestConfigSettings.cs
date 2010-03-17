using Extract;
using Extract.Utilities;
using Extract.Utilities.Test.Properties;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace Extract.Utilities.Test
{
    /// <summary>
    /// Class for testing the ConfigSettings class
    /// </summary>
    [TestFixture]
    [Category("ConfigSettings")]
    public class TestConfigSettings
    {
        /// <summary>
        /// Tests the static vs dynamic behavior against the same config file.
        /// </summary>
        [Test]
        public static void StaticVSDynamic()
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

                // A new value specifyied in the static instance should not affect the value in the
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
        public static void AutomaticFileCreation()
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
        public static void NoAutomaticFileCreation()
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

                    // Test that an execption is thrown if a ConfigSettings instance is initialized
                    // with no existing config file and createIfMissing set to false. 
                    ConfigSettings<Settings> noFileCreationConfig =
                        new ConfigSettings<Settings>(configFileName, true, false);
                }
                catch (ExtractException ee)
                {
                    for (ExtractException eeNext = ee; eeNext != null;
                            eeNext = eeNext.InnerException as ExtractException)
                    {
                        if (eeNext.EliCode == "ELI29688")
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
    }
}
