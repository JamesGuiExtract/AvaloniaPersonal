using System.Collections.Generic;
using System.ComponentModel;

namespace Extract.Utilities
{
    /// <summary>
    /// Represents File Action Manager document tags.
    /// </summary>
    [DisplayName("File Action Manager Supplier")]
    public class FileActionManagerSupplierPathTags : PathTagsBase
    {
        #region Constants
        
        /// <summary>
        /// Constant string for the FPS file directory tag.
        /// </summary>
        public static readonly string FpsFileDirectoryTag = "<FPSFileDir>";

        #endregion

        #region FileActionManagerSupplierPathTags Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileActionManagerSupplierPathTags"/> class.
        /// </summary>
        public FileActionManagerSupplierPathTags()
            : this("")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileActionManagerSupplierPathTags"/> class.
        /// </summary>
        /// <param name="fpsDirectory">The directory of the fps file.</param>
        public FileActionManagerSupplierPathTags(string fpsDirectory)
            : base(GetTagsToValues(fpsDirectory))
        {
        }

        #endregion FileActionManagerSupplierPathTags Constructors

        #region FileActionManagerSupplierPathTags Methods

        /// <summary>
        /// Gets the path tags mapped to their expanded form.
        /// </summary>
        /// <param name="fpsDirectory">The fps file.</param>
        /// <returns>The path tags mapped to their expanded form.</returns>
        static Dictionary<string, string> GetTagsToValues(string fpsDirectory)
        {
            Dictionary<string, string> tagsToValues = new Dictionary<string,string>(1);
            tagsToValues.Add(FpsFileDirectoryTag, fpsDirectory);
            return tagsToValues;
        }

        #endregion FileActionManagerSupplierPathTags Methods
    }
}
