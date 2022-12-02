using System;

namespace Extract.Web.ApiConfiguration.Models
{
    public class ConfigurationForEditing
    {
        public string NameColumn { get; }
        public ICommonWebConfiguration Configuration { get; }

        public ConfigurationForEditing(string nameColumn, ICommonWebConfiguration configuration)
        {
            _ = nameColumn ?? throw new ArgumentNullException(nameof(nameColumn));
            _ = configuration ?? throw new ArgumentNullException(nameof(configuration));

            NameColumn = nameColumn;
            Configuration = configuration;
        }
    }
}
