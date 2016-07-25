using Extract.Database;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Extract.LabResultsCustomComponents
{
    /// <summary>
    /// Used to represent the table and column names for ResultComponent that will be changed eventually
    /// </summary>
    internal class ResultComponent
    {
        private ResultComponent() {}
        public static string Table = "[LabTest]";
        public static string Code = Table+".[TestCode]";
        public static string Name = Table+".[OfficialName]";
    }

    /// <summary>
    /// Used to represent the table and column names for Order that will be changed eventually
    /// </summary>
    internal class Order
    {
        private Order() {}
        public static string Table = "[LabOrder]";
        public static string Code = Table+".[Code]";
        public static string Name = Table+".[Name]";
    }

    /// <summary>
    /// Used to represent the table and column names for Order that will be changed eventually
    /// </summary>
    internal class OrderComponent
    {
        private OrderComponent() {}
        public static string Table = "[LabOrderTest]";
        public static string OrderCode = Table+".[OrderCode]";
        public static string ComponentCode = Table+".[TestCode]";
    }

    /// <summary>
    /// Represents a pair of Code and Name strings
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
    public class CodeNamePair
    {
        /// <summary>
        /// The code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// The name
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// The data source for the Components datagridview.
    /// </summary>
    public class ComponentsDataSource
    {
        #region Fields

        // The database connection used by this instance
        private DbConnection _connection;

        // Dictionaries to hold the data for easy access
        private Dictionary<string, HashSet<string>> _componentToOrders;
        private Dictionary<string, HashSet<string>> _orderToComponents;
        private Dictionary<string, string> _componentCodeToName;
        private Dictionary<string, string> _orderCodeToName;

        #endregion Fields

        #region Constructors
 
        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentsDataSource"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public ComponentsDataSource(DbConnection connection)
        {
            _connection = connection;
        }

        #endregion Constructors


        #region Public Methods
        /// <summary>
        /// Gets the components table.
        /// </summary>
        /// <param name="filter">optional filter to be applied to results</param>
        /// <returns>Data table of results</returns>
        public DataTable ComponentsTable(ComponentsFilter filter = null)
        {
            try
            {
                if (filter == null)
                {
                    // Note that the default CTOR create an "empty" filter.
                    // This greatly simplifies following code, no need to test for null filter.
                    filter = new ComponentsFilter(); 
                }
                filter.DataSource = this;

                // Compute mapped-to dictionary
                string query = "SELECT [ComponentCode], [ESComponentCode] FROM [ComponentToESComponentMap]";
                var mappedTo = DBMethods.ExecuteDBQuery(_connection, query)
                    .Rows.Cast<DataRow>()
                    .GroupBy(row => row.Field<string>(0),
                             row => row.Field<string>(1),
                             StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(group => group.Key,
                                  group => group,
                                  StringComparer.OrdinalIgnoreCase);

                // Compute order code to name dictionary
                query = " SELECT " + Order.Code + ", " + Order.Name
                        + " FROM " + Order.Table;
                _orderCodeToName = DBMethods.ExecuteDBQuery(_connection, query).Rows.Cast<DataRow>()
                    .ToDictionary(row => row.Field<string>(0),
                                  row => row.Field<string>(1),
                                  StringComparer.OrdinalIgnoreCase);

                // Compute order to component and component to order dictionaries
                query = " SELECT " + OrderComponent.OrderCode + ", " + OrderComponent.ComponentCode
                        + " FROM " + OrderComponent.Table;
                var orderComponent = DBMethods.ExecuteDBQuery(_connection, query);

                _orderToComponents = orderComponent.Rows.Cast<DataRow>()
                    .GroupBy(row => row.Field<string>(0),
                             row => row.Field<string>(1),
                             StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(group => group.Key,
                                  group => new HashSet<string>(group, StringComparer.OrdinalIgnoreCase),
                                  StringComparer.OrdinalIgnoreCase);

                _componentToOrders = orderComponent.Rows.Cast<DataRow>()
                    .GroupBy(row => row.Field<string>(1),
                             row => row.Field<string>(0),
                             StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(group => group.Key,
                                  group => new HashSet<string>(group, StringComparer.OrdinalIgnoreCase),
                                  StringComparer.OrdinalIgnoreCase);

                // Get list of result components
                query = "SELECT " + ResultComponent.Code + ", " + ResultComponent.Name
                        + " FROM " + ResultComponent.Table
                        + " ORDER BY " + ResultComponent.Code;
                var resultComponents = DBMethods.ExecuteDBQuery(_connection, query);

                var dt = new DataTable();
                dt.Locale = CultureInfo.CurrentCulture;

                dt.Columns.Add("Code", typeof(string));
                dt.Columns[0].MaxLength = resultComponents.Columns[0].MaxLength;

                dt.Columns.Add("Name", typeof(string));
                dt.Columns[1].MaxLength = resultComponents.Columns[1].MaxLength;

                dt.Columns.Add("Mapped to", typeof(string));
                dt.Columns[2].MaxLength = 500;

                dt.Columns.Add("# Orders", typeof(int));
                dt.Columns.Add("# Mapped components", typeof(int));

                // Initialize the map of result component codes to names
                _componentCodeToName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach(var row in resultComponents.Rows.Cast<DataRow>())
                {
                    string code = row.Field<string>(0);
                    string name = row.Field<string>(1);

                    // Fill in the map of codes to names
                    _componentCodeToName[code] = name;

                    int numberOfMappings = 0;
                    string mapTo = "";
                    IGrouping<string,string> mappings = null;
                    if (mappedTo.TryGetValue(code, out mappings))
                    {
                        numberOfMappings = mappings.Count();
                        mapTo = String.Join(", ", mappings);
                    }

                    int numberOfOrders = OrdersContainingComponent(code).Count;

                    if (!filter.Matches(code, name, numberOfMappings))
                        continue;

                    dt.Rows.Add(code, name, mapTo, numberOfOrders, numberOfMappings);
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39357");
            }
        }

        /// <summary>
        /// Gets a collection of order codes that contain the <paramref name="componentCode"/>
        /// </summary>
        /// <param name="componentCode">The component code</param>
        /// <returns>A collection of order codes that the <paramref name="componentCode"/> is a part of</returns>
        public HashSet<string> OrdersContainingComponent(string componentCode)
        {
            try
            {
                HashSet<string> orders = null;
                if (_componentToOrders.TryGetValue(componentCode, out orders))
                {
                    return orders;
                }
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39364");
            }
        }

        /// <summary>
        /// Gets a collection of components that belong to the order specified by <paramref name="orderCode"/>
        /// </summary>
        /// <param name="orderCode">The order code</param>
        /// <returns>A collection of the component codes that are a part of the <paramref name="orderCode"/></returns>
        public HashSet<string> ComponentsInOrder(string orderCode)
        {
            try
            {
                HashSet<string> components = null;
                if (_orderToComponents.TryGetValue(orderCode, out components))
                {
                    return components;
                }
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39365");
            }
        }


        /// <summary>
        /// Deletes the specified row(s).
        /// </summary>
        /// <param name="rowsToDelete">The DataRows to delete.</param>
        /// <returns></returns>
        public string[] DeleteRows(IEnumerable<DataRow> rowsToDelete)
        {
            List<string> results = new List<string>();

            foreach (var row in rowsToDelete)
            {
                string componentCode = row.Field<string>("Code");

                var cmd = "DELETE " + ResultComponent.Table + " WHERE " + ResultComponent.Code + " = @0";

                try
                {
                    DBMethods.ExecuteDBQuery(_connection, cmd,
                        new Dictionary<string, string>() { { "@0", componentCode } });

                    results.Add(String.Format(CultureInfo.CurrentCulture,
                        "Deleted: {0}, from table: {1}", componentCode, ResultComponent.Table));
                }
                catch (Exception ex)
                {
                    results.Add(String.Format(CultureInfo.CurrentCulture,
                        "Could not delete: {0}, {1}, from table: {2}.\nReason: {3}",
                        componentCode, ResultComponent.Table, ex.Message));
                }
            }

            return results.ToArray();
        }

        /// <summary>
        /// Gets the components represented by the selected rows
        /// </summary>
        /// <param name="selectedRows">The currently selected <see cref="DataRow"/>s</param>
        /// <returns>The components of the selected rows</returns>
        public static IEnumerable<CodeNamePair> GetSelectedComponents(IEnumerable<DataRow> selectedRows)
        {
            try
            {
                if (selectedRows.Count() == 0)
                {
                    return Enumerable.Empty<CodeNamePair>();
                }

                return selectedRows.Select(r =>
                    new CodeNamePair { Code = r.Field<string>("Code"), Name = r.Field<string>("Name") });
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39356");
            }
        }

        /// <summary>
        /// Gets a list of orders that contain all of the supplied component codes
        /// </summary>
        /// <param name="componentCodes">The component codes used to limit the orders</param>
        /// <returns>A collection of the orders that contain all of the selected components</returns>
        public IEnumerable<CodeNamePair> GetOrdersThatContainComponents(IEnumerable<string> componentCodes)
        {
            try
            {
                if (componentCodes.Count() == 0)
                {
                    return Enumerable.Empty<CodeNamePair>();
                }


                var orders = new HashSet<string>(OrdersContainingComponent(componentCodes.First()),
                        StringComparer.OrdinalIgnoreCase);

                foreach (var componentCode in componentCodes.Skip(1))
                {
                    orders.IntersectWith(OrdersContainingComponent(componentCode));
                }

                return orders.Select(code =>
                        new CodeNamePair { Code = code, Name = _orderCodeToName[code] });
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39359");
            }
        }

        #endregion Public Methods

    }           // end of class ComponentsDataSource


    /// <summary>
    /// Possible mapping status filter values
    /// </summary>
    public enum ComponentMappingStatus
    {
        /// <summary>
        /// Any mapping status
        /// </summary>
        All = 0,
        /// <summary>
        /// Only components mapped to at least one ESComponent
        /// </summary>
        Mapped,
        /// <summary>
        /// Only components that are not mapped to any ESComponent
        /// </summary>
        Unmapped
    }

    /// <summary>
    /// Class to represent the filter to be applied to the data
    /// </summary>
    public class ComponentsFilter
    {
        #region Fields

        // The contained-in-the-orders filter string
        private string _containedInTheOrders;

        // The set of component codes matching the contained-in-the-orders filter
        private HashSet<string> _componentCodesFilteredByOrder;

        // The data source to be used to get information about component/order relationships, e.g.
        private ComponentsDataSource _dataSource;

        #endregion Fields

        #region Properties

        /// <summary>
        /// The component code filter value
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// The component name filter value
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// The mapping status filter value
        /// </summary>
        public ComponentMappingStatus MappingStatus { get; set; }

        /// <summary>
        /// The <see cref="ComponentsDataSource"/> to be used by this filter
        /// </summary>
        public ComponentsDataSource DataSource
        {
            get
            {
                return _dataSource;
            }

            set
            {
                try
                {
                    if (value != _dataSource)
                    {
                        _dataSource = value;

                        // Compute the component code collection if the data source has been set
                        ComputeComponentCodesFilteredByOrder();
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI39355");
                }
            }
        }

        /// <summary>
        /// The string representation of the contained in the orders filter list
        /// </summary>
        public string ContainedInTheOrders
        {
            get
            {
                return _containedInTheOrders;
            }

            set
            {
                try
                {
                    if (value != _containedInTheOrders)
                    {
                        _containedInTheOrders = value;

                        // Recompute the component code collection if the filter has changed
                        ComputeComponentCodesFilteredByOrder();
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI40367");
                }
            }
        }

        /// <summary>
        /// Whether the component code filter is set (is not empty)
        /// </summary>
        public bool IsCodeFilterSet
        {
            get
            {
                return !String.IsNullOrWhiteSpace(Code);
            }
        }

        /// <summary>
        /// Whether the component name filter is set (is not empty)
        /// </summary>
        public bool IsNameFilterSet
        {
            get
            {
                return !String.IsNullOrWhiteSpace(Name);
            }
        }

        /// <summary>
        /// Whether the contained in the orders filter is set (is not empty)
        /// </summary>
        public bool IsContainedInTheOrdersFilterSet
        {
            get
            {
                return !String.IsNullOrWhiteSpace(ContainedInTheOrders);
            }
        }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Creates an empty filter that will match all components
        /// </summary>
        public ComponentsFilter()
        {
            MappingStatus = ComponentMappingStatus.All;
        }

        /// <summary>
        /// Creates a filter using the specified values
        /// </summary>
        /// <param name="code">The value of the component code filter</param>
        /// <param name="name">The value of the component name filter</param>
        /// <param name="containedInTheOrders">The string representation of the contained in the orders filter list</param>
        /// <param name="status">The value of the mapping status filter</param>
        public ComponentsFilter(string code, string name, string containedInTheOrders, ComponentMappingStatus status)
        {
            Code = code;
            Name = name;
            MappingStatus = status;
            ContainedInTheOrders = containedInTheOrders;
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Determines whether input args match the filter
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="name">The name.</param>
        /// <param name="numberOfMappings">The number of mappings that exist, used to 
        /// filter against the current mapping status</param>
        /// <returns>true on match, or false</returns>
        /// NOTE: This method supports simple wildcard searching.
        public bool Matches(string code, string name, int numberOfMappings)
        {
            try
            {
                // Result component code filter
                if (IsCodeFilterSet)
                {
                    if (ContainsWildCard(Code))
                    {
                        if (!IsWildCardMatch(code, Code))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (0 != String.Compare(code, Code, ignoreCase: true, culture: CultureInfo.CurrentCulture))
                        {
                            return false;
                        }
                    }
                }

                // Result component name filter
                if (IsNameFilterSet)
                {
                    if (ContainsWildCard(Name))
                    {
                        if (!IsWildCardMatch(name, Name))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (0 != String.Compare(name, Name, ignoreCase: true, culture: CultureInfo.CurrentCulture))
                        {
                            return false;
                        }
                    }
                }

                // Contained in the orders filter
                if (IsContainedInTheOrdersFilterSet)
                {
                    // If the collection is null that means that the DataSource has not been set so
                    // there is nothing to do here
                    ExtractException.Assert("ELI39366", "The DataSource must be set before the contained-in-the-orders filter can be used",
                        DataSource != null);

                    if (!_componentCodesFilteredByOrder.Contains(code))
                    {
                        return false;
                    }
                }

                // Mapping status filter
                switch (MappingStatus)
                {
                    case ComponentMappingStatus.Mapped:
                        if (numberOfMappings <= 0)
                        {
                            return false;
                        }
                        break;

                    case ComponentMappingStatus.Unmapped:
                        if (numberOfMappings > 0)
                        {
                            return false;
                        }
                        break;

                    default:
                        break;
                }

                // Else it matches
                return true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39354");
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Sets _componentCodesFilteredByOrder to the set of component codes that are contained in
        /// the list of orders matching the contained in orders filter
        /// </summary>
        private void ComputeComponentCodesFilteredByOrder()
        {
            if (DataSource == null || !IsContainedInTheOrdersFilterSet)
            {
                _componentCodesFilteredByOrder = new HashSet<string>();
                return;
            }

            // Compute the set of order to be filtered by
            // Allow order codes to be separated by semicolons. Optionally quoted. Capture order codes in OrderCodes group and unparseable text in 
            // UnParseable group.
            string orderListPattern =
                @"(?inx-m)
                  (?'UnParseable'[\S\s]*?)
                  (\s*
                    ( 
                        [""](?>(?'OrderCode'([^""]|[""]{2})+))[""]
                      | (?>(?'OrderCode'[^;""]+))
                    ) (\s*(;|\z))
                  )+
                  (?'UnParsable'[\S\s]*)
                ";

            var match = Regex.Match(ContainedInTheOrders, orderListPattern);
            var unparseableTextCaptures = match.Groups["UnParseable"].Captures;
            if (unparseableTextCaptures.Count > 0)
            {
                // TODO: Show error somehow
            }
            var limitToOrders = match.Groups["OrderCode"].Captures.Cast<Capture>().Select(c => c.Value);
            if (limitToOrders.Count() > 0)
            {
                _componentCodesFilteredByOrder =
                    new HashSet<string>(DataSource.ComponentsInOrder(limitToOrders.First()),
                        StringComparer.OrdinalIgnoreCase);
                foreach (var orderCode in limitToOrders.Skip(1))
                {
                    _componentCodesFilteredByOrder.UnionWith(DataSource.ComponentsInOrder(orderCode));
                }
            }
        }

        static private bool ContainsWildCard(string text)
        {
            return text.Contains('*') || text.Contains('?');
        }

        static private bool IsWildCardMatch(string text, string filterText)
        {
            var pattern = WildcardToRegex(filterText);

            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(text);
        }

        private static string WildcardToRegex(string pattern)
        {
            return "^" + Regex.Escape(pattern)
                              .Replace(@"\*", ".*")
                              .Replace(@"\?", ".")
                       + "$";
        }

        #endregion Private Methods

    }
}