using Extract.Licensing.Internal;
using FirstFloor.ModernUI.Presentation;
using System;

namespace ExtractLicenseUI.Database
{
    public class ExtractVersion : NotifyPropertyChanged
    {
        private Guid _Guid;
        private string _Version;


        /// <summary>
        /// A unique identifier for the version.
        /// </summary>
        public Guid Guid
        {
            get { return this._Guid; }
            set
            {
                if (this._Guid != value)
                {
                    this._Guid = value;
                    OnPropertyChanged(nameof(Guid));
                }
            }
        }

        /// <summary>
        /// The actual version 
        /// </summary>
        public string Version
        {
            get { return this._Version; }
            set
            {
                if (this._Version != value)
                {
                    this._Version = value;
                    OnPropertyChanged(nameof(Version));
                }
            }
        }

        /// <summary>
        /// Equals override required for the UI.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is ExtractVersion otherVersion)
            {
                return otherVersion.Guid == Guid && otherVersion.Version == Version;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Guid.GetHashCode() + Version.GetHashCode();
        }
    }
}
