using System.Data.Linq.Mapping;

namespace Extract.Database
{
    /// <summary>
    /// Table mapping definition for the settings table
    /// </summary>
    [Table()]
    public class Settings
    {
        /// <summary>
        /// The Name column of the settings table.
        /// </summary>
        string _name;

        /// <summary>
        /// The Value column of the settings table.
        /// </summary>
        string _value;

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        [Column(Name = "Name", IsPrimaryKey = true, DbType = "NVARCHAR(100)")]
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        [Column(Name = "Value", DbType = "NVARCHAR(512)")]
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
            }
        }
    }
}
