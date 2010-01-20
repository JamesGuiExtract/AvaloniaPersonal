extern alias SC;

using SC::Extract.SourceControl;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SourceControl
{
    /// <summary>
    /// Represents a mutable get message.
    /// </summary>
    public class GetMessageBuilder
    {
        #region Fields

        /// <summary>
        /// Renamed, moved, or deleted files or folders that may be deleted locally.
        /// </summary>
        readonly List<string> _deleted = new List<string>();

        /// <summary>
        /// The modified directories.
        /// </summary>
        readonly DisplayDirectoryBuilder _modified;

        /// <summary>
        /// The comments associated with the modifications.
        /// </summary>
        readonly List<string> _comments = new List<string>();

        /// <summary>
        /// The reviewers who reviewed by code changes.
        /// </summary>
        readonly List<string> _reviewers = new List<string>();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GetMessageBuilder"/> class.
        /// </summary>
        public GetMessageBuilder(string rootDirectory)
        {
            _modified = new DisplayDirectoryBuilder(rootDirectory);
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Adds the specified history item to the get message.
        /// </summary>
        /// <param name="item">The history item to add to the get message.</param>
        public void Add(IHistoryItem item)
        {
            // Add deleted files
            if (item.LocalFileDeleted)
            {
                string fileName = item.FormerRepositoryPath;
                fileName = fileName.Replace("/", @"\");
                AddIfUnique(_deleted, fileName);
            }

            // Add directory
            _modified.AddRepositoryPath(item.RepositoryPath);

            // Add comment (description and reviewer)
            AddComment(item.Comment);
        }

        void AddComment(string comment)
        {
            // If the comment is blank, we are done
            if (string.IsNullOrEmpty(comment))
            {
                return;
            }

            // Split the comment into two parts:
            // 1) Change description
            // 2) Reviewer names
            Regex splitReviewFromComment = new Regex(" *Reviewed by:? *",
                RegexOptions.RightToLeft & RegexOptions.IgnoreCase);
            string[] commentParts = splitReviewFromComment.Split(comment, 2);

            // Add the description if it is not already added
            AddIfUnique(_comments, commentParts[0]);

            // Get the reviewer names from the second part
            if (commentParts.Length > 1)
            {
                string[] reviewers = Regex.Split(commentParts[1], "(?:\\s+|[,&.]|and)+");
                foreach (string reviewer in reviewers)
                {
                    AddIfUnique(_reviewers, reviewer);
                }
            }
        }

        static void AddIfUnique(List<string> list, string value)
        {
            if (value.Length > 0)
            {
                if (!list.Contains(value))
                {
                    list.Add(value);
                }
            }
        }

        #endregion Methods

        #region Overrides

        /// <summary>
        /// Converts the value of this instance to a <see cref="String"/>.
        /// </summary>
        /// <returns>A string whose value is the same as this instance.</returns>
        public override string ToString()
        {
            StringBuilder message = new StringBuilder();

            // Append deleted files
            if (_deleted.Count > 0)
            {
                _deleted.Sort(StringComparer.OrdinalIgnoreCase);
                message.AppendLine("You may delete:");
                foreach (string file in _deleted)
                {
                    message.AppendLine(file);
                }

                if (_modified.Count > 0)
                {
                    message.AppendLine();
                }
            }

            // Append directories that were modified
            foreach (string directory in _modified.GetDisplayDirectories())
            {
                message.AppendLine(directory);
            }

            // Append comments
            if (_comments.Count > 0)
            {
                message.AppendLine();
                foreach (string comment in _comments)
                {
                    message.Append("- ");
                    message.AppendLine(comment);
                }
            }

            // Append reviewers
            if (_reviewers.Count > 0)
            {
                message.AppendLine();
                message.AppendLine("Reviewed by:");
                message.AppendLine();
                foreach (string reviewer in _reviewers)
                {
                    message.AppendLine(reviewer);
                }
            }

            return message.ToString();
        }

        #endregion Overrides
    }
}




