﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using UCLID_COMUTILSLib;

namespace Extract.Utilities.Email
{
    /// <summary>
    /// Interface for COM access to the extract mail message methods/functions.
    /// </summary>
    [Guid("5904787B-5F34-4BC3-B6D5-3B3C5EBEAACB")]
    [ComVisible(true)]
    [CLSCompliant(false)]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IExtractEmailMessage
    {
        #region Methods

        /// <summary>
        /// Sends the message.
        /// </summary>
        void Send();

        /// <summary>
        /// Clears the message.
        /// </summary>
        void Clear();

        /// <summary>
        /// Shows the email message in the local client mail application (if one is configured).
        /// </summary>
        /// <param name="zipAttachments">If <see langword="true"/> then any file attachments
        /// should be zipped.</param>
        void ShowInClient(bool zipAttachments);

        #endregion Methods

        #region Properties

        /// <summary>
        /// Sets the email settings.
        /// </summary>
        /// <value>
        /// The email settings.
        /// </value>
        ISmtpEmailSettings EmailSettings { get;  set; }

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>
        /// The subject.
        /// </value>
        string Subject { get; set; }

        /// <summary>
        /// Gets or sets the body of the message.
        /// </summary>
        /// <value>
        /// The body of the message.
        /// </value>
        string Body { get; set; }

        /// <summary>
        /// Gets or sets the attachments for the message.
        /// </summary>
        /// <value>
        /// The attachments.
        /// </value>
        [CLSCompliant(false)]
        VariantVector Attachments { get; set; }

        /// <summary>
        /// Gets or sets the recipients of the message.
        /// </summary>
        /// <value>
        /// The recipients.
        /// </value>
        [CLSCompliant(false)]
        VariantVector Recipients { get; set; }

        /// <summary>
        /// Gets or sets the sender address.
        /// </summary>
        /// <value>
        /// The sender address to use or <see langword="null"/> to use the value from the general
        /// Extract email settings.
        /// </value>
        string SenderAddress { get; set; }

        /// <summary>
        /// Gets or sets the name of the sender.
        /// </summary>
        /// <value>
        /// The name of the sender or <see langword="null"/> to use the value from the general
        /// Extract email settings.
        /// </value>
        string SenderName { get; set; }

        /// <summary>
        /// Allows the adding of a single recipient to the recipients list.
        /// </summary>
        /// <param name="recipient">The recipient to add to the list.</param>
        void AddRecipient(string recipient);

        #endregion Properties
    }

