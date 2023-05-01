using MimeKit;
using MimeKit.Text;
using MimeKit.Tnef;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Extract.FileConverter.ConvertToPdf
{
    /// <summary>
    /// Visits a MimeMessage and generates HTML suitable for converting to PDF
    /// Modified version of MimeKit example code: http://www.mimekit.net/docs/html/T_MimeKit_Text_HtmlTagCallback.htm
    /// </summary>
    internal class MimeKitHtmlMessageVisitor : MimeVisitor
    {
        readonly List<MultipartRelated> _stack = new();
        readonly List<MimeEntity> _attachments = new();
        readonly string _header;
        string _body;
        Encoding _encoding = Encoding.Default;

        /// <summary>
        /// Creates a new MimeKitHtmlMessageVisitor
        /// </summary>
        public MimeKitHtmlMessageVisitor(string header = null)
        {
            _header = header;
        }

        /// <summary>
        /// The list of attachments that were in the MimeMessage.
        /// </summary>
        public IList<MimeEntity> Attachments => _attachments;

        /// <summary>
        /// HTML version of the body
        /// </summary>
        public string HtmlBody => _body ?? string.Empty;

        /// <summary>
        /// The character encoding of the body text
        /// </summary>
        public Encoding Encoding => _encoding;

        protected override void VisitMultipartAlternative(MultipartAlternative alternative)
        {
            // walk the multipart/alternative children backwards from greatest level of faithfulness to the least faithful
            for (int i = alternative.Count - 1; i >= 0 && _body == null; i--)
            {
                alternative[i].Accept(this);
            }
        }

        protected override void VisitMultipartRelated(MultipartRelated related)
        {
            var root = related.Root;

            // push this multipart/related onto our stack
            _stack.Add(related);

            // visit the root document
            root.Accept(this);

            // pop this multipart/related off our stack
            _stack.RemoveAt(_stack.Count - 1);
        }

        // look up the image based on the img src url within our multipart/related stack
        bool TryGetImage(string url, out MimePart image)
        {
            UriKind kind;
            int index;
            Uri uri;

            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                kind = UriKind.Absolute;
            }
            else if (Uri.IsWellFormedUriString(url, UriKind.Relative))
            {
                kind = UriKind.Relative;
            }
            else
            {
                kind = UriKind.RelativeOrAbsolute;
            }

            try
            {
                uri = new Uri(url, kind);
            }
            catch
            {
                image = null;
                return false;
            }

            for (int i = _stack.Count - 1; i >= 0; i--)
            {
                if ((index = _stack[i].IndexOf(uri)) == -1)
                {
                    continue;
                }

                image = _stack[i][index] as MimePart;
                return image != null;
            }

            image = null;

            return false;
        }

        // Encode an image as a data uri
        static string GetDataUri(MimePart image)
        {
            using MemoryStream memory = new();
            image.Content.DecodeTo(memory);
            var buffer = memory.GetBuffer();
            var length = (int)memory.Length;
            var base64 = Convert.ToBase64String(buffer, 0, length);

            return string.Format(CultureInfo.InvariantCulture, "data:{0};base64,{1}", image.ContentType.MimeType, base64);
        }

        // Replaces <img src=...> urls that refer to images embedded within the message with "data:" URIs 
        void HtmlTagCallback(HtmlTagContext ctx, HtmlWriter htmlWriter)
        {
            if (ctx.TagId == HtmlTagId.Image && !ctx.IsEndTag && _stack.Count > 0)
            {
                ctx.WriteTag(htmlWriter, false);

                // replace the src attribute with a data: URI
                foreach (var attribute in ctx.Attributes)
                {
                    if (attribute.Id == HtmlAttributeId.Src)
                    {
                        if (!TryGetImage(attribute.Value, out MimePart image))
                        {
                            htmlWriter.WriteAttribute(attribute);
                            continue;
                        }

                        string uri = GetDataUri(image);

                        htmlWriter.WriteAttributeName(attribute.Name);
                        htmlWriter.WriteAttributeValue(uri);
                    }
                    else
                    {
                        htmlWriter.WriteAttribute(attribute);
                    }
                }
            }
            else
            {
                // pass the tag through to the output
                ctx.WriteTag(htmlWriter, true);
            }
        }

        protected override void VisitTextPart(TextPart entity)
        {
            TextConverter converter;

            if (_body != null)
            {
                // since we've already found the body, treat this as an attachment
                _attachments.Add(entity);
                return;
            }

            if (entity.IsHtml)
            {
                converter = new HtmlToHtml
                {
                    FilterHtml = true,
                    HtmlTagCallback = HtmlTagCallback,
                    HeaderFormat = HeaderFooterFormat.Html
                };
            }
            else if (entity.IsFlowed)
            {
                var flowed = new FlowedToHtml
                {
                    HeaderFormat = HeaderFooterFormat.Html
                };

                if (entity.ContentType.Parameters.TryGetValue("delsp", out string delsp))
                {
                    flowed.DeleteSpace = delsp.Equals("yes", StringComparison.OrdinalIgnoreCase);
                }

                converter = flowed;
            }
            else
            {
                converter = new TextToHtml
                {
                    HeaderFormat = HeaderFooterFormat.Html
                };
            }

            if (!string.IsNullOrEmpty(_header))
            {
                converter.Header = _header;
            }

            _body = converter.Convert(entity.GetText(out _encoding));
        }

        protected override void VisitTnefPart(TnefPart entity)
        {
            // extract any attachments in the MS-TNEF part
            _attachments.AddRange(entity.ExtractAttachments());
        }

        protected override void VisitMessagePart(MessagePart entity)
        {
            // treat message/rfc822 parts as attachments
            _attachments.Add(entity);
        }

        protected override void VisitMimePart(MimePart entity)
        {
            // realistically, if we've gotten this far, then we can treat this as an attachment
            // even if the IsAttachment property is false.
            _attachments.Add(entity);
        }
    }
}
