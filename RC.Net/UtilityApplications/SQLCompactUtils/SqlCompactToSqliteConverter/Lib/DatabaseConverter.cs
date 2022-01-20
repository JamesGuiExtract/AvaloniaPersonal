using ErikEJ.SqlCeScripting;
using Extract.Database;
using Extract.Database.Sqlite;
using LinqToDB.Data;
using LinqToDB.DataProvider.SQLite;
using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Logger = System.Action<string>;

namespace Extract.Utilities.SqlCompactToSqliteConverter
{
    /// Class to convert one file-based database to another
    public interface IDatabaseConverter
    {
        /// <summary>
        /// Asynchronously convert a database
        /// </summary>
        /// <param name="inputPath">The input database path</param>
        /// <param name="outputPath">The output database path</param>
        /// <param name="statusCallback">Action that will be called with status update messages</param>
        Task Convert(string inputPath, string outputPath, Logger statusCallback);
    }

    /// <inheritdoc/>
    public class DatabaseConverter : IDatabaseConverter
    {
        private static object _sqlceMonitor = new();

        private readonly IDatabaseSchemaManagerProvider _schemaManagerProvider;

        public DatabaseConverter(IDatabaseSchemaManagerProvider schemaManagerProvider)
        {
            _schemaManagerProvider = schemaManagerProvider;
        }

