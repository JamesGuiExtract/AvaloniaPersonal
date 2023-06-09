﻿using Extract.Database;
using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Database.Test
{
    /// <summary>
    /// Manages FAM databases for unit tests.
    /// </summary>
    /// <typeparam name="T">The unit test class for which this manager is needed.</typeparam>
    /// <seealso cref="System.IDisposable" />
    [CLSCompliant(false)]
    public class FAMTestDBManager<T> : IDisposable
    {
        #region Fields

        /// <summary>
        /// The <see cref="TestFileManager"/> that manages DB backup files.
        /// </summary>
        TestFileManager<T> _backupFileManager = new TestFileManager<T>();

        /// <summary>
        /// The FAM databases being actively managed by this instance.
        /// </summary>
        ConcurrentDictionary<string, FileProcessingDB> _activeDatabases = new ConcurrentDictionary<string, FileProcessingDB>();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TestFileManager{T}"/> class.
        /// </summary>
        public FAMTestDBManager()
        {
        }

        #endregion Constructors

        #region Methods

        /// An automatically generated suggestion for the DB name to use for the current test.
        public string GenerateDatabaseName()
        {
            string unsafeName = UtilityMethods.FormatInvariant($"Test_{typeof(T).Name}_{TestContext.CurrentContext.Test.Name}");

            // Replace non-word chars except "-"
            // Some unit test values use negative numbers that are useful to see as negative
            string safeName = Regex.Replace(unsafeName, @"([^\w-]|_)+", "_");
            safeName = safeName.Substring(0, Math.Min(safeName.Length, 115));
            string randomSuffix = UtilityMethods.GetRandomString(8, true, false, false);

            return safeName + "_" + randomSuffix;
        }

        /// <summary>
        /// Gets an IFileProcessingDB connection to a new FAM database created on the local database
        /// instance. 
        /// </summary>
        /// <param name="databaseName">The name to give to the database.</param>
        /// <returns>A <see cref="IFileProcessingDB"/> for the database.</returns>
        public FileProcessingDB GetNewDatabase(string databaseName)
        {
            try
            {
                FileProcessingDB fileProcessingDb = null;
                if (_activeDatabases.TryGetValue(databaseName, out fileProcessingDb))
                {
                    var ee = new ExtractException("ELI42028", "Database already exists");
                    ee.AddDebugData("Database name", databaseName, false);
                    throw ee;
                }
                else
                {
                    fileProcessingDb = new FileProcessingDB();
                    fileProcessingDb.DatabaseServer = "(local)";
                    fileProcessingDb.CreateNewDB(databaseName, bstrInitWithPassword: "a");
                    if (!_activeDatabases.TryAdd(databaseName, fileProcessingDb))
                    {
                        var ee = new ExtractException("ELI51556", "Database already exists");
                        ee.AddDebugData("Database name", databaseName, false);
                        throw ee;
                    }

                    return fileProcessingDb;
                }
            }
            catch (Exception ex)
            {
                try
                {
                    RemoveDatabase(databaseName);
                }
                catch { }

                var ee = ex.AsExtract("ELI42034");
                ee.AddDebugData("Database", databaseName, false);
                throw ee;
            }
        }

        /// <summary>
        /// Gets an IFileProcessingDB connection to local database <see paramref="destinationDbName"/>
        /// as restored from the backup <see paramref="dbBackupResourceName"/>.
        /// </summary>
        /// <param name="dbBackupResourceName">The database backup as an embedded resource.</param>
        /// <param name="destinationDBName">The name the database should be restored to.</param>
        public FileProcessingDB GetDatabase(string dbBackupResourceName, string destinationDBName, bool updateSchema = true)
        {
            try
            {
                FileProcessingDB fileProcessingDb = null;
                return _activeDatabases.GetOrAdd(destinationDBName, dbName =>
                {
                    string backupDbFile = _backupFileManager.GetFile(dbBackupResourceName);

                    // In most cases SQL server will not have access to the file; giving access to
                    // all users will allow it access.
                    FileSecurity fSecurity = File.GetAccessControl(backupDbFile);
                    fSecurity.AddAccessRule(new FileSystemAccessRule(
                        @".\users", FileSystemRights.FullControl, AccessControlType.Allow));
                    File.SetAccessControl(backupDbFile, fSecurity);

                    DBMethods.RestoreDatabaseToLocalServer(backupDbFile, destinationDBName);
                    _backupFileManager.RemoveFile(dbBackupResourceName);

                    // Get a FileProcessingDB instance to the DB, and use it to upgrade the database schema.
                    fileProcessingDb = new FileProcessingDB();
                    fileProcessingDb.DatabaseServer = "(local)";
                    fileProcessingDb.DatabaseName = destinationDBName;

                    if (updateSchema)
                    {
                        fileProcessingDb.UpgradeToCurrentSchema(null);
                    }

                    return fileProcessingDb;
                });
            }
            catch (Exception ex)
            {
                try
                {
                    RemoveDatabase(destinationDBName);
                }
                catch { }

                var ee = ex.AsExtract("ELI41908");
                ee.AddDebugData("Database", destinationDBName, false);
                throw ee;
            }
        }

        /// Factory method to get a database wrapper from a database backup
        public IDisposableDatabase<T> GetDisposableDatabase(
            string dbResourceName,
            string destinationDBName,
            int numberOfWorkflows = 0)
        {
            return numberOfWorkflows switch
            {
                0 => new NoWorkflows<T>(this, destinationDBName, GetDatabase(dbResourceName, destinationDBName), null),
                _ => throw new ArgumentException(UtilityMethods.FormatInvariant(
                    $"Unsupported number of workflows: {numberOfWorkflows}"))
            };
        }

        /// Factory method to get a database wrapper from a new database
        public IDisposableDatabase<T> GetDisposableDatabase(
            string destinationDBName,
            int numberOfWorkflows = 0,
            bool enableLoadBalancing = false,
            string[] actionNames = null)
        {
            return numberOfWorkflows switch
            {
                0 => new NoWorkflows<T>(this, destinationDBName, GetNewDatabase(destinationDBName), actionNames),
                1 => new OneWorkflow<T>(this, destinationDBName, enableLoadBalancing),
                2 => new TwoWorkflows<T>(this, destinationDBName, enableLoadBalancing),
                _ => throw new ArgumentException(UtilityMethods.FormatInvariant(
                    $"Unsupported number of workflows: {numberOfWorkflows}"))
            };
        }

        /// <summary>
        /// Removes the specified <see paramref="databaseName"/> from the local SQL instance.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        public void RemoveDatabase(string databaseName)
        {
            FileProcessingDB fileProcessingDb = null;
            ExtractException ee = null;

            // First try to close all DB connections for the FileProcessingDb instance
            try
            {
                if (_activeDatabases.TryRemove(databaseName, out fileProcessingDb))
                {
                    fileProcessingDb.CloseAllDBConnections();
                }
            }
            catch (Exception ex)
            {
                ee = ex.AsExtract("ELI41909");
            }

            // Then try to drop the DB regardless of whether there were errors trying to close the
            // connections.
            try
            {
                if (fileProcessingDb != null)
                {
                    DBMethods.DropLocalDB(databaseName);
                }
            }
            catch (Exception ex)
            {
                if (ee == null)
                {
                    ex.AsExtract("ELI41910").ExtractLog("ELI43552");
                }
                else
                {
                    ex.ExtractLog("ELI41912");
                }
            }
            fileProcessingDb = null;
        }

        #endregion Methods

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="FAMTestDBManager{T}"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);

            // Prevent nunit error after tests are run:
            // "Unhandled Exception: NUnit.Engine.NUnitEngineException: Remote test agent exited with non-zero exit code -2146233020"
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        /// <overloads>Releases resources used by the <see cref="FAMTestDBManager{T}"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="FAMTestDBManager{T}"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_activeDatabases != null)
                {
                    var activeDbs = _activeDatabases.Keys.ToArray();
                    foreach (var databaseName in activeDbs)
                    {
                        try
                        {
                            RemoveDatabase(databaseName);
                        }
                        catch { }
                    }

                    _activeDatabases = null;
                }

                // Dispose of each of the temporary files (this will delete the files)
                if (_backupFileManager != null)
                {
                    _backupFileManager.Dispose();
                    _backupFileManager = null;
                }
            }

            // No unmanaged resources
        }

        #endregion IDisposable Members
    }
}
