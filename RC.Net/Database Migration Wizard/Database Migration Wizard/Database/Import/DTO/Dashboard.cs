namespace DatabaseMigrationWizard.Database.Input.DataTransformObject
{
    public class Dashboard
    {
        public string DashboardName { get; set; }

        public string Definition { get; set; }

        public string LastImportedDate { get; set; }

        public bool UseExtractedData { get; set; }

        public string ExtractedDataDefinition { get; set; }

        public string UserName { get; set; }

        public string FullUserName { get; set; }

        public override string ToString()
        {
            return $@"('
                        {DashboardName}'
                        , CONVERT(XML, N'{Definition.Replace("'", "''")}')
                        , CAST('{LastImportedDate}' AS DATETIME)
                        , {(UseExtractedData == true ? "1" : "0")}
                        , {(ExtractedDataDefinition == null ? "NULL" : "CONVERT(XML, N'" + ExtractedDataDefinition.Replace("'", "''") + "')")}
                        , '{UserName}'
                        , '{FullUserName}'
                    )";
        }
    }
}
