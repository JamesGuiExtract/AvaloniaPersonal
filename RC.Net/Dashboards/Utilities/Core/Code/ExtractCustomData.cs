using DevExpress.Utils.Extensions;
using Extract.Dashboard.Forms;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;


namespace Extract.Dashboard.Utilities
{
    public class ExtractCustomData : IEquatable<ExtractCustomData>
    {
        #region Constructors

        public ExtractCustomData()
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="xml">The complete xml of the dashboard definition</param>
        public ExtractCustomData(XDocument dashboardXml)
            : base()
        {
            ExtractConfigData(new XDocument(dashboardXml));
        }
        #endregion

        #region Constants
        /// <summary>
        /// Version 1 was not represented by a class and did not have a version This should be incremented when things
        /// are added or removed from UserData to be saved
        /// </summary
        public const int ExtractUserDataVersion = 2;

        #endregion

        #region Public Properties

        /// <summary>
        /// The key used is the control name
        /// </summary>
        public Dictionary<string, GridDetailConfiguration> CustomGridValues
        {
            get;
        } = new Dictionary<string, GridDetailConfiguration>();

        /// <summary>
        /// Flag that indicates that the dashboard does not require the Dashboard apps to be licenses
        /// </summary>
        public bool CoreLicensed { get; set; } = false;

        #endregion

        #region Public Methods

        /// <summary>
        /// Clears the ExtractCustomData instance
        /// </summary>
        public void ClearData()
        {
            try
            {
                CoreLicensed = false;
                CustomGridValues.Clear();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49750");
            }
        }

        /// <summary>
        /// Adds the UserData element to the <paramref name="dashboardXml"/> and returns it"
        /// </summary>
        /// <param name="dashboardXml">The xml to add the UserData element to</param>
        /// <returns>Modified <paramref name="dashboardXml"/></returns>
        public XDocument AddExtractCustomDataToDashboardXml(XDocument dashboardXml)
        {
            try
            {
                // Clear existing user data
                ClearUserData(dashboardXml);

                XElement extractCustomData = new XElement("ExtractCustomData");

                extractCustomData.Add(new XElement("Version", ExtractUserDataVersion));

                if (CustomGridValues.Count > 0)
                {
                    var gridsItems = dashboardXml.XPathSelectElements("/Dashboard/Items/Grid").ToList();

                    // Create elements only for grid components that exist in the dashboard
                    var gridElements = CustomGridValues
                        .Where(kvp => gridsItems.Select(g => g.Attribute("ComponentName").Value).Contains(kvp.Key))
                        .Select(kv => CreateGridComponentElement(kv.Key, kv.Value));

                    extractCustomData.Add(new XElement("ExtractConfiguredGrids", gridElements));
                }
                var dashboardHash = CalculateHashCode(dashboardXml);

                extractCustomData.Add(new XElement("CoreLicensed", dashboardHash ^ ((CoreLicensed) ? 1 : 0)));

                // Add extractCustomData to the User data
                dashboardXml.XPathSelectElement("/Dashboard/UserData").Add(extractCustomData);
                return dashboardXml;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49749");
            }
        }
        public void AssignDataFromDashboardDefinition(XDocument dashboardXml)
        {
            ExtractConfigData(new XDocument(dashboardXml));
        }

        #endregion

        #region IEquatable<ExtractCustomData> implementation

        public bool Equals(ExtractCustomData other)
        {
            if (other == null)
            {
                return false;
            }

            return CoreLicensed == other.CoreLicensed &&
                CustomGridValues
                     .OrderBy(kvp => kvp.Key)
                     .SequenceEqual(other.CustomGridValues.OrderBy(kvp => kvp.Key));
        }

        #endregion


        #region Operators

        public static bool operator ==(ExtractCustomData first, ExtractCustomData second)
        {
            try
            {
                if (((object)first) == null || ((object)second) == null)
                {
                    return Object.Equals(first, second);
                }

                return first.Equals(second);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49761");
            }
        }

        public static bool operator !=(ExtractCustomData first, ExtractCustomData second)
        {
            try
            {
                if (((object)first) == null || ((object)second) == null)
                {
                    return !Object.Equals(first, second);
                }

                return !first.Equals(second);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49760");
            }
        }

        #endregion

        #region Overrides

        public override int GetHashCode()
        {
            return HashCode
                .Start
                .Hash(CoreLicensed)
                .Hash(CustomGridValues.Aggregate(HashCode.Start,
                                                 (current, next) => current.Hash(next.Key).Hash(next.Value)));
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var tmp = obj as ExtractCustomData;
            if (tmp == null)
            {
                return false;
            }

            return Equals(tmp);
        }

