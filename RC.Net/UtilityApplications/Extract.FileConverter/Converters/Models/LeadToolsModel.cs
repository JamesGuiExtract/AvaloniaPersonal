using Extract.Utilities;
using System.ComponentModel;

namespace Extract.FileConverter
{
    public sealed class LeadtoolsModel : NotifyPropertyChangedObject, IDataErrorInfo
    {
        private bool _Retain;
        private int _PerspectiveID = -1;
        private string _RemovePages;

        /// <summary>
        /// Gets or sets the optional argument retain. Retain burns the annotations into the resulting image if the source is a tif,
        /// and the destination is a .pdf or jpeg. If going from tif to tif, all annotations are retained, and if the source is pdf
        /// then there are no annotations to retain.
        /// </summary>
        public bool Retain
        {
            get => _Retain;
            set => Set(ref _Retain, value);
        }

        /// <summary>
        /// Gets or sets the perspective ID with a value 1-8. Must be used with the /vp argument
        /// </summary>
        public int PerspectiveID
        {
            get => _PerspectiveID;
            set => Set(ref _PerspectiveID, value);
        }

        /// <summary>
        /// Gets or sets the remove pages string. Can be an individual number, a comma-separated list, 
        /// a range of pages denoted with a hyphen, or a dash followed by a number to indicate you should remove last x pages.
        /// </summary>
        public string RemovePages
        {
            get => _RemovePages;
            set => Set(ref _RemovePages, value);
        }

        public bool HasDataError { get; private set; } = false;

        string IDataErrorInfo.Error => null;

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                HasDataError = true;
                if (columnName == "PerspectiveID")
                {
                    if (PerspectiveID != -1 && (PerspectiveID < 1 || PerspectiveID > 8))
                    {
                        return "The perspectiveID must be between 1-8";
                    }
                }

                if (columnName == "RemovePages")
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(RemovePages))
                        {
                            UtilityMethods.ValidatePageNumbers(RemovePages);
                        }
                    }
                    catch (ExtractException)
                    {
                        return "Invalid input! The input must be an individual number, a comma-separated list, a range of pages denoted with a hyphen, or a dash followed by a number.";
                    }
                }

                HasDataError = false;
                // If there's no error, null gets returned
                return null;
            }
        }

        /// <summary>
        /// Performs a memberwise clone on the LeadtoolsModel.
        /// </summary>
        /// <returns>A deep clone of the LeadtoolsModel.</returns>
        public LeadtoolsModel Clone()
        {
            return (LeadtoolsModel)MemberwiseClone();
        }
    }
}
