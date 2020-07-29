using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Extract.Licensing.Internal
{
    public class LicenseInfo: IEquatable<LicenseInfo>
    {
        // This is the version saved to the .lic file
        public const int Version = 1;

        public LicenseInfo()
        {
        }

        public LicenseInfo(LicenseInfo licenseInfo)
        {
            CopyFrom(licenseInfo);
        }

        public LicenseInfo(string code)
        {
            FromCode(code);
        }

        public string IssuerName { get; set; }

        public string LicenseeName { get; set; }

        public string OrganizationName { get; set; }

        private DateTime issueDate;
        public DateTime IssueDate 
        {
            get { return issueDate; } 
            private set { issueDate = UtilityMethods.Round(value, new TimeSpan(0, 0, 0, 1)); }
        }

        public bool UseComputerName { get; set; }

        public bool UseMACAddress { get; set; }

        public bool UseSerialNumber { get; set; }

        private string userString;

        public string UserString 
        {
            get { return userString; }
            
            set
            { 
                userString = value;

                var userData = new ByteArrayManipulator(UtilityMethods.TranslateBytesWithUserKey(true, UserString.HexStringToBytes()));

                UserComputerName = userData.ReadString();
                UserSerialNumber = userData.ReadUInt32();
                UserMACAddress = userData.ReadString();
            }
        }

        public Dictionary<UInt32, ComponentInfo> ComponentIDToInfo { get; } = new Dictionary<uint, ComponentInfo>();

        public string UserComputerName { get; private set; }
        
        public UInt32 UserSerialNumber { get; private set; }
        
        public string UserMACAddress { get; private set; }

        public string CreateCode()
        {
            string code = "";

            // TODO: Add check that this called from our assembly and is internally licensed
            var LicenseData = new ByteArrayManipulator();

            LicenseData.Write(IssuerName);
            LicenseData.Write(LicenseeName);
            LicenseData.Write(OrganizationName);
            // Write the IssueDate
            IssueDate = UtilityMethods.Round(DateTime.Now.ToLocalTime(), new TimeSpan(0, 0, 0, 1));
            LicenseData.WriteAsCTime(IssueDate);
            LicenseData.Write(UseComputerName);
            LicenseData.Write(UseSerialNumber);
            LicenseData.Write(UseMACAddress);

            if (UseComputerName || UseSerialNumber || UseMACAddress)
            {
                LicenseData.Write(UserString);
            }

            // Save the Components
            LicenseData.Write((UInt32)ComponentIDToInfo.Count);
            foreach( var keyPair in ComponentIDToInfo)
            {
                LicenseData.Write(keyPair.Key);
                keyPair.Value.Write(LicenseData);
            }

            // encrypt 
            var code1 = UtilityMethods.TranslateBytesToLicenseStringWithKey(LicenseData.GetBytes(8),
                                                                            NativeMethods.Key5,
                                                                            NativeMethods.Key6,
                                                                            NativeMethods.Key7,
                                                                            NativeMethods.Key8);

            // This is the string that would be encrypted using the generated password if or when that is done
            var code2 = UtilityMethods.TranslateBytesToLicenseStringWithKey(LicenseData.GetBytes(8),
                                                                            NativeMethods.Key1,
                                                                            NativeMethods.Key2,
                                                                            NativeMethods.Key3,
                                                                            NativeMethods.Key4);
            
            for (int i = 0; i < code1.Length; i++)
            {
                code += code1[i];
                code += code2[i];
            }

            return code;
        }

        public bool FromCode(string code)
        {
            try
            {
                code = code.Trim();
                // extract code2
                var code2 = "";
                var code1 = "";
                for (var i = 0; i < code.Length; i+=2)
                {
                    if ((i % 2) == 0)
                        code1 += code[i];
                    else
                        code2 += code[i];
                }

                var LicenseData = UtilityMethods.GetLicenseBytesFromCode(code1);

                IssuerName = LicenseData.ReadString();
                LicenseeName = LicenseData.ReadString();
                OrganizationName = LicenseData.ReadString();
                IssueDate = LicenseData.ReadCTimeAsDateTime().ToLocalTime();
                UseComputerName = LicenseData.ReadBoolean();
                UseSerialNumber = LicenseData.ReadBoolean();
                UseMACAddress = LicenseData.ReadBoolean();

                if (UseComputerName || UseSerialNumber || UseMACAddress)
                {
                    UserString = LicenseData.ReadString();
                }

                ComponentIDToInfo.Clear();
                UInt32 countOfComponents = LicenseData.ReadUInt32();
                for (int i = 0; i < countOfComponents; i++)
                {
                    var componentID = LicenseData.ReadUInt32();
                    var component = new ComponentInfo();
                    component.Read(LicenseData);
                    ComponentIDToInfo[componentID] = component;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool Equals(LicenseInfo other)
        {
            if (other is null)
                return false;

            return
                IssuerName == other.IssuerName &&
                LicenseeName == other.LicenseeName &&
                OrganizationName == other.OrganizationName &&
                DateTime.Compare(IssueDate, other.IssueDate) == 0 &&
                UseComputerName == other.UseComputerName &&
                UseMACAddress == other.UseMACAddress &&
                UseSerialNumber == other.UseSerialNumber &&
                UserString == other.UserString &&
                ComponentIDToInfo.OrderBy(kvp => kvp.Key).SequenceEqual(other.ComponentIDToInfo.OrderBy(kvp => kvp.Key));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return Equals(obj as LicenseInfo);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                hashCode += (hashCode * 31) ^ IssuerName.GetHashCode();
                hashCode += (hashCode * 31) ^ LicenseeName.GetHashCode();
                hashCode += (hashCode * 31) ^ OrganizationName.GetHashCode();
                hashCode += (hashCode * 31) ^ IssueDate.GetHashCode();
                hashCode += (hashCode * 31) ^ UseComputerName.GetHashCode();
                hashCode += (hashCode * 31) ^ UseMACAddress.GetHashCode();
                hashCode += (hashCode * 31) ^ UseSerialNumber.GetHashCode();
                hashCode += (hashCode * 31) ^ UserString.GetHashCode();
                foreach(var kvp in ComponentIDToInfo)
                {
                    hashCode += (hashCode * 31) ^ (int) kvp.Key;
                    hashCode += (hashCode * 31) ^ kvp.Value.GetHashCode();
                }

                return hashCode;
            }
        }

        /// <summary>
        /// Creates license file
        /// </summary>
        /// <returns>License code saved the file</returns>
        public string SaveToFile(string licenseFileName)
        {
            using (StreamWriter licenseFile = new StreamWriter(licenseFileName))
            {
                licenseFile.WriteLine(Version);
                string code = CreateCode();
                licenseFile.WriteLine(code);
                licenseFile.Flush();
                return code;
            }
        }

        public string LoadFromFile(string licenseFileName)
        {
            using (StreamReader licenseFile = new StreamReader(licenseFileName))
            {
                var versionString = licenseFile.ReadLine();
                if (int.TryParse(versionString, out int value) && value != Version)
                {
                    var ex = new Exception("ELI50164: Unknown License version");
                    ex.Data["Version in File"] = versionString;
                    ex.Data["Current version"] = Version;
                    throw ex;
                }
                var code = licenseFile.ReadLine().Trim();
                FromCode(code);
                return code;
            }

        }

        public void CopyFrom(LicenseInfo licenseInfo)
        {
            IssuerName = licenseInfo.IssuerName;
            LicenseeName = licenseInfo.LicenseeName;
            OrganizationName = licenseInfo.OrganizationName;
            IssueDate = licenseInfo.IssueDate;
            UseComputerName = licenseInfo.UseComputerName;
            UseMACAddress = licenseInfo.UseMACAddress;
            UseSerialNumber = licenseInfo.UseSerialNumber;
            UserString = licenseInfo.UserString;
            
            foreach (var component in licenseInfo.ComponentIDToInfo)
            {
                ComponentIDToInfo[component.Key] = component.Value.PermanentLicense
                    ? new ComponentInfo()
                    : new ComponentInfo(component.Value.ExpirationDate);
            }
        }
    }
    
}