        #endregion



        #region Private methods

        static private XElement CreateGridComponentElement(string componentName, GridDetailConfiguration configuration)
        {
            string dataMemberUsedForFileName = string.IsNullOrWhiteSpace(configuration.DataMemberUsedForFileName)
                ? "FileName" : configuration.DataMemberUsedForFileName;
            var dataMemberUsedForFileNameElement = new XElement("DataMemberUsedForFileName", dataMemberUsedForFileName);
            var dashboardLinks = new XElement("DashboardLinks", string.Join(",", configuration.DashboardLinks));

            return new XElement("Component",
                                new XAttribute("Name", componentName),
                                new XElement("RowQuery", configuration.RowQuery),
                                dataMemberUsedForFileNameElement,
                                dashboardLinks);
        }

        private static void ClearUserData(XDocument dashboardXml)
        {
            try
            {
                var existingUserData = dashboardXml.XPathSelectElement("/Dashboard/UserData");
                if (existingUserData is null)
                {
                    var dashboardNode = dashboardXml.XPathSelectElement("Dashboard");
                    dashboardNode.LastNode.AddAfterSelf(new XElement("UserData", string.Empty));
                }
                else
                {
                    existingUserData.RemoveAll();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49759");
            }
        }

        /// <summary>
        /// Extract the config data from the dashboard
        /// </summary>
        private void ExtractConfigData(XDocument dashboardXml)
        {
            try
            {
                ClearData();

                IEnumerable<XElement> configuredGridData;

                // Build the path to grid components based on the existence of ExtractCustomData node
                var extractCustomData = dashboardXml?.XPathSelectElement("/Dashboard/UserData/ExtractCustomData");
                string pathToGridComponents = "/Dashboard/UserData/ExtractCustomData";
                if (extractCustomData is null)
                {
                    pathToGridComponents = "/Dashboard/UserData";
                }
                pathToGridComponents += "/ExtractConfiguredGrids/Component";

                configuredGridData = dashboardXml?.XPathSelectElements(pathToGridComponents);
                ExtractConfiguredGridsData(configuredGridData, dashboardXml);

                var coreLicenseHash = dashboardXml?.XPathSelectElement("/Dashboard/UserData/ExtractCustomData/CoreLicensed")?.Value;

                // Remove the ExtractCustomData from the dashboard xml
                var userDataNode = dashboardXml?.XPathSelectElement("/Dashboard/UserData");
                userDataNode?.RemoveAll();

                // Now get the hash without the user data
                var dashboardHash = CalculateHashCode(dashboardXml);

                CoreLicensed = (coreLicenseHash != null) &&
                    (int.Parse(coreLicenseHash, CultureInfo.InvariantCulture) ^ dashboardHash) == 1;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49746");
            }
        }

        private static HashCode CalculateHashCode(XDocument dashboardXml)
        {
            try
            {
                // normalize xml empty elements to have separate closing tags
                dashboardXml
                    .DescendantNodes()
                    .OfType<XElement>()
                    .Where(x => x.IsEmpty)
                    .ForEach(e => e.Value = string.Empty);
                return HashCode
                    .Start
                    .Hash(dashboardXml.XPathSelectElement("/Dashboard").ToString(SaveOptions.None));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49762");
            }
        }

        /// <summary>
        /// Extracts the Extract Grid configuration
        /// </summary>
        /// <param name="configuredGridData"></param>
        private void ExtractConfiguredGridsData(IEnumerable<XElement> configuredGridData, XDocument dashboardXml)
        {
            try
            {
                var gridsItems = dashboardXml.XPathSelectElements("/Dashboard/Items/Grid").ToList();
                foreach (var e in configuredGridData)
                {
                    var dashboardItem = gridsItems
                        .Any(i => i.Attribute("ComponentName").Value == e.Attribute("Name").Value);
                    if (dashboardItem)
                    {
                        var gridDetailConfig = new GridDetailConfiguration
                        {
                            DashboardGridName = e.Attribute("Name").Value,
                            RowQuery = e.Element("RowQuery").Value,
                            DataMemberUsedForFileName = e.Element("DataMemberUsedForFileName")?.Value ?? "FileName"
                        };
                        var links = e.Element("DashboardLinks")?.Value.Split(',')?
                                .Where(s => !string.IsNullOrWhiteSpace(s));
                        if (links != null && links.Count() > 0)
                        {
                            gridDetailConfig.DashboardLinks.AddRange(links);
                        }

                        CustomGridValues[e.Attribute("Name").Value] = gridDetailConfig;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49763");
            }
        }
        #endregion
    }
}
