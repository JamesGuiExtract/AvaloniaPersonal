using System.Collections.Generic;

namespace WebAPI
{
    public class ConfigurationData
    {
        public IList<string> Configurations { get; set; }
        public string ActiveConfiguration { get; set; }
    }
}
