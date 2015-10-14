using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Extract.FAMDBCounterManager
{
    /// <summary>
    /// Allows for creating and sending an email message.
    /// <para><b>Note</b></para>
    /// This class is a modified copy of Extract.Utilities.Email.ExtractEmailMessage. This project
    /// is not linked to Extract.Utilities.Email to avoid COM dependencies.
    /// </summary>
    internal class EmailMessage
    {
        #region Fields

        /// <summary>
        /// The list of recipients for the email.
        /// </summary>
        List<string> _recipients = new List<string>();

        /// <summary>
        /// The list of carbon copy recipients for the email.
        /// </summary>
        List<string> _carbonCopyRecipients = new List<string>();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailMessage"/> class.
        /// </summary>
        public EmailMessage()
        {
            // Sets default values for all members
            Clear();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Sets the email settings.
        /// </summary>
        /// <value>
        /// The email settings.
        /// </value>
        public EmailSettingsManager EmailSettings { get; set; }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>
        /// The subject.
        /// </value>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the body of the message.
        /// </summary>
        /// <value>
        /// The body of the message.
        /// </value>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the recipients of the message.
        /// </summary>
        /// <value>
        /// The recipients.
        /// </value>
        public string[] Recipients
        {
            get
            {
                return _recipients.ToArray();
            }
            set
            {
                _recipients = new List<string>(value);
            }
        }

        /// <summary>
        /// Gets or sets the carbon copy recipients of the message.
        /// </summary>
        /// <value>
        /// The carbon copy recipients.
        /// </value>
        public string[] CarbonCopyRecipients
        {
            get
            {
                return _carbonCopyRecipients.ToArray();
            }
            set
            {
                _carbonCopyRecipients = new List<string>(value);
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Sends the message.
        /// </summary>
        public void Send()
        {
            using (var client = new SmtpClient(EmailSettings.Server, EmailSettings.Port))
            using (var message = new MailMessage())
            {
                // Build the email message
                var sb = new StringBuilder();
                sb.AppendLine(Body);
                sb.AppendLine();

                AddRecipients(message.To);
                AddCarbonCopyRecipients(message.CC);
                message.Subject = Subject;
                message.Body = sb.ToString();
                message.From = new MailAddress(EmailSettings.SenderAddress,
                    EmailSettings.SenderName);

                string userName = EmailSettings.UserName;
                if (!string.IsNullOrWhiteSpace(userName))
                {
                    client.Credentials = new NetworkCredential(userName,
                        EmailSettings.Password);
                }

                int timeOut = EmailSettings.Timeout;
                if (timeOut > 0)
                {
                    client.Timeout = timeOut;
                }

                client.EnableSsl = EmailSettings.UseSsl;

                client.Send(message);
            }
        }

        /// <summary>
        /// Clears the message content.
        /// </summary>
        public void Clear()
        {
            EmailSettings = null;
            _recipients.Clear();
            _carbonCopyRecipients.Clear();
            Subject = string.Empty;
            Body = string.Empty;
        }

        /// <summary>
        /// Adds the mail recipients to the <see cref="MailAddressCollection"/>.
        /// </summary>
        /// <param name="addresses">The collection to add the addresses to.</param>
        void AddRecipients(MailAddressCollection addresses)
        {
            foreach (var address in _recipients)
            {
                addresses.Add(address);
            }
        }

        /// <summary>
        /// Adds the carbon copy recipients to the <see cref="MailAddressCollection"/>.
        /// </summary>
        /// <param name="addresses">The collection to add the addresses to.</param>
        void AddCarbonCopyRecipients(MailAddressCollection addresses)
        {
            foreach (var address in _carbonCopyRecipients)
            {
                addresses.Add(address);
            }
        }

        #endregion Methods
    }
}