        public async Task Convert(string inputPath, string outputPath, Logger statusCallback = null)
        {
            string[] scriptPaths = Array.Empty<string>();
            try
            {
                _ = inputPath ?? throw new ArgumentNullException(nameof(inputPath));
                _ = outputPath ?? throw new ArgumentNullException(nameof(outputPath));

                if (!Path.IsPathRooted(outputPath))
                {
                    throw new ArgumentException($"{nameof(outputPath)} cannot be a relative path");
                }

                statusCallback ??= delegate { };

                // Make a temporary copy of the input so we can update the schema without affecting the original
                using TemporaryFile databaseCopy = new(extension: ".sdf", sensitive: false);
                File.Copy(inputPath, databaseCopy.FileName, overwrite: true);

                statusCallback($"Converting {Path.GetFileName(inputPath)} to {Path.GetFileName(outputPath)}:");

                // Do work on the thread pool to avoid blocking the caller
                await Task.Run(async () =>
                {
                    // No need to avoid continuing on captured context here because there will be no context captured
                    // by Task.Run (threadpool) but also doesn't hurt anything to add ConfigureAwait(false) on every await
                    await UpdateSqlCompactSchema(databaseCopy.FileName, statusCallback).ConfigureAwait(false);

                    using TemporaryFile scriptFile = new(extension: ".sql", sensitive: false);

                    scriptPaths = ExportSqlCompact(databaseCopy.FileName, scriptFile.FileName, statusCallback);
                    ImportToSqlite(scriptPaths, outputPath, statusCallback);

                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51788");
            }
            finally
            {
                foreach (string path in scriptPaths)
                {
                    File.Delete(path);
                }
            }
        }

        /// Check for a configured IDatabaseSchemaManager and update the DB schema if needed
        public async Task UpdateSqlCompactSchema(string databasePath, Logger statusCallback = null)
        {
            try
            {
                statusCallback ??= delegate { };

                statusCallback(Environment.NewLine + "Checking for schema updates...");

                if (_schemaManagerProvider.GetSqlCompactSchemaManager(databasePath) is IDatabaseSchemaManager updater)
                {
                    try
                    {
                        using SqlCeConnection connection = new(SqlCompactMethods.BuildDBConnectionString(databasePath, true));
                        updater.SetDatabaseConnection(connection);
                        if (updater.IsUpdateRequired)
                        {
                            using CancellationTokenSource tokenSource = new();
                            string backupPath = await updater.BeginUpdateToLatestSchema(null, tokenSource).ConfigureAwait(false);

                            if (!string.IsNullOrEmpty(backupPath))
                            {
                                File.Delete(backupPath);
                            }

                            statusCallback(" Updated schema.");
                        }
                        else
                        {
                            statusCallback(" No updates required.");
                        }
                    }
                    finally
                    {
                        // ContextTagDatabaseManager is an IDisposable, e.g.
                        if (updater is IDisposable disposable)
                        {
                            disposable.Dispose();
                        }
                    }
                }
                else
                {
                    statusCallback(" No schema manager found.");
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51789");
            }
        }

        /// Break-up scripts so they run faster (workaround poor interop code)
        public static IEnumerable<string> BreakupSqliteScript(IEnumerable<string> scriptLines)
        {
            _ = scriptLines ?? throw new ArgumentNullException(nameof(scriptLines));

            StringBuilder accumulator = new();
            bool previousLineEndedWithCloseParenSemicolon = false;

            foreach (string line in scriptLines)
            {
                // Use a heuristic to parse the files:
                // If the previous line ended in ); and this line starts with INSERT INTO
                // then it is probably safe to assume the script can be split here
                // (although it is possible this would be part of a multi-line string it probably won't happen and if it does then
                // we can manually convert the DB with SqlCe35Toolbox.exe and sqlite3.exe)
                if (previousLineEndedWithCloseParenSemicolon && line.StartsWith("INSERT INTO ", StringComparison.Ordinal))
                {
                    string result = accumulator.ToString().TrimEnd();
                    accumulator.Clear().AppendLine(line);
                    previousLineEndedWithCloseParenSemicolon = false;
                    yield return result;
                }
                else
                {
                    accumulator.AppendLine(line);
                    previousLineEndedWithCloseParenSemicolon = line.EndsWith(");", StringComparison.Ordinal);
                }
            }

            if (accumulator.Length > 0)
            {
                yield return accumulator.ToString().TrimEnd();
            }
        }

        /// <summary>
        /// Creates a sqlite database at the supplied path if the file doesn't already exist and there is
        /// an existing sql compact database with the same name (the extension of the supplied path changed to .sdf)
        /// </summary>
        /// <param name="databaseFile">The path to a sqlite database that may or may not exist</param>
        /// <returns>True if the file was created by this method and false if it was not</returns>
        public static async Task<bool> ConvertDatabaseIfNeeded(string databaseFile)
        {
            string legacyDatabaseFile = Path.ChangeExtension(databaseFile, ".sdf");
            if (File.Exists(legacyDatabaseFile) && !File.Exists(databaseFile))
            {
                var converter = new DatabaseConverter(new DatabaseSchemaManagerProvider());
                await converter.Convert(legacyDatabaseFile, databaseFile).ConfigureAwait(false);

                // https://extract.atlassian.net/browse/ISSUE-17670
                RenameOldDatabaseFile(legacyDatabaseFile);

                return true;
            }

            return false;
        }

        /// Extract all paths that have been joined with ", " if they are derived from the base path
        /// (either exactly match or match with four-digit number inserted before the extension)
        public static string[] SplitConcatenatedPaths(string basePath, string concatenatedPaths)
        {
            _ = concatenatedPaths ?? throw new ArgumentNullException(nameof(concatenatedPaths));

            if (!concatenatedPaths.Contains(","))
            {
                return new[] { concatenatedPaths };
            }

            // Don't assume the temp path is free from commas and simply split the concatenated paths!
            string directoryPattern = Regex.Escape(Path.GetDirectoryName(basePath));
            string namePattern = Regex.Escape(Path.GetFileNameWithoutExtension(basePath));
            string extPattern = Regex.Escape(Path.GetExtension(basePath));
            string pathPattern = $@"(\G{directoryPattern}[\\/]{namePattern}(?:_\d{{4}})?{extPattern})(?:,\x20|$)";

            string[] paths = Regex.Matches(concatenatedPaths, pathPattern).Cast<Match>().Select(m => m.Groups[1].Value).ToArray();

            ExtractException.Assert("ELI51919", "Logic error: At least one path should have been extracted", paths.Length > 0);

            return paths;
        }

        // Export a SQL Compact DB to a script file(s)
        // Returns an array with the script file paths that were exported
        private static string[] ExportSqlCompact(string inputPath, string outputPath, Logger statusCallback = null)
        {
            statusCallback ??= delegate { };

            statusCallback(Environment.NewLine + "Exporting database...");

            string exportStatus;
            string finalFiles;

            // ErikEJ.SqlCeScripting code is not thread-safe (Helper.FinalFiles is a static, e.g.)
            lock (_sqlceMonitor)
            {
                using IRepository repository = new DBRepository($"Data Source='{inputPath}';");
                IGenerator generator = new Generator(repository, outputPath);
                exportStatus = generator.ScriptDatabaseToFile(Scope.SchemaDataSQLite);
                finalFiles = Helper.FinalFiles;
            }

            statusCallback(" " + exportStatus);

            return SplitConcatenatedPaths(outputPath, finalFiles);
        }

        // Run scripted commands to create a SQLite DB
        private static void ImportToSqlite(string[] inputPaths, string outputPath, Logger statusCallback = null)
        {
            statusCallback ??= delegate { };

            statusCallback(Environment.NewLine + "Importing into new database...");

            Stopwatch stopwatch = new();
            stopwatch.Start();

            // Create the folder for the output if it is missing
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            SQLiteTools.CreateDatabase(outputPath, deleteIfExists: true);
            using DataConnection db = SQLiteTools.CreateDataConnection(SqliteMethods.BuildConnectionString(outputPath));
            int rowsAffected = 0;

            foreach (string inputPath in inputPaths)
            {
                foreach (string commandText in BreakupSqliteScript(File.ReadLines(inputPath)))
                {
                    db.Command.CommandText = commandText;
                    rowsAffected += db.Command.ExecuteNonQuery();
                }
            }

            string importStatus = $"Created database at {outputPath}" + Environment.NewLine
                + $"{rowsAffected} rows inserted/updated in {stopwatch.ElapsedMilliseconds} ms";

            statusCallback(" Done." + Environment.NewLine + importStatus);
        }

        // Rename by appending a timestamp and .bak
        private static void RenameOldDatabaseFile(string legacyDatabaseFile)
        {
            string backupFile = FileSystemMethods.BuildTimeStampedBackupFileName(legacyDatabaseFile, false) + ".bak";
            try
            {
                FileSystemMethods.MoveFile(legacyDatabaseFile, backupFile, true);
            }
            catch (IOException ex)
            {
                ex.ExtractLog("ELI51936");
            }
        }
    }
}
