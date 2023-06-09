﻿using Extract.Utilities;
using org.apache.pdfbox.pdmodel;
using System;
using System.Collections.Generic;
using System.Linq;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.AttributeFinder.Rules
{
    public static class HawkeyePaginationSplitter
    {
        /// <summary>
        /// Use ExtractSystems.LogicalDocumentNumber values encoded in PDF pages to create one or more Document attributes
        /// </summary>
        /// <remarks>
        /// This is used for Essentia Accounts Payable (Hawkeye) rules. If the input is not a PDf or doesn't contain
        /// ExtractSystems.LogicalDocumentNumber information then a single Document attribute will be returned.
        /// The Document attributes will have Pages, SubFileID, UnitID, and (usually) DocumentData subattributes.
        /// DocumentData is omitted for the first document (unless there is only one) and its copies.
        /// </remarks>
        /// <returns>Returns the input document where the subattributes have been replaced with Document attributes</returns>
        [CLSCompliant(false)]
        public static AFDocument SplitDocument(AFDocument afDocument)
        {
            try
            {
                if (!afDocument.Text.HasSpatialInfo())
                {
                    return afDocument;
                }

                IEnumerable<AttributeClass> documents = GetDocumentsFromPdf(afDocument);
                afDocument.Attribute.SubAttributes = documents.ToIUnknownVector();

                return afDocument;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53327");
            }
        }

        // Get Document attributes from the input
        private static IEnumerable<AttributeClass> GetDocumentsFromPdf(AFDocument inputDocument)
        {
            NumericSequencer pageNumberContractor = new()
            {
                ExpandSequence = false
            };

            var documents = GetLogicalDocuments(inputDocument.Text);

            if (documents.Count == 0)
            {
                yield break;
            }

            if (documents.Count == 1)
            {
                // If there is only a single document then the SubFileID needs changing from 2 to 1
                // Also assume it needs rules to be run on it (CreateDocumentData = true)
                documents[0].SubFileID = 1;
                documents[0].CreateDocumentData = true;

                yield return documents[0].CreateDocument(inputDocument, pageNumberContractor);
            }

            // Output each document after the first one with a copy of the first document after each
            foreach (var subFile in documents.Skip(1))
            {
                yield return subFile.CreateDocument(inputDocument, pageNumberContractor);

                var copyOfEmailBody = documents[0].ShallowClone();
                copyOfEmailBody.UnitID = subFile.UnitID;
                copyOfEmailBody.SubFileID = subFile.SubFileID + 1;

                yield return copyOfEmailBody.CreateDocument(inputDocument, pageNumberContractor);
            }
        }

        // Get a SubFile for each document that makes up a PDF
        private static IList<SubFile> GetLogicalDocuments(ISpatialString inputDocument)
        {
            var ocrPages = inputDocument.GetPages(true, Constants.EmptyPagePlaceholderText);

            string sourceDocName = inputDocument.SourceDocName;
            var logicalDocumentNumbers = GetLogicalDocumentNumbers(sourceDocName);

            if (logicalDocumentNumbers == null)
            {
                return new[] { new SubFile(1, Enumerable.Range(1, ocrPages.Size()), ocrPages) };
            }

            return logicalDocumentNumbers
                .Select((docNum, i) => new { docNum, pageNum = i + 1 })
                .GroupBy(docNumToPageNum => docNumToPageNum.docNum)
                .Select(group =>
                {
                    int docNum = group.Key;
                    var pageNumbers = group.Select(docNumToPageNum => docNumToPageNum.pageNum);
                    var pages = group.Select(docNumToPageNum => (ISpatialString)ocrPages.At(docNumToPageNum.pageNum - 1)).ToIUnknownVector();
                    return new SubFile(docNum, pageNumbers, pages, docNum > 1);
                })
                .ToList();

        }

        // Extract document numbers saved to PDF pages as ExtractSystems.LogicalDocumentNumber 
        // returns null if all pages should be treated as part of the same document
        private static List<int> GetLogicalDocumentNumbers(string sourceDocName)
        {
            try
            {
                bool isPdf =
                    !String.IsNullOrEmpty(sourceDocName)
                    && String.Equals(".pdf", System.IO.Path.GetExtension(sourceDocName), StringComparison.OrdinalIgnoreCase);

                if (!isPdf)
                {
                    return null;
                }

                using PDDocument pdfDocument = PDDocument.load(new java.io.File(sourceDocName));
                var pages = pdfDocument.getPages();
                var docNumbers = pages
                    .Cast<PDPage>()
                    .Select(page => page.getCOSObject().getInt(Constants.LogicalDocumentNumberPdfTag))
                    .TakeWhile(docNumber => docNumber > 0) // If the key is missing then getInt() returns 0
                    .ToList();

                // If ExtractSystems.LogicalDocumentNumber is missing, the PDF was probably
                // not converted from an email and should be treated as a single document
                if (docNumbers.Count != pages.getCount())
                {
                    return null;
                }

                return docNumbers;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53350");
            }
        }

        /// <summary>
        /// Class to encapsulate creating a Document attribute graph
        /// </summary>
        class SubFile
        {
            public bool CreateDocumentData { get; set; }
            public int UnitID { get; set; }
            public int SubFileID { get; set; }
            public List<int> PageNumbers { get; private set; }
            public IUnknownVector Pages { get; private set; }

            /// <summary>
            /// Create a SubFile
            /// </summary>
            /// <remarks>
            /// UnitID and SubFileID will be calculated from the documentNumber to leave room for copies of the first document to be inserted into the sequence later
            /// SubFileID will need to be changed if there is only a single logical document
            /// </remarks>
            public SubFile(
                int documentNumber,
                IEnumerable<int> pageNumbers,
                IUnknownVector ocrPages,
                bool createDocumentData = true)
            {
                CreateDocumentData = createDocumentData;
                UnitID = Math.Max(1, documentNumber - 1);
                SubFileID = documentNumber switch
                {
                    1 => 2,
                    2 => 1,
                    _ => documentNumber * 2 - 3
                };
                PageNumbers = pageNumbers.ToList();
                Pages = ocrPages;
            }

            /// <summary>
            /// Make a shallow copy of this
            /// </summary>
            public SubFile ShallowClone()
            {
                return (SubFile)MemberwiseClone();
            }

            /// <summary>
            /// Create a Document attribute with subattributes
            /// </summary>
            public AttributeClass CreateDocument(AFDocument inputDocument, NumericSequencer pageNumberContractor)
            {
                try
                {
                    string sourceDocName = inputDocument.Text.SourceDocName;

                    AttributeClass currentDocument = new();
                    currentDocument.Name = "Document";
                    currentDocument.Value.CreateFromSpatialStrings(Pages, false);

                    AttributeClass pages = new();
                    currentDocument.SubAttributes.PushBack(pages);
                    pages.Name = "Pages";
                    pages.Value.CreateNonSpatialString(String.Join(",", PageNumbers), sourceDocName);
                    pageNumberContractor.ModifyValue(pages, inputDocument, null);

                    AttributeClass subFileID = new();
                    currentDocument.SubAttributes.PushBack(subFileID);
                    subFileID.Name = "SubFileID";
                    subFileID.Value.CreateNonSpatialString(UtilityMethods.FormatInvariant($"{SubFileID}"), sourceDocName);

                    AttributeClass unitID = new();
                    currentDocument.SubAttributes.PushBack(unitID);
                    unitID.Name = "UnitID";
                    unitID.Value.CreateNonSpatialString(UtilityMethods.FormatInvariant($"{UnitID}"), sourceDocName);

                    if (CreateDocumentData)
                    {
                        AttributeClass documentData = new();
                        currentDocument.SubAttributes.PushBack(documentData);
                        documentData.Name = "DocumentData";
                        documentData.Value = currentDocument.Value;
                        currentDocument.Value = pages.Value;
                    }

                    return currentDocument;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI53328");
                }
            }
        }
    }
}
