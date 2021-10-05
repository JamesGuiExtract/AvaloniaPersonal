using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace AttributeDBMgrTest
{
    class AttributeTest
    {
        public AttributeTest( string dbName )
        {
            Contract.Assert(!String.IsNullOrEmpty(dbName), "Must set a valid DB Name");
            m_dbName = dbName;

            SetConnectString();
            m_ASFF_IDs = GetAllAttributeSetForFileIDs();
        }

        private void SetConnectString()
        {
            SqlConnectionStringBuilder scsb = new SqlConnectionStringBuilder();
            scsb.DataSource = "(local)";
            scsb.InitialCatalog = m_dbName;
            scsb.IntegratedSecurity = true;         // use current Windows account credentials
            m_connectString = scsb.ToString();
            //scsb.UserID = "admin";
            //scsb.Password = "a";
        }

        public List<Int64> GetAllAttributeSetForFileIDs()
        {
            List<Int64> results = new List<Int64>();

            string query = "SELECT [ID] from [dbo].[AttributeSetForFile]";

            using (SqlConnection connection = new SqlConnection(m_connectString))
            {
                SqlCommand cmd = new SqlCommand(query, connection);

                connection.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                int index = 0;
                while (reader.Read())
                {
                    results.Add( Convert.ToInt32(reader[0]) );
                    ++index;
                }
                reader.Close();
            }

            return results;
        }

        // NOTE: The following script only handles attribute vlaues and types nested three levels deep. In theory there
        // is no limit to depth, but in practice three levels is adequate, and five levels is the deepest known nesting.
        public List<String> GetAttribute(Int64 asffID)
        {
            List<String> namedValues = new List<string>();

            string query = 
                String.Format("SELECT\n"+
                              " CASE \n"+
                              " 	 WHEN LEN(GreatGrandParentAttributeName.Name) > 0 THEN '.' ELSE '' \n"+
                              " END + \n"+
	                          " CASE \n"+
		                      "     WHEN LEN(GrandParentAttributeName.Name) > 0 THEN '.' ELSE ''\n"+
	                          " END +\n"+
	                          " CASE \n"+
		                      "     WHEN LEN(ParentAttributeName.Name) > 0 THEN '.' ELSE ''\n"+
	                          " END +\n"+
	                          " CASE \n"+
		                      "     WHEN LEN(AttributeName.Name) > 0 THEN AttributeName.Name ELSE ''\n"+
	                          " END AS AttributePath,\n"+
                              " REPLACE(Attribute.Value, '\', '\\') as AttributeValue,\n"+
                              " CASE \n" +
                              "	WHEN LEN(GreatGrandParentAttributeType.Type) > 0 THEN GreatGrandParentAttributeType.Type ELSE '' \n"+
                              " END + \n"+
                              " CASE \n"+
                              "	WHEN LEN(GrandParentAttributeType.Type) > 0 THEN GrandParentAttributeType.Type ELSE '' \n"+
                              " END + \n"+
                              " CASE \n"+
                              "	WHEN LEN(ParentAttributeType.Type) > 0 THEN ParentAttributeType.Type ELSE '' \n"+
                              " END + \n"+
                              " CASE \n"+
                              "	WHEN LEN(AttributeType.Type) > 0 THEN AttributeType.Type ELSE '' \n" +
                              " END AS AttributeTypeName \n"+
                              "FROM [{0}].[dbo].[Attribute]\n"+
                              "left join AttributeName on AttributeName.ID=Attribute.AttributeNameID\n"+
                              "left join Attribute as ParentAttibute on ParentAttibute.ID=Attribute.ParentAttributeID\n"+
                              "left join AttributeName as ParentAttributeName on ParentAttibute.AttributeNameID=ParentAttributeName.ID\n"+
                              "left join Attribute as GrandParentAttribute on GrandParentAttribute.id=ParentAttibute.ParentAttributeID\n"+
                              "left join AttributeName as GrandParentAttributeName on GrandParentAttributeName.id=GrandParentAttribute.AttributeNameID\n"+
                              "left join Attribute as GreatGrandParentAttribute on GreatGrandParentAttribute.id=GrandParentAttribute.ParentAttributeID\n"+
                              "left join AttributeName as GreatGrandParentAttributeName on GreatGrandParentAttributeName.id=GreatGrandParentAttribute.AttributeNameID\n" +
                              "left join AttributeType on AttributeType.ID=(select AttributeTypeID from AttributeInstanceType where AttributeID=Attribute.ID) \n"+
                              "left join AttributeType as ParentAttributeType on ParentAttributeType.ID=(select AttributeTypeID from AttributeInstanceType where AttributeID=ParentAttibute.ID) \n"+
                              "left join AttributeType as GrandParentAttributeType on GrandParentAttributeType.ID=(select AttributeTypeID from AttributeInstanceType where AttributeID=GrandParentAttribute.ID) \n"+
                              "left join AttributeType as GreatGrandParentAttributeType on GreatGrandParentAttributeType.ID=(select AttributeTypeID from AttributeInstanceType where AttributeID=GreatGrandParentAttribute.ID) \n"+
                              "where Attribute.AttributeSetForFileID={1};",
                              m_dbName,
                              asffID);

            using (SqlConnection connection = new SqlConnection(m_connectString))
            {
                SqlCommand cmd = new SqlCommand(query, connection);

                connection.Open();
                using SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string attributePath = reader[0].ToString();
                    string attributeValue = ConvertToNormalString(reader[1].ToString());
                    string attributeType = reader[2].ToString();

                    string result = String.Format("{0}|{1}", attributePath, attributeValue);
                    if (!String.IsNullOrEmpty(attributeType))
                        result += '|' + attributeType;

                    namedValues.Add(result);
                }
            }

            return namedValues;
        }

        // This is done to emulate the convertCppStringToNormalString() call used by
        // EAVGenerator when it is writing EAV files.
        string ConvertToNormalString(string s)
        {
            var s1 = s.Replace("\\", "\\\\").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t").
                Replace("\a", "\\a").Replace("\b", "\\b").Replace("\f","\\f").Replace("\v","\\v").
                Replace("\"", "\\\"");

            return s1;
        }

        public string GetFileName(Int64 asffID)
        {
            string query = String.Format("SELECT [FileName] FROM [dbo].[FAMFile] WHERE ID=("+
                                         "SELECT [FileID] FROM [dbo].[FileTaskSession] WHERE [ID]=("+
                                         "SELECT [FileTaskSessionID] from [dbo].[AttributeSetForFile] where ID={0}))",
                                         asffID);

            string filename;
            using (SqlConnection connection = new SqlConnection(m_connectString))
            {
                SqlCommand cmd = new SqlCommand(query, connection);

                connection.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                Contract.Assert(reader.Read(), "Could not get the filename associated with ASFF ID: {0}", asffID);
                filename = reader[0].ToString();
            }

            return filename;
        }

        private string m_dbName;
        private string m_connectString;
        private List<Int64> m_ASFF_IDs;
    }
}
