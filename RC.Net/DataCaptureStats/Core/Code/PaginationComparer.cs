using Extract.AttributeFinder;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

namespace Extract.DataCaptureStats
{
    /// <summary>
    /// Class to handle comparing attribute trees (VOAs)
    /// </summary>
    public static class PaginationComparer
    {
        /// <summary>
        /// Compares one set of attributes with another. The per-file, map function of the design
        /// </summary>
        /// <param name="ignoreXPath">The XPath to select attributes to ignore.</param>
        /// <param name="containerXPath">The XPath to select attributes that will be considered as containers only</param>
        /// <remarks><see paramref="found"/> and <see paramref="expected"/> hierarchies may be modified
        /// by this method.</remarks>
        /// <param name="collectMatchData">Whether to collect each correct/incorrect/missed attribute in addition to counts</param>
        /// <param name="cancelToken">CancellationToken to allow cancellation of comparison</param>
        public static IEnumerable<AccuracyDetail> CompareAttributes(IUnknownVector expected, IUnknownVector found,
            CompareAttributesFunc compareDocumentData,
            IncludeFoundDocumentFunc includeFoundDocument = null,
            DocumentsMatchFunc compareDocuments = null,
            IEnumerable<string> requiredForDocumentMatchPaths = null,
            CancellationToken cancelToken = default(CancellationToken))
        {
            try
            {
                compareDocumentData = compareDocumentData ?? ((e, f) => AttributeTreeComparer.CompareAttributes(e, f, collectMatchData: false, cancelToken: cancelToken));
                includeFoundDocument = includeFoundDocument ?? DefaultIncludeFoundDocument;
                compareDocuments = compareDocuments ?? DefaultCompareDocuments;
                var requiredPaths = new HashSet<string>(requiredForDocumentMatchPaths);

                string DocumentRequiredFields = SpecialAttributeNames.Document + " (all required data correct)";
                const string AutoPaginationDocumentDataStatsType = "Pagination,  auto DocumentData";
                const string UserPaginationDocumentDataStatsType = "Pagination,  user (ceiling) DocumentData";
                const string TotalPaginationDocumentDataStatsType = "Pagination, total DocumentData (auto + user ceiling)";

                List<IAttribute> foundDocuments = GetDocuments(found);
                List<IAttribute> expectedDocuments = GetDocuments(expected);

                // Limit found to cases that pass the include test
                List<IAttribute> autoFound =
                    foundDocuments
                    .Where(doc => includeFoundDocument(doc))
                    .ToList();

                // Limit expected to cases that match one of the included found and pair up
                List<(IAttribute expected, IAttribute found)> autoPaginationDocumentDataExpectedToFound =
                    GetAutoPaginationDocumentDataExpectedToFound(expectedDocuments, autoFound, compareDocuments);

                List<IAttribute> autoExpected =
                    autoPaginationDocumentDataExpectedToFound
                    .Select(pair => pair.expected)
                    .Where(x => x != null) // Not all found have an expected
                    .ToList();

                List<IAttribute> userExpected = expectedDocuments.Except(autoExpected).ToList();
                List<IAttribute> userFound = foundDocuments.Except(autoFound).ToList();

                // Compute pagination-only accuracy (NotDeletedPages and DeletedPages stats)
                List<AccuracyDetail> results = ComparePaginationOnlyData(autoExpected, autoFound, userExpected, userFound, cancelToken);

                // Compute auto document data accuracies

                // Always add a row for _total_ expected, even if count is zero
                results.Add(new AccuracyDetail(AccuracyDetailLabel.Expected, DocumentRequiredFields, expectedDocuments.Count, TotalPaginationDocumentDataStatsType));

                // Only add a row for _auto_ expected if there is something that made the cut (else file count will be off)
                if (autoExpected.Count > 0)
                {
                    results.Add(new AccuracyDetail(AccuracyDetailLabel.Expected, DocumentRequiredFields, autoExpected.Count, AutoPaginationDocumentDataStatsType));
                }

                // Only add a row for _user_ expected if there is something that made the cut (else file count will be off)
                if (userExpected.Count > 0)
                {
                    results.Add(new AccuracyDetail(AccuracyDetailLabel.Expected, DocumentRequiredFields, userExpected.Count, UserPaginationDocumentDataStatsType));
                }

                // If this document was not fully auto-paginated then a user be able to correct anything that's wrong so count as correct
                if (userFound.Count > 0)
                {
                    results.Add(new AccuracyDetail(AccuracyDetailLabel.Correct, DocumentRequiredFields, userExpected.Count, UserPaginationDocumentDataStatsType));
                    results.Add(new AccuracyDetail(AccuracyDetailLabel.Correct, DocumentRequiredFields, userExpected.Count, TotalPaginationDocumentDataStatsType));
                }

                // Actually compare the data for the pairs of auto documents
                foreach (var (expectedDocument, foundDocument) in autoPaginationDocumentDataExpectedToFound)
                {
                    // Compare the document data and label as auto-docdata
                    var expectedData = GetDocData(expectedDocument);
                    var foundData = GetDocData(foundDocument);
                    var docResults = compareDocumentData(expectedData, foundData).ToList();
                    results.AddRange(docResults.Select(detail => new AccuracyDetail(detail, AutoPaginationDocumentDataStatsType)));

                    // Compute whether this doc should be considered 'correct' or not
                    var resultsRequiredToBeCorrect =
                        docResults
                        .Where(detail => requiredPaths.Contains(detail.Path));

                    // AttributeTreeComparer doesn't output number-carrying Missed details but I think the IDS version does so check for both Incorrect and Missed
                    bool hasMissingOrIncorrect =
                        resultsRequiredToBeCorrect
                        .Any(detail =>
                            detail.Value > 0
                            && (detail.Label == AccuracyDetailLabel.Missed || detail.Label == AccuracyDetailLabel.Incorrect));

                    // Check for correct < expected to know if there are data-capture misses
                    var grouped = resultsRequiredToBeCorrect.ToLookup(detail => detail.Label);
                    bool hasLessCorrectThanExpected =
                        grouped[AccuracyDetailLabel.Correct].Sum(detail => detail.Value)
                        < grouped[AccuracyDetailLabel.Expected].Sum(detail => detail.Value);

                    bool exactMatch = !(hasMissingOrIncorrect || hasLessCorrectThanExpected);

                    if (exactMatch)
                    {
                        results.Add(new AccuracyDetail(AccuracyDetailLabel.Correct, DocumentRequiredFields, 1, AutoPaginationDocumentDataStatsType));
                        results.Add(new AccuracyDetail(AccuracyDetailLabel.Correct, DocumentRequiredFields, 1, TotalPaginationDocumentDataStatsType));
                    }
                    else
                    {
                        results.Add(new AccuracyDetail(AccuracyDetailLabel.Incorrect, DocumentRequiredFields, 1, AutoPaginationDocumentDataStatsType));
                        results.Add(new AccuracyDetail(AccuracyDetailLabel.Incorrect, DocumentRequiredFields, 1, TotalPaginationDocumentDataStatsType));
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49992");
            }
        }

        private static List<AccuracyDetail> ComparePaginationOnlyData(
            List<IAttribute> autoExpected,
            List<IAttribute> autoFound,
            List<IAttribute> userExpected,
            List<IAttribute> userFound,
            CancellationToken cancelToken)
        {
            const string AutoPaginationStatsType = "Pagination,  auto prediction accuracy";
            const string UserPaginationStatsType = "Pagination,  user prediction accuracy";
            const string TotalPaginationStatsType = "Pagination, total prediction accuracy (auto + user)";

            var results = new List<AccuracyDetail>();

            // Create Document/Page DeletedPage hierarchy for computing pagination-only stats
            IUnknownVector autoPaginationExpected = GetPaginationOnlyData(autoExpected);
            IUnknownVector userPaginationExpected = GetPaginationOnlyData(userExpected);

            IUnknownVector autoPaginationFound = GetPaginationOnlyData(autoFound);
            IUnknownVector userPaginationFound = GetPaginationOnlyData(userFound);

            // These document attributes only have Page and DeletedPage subattributes so we can use the default AttributeTreeComparer options
            IEnumerable<AccuracyDetail> comparePages(IUnknownVector e, IUnknownVector f) =>
                AttributeTreeComparer.CompareAttributes(e, f, cancelToken: cancelToken);

            var autoResults = comparePages(autoPaginationExpected, autoPaginationFound);
            results.AddRange(autoResults.Select(detail => new AccuracyDetail(detail, AutoPaginationStatsType)));
            results.AddRange(autoResults.Select(detail => new AccuracyDetail(detail, TotalPaginationStatsType)));

            var userResults = comparePages(userPaginationExpected, userPaginationFound);
            results.AddRange(userResults.Select(detail => new AccuracyDetail(detail, UserPaginationStatsType)));
            results.AddRange(userResults.Select(detail => new AccuracyDetail(detail, TotalPaginationStatsType)));

            return results;
        }

        private static List<(IAttribute expected, IAttribute found)> GetAutoPaginationDocumentDataExpectedToFound(
            List<IAttribute> expectedDocuments,
            List<IAttribute> autoFound,
            DocumentsMatchFunc compareDocuments)
        {
            List<int> deletedPagesFromAllExpected =
                expectedDocuments
                .SelectMany(doc => GetPages(doc, true))
                .ToList();

            var remainingAutoFound = new HashSet<IAttribute>(autoFound);
            return
                (   from expectedDocument in expectedDocuments
                    let matchingFound = remainingAutoFound.FirstOrDefault(f => compareDocuments(expectedDocument, f, deletedPagesFromAllExpected))
                    where matchingFound != null && remainingAutoFound.Remove(matchingFound)
                    select (expected: expectedDocument, found: matchingFound)
                )
                .Concat(remainingAutoFound.Select(leftoverFound => (expected: (IAttribute)null, found: leftoverFound)))
                .ToList();
        }

        private static List<IAttribute> GetDocuments(IUnknownVector attrr)
        {
            return attrr
                .ToIEnumerable<IAttribute>()
                .Where(a => a.Name == SpecialAttributeNames.Document)
                .ToList();
        }

        private static IUnknownVector GetDocData(IAttribute doc)
        {
            return doc
                ?.SubAttributes
                .ToIEnumerable<IAttribute>()
                .Where(a => a.Name == SpecialAttributeNames.DocumentData)
                .FirstOrDefault()
                ?.SubAttributes
                ?? new IUnknownVectorClass();
        }

        private static IEnumerable<int> GetPages(IAttribute doc, bool deleted)
        {
            var name = deleted ? "DeletedPages" : "Pages";
            var pagesString = doc.SubAttributes
                .ToIEnumerable<IAttribute>()
                .FirstOrDefault(a => a.Name == name)
                ?.Value.String ?? "";
            return UtilityMethods.GetPageNumbersFromString(pagesString, -1, false);
        }

        private static IUnknownVector GetPaginationOnlyData(IEnumerable<IAttribute> documents)
        {
            return documents
                .Select(doc => GetDocumentWithPageSubattributes(doc))
                .ToIUnknownVector();
        }

        private static IAttribute GetDocumentWithPageSubattributes(IAttribute doc)
        {
            var deletedPages = new HashSet<int>(GetPages(doc, true));
            var notDeletedPages = new HashSet<int>(GetPages(doc, false).Except(deletedPages));

            var creator = new AttributeCreator("dummy");
            var newDoc = creator.Create(SpecialAttributeNames.Document);
            newDoc.SubAttributes.Append(
                notDeletedPages
                .Select(page => creator.Create("NotDeletedPage", page))
                .ToIUnknownVector());
            newDoc.SubAttributes.Append(
                deletedPages
                .Select(page => creator.Create("DeletedPage", page))
                .ToIUnknownVector());

            return newDoc;
        }

        private static bool DefaultIncludeFoundDocument(IAttribute document)
        {
            return document
                .SubAttributes
                .ToIEnumerable<IAttribute>()
                .FirstOrDefault(a =>
                    a.Name == SpecialAttributeNames.QualifiedForAutomaticOutput
                    && bool.TryParse(a.Value.String, out bool qualifies)
                    && qualifies)
                != null;
        }

        private static bool DefaultCompareDocuments(IAttribute expected, IAttribute found, IEnumerable<int> deletedPagesFromAllExpected)
        {
            var expectedDeletedPages = new HashSet<int>(deletedPagesFromAllExpected);
            var foundDeletedPages = new HashSet<int>(GetPages(found, true));

            var expectedPages = new HashSet<int>(GetPages(expected, false).Union(expectedDeletedPages));
            var foundPages = new HashSet<int>(GetPages(found, false).Union(foundDeletedPages));

            // Require some overlap in pages
            if (!expectedPages.Intersect(foundPages).Any())
            {
                return false;
            }

            var missedPages = new HashSet<int>(expectedPages.Except(foundPages));
            var extraPages = new HashSet<int>(foundPages.Except(expectedPages));

            // Don't count missed pages if they were marked for deletion in any expected document
            missedPages.ExceptWith(expectedDeletedPages);

            // Don't count extra pages if they were marked for deletion in any expected document
            extraPages.ExceptWith(expectedDeletedPages);

            // Don't count extra pages if they were marked for deletion in the found (risky)
            extraPages.ExceptWith(foundDeletedPages);

            return missedPages.Count == 0 && extraPages.Count == 0;
        }
    }
}