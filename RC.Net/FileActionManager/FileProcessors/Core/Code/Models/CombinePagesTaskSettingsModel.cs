using Extract.Utilities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Extract.FileActionManager.FileProcessors.Models
{
    public class PageSourceV1
    {
        public PageSourceV1(string document, string pages)
        {
            Document = document;
            Pages = pages;
        }

        public string Document { get; set; }
        public string Pages { get; set; }
    }

    public class CombinePagesTaskSettingsModelV1 : IDataTransferObject
    {

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public ReadOnlyCollection<PageSourceV1> PageSources { get; set; }
            = new List<PageSourceV1>().AsReadOnly();

        public string OutputPath { get; set; }

        public bool UpdateData { get; set; }

        public IDomainObject CreateDomainObject()
        {
            var instance = new CombinePagesTask();
            instance.CopyFrom(this);
            return instance;
        }
    }
}
