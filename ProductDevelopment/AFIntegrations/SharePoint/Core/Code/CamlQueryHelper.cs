using System.Collections.Generic;


namespace Extract.Sharepoint
{
    /// <summary>
    /// Interface used as a base for the caml based types so they all have camlString
    /// </summary>
    public interface camlBase
    {
        /// <summary>
        /// Method to return the Caml formatted string
        /// </summary>
        /// <returns>Caml formatted string</returns>
        string camlString();
    }

    /// <summary>
    /// Value to represent the value for a Caml query
    /// </summary>
    public class camlValue : camlBase
    {
        string _type;
        bool _includeTimeValue;
        string _value;

        /// <summary>
        /// ValueElement should always be created with parameters
        /// </summary>
        private camlValue()
        {
        }

        /// <summary>
        /// ValueElement with only a type and value - if type is DateTime the time will not be used
        /// for the comparison
        /// </summary>
        /// <param name="valueType">The Sharepoint type being queried</param>
        /// <param name="value">The value that is being queried</param>
        public camlValue(string valueType, string value)
        {
            _type = valueType;
            _value = value;
            _includeTimeValue = false;
        }

        /// <summary>
        /// Value element for a DateTime field that allows user to specifiy that the 
        /// time should be included as part of a comparison
        /// </summary>
        /// <param name="valueType">The name of a SPFieldType</param>
        /// <param name="value">Value that will be used in a comparison</param>
        /// <param name="includeTimeValue">Flag to indicate a DateTime comparison
        /// should use the time</param>
        public camlValue(string valueType, string value, bool includeTimeValue)
        {
            _type = valueType;
            _value = value;
            _includeTimeValue = includeTimeValue;
        }

        /// <summary>
        /// Used to generate the Value in the format used in a Caml Query
        /// </summary>
        /// <returns>String with value formated for use in a Caml Query</returns>
        public string camlString()
        {
            string rtnValue = "<Value ";

            if (_type == "DateTime")
            {
                rtnValue += "IncludeTimeValue=" + ((_includeTimeValue) ? "'TRUE' " : "'FALSE' ");
            }
            rtnValue += "Type='" + _type + "'>";

            if (_type == "Boolean")
            {
                // This allows other values to represent true or yes than "1" 
                if (_value.ToLower() == "yes" || _value.ToLower() == "true")
                {
                    rtnValue += "1" + "</Value>";
                }
                else if (_value.ToLower() == "no" || _value.ToLower() == "false")
                {
                    rtnValue += "1" + "</Value>";
                }
                else
                {
                    rtnValue += _value + "</Value>";
                }
            }
            else
            {
                rtnValue += _value + "</Value>";
            }

            return rtnValue;
        }
    }

    /// <summary>
    /// List to be used as a Values list in a Caml Query
    /// </summary>
    public class camlValues : List<camlValue>, camlBase
    {

        /// <summary>
        /// Initializes an empty list
        /// </summary>
        public camlValues()
        {
        }

        /// <summary>
        /// Initializes list with the passed in values
        /// </summary>
        /// <param name="values">List of Values</param>
        public camlValues(List<camlValue> values)
            : base(values)
        {
        }

        /// <summary>
        /// Used to generate the list of values in a format used by Caml queries
        /// </summary>
        /// <returns>String of the values formated for use in a Caml query</returns>
        public string camlString()
        {
            string rtnValue = "";
            foreach (var v in this)
            {
                rtnValue += v.camlString();
            }
            return camlExtensions.surround("Values", rtnValue);
        }
    }

    /// <summary>
    /// Class for representing a caml field reference
    /// </summary>
    public class camlFieldRef : camlBase
    {
        string _fieldName;

        /// <summary>
        /// Initializes the Field reference with the fieldName
        /// </summary>
        /// <param name="fieldName">Name of the field</param>
        public camlFieldRef(string fieldName)
        {
            _fieldName = fieldName;
        }

        /// <summary>
        /// Used to generate the field in a format used by Caml queries
        /// </summary>
        /// <returns>String of the field formated for use in a Caml query</returns>
        public string camlString()
        {
            return "<FieldRef Name=\"" + _fieldName + "\" />";
        }
    }

    /// <summary>
    /// Classed used to contain the extension functions used to help
    /// generate a complete caml query
    /// For reference on proper use http://msdn.microsoft.com/en-us/library/ms467521(v=office.14).aspx
    /// </summary>
    public static class camlExtensions
    {
        /// <summary>
        /// Used to create a string with the <see parm="valueToSurround"/> in
        /// a XML element named <see param="op"/>
        /// </summary>
        /// <param name="op">Name for XML Element</param>
        /// <param name="valueToSurround">Content for XML Element <see param="op"/></param>
        /// <returns>Stirng with the <see param="valueToSurround"/> as content in
        /// an XML element named <see param="op"/></returns>
        public static string surround(string op, string valueToSurround)
        {
            return "<" + op + ">" + valueToSurround + "</" + op + ">";
        }

        /// <summary>
        /// Caml Eq operator
        /// </summary>
        /// <param name="f">Field for the comparison</param>
        /// <param name="v">Value to check if field is equal to</param>
        /// <returns>Caml Query string for Eq</returns>
        public static string Eq(this camlFieldRef f, camlBase v)
        {
            return surround("Eq", f.camlString() + v.camlString());
        }

