using EnvDTE;
using EnvDTE80;
using Extract.SourceControl;
using Microsoft;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using SourceControl;

namespace LICode
{
    /// <summary>
    /// Represents a grouping of Source Control methods.
    /// </summary>
    public static class SourceControlMethods
    {
        /// <summary>
        /// Checks out a source control item from the path in a particular repository.
        /// </summary>
        /// <param name="database">The database from which to retrieve the item.</param>
        /// <param name="path">The path to the source control item.</param>
        /// <returns>A source control item in <paramref name="database"/> at 
        /// <paramref name="path"/>.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId="CheckOut")]
        public static ISourceControlItem CheckOutItemFromPath(ISourceControl database, string path)
        {
            // get the vss item for this vss path
            ISourceControlItem item = database.GetItem(path);

            // get the LI code dat file from Source Safe
            if (item == null)
            {
                throw new InvalidOperationException("Could not get file from database: " + path);
            }
            else
            {
                // TODO: Check out could be more efficient if check out is attempted first and 
                // checking if someone else has item checked is done in exception handling

                // check if the file is already checked out
                if (!item.IsCheckedOut)
                {
                    // the file is not checked out, check out this item from SourceSafe
                    item.CheckOut();
                }
                else if (!item.IsCheckedOutToMe)
                {
                    // the file is checked out to someone else, throw an exception
                    throw new InvalidOperationException(item.UserWhoCheckedOut 
                        + " has already checked out " + path + ". Please try again later.");
                }
            }
            return item;
        }

        /// <summary>
        /// Opens the source control database for the current Visual Studio session.
        /// </summary>
        /// <returns>The source control database for the current Visual Studio session.</returns>
        public static ISourceControl OpenSourceControlDatabase()
        {
            // Open the source control database
            ISourceControl db = SourceControlFactory.Create(new LogOnSettings(),
                RegistryManager.EngineeringRoot);

            // TODO: The repository is hard-coded at the moment. What's a better way?
            //db.Open(null);

            return db;
        }
    } 
}
