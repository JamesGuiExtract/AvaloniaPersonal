using DatabaseMigrationWizard.Database;
using Extract;
using Extract.SqlDatabase;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DatabaseMigrationWizard.Pages.Utility
{
    public static class Universal
    {
        /// <summary>
        /// Return all the classes that implement the Interface <typeparamref name="T"/>
        /// </summary>
        /// <typeparam name="T">The interface implemented</typeparam>
        /// <returns>Returns Ienum containing all the interfaces.</returns>
        public static IEnumerable<T> GetClassesThatImplementInterface<T>()
        {
            try
            {
                return Assembly.GetExecutingAssembly()
                    .GetTypes()
                    .Where(type => typeof(T).IsAssignableFrom(type))
                    .Where(type =>
                        !type.IsAbstract &&
                        !type.IsGenericType &&
                        type.GetConstructor(new Type[0]) != null)
                    .Select(type => (T)Activator.CreateInstance(type))
                    .ToList();
            }
            catch(Exception e)
            {
                throw e.AsExtract("ELI49684");
            }
        }

        public static void BrowseToFolder(string filePath)
        {
            if(Directory.Exists(filePath))
            {
                System.Diagnostics.Process.Start("explorer.exe", filePath);
            }
        }

        public static string SelectFolder()
        {
            try
            {
                return new FileBrowser().BrowseForFolder("Database Migration Wizard", "C:\\") ?? string.Empty;
            }
            catch(Exception e)
            {
                throw new ExtractException("ELI49694", "Error selecting the folder", e);
            }
        }

        // Checks to see if the database server is valid
        public static bool IsDBServerValid(ConnectionInformation connectionInformation)
        {
            try
            {
                if (string.IsNullOrEmpty(connectionInformation.DatabaseServer))
                {
                    return false;
                }
                using var sqlConnection = new SqlConnection($@"Server={connectionInformation.DatabaseServer};Integrated Security=SSPI; Connection Timeout=5");

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Tries to read the database names, returns an empty list if it fails
        /// or if the user does not have permissions to the table.
        /// </summary>
        /// <param name="connectionInformation"></param>
        /// <returns></returns>
        public static Collection<string> GetDatabaseNames(ConnectionInformation connectionInformation)
        {
            Collection<string> databaseNames = new Collection<string>();
            try
            {
                using var sqlConnection = new SqlConnection($@"Server={connectionInformation.DatabaseServer};Integrated Security=SSPI; Connection Timeout=5");

                using var command = sqlConnection.CreateCommand();
                command.CommandText = "SELECT name FROM master.sys.databases";
                using SqlDataReader rdr = command.ExecuteReader();

                while (rdr.Read())
                {
                    var myString = rdr.GetString(0);
                    databaseNames.Add(myString);
                }
            }
            catch (Exception) { }

            return databaseNames;
        }

        // Checks to see if the database name is valid. It also assumes the server is correct.
        public static bool IsDBDatabaseValid(ConnectionInformation connectionInformation)
        {
            try
            {
                if (String.IsNullOrEmpty(connectionInformation.DatabaseName))
                {
                    return false;
                }
                using (var sqlConnection = new SqlConnection($@"Server={connectionInformation.DatabaseServer};Database={connectionInformation.DatabaseName};Integrated Security=SSPI; Connection Timeout=5"))
                {
                    sqlConnection.Open();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
