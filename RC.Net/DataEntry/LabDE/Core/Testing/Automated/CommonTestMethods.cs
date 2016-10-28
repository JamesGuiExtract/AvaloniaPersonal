using NUnit.Framework;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Extract.DataEntry.LabDE.Test
{
    class CommonTestMethods
    {
        /// <summary>
        /// Extracts the value from name value pair, delimited by '='.
        /// </summary>
        /// <param name="nameValuePair">The name value pair.</param>
        /// <returns>The requested value.</returns>
        /// NOTE: This function asserts if the input is null or empty, 
        /// if there isn't an '=' character in the input,
        /// or if the value is empty.
        public static string ValueFromNameValuePair(string nameValuePair)
        {
            Assert.That(!String.IsNullOrWhiteSpace(nameValuePair));

            const int notFound = -1;
            int startOfValue = nameValuePair.IndexOf('=');
            Assert.That(startOfValue != notFound);

            startOfValue += 1;

            var value = nameValuePair.Substring(startOfValue);
            Assert.That(!String.IsNullOrWhiteSpace(value));

            return value;
        }

        /// <summary>
        /// Gets the SQL results using the specified query.
        /// </summary>
        /// <param name="query">The specified query. 
        /// Note that the query is assumed to only return a single row.</param>
        /// <returns>A string with all of the column values pasted together with commas. 
        /// There is no comma on the final column.</returns>
        /// NOTE: This function waits for an amount of time to ensure that prior operations have had time 
        /// to be written into the DB tables.
        public static string GetSqlResults(string query, SqlConnection dbConnection)
        {
            const int twoSeconds = 2 * 1000;
            Thread.Sleep(twoSeconds);

            SqlCommand cmd = new SqlCommand(query, dbConnection);
            using (var reader = cmd.ExecuteReader())
            {
                if (!reader.HasRows)
                    return "";

                StringBuilder sb = new StringBuilder();

                while (reader.Read())
                {
                    int numberOfColumns = reader.FieldCount;
                    for (int column = 0; column < numberOfColumns; ++column)
                    {
                        string value = reader[column].ToString();

                        if (column < numberOfColumns - 1)
                            sb.AppendFormat("{0}, ", value);
                        else
                            sb.AppendFormat("{0}", value);
                    }
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// This function is primarily a logical description of the operation, and a good 
        /// location for displaying the result and expected strings for trouble-shooting.
        /// </summary>
        /// <param name="result">The result (read from the DB tables).</param>
        /// <param name="expected">The expected value.</param>
        /// <returns></returns>
        public static bool ExpectedMatch(string result, string expected)
        {
            Debug.WriteLine($"\n  Result: {result}\nExpected: {expected}");

            return result == expected;
        }


    }

}
