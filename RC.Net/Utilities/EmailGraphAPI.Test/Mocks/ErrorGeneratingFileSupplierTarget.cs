using System;
using UCLID_FILEPROCESSINGLib;

namespace Extract.Email.GraphClient.Test.Mocks
{
    /// <summary>
    /// Pass through most calls to a real file supplier target but randomly throw
    /// exceptions for NotifyFileAdded in order to test EmailFileSupplier error handling
    /// </summary>
    internal class ErrorGeneratingFileSupplierTarget : IFileSupplierTarget
    {
        readonly IFileSupplierTarget _databaseFileSupplierTarget;
        readonly int _errorPercent;
        readonly Random _rng = new();

        public ErrorGeneratingFileSupplierTarget(IFileSupplierTarget databaseFileSupplierTarget, int errorPercent)
        {
            _databaseFileSupplierTarget = databaseFileSupplierTarget;
            _errorPercent = errorPercent;
        }

        public FileRecord NotifyFileAdded(string bstrFile, IFileSupplier pSupplier)
        {
            if (_rng.Next(0, 100) < _errorPercent)
            {
                throw new ExtractException("ELI53447", "Test NotifyFileAdded");
            }
            return _databaseFileSupplierTarget.NotifyFileAdded(bstrFile, pSupplier);
        }

        public void NotifyFileRemoved(string bstrFile, IFileSupplier pSupplier)
        {
            _databaseFileSupplierTarget.NotifyFileRemoved(bstrFile, pSupplier);
        }

        public void NotifyFileRenamed(string bstrOldFile, string bstrNewFile, IFileSupplier pSupplier)
        {
            _databaseFileSupplierTarget.NotifyFileRenamed(bstrOldFile, bstrNewFile, pSupplier);
        }

        public void NotifyFolderDeleted(string bstrFolder, IFileSupplier pSupplier)
        {
            _databaseFileSupplierTarget.NotifyFolderDeleted(bstrFolder, pSupplier);
        }

        public void NotifyFolderRenamed(string bstrOldFolder, string bstrNewFolder, IFileSupplier pSupplier)
        {
            _databaseFileSupplierTarget.NotifyFolderRenamed(bstrOldFolder, bstrNewFolder, pSupplier);
        }

        public void NotifyFileModified(string bstrFile, IFileSupplier pSupplier)
        {
            _databaseFileSupplierTarget.NotifyFileModified(bstrFile, pSupplier);
        }

        public void NotifyFileSupplyingDone(IFileSupplier pSupplier)
        {
            _databaseFileSupplierTarget.NotifyFileSupplyingDone(pSupplier);
        }

        public void NotifyFileSupplyingFailed(IFileSupplier pSupplier, string strError)
        {
            _databaseFileSupplierTarget.NotifyFileSupplyingFailed(pSupplier, strError);
        }
    }
}
