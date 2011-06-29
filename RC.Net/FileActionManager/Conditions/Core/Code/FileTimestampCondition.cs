using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Conditions
{
    /// <summary>
    /// Represents the available timestamp properties of a file.
    /// </summary>
    [ComVisible(true)]
    [Guid("FA88AF8A-2C67-4A87-AA63-803971B0BC52")]
    public enum FileTimestampProperty
    {
        /// <summary>
        /// The date and time the file was created.
        /// </summary>
        Created = 0,

        /// <summary>
        /// The date and time the file was accessed.
        /// </summary>
        Accessed = 1,

        /// <summary>
        /// The date and time the file was modified.
        /// </summary>
        Modified = 2
    }

    /// <summary>
    /// Specifies how a file timestamp is to be compared by the <see cref="FileTimestampCondition"/>.
    /// </summary>
    [ComVisible(true)]
    [Guid("D7010110-8EDB-45F3-BF59-C67AB4B7BDE6")]
    public enum DateComparisonOperator
    {
        /// <summary>
        /// Tests whether the timestamp is equal to the reference <see cref="DateTime"/>.
        /// </summary>
        Equal = 0,

        /// <summary>
        /// Tests whether the timestamp is not equal to the reference <see cref="DateTime"/>.
        /// </summary>
        NotEqual = 1,

        /// <summary>
        /// Tests whether the timestamp is before the reference <see cref="DateTime"/>.
        /// </summary>
        Before = 2,

        /// <summary>
        /// Tests whether the timestamp is equal to or before the reference <see cref="DateTime"/>.
        /// </summary>
        OnOrBefore = 3,

        /// <summary>
        /// Tests whether the timestamp is after the reference <see cref="DateTime"/>.
        /// </summary>
        After = 4,

        /// <summary>
        /// Tests whether the timestamp is equal to or after the reference <see cref="DateTime"/>.
        /// </summary>
        OnOrAfter = 5
    }

    /// <summary>
    /// Specifes a time span unit for calculating <see cref="DateTime"/>s relative to the current
    /// time in the <see cref="FileTimestampCondition"/>.
    /// </summary>
    [ComVisible(true)]
    [Guid("5162F5EC-2D3D-4DC2-9AEC-6EC30200C6AC")]
    public enum TimeSpanUnit
    {
        /// <summary>
        /// Seconds.
        /// </summary>
        Seconds = 0,

        /// <summary>
        /// Minutes.
        /// </summary>
        Minutes = 1,

        /// <summary>
        /// Hours.
        /// </summary>
        Hours = 2,

        /// <summary>
        /// Days.
        /// </summary>
        Days = 3,

        /// <summary>
        /// Weeks.
        /// </summary>
        Weeks = 4,

        /// <summary>
        /// Months.
        /// </summary>
        Months = 5,

        /// <summary>
        /// Years.
        /// </summary>
        Years = 6
    }

    /// <summary>
    /// Specifies the way in which a file <see cref="DateTime"/> will be compared by the
    /// <see cref="FileTimestampCondition"/>.
    /// </summary>
    [ComVisible(true)]
    [Guid("AC3E1B4E-78A5-4CB2-95F3-88274AF2A7D0")]
    public enum TimestampComparisonMethod
    {
        /// <summary>
        /// The file timestamp will be compared against a static range of time.
        /// </summary>
        RangeComparison = 0,

        /// <summary>
        /// The file timestamp will be compared against a static <see cref="DateTime"/>.
        /// </summary>
        StaticComparison = 1,

        /// <summary>
        /// The file timestamp will be compared against <see cref="DateTime"/> relative to the
        /// current time.
        /// </summary>
        RelativeComparison = 2,

        /// <summary>
        /// The file timestamp will be compared against a colloquial time period.
        /// </summary>
        RelativeTimePeriod = 3,

        /// <summary>
        /// The file timestamp will be compared against the a timestamp of a specified file.
        /// </summary>
        FileComparison = 4
    }

    /// <summary>
    /// Colloquial time periods which can be used for comparison by the
    /// <see cref="FileTimestampCondition"/>.
    /// </summary>
    [ComVisible(true)]
    [Guid("8A27F48C-EC61-4A20-BAC1-40B792D14EAB")]
    public enum TimePeriod
    {
        /// <summary>
        /// Any time from the start of the current minute up to the start of the next minute.
        /// </summary>
        ThisMinute = 0,

        /// <summary>
        /// Any time from the start of the last minute up to the start of this minute.
        /// </summary>
        LastMinute = 1,

        /// <summary>
        /// Any time from the start of this hour up to the start of next hour.
        /// </summary>
        ThisHour = 2,

        /// <summary>
        /// Any time from the start of this hour up to the start of this hour.
        /// </summary>
        LastHour = 3,

        /// <summary>
        /// Any time from midnight today up to midnight tomorrow.
        /// </summary>
        Today = 4,

        /// <summary>
        /// Any time from midnight yesterday up to midnight today.
        /// </summary>
        Yesterday = 5,

        /// <summary>
        /// Any time this week where the start and end of the week is dictated by
        /// CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek.
        /// </summary>
        ThisWeek = 6,

        /// <summary>
        /// Any time last week where the start and end of the week is dictated by
        /// CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek.
        /// </summary>
        LastWeek = 7,

        /// <summary>
        /// Any time this month.
        /// </summary>
        ThisMonth = 8,

        /// <summary>
        /// Any time last month.
        /// </summary>
        LastMonth = 9,

        /// <summary>
        /// Any time this year.
        /// </summary>
        ThisYear = 10,

        /// <summary>
        /// Any time last year.
        /// </summary>
        LastYear = 11
    }

    /// <summary>
    /// A <see cref="IFAMCondition"/> based on a timestamp of a file.
    /// </summary>
    [ComVisible(true)]
    [Guid("256BE123-C083-4356-B7F5-D879AEE1AAD6")]
    [ProgId("Extract.FileActionManager.Conditions.FileTimeStampCondition")]
    public class FileTimestampCondition : ICategorizedComponent, IConfigurableObject,
        IMustBeConfiguredObject, ICopyableObject, IFAMCondition, ILicensedComponent,
        IPersistStream
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "File timestamp condition";

        /// <summary>
        /// Current task version.
        /// </summary>
        internal const int _CURRENT_VERSION = 1;

        #endregion Constants

        #region Fields

        /// <summary>
        /// If <see langword="true"/> the condition will be met if the specified condition is
        /// <see langword="true"/>; if <see langword="false"/> the condition will be met if the
        /// specified condition is <see langword="false"/>.
        /// </summary>
        bool _metIfTrue = true;

        /// <summary>
        /// Specifies name of the file whose timestamp is to be compared.
        /// </summary>
        string _file1Name = "<SourceDocName>";

        /// <summary>
        /// Specifies which timestamp property from the primary file should be compared.
        /// </summary>
        FileTimestampProperty _file1Property = FileTimestampProperty.Modified;

        /// <summary>
        /// When comparing using the when using the
        /// <see cref="TimestampComparisonMethod.RangeComparison"/> method, the start
        /// <see cref="DateTime"/> of the range.
        /// </summary>
        DateTime _rangeStartDateTime;

        /// <summary>
        /// When comparing using the when using the
        /// <see cref="TimestampComparisonMethod.RangeComparison"/> method, the end
        /// <see cref="DateTime"/> of the range.
        /// </summary>
        DateTime _rangeEndDateTime;

        /// <summary>
        /// When comparing against another <see cref="DateTime"/> or time period, the
        /// <see cref="DateComparisonOperator"/> to use.
        /// </summary>
        DateComparisonOperator _comparisonOperator = DateComparisonOperator.Equal;

        /// <summary>
        /// When comparing against another <see cref="DateTime"/> or time period, the
        /// <see cref="TimestampComparisonMethod"/> to use.
        /// </summary>
        TimestampComparisonMethod _comparisonMethod = TimestampComparisonMethod.StaticComparison;

        /// <summary>
        /// The <see cref="DateTime"/> to compare against when using the
        /// <see cref="TimestampComparisonMethod.StaticComparison"/> method.
        /// </summary>
        DateTime _comparisonDateTime;

        /// <summary>
        /// The number of the specified <see cref="TimeSpanUnit"/>s ago to use when using a relative
        /// comparison.
        /// </summary>
        int _relativeNumberAgo = 0;

        /// <summary>
        /// The <see cref="TimeSpanUnit"/> to use when using the
        /// <see cref="TimestampComparisonMethod.RelativeComparison"/> method.
        /// </summary>
        TimeSpanUnit _relativeTimeSpanUnit = TimeSpanUnit.Days;

        /// <summary>
        /// The <see cref="TimePeriod"/> to use when using the
        /// <see cref="TimestampComparisonMethod.RelativeTimePeriod"/> method.
        /// </summary>
        TimePeriod _relativeTimePeriod = TimePeriod.Today;

        /// <summary>
        /// Specifies which timestamp property from the file to be compared to the primary file.
        /// </summary>
        FileTimestampProperty _file2Property = FileTimestampProperty.Modified;

        /// <summary>
        /// Specifies name of the file whose timestamp is to be compared to the primary file.
        /// </summary>
        string _file2Name;

        /// <summary>
        /// <see langword="true"/> if changes have been made to
        /// <see cref="FileTimestampCondition"/> since it was created;
        /// <see langword="false"/> if no changes have been made since it was created.
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTimestampCondition"/> class.
        /// </summary>
        public FileTimestampCondition()
        {
            try
            {
                // Use midnight today as the default date time to use for the DateTime fields.
                DateTime defaultDateTime = DateTime.Now.Date;
                _rangeStartDateTime = defaultDateTime;
                _rangeEndDateTime = defaultDateTime;
                _comparisonDateTime = defaultDateTime;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32782");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTimestampCondition"/> class as a copy
        /// of the specified <see paramref="task"/>.
        /// </summary>
        /// <param name="task">The <see cref="FileTimestampCondition"/> from which
        /// settings should be copied.</param>
        public FileTimestampCondition(FileTimestampCondition task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32783");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the condition is met when the specified
        /// condition is <see langword="true"/>.
        /// </summary>
        /// <value>If <see langword="true"/> the condition will be met if the specified condition is
        /// <see langword="true"/>; if <see langword="false"/> the condition will be met if the
        /// specified condition is <see langword="false"/>.
        /// </value>
        public bool MetIfTrue
        {
            get
            {
                return _metIfTrue;
            }

            set
            {
                if (value != _metIfTrue)
                {
                    _metIfTrue = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the file whose timestamp is to be compared.
        /// </summary>
        /// <value>
        /// The name of the file whose timestamp is to be compared.
        /// </value>
        public string File1Name
        {
            get
            {
                return _file1Name;
            }

            set
            {
                try
                {
                    if (value != _file1Name)
                    {
                        _file1Name = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32781", ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets or sets which timestamp property from the primary file should be compared.
        /// </summary>
        /// <value>The <see cref="FileTimestampProperty"/> to be compared.</value>
        public FileTimestampProperty File1Property
        {
            get
            {
                return _file1Property;
            }

            set
            {
                if (value != _file1Property)
                {
                    _file1Property = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the start <see cref="DateTime"/> of the range to use when comparing using
        /// the <see cref="TimestampComparisonMethod.RangeComparison"/> method.
        /// </summary>
        /// <value>The the start <see cref="DateTime"/> of the range.</value>
        public DateTime RangeStartDateTime
        {
            get
            {
                return _rangeStartDateTime;
            }

            set
            {
                try
                {
                    if (value != _rangeStartDateTime)
                    {
                        _rangeStartDateTime = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32812", ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets or sets the start <see cref="DateTime"/> of the range to use when comparing using
        /// the <see cref="TimestampComparisonMethod.RangeComparison"/> method.
        /// </summary>
        /// <value>The the end <see cref="DateTime"/> of the range.</value>
        public DateTime RangeEndDateTime
        {
            get
            {
                return _rangeEndDateTime;
            }

            set
            {
                try
                {
                    if (value != _rangeEndDateTime)
                    {
                        _rangeEndDateTime = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32813", ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DateComparisonOperator"/> to use when comparing against
        /// another <see cref="DateTime"/> or time period
        /// </summary>
        /// <value>The <see cref="DateComparisonOperator"/> to use.</value>
        public DateComparisonOperator ComparisonOperator
        {
            get
            {
                return _comparisonOperator;
            }

            set
            {
                if (value != _comparisonOperator)
                {
                    _comparisonOperator = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="TimestampComparisonMethod"/> to use when comparing against
        /// another <see cref="DateTime"/> or time period.
        /// </summary>
        /// <value>The <see cref="TimestampComparisonMethod"/> to use.</value>
        public TimestampComparisonMethod ComparisonMethod
        {
            get
            {
                return _comparisonMethod;
            }

            set
            {
                if (value != _comparisonMethod)
                {
                    _comparisonMethod = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DateTime"/> to compare against when using the
        /// the <see cref="TimestampComparisonMethod.StaticComparison"/> method.
        /// </summary>
        /// <value>The  the <see cref="DateTime"/> to compare against.</value>
        public DateTime ComparisonDateTime
        {
            get
            {
                return _comparisonDateTime;
            }

            set
            {
                try
                {
                    if (value != _comparisonDateTime)
                    {
                        _comparisonDateTime = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32810", ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets or sets the number of the specified <see cref="TimeSpanUnit"/>s ago to use when
        /// using the <see cref="TimestampComparisonMethod.RelativeComparison"/> method.
        /// </summary>
        /// <value>The number of the specified <see cref="TimeSpanUnit"/>s ago to use.</value>
        public int RelativeNumberAgo
        {
            get
            {
                return _relativeNumberAgo;
            }

            set
            {
                if (value != _relativeNumberAgo)
                {
                    _relativeNumberAgo = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="TimeSpanUnit"/> to use when using the
        /// <see cref="TimestampComparisonMethod.RelativeComparison"/> method.
        /// </summary>
        /// <value>The <see cref="TimeSpanUnit"/> to use.</value>
        public TimeSpanUnit RelativeTimeSpanUnit
        {
            get
            {
                return _relativeTimeSpanUnit;
            }

            set
            {
                if (value != _relativeTimeSpanUnit)
                {
                    _relativeTimeSpanUnit = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="TimePeriod"/> to use when using the
        /// <see cref="TimestampComparisonMethod.RelativeTimePeriod"/> method.
        /// </summary>
        /// <value>The <see cref="TimePeriod"/> to use.</value>
        public TimePeriod RelativeTimePeriod
        {
            get
            {
                return _relativeTimePeriod;
            }

            set
            {
                if (value != _relativeTimePeriod)
                {
                    _relativeTimePeriod = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="FileTimestampProperty"/> from the file to be compared to the
        /// primary file.
        /// </summary>
        /// <value>The <see cref="FileTimestampProperty"/> from the file to be compared to the primary
        /// file.</value>
        public FileTimestampProperty File2Property
        {
            get
            {
                return _file2Property;
            }

            set
            {
                if (value != _file2Property)
                {
                    _file2Property = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the file whose timestamp is to be compared to the primary
        /// file.
        /// </summary>
        /// <value>The name of the file whose timestamp is to be compared to the primary file.
        /// </value>
        public string File2Name
        {
            get
            {
                return _file2Name;
            }

            set
            {
                try
                {
                    if (value != _file2Name)
                    {
                        _file2Name = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32811", ex.Message);
                }
            }
        }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Validates the instance's current settings.
        /// </summary>
        /// <throws><see cref="ExtractException"/> if the instance's settings are not valid.</throws>
        public void ValidateSettings()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(File1Name))
                {
                    throw new ExtractException("ELI32814",
                        "A file whose timestamp is to be evaluated must be specified.");
                }

                if (ComparisonMethod == TimestampComparisonMethod.RangeComparison &&
                    RangeStartDateTime > RangeEndDateTime)
                {
                    throw new ExtractException("ELI32815",
                       "A file to compare against must be specified.");
                }

                if (ComparisonMethod == TimestampComparisonMethod.FileComparison &&
                    string.IsNullOrWhiteSpace(File2Name))
                {
                    throw new ExtractException("ELI32816",
                        "A file to compare against must be specified.");
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32817", ex.Message);
            }
        }

        #endregion Public Methods

        #region IFAMCondition Members

        /// <summary>
        /// Compares the the specified timestamp using the configured parameters to determine if the
        /// condition is met.
        /// </summary>
        /// <param name="pFileRecord">A <see cref="FileRecord"/> specifing the file to be tested.
        /// </param>
        /// <param name="pFPDB">The <see cref="FileProcessingDB"/> currently in use.</param>
        /// <param name="lActionID">The ID of the database action in use.</param>
        /// <param name="pFAMTagManager">A <see cref="FAMTagManager"/> to be used to evaluate any
        /// FAM tags used by the condition.</param>
        /// <returns><see langword="true"/> if the condition was met, <see langword="false"/> if it
        /// was not.</returns>
        public bool FileMatchesFAMCondition(FileRecord pFileRecord, FileProcessingDB pFPDB,
            int lActionID, FAMTagManager pFAMTagManager)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI32784",
                    _COMPONENT_DESCRIPTION);

                ValidateSettings();

                bool comparisonIsTrue;

                // Get the timestamp DateTime to test.
                string file1Name = pFAMTagManager.ExpandTags(File1Name, pFileRecord.Name);
                DateTime file1DateTime = GetFileDateTime(file1Name, File1Property);

                if (ComparisonMethod == TimestampComparisonMethod.FileComparison)
                {
                    // Check to see if it is within the specified range (inclusive).
                    comparisonIsTrue =
                        CompareDates(file1DateTime, RangeStartDateTime, DateComparisonOperator.OnOrAfter) &&
                        CompareDates(file1DateTime, RangeEndDateTime, DateComparisonOperator.OnOrBefore);
                }
                else if (ComparisonMethod == TimestampComparisonMethod.RelativeTimePeriod)
                {
                    comparisonIsTrue = CompareRelativeTimePeriod(file1DateTime);
                }
                else
                {
                    // If using any of the comparison methods except RelativeTimePeriod, first find
                    // the DateTime to compare against.
                    DateTime comparisonDateTime;

                    switch (ComparisonMethod)
                    {
                        case TimestampComparisonMethod.StaticComparison:
                            comparisonDateTime = ComparisonDateTime;
                            break;

                        case TimestampComparisonMethod.RelativeComparison:
                            comparisonDateTime = GetRelativeDateTime();
                            break;

                        case TimestampComparisonMethod.FileComparison:
                            {
                                string file2Name = pFAMTagManager.ExpandTags(File2Name, pFileRecord.Name);
                                comparisonDateTime = GetFileDateTime(file2Name, File2Property);
                            }
                            break;

                        default:
                            throw new ExtractException("ELI32785",
                                "Unexpected file timestamp comparison method.");
                    }

                    // Once we have the comparisonDateTime, compare using the configured ComparisonOperator.
                    comparisonIsTrue = CompareDates(file1DateTime, comparisonDateTime, ComparisonOperator);
                }

                return comparisonIsTrue == MetIfTrue;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI32786",
                    "Error occured in '" + _COMPONENT_DESCRIPTION + "'", ex);
            }
        }
        
        /// <summary>
        /// Returns bool value indicating if the condition requires admin access
        /// </summary>
        /// <returns><see langword="true"/> if the condition requires admin access
        /// <see langword="false"/> if condition does not require admin access</returns>
        public bool RequiresAdminAccess()
        {
            return false;
        }

        #endregion IFAMCondition Members

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="FileTimestampCondition"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI32793",
                    _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                FileTimestampCondition cloneOfThis = (FileTimestampCondition)Clone();

                using (FileTimestampConditionSettingsDialog dlg
                    = new FileTimestampConditionSettingsDialog(cloneOfThis))
                {
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        CopyFrom(dlg.Settings);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32794", "Error running configuration.");
            }
        }

        #endregion IConfigurableObject Members

        #region IMustBeConfiguredObject Members

        /// <summary>
        /// Checks if the object has been configured properly.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the object has been configured and <see langword="false"/>
        /// otherwise.
        /// </returns>
        public bool IsConfigured()
        {
            try
            {
                try
                {
                    // Return true if ValidateSettings does not throw an exception.
                    ValidateSettings();

                    return true;
                }
                catch
                {
                    // Otherwise return false and eat the exception.
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32685",
                    "Failed to check '" + _COMPONENT_DESCRIPTION + "' configuration.");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="FileTimestampCondition"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="FileTimestampCondition"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                return new FileTimestampCondition(this);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32795",
                    "Failed to clone '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="FileTimestampCondition"/> instance into
        /// this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var source = pObject as FileTimestampCondition;
                if (source == null)
                {
                    throw new InvalidCastException("Invalid cast to FileTimeStampCondition");
                }
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32796",
                    "Failed to copy '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        #endregion ICopyableObject Members

        #region ICategorizedComponent Members

        /// <summary>
        /// Gets the name of the COM object.
        /// </summary>
        /// <returns>The name of the COM object.</returns>
        public string GetComponentDescription()
        {
            return _COMPONENT_DESCRIPTION;
        }

        #endregion ICategorizedComponent Members

        #region ILicensedComponent Members

        /// <summary>
        /// Gets whether this component is licensed.
        /// </summary>
        /// <returns><see langword="true"/> if the component is licensed; <see langword="false"/> 
        /// if the component is not licensed.</returns>
        public bool IsLicensed()
        {
            return LicenseUtilities.IsLicensed(LicenseIdName.ExtractCoreObjects);
        }

        #endregion ILicensedComponent Members

        #region IPersistStream Members

        /// <summary>
        /// Returns the class identifier (CLSID) <see cref="Guid"/> for the component object.
        /// </summary>
        /// <param name="classID">Pointer to the location of the CLSID <see cref="Guid"/> on 
        /// return.</param>
        public void GetClassID(out Guid classID)
        {
            classID = GetType().GUID;
        }

        /// <summary>
        /// Checks the object for changes since it was last saved.
        /// </summary>
        /// <returns>
        ///   <see cref="HResult.Ok"/> if changes have been made;
        /// <see cref="HResult.False"/> if changes have not been made.
        /// </returns>
        public int IsDirty()
        {
            return HResult.FromBoolean(_dirty);
        }

        /// <summary>
        /// Initializes an object from the IStream where it was previously saved.
        /// </summary>
        /// <param name="stream">IStream from which the object should be loaded.</param>
        public void Load(System.Runtime.InteropServices.ComTypes.IStream stream)
        {
            try
            {
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    MetIfTrue = reader.ReadBoolean();
                    File1Name = reader.ReadString();
                    File1Property = (FileTimestampProperty)reader.ReadInt32();
                    RangeStartDateTime = reader.ReadStruct<DateTime>();
                    RangeEndDateTime = reader.ReadStruct<DateTime>();
                    ComparisonOperator = (DateComparisonOperator)reader.ReadInt32();
                    ComparisonMethod = (TimestampComparisonMethod)reader.ReadInt32();
                    ComparisonDateTime = reader.ReadStruct<DateTime>();
                    RelativeNumberAgo = reader.ReadInt32();
                    RelativeTimeSpanUnit = (TimeSpanUnit)reader.ReadInt32();
                    RelativeTimePeriod = (TimePeriod)reader.ReadInt32();
                    File2Property = (FileTimestampProperty)reader.ReadInt32();
                    File2Name = reader.ReadString();
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32797",
                    "Failed to load '" + _COMPONENT_DESCRIPTION + "'.");
            }
        }

        /// <summary>
        /// Saves an object into the specified IStream and indicates whether the object should reset
        /// its dirty flag.
        /// </summary>
        /// <param name="stream">IStream into which the object should be saved.</param>
        /// <param name="clearDirty">Value that indicates whether to clear the dirty flag after the
        /// save is complete. If <see langword="true"/>, the flag should be cleared. If
        /// <see langword="false"/>, the flag should be left unchanged.</param>
        public void Save(System.Runtime.InteropServices.ComTypes.IStream stream, bool clearDirty)
        {
            try
            {
                ValidateSettings();

                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    writer.Write(MetIfTrue);
                    writer.Write(File1Name);
                    writer.Write((int)File1Property);
                    writer.WriteStruct(RangeStartDateTime);
                    writer.WriteStruct(RangeEndDateTime);
                    writer.Write((int)ComparisonOperator);
                    writer.Write((int)ComparisonMethod);
                    writer.WriteStruct(ComparisonDateTime);
                    writer.Write(RelativeNumberAgo);
                    writer.Write((int)RelativeTimeSpanUnit);
                    writer.Write((int)RelativeTimePeriod);
                    writer.Write((int)File2Property);
                    writer.Write(File2Name);

                    // Write to the provided IStream.
                    writer.WriteTo(stream);
                }

                if (clearDirty)
                {
                    _dirty = false;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32798",
                    "Failed to save '" + _COMPONENT_DESCRIPTION + "'.");
            }
        }

        /// <summary>
        /// Returns the size in bytes of the stream needed to save the object.
        /// </summary>
        /// <param name="size">Pointer to a 64-bit unsigned integer value indicating the size, in
        /// bytes, of the stream needed to save this object.</param>
        public void GetSizeMax(out long size)
        {
            throw new NotImplementedException();
        }

        #endregion IPersistStream Members

        #region Private Members

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// "Extract FAM Conditions" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        static void RegisterFunction(Type type)
        {
            ComMethods.RegisterTypeInCategory(type, ExtractGuids.FileActionManagerConditions);
        }

        /// <summary>
        /// Code to be executed upon unregistration in order to remove this class from the
        /// "Extract FAM Conditions" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractGuids.FileActionManagerConditions);
        }

        /// <summary>
        /// Copies the specified <see cref="FileTimestampCondition"/> instance into this one.
        /// </summary>
        /// <param name="source">The <see cref="FileTimestampCondition"/> from which to copy.
        /// </param>
        void CopyFrom(FileTimestampCondition source)
        {
            MetIfTrue = source.MetIfTrue;
            File1Name = source.File1Name;
            File1Property = source.File1Property;
            RangeStartDateTime = source.RangeStartDateTime;
            RangeEndDateTime = source.RangeEndDateTime;
            ComparisonOperator = source.ComparisonOperator;
            ComparisonMethod = source.ComparisonMethod;
            ComparisonDateTime = source.ComparisonDateTime;
            RelativeNumberAgo = source.RelativeNumberAgo;
            RelativeTimeSpanUnit = source.RelativeTimeSpanUnit;
            RelativeTimePeriod = source.RelativeTimePeriod;
            File2Property = source.File2Property;
            File2Name = source.File2Name;

            _dirty = true;
        }

        /// <summary>
        /// Gets the <see cref="DateTime"/> to use for relative time comparisons.
        /// </summary>
        /// <returns>The <see cref="DateTime"/> calculated relative to the current time.</returns>
        DateTime GetRelativeDateTime()
        {
            DateTime comparisonDateTime = DateTime.Now;

            switch (RelativeTimeSpanUnit)
            {
                case TimeSpanUnit.Seconds:
                    comparisonDateTime = comparisonDateTime.AddSeconds(-RelativeNumberAgo);
                    break;

                case TimeSpanUnit.Minutes:
                    comparisonDateTime =
                        comparisonDateTime.AddMinutes(-RelativeNumberAgo);
                    break;

                case TimeSpanUnit.Hours:
                    comparisonDateTime =
                        comparisonDateTime.AddHours(-RelativeNumberAgo);
                    break;

                case TimeSpanUnit.Days:
                    comparisonDateTime =
                        comparisonDateTime.AddDays(-RelativeNumberAgo);
                    break;

                case TimeSpanUnit.Weeks:
                    comparisonDateTime =
                        comparisonDateTime.AddDays(-RelativeNumberAgo * 7);
                    break;

                case TimeSpanUnit.Months:
                    comparisonDateTime =
                        comparisonDateTime.AddMonths(-RelativeNumberAgo);
                    break;

                case TimeSpanUnit.Years:
                    comparisonDateTime =
                        comparisonDateTime.AddYears(-RelativeNumberAgo);
                    break;

                default:
                    throw new ExtractException("ELI32787", "Unexpected time period");
            }

            return comparisonDateTime;
        }

        /// <summary>
        /// Compares the <see paramref="fileDateTime"/> to the <see paramref="comparisonDateTime"/>
        /// using the specified <see paramref="comparisonOperator"/>.
        /// </summary>
        /// <param name="fileDateTime">The file <see cref="DateTime"/> to compare</param>
        /// <param name="comparisonDateTime">The <see cref="DateTime"/> to compare
        /// <see paramref="fileDateTime"/> against.</param>
        /// <param name="comparisonOperator"></param>
        /// <returns><see langword="true"/> if the comparison is true; <see langword="false"/>
        /// otherwise.</returns>
        static bool CompareDates(DateTime fileDateTime, DateTime comparisonDateTime,
            DateComparisonOperator comparisonOperator)
        {
            bool comparisonIsTrue;
            switch (comparisonOperator)
            {
                case DateComparisonOperator.Equal:
                    comparisonIsTrue = fileDateTime.Floor(DateTimeUnit.Second) ==
                                       comparisonDateTime.Floor(DateTimeUnit.Second);
                    break;

                case DateComparisonOperator.NotEqual:
                    comparisonIsTrue = fileDateTime.Floor(DateTimeUnit.Second) !=
                                       comparisonDateTime.Floor(DateTimeUnit.Second);
                    break;

                case DateComparisonOperator.After:
                    comparisonIsTrue = fileDateTime.Floor(DateTimeUnit.Second) >
                                       comparisonDateTime.Floor(DateTimeUnit.Second);
                    break;

                case DateComparisonOperator.OnOrAfter:
                    comparisonIsTrue = fileDateTime.Floor(DateTimeUnit.Second) >=
                                       comparisonDateTime.Floor(DateTimeUnit.Second);
                    break;

                case DateComparisonOperator.Before:
                    comparisonIsTrue = fileDateTime.Floor(DateTimeUnit.Second) <
                                       comparisonDateTime.Floor(DateTimeUnit.Second);
                    break;

                case DateComparisonOperator.OnOrBefore:
                    comparisonIsTrue = fileDateTime.Floor(DateTimeUnit.Second) <=
                                       comparisonDateTime.Floor(DateTimeUnit.Second);
                    break;

                default:
                    throw new ExtractException("ELI32788", "Unexpected comparison operator.");
            }
            return comparisonIsTrue;
        }

        /// <summary>
        /// Compares the <see paramref="fileDateTime"/> to the configured
        /// <see cref="RelativeTimePeriod"/> using the configured <see cref="ComparisonOperator"/>.
        /// </summary>
        /// <param name="fileDateTime">The file <see cref="DateTime"/> to compare.</param>
        /// <returns><see langword="true"/> if the comparison is true; <see langword="false"/>
        /// otherwise.</returns>
        bool CompareRelativeTimePeriod(DateTime fileDateTime)
        {
            DateTime comparisonDateTime = DateTime.Now;
            DateTimeUnit dateTimePart;

            switch (RelativeTimePeriod)
            {
                case TimePeriod.ThisMinute:
                    dateTimePart = DateTimeUnit.Minute;
                    break;

                case TimePeriod.LastMinute:
                    comparisonDateTime = comparisonDateTime.AddMinutes(-1);
                    dateTimePart = DateTimeUnit.Minute;
                    break;

                case TimePeriod.ThisHour:
                    dateTimePart = DateTimeUnit.Hour;
                    break;

                case TimePeriod.LastHour:
                    comparisonDateTime = comparisonDateTime.AddHours(-1);
                    dateTimePart = DateTimeUnit.Hour;
                    break;

                case TimePeriod.Today:
                    dateTimePart = DateTimeUnit.Day;
                    break;

                case TimePeriod.Yesterday:
                    comparisonDateTime = comparisonDateTime.AddDays(-1);
                    dateTimePart = DateTimeUnit.Day;
                    break;

                case TimePeriod.ThisWeek:
                    dateTimePart = DateTimeUnit.Week;
                    break;

                case TimePeriod.LastWeek:
                    comparisonDateTime = comparisonDateTime.AddDays(-7);
                    dateTimePart = DateTimeUnit.Week;
                    break;

                case TimePeriod.ThisMonth:
                    dateTimePart = DateTimeUnit.Month;
                    break;

                case TimePeriod.LastMonth:
                    comparisonDateTime = comparisonDateTime.AddMonths(-1);
                    dateTimePart = DateTimeUnit.Month;
                    break;

                case TimePeriod.ThisYear:
                    dateTimePart = DateTimeUnit.Year;
                    break;

                case TimePeriod.LastYear:
                    comparisonDateTime = comparisonDateTime.AddYears(-1);
                    dateTimePart = DateTimeUnit.Year;
                    break;

                default:
                    throw new ExtractException("ELI32789",
                        "Unexpected date/time component");
            }

            switch (ComparisonOperator)
            {
                case DateComparisonOperator.Equal:
                case DateComparisonOperator.NotEqual:
                    {
                        DateTime periodStart = comparisonDateTime.Floor(dateTimePart);
                        DateTime periodEnd = comparisonDateTime.Ceiling(dateTimePart);

                        bool fallsInPeriod = (fileDateTime >= periodStart && fileDateTime < periodEnd);

                        if (ComparisonOperator == DateComparisonOperator.Equal)
                        {
                            return fallsInPeriod;
                        }
                        else
                        {
                            return !fallsInPeriod;
                        }
                    }

                case DateComparisonOperator.After:
                case DateComparisonOperator.OnOrBefore:
                    {
                        DateTime periodEnd = comparisonDateTime.Ceiling(dateTimePart);

                        if (ComparisonOperator == DateComparisonOperator.After)
                        {
                            return fileDateTime >= periodEnd;
                        }
                        else
                        {
                            return fileDateTime < periodEnd;
                        }
                    }

                case DateComparisonOperator.Before:
                case DateComparisonOperator.OnOrAfter:
                    {
                        DateTime periodStart = comparisonDateTime.Floor(dateTimePart);

                        if (ComparisonOperator == DateComparisonOperator.Before)
                        {
                            return fileDateTime < periodStart;
                        }
                        else
                        {
                            return fileDateTime >= periodStart;
                        }
                    }

                default:
                    throw new ExtractException("ELI32790", "Unexpected comparison operator.");
            }
        }

        /// <summary>
        /// Gets the <see cref="DateTime"/> of the specified <see paramref="fileProperty"/> of
        /// the specified <see paramref="fileName"/>.
        /// </summary>
        /// <param name="fileName">Name of the file whose timestamp is needed.</param>
        /// <param name="fileProperty">The <see cref="FileTimestampProperty"/> that is needed.
        /// </param>
        /// <returns>The <see cref="DateTime"/> of the specified <see paramref="fileProperty"/> of
        /// the specified <see paramref="fileName"/>.</returns>
        static DateTime GetFileDateTime(string fileName, FileTimestampProperty fileProperty)
        {
            if (!File.Exists(fileName))
            {
                ExtractException ee =
                    new ExtractException("ELI32791", "Could not find specified file");
                ee.AddDebugData("Filename", fileName, false);
                throw ee;
            }

            switch (fileProperty)
            {
                case FileTimestampProperty.Created:
                    return File.GetCreationTime(fileName);

                case FileTimestampProperty.Accessed:
                    return File.GetLastAccessTime(fileName);

                case FileTimestampProperty.Modified:
                    return File.GetLastWriteTime(fileName);

                default:
                    throw new ExtractException("ELI32792", "Unexpected timesptamp proerty");
            }
        }

        #endregion Private Members
    }
}
