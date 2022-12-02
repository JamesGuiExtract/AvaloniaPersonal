using Extract.Web.ApiConfiguration.Models;
using System;

namespace Extract.Web.ApiConfiguration.ViewModels
{
    public interface ICommonApiConfigViewModel
    {
        Guid ID { get; }

        string ConfigurationName { get; set; }

        string ConfigurationNamePlus { get; }

        bool IsDefault { get; set; }

        string WorkflowName { get; set; }

        string ConfigurationDisplayType { get; }

        Type ConfigurationType { get; }

        bool IsDirty { get; }

        bool IsSaving { get; }

        void Save();

        void UpdateFromModel(ICommonWebConfiguration config);
    }
}