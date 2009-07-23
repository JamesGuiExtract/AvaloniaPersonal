// SelectPageRegion.cpp : Implementation of CSelectPageRegion
#include "stdafx.h"
#include "AFPreProcessors.h"
#include "SelectPageRegion.h"
#include "..\\..\\AFCore\\Code\\Common.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>
#include <COMUtils.h>
#include <StringTokenizer.h>
#include <Misc.h>
#include <TemporaryFileName.h>
#include <MiscLeadUtils.h>
#include <ComponentLicenseIDs.h>

#include <algorithm>
#include <math.h>

// current version
const unsigned long gnCurrentVersion = 3;

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// CSelectPageRegion
//-------------------------------------------------------------------------------------------------
CSelectPageRegion::CSelectPageRegion()
: m_bDirty(false),
  m_bIncludeRegion(true),
  m_strSpecificPages(""),
  m_nHorizontalStartPercentage(-1),
  m_nHorizontalEndPercentage(-1),
  m_nVerticalStartPercentage(-1),
  m_nVerticalEndPercentage(-1),
  m_ipSpatialStringSearcher(NULL),
  m_ePageSelectionType(kSelectAll),
  m_strPattern(""),
  m_bIsRegExp(false),
  m_bIsCaseSensitive(false),
  m_eRegExpPageSelectionType(kSelectAllPagesWithRegExp),
  m_bOCRSelectedRegion(false),
  m_nRegionRotation(-1)
{
	m_ipAFUtility.CreateInstance(CLSID_AFUtility);
	ASSERT_RESOURCE_ALLOCATION("ELI09753", m_ipAFUtility != NULL);
}
//-------------------------------------------------------------------------------------------------
CSelectPageRegion::~CSelectPageRegion()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16326");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ISelectPageRegion,
		&IID_IDocumentPreprocessor,
		&IID_IPersistStream,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_IMustBeConfiguredObject,
		&IID_ILicensedComponent
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ISelectPageRegion
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::get_IncludeRegionDefined(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = asVariantBool(m_bIncludeRegion);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07999");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::put_IncludeRegionDefined(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bIncludeRegion = asCppBool(newVal);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08000");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::SelectPages(VARIANT_BOOL bSpecificPages, BSTR strSpecificPages)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		
		string strPages = asString(strSpecificPages);
		// validate the string format
		::validatePageNumbers(strPages);

		m_strSpecificPages = strPages;
		

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08001");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::GetPageSelections(VARIANT_BOOL *pbSpecificPages, 
												  BSTR *pstrSpecificPages)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pstrSpecificPages = _bstr_t(m_strSpecificPages.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08031");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::SetHorizontalRestriction(long nStartPercentage, long nEndPercentage)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		validateStartEndPercentage(nStartPercentage, nEndPercentage);

		m_nHorizontalStartPercentage = nStartPercentage;
		m_nHorizontalEndPercentage = nEndPercentage;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08002");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::GetHorizontalRestriction(long *pnStartPercentage, long *pnEndPercentage)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pnStartPercentage = m_nHorizontalStartPercentage;
		*pnEndPercentage = m_nHorizontalEndPercentage;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08003");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::SetVerticalRestriction(long nStartPercentage, long nEndPercentage)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		validateStartEndPercentage(nStartPercentage, nEndPercentage);

		m_nVerticalStartPercentage = nStartPercentage;
		m_nVerticalEndPercentage = nEndPercentage;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08004");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::GetVerticalRestriction(long *pnStartPercentage, long *pnEndPercentage)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pnStartPercentage = m_nVerticalStartPercentage;
		*pnEndPercentage = m_nVerticalEndPercentage;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08005");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::get_PageSelectionType(EPageSelectionType *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_ePageSelectionType;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09364");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::put_PageSelectionType(EPageSelectionType newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_ePageSelectionType = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09365");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::get_Pattern(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = _bstr_t(m_strPattern.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09366");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::put_Pattern(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		string strPattern = asString(newVal);

		if (strPattern.empty())
		{
			throw UCLIDException("ELI09367", "Please provide non-empty pattern.");
		}

		m_strPattern = strPattern;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09368");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::get_IsRegExp(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = asVariantBool(m_bIsRegExp);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09369");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::put_IsRegExp(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bIsRegExp = asCppBool(newVal);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09370");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::get_IsCaseSensitive(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = asVariantBool(m_bIsCaseSensitive);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09371");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::put_IsCaseSensitive(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bIsCaseSensitive = asCppBool(newVal);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09372");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::get_RegExpPageSelectionType(ERegExpPageSelectionType *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_eRegExpPageSelectionType;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09375");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::put_RegExpPageSelectionType(ERegExpPageSelectionType newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_eRegExpPageSelectionType = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09374");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::get_OCRSelectedRegion(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Provide setting
		*pVal = asVariantBool(m_bOCRSelectedRegion);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12627");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::put_OCRSelectedRegion(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Save setting
		m_bOCRSelectedRegion = asCppBool(newVal);

		// Set flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12628");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::get_SelectedRegionRotation(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Provide setting
		*pVal = m_nRegionRotation;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12629");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::put_SelectedRegionRotation(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Validate new setting
		if ((newVal != 0) && (newVal != 90) && (newVal != 180) && (newVal != 270))
		{
			// Create and throw exception
			UCLIDException ue("ELI12626", "Invalid degree of rotation - must be {0, 90, 180, 270}.");
			ue.addDebugInfo( "Rotation", newVal );
			throw ue;
		}

		// Save setting
		m_nRegionRotation = newVal;

		// Set flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12630");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IAttributFindingRule
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::raw_ParseText(IAFDocument* pAFDoc, IProgressStatus *pProgressStatus, 
											  IIUnknownVector **pAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Check the arguments
		ASSERT_ARGUMENT("ELI23900", pAttributes != NULL);

		// Create local Document pointer
		IAFDocumentPtr ipAFDoc(pAFDoc);
		ASSERT_RESOURCE_ALLOCATION("ELI18452", ipAFDoc != NULL);

		// Get the spatial string
		ISpatialStringPtr ipInputText(ipAFDoc->Text);
		ASSERT_RESOURCE_ALLOCATION("ELI18453", ipInputText != NULL);

		// Create collection of Attributes to return
		IIUnknownVectorPtr ipAttributes(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI18451", ipAttributes != NULL);

		// Only perform selection if the string is spatial
		if (ipInputText->GetMode() == kSpatialMode)
		{
			// Get the last page number
			long nLastPageNumber = ipInputText->GetLastPageNumber();

			// Get specific pages within which to Select Regions
			std::vector<int> vecPageNumbers = getActualPageNumbers( nLastPageNumber, 
				ipInputText, ipAFDoc );

			bool bRestrictionDefined = isRestrictionDefined();
			IIUnknownVectorPtr ipPages = ipInputText->GetPages();

			int nCount = ipPages->Size();

			// Step through each page in the AFDocument, check specification for this page, 
			// and create an Attribute for each non-NULL Spatial String
			int i;
			for (i = 0; i < nCount; i++)
			{
				// Get this page
				ISpatialStringPtr ipPage = ipPages->At(i);
				ASSERT_RESOURCE_ALLOCATION("ELI18572", ipPage != NULL);
				long nPageNum = ipPage->GetFirstPageNumber();

				// Check the page specification
				bool bPageSpecified = find(vecPageNumbers.begin(), vecPageNumbers.end(), 
					nPageNum) != vecPageNumbers.end();

				// Get the appropriate content for this page
				ISpatialStringPtr ipContentFromThisPage = getRegionContent( ipPage, 
					bPageSpecified );

				// Provide non-NULL content to an Attribute to be included in the collection
				if (ipContentFromThisPage != NULL)
				{
					// Create an Attribute
					IAttributePtr ipNewAttribute( CLSID_Attribute );
					ASSERT_RESOURCE_ALLOCATION("ELI18570", ipNewAttribute != NULL);

					// Update the SourceDocName of the Spatial String
					ipContentFromThisPage->SourceDocName = ipInputText->SourceDocName;

					// Set the Attribute Value
					ipNewAttribute->Value = ipContentFromThisPage;

					// Add the Attribute to the collection
					ipAttributes->PushBack( ipNewAttribute );
				}
			}
		}

		// Provide the collected Attributes to the caller
		*pAttributes = ipAttributes.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18447");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IDocumentPreprocessor
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::raw_Process(IAFDocument* pDocument, IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		IAFDocumentPtr ipAFDoc(pDocument);
		ASSERT_RESOURCE_ALLOCATION("ELI19148", ipAFDoc != NULL);

		// get the spatial string
		ISpatialStringPtr ipInputText = ipAFDoc->Text;
		ASSERT_RESOURCE_ALLOCATION("ELI08020", ipInputText != NULL);

		// if the string is not in kSpatialMode return immediately
		if (ipInputText->GetMode() != kSpatialMode)
		{
			return S_OK;
		}

		// Get the last page number
		long nLastPageNumber = ipInputText->GetLastPageNumber();

		// all page numbers the user wants to get
		vector<int> vecActualPageNumbers = getActualPageNumbers(nLastPageNumber, ipInputText, ipAFDoc);

		bool bRestrictionDefined = isRestrictionDefined();
		IIUnknownVectorPtr ipPages = ipInputText->GetPages();

		int nCount = ipPages->Size();

		// Step through each page in the AFDocument, check specification for this page, 
		// and add the appropriate Region Content to the final Spatial String
		ISpatialStringPtr ipResult( CLSID_SpatialString );
		ASSERT_RESOURCE_ALLOCATION("ELI18561", ipResult != NULL);
		int i;
		for (i = 0; i < nCount; i++)
		{
			// Get this page
			ISpatialStringPtr ipPage = ipPages->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI18562", ipPage != NULL);
			long nPageNum = ipPage->GetFirstPageNumber();
	
			// If the page is specifed
			bool bPageSpecified = find(vecActualPageNumbers.begin(), vecActualPageNumbers.end(), 
				nPageNum) != vecActualPageNumbers.end();

			// Get the appropriate content
			ISpatialStringPtr ipContentFromThisPage = getRegionContent( ipPage, 
				bPageSpecified );

			// Append non-NULL content to the result string
			if (ipContentFromThisPage != NULL)
			{
				ipResult->Append( ipContentFromThisPage );
			}
		}

		// Check the result
		if (ipResult->Size == 0)
		{
			// Retrieve the text from the AFDocument
			ISpatialStringPtr ipText = ipAFDoc->Text;
			ASSERT_RESOURCE_ALLOCATION("ELI15537", ipText != NULL);

			// if the returned string is empty
			// clear the text but leave the source document name
			ipText->ReplaceAndDowngradeToNonSpatial("");
		}
		else
		{
			// else just replace the text.
			// the SourceDocName will be retained
			ipAFDoc->Text = ipResult;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07982");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_SelectPageRegion;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		m_bIncludeRegion = true;
		m_strSpecificPages = "";
		m_nHorizontalStartPercentage = -1;
		m_nHorizontalEndPercentage = -1;
		m_nVerticalStartPercentage = -1;
		m_nVerticalEndPercentage = -1;

		m_ePageSelectionType = kSelectAll;
		m_strPattern = "";
		m_bIsRegExp = false;
		m_bIsCaseSensitive = false;
		m_eRegExpPageSelectionType = kSelectAllPagesWithRegExp;

		m_bOCRSelectedRegion = false;
		m_nRegionRotation = -1;

		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read(&nDataLength, sizeof(nDataLength), NULL);
		ByteStream data(nDataLength);
		pStream->Read(data.getData(), nDataLength, NULL);
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// Check for newer version
		if (nDataVersion > gnCurrentVersion)
		{
			// Throw exception
			UCLIDException ue("ELI08017", "Unable to load newer SelectPageRegion component.");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}
		
		dataReader >> m_bIncludeRegion;
		if (nDataVersion < 2)
		{
			bool bSpecificPages;
			dataReader >> bSpecificPages;
			if (bSpecificPages)
			{
				m_ePageSelectionType = kSelectSpecified;
			}
			else
			{
				m_ePageSelectionType = kSelectAll;
			}
		}
		else
		{
			long tmp;
			dataReader >> tmp;
			m_ePageSelectionType = (EPageSelectionType)tmp;
		}

		dataReader >> m_strSpecificPages;

		if (m_ePageSelectionType == kSelectSpecified)
		{
			::validatePageNumbers(m_strSpecificPages);
		}

		if (nDataVersion >= 2)
		{
			long tmp;
			dataReader >> tmp;
			m_eRegExpPageSelectionType = (ERegExpPageSelectionType)tmp;
			dataReader >> m_strPattern;
			dataReader >> m_bIsRegExp;
			dataReader >> m_bIsCaseSensitive;
		}

		dataReader >> m_nHorizontalStartPercentage;
		dataReader >> m_nHorizontalEndPercentage;
		validateStartEndPercentage(m_nHorizontalStartPercentage, m_nHorizontalEndPercentage);

		dataReader >> m_nVerticalStartPercentage;
		dataReader >> m_nVerticalEndPercentage;
		validateStartEndPercentage(m_nVerticalStartPercentage, m_nVerticalEndPercentage);

		// Read OCR of region and associated rotation amount
		if (nDataVersion >= 3)
		{
			dataReader >> m_bOCRSelectedRegion;
			dataReader >> m_nRegionRotation;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07983");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		dataWriter << gnCurrentVersion;
		dataWriter << m_bIncludeRegion;
		
		dataWriter << (long)m_ePageSelectionType;

		dataWriter << m_strSpecificPages;

		dataWriter << (long)m_eRegExpPageSelectionType;
		dataWriter << m_strPattern;
		dataWriter << m_bIsRegExp;
		dataWriter << m_bIsCaseSensitive;

		dataWriter << m_nHorizontalStartPercentage;
		dataWriter << m_nHorizontalEndPercentage;
		dataWriter << m_nVerticalStartPercentage;
		dataWriter << m_nVerticalEndPercentage;

		// Write OCR of region and associated rotation amount
		dataWriter << m_bOCRSelectedRegion;
		dataWriter << m_nRegionRotation;

		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07984");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19561", pstrComponentDescription != NULL)

		*pstrComponentDescription = _bstr_t("Select page region").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07985");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::raw_IsConfigured(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		bool bConfigured = true;

		// Check requirements based on PageSelectionType
		if (m_ePageSelectionType == kSelectAll)
		{
			bConfigured = isRestrictionDefined();
		}
		else if (m_ePageSelectionType == kSelectSpecified)
		{
			if (m_strSpecificPages.empty())
			{
				bConfigured = false;
			}
		}
		else if (m_ePageSelectionType == kSelectWithRegExp)
		{
			if (m_strPattern.empty())
			{
				bConfigured = false;
			}
		}
		else
		{
			bConfigured = false;
		}

		// Check requirements based on OCR Of Selected Region
		if (m_bOCRSelectedRegion)
		{
			// Region Rotation must be between 0 and 360 degrees
			if ((m_nRegionRotation < 0) || (m_nRegionRotation > 360))
			{
				bConfigured = false;
			}
		}

		*pbValue = asVariantBool(bConfigured);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07986");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// If no exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		UCLID_AFPREPROCESSORSLib::ISelectPageRegionPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI08322", ipSource!=NULL);

		m_bIncludeRegion = asCppBool( ipSource->GetIncludeRegionDefined() );

		CComBSTR bstrTmp;
		VARIANT_BOOL bTmp;
		ipSource->GetPageSelections(&bTmp, &bstrTmp);
		m_strSpecificPages = asString(bstrTmp);

		m_ePageSelectionType = (EPageSelectionType)ipSource->GetPageSelectionType();
		m_bIsRegExp = asCppBool( ipSource->GetIsRegExp() );
		m_bIsCaseSensitive = asCppBool( ipSource->GetIsCaseSensitive() );
		m_strPattern = ipSource->Pattern;

		ipSource->GetHorizontalRestriction(&m_nHorizontalStartPercentage, &m_nHorizontalEndPercentage);
		ipSource->GetVerticalRestriction(&m_nVerticalStartPercentage, &m_nVerticalEndPercentage);

		m_eRegExpPageSelectionType = (ERegExpPageSelectionType)ipSource->GetRegExpPageSelectionType();

		// Copy OCR flag and Rotation amount
		m_bOCRSelectedRegion = asCppBool( ipSource->GetOCRSelectedRegion() );
		m_nRegionRotation = ipSource->GetSelectedRegionRotation();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08323");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_SelectPageRegion);
		ASSERT_RESOURCE_ALLOCATION("ELI08338", ipObjCopy != NULL);
		
		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07988");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
vector<int> CSelectPageRegion::getActualPageNumbers(int nLastPageNumber, 
													ISpatialStringPtr ipInputText, 
													IAFDocumentPtr ipAFDoc)
{
	vector<int> vecRet;
	// include region defined
	
	bool bRestrictionDefined = isRestrictionDefined();
	if (m_ePageSelectionType == kSelectAll)
	{
		if (!bRestrictionDefined)
		{
			// if all pages are selected and no restriction is posted, it is invalid
			throw UCLIDException("ELI08023", "You must define restrictions after selecting all pages.");
		}
		int i;
		for(i = 0; i <= nLastPageNumber; i++)
		{
			vecRet.push_back(i);
		}
	}
	else if (m_ePageSelectionType == kSelectSpecified)
	{
		return ::getPageNumbers(nLastPageNumber, m_strSpecificPages);
	}
	else if (m_ePageSelectionType == kSelectWithRegExp)
	{
		vector<int> vecPages;
		if (m_bIsRegExp)
		{
			IMiscUtilsPtr ipMiscUtils(CLSID_MiscUtils);
			ASSERT_RESOURCE_ALLOCATION("ELI13023", ipMiscUtils != NULL );

			// Find with a regular expression pattern
			IRegularExprParserPtr ipRegExpParser = 	ipMiscUtils->GetNewRegExpParserInstance("SelectPageRegion");
			ASSERT_RESOURCE_ALLOCATION("ELI09378", ipRegExpParser != NULL);

			string strRootFolder = "";

			// We eat the exception that will be thrown if the RSDFileDir tag does not exist 
			// because we can still run the regexp providing of course that the regexp contains no
			// #import statements
			try
			{
				string rootTag = "<RSDFileDir>";
				strRootFolder = m_ipAFUtility->ExpandTags(rootTag.c_str(), ipAFDoc );
			}
			catch(...)
			{
			}
			string strRegExp = getRegExpFromText(m_strPattern, strRootFolder, true, gstrAF_AUTO_ENCRYPT_KEY_PATH);

			ipRegExpParser->PutPattern(_bstr_t(strRegExp.c_str()));
			ipRegExpParser->PutIgnoreCase( asVariantBool(!m_bIsCaseSensitive) );

			IIUnknownVectorPtr ipPages = ipInputText->GetPages();
			if(m_eRegExpPageSelectionType == kSelectAllPagesWithRegExp)
			{
				int i;
				for (i = 0; i < ipPages->Size(); i++)
				{
					ISpatialStringPtr ipPage = ipPages->At(i);
					ASSERT_RESOURCE_ALLOCATION("ELI09379", ipPage != NULL);
					IIUnknownVectorPtr ipFound = ipRegExpParser->Find(ipPage->GetString(), 
						VARIANT_TRUE, VARIANT_FALSE);
					if (ipFound->Size() > 0)
					{
						vecPages.push_back(ipPage->GetFirstPageNumber());
					}
				}
			}
			else if(m_eRegExpPageSelectionType == kSelectLeadingPagesWithRegExp)
			{
				int i;
				for (i = 0; i < ipPages->Size(); i++)
				{
					ISpatialStringPtr ipPage = ipPages->At(i);
					ASSERT_RESOURCE_ALLOCATION("ELI19149", ipPage != NULL);
					IIUnknownVectorPtr ipFound = ipRegExpParser->Find(ipPage->GetString(), 
						VARIANT_TRUE, VARIANT_FALSE);
					if (ipFound->Size() > 0)
					{
						vecPages.push_back(ipPage->GetFirstPageNumber());
					}
					else 
					{
						break;
					}
				}
			}
			else if(m_eRegExpPageSelectionType == kSelectTrailingPagesWithRegExp)
			{
				int i;
				for (i = ipPages->Size() - 1; i >= 0; i--)
				{
					ISpatialStringPtr ipPage = ipPages->At(i);
					ASSERT_RESOURCE_ALLOCATION("ELI19150", ipPage != NULL);
					IIUnknownVectorPtr ipFound = ipRegExpParser->Find(ipPage->GetString(), 
						VARIANT_TRUE, VARIANT_FALSE);
					if (ipFound->Size() > 0)
					{
						vecPages.insert(vecPages.begin(), ipPage->GetFirstPageNumber());
					}
					else 
					{
						break;
					}
				}
			}
		}
		else
		{
			// just find normal text pattern
			IIUnknownVectorPtr ipPages = ipInputText->GetPages();

			if(m_eRegExpPageSelectionType == kSelectAllPagesWithRegExp)
			{
				int i;
				for (i = 0; i < ipPages->Size(); i++)
				{
					ISpatialStringPtr ipPage = ipPages->At(i);
					ASSERT_RESOURCE_ALLOCATION("ELI09380", ipPage != NULL);
					string strPage = asString(ipPage->GetString());
					
					string strTmpPattern = m_strPattern;
					if (!m_bIsCaseSensitive)
					{
						makeLowerCase(strTmpPattern);
						makeLowerCase(strPage);
					}
					long pos = strPage.find(strTmpPattern, 0);
					if (pos != string::npos)
					{
						vecPages.push_back(ipPage->GetFirstPageNumber());
					}
				}
			}
			else if(m_eRegExpPageSelectionType == kSelectLeadingPagesWithRegExp)
			{
				int i;
				for (i = 0; i < ipPages->Size(); i++)
				{
					ISpatialStringPtr ipPage = ipPages->At(i);
					ASSERT_RESOURCE_ALLOCATION("ELI19151", ipPage != NULL);
					string strPage = asString(ipPage->GetString());
					
					string strTmpPattern = m_strPattern;
					if (!m_bIsCaseSensitive)
					{
						makeLowerCase(strTmpPattern);
						makeLowerCase(strPage);
					}
					long pos = strPage.find(strTmpPattern, 0);
					if (pos != string::npos)
					{
						vecPages.push_back(ipPage->GetFirstPageNumber());
					}
					else 
					{
						break;
					}
				}
			}
			else if(m_eRegExpPageSelectionType == kSelectTrailingPagesWithRegExp)
			{
				int i;
				for (i = ipPages->Size() - 1; i >= 0; i--)
				{
					ISpatialStringPtr ipPage = ipPages->At(i);
					ASSERT_RESOURCE_ALLOCATION("ELI19152", ipPage != NULL);
					string strPage = asString(ipPage->GetString());
					
					string strTmpPattern = m_strPattern;
					if (!m_bIsCaseSensitive)
					{
						makeLowerCase(strTmpPattern);
						makeLowerCase(strPage);
					}
					long pos = strPage.find(strTmpPattern, 0);
					if (pos != string::npos)
					{
						long nPageNum = ipPage->GetFirstPageNumber();
						vecPages.insert(vecPages.begin(), nPageNum);
					}
					else 
					{
						break;
					}
				}
			}
		}

		return vecPages;
	}

	return vecRet;
}
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr CSelectPageRegion::getIndividualPageContent(ISpatialStringPtr ipOriginPage)
{
	try
	{
		// if no restriction is defined, assume that inclusion/exclusion has
		// already been taken care of by the caller
		if (!isRestrictionDefined())
		{
			// just return the original string
			return ipOriginPage;
		}

		if (m_ipSpatialStringSearcher == NULL)
		{
			m_ipSpatialStringSearcher.CreateInstance(CLSID_SpatialStringSearcher);
			ASSERT_RESOURCE_ALLOCATION("ELI08022", m_ipSpatialStringSearcher != NULL);
			m_ipSpatialStringSearcher->SetIncludeDataOnBoundary(VARIANT_TRUE);
			m_ipSpatialStringSearcher->SetBoundaryResolution(kCharacter);
		}

		m_ipSpatialStringSearcher->InitSpatialStringSearcher(ipOriginPage);

		ISpatialStringPtr ipResult(NULL);

		long nWidth = -1;
		long nHeight = -1;

		// Get the width and height of the page using the page info
		// this will give us the real page boundaries as they relate to letter positions
		long nPageNum = ipOriginPage->GetFirstPageNumber();
		ISpatialPageInfoPtr ipPageInfo = ipOriginPage->GetPageInfo(nPageNum);
		if(ipPageInfo == NULL)
		{
			UCLIDException ue("ELI10502", "Unable to obtain spatial page info.");
			ue.addDebugInfo("Page Number", nPageNum);
			throw ue;
		}

		// Get the width and height of the page
		ipPageInfo->GetWidthAndHeight(&nWidth, &nHeight);

		// get current page's boundary
		ILongRectanglePtr ipRect( CLSID_LongRectangle );
		ASSERT_RESOURCE_ALLOCATION("ELI08021", ipRect != NULL);
		ipRect->SetBounds(0, 0, nWidth, nHeight);

		// if left and right boundaries are defined
		if (m_nHorizontalStartPercentage >= 0 && m_nHorizontalEndPercentage >= 0)
		{
			// record the left most boundary
			ipRect->Left = (long)floor((double)nWidth * (double)m_nHorizontalStartPercentage/100.0 + 0.5);
			ipRect->Right = (long)floor((double)nWidth * (double)m_nHorizontalEndPercentage/100.0 + 0.5);
		}

		// if top and bottom boundaries are defined
		if (m_nVerticalStartPercentage >= 0 && m_nVerticalEndPercentage >= 0)
		{
			// record top most boundary
			ipRect->Top = (long)floor((double)nHeight * (double)m_nVerticalStartPercentage/100.0 + 0.5);
			ipRect->Bottom = (long)floor((double)nHeight * (double)m_nVerticalEndPercentage/100.0 + 0.5);
		}

		// Get extension of source file
		string strPath = asString( ipOriginPage->SourceDocName );
		string strExt = getExtensionFromFullPath( strPath.c_str() );

		// calculate the region
		if (m_bIncludeRegion)
		{
			// Search the previously OCR'd text
			if (!m_bOCRSelectedRegion)
			{
				// Rotate the rectangle per OCR results
				ipResult = m_ipSpatialStringSearcher->GetDataInRegion( ipRect, VARIANT_TRUE );
			}
			// Do new OCR after rotation
			else
			{
				// Handle 0 degree rotation in special way
				int nActualRotation = m_nRegionRotation;
				if (nActualRotation == 0)
				{
					// RecognizeTextInImageZone() interprets 
					//		  0 = automatic rotation
					//		360 = no rotation
					nActualRotation = 360;
				}

				// Get the text from specified area after rotation
				ipResult = getOCREngine()->RecognizeTextInImageZone(strPath.c_str(), 
					nPageNum, nPageNum, ipRect, nActualRotation, kNoFilter, "", VARIANT_FALSE, 
					VARIANT_FALSE, VARIANT_TRUE, NULL );
				ASSERT_RESOURCE_ALLOCATION( "ELI12698", ipResult != NULL );
			}
		}
		else
		{
			// Search the previously OCR'd text
			if (!m_bOCRSelectedRegion)
			{
				ipResult = m_ipSpatialStringSearcher->GetDataOutOfRegion(ipRect);
			}
			else
			{
				// Whiten the specified zone on this page - to a temporary file
				TemporaryFileName tmpFile2( NULL, strExt.c_str(), true );
				string strTempFileName2 = tmpFile2.getName();
				string strSource = asString( ipOriginPage->GetSourceDocName() );
				long nLeft(-1), nTop(-1), nRight(-1), nBottom(-1);
				ipRect->GetBounds(&nLeft, &nTop, &nRight, &nBottom);
				fillImageArea(strSource.c_str(), strTempFileName2.c_str(), nLeft, nTop, 
					nRight, nBottom, nPageNum, RGB(255,255,255), true, false);

				// Handle 0 degree rotation in special way
				int nActualRotation = m_nRegionRotation;
				if (nActualRotation == 0)
				{
					// RecognizeTextInImageZone() interprets 
					//		  0 = automatic rotation
					//		360 = no rotation
					nActualRotation = 360;
				}

				// Get the text from entire remaining area
				ipResult = getOCREngine()->RecognizeTextInImageZone(strTempFileName2.c_str(), 1, -1, 
					NULL, nActualRotation, kNoFilter, "", VARIANT_FALSE, VARIANT_FALSE, VARIANT_TRUE, 
					NULL);

				// Assign original filename to Spatial String
				ipResult->SourceDocName = strPath.c_str();
			}
		}

		return ipResult;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26878");
}
//-------------------------------------------------------------------------------------------------
IOCREnginePtr CSelectPageRegion::getOCREngine()
{
	// create a new OCR engine every time this function is called [P13 #2909]
	IOCREnginePtr ipOCREngine( CLSID_ScansoftOCR );
	ASSERT_RESOURCE_ALLOCATION( "ELI12688", ipOCREngine != NULL );

	// license OCR engine
	IPrivateLicensedComponentPtr ipScansoftEngine(ipOCREngine);
	ASSERT_RESOURCE_ALLOCATION("ELI20469", ipScansoftEngine != NULL);
	ipScansoftEngine->InitPrivateLicense(LICENSE_MGMT_PASSWORD.c_str());

	return ipOCREngine;
}
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr CSelectPageRegion::getRegionContent(ISpatialStringPtr ipPageText, 
													  bool bPageSpecified)
{
	// Check for defined restriction
	bool bRestrictionDefined = isRestrictionDefined();

	// Get the desired Spatial String portion for this page depending on:
	//	bRestrictionDefined - whether or not a subregion has been defined
	//	m_bIncludeRegion - this page or region is being included or excluded
	//	bPageSpecified - this page has been chosen by the caller
	ISpatialStringPtr ipSS = NULL;
	if (!bRestrictionDefined && m_bIncludeRegion && bPageSpecified)
	{
		// No restriction, Include, Page Is specified ---> provide entire page
		ipSS = ipPageText;
	}
	else if (bRestrictionDefined && m_bIncludeRegion && bPageSpecified)
	{
		// Restriction, Include, Page Is specified ---> provide desired region
		ipSS = getIndividualPageContent( ipPageText );
	}
	else if (!bRestrictionDefined && !m_bIncludeRegion && bPageSpecified)
	{
		// No restriction, Exclude, Page Is specified ---> do NOT provide page
		ipSS = NULL;
	}
	else if (bRestrictionDefined && !m_bIncludeRegion && bPageSpecified)
	{
		// No restriction, Include, Page Is specified ---> provide entire page
		ipSS = getIndividualPageContent( ipPageText );
	}
	else if (!bRestrictionDefined && m_bIncludeRegion && !bPageSpecified)
	{
		// No restriction, Include, Page Not specified ---> do NOT provide page
		ipSS = NULL;
	}
	else if (bRestrictionDefined && m_bIncludeRegion && !bPageSpecified)
	{
		// Restriction, Include, Page Not specified ---> do NOT provide page
		ipSS = NULL;
	}
	else if (!bRestrictionDefined && !m_bIncludeRegion && !bPageSpecified)
	{
		// No restriction, Exclude, Page Not specified ---> provide entire page
		ipSS = ipPageText;
	}
	else if (bRestrictionDefined && !m_bIncludeRegion && !bPageSpecified)
	{
		// Restriction, Exclude, Page Not specified ---> provide entire page
		ipSS = ipPageText;
	}

	// Return the appropriate Spatial String
	return ipSS;
}
//-------------------------------------------------------------------------------------------------
bool CSelectPageRegion::isRestrictionDefined()
{
	if ((m_nHorizontalStartPercentage >= 0 && m_nHorizontalEndPercentage >= 0)
		|| (m_nVerticalStartPercentage >= 0 && m_nVerticalEndPercentage >= 0))
	{
		return true;
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
void CSelectPageRegion::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( THIS_COMPONENT_ID, "ELI07989", "Select Page Region" );
}
//-------------------------------------------------------------------------------------------------
void CSelectPageRegion::validateStartEndPercentage(long nStartPercentage, long nEndPercentage)
{
	// this means that the restriction is undefined
	if (nStartPercentage < 0 && nEndPercentage < 0)
	{
		return;
	}

	if (nStartPercentage > 100)
	{
		UCLIDException ue("ELI08008", "Invalid starting percentage.");
		ue.addDebugInfo("Start Percentage", nStartPercentage);
		throw ue;
	}

	if (nEndPercentage == 0 || nEndPercentage > 100)
	{
		UCLIDException ue("ELI08009", "Invalid ending percentage.");
		ue.addDebugInfo("End Percentage", nEndPercentage);
		throw ue;
	}

	if (nEndPercentage <= nStartPercentage)
	{
		UCLIDException ue("ELI08010", "Starting percentage shall not be greater than or equal to ending percentage.");
		ue.addDebugInfo("Start Percentage", nStartPercentage);
		ue.addDebugInfo("End Percentage", nEndPercentage);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
