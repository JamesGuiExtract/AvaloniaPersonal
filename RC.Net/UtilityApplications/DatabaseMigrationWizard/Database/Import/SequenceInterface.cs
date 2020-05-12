using Newtonsoft.Json;
using System;
using System.Data.Common;

namespace DatabaseMigrationWizard.Database.Input
{
    [FlagsAttribute]
    public enum Priorities : Int32
    {
        None = 0,
        Reporting = 2,
        Low = 4,
        MediumLow = 8,
        Medium = 16,
        MediumHigh = 32,
        High = 64
    };

    public interface ISequence
    {
        /// <summary>
        /// The higher the priority, the sooner it will be run.
        /// </summary>
        Priorities Priority { get; }

        /// <summary>
        /// Returns the table name of its representing class.
        /// </summary>
        string TableName { get; }

        /// <summary>
        /// Executes the import for the respective table.
        /// </summary>
        /// <param name="importOptions">The options for importing</param>
        void ExecuteSequence(ImportOptions importOptions);
    }

}
