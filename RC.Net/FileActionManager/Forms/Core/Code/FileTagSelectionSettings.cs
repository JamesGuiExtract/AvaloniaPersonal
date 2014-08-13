using Extract.Interop;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Forms
{
    /// <summary>
    /// Represents settings regarding which tags should be available for use for a given object or
    /// situation.
    /// </summary>
    public class FileTagSelectionSettings
    {
        #region Constants

        /// <summary>
        /// The current version number for this object
        /// </summary>
        const int _CURRENT_VERSION = 1;

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTagSelectionSettings"/> class.
        /// </summary>
        public FileTagSelectionSettings()
            : this(true, false, new string[0], false, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTagSelectionSettings"/> class.
        /// </summary>
        /// <param name="source">The <see cref="FileTagSelectionSettings"/> instance to be used to
        /// initialize the settings.</param>
        public FileTagSelectionSettings(FileTagSelectionSettings source)
            : this(source.UseAllTags, source.UseSelectedTags, source.SelectedTags,
                source.UseTagFilter, source.TagFilter)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTagSelectionSettings"/> class.
        /// </summary>
        /// <param name="useAllTags">Indicates whether all tags should be available for selection.
        /// </param>
        /// <param name="useSelectedTags">Indicates whether specific tags are available for selection.
        /// </param>
        /// <param name="selectedTags">The specific tags that should be available for selection.
        /// </param>
        /// <param name="useTagFilter">Indicates whether tags matching <see paramref="tagFilter"/>
        /// should be available for selection.</param>
        /// <param name="tagFilter">A wildcard filter dictating which files should be available for
        /// selection.</param>
        [CLSCompliant(false)]
        public FileTagSelectionSettings(bool useAllTags, bool useSelectedTags,
            IEnumerable<string> selectedTags, bool useTagFilter, string tagFilter)
        {
            try
            {
                ExtractException.Assert("ELI37228", "Invalid tag selection settings; cannot combine " +
                    "'use all tags' option with any other option.",
                    !UseAllTags || (!UseSelectedTags && !UseTagFilter));

                UseAllTags = useAllTags;
                UseSelectedTags = useSelectedTags;
                SelectedTags = new ReadOnlyCollection<string>(selectedTags.ToList());
                UseTagFilter = useTagFilter;
                TagFilter = tagFilter ?? "";
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37229");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets a value indicating whether all tags should be available for selection.
        /// </summary>
        /// <value><see langword="true"/> if all tags should be available for selection; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool UseAllTags
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether specific tags are available for selection.
        /// </summary>
        /// <value><see langword="true"/> if specific tags are available for selection; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool UseSelectedTags
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the specific tags that should be available for selection.
        /// </summary>
        /// <value>
        /// The specific tags that should be be available for selection.
        /// </value>
        public ReadOnlyCollection<string> SelectedTags
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether tags matching <see cref="TagFilter"/> should be
        /// available for selection.
        /// </summary>
        /// <value><see langword="true"/> if tags matching <see cref="TagFilter"/> should be
        /// available for selectio; otherwise, <see langword="false"/>.
        /// </value>
        public bool UseTagFilter
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a wildcard filter dictating which files should be available for selection.
        /// </summary>
        /// <value>
        /// The wildcard filter dictating which files should be available for selection.
        /// </value>
        public string TagFilter
        {
            get;
            private set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Creates a <see cref="FileTagSelectionSettings"/> from the specified 
        /// <see cref="IStreamReader"/>.
        /// </summary>
        /// <param name="reader">The reader from which to create the 
        /// <see cref="FileTagSelectionSettings"/>.</param>
        /// <returns>A <see cref="FileTagSelectionSettings"/> created from the specified
        /// <see cref="IStreamReader"/>.</returns>
        public static FileTagSelectionSettings ReadFrom(IStreamReader reader)
        {
            try
            {
                int version = reader.ReadInt32();
                if (version > _CURRENT_VERSION)
                {
                    ExtractException ee = new ExtractException("ELI37230",
                        "Cannot load newer version of FileTagSelectionSettings");
                    ee.AddDebugData("Current Version", _CURRENT_VERSION, false);
                    ee.AddDebugData("Version To Load", version, false);
                    throw ee;
                }

                bool useAllTags = reader.ReadBoolean();
                bool useSelectedTags = reader.ReadBoolean();
                string[] selectedTags = reader.ReadStringArray();
                bool useTagFilter = reader.ReadBoolean();
                string tagFilter = reader.ReadString();

                return new FileTagSelectionSettings(useAllTags, useSelectedTags, selectedTags,
                    useTagFilter, tagFilter);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI37231", "Unable to read tag selection settings.", ex);
            }
        }

        /// <summary>
        /// Writes the <see cref="FileTagSelectionSettings"/> to the specified 
        /// <see cref="IStreamWriter"/>.
        /// </summary>
        /// <param name="writer">The writer into which the 
        /// <see cref="FileTagSelectionSettings"/> will be written.</param>
        public void WriteTo(IStreamWriter writer)
        {
            try
            {
                writer.Write(_CURRENT_VERSION);
                writer.Write(UseAllTags);
                writer.Write(UseSelectedTags);
                writer.Write(SelectedTags.ToArray());
                writer.Write(UseTagFilter);
                writer.Write(TagFilter);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI37232", "Unable to write tag selection settings.", ex);
            }
        }

        /// <overloads>
        /// Gets the tags qualified for availability under the current settings.
        /// </overloads>
        /// <summary>
        /// Gets the tags qualified for availability from the specified
        /// <see paramref="fileProcessingDB"/>.
        /// </summary>
        /// <param name="fileProcessingDB">The <see cref="IFileProcessingDB"/> containing the tags.
        /// </param>
        /// <returns>The tags from <see paramref="fileProcessingDB"/> that qualify under the current
        /// settings.</returns>
        [CLSCompliant(false)]
        public IEnumerable<string> GetQualifiedTags(IFileProcessingDB fileProcessingDB)
        {
            try
            {
                var tagNames = fileProcessingDB.GetTagNames().ToIEnumerable<string>();

                return GetQualifiedTags(tagNames);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37233");
            }
        }

        /// <summary>
        /// Gets the tags qualified for availability from the specified
        /// <see paramref="availableTags"/>.
        /// </summary>
        /// <param name="availableTags">The overall set of tags to be qualified.</param>
        /// <returns>The <see cref="FileTag"/>s from <see paramref="availableTags"/> that qualify
        /// under the current settings.</returns>
        public IEnumerable<FileTag> GetQualifiedTags(IEnumerable<FileTag> availableTags)
        {
            try
            {
                var matchingTags = GetQualifiedTags(availableTags.Select(tag => tag.Name));

                return availableTags.Where(tag => matchingTags.Contains(tag.Name));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37234");
            }
        }

        /// <summary>
        /// Gets the tags qualified for availability from the specified
        /// <see paramref="availableTags"/>.
        /// </summary>
        /// <param name="availableTags">The overall set of tags to be qualified.</param>
        /// <returns>The tags from <see paramref="availableTags"/> that qualify under the current
        /// settings.</returns>
        public IEnumerable<string> GetQualifiedTags(IEnumerable<string> availableTags)
        {
            try
            {
                if (UseAllTags)
                {
                    return availableTags;
                }

                var matchingTags = new List<IEnumerable<string>>();

                if (UseTagFilter)
                {
                    matchingTags.Add(GetTagsMatchingFilter(availableTags));
                }

                if (UseSelectedTags)
                {
                    matchingTags.Add(
                        availableTags.Where(tagName => SelectedTags.Contains(tagName)));
                }

                return matchingTags
                    .SelectMany(tags => tags)
                    .Distinct();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37235");
            }
        }

        /// <summary>
        /// Gets the tags that match the specified <see cref="TagFilter"/> pattern(s) from
        /// <see paramref="availableTags"/>.
        /// </summary>
        /// <param name="availableTags">The overall set of tags to be tested.</param>
        /// <returns>The tags that match the specified <see cref="TagFilter"/> pattern(s).</returns>
        public IEnumerable<string> GetTagsMatchingFilter(IEnumerable<string> availableTags)
        {
            try
            {
                string[] filters =
                    TagFilter.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

                var regExes = new List<string>();
                foreach (string filter in filters)
                {
                    StringBuilder regEx = new StringBuilder(filter);

                    Dictionary<int, string> replacementIndices = Regex.Matches(filter, "[?]")
                        .OfType<Match>()
                        .ToDictionary(match => match.Index, match => @"[\s\S]");
                    replacementIndices = replacementIndices.Concat(Regex.Matches(filter, "[*]")
                        .OfType<Match>()
                        .ToDictionary(match => match.Index, match => @"[\s\S]*"))
                            .ToDictionary(entry => entry.Key, entry => entry.Value);

                    // Remove wildcards in reverse order so that each index used is still valid in
                    // regEx.
                    foreach (var replacement in replacementIndices
                        .OrderBy(replacement => replacement.Key)
                        .Reverse())
                    {
                        regEx.Remove(replacement.Key, 1);
                    }

                    regEx = new StringBuilder(Regex.Escape(regEx.ToString()));

                    int indexAdjustment = 0;
                    foreach (var replacement in replacementIndices
                        .OrderBy(replacement => replacement.Key))
                    {
                        regEx.Insert(replacement.Key + indexAdjustment, replacement.Value);

                        // Account for extra length of replacement vs the original char for all
                        // subsequent replacements.
                        indexAdjustment += replacement.Value.Length - 1;
                    }

                    regExes.Add(regEx.ToString());
                }

                string finalRegEx = "^" + string.Join("|", regExes) + "$";

                return availableTags.Where(tagName => Regex.IsMatch(tagName, finalRegEx));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37236");
            }
        }

        #endregion Methods
    }
}
