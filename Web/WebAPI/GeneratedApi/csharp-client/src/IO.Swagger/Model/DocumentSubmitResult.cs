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
    /// This class is used to return File or Text submission result
    /// </summary>
    [DataContract]
    public partial class DocumentSubmitResult :  IEquatable<DocumentSubmitResult>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentSubmitResult" /> class.
        /// </summary>
        /// <param name="Id">The identifier for the submitted file.</param>
        /// <param name="Error">error info.</param>
        public DocumentSubmitResult(string Id = default(string), ErrorInfo Error = default(ErrorInfo))
        {
            this.Id = Id;
            this.Error = Error;
        }
        
        /// <summary>
        /// The identifier for the submitted file
        /// </summary>
        /// <value>The identifier for the submitted file</value>
        [DataMember(Name="id", EmitDefaultValue=false)]
        public string Id { get; set; }
        /// <summary>
        /// error info
        /// </summary>
        /// <value>error info</value>
        [DataMember(Name="error", EmitDefaultValue=false)]
        public ErrorInfo Error { get; set; }
        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class DocumentSubmitResult {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
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
            return this.Equals(obj as DocumentSubmitResult);
        }

        /// <summary>
        /// Returns true if DocumentSubmitResult instances are equal
        /// </summary>
        /// <param name="other">Instance of DocumentSubmitResult to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(DocumentSubmitResult other)
        {
            // credit: http://stackoverflow.com/a/10454552/677735
            if (other == null)
                return false;

            return 
                (
                    this.Id == other.Id ||
                    this.Id != null &&
                    this.Id.Equals(other.Id)
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
                if (this.Id != null)
                    hash = hash * 59 + this.Id.GetHashCode();
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
