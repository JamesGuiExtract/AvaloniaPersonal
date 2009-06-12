using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using SourceSafeTypeLib;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Win32;

namespace VssEvents 
{
    // Extract Systems Visual SourceSafe Event Handler
    [ComVisible(true)]
    [Guid("68DD9B5C-CEB4-4cd2-ACBE-95EF7ED8DDBD")]
    [ProgId("Extract.VssEventHandler")]
    public class ExtractVssEventHandler : IVSSEventHandler, IVSSEvents 
    {
        IConnectionPoint vssEvents;
        int cookie;

        XmlDocument checkinsXml;
        XmlNode checkinNode;
        XmlNode checkinFilesNode;
        XmlNode checkinCommentNode;
        XmlNode checkinReviewersNode;

        public ExtractVssEventHandler() 
        {
            // create a new xml document object
            checkinsXml = new XmlDocument();
            
            // set up the structure of for a new checkin node
            checkinNode = checkinsXml.CreateElement("Checkin");
            checkinFilesNode = checkinNode.AppendChild(checkinsXml.CreateElement("Files"));
            checkinCommentNode = checkinNode.AppendChild(checkinsXml.CreateElement("Comment"));
            checkinReviewersNode = checkinNode.AppendChild(checkinsXml.CreateElement("Reviewers"));
        }

        public void Init(VSSApp app) 
        {
            // set up the COM connection
            IConnectionPointContainer cpc = (IConnectionPointContainer) app;
            Guid guid = typeof(IVSSEvents).GUID;
            cpc.FindConnectionPoint(ref guid, out vssEvents);
            vssEvents.Advise(this, out cookie);
        }

        public bool BeforeAdd(VSSItem vssItem, string localSpec, string comment)
        {
            // do nothing

            return true;
        }

        public void AfterAdd(VSSItem vssItem, string localSpec, string comment) 
        {
            AppendToCheckinsXML(localSpec, comment);
        }

        public bool BeforeBranch(VSSItem vssItem, string comment)
        {
            // do nothing

            return true;
        }

        public void AfterBranch(VSSItem vssItem, string comment) 
        {
            // do nothing
        }

        public bool BeforeCheckin(VSSItem vssItem, string localSpec, string comment)
        {
            // do nothing

            return true;
        }

        public void AfterCheckin(VSSItem vssItem, string localSpec, string comment)
        {
            AppendToCheckinsXML(localSpec, comment);
        }

        public bool BeforeCheckout(VSSItem vssItem, string localSpec, string comment)
        {
            // do nothing

            return true;
        }

        public void AfterCheckout(VSSItem vssItem, string localSpec, string comment)
        {
            // do nothing
        }

        public bool BeginCommand(int unused)
        {
            // do nothing
            return true;
        }

        public void EndCommand(int unused)
        {
            // check if a checkin comment has been processed
            if (checkinCommentNode.HasChildNodes)
            {             
                // check if the checkins xml file already exists
                string checkinsXmlFile = System.IO.Path.Combine(GetSccServerDir(), "checkins.xml");
                if (System.IO.File.Exists(checkinsXmlFile))
                {
                    // load the existing xml file
                    checkinsXml.Load(checkinsXmlFile);
                }
                else
                {
                    // create a new xml file
                    checkinsXml.RemoveAll();

                    checkinsXml.AppendChild(checkinsXml.CreateXmlDeclaration("1.0", "UTF-8", null));

                    checkinsXml.AppendChild(checkinsXml.CreateElement("Checkins"));
                }

                // add the new checkin node and save the xml file
                checkinsXml.LastChild.AppendChild(checkinNode);
                checkinsXml.Save(checkinsXmlFile);

                // create a new checkin node
                checkinNode = checkinsXml.CreateElement("Checkin");
                checkinFilesNode = checkinNode.AppendChild(checkinsXml.CreateElement("Files"));
                checkinCommentNode = checkinNode.AppendChild(checkinsXml.CreateElement("Comment"));
                checkinReviewersNode = checkinNode.AppendChild(checkinsXml.CreateElement("Reviewers"));
            }
        }

        public bool BeforeRename(VSSItem vssItem, string oldName)
        {
            // do nothing

            return true;
        }

        public void AfterRename(VSSItem vssItem, string oldName) 
        {
            // do nothing
        }

        public bool BeforeUndoCheckout(VSSItem vssItem, string localSpec)
        {
            // do nothing
            return true;
        }

        public void AfterUndoCheckout(VSSItem vssItem, string localSpec) 
        {
            // do nothing
        }

        public bool BeforeEvent(int eventNum, VSSItem vssItem, string str, object var)
        {
            // not currently used, defined for future expansion
            return true;
        }

        public void AfterEvent(int eventNum, VSSItem vssItem, string str, object var)
        {
            // not currently used, defined for future expansion
        }

        private void AppendToCheckinsXML(string localSpec, string comment)
        {
            // skip this checkin command if there is no comment associated with it
            if (comment.Length == 0)
            {
                return;
            }

            // add node for this file name
            XmlNode fileNode = checkinsXml.CreateElement("File");
            fileNode.AppendChild(checkinsXml.CreateTextNode(localSpec));
            checkinFilesNode.AppendChild(fileNode);

            // skip this checkin command if it has already been processed
            if (checkinCommentNode.HasChildNodes)
            {
                return;
            }

            // split the comment into two parts:
            // 1) checkin description
            // 2) reviewer information
            Regex splitReviewFromComment = new Regex("Reviewed by:?",
                RegexOptions.RightToLeft & RegexOptions.IgnoreCase);
            string[] commentParts = splitReviewFromComment.Split(comment, 2);

            checkinCommentNode.AppendChild(checkinsXml.CreateTextNode(commentParts[0]));

            // get the reviewer names from the second part
            string[] reviewers = Regex.Split(commentParts[1], "(?:\\s+|[,&]|and)+");

            foreach (string reviewer in reviewers)
            {
                if (reviewer.Length > 0)
                {
                    XmlNode reviewerNode = checkinsXml.CreateElement("Reviewer");
                    reviewerNode.AppendChild(checkinsXml.CreateTextNode(reviewer.Trim('.')));
                    checkinReviewersNode.AppendChild(reviewerNode);
                }
            }
        }

        private string GetSccServerDir()
        {
            // get the Visual SourceSafe (tm) registry key
            RegistryKey vssKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\SourceSafe");

            // get the full path to SSSCC.DLL
            string sccServerPath = vssKey.GetValue("SCCServerPath").ToString();
            
            // return the directory of the SourceSafe server
            return System.IO.Path.GetDirectoryName(sccServerPath);
        }
    }
}