/* 
 * Document API
 *
 * Extract Document API documentation
 *
 * OpenAPI spec version: v1
 * Contact: developers@extractsystems.com
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */

using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;

namespace IO.Swagger.Model
{
    /// <summary>
    /// Document attribute set - contains a (possibly empty) list of document attributes
    /// </summary>
    [DataContract]
    public partial class DocumentAttributeSet :  IEquatable<DocumentAttributeSet>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentAttributeSet" /> class.
        /// </summary>
        /// <param name="Attributes">list of attributes - may be empty (on error WILL be empty).</param>
        /// <param name="Error">Error info - Error &#x3D;&#x3D; true if there has been an error.</param>
        public DocumentAttributeSet(List<DocumentAttribute> Attributes = default(List<DocumentAttribute>), ErrorInfo Error = default(ErrorInfo))
        {
            this.Attributes = Attributes;
            this.Error = Error;
        }
        
        /// <summary>
        /// list of attributes - may be empty (on error WILL be empty)
        /// </summary>
        /// <value>list of attributes - may be empty (on error WILL be empty)</value>
        [DataMember(Name="attributes", EmitDefaultValue=false)]
        public List<DocumentAttribute> Attributes { get; set; }
        /// <summary>
        /// Error info - Error &#x3D;&#x3D; true if there has been an error
        /// </summary>
        /// <value>Error info - Error &#x3D;&#x3D; true if there has been an error</value>
        [DataMember(Name="error", EmitDefaultValue=false)]
        public ErrorInfo Error { get; set; }
        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class DocumentAttributeSet {\n");
            sb.Append("  Attributes: ").Append(Attributes).Append("\n");
            sb.Append("  Error: ").Append(Error).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
  
        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="obj">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object obj)
        {
            // credit: http://stackoverflow.com/a/10454552/677735
            return this.Equals(obj as DocumentAttributeSet);
        }

        /// <summary>
        /// Returns true if DocumentAttributeSet instances are equal
        /// </summary>
        /// <param name="other">Instance of DocumentAttributeSet to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(DocumentAttributeSet other)
        {
            // credit: http://stackoverflow.com/a/10454552/677735
            if (other == null)
                return false;

            return 
                (
                    this.Attributes == other.Attributes ||
                    this.Attributes != null &&
                    this.Attributes.SequenceEqual(other.Attributes)
                ) && 
                (
                    this.Error == other.Error ||
                    this.Error != null &&
                    this.Error.Equals(other.Error)
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            // credit: http://stackoverflow.com/a/263416/677735
            unchecked // Overflow is fine, just wrap
            {
                int hash = 41;
                // Suitable nullity checks etc, of course :)
                if (this.Attributes != null)
                    hash = hash * 59 + this.Attributes.GetHashCode();
                if (this.Error != null)
                    hash = hash * 59 + this.Error.GetHashCode();
                return hash;
            }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        { 
            yield break;
        }
    }

}
