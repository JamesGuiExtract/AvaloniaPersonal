using Extract.Utilities;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Extract.AttributeFinder
{
    [Serializable]
    // Don't rename because it could break serialization
    [Obfuscation(Feature = "renaming", Exclude = true)]
    public class CategoryQueryPair : INotifyPropertyChanged
    {
        private string _category;
        private bool _categoryIsXPath;
        private string _query;

        #region Properties

        /// <summary>
        /// Gets or sets the category
        /// </summary>
        public string Category
        {
            get
            {
                return _category;
            }
            set
            {
                try
                {
                    var newValue = string.IsNullOrWhiteSpace(value) ? null : value;
                    if (newValue != _category)
                    {
                        _category = newValue;
                        NotifyPropertyChanged();
                    }
                }
                catch (Exception e)
                {
                    throw e.AsExtract("ELI41451");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="Category"/> is an XPath
        /// </summary>
        public bool CategoryIsXPath
        {
            get
            {
                return _categoryIsXPath;
            }
            set
            {
                if (value != _categoryIsXPath)
                {
                    _categoryIsXPath = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the query
        /// </summary>
        public string Query
        {
            get
            {
                return _query;
            }
            set
            {
                try
                {
                    var newValue = string.IsNullOrWhiteSpace(value) ? null : value;
                    if (_query != newValue)
                    {
                        _query = newValue;
                        NotifyPropertyChanged();
                    }
                }
                catch (Exception e)
                {
                    throw e.AsExtract("ELI41452");
                }
            }
        }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Creates an instance of <see cref="CategoryQueryPair" /> that is a shallow clone of this instance
        /// </summary>
        /// <remarks>
        /// All fields are immutable or value types so there is no reason for a deep clone
        /// </remarks>
        public CategoryQueryPair ShallowClone()
        {
            try
            {
                return (CategoryQueryPair)MemberwiseClone();
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI41434");
            }
        }

        #endregion Public Methods

        #region INotifyPropertyChanged

        /// <summary>
        /// Property changed event
        /// </summary>
        [field:NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion INotifyPropertyChanged

        #region Private Methods

        /// <summary>
        /// This method is called by the Set accessor of each property
        /// </summary>
        /// <param name="propertyName">Optional name of property that changed</param>
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Private Methods

        #region Overrides

        /// <summary>
        /// Whether this instance has equal property values to another
        /// </summary>
        /// <param name="obj">The instance to compare with</param>
        /// <returns><c>true</c> if this instance has equal property values, else <c>false<c/></returns>
        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(this, obj))
            {
                return true;
            }

            var other = obj as CategoryQueryPair;
            if (other == null
                || other.Category != Category
                || other.CategoryIsXPath != CategoryIsXPath
                || other.Query != Query)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the hash code for this object
        /// </summary>
        /// <returns>The hash code for this object</returns>
        public override int GetHashCode()
        {
            return HashCode.Start
                .Hash(Category)
                .Hash(CategoryIsXPath)
                .Hash(Query);
        }

        #endregion Overrides
    }
}
