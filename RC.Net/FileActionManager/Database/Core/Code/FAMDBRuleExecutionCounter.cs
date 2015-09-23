using Extract.Interfaces;
using System;
using System.Runtime.InteropServices;
using UCLID_FILEPROCESSINGLib;
using Extract.Licensing;

namespace Extract.FileActionManager.Database
{
    /// <summary>
    /// The interface for <see cref="FAMDBRuleExecutionCounter"/>
    /// </summary>
    [ComVisible(true)]
    [Guid("FF01AAAD-2CC7-40D0-A3D2-FB4932BBB3E1")]
    [CLSCompliant(false)]
    public interface IFAMDBRuleExecutionCounter
    {
        /// <summary>
        /// Initializes the counter of the specified <see paramref="counterID"/> against the
        /// specified <see paramref="fileProcessingDB"/>.
        /// </summary>
        /// <param name="fileProcessingDB">The <see cref="FileProcessingDB"/> where the counts will
        /// be tracked.</param>
        /// <param name="counterID">The ID of the counter.</param>
        void Initialize(FileProcessingDB fileProcessingDB, int counterID);
    }

    /// <summary>
    /// A class used for tracking counts decremented by rule execution in the
    /// <see cref="FileProcessingDB"/>.
    /// </summary>
    [ComVisible(true)]
    [Guid("8CDB95DB-D580-4E26-B4FE-2EF777E3E712")]
    [ProgId("Extract.FileActionManager.Database.FAMDBRuleExecutionCounter")]
    [CLSCompliant(false)]
    public class FAMDBRuleExecutionCounter: IFAMDBRuleExecutionCounter, IRuleExecutionCounter
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(FAMDBRuleExecutionCounter).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// For testing only.
        /// </summary>
        static int _countsRemaining = 10;

        /// <summary>
        /// The ID of the counter.
        /// </summary>
        int _counterID;

        /// <summary>
        /// The name of the counter.
        /// </summary>
        string _counterName;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FAMDBRuleExecutionCounter"/> class.
        /// </summary>
        public FAMDBRuleExecutionCounter()
        {
        }

        #endregion Constructors

        #region IFAMDBRuleExecutionCounter

        /// <summary>
        /// Initializes the counter of the specified <see paramref="counterID"/> against the
        /// specified <see paramref="fileProcessingDB"/>.
        /// </summary>
        /// <param name="fileProcessingDB">The <see cref="FileProcessingDB"/> where the counts will
        /// be tracked.</param>
        /// <param name="counterID">The ID of the counter.</param>
        public void Initialize(FileProcessingDB fileProcessingDB, int counterID)
        {
            try
            {
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.ExtractCoreObjects, "ELI38744", _OBJECT_NAME);

                _counterID = counterID;
                _counterName = "ID Shield - Redaction (By Page)";
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI38746",
                    "Failed to initialize '" + CounterName + "' counter");
            }
        }

        #endregion IFAMDBRuleExecutionCounter

        #region IRuleExecutionCounter

        /// <summary>
        /// Gets the ID of the counter.
        /// </summary>
        public int CounterID
        {
            get
            {
                return _counterID;
            }
        }

        /// <summary>
        /// Gets the name of the counter.
        /// </summary>
        public string CounterName
        {
            get
            {
                return _counterName;
            }
        }

        /// <summary>
        /// Decrements the counter by the specified <see paramref="count"/> assuming enough counts
        /// are available.
        /// </summary>
        /// <param name="count">The number of counts to decrement.</param>
        /// <returns>The new number of counts left or -1 if there were not enough counts to be able
        /// to decrement.</returns>
        public int DecrementCounter(int count)
        {
            try
            {
                if (count > _countsRemaining)
                {
                    return -1;
                }
                else
                {
                    _countsRemaining -= count;
                    return _countsRemaining;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI38747",
                    "Failed to decrement '" + CounterName + "' counter");
            }
        }

        #endregion IRuleExecutionCounter
    }
}