    /// <summary>
    /// Class for handling sending email.
    /// </summary>
    [Guid("ADBCE6D8-BF06-4219-819B-A1DB75F1562F")]
    [ProgId("Extract.Utilities.Email.ExtractEmailMessage")]
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    public class ExtractEmailMessage : IExtractEmailMessage
    {
        #region Constants

        /// <summary>
        /// Path to the EmailFile.exe
        /// </summary>
        static readonly string _EMAIL_FILE_EXE = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase),
            "EmailFile.exe");

        #endregion Constants

        #region Fields

        /// <summary>
        /// The list of files to attach to the email
        /// </summary>
        List<string> _fileAttachements = new List<string>();

        /// <summary>
        /// The list of recipients for the email.
        /// </summary>
        List<string> _recipients = new List<string>();

        /// <summary>
        /// The list of carbon copy recipients for the email.
        /// </summary>
        List<string> _carbonCopyRecipients = new List<string>();

        /// <summary>
        /// Mutex used to synchronize calls to Send();
        /// </summary>
        object _lock = new object();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractEmailMessage"/> class.
        /// </summary>
        public ExtractEmailMessage()
        {
            try
            {
                // Sets default values for all members
                Clear();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33217");
            }
        }

        #endregion Constructors

        #region IExtractEmailMessage Members

        /// <summary>
        /// Sends the message.
        /// </summary>
        public void Send()
        {
            try
            {
                lock (_lock)
                {
                    // Add support for secure protocols when this is run from non-CLR apps (else ServicePointManager.SecurityProtocol = Ssl3 | Tls)
                    // https://extract.atlassian.net/browse/ISSUE-18101
                    ServicePointManager.SecurityProtocol |=
                         SecurityProtocolType.Tls11
                        | SecurityProtocolType.Tls12
                        | SecurityProtocolType.Tls13;

                    using var client = new SmtpClient(EmailSettings.Server, EmailSettings.Port);
                    using var message = new MailMessage();
                    // Build the email message
                    var sb = new StringBuilder();
                    sb.AppendLine(Body);
                    sb.AppendLine();
                    sb.AppendLine(EmailSettings.EmailSignature);

                    AddRecipients(message.To);
                    AddCarbonCopyRecipients(message.CC);
                    message.Subject = Subject;
                    message.Body = sb.ToString();
                    message.From = new MailAddress(
                        SenderAddress ?? EmailSettings.SenderAddress,
                        SenderName ?? EmailSettings.SenderName);

                    if (_fileAttachements.Count > 0)
                    {
                        AddAttachments(message.Attachments);
                    }

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
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32256", "Failed to send message.");
            }
        }

        /// <summary>
        /// Clears the message content.
        /// </summary>
        public void Clear()
        {
            try
            {
                EmailSettings = null;
                _fileAttachements.Clear();
                _recipients.Clear();
                _carbonCopyRecipients.Clear();
                Subject = string.Empty;
                Body = string.Empty;
                
                // null for SenderAddress and SenderName indicates that the general email setting
                // values should be used.
                SenderAddress = null;
                SenderName = null;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32257", "Unable to clear message.");
            }
        }

        /// <summary>
        /// Shows the email message in the local client mail application (if one is configured).
        /// </summary>
        /// <param name="zipAttachments">If <see langword="true"/> then any file attachments
        /// should be zipped.</param>
        public void ShowInClient(bool zipAttachments)
        {
            try
            {
                using (var tempBody = new TemporaryFile(true))
                {
                    var arguments = new List<string>();
                    arguments.Add("\"" + string.Join(";", _recipients) + "\"");
                    arguments.Add("/client");
                    if (zipAttachments)
                    {
                        arguments.Add("/z");
                    }
                    if (!string.IsNullOrWhiteSpace(Subject))
                    {
                        arguments.Add("/subject");
                        arguments.Add(Subject.Quote());
                    }
                    if (!string.IsNullOrWhiteSpace(Body))
                    {
                        File.WriteAllText(tempBody.FileName, Body);
                        arguments.Add("/body");
                        arguments.Add(tempBody.FileName.Quote());
                    }

                    // Currently this only supports adding the first attachment
                    if (_fileAttachements.Count > 0)
                    {
                        arguments.Add("\"" + _fileAttachements[0] + "\"");
                    }

                    SystemMethods.RunExtractExecutable(_EMAIL_FILE_EXE, arguments);
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32290", "Unable to open message in email client.");
            }
        }

        /// <summary>
        /// Allows the adding of a single recipient to the recipients list.
        /// </summary>
        /// <param name="recipient">The recipient to add to the list.</param>
        public void AddRecipient(string recipient)
        {
            try
            {
                // Add the recipient if they are not in the list already
                if (!_recipients
                    .Any(s => s.Equals(recipient, StringComparison.OrdinalIgnoreCase)))
                {
                    _recipients.Add(recipient);
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32460", "Unable to add single recipient.");
            }
        }

        /// <summary>
        /// Allows the adding of a single recipient to the recipients list.
        /// </summary>
        /// <param name="carbonCopyRecipient">The carbon copy recipient to add to the list.
        /// </param>
        public void AddCarbonCopyRecipient(string carbonCopyRecipient)
        {
            try
            {
                // Add the recipient if they are not in the list already
                if (!_recipients
                    .Any(s => s.Equals(carbonCopyRecipient, StringComparison.OrdinalIgnoreCase)))
                {
                    _carbonCopyRecipients.Add(carbonCopyRecipient);
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI35994", "Unable to add carbon copy recipient.");
            }
        }

        /// <summary>
        /// Sets the email settings.
        /// </summary>
        /// <value>
        /// The email settings.
        /// </value>
        public ISmtpEmailSettings EmailSettings { get; set; }

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
        /// Gets or sets the attachments for the message.
        /// </summary>
        /// <value>
        /// The attachments.
        /// </value>
        [CLSCompliant(false)]
        public VariantVector Attachments
        {
            get
            {
                try
                {
                    var vector = new VariantVector();
                    foreach (var fileName in _fileAttachements)
                    {
                        vector.PushBack(fileName);
                    }

                    return vector;
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32258",
                        "Unable to get list of attachements.");
                }
            }
            set
            {
                try
                {
                    var size = value.Size;
                    _fileAttachements = new List<string>(size);
                    for (int i = 0; i < size; i++)
                    {
                        _fileAttachements.Add(value[i].ToString());
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32259",
                        "Unable to set list of attachments.");
                }
            }
        }

        /// <summary>
        /// Gets or sets the recipients of the message.
        /// </summary>
        /// <value>
        /// The recipients.
        /// </value>
        [CLSCompliant(false)]
        public VariantVector Recipients
        {
            get
            {
                try
                {
                    var vector = new VariantVector();
                    foreach (var recipient in _recipients)
                    {
                        vector.PushBack(recipient);
                    }

                    return vector;
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32260",
                        "Unable to get list of recipients.");
                }
            }
            set
            {
                try
                {
                    var size = value.Size;
                    _recipients = new List<string>(size);
                    for (int i = 0; i < size; i++)
                    {
                       _recipients.Add(value[i].ToString());
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32261",
                        "Unable to set list of recipients.");
                }
            }
        }

        /// <summary>
        /// Gets or sets the carbon copy recipients of the message.
        /// </summary>
        /// <value>
        /// The carbon copy recipients.
        /// </value>
        [CLSCompliant(false)]
        public VariantVector CarbonCopyRecipients
        {
            get
            {
                try
                {
                    var vector = new VariantVector();
                    foreach (var recipient in _carbonCopyRecipients)
                    {
                        vector.PushBack(recipient);
                    }

                    return vector;
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI35995",
                        "Unable to get list of carbon copy recipients.");
                }
            }
            set
            {
                try
                {
                    var size = value.Size;
                    _carbonCopyRecipients = new List<string>(size);
                    for (int i = 0; i < size; i++)
                    {
                        _carbonCopyRecipients.Add(value[i].ToString());
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI35996",
                        "Unable to set list of carbon copy recipients.");
                }
            }
        }

        /// <summary>
        /// Gets or sets the sender address.
        /// </summary>
        /// <value>
        /// The sender address to use or <see langword="null"/> to use the value from the general
        /// Extract email settings.
        /// </value>
        public string SenderAddress { get; set; }

        /// <summary>
        /// Gets or sets the name of the sender.
        /// </summary>
        /// <value>
        /// The name of the sender or <see langword="null"/> to use the value from the general
        /// Extract email settings.
        /// </value>
        public string SenderName { get; set; }

        #endregion

        #region Methods

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

        /// <summary>
        /// Adds the file attachments to the message attachment collection.
        /// </summary>
        /// <param name="attachmentCollection">The attachment collection.</param>
        void AddAttachments(AttachmentCollection attachmentCollection)
        {
            foreach (var fileName in _fileAttachements)
            {
                try
                {
                    // Create new attachment
                    var data = new Attachment(fileName);

                    // Add the file information
                    var info = new FileInfo(fileName);
                    var disposition = data.ContentDisposition;
                    disposition.CreationDate = info.CreationTime;
                    disposition.ModificationDate = info.LastWriteTime;
                    disposition.ReadDate = info.LastAccessTime;

                    attachmentCollection.Add(data);
                }
                catch (Exception ex)
                {
                    var ee = ex.AsExtract("ELI32262");
                    ee.AddDebugData("File Name", fileName, false);
                    throw ee;
                }
            }
        }

        #endregion Methods
    }
}
