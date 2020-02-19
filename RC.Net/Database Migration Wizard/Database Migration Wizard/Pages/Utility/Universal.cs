using DatabaseMigrationWizard.Database;
using Extract;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

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
                throw new ExtractException("ELI49684", "Could not get interfaces for some reason? This should never be seen...", e);
            }
        }

        /// <summary>
        /// Waits for all threads to finish processing.
        /// </summary>
        /// <param name="threads">An IEnumerable containing several threads.</param>
        public static void WaitAll(this IEnumerable<Thread> threads)
        {
            try
            {
                if (threads != null)
                {
                    foreach (Thread thread in threads)
                    { thread.Join(); }
                }
            }
            catch (Exception e)
            {
                throw new ExtractException("ELI49677", "One of the threads failed to complete", e);
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

        public static bool IsDBConfigurationValid(ConnectionInformation connectionInformation)
        {
            try
            {
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
