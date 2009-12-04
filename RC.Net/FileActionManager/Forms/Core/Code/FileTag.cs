namespace Extract.FileActionManager.Forms
{
    /// <summary>
    /// Represents settings for a file action manager file tag.
    /// </summary>
    public class FileTag
    {
        #region Fields
	
        /// <summary>
        /// Name of the file tag.
        /// </summary>
        readonly string _name;
        
        /// <summary>
        /// Description of the file tag.
        /// </summary>
        readonly string _description;
		
        #endregion Fields

        #region Constructors
	
        /// <overloads>Initializes a new instance of the <see cref="FileTag"/> class.
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="FileTag"/> class with the specified name.
        /// </summary>
        /// <param name="name">Name of the file tag.</param>
        public FileTag(string name) 
            : this(name, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTag"/> with the specified name and 
        /// description.
        /// </summary>
        /// <param name="name">Name of the file tag.</param>
        /// <param name="description">Description of the file tag.</param>
        public FileTag(string name, string description)
        {
            _name = name;
            _description = description;
        }
		
        #endregion Constructors
        
        #region Properties
	
        /// <summary>
        /// Gets the name of the file tag.
        /// </summary>
        /// <value>The name of the file tag.</value>
        public string Name
        {
            get
            {
                return _name;
            }
        }
		
        /// <summary>
        /// Gets the description of the file tag.
        /// </summary>
        /// <value>The description of the file tag.</value>
        public string Description
        {
            get
            {
                return _description;
            }
        }
		
        #endregion Properties
    }
}
