// The encryption keys for PDF passwords when passing as arguments to
// ImageFormatConverter or ESConvertToPDF
const unsigned long	gulPdfKey1 = 0x3C214A7D;
const unsigned long	gulPdfKey2 = 0x499282E1;
const unsigned long	gulPdfKey3 = 0xE5BED767;
const unsigned long	gulPdfKey4 = 0x2B840052;

// Constant values for the PDF security properties
// These values can be OR'd together to set multiple flags
const int giAllowLowQualityPrinting = 0x1;
const int giAllowHighQualityPrinting = 0x2;
const int giAllowDocumentModifications = 0x4;
const int giAllowContentCopying = 0x8;
const int giAllowContentCopyingForAccessibility = 0x10;
const int giAllowAddingModifyingAnnotations = 0x20;
const int giAllowFillingInFields = 0x40;
const int giAllowDocumentAssembly = 0x80;

// Returns true if the specific flag value has been set in the all flags value
inline bool isFlagSet(int nAllFlags, int nSpecificFlag)
{
	return (nAllFlags & nSpecificFlag) == nSpecificFlag;
}