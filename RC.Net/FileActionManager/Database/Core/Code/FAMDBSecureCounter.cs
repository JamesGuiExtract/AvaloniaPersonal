using Extract.Interfaces;
using Extract.Licensing;
using System;
using System.Runtime.InteropServices;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Database
{
    /// <summary>
    /// A class used for tracking counts decremented by rule execution in the
    /// <see cref="FileProcessingDB"/>.
    /// </summary>
    [ComVisible(true)]
    [Guid("8CDB95DB-D580-4E26-B4FE-2EF777E3E712")]
    [ProgId("Extract.FileActionManager.Database.FAMDBRuleExecutionCounter")]
    [CLSCompliant(false)]
    public class FAMDBSecureCounter : ISecureCounterCreator, ISecureCounter
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(FAMDBSecureCounter).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="FileProcessingDB"/> that is the source of this counter.
        /// </summary>
        FileProcessingDB _fileProcessingDB;

        /// <summary>
        /// The ID of the counter.
        /// </summary>
        int _id;

        /// <summary>
        /// The name of the counter.
        /// </summary>
        string _name;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FAMDBSecureCounter"/> class.
        /// </summary>
        public FAMDBSecureCounter()
        {
        }

        #endregion Constructors

        #region ISecureCounterCreator

        /// <summary>
        /// Initializes the counter of the specified <see paramref="counterID"/> against the
        /// specified <see paramref="fileProcessingDB"/>.
        /// </summary>
        /// <param name="pFAMDB">The <see cref="FileProcessingDB"/> that is the source of
        /// this counter.</param>
        /// <param name="nID">The ID of the counter.</param>
        public void Initialize(FileProcessingDB pFAMDB, int nID)
        {
            try
            {
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.ExtractCoreObjects, "ELI38744", _OBJECT_NAME);

                ExtractException.Assert("ELI38766", "Null argument exception.",
                    pFAMDB != null);

                _fileProcessingDB = pFAMDB;
                _id = nID;
                _name = _fileProcessingDB.GetSecureCounterName(_id);
                
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI38746", "Failed to initialize secure counter.");
            }
        }

        /// <summary>
        /// Applies an update code for this secure counter in order to increment or set its current
        /// value.
        /// </summary>
        /// <param name="bstrCode">The update code.</param>
        public void ApplyUpdateCode(string bstrCode)
        {
            try
            {
                 _fileProcessingDB.ApplySecureCounterUpdateCode(bstrCode);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI38768", "Failed to apply secure counter update code");
            }
        }

        #endregion ISecureCounterCreator

        #region ISecureCounter

        /// <summary>
        /// Gets the ID of the counter.
        /// </summary>
        public int ID
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Gets the name of the counter.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the value of the counter.
        /// </summary>
        public int Value
        {
            get
            {
                try
                {
                    return _fileProcessingDB.GetSecureCounterValue(_id);
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI38769",
                        "Failed to retrieve value of '" + Name + "' counter");
                }
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
                return _fileProcessingDB.DecrementSecureCounter(_id, count);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI38747",
                    "Failed to decrement '" + Name + "' counter");
            }
        }

        #endregion ISecureCounter
    }
}