        /// <summary>
        /// Caml Lt operator
        /// </summary>
        /// <param name="f">Field for the comparison</param>
        /// <param name="v">Value to check if field is less than</param>
        /// <returns>Caml Query string for Lt</returns>
        public static string Lt(this camlFieldRef f, camlBase v)
        {
            return surround("Lt", f.camlString() + v.camlString());
        }

        /// <summary>
        /// Caml Gt operator
        /// </summary>
        /// <param name="f">Field for the comparison</param>
        /// <param name="v">Value to check if field is greater than </param>
        /// <returns>Caml Query string for Gt</returns>
        public static string Gt(this camlFieldRef f, camlBase v)
        {
            return surround("Gt", f.camlString() + v.camlString());
        }

        /// <summary>
        /// Caml Leq operator
        /// </summary>
        /// <param name="f">Field for the comparison</param>
        /// <param name="v">Value to check if field is Less than or equal to</param>
        /// <returns>Caml Query string for Leq</returns>
        public static string Leq(this camlFieldRef f, camlBase v)
        {
            return surround("Leq", f.camlString() + v.camlString());
        }

        /// <summary>
        /// Caml Geq operator
        /// </summary>
        /// <param name="f">Field for the comparison</param>
        /// <param name="v">Value to check if field is Greater than or equal to</param>
        /// <returns>Caml Query string for Geq</returns>
        public static string Geq(this camlFieldRef f, camlBase v)
        {
            return surround("Geq", f.camlString() + v.camlString());
        }

        /// <summary>
        /// Caml Neq operator
        /// </summary>
        /// <param name="f">Field for the comparison</param>
        /// <param name="v">Value to check if field is Not equal to</param>
        /// <returns>Caml Query string for Neq</returns>
        public static string Neq(this camlFieldRef f, camlBase v)
        {
            return surround("Neq", f.camlString() + v.camlString());
        }

        /// <summary>
        /// Caml BeginsWith operator
        /// </summary>
        /// <param name="f">Field for the comparison</param>
        /// <param name="v">Value to check if the field begins with</param>
        /// <returns>Caml Query string for BeginsWith</returns>
        public static string BeginsWith(this camlFieldRef f, camlBase v)
        {
            return surround("BeginsWith", f.camlString() + v.camlString());
        }

        /// <summary>
        /// Caml Contains operator
        /// </summary>
        /// <param name="f">Field for the comparison</param>
        /// <param name="v">Value to check if the field contains</param>
        /// <returns>Caml query string for Contains</returns>
        public static string Contains(this camlFieldRef f, camlBase v)
        {
            return surround("Contains", f.camlString() + v.camlString());
        }

        /// <summary>
        /// Caml Includes operator
        /// </summary>
        /// <param name="f">Field for the comparison</param>
        /// <param name="v">Value to check if the field includes the value</param>
        /// <returns>Caml query string for the Includes</returns>
        public static string Includes(this camlFieldRef f, camlBase v)
        {
            return surround("Includes", f.camlString() + v.camlString());
        }

        /// <summary>
        /// Caml NotIncludes operator
        /// </summary>
        /// <param name="f">Field for the comparison</param>
        /// <param name="v">Value to check if the field does not include the value</param>
        /// <returns>Caml query string for NotIncludes</returns>
        public static string NotIncludes(this camlFieldRef f, camlBase v)
        {
            return surround("NotIncludes", f.camlString() + v.camlString());
        }

        /// <summary>
        /// Caml In operator
        /// </summary>
        /// <param name="f">Field for the comparison</param>
        /// <param name="values">Values to check for in field</param>
        /// <returns>Caml query string for In</returns>
        public static string In(this camlFieldRef f, camlValues values)
        {
            return surround("In", f.camlString() + values.camlString());
        }

        /// <summary>
        /// Caml IsNull operator
        /// </summary>
        /// <param name="f">Field for the comparison</param>
        /// <returns>Caml query string for IsNull</returns>
        public static string IsNull(this camlFieldRef f)
        {
            return surround("IsNull", f.camlString());
        }

        /// <summary>
        /// Caml IsNotNull operator
        /// </summary>
        /// <param name="f">Field for the comparison</param>
        /// <returns>Caml query string for IsNotNull</returns>
        public static string IsNotNull(this camlFieldRef f)
        {
            return surround("IsNotNull", f.camlString());
        }

        /// <summary>
        /// Caml And logical operator
        /// </summary>
        /// <param name="s1">First comparison to And</param>
        /// <param name="s2">Second comparison to And</param>
        /// <returns>Caml query string for And</returns>
        public static string And(this string s1, string s2)
        {
            return surround("And", s1 + s2);
        }

        /// <summary>
        /// Caml Or logical operator
        /// </summary>
        /// <param name="s1">First comparison to Or</param>
        /// <param name="s2">Second comparison to Or</param>
        /// <returns>Caml query string for Or</returns>
        public static string Or(this string s1, string s2)
        {
            return surround("Or", s1 + s2);
        }

        /// <summary>
        /// Caml Where operator
        /// </summary>
        /// <param name="s">String to include in the Where XML Element</param>
        /// <returns>Caml query string for Where</returns>
        public static string Where(this string s)
        {
            return surround("Where", s);
        }
    }
}