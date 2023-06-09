﻿using Microsoft.Graph;
using System.Collections.Generic;
using System.IO;

namespace Extract.Email.GraphClient.Test.Utilities
{
    public class EmailService
    {
        MessageAttachmentsCollectionPage MessageAttachmentsCollectionPage
            = new();

        public Message CreateStandardEmail(string recipient, string header, string body)
        {
            var message = new Message
            {
                Subject = header,
                Body = new ItemBody
                {
                    ContentType = BodyType.Text,
                    Content = body
                },
                ToRecipients = new List<Recipient>()
            {
                new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = recipient
                    }
                },
                new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = "Test_" + recipient
                    }
                }
            },
                Attachments = MessageAttachmentsCollectionPage
            };

            return message;
        }

        public Message CreateHtmlEmail(string recipient, string header, string body)
        {
            var message = new Message
            {
                Subject = header,
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = body
                },
                ToRecipients = new List<Recipient>()
            {
                new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = recipient
                    }
                }
            },
                Attachments = MessageAttachmentsCollectionPage
            };

            return message;
        }

        public void AddAttachment(string filePath)
        {
            byte[] data = System.IO.File.ReadAllBytes(filePath);
            MessageAttachmentsCollectionPage.Add(new FileAttachment
            {
                Name = Path.GetFileName(filePath),
                ContentBytes = data
            });
        }

        public void ClearAttachments()
        {
            MessageAttachmentsCollectionPage.Clear();
        }
    }
}
