using System.Security.Claims;

namespace WebAPI.Models
{
    /// Create instances of IDocument
    public interface IDocumentDataFactory
    {
        /// Create from an ApiContext
        IDocumentData Create(ApiContext apiContext);
        /// Create from a user
        IDocumentData Create(ClaimsPrincipal user, bool requireSession);
    }
}