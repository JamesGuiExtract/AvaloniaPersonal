using static FileAPI_VS2017.Utils;

namespace FileAPI_VS2017
{
    /// <summary>
    /// FileItem
    /// </summary>
    public class FileItem
    {
        /// <summary>
        /// A numeric (integer) identifer. The value must be > -1.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The name of the file.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// the size of the file (type is long)
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// ToString() - an override
        /// </summary>
        /// <returns>string containing all public properties, listed</returns>
        public override string ToString()
        {
            return Inv($"Name: {Name}, Id: {Id}, Size: {Size}");
        }
    }
}
