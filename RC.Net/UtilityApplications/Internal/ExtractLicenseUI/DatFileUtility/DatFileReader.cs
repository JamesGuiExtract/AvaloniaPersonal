using ExtractLicenseUI.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtractLicenseUI.DatFileUtility
{
    class DatFileReader
    {
        private readonly string folderPath = @"C:\Users\trever_gannon\Desktop\dat files from years past";

        public HashSet<ComponentModel> ReadComponents()
        {
            HashSet<ComponentModel> components = new HashSet<ComponentModel>();

            var directories = Directory.GetDirectories(folderPath);
            Array.Reverse(directories);
            foreach(string versionFolder in directories)
            {
                // Read all text from component file.
                string componentFile = System.IO.File.ReadAllText(versionFolder + "\\Components.dat");

                var filteredComponetLines = RemoveWhiteSpaceAndComents(componentFile.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None));
                
                foreach(string componentLine in filteredComponetLines)
                {
                    // In the components file, everything before the ',' is a ID, everything after is the component name.
                    components.Add(new ComponentModel()
                    {
                        ComponentID = int.Parse(componentLine.Split(',')[0].ToString()),
                        ComponentName = componentLine.Split(',')[1].ToString(),
                    });
                }
            }

            return components;
        }

        public Collection<PackageModel> ReadPackages()
        {
            Collection<PackageModel> packages = new Collection<PackageModel>();
            var versions = new DatabaseReader().ReadVersions();

            var directories = Directory.GetDirectories(folderPath);
            foreach (string versionFolder in directories)
            {
                // Get the folder name for the version.
                var version = new DirectoryInfo(versionFolder).Name;

                // Read all text from package file.
                string packageFile = System.IO.File.ReadAllText(versionFolder + "\\Packages.dat");

                var filteredPackageLines = RemoveWhiteSpaceAndComents(packageFile.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None));

                string packageHeader = string.Empty;
                var packageVariables = new Collection<PackageVariable>();
                foreach (string packageLine in filteredPackageLines)
                {
                    var package = new PackageModel();
                    // variable
                    if(packageLine.StartsWith("!"))
                    {
                        var packageVarible = new PackageVariable();

                        var variableToParse = packageLine.Split(',');
                        packageVarible.VariableName = variableToParse[0].TrimStart('!');
                        variableToParse = variableToParse[1].Split(':');

                        foreach(string variable in variableToParse)
                        {
                            var isNumeric = int.TryParse(variable, out int number);
                            if(isNumeric)
                            {
                                packageVarible.VariableIDs.Add(number);
                            }
                            else
                            {
                                packageVarible.OtherVariables.Add(variable.TrimStart('$'));
                            }
                        }
                        packageVariables.Add(packageVarible);
                    }
                    // Package Definition
                    else if(packageLine.StartsWith("-"))
                    {
                        package.Version = GetVersionGuid(version, versions);
                        package.PackageHeader = packageHeader;
                        var packageToParse = packageLine.TrimStart('-').Trim().Split(',');
                        package.PackageName = packageToParse[0];

                        packageToParse = packageToParse[1].Split(':');
                        foreach (string variable in packageToParse)
                        {
                            var isNumeric = int.TryParse(variable, out int number);
                            if (isNumeric)
                            {
                                package.VariableIDs.Add(number);
                            }
                            else
                            {
                                package.Variables.Add(variable.TrimStart('$'));
                            }
                        }
                        FlattenPackageIDs(package, packageVariables);
                        packages.Add(package);
                    }
                    // Package Header/label
                    else
                    {
                        packageHeader = packageLine.Split(',')[0];
                    }
                }
            }

            return packages;
        }

        private static Guid GetVersionGuid(string numericVersion, Collection<ExtractVersion> extractVersions)
        {
            foreach(var version in extractVersions)
            {
                if(version.Version.Equals(numericVersion))
                {
                    return version.Guid;
                }
            }
            throw new Exception("Version not in the database");
        }

        private Collection<string> RemoveWhiteSpaceAndComents(string[] toProcess)
        {
            Collection<string> toReturn = new Collection<string>();

            foreach(string process in toProcess)
            {
                if(!process.Equals(String.Empty) && !process.StartsWith("//"))
                {
                    toReturn.Add(process);
                }
            }
            return toReturn;
        }

        private PackageModel FlattenPackageIDs(PackageModel package, Collection<PackageVariable> variables)
        {
            foreach(string variable in package.Variables)
            {
                HashSet<int> flattenIDs = FlattenPackageVariable(variables.Where(m => m.VariableName.Equals(variable)).First(), variables);
                foreach(int id in flattenIDs)
                {
                    package.VariableIDs.Add(id);
                }
            }
            return package;
        }

        private HashSet<int> FlattenPackageVariable(PackageVariable variable, Collection<PackageVariable> variables)
        {
            if(variable.OtherVariables.Count > 0)
            {
                foreach(var otherVariable in variable.OtherVariables)
                {
                    var variableLookup = variables.Where(m => m.VariableName.Equals(otherVariable)).First();
                    var ids = FlattenPackageVariable(variableLookup, variables);
                    foreach (int id in ids)
                    {
                        variable.VariableIDs.Add(id);
                    }
                }
            }

            return variable.VariableIDs;
        }
    }
}
