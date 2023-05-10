using System;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;
using static ExtractDataExplorer.Utils;

namespace ExtractDataExplorer.Services
{
    public class ExpandPathTagsService : IExpandPathTagsService
    {
        readonly IAFUtility _afutil;

        public ExpandPathTagsService(IAFUtilityFactory afutilityFactory)
        {
            _ = afutilityFactory ?? throw new ArgumentNullException(nameof(afutilityFactory));

            _afutil = afutilityFactory.Create();
        }

        /// <summary>
        /// Trim and expand <SourceDocName>, $DirOf(), etc
        /// </summary>
        public string? ExpandPathTags(string? sourceDocName, string? pathToExpand)
        {
            if (TrimPath(pathToExpand) is string trimmedPath)
            {
                AFDocumentClass doc = new();
                doc.Text.SourceDocName = sourceDocName ?? "";

                try
                {
                    return TrimPath(_afutil.ExpandTagsAndFunctions(trimmedPath, doc));
                }
                // Path tag function is probably malformed if there is an exception, return the trimmed path
                catch
                {
                    return trimmedPath;
                }
            }

            return null;
        }
    }
}
