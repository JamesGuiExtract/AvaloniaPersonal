using Extract.Utilities;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Extract.UtilityApplications.NERAnnotation
{
    public class EntityDefinition : INotifyPropertyChanged
    {
        private string _category;
        private bool _categoryIsXPath;
        private string _rootQuery;
        private string _valueQuery = ".";

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
                    throw e.AsExtract("ELI44906");
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
        /// Gets or sets the root query
        /// </summary>
        public string RootQuery
        {
            get
            {
                return _rootQuery;
            }
            set
            {
                try
                {
                    var newValue = string.IsNullOrWhiteSpace(value) ? null : value;
                    if (_rootQuery != newValue)
                    {
                        _rootQuery = newValue;
                        NotifyPropertyChanged();
                    }
                }
                catch (Exception e)
                {
                    throw e.AsExtract("ELI44907");
                }
            }
        }

        /// <summary>
        /// Gets or sets the value query
        /// </summary>
        public string ValueQuery
        {
            get
            {
                return _valueQuery;
            }
            set
            {
                try
                {
                    var newValue = string.IsNullOrWhiteSpace(value) ? null : value;
                    if (_valueQuery != newValue)
                    {
                        _valueQuery = newValue;
                        NotifyPropertyChanged();
                    }
                }
                catch (Exception e)
                {
                    throw e.AsExtract("ELI44908");
                }
            }
        }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Creates an instance of <see cref="EntityDefinition" /> that is a shallow clone of this instance
        /// </summary>
        /// <remarks>
        /// All fields are immutable or value types so there is no reason for a deep clone
        /// </remarks>
        public EntityDefinition ShallowClone()
        {
            try
            {
                return (EntityDefinition)MemberwiseClone();
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI44909");
            }
        }

        #endregion Public Methods

        #region INotifyPropertyChanged

        /// <summary>
        /// Property changed event
        /// </summary>
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
            if (object.ReferenceEquals(this, obj))
            {
                return true;
            }

            var other = obj as EntityDefinition;
            if (other == null
                || other.Category != Category
                || other.CategoryIsXPath != CategoryIsXPath
                || other.RootQuery != RootQuery
                || other.ValueQuery != ValueQuery)
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
                .Hash(RootQuery)
                .Hash(ValueQuery);
        }

        #endregion Overrides
    }
}
