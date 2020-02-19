using Newtonsoft.Json;
using System;
using System.Data.Common;

namespace DatabaseMigrationWizard.Database.Input
{
    [FlagsAttribute]
    public enum Priorities : Int32
    {
        None = 0,
        Low = 2,
        MediumLow = 4,
        Medium = 8,
        MediumHigh = 16,
        High = 32
    };

    public interface ISequence
    {
        /// <summary>
        /// The higher the priority, the sooner it will be run.
        /// </summary>
        Priorities Priority { get; }

        string TableName { get; }

        void ExecuteSequence(DbConnection dbConnection, ImportOptions importOptions);
    }

}
