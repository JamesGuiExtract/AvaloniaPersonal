using System.IO;
using System.Reflection;

namespace Extract.FileConverter.Test
{
    public class Utility
    {
        public static void WriteResourceToFile(string resourceName, string fileName)
        {
            using Stream resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            using FileStream file = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            resource.CopyTo(file);
        }
    }
}
