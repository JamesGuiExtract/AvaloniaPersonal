using System;
using System.Configuration;
using System.IO;

namespace Extract.Utilities
{
    /// <summary>
    /// A class to check for a corrupt user.config file when it is created and to make a new backup
    /// of the user.config if the file is not corrupt when destroyed.
    /// https://extract.atlassian.net/browse/ISSUE-12830
    /// </summary>
    public class UserConfigChecker
    {
        // Flag used to limit the exceptions logged by this class
        volatile static bool _exceptionLogged = false;

        /// <summary>
        /// Initializes the UserConfigChecker class which will check the user.config file for
        /// corruption and backup the file if not corrupt and if is corrupt and replaces
        /// the user.config file with the backup if it exists otherwise deletes the corrupt file
        /// </summary>
        public UserConfigChecker()
        {
            try
            {
                EnsureValidUserConfigFile();
            }
            catch (Exception e)
            {
                e.ExtractLog("ELI38212");
            }
        }

        /// <summary>
        /// Checks the user.config file for corruption and backups the file if not corrupt and if 
        /// is corrupt and replaces the user.config file with the backup if it exists otherwise 
        /// deletes the corrupt file
        /// </summary>
        ~UserConfigChecker()
        {
            try
            {
                EnsureValidUserConfigFile();
            }
            catch (Exception e)
            {
                e.ExtractLog("ELI38213");
            }
        }

        /// <summary>
        /// Method attempts to open the User.config file, if it throws a configuration error
        /// exception and there is a user.config.bak file the user.config file will be replaced
        /// with that file, if there is no user.config.bak file the user.config file is deleted 
        /// which resets the settings to default settings.
        /// if the user.config file is successfully opened a copy is made and named user.config.bak
        /// This method logs all exceptions and will not throw an exception.
        /// 
        /// Note: Since this will backup the current user.confg file if it is valid, this method
        /// can be used to make a backup of a good config file.
        /// </summary>
        public static void EnsureValidUserConfigFile()
        {
            Configuration config;
            bool configLoaded = false;
            bool configWasReplacedWithBackup = false;

            // Attempt to open user.config file and if bad replace with user.config.bak and
            // try to open again
            do
            {
                try
                {
                    // Try to open the User.config file
                    config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);

                    // Make a backup of the good configuration overwriting any existing file
                    // Before copying the file make sure it exists the OpenExeConfiguration 
                    // method will return a configuration even if the file doesn't exist
                    if (File.Exists(config.FilePath))
                    {
                        string backupFileName = config.FilePath + ".bak";

                        bool backupExists = File.Exists(backupFileName);

                        // if the backup file is readonly don't copy over it
                        if (!backupExists || !File.GetAttributes(backupFileName).HasFlag(FileAttributes.ReadOnly))
                        {
                            File.Copy(config.FilePath, backupFileName, true);
                        }
                        // Log an exception to indicate that the backup if readonly
                        else if (backupExists && File.GetAttributes(backupFileName).HasFlag(FileAttributes.ReadOnly) && !_exceptionLogged)
                        {
                            ExtractException exNoBackup = new ExtractException("ELI38221",
                                "Application Trace: Backup file for user.config is readonly and cannot be overwritten.");
                            exNoBackup.AddDebugData("Backup file name", backupFileName, false);
                            exNoBackup.Log();
                            _exceptionLogged = true;
                        }
                    }
                    configLoaded = true;
                }
                catch (ConfigurationErrorsException e)
                {
                    try
                    {
                        // Check that the file name on the exception is the user.config file.
                        if (e.Filename.EndsWith("user.config", StringComparison.OrdinalIgnoreCase))
                        {
                            // Check if the user.config file is readonly
                            // if the backup file is readonly don't copy over it
                            if (File.GetAttributes(e.Filename).HasFlag(FileAttributes.ReadOnly))
                            {
                                // Throw exception because the user.config file is corrupt but should
                                // not be replaced because it is set to readonly
                                ExtractException roException = new ExtractException("ELI38220",
                                    "User.config file is corrupt but cannot be replaced because it is set to readonly.");
                                throw roException;
                            }

                            // Delete the corrupt file
                            File.Delete(e.Filename);
                            string exceptonMsg = "User.Config file was corrupt and has been deleted.";

                            // Check for a backup copy
                            if (!configWasReplacedWithBackup && File.Exists(e.Filename + ".bak"))
                            {
                                string backupFileName = e.Filename + ".bak";

                                // Copy the backup to the current User.Config file
                                File.Copy(backupFileName, e.Filename, true);
                                
                                configWasReplacedWithBackup = true;

                                // if the backup file is not readonly delete it
                                if (!File.GetAttributes(backupFileName).HasFlag(FileAttributes.ReadOnly))
                                {
                                    File.Delete(backupFileName);
                                }
                               
                                // Make sure the user.config file is not readonly
                                var fileAttributes = File.GetAttributes(e.Filename);
                                if (fileAttributes.HasFlag(FileAttributes.ReadOnly))
                                {
                                    File.SetAttributes(e.Filename, fileAttributes & (~FileAttributes.ReadOnly));
                                }

                                exceptonMsg = "User.Config file was corrupt and has been replaced with last known good User.Config.";
                            }
                            else
                            {
                                // if there was no backup file 
                                configLoaded = true;
                            }
                            ExtractException ee = new ExtractException("ELI38197",
                                "Application trace:" + exceptonMsg, e);
                            ee.AddDebugData("ConfigFileName", e.Filename, false);
                            ee.Log();
                        }
                        else
                        {
                            // Either both the User.Config file and the backup are corrupt or there
                            // is some other problem with loading the config file so log the 
                            // original exception
                            ExtractException ee = new ExtractException("ELI38198", "Unable to load User.config file.", e);
                            ee.AddDebugData("ConfigFileName", e.Filename, false);
                            ee.Log();
                            break;
                        }
                    }
                    catch (Exception e2)
                    {
                        if (!_exceptionLogged)
                        {
                            // Log the exception that was thrown during processing of the exception
                            e2.ExtractLog("ELI38177");

                            // Log the original exception
                            ExtractException ee = new ExtractException("ELI38199", "Unable to reset corrupt User.config file.", e);
                            ee.AddDebugData("ConfigFileName", e.Filename, false);
                            ee.Log();
                            _exceptionLogged = true;
                        }
                        break;
                    }
                }
                catch (Exception ex)
                {
                    if (!_exceptionLogged)
                    {
                        // Exceptions here are not critical but could be useful in tracking problems.
                        ExtractException ee = new ExtractException("ELI38214",
                            "Unable to check and/or save the user.config or user.config.backup file", ex);
                        ee.Log();
                        _exceptionLogged = true;
                    }
                    break;
                }
            }
            while (!configLoaded);
        }
    }
}
