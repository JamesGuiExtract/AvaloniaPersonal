// SSNFinder.cpp : Implementation of CSSNFinder
#include "stdafx.h"
#include "RedactionCustomComponents.h"
#include "SSNFinder.h"

#include <UCLIDException.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <EncryptedFileManager.h>
#include <ComponentLicenseIDs.h>
#include <LicenseMgmt.h>
#include <OCRConstants.h>

// add license management function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 2;

// minimum and maximum digits contained in the first, second, and third part of a found SSN
const int giMIN_FIRST_DIGITS = 2;
const int giMAX_FIRST_DIGITS = 4;
const int giMAX_SECOND_DIGITS = 3;
const int giMIN_THIRD_DIGITS = 2;
const int giMAX_THIRD_DIGITS = 5;

// minimum and maximum number of total characters contained in a found SSN
const int giMAX_TOTAL_SPACES = 2;
const int giMAX_TOTAL_UNREC = 3;
const int giMIN_TOTAL_DIGITS = 5;
const int giMIN_TOTAL_CHARS = giMIN_TOTAL_DIGITS + 2; // + 2 for the hyphens

// OCR engine filter character options
const EFilterCharacters eFILTER_CHARS = EFilterCharacters(kNumeralFilter | kHyphenFilter | kUnderscoreFilter);

