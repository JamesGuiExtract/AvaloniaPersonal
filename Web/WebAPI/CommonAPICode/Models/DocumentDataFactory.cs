using System;
using System.Security.Claims;

namespace WebAPI.Models
{
    /// <summary>
    /// This class is the data model for the DocumentController.
    /// </summary>
    public sealed class DocumentDataFactory : IDocumentDataFactory
    {
        readonly IFileApiMgr _fileApiMgr;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentData"/> class.
        /// </summary>
        /// <param name="fileApiMgr">IFileApiMgr implementation to use for creating FileAPI instances</param>
        public DocumentDataFactory(IFileApiMgr fileApiMgr)
        {
            _fileApiMgr = fileApiMgr;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentData"/> class.
        /// </summary>
        /// <para><b>Note</b></para>
        /// This should be used only inside a using statement, so the fileApi in-use flag can be cleared.
        /// <param name="apiContext">The API context.</param>
        public IDocumentData Create(ApiContext apiContext)
        {
            return new DocumentData(apiContext, _fileApiMgr);
        }

        /// <summary>
        /// Initializes a <see cref="DocumentData"/> instance.
        /// <para><b>Note</b></para>
        /// This should be used only inside a using statement, so the fileApi in-use flag can be cleared.
        /// </summary>
        /// <param name="user">The <see cref="ClaimsPrincipal"/> this instance should be specific to.</param>
        /// <param name="requireSession"><c>true</c> if an active FAM session is required; otherwise, <c>false</c>.</param>
        public IDocumentData Create(ClaimsPrincipal user, bool requireSession)
        {
            return new DocumentData(user, requireSession, _fileApiMgr);
        }
    }
}
