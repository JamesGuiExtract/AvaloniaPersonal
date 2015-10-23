using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AttributeDBMgrTest
{
    /* This program is intended to test the AttributeDBMgrComponents, specifically:
    2) verify that all attribute info is stored correctly - diff against .EAV output files
	a) run sql query to get list of ASFF_ID's 
	b) for each ID, get complete Attribute set, write file in EAV format
	c) weak link - create an EAV for each VOA - today this is done manually using the voaFileViewer
	d) diff all EAVs and note any discrepencies
    */
    class Program
    {
        // Pass in the database name, and the output directory path
        // dbName outputPath
        static void Main(string[] args)
        {
            try
            {
                Contract.Assert(args.Length >= 2, "Must pass dbName, outputPath on the command line");

                string dbName = args[0];
                Contract.Assert(!String.IsNullOrEmpty(dbName), "Must pass a non-empty DB name on the command line");

                string outputPath = args[1];
                Contract.Assert(!String.IsNullOrEmpty(outputPath), "Must pass a non-empty outputPath on the command line");

                // Make sure the output path exists - if not, create it.
                if (!Directory.Exists(outputPath))
                {
                    Directory.CreateDirectory(outputPath);
                }

                AttributeTest at = new AttributeTest(dbName);
                var results = at.GetAllAttributeSetForFileIDs();
                for (int i = 0; i < results.Count; ++i)
                {
                    Console.WriteLine("ID[{0}]: {1}", i, results[i]);
                }

                foreach (var ID in results)
                {
                    string filename = at.GetFileName(ID);
                    Console.WriteLine("ID: {0}, filename: {1}", ID, filename);

                    List<string> attrNamedValues = at.GetAttribute(ID);
                    AttributeToFile(filename, outputPath, attrNamedValues);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void AttributeToFile(string filename, string outputPath, List<string> attrValues)
        {
            // Write the file even if it is empty - sometimes the source VOA file really had nothing in it.
            //if (0 == attrValues.Count)
            //    return;

            string[] parts = filename.Split('\\');
            string name = parts.Last();

            // Now remove the ".voa" extension from the name
            int index = name.LastIndexOf('.');
            var nameWithoutExt = name.Remove(index);

            if (!outputPath.EndsWith("\\"))
            {
                outputPath += '\\';
            }

            string outputFilename = String.Format("{0}{1}.eav", outputPath, nameWithoutExt);
            using( StreamWriter sw = new StreamWriter(outputFilename, false, Encoding.Default))
            {
                foreach (var value in attrValues)
                {
                    sw.WriteLine(value);
                }

                sw.Close();
            }
        }
    }
}
