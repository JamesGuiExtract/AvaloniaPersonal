using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SqlClient;

namespace ExtractLicenseUI.DatFileUtility
{
    /// <summary>
    /// This contains the main code for loading dat files into the database (package.dat/components.dat).
    /// Call LoadFilesIntoDatabase() to use this code (technically this code is un-used by the license utility).
    /// </summary>
    public static class LoadDatFilesIntoDatabase
    {

        private static SqlConnection SqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
		private static readonly string InsertComponentQuery = @"
INSERT INTO dbo.Component
(
	[ID]
	, [Name]
)
VALUES
(
    @ID
    , @Name
)";
        private static readonly string InsertPackageQuery = @"
INSERT INTO dbo.Package
(
    Guid
	, [Name]
    , [PackageHeader]
    , [Version_GUID]
)
VALUES
(
    @Guid
    , @Name
    , @PackageHeader
    , @VersionGUID
)";

        private static readonly string InsertPackageComponentMapping = @"
INSERT INTO dbo.PackageComponentMapping
(
	[Package_Guid]
    , [Component_ID]
)
VALUES
(
    @Package_Guid
    , @Component_ID
)";

        /// <summary>
        /// This is currently not setup to be user friendly.
        /// This will load multiple/or singular dat files into a licensing database
        /// 1. Update the filepath in DatFileReader.
        /// 2. This code expects the folder format to be as such: folderpath/VersionOfDatFiles/ packages.dat or components.dat
        /// 3. Call this method.
        /// </summary>
        public static void LoadFilesIntoDatabase()
        {
            /* Run these commands to clear everything out before running this process.
             * DELETE FROM PackageComponentMapping;
                DELETE FROM Component;
                DELETE FROM Package;
             */
            var components = new DatFileReader().ReadComponents();
            var packages = new DatFileReader().ReadPackages();

            StoreComponentsInDatabase(components);
            StorePackagesInDatabase(packages);
            CreatePackageComponentMapping(packages);
        }

        private static void StoreComponentsInDatabase(HashSet<ComponentModel> components)
        {
            SqlConnection.Open();
            foreach (var component in components)
            {
                using (SqlCommand command = new SqlCommand(InsertComponentQuery, SqlConnection))
                {
                    command.Parameters.AddWithValue("@ID", component.ComponentID);
                    command.Parameters.AddWithValue("@Name", component.ComponentName);
                    command.ExecuteNonQuery();
                }
            }
            SqlConnection.Close();
        }

        private static void StorePackagesInDatabase(Collection<PackageModel> packages)
        {
            SqlConnection.Open();
            foreach (var package in packages)
            {
                using (SqlCommand command = new SqlCommand(InsertPackageQuery, SqlConnection))
                {
                    command.Parameters.AddWithValue("@Guid", package.Guid);
                    command.Parameters.AddWithValue("@Name", package.PackageName);
                    command.Parameters.AddWithValue("@PackageHeader", package.PackageHeader);
                    command.Parameters.AddWithValue("@VersionGUID", package.Version);
                    command.ExecuteNonQuery();
                }
            }
            SqlConnection.Close();
        }

        private static void CreatePackageComponentMapping(Collection<PackageModel> packages)
        {
            SqlConnection.Open();
            foreach (var package in packages)
            {
                foreach(var id in package.VariableIDs)
                {
                    using (SqlCommand command = new SqlCommand(InsertPackageComponentMapping, SqlConnection))
                    {
                        command.Parameters.AddWithValue("@Package_Guid", package.Guid);
                        command.Parameters.AddWithValue("@Component_ID", id);
                        command.ExecuteNonQuery();
                    }
                }
            }

            SqlConnection.Close();
        }
    }
}
