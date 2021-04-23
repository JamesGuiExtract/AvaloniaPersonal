using Extract.FileConverter.Pages.Utility;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Extract.FileConverter.Converters.Models
{
    sealed public class LeadtoolsModel : NotifyPropertyChangeObject, IDataErrorInfo
    {
        private bool _Retain;
        private int _PerspectiveID = -1;
        private string _RemovePages;

        /// <summary>
        /// Gets or sets the optional argument retain. Retain burns the annotations into the resulting image if the source is a tiff,
        /// and the destination is a .pdf or jpeg. If going from tiff to tiff, all annotations are retained, and if the source is pdf
        /// then there are no annotations to retain.
        /// </summary>
        public bool Retain 
        { 
            get 
            {
                return this._Retain;
            }
            set 
            {
                ApplyPropertyChange<LeadtoolsModel, bool>(ref _Retain, o => o._Retain, value);
            } 
        }

        /// <summary>
        /// Gets or sets the perspective ID with a value 1-8. Must be used with the /vp argument
        /// </summary>
        public int PerspectiveID
        {
            get
            {
                return this._PerspectiveID;
            }
            set
            {
                ApplyPropertyChange<LeadtoolsModel, int>(ref _PerspectiveID, o => o._PerspectiveID, value);
            }
        }

        /// <summary>
        /// Gets or sets the remove pages string. Can be an individual number, a comma-separated list, 
        /// a range of pages denoted with a hyphen, or a dash followed by a number to indicate you should remove last x pages.
        /// </summary>
        public string RemovePages
        {
            get
            {
                return this._RemovePages;
            }
            set
            {
                ApplyPropertyChange<LeadtoolsModel, string>(ref _RemovePages, o => o._RemovePages, value);
            }
        }

        string IDataErrorInfo.Error
        {
            get { return null; }
        }

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                if (columnName == "PerspectiveID")
                {                        
                    if (PerspectiveID != -1 && (PerspectiveID < 1 || PerspectiveID > 8))
                    {
                        return "The perspectiveID must be between 1-8";
                    }
                }

                if (columnName == "RemovePages")
                {
                    Regex rx = new Regex(@"^(\d*-\d+)|(\d+(,\d+)*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    if(!string.IsNullOrEmpty(RemovePages) && !rx.IsMatch(RemovePages))
                    {
                        return "Invalid input! The input must be an individual number, a comma-separated list, a range of pages denoted with a hyphen, or a dash followed by a number.";
                    }
                }

                // If there's no error, null gets returned
                return null;
            }
        }
    }
}
