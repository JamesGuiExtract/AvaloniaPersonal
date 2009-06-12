using System;
using System.Collections.Generic;
using System.Text;

namespace Extract
{
    /// <summary>
    /// Custom attribute for storing the companies Url in an assembly attribute.
    /// </summary>
    // Based on the following blog post:
    // http://www.codinghorror.com/blog/archives/000142.html
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class AssemblyCompanyUrlAttribute : Attribute
    {
        /// <summary>
        /// The company Url.
        /// </summary>
        private string _companyUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyCompanyUrlAttribute"/> class.
        /// </summary>
        /// <param name="companyUrl">The Url for the company.</param>
        public AssemblyCompanyUrlAttribute(string companyUrl)
        {
            try
            {
                _companyUrl = companyUrl;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23076", ex);
            }
        }

        /// <summary>
        /// Gets the company url.
        /// </summary>
        /// <returns>The company url.</returns>
        public string CompanyUrl
        {
            get
            {
                return _companyUrl;
            }
        }
    }
}