//-------------------------------------------------------------------------------------------------
// CSSNFinder
//-------------------------------------------------------------------------------------------------
CSSNFinder::CSSNFinder()
  : m_bDirty(false),
    m_strSubattributeName("SSN"),
	m_bSpatialSubattribute(true),
	m_bClearIfNoneFound(true)
{
	try
	{
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI17271");
}
//-------------------------------------------------------------------------------------------------
CSSNFinder::~CSSNFinder()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI17272");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSSNFinder::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ISSNFinder,
		&IID_IAttributeModifyingRule,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_ILicensedComponent,
		&IID_IMustBeConfiguredObject
	};

	for (int i = 0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
		{
			return S_OK;
		}
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ISSNFinder
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSSNFinder::SetOptions(BSTR bstrSubattributeName, VARIANT_BOOL vbSpatialSubattribute, 
									VARIANT_BOOL vbClearIfNoneFound)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// validate license
		validateLicense();

		// store options
		m_strSubattributeName = asString(bstrSubattributeName);
		m_bSpatialSubattribute = asCppBool(vbSpatialSubattribute);
		m_bClearIfNoneFound = asCppBool(vbClearIfNoneFound);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18167");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSSNFinder::GetOptions(BSTR* pbstrSubattributeName, VARIANT_BOOL* pvbSpatialSubattribute, 
		VARIANT_BOOL* pvbClearIfNoneFound)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// validate license
		validateLicense();

		// ensure parameters are non-NULL
		ASSERT_ARGUMENT("ELI18300", pbstrSubattributeName != NULL);
		ASSERT_ARGUMENT("ELI18301", pvbSpatialSubattribute != NULL);
		ASSERT_ARGUMENT("ELI18302", pvbClearIfNoneFound != NULL);

		// set options
		*pbstrSubattributeName = _bstr_t(m_strSubattributeName.c_str()).Detach();
		*pvbSpatialSubattribute = asVariantBool(m_bSpatialSubattribute);
		*pvbClearIfNoneFound = asVariantBool(m_bClearIfNoneFound);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18170");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IAttributeModifyingRule
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSSNFinder::raw_ModifyValue(IAttribute* pAttribute, IAFDocument* pOriginInput, 
													 IProgressStatus* pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// check licensing
		validateLicense();

		// get the attribute
		IAttributePtr ipAttribute(pAttribute);
		ASSERT_RESOURCE_ALLOCATION("ELI17310", ipAttribute != NULL);

		// get the attribute's spatial string value	
		ISpatialStringPtr ipSpatialString(ipAttribute->Value);
		ASSERT_RESOURCE_ALLOCATION("ELI17311", ipSpatialString != NULL);

		// get the source document name
		string strSourceDocName = asString(ipSpatialString->SourceDocName);

		// get the spatial page info map
		ILongToObjectMapPtr ipPageInfoMap(ipSpatialString->SpatialPageInfos);
		ASSERT_RESOURCE_ALLOCATION("ELI19867", ipPageInfoMap != NULL);

		// get the raster zones of this attribute
		IIUnknownVectorPtr ipZones( ipSpatialString->GetOriginalImageRasterZones() );
		ASSERT_RESOURCE_ALLOCATION("ELI19688", ipZones != NULL);

		// create a spatial string to hold the result
		ISpatialStringPtr ipFoundText(CLSID_SpatialString);
		ASSERT_RESOURCE_ALLOCATION("ELI19689", ipFoundText != NULL);
		
		// set a flag to indicate no text has been found yet
		bool bFoundText = false;

		// instantiate a new OCR engine if there is at least one zone to OCR
		long lSize = ipZones->Size();
		IOCREnginePtr ipOCREngine(lSize > 0 ? getOCREngine() : NULL);

		map<int, ILongRectanglePtr> mapPageBounds;

		// iterate through each zone in the attribute
		for(long i=0; i<lSize; i++)
		{
			// get the ith raster zone
			IRasterZonePtr ipZone = ipZones->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI19690", ipZone != NULL);

			// Get the page number of this raster zone
			long lPage = ipZone->PageNumber;

			// Get the page bounds (for use by GetRectangularBounds)
			if (mapPageBounds.find(lPage) == mapPageBounds.end())
			{
				mapPageBounds[lPage] = ipSpatialString->GetOriginalImagePageBounds(lPage);
			}
			ASSERT_RESOURCE_ALLOCATION("ELI30318", mapPageBounds[lPage] != NULL);

			// find handwritten numerals within the specified bounds of the spatial string
			ISpatialStringPtr ipZoneText = ipOCREngine->RecognizeTextInImageZone(
				strSourceDocName.c_str(), lPage, lPage, 
				ipZone->GetRectangularBounds(mapPageBounds[lPage]), 0, eFILTER_CHARS, "", 
				VARIANT_TRUE, VARIANT_TRUE, VARIANT_TRUE, pProgressStatus);
			ASSERT_RESOURCE_ALLOCATION("ELI18063", ipZoneText != NULL);

			// if any text was found, append it
			if(ipZoneText->IsEmpty() == VARIANT_FALSE)
			{
				// check if this is the first found text
				if(bFoundText)
				{
					// the result already contains some found text.
					// insert a line break between the previous and current found text.
					ipFoundText->AppendString("\r\n");
				}
				else
				{
					// this is the first found text
					bFoundText = true;
				}

				// append the found text
				ipFoundText->Append(ipZoneText);
			}
		}

		// get the found text as a string
		string strFoundText = ipFoundText->String;

		// examine each line of found text for SSNs
		list<StringSegmentType> listSegments;
		int iStartIndex=0, iEndIndex = strFoundText.find_first_of("\r\n");
		while(iEndIndex != string::npos)
		{	
			// look for SSNs within this line, if this line
			// has enough characters to contain an SSN
			if(iEndIndex - iStartIndex >= giMIN_TOTAL_CHARS)
			{
				findSSNs(strFoundText, iStartIndex, iEndIndex, listSegments);
			}

			iStartIndex = iEndIndex + 1;
			iEndIndex = strFoundText.find_first_of("\r\n", iStartIndex);
		}

		// search the last line for SSNs
		iEndIndex = strFoundText.length();
		if(iEndIndex - iStartIndex >= giMIN_TOTAL_CHARS)
		{
			findSSNs(strFoundText, iStartIndex, iEndIndex, listSegments);
		}

		// check if any SSNs were found
		if( !listSegments.empty() )
		{
			// create an IIUnknownVector to store the found subattributes
			IIUnknownVectorPtr ipSubAttributes(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI18074", ipSubAttributes != NULL);

			// get the subattribute type
			_bstr_t bstrSubattributeType = ipAttribute->Type;

			for each(StringSegmentType segment in listSegments)
			{
				// get the next found social security number
				ISpatialStringPtr ipSSN = ipFoundText->GetSubString(segment.iStartIndex, segment.iEndIndex);
				ASSERT_RESOURCE_ALLOCATION("ELI18075", ipSSN != NULL);

				// if the subattributes should not be spatial, downgrade them to hybrid
				if(!m_bSpatialSubattribute)
				{
					ipSSN->DowngradeToHybridMode();
				}

				// create an attribute to store the current found subattribute
				IAttributePtr ipSubAttribute(CLSID_Attribute);
				ASSERT_RESOURCE_ALLOCATION("ELI18147", ipSubAttribute != NULL);
				ipSubAttribute->Name = m_strSubattributeName.c_str();
				ipSubAttribute->Type = bstrSubattributeType;
				ipSubAttribute->Value = ipSSN;

				// add it to the vector of subattributes
				ipSubAttributes->PushBack(ipSubAttribute);
			}

			// set the sub attributes
			ipAttribute->SubAttributes = ipSubAttributes;
		}
		else if(m_bClearIfNoneFound)
		{
			// if no SSNs were found and the clear if none found option is set,
			// clear the original attribute
			ISpatialStringPtr ipEmptySpatialString(CLSID_SpatialString);
			ASSERT_RESOURCE_ALLOCATION("ELI18077", ipEmptySpatialString != NULL);
			ipAttribute->Value = ipEmptySpatialString;
		}	
		// else no SSNs were found and the original attribute should be retained
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17273");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSSNFinder::raw_CopyFrom(IUnknown* pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// validate license first
		validateLicense();

		// get the SSNFinder interface
		UCLID_REDACTIONCUSTOMCOMPONENTSLib::ISSNFinderPtr ipSSNFinder(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI18174", ipSSNFinder != NULL);

		// get the options from the SSNFinder object
		_bstr_t bstrSubattributeName;
		VARIANT_BOOL vbSpatialSubattribute, vbClearIfNoneFound;
		ipSSNFinder->GetOptions(bstrSubattributeName.GetAddress(), &vbSpatialSubattribute,
			&vbClearIfNoneFound);
		
		// store the found options
		m_strSubattributeName = bstrSubattributeName;
		m_bSpatialSubattribute = asCppBool(vbSpatialSubattribute);
		m_bClearIfNoneFound = asCppBool(vbClearIfNoneFound);

		// set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17274");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSSNFinder::raw_Clone(IUnknown** pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// validate license first
		validateLicense();

		// ensure that the return value pointer is non-NULL
		ASSERT_ARGUMENT("ELI18303", pObject != NULL);

		// get the copyable object interface
		ICopyableObjectPtr ipObjCopy(CLSID_SSNFinder);
		ASSERT_RESOURCE_ALLOCATION("ELI17276", ipObjCopy != NULL);

		// create a shallow copy
		IUnknownPtr ipUnknown(this);
		ipObjCopy->CopyFrom(ipUnknown);

		// return the new SSNFinder to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17275");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSSNFinder::raw_GetComponentDescription(BSTR* pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18304", pstrComponentDescription);
		
		*pstrComponentDescription = _bstr_t("Find SSNs within image area").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17277");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSSNFinder::raw_IsLicensed(VARIANT_BOOL* pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// ensure the return value pointer is non-NULL
		ASSERT_ARGUMENT("ELI18306", pbValue != NULL);

		try
		{
			// check license
			validateLicense();

			// if no exception was thrown, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18305");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSSNFinder::raw_IsConfigured(VARIANT_BOOL* pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		// ensure the return value pointer is non-NULL
		ASSERT_ARGUMENT("ELI18307", pbValue != NULL);

		// the SSNFinder is configured if it has a subattribute name
		*pbValue = asVariantBool(!m_strSubattributeName.empty());
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17289");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSSNFinder::GetClassID(CLSID* pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	*pClassID = CLSID_SSNFinder;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSSNFinder::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
// Version 1:
//   no persistent options
// Version 2:
//   added subattribute name, spatial subattribute, and clear if none found options
STDMETHODIMP CSSNFinder::Load(IStream* pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// check license state
		validateLicense();
		
		// check parameter
		ASSERT_RESOURCE_ALLOCATION("ELI17278", pStream != NULL);

		// clear the options
		m_strSubattributeName = "SSN";
		m_bSpatialSubattribute = true;
		m_bClearIfNoneFound = true;
		
		// read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read(&nDataLength, sizeof(nDataLength), NULL);
		ByteStream data(nDataLength);
		pStream->Read(data.getData(), nDataLength, NULL);
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);

		// read the data version
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// check if file is a newer version than this object can use
		if (nDataVersion > gnCurrentVersion)
		{
			// throw exception
			UCLIDException ue("ELI17279", "Unable to load newer SSNFinder.");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// read data items
		if(nDataVersion > 1)
		{
			dataReader >> m_strSubattributeName;
			dataReader >> m_bSpatialSubattribute;
			dataReader >> m_bClearIfNoneFound;
		}
		
		// clear the dirty flag since a new object was loaded
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17280");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSSNFinder::Save(IStream* pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// check license state
		validateLicense();

		// check parameter
		ASSERT_ARGUMENT("ELI17281", pStream != NULL);

		// create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);
		dataWriter << gnCurrentVersion;
		dataWriter << m_strSubattributeName;
		dataWriter << m_bSpatialSubattribute;
		dataWriter << m_bClearIfNoneFound;
		dataWriter.flushToByteStream();

		// write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17283");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSSNFinder::GetSizeMax(ULARGE_INTEGER* pcbSize)
{
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
IOCREnginePtr CSSNFinder::getOCREngine()
{
	// instantiate a new OCR engine every time this function is called [P13 #2909]
	IOCREnginePtr ipOCREngine(CLSID_ScansoftOCR);
	ASSERT_RESOURCE_ALLOCATION("ELI17313", ipOCREngine != NULL);

	// license the engine
	IPrivateLicensedComponentPtr ipOCREngineLicense(ipOCREngine);
	ASSERT_RESOURCE_ALLOCATION("ELI17314", ipOCREngineLicense != NULL);
	ipOCREngineLicense->InitPrivateLicense(LICENSE_MGMT_PASSWORD.c_str());

	return ipOCREngine;
}
//-------------------------------------------------------------------------------------------------
void CSSNFinder::validateLicense()
{
	// ensure IDShield is licensed for this object to run
	VALIDATE_LICENSE(gnIDSHIELD_CORE_OBJECTS, "ELI18565", "SSNFinder");

	// ensure that handwriting recognition in the OCR engine is licensed
	VALIDATE_LICENSE(gnHANDWRITING_RECOGNITION_FEATURE, "ELI18298", "SSNFinder");
}
//-------------------------------------------------------------------------------------------------
// This algorithm looks for SSNs that match certain constraints as defined by global constants.
// The constraints define a valid SSN as a string containing a certain minimum and maximum number 
// of characters that precede, succeed, and are in between a pair of hyphens.
//
// The overall steps are:
// 1. Find next pair of hyphens
// 2. Ensure constraints are met before the first hyphen
// 3. Ensure constraints are met between the hyphens
// 4. Ensure constraints are met after the second hyphen
// 5. Ensure overall constraints are met and, if so, store result
// 6. Repeat
void CSSNFinder::findSSNs(const string& strText, int iStartLineIndex, int iEndLineIndex, 
						  list<StringSegmentType>& listSegments)
{
	// Step 1: Find first pair of hyphens

	// find the first hyphen
	int iFirstHyphen = strText.find_first_of('-', iStartLineIndex);

	// if the first hyphen is before the end of the line
	if(iFirstHyphen >= iEndLineIndex || iFirstHyphen == string::npos)
	{
		return;
	}

	// find the second hyphen
	int iSecondHyphen = strText.find_first_of('-', iFirstHyphen+1);
	if(iSecondHyphen - iFirstHyphen == 1)
	{
		// a double hyphen counts as a single hyphen
		iSecondHyphen = strText.find_first_of('-', iSecondHyphen+1);
	}

	// continue as long as two hyphen have been found in the range [iStartLineIndex, iEndLineIndex)
	while(iSecondHyphen < iEndLineIndex && iSecondHyphen != string::npos)
	{
		while(true)
		{
			// Step 2: Ensure constraints are met before the first hyphen

			// initialize counters
			int iNumFirstDigits = 0;
			int iNumFirstUnrec = 0;
			int iNumFirstSpaces = 0;

			// initialize start index of found SSN
			int iStartSSN = -1;

			// look for approximately three digits before the first hyphen
			for(int i=iFirstHyphen-1; i>=iStartLineIndex; i--)
			{
				// increment the appropriate counters
				if( !incrementCounters(strText[i], iNumFirstDigits, iNumFirstUnrec, 
					iNumFirstSpaces, iStartSSN != -1) )
				{
					// stop if we encountered a disqualifying characer
					break;
				}

				// check if we have found the start of the SSN
				if( findMatchingIndex(giMIN_FIRST_DIGITS, giMAX_FIRST_DIGITS, i, iNumFirstDigits, 
					iNumFirstUnrec, iStartSSN) )
				{
					// stop if we found the start index
					break;
				}
			}

			// if the first part does not appear to be an SSN, check for more hyphens
			if(iStartSSN == -1)
			{		
				break;
			}

			// Step 3: Ensure constraints are met between the hyphens

			// account for a double hyphen
			int i = iFirstHyphen + (strText[iFirstHyphen+1] == '-' ? 2 : 1);

			// look for digits between the two hyphens
			bool bIsDisqualified = false;
			int iNumSecondDigits = 0;
			int iNumSecondUnrec = 0;
			int iNumSecondSpaces = 0;
			for( ; i < iSecondHyphen; i++)
			{
				// increment counters as appropriate
				if( !incrementCounters(strText[i], iNumSecondDigits, iNumSecondUnrec, 
					iNumSecondSpaces) )
				{
					// stop if we encountered a disqualifying character
					bIsDisqualified = true;
					break;
				}
			}

			// check if requirements have been met for the middle digits
			if(bIsDisqualified || iNumSecondDigits == 0 && iNumSecondUnrec == 0 && iNumSecondSpaces == 0 || 
				iNumSecondDigits + iNumSecondUnrec > giMAX_SECOND_DIGITS)
			{
				break;
			}
			
			// Step 4: Ensure constraints are met after the second hyphen

			// account for a double hyphen
			i = iSecondHyphen + 1;
			if(i < (int) strText.length() && strText[i] == '-')
			{
				i++;
			}

			// prepare counters and the index of the end of the SSN
			int iEndSSN = -1;
			int iNumThirdDigits = 0;
			int iNumThirdUnrec = 0;
			int iNumThirdSpaces = 0;

			// search for digits after the second hyphen
			for( ; i<iEndLineIndex; i++)
			{
				// increment the appropriate counters
				if( !incrementCounters(strText[i], iNumThirdDigits, iNumThirdUnrec, 
					iNumThirdSpaces, iEndSSN != -1) )
				{
					// stop if we encountered a disqualifying characer
					break;
				}

				// check if we have found the end of the SSN
				if( findMatchingIndex(giMIN_THIRD_DIGITS, giMAX_THIRD_DIGITS, i, iNumThirdDigits, 
					iNumThirdUnrec, iEndSSN) )
				{
					// stop if found the final matching index
					break;
				}
			}

			// Step 5: Ensure overall constraints are met and, if so, store result

			// check if requirements have been met for a valid SSN, namely:
			// (1) A start and end SSN index have been found.
			// (2) The total number of spaces, unrecognized characters, 
			// and digits do not exceed their corresponding maximum.
			if(iEndSSN != -1 && (iNumFirstSpaces + iNumSecondSpaces + iNumThirdSpaces) <= giMAX_TOTAL_SPACES &&
				(iNumFirstUnrec + iNumSecondUnrec + iNumThirdUnrec) <= giMAX_TOTAL_UNREC &&
				(iNumFirstDigits + iNumSecondDigits + iNumThirdDigits) >= giMIN_TOTAL_DIGITS )
			{
				// check if the currently found SSN is overlaps the previously found SSN
				if(!listSegments.empty() && iStartSSN <= listSegments.back().iEndIndex)
				{
					// expand the previously found SSN to encompass both
					listSegments.back().iEndIndex = iEndSSN;
				}
				else
				{
					// add this SSN
					StringSegmentType segment = {iStartSSN, iEndSSN};
					listSegments.push_back(segment);
				}
			}

			break;
		} // end while(true)

		// Step 6: Repeat

		// search for the next pair of hyphens
		iFirstHyphen = iSecondHyphen;
		iSecondHyphen = strText.find_first_of('-', iFirstHyphen+1);
		if(iSecondHyphen - iFirstHyphen == 1)
		{
			// a double hyphen counts as a single hyphen
			iSecondHyphen = strText.find_first_of('-', iSecondHyphen+1);
		}
	}
}
//-------------------------------------------------------------------------------------------------
bool CSSNFinder::incrementCounters(char cChar, int& riNumDigits, int& riNumUnrec, int& riNumSpaces, 
					   bool bSpaceDisqualifies)
{
	// increment the appropriate counters
	if( isDigitChar(cChar) )
	{
		// cChar is a digit
		riNumDigits++;
	}
	else if(cChar == gcUNRECOGNIZED)
	{
		// cChar is an unrecognized character
		riNumUnrec++;
	}
	else if(cChar == ' ' || cChar == '_')
	{
		// cChar is a space (underscores are treated like spaces)

		// if the space is a disqualifying character, stop here
		if(bSpaceDisqualifies)
		{
			return false;
		}

		riNumSpaces++;
	}
	else
	{
		// this is a disqualifying character
		return false;
	}

	// continue searching
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CSSNFinder::findMatchingIndex(int iMinDigits, int iMaxDigits, int iIndex, int& riNumDigits, 
					   int& riNumUnrec, int& riMatchingIndex)
{
	// check if we have found the minimum number of digits
	if(riNumDigits + riNumUnrec >= iMinDigits)
	{
		// store the index of the found SSN
		riMatchingIndex = iIndex;
		
		// if this was the maximum allowable number of digits, return true.
		// otherwise, this may be a non-terminal matching index.
		return (riNumDigits + riNumUnrec >= iMaxDigits);
	}

	// we have not yet found a matching index
	return false;
}
//-------------------------------------------------------------------------------------------------
