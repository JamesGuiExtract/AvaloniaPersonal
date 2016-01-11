using Extract.Interfaces;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Email;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Database
{
    /// <summary>
    /// A class used for tracking counts decremented by rule execution in the
    /// <see cref="FileProcessingDB"/>.
    /// </summary>
    [ComVisible(true)]
    [Guid("8CDB95DB-D580-4E26-B4FE-2EF777E3E712")]
    [ProgId("Extract.FileActionManager.Database.FAMDBRuleExecutionCounter")]
    [CLSCompliant(false)]
    public class FAMDBSecureCounter : IFAMDBSecureCounter, ISecureCounter
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(FAMDBSecureCounter).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="FileProcessingDB"/> that is the source of this counter.
        /// </summary>
        FileProcessingDB _fileProcessingDB;

        /// <summary>
        /// The ID of the counter.
        /// </summary>
        int _id;

        /// <summary>
        /// The name of the counter.
        /// </summary>
        string _name;

        /// <summary>
        /// The number of counts remaining at which the software should start sending alerts.
        /// </summary>
        int _alertLevel;

        /// <summary>
        /// How frequently alerts should be repeated after reaching _alertLevel.
        /// </summary>
        int _alertMultiple;

        /// <summary>
        /// In the case that _alertMultiple has not been explicitly set, what value should be
        /// assumed.
        /// </summary>
        int _effectiveAlertMultiple;

        /// <summary>
        /// <see langword="true"/> if this counter is valid; otherwise, <see langword="false"/>.
        /// </summary>
        bool _isValid;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FAMDBSecureCounter"/> class.
        /// </summary>
        public FAMDBSecureCounter()
        {
        }

        #endregion Constructors

        #region IFAMDBSecureCounter

        /// <summary>
        /// Initializes the counter of the specified <see paramref="counterID"/> against the
        /// specified <see paramref="fileProcessingDB"/>.
        /// </summary>
        /// <param name="fileProcessingDB">The <see cref="FileProcessingDB"/> that is the source of
        /// this counter.</param>
        /// <param name="ID">The ID of the counter.</param>
        /// <param name="name"></param>
        /// <param name="alertLevel"></param>
        /// <param name="alertMultiple"></param>
        /// <param name="isValid"><see langword="true"/> if this counter is valid; otherwise,
        /// <see langword="false"/>.</param>
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "0#")]
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "1#")]
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "2#")]
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "3#")]
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "4#")]
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "5#")]
        [SuppressMessage("Microsoft.Naming", "CA1725:ParameterNamesShouldMatchBaseDeclaration", MessageId = "6#")]
        public void Initialize(FileProcessingDB fileProcessingDB, int ID, string name,
            int alertLevel, int alertMultiple, bool isValid)
        {
            try
            {
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.ExtractCoreObjects, "ELI38744", _OBJECT_NAME);

                ExtractException.Assert("ELI38766", "Null argument exception.",
                    fileProcessingDB != null);

                _fileProcessingDB = fileProcessingDB;
                _id = ID;
                _name = name;
                _alertLevel = alertLevel;
                _alertMultiple = alertMultiple;
                if (_alertLevel > 0 && _alertMultiple <= 0)
                {
                    _effectiveAlertMultiple = _alertLevel;
                }
                else
                {
                    _effectiveAlertMultiple = _alertMultiple;
                }
                _isValid = isValid;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI38746", "Failed to initialize secure counter.");
            }
        }

        /// <summary>
        /// Gets the number of counts remaining at which the software should start sending alerts.
        /// </summary>
        public int AlertLevel
        {
            get
            {
                return _alertLevel;
            }
        }

        /// <summary>
        /// Gets how frequently alerts should be repeated after reaching <see cref="AlertLevel"/>.
        /// </summary>
        public int AlertMultiple
        {
            get
            {
                return _alertMultiple;
            }
        }

        #endregion IFAMDBSecureCounter

        #region ISecureCounter

        /// <summary>
        /// Gets the ID of the counter.
        /// </summary>
        public int ID
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// Gets the name of the counter.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the value of the counter.
        /// </summary>
        public int Value
        {
            get
            {
                try
                {
                    return _fileProcessingDB.GetSecureCounterValue(_id);
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI38769",
                        "Failed to retrieve value of '" + Name + "' counter");
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this counter is valid and can be used.
        /// </summary>
        /// <value><see langword="true"/> if this counter is valid; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsValid
        {
            get
            {
                return _isValid;
            }
        }

        /// <summary>
        /// Decrements the counter by the specified <see paramref="count"/> assuming enough counts
        /// are available.
        /// </summary>
        /// <param name="count">The number of counts to decrement.</param>
        /// <returns>The new number of counts left or -1 if there were not enough counts to be able
        /// to decrement.</returns>
        public int DecrementCounter(int count)
        {
            try
            {
                int newValue = _fileProcessingDB.DecrementSecureCounter(_id, count);

                if (AlertLevel > 0 && newValue <= AlertLevel)
                {
                    int lastValue = newValue + count;
                    if (lastValue > AlertLevel ||
                        ((AlertLevel - newValue) / _effectiveAlertMultiple !=
                         (AlertLevel - lastValue) / _effectiveAlertMultiple))
                    {
                        SendEmailAlert(newValue);
                    }
                }

                return newValue;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI38747",
                    "Failed to decrement '" + Name + "' counter");
            }
        }

        #endregion ISecureCounter

        #region Private Methods

        /// <summary>
        /// Sends an email to alert the recipients that the counter is running low.
        /// </summary>
        /// <param name="counterValue">The number of counts currently remaining in this counter.
        /// </param>
        void SendEmailAlert(int counterValue)
        {
            try
            {
                var emailSettings = new SmtpEmailSettings();
                emailSettings.LoadSettings(
                        new FAMDatabaseSettings<ExtractSmtp>(
                            _fileProcessingDB, false, SmtpEmailSettings.PropertyNameLookup));

                if (string.IsNullOrWhiteSpace(emailSettings.Server))
                {
                    throw new ExtractException("ELI39124",
                        "FAM DB email settings are not configured.");
                }
                else
                {
                    var requestTextGenerator = new SecureCounterTextManipulator(_fileProcessingDB);
                    requestTextGenerator.Reason = string.Format(CultureInfo.CurrentCulture,
                        "The counter \"{0}\" is running low.", _name);

                    var contactSettings = new FAMDatabaseSettings<LicenseContact>(
                        _fileProcessingDB, false);
                    requestTextGenerator.Organization
                        = contactSettings.Settings.LicenseContactOrganization;
                    requestTextGenerator.EmailAddress
                        = contactSettings.Settings.LicenseContactEmail;
                    requestTextGenerator.Phone
                        = contactSettings.Settings.LicenseContactPhone;

                    var emailMessage = new ExtractEmailMessage();
                    emailMessage.EmailSettings = emailSettings;

                    List<string> recipients = contactSettings.Settings.SendAlertsToExtract
                        ? new List<string>(new[] { "support@extractsystems.com" })
                        : new List<string>();

                    if (contactSettings.Settings.SendAlertsToSpecified)
                    {
                        recipients.AddRange(contactSettings.Settings.SpecifiedAlertRecipients
                            .Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => s.Trim())
                            .Where(s => !string.IsNullOrEmpty(s)));
                    }

                    if (!recipients.Any())
                    {
                        return;
                    }

                    emailMessage.Recipients = recipients.ToVariantVector();

                    if (!string.IsNullOrWhiteSpace(contactSettings.Settings.LicenseContactEmail))
                    {
                        emailMessage.CarbonCopyRecipients = new[] { contactSettings.Settings.LicenseContactEmail }.ToVariantVector();
                    }

                    emailMessage.Subject = string.Format(CultureInfo.CurrentCulture,
                        "FAM DB counter \"{0}\" is running low", _name);
                    emailMessage.Body = requestTextGenerator.GetRequestText();

                    emailMessage.Send();
                }
            }
            catch (Exception ex)
            {
                var ee = new ExtractException("ELI39125",
                    "Failed to send low counter value alert email.", ex);
                ee.AddDebugData("Counter name", _name, false);
                ee.AddDebugData("Counter value", counterValue, false);
                ee.Log();
            }
        }

        #endregion Private Methods
    }
}
