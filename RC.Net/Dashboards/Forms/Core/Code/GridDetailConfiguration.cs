using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Extract.Dashboard.Forms
{
    /// <summary>
    /// Configuration data class for DashboardFileDetailConfigurationForm
    /// </summary>
    public class GridDetailConfiguration : IEquatable<GridDetailConfiguration>
    {
        public string DashboardGridName { get; set; }

        /// <summary>
        /// SQL Query used for populating the DashboardFileDetailForm used when double clicking on a row  in a grid that
        /// has a FileName member that is defined with DataMemberUsedForFileName
        /// </summary>
        public string RowQuery { get; set; }

        /// <summary>
        /// The DataMember defined in the data query used for a grid that is included in a grid as a Dimension that will
        /// be interpreted as a FileName used for any actions that use a file name
        /// </summary>
        public string DataMemberUsedForFileName { get; set; }

        /// <summary>
        /// This contains all the dashboards in a database that have been configured to be opened  via the "Open
        /// Dashboard" menu for a grid
        /// </summary>
        public HashSet<string> DashboardLinks { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        #region Operators

        public static bool operator ==(GridDetailConfiguration first, GridDetailConfiguration second)
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
                throw ex.AsExtract("ELI49774");
            }
        }

        public static bool operator !=(GridDetailConfiguration first, GridDetailConfiguration second)
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
                throw ex.AsExtract("ELI49775");
            }
        }
        #endregion

        #region Overrides

        public override int GetHashCode()
        {
            try
            {
                return HashCode.Start
                               .Hash(DashboardGridName)
                               .Hash(DataMemberUsedForFileName)
                               .Hash(RowQuery)
                               .Hash(DashboardLinks.Aggregate(HashCode.Start, (current, next) => current.Hash(next)));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49776");
            }
        }

        public override bool Equals(object obj)
        {
            try
            {
                if (obj == null)
                {
                    return false;
                }

                var tmp = obj as GridDetailConfiguration;
                if (tmp == null)
                {
                    return false;
                }

                return Equals(tmp);
            }
            catch(Exception ex)
            {
                throw ex.AsExtract("ELI49777");
            }
        }

        #endregion

        #region IEquatable<GridDetailConfiguration> Implementation

        public bool Equals(GridDetailConfiguration other)
        {
            try
            {
                if (other == null)
                {
                    return false;
                }

                return this.DashboardGridName == other.DashboardGridName &&
                    this.RowQuery == other.RowQuery &&
                    this.DataMemberUsedForFileName == other.DataMemberUsedForFileName &&

                    this.DashboardLinks.SetEquals(other.DashboardLinks);
            }
            catch(Exception ex)
            {
                throw ex.AsExtract("ELI49778");
            }
        }

        #endregion
    }
}
