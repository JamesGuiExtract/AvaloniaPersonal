using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Extract.Licensing.Internal
{
    /// <summary>
    /// This is patterned after the ComponentData class in COMLMCore
    /// </summary>
    public class ComponentInfo : IEquatable<ComponentInfo>
    {

        public ComponentInfo(DateTime expireDate)
        {
            PermanentLicense = false;
            ExpirationDate = expireDate.ToLocalTime().Date;
            
            Disabled = false;
        }

        public ComponentInfo()
        {
            PermanentLicense = true;
            Disabled = false;
            ExpirationDate = DateTime.Now.ToLocalTime().Date;
        }

        public bool PermanentLicense { get; set; }

        public bool Disabled { get; set; }

        DateTime expirationDate;

        public DateTime ExpirationDate
        {
            get { return expirationDate; } 
            set { expirationDate = value.Date.ToLocalTime(); }
        }

        public ByteArrayManipulator Write(ByteArrayManipulator byteArray)
        {

            byteArray.Write(PermanentLicense);
            byteArray.WriteAsCTime(ExpirationDate);

            return byteArray;
        }

        public ByteArrayManipulator Read(ByteArrayManipulator byteArray)
        {
            PermanentLicense = byteArray.ReadBoolean();
            ExpirationDate = byteArray.ReadCTimeAsDateTime().ToLocalTime();
            Disabled = false;

            return byteArray;
        }

        public bool Equals(ComponentInfo other)
        {
            if (other is null)
                return false;
            return
                PermanentLicense == other.PermanentLicense &&
                (PermanentLicense || DateTime.Compare(ExpirationDate, other.ExpirationDate) == 0);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return Equals(obj as ComponentInfo);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                hashCode += (hashCode * 31) ^ PermanentLicense.GetHashCode();
                hashCode += (hashCode * 32) ^ (PermanentLicense ? 0 : ExpirationDate.GetHashCode());
                return hashCode;
            }
        }
    }
}
