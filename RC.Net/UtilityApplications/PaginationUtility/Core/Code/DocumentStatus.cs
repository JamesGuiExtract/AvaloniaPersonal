using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// Class that represents status info for a proposed pagionation output document.
    /// </summary>
    [DataContract]
    public class DocumentStatus
    {
        /// <summary>
        /// Indicates whether data for the document has been modified in the DEP.
        /// </summary>
        [DataMember]
        public bool DataModified { get; set; }

        /// <summary>
        /// Indicates whether there is an active data validation error for the document's data.
        /// </summary>
        [DataMember]
        public bool DataError { get; set; }

        /// <summary>
        /// <c>true</c> if the document type displayed in the panel is valid.
        /// </summary>
        [DataMember]
        public bool DocumentTypeIsValid { get; set; }

        /// <summary>
        /// Indicates whether there is an active data validation warning for the document's data.
        /// </summary>
        [DataMember]
        public bool DataWarning { get; set; }

        /// <summary>
        /// Indicates whether this document is to be sent for reprocessing (or <c>null</c> to
        /// make the determination based on whether pagination has been changed or pages have
        /// been rotated.
        /// </summary>
        [DataMember]
        public bool? Reprocess { get; set; }

        /// <summary>
        /// The summary text to display for the document.
        /// </summary>
        [DataMember]
        public string Summary { get; set; }

        /// <summary>
        /// The order numbers for the current document along with the order collection date (if known)
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ReadOnlyCollection<(string OrderNumber, DateTime? CollectionDate)> Orders { get; set; }

        /// <summary>
        /// Gets whether to prompt about order numbers for which a document has already been filed.
        /// </summary>
        [DataMember]
        public bool PromptForDuplicateOrders { get; set; }

        /// <summary>
        /// The encounter numbers for the current document along with the encounter date (if known)
        /// </summary>
        [DataMember]
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ReadOnlyCollection<(string EncounterNumber, DateTime? EncounterDate)> Encounters { get; set; }

        /// <summary>
        /// Gets whether to prompt about encounter numbers for which a document has already been filed.
        /// </summary>
        [DataMember]
        public bool PromptForDuplicateEncounters { get; set; }

        /// <summary>
        /// A stringized representation of the document data.
        /// </summary>
        [DataMember]
        public string StringizedData { get; set; }

        /// <summary>
        /// A stringized representation of a validation error in the document data (when DataError = true).
        /// </summary>
        [DataMember]
        public string StringizedError { get; set; }

        /// <summary>
        /// An exception that occured trying to generate document status data.
        /// </summary>
        public ExtractException Exception { get; set; }

        /// <summary>
        /// Produces a JSON representation of this instance.
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            try
            {
                return JsonConvert.SerializeObject(this,
                    new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Objects,
                        Formatting = Formatting.Indented
                    });
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47129");
            }
        }
        
        /// <summary>
        /// Generates an instance from the specified <see paramref="json"/> representation.
        /// </summary>
        public static DocumentStatus FromJson(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<DocumentStatus>(json);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47130");
            }
        }
    }
}
