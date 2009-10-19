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
#include <LeadtoolsBitmapFreeer.h>
#include <ComponentLicenseIDs.h>

#include <algorithm>
#include <math.h>

// current version
// Version 4 - Added ability to choose what is returned, whether it be the existing
//			   text, text from a ReOCR of the region, or just the spatial region with
//			   specified text assigned to it.
const unsigned long gnCurrentVersion = 4;

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
  m_eReturnType(kReturnText),
  m_bIncludeIntersectingText(true),
  m_eTextIntersectionType(kCharacter),
  m_strTextToAssign(""),
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
		m_ipSpatialStringSearcher = NULL;
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
STDMETHODIMP CSelectPageRegion::get_SpecificPages(BSTR *pbstrSpecificPages)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI28104", pbstrSpecificPages != NULL);

		*pbstrSpecificPages = _bstr_t(m_strSpecificPages.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28105");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::put_SpecificPages(BSTR bstrSpecificPages)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		string strPages = asString(bstrSpecificPages);

		// validate the string format
		::validatePageNumbers(strPages);

		m_strSpecificPages = strPages;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28106");
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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08002");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::GetHorizontalRestriction(long *pnStartPercentage, long *pnEndPercentage)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		ASSERT_ARGUMENT("ELI28107", pnStartPercentage != NULL);
		ASSERT_ARGUMENT("ELI28108", pnEndPercentage != NULL);

		*pnStartPercentage = m_nHorizontalStartPercentage;
		*pnEndPercentage = m_nHorizontalEndPercentage;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08003");
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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08004");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::GetVerticalRestriction(long *pnStartPercentage, long *pnEndPercentage)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		ASSERT_ARGUMENT("ELI28109", pnStartPercentage != NULL);
		ASSERT_ARGUMENT("ELI28110", pnEndPercentage != NULL);

		*pnStartPercentage = m_nVerticalStartPercentage;
		*pnEndPercentage = m_nVerticalEndPercentage;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08005");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::get_PageSelectionType(EPageSelectionType *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		ASSERT_ARGUMENT("ELI28111", pVal != NULL);

		*pVal = m_ePageSelectionType;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09364");
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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09365");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::get_Pattern(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI28112", pVal != NULL);

		*pVal = _bstr_t(m_strPattern.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09366");
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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09368");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::get_IsRegExp(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI28113", pVal != NULL);

		*pVal = asVariantBool(m_bIsRegExp);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09369");
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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09370");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::get_IsCaseSensitive(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		ASSERT_ARGUMENT("ELI28114", pVal != NULL);

		*pVal = asVariantBool(m_bIsCaseSensitive);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09371");
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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09372");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::get_RegExpPageSelectionType(ERegExpPageSelectionType *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI28115", pVal != NULL);

		*pVal = m_eRegExpPageSelectionType;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09375");
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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09374");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::get_SelectPageRegionReturnType(ESelectPageRegionReturnType* pReturnType)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI28116", pReturnType != NULL);

		*pReturnType = m_eReturnType;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28117");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::put_SelectPageRegionReturnType(ESelectPageRegionReturnType returnType)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_eReturnType = returnType;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28118");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::get_SelectedRegionRotation(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI28119", pVal != NULL);

		// Provide setting
		*pVal = m_nRegionRotation;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12629");
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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12630");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::get_IncludeIntersectingText(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI28093", pVal != NULL);

		*pVal = asVariantBool(m_bIncludeIntersectingText);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28094");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::put_IncludeIntersectingText(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bIncludeIntersectingText = asCppBool(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28095");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::get_TextIntersectionType(ESpatialEntity* pIntersectionType)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI28096", pIntersectionType != NULL);

		*pIntersectionType = m_eTextIntersectionType;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28097");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::put_TextIntersectionType(ESpatialEntity intersectionType)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_eTextIntersectionType = intersectionType;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28098");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::get_TextToAssignToRegion(BSTR* pbstrTextToAssign)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI28099", pbstrTextToAssign != NULL);

		*pbstrTextToAssign = _bstr_t(m_strTextToAssign.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28100");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::put_TextToAssignToRegion(BSTR bstrTextToAssign)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_strTextToAssign = asString(bstrTextToAssign);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28101");
}

//-------------------------------------------------------------------------------------------------
// IAttributFindingRule
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::raw_ParseText(IAFDocument* pAFDoc, IProgressStatus *pProgressStatus, 
											  IIUnknownVector **pAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

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

		// Get the source document name from the spatial string
		string strSourceDoc = asString(ipInputText->SourceDocName);

		// Create collection of Attributes to return
		IIUnknownVectorPtr ipAttributes(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI18451", ipAttributes != NULL);

		// Get the spatial mode of the string
		ESpatialStringMode eMode = ipInputText->GetMode();

		// Get the last page number
		long nLastPageNumber = eMode == kNonSpatialMode ?
			getNumberOfPagesInImage(strSourceDoc) : ipInputText->GetLastPageNumber();

		// Get specific pages within which to Select Regions
		vector<int> vecPageNumbers = getActualPageNumbers( nLastPageNumber, 
			ipInputText, ipAFDoc );

		bool bRestrictionDefined = isRestrictionDefined();
		if (eMode == kNonSpatialMode)
		{
			for (long i = 1; i <= nLastPageNumber; i++)
			{
				int nHeight(-1), nWidth(-1);
				getImagePixelHeightAndWidth(strSourceDoc, nHeight, nWidth, i);

				// Check the page specification
				bool bPageSpecified = find(vecPageNumbers.begin(), vecPageNumbers.end(), 
					i) != vecPageNumbers.end();

				// Get the appropriate content for this page
				ISpatialStringPtr ipContentFromThisPage = getRegionContent( ipInputText, 
					bPageSpecified, bRestrictionDefined, i, nWidth, nHeight);

				// Provide non-NULL content to an Attribute to be included in the collection
				if (ipContentFromThisPage != NULL)
				{
					// Create an Attribute
					IAttributePtr ipNewAttribute( CLSID_Attribute );
					ASSERT_RESOURCE_ALLOCATION("ELI28166", ipNewAttribute != NULL);

					// Update the SourceDocName of the Spatial String
					ipContentFromThisPage->SourceDocName = strSourceDoc.c_str();

					// Set the Attribute Value
					ipNewAttribute->Value = ipContentFromThisPage;

					// Add the Attribute to the collection
					ipAttributes->PushBack( ipNewAttribute );
				}
			}
		}
		else
		{
			IIUnknownVectorPtr ipPages = ipInputText->GetPages();
			ASSERT_RESOURCE_ALLOCATION("ELI28167", ipPages != NULL);

			int nCount = ipPages->Size();

			// Step through each page in the AFDocument, check specification for this page, 
			// and create an Attribute for each non-NULL Spatial String
			for (long i = 0; i < nCount; i++)
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
					bPageSpecified, bRestrictionDefined );

				// Provide non-NULL content to an Attribute to be included in the collection
				if (ipContentFromThisPage != NULL)
				{
					// Create an Attribute
					IAttributePtr ipNewAttribute( CLSID_Attribute );
					ASSERT_RESOURCE_ALLOCATION("ELI18570", ipNewAttribute != NULL);

					// Update the SourceDocName of the Spatial String
					ipContentFromThisPage->SourceDocName = strSourceDoc.c_str();

					// Set the Attribute Value
					ipNewAttribute->Value = ipContentFromThisPage;

					// Add the Attribute to the collection
					ipAttributes->PushBack( ipNewAttribute );
				}
			}
		}

		// Provide the collected Attributes to the caller
		*pAttributes = ipAttributes.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18447");
}

//-------------------------------------------------------------------------------------------------
// IDocumentPreprocessor
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::raw_Process(IAFDocument* pDocument, IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		IAFDocumentPtr ipAFDoc(pDocument);
		ASSERT_RESOURCE_ALLOCATION("ELI19148", ipAFDoc != NULL);

		// get the spatial string
		ISpatialStringPtr ipInputText = ipAFDoc->Text;
		ASSERT_RESOURCE_ALLOCATION("ELI08020", ipInputText != NULL);

		// Get the source doc name
		string strSourceDoc = asString(ipInputText->SourceDocName);

		// Get the spatial mode of the string
		ESpatialStringMode eMode = ipInputText->GetMode();

		// Get the last page number
		long nLastPageNumber = eMode == kNonSpatialMode ?
			getNumberOfPagesInImage(strSourceDoc) : ipInputText->GetLastPageNumber();

		// Get specific pages within which to Select Regions
		vector<int> vecActualPageNumbers = getActualPageNumbers( nLastPageNumber, 
			ipInputText, ipAFDoc );

		bool bRestrictionDefined = isRestrictionDefined();

		ISpatialStringPtr ipResult( CLSID_SpatialString );
		ASSERT_RESOURCE_ALLOCATION("ELI18561", ipResult != NULL);
		// Step through each page of the document
		if (eMode == kNonSpatialMode)
		{
			for (long i=1; i <= nLastPageNumber; i++)
			{
				int nWidth(-1), nHeight(-1);
				getImagePixelHeightAndWidth(strSourceDoc, nHeight, nWidth, i);

				// If the page is specifed
				bool bPageSpecified = find(vecActualPageNumbers.begin(), vecActualPageNumbers.end(), 
					i) != vecActualPageNumbers.end();

				ISpatialStringPtr ipContentFromThisPage = getRegionContent(ipInputText,
					bPageSpecified, bRestrictionDefined, i, nWidth, nHeight);

				// Append non-NULL content to the result string
				if (ipContentFromThisPage != NULL)
				{
					ipResult->Append( ipContentFromThisPage );
				}
			}
		}
		// Step through each page in the AFDocument, check specification for this page, 
		// and add the appropriate Region Content to the final Spatial String
		else
		{
			IIUnknownVectorPtr ipPages = ipInputText->GetPages();

			long nCount = ipPages->Size();

			for (long i = 0; i < nCount; i++)
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
					bPageSpecified, bRestrictionDefined );

				// Append non-NULL content to the result string
				if (ipContentFromThisPage != NULL)
				{
					ipResult->Append( ipContentFromThisPage );
				}
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
			// Ensure the source doc name is preserved
			ipResult->SourceDocName = strSourceDoc.c_str();

			// Replace the text
			ipAFDoc->Text = ipResult;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07982");
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

		m_eReturnType = kReturnText;
		m_bIncludeIntersectingText = true;
		m_eTextIntersectionType = kCharacter;
		m_nRegionRotation = -1;
		m_strTextToAssign = "";

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
		if (nDataVersion == 3)
		{
			bool bTemp;
			dataReader >> bTemp;
			dataReader >> m_nRegionRotation;

			if (bTemp)
			{
				m_eReturnType = kReturnReOcr;
			}
		}
		else if (nDataVersion >= 4)
		{
			// Read the return type
			long tmp;
			dataReader >> tmp;
			m_eReturnType = (ESelectPageRegionReturnType) tmp;

			switch(m_eReturnType)
			{
			case kReturnText:
				{
					dataReader >> m_bIncludeIntersectingText;
					dataReader >> tmp;
					m_eTextIntersectionType = (ESpatialEntity) tmp;
				}
				break;

			case kReturnReOcr:
				{
					dataReader >> m_nRegionRotation;
				}
				break;

			case kReturnImageRegion:
				{
					dataReader >> m_strTextToAssign;
				}
				break;

			default:
				THROW_LOGIC_ERROR_EXCEPTION("ELI28102");
			}
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

		// Write return type and data associated with the return type
		dataWriter << (long)m_eReturnType;
		switch(m_eReturnType)
		{
		case kReturnText:
			{
				dataWriter << m_bIncludeIntersectingText;
				dataWriter << (long)m_eTextIntersectionType;
			}
			break;

		case kReturnReOcr:
			{
				dataWriter << m_nRegionRotation;
			}
			break;

		case kReturnImageRegion:
			{
				dataWriter << m_strTextToAssign;
			}
			break;

		default:
			THROW_LOGIC_ERROR_EXCEPTION("ELI28103");
		}

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

		// Only need to check return type settings if the object is properly
		// configured up to this point
		if (bConfigured)
		{
			switch(m_eReturnType)
			{
			case kReturnReOcr:
				{
					// Region Rotation must be between 0 and 360 degrees
					if ((m_nRegionRotation < 0) || (m_nRegionRotation > 360))
					{
						bConfigured = false;
					}
				}
				break;

			case kReturnImageRegion:
				{
					bConfigured = !m_strTextToAssign.empty();
				}
				break;
			}
		}

		*pbValue = asVariantBool(bConfigured);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07986");
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

		// Copy the values from the other object
		m_bIncludeRegion = asCppBool( ipSource->IncludeRegionDefined );
		m_strSpecificPages = asString(ipSource->SpecificPages);
		m_ePageSelectionType = (EPageSelectionType)ipSource->PageSelectionType;
		m_bIsRegExp = asCppBool( ipSource->IsRegExp );
		m_bIsCaseSensitive = asCppBool( ipSource->IsCaseSensitive );
		m_strPattern = asString(ipSource->Pattern);
		ipSource->GetHorizontalRestriction(&m_nHorizontalStartPercentage, &m_nHorizontalEndPercentage);
		ipSource->GetVerticalRestriction(&m_nVerticalStartPercentage, &m_nVerticalEndPercentage);
		m_eRegExpPageSelectionType = (ERegExpPageSelectionType)ipSource->RegExpPageSelectionType;
		m_eReturnType = (ESelectPageRegionReturnType)ipSource->SelectPageRegionReturnType;

		// Get the appropriate values for the return type
		switch(m_eReturnType)
		{
		case kReturnText:
			{
				m_bIncludeIntersectingText = asCppBool(ipSource->IncludeIntersectingText);
				m_eTextIntersectionType = (ESpatialEntity)ipSource->TextIntersectionType;
			}
			break;

		case kReturnReOcr:
			{
				m_nRegionRotation = ipSource->SelectedRegionRotation;
			}
			break;

		case kReturnImageRegion:
			{
				m_strTextToAssign = asString(ipSource->TextToAssignToRegion);
			}
			break;

		default:
			THROW_LOGIC_ERROR_EXCEPTION("ELI28120");
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08323");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectPageRegion::raw_Clone(IUnknown** ppObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ASSERT_ARGUMENT("ELI28121", ppObject != NULL);

		ICopyableObjectPtr ipObjCopy(CLSID_SelectPageRegion);
		ASSERT_RESOURCE_ALLOCATION("ELI08338", ipObjCopy != NULL);
		
		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*ppObject = ipObjCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07988");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
vector<int> CSelectPageRegion::getActualPageNumbers(int nLastPageNumber, 
													const ISpatialStringPtr& ipInputText, 
													const IAFDocumentPtr& ipAFDoc)
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

			ipRegExpParser->Pattern = strRegExp.c_str();
			ipRegExpParser->IgnoreCase = asVariantBool(!m_bIsCaseSensitive);

			IIUnknownVectorPtr ipPages = ipInputText->GetPages();
			ASSERT_RESOURCE_ALLOCATION("ELI28168", ipPages != NULL);

			// Get the number of pages
			long lSize = ipPages->Size();
			if(m_eRegExpPageSelectionType == kSelectAllPagesWithRegExp)
			{
				for (long i = 0; i < lSize; i++)
				{
					ISpatialStringPtr ipPage = ipPages->At(i);
					ASSERT_RESOURCE_ALLOCATION("ELI09379", ipPage != NULL);
					IIUnknownVectorPtr ipFound = ipRegExpParser->Find(ipPage->String, 
						VARIANT_TRUE, VARIANT_FALSE);
					ASSERT_RESOURCE_ALLOCATION("ELI28169", ipFound != NULL);
					if (ipFound->Size() > 0)
					{
						vecPages.push_back(ipPage->GetFirstPageNumber());
					}
				}
			}
			else if(m_eRegExpPageSelectionType == kSelectLeadingPagesWithRegExp)
			{
				for (long i = 0; i < lSize; i++)
				{
					ISpatialStringPtr ipPage = ipPages->At(i);
					ASSERT_RESOURCE_ALLOCATION("ELI19149", ipPage != NULL);
					IIUnknownVectorPtr ipFound = ipRegExpParser->Find(ipPage->String, 
						VARIANT_TRUE, VARIANT_FALSE);
					ASSERT_RESOURCE_ALLOCATION("ELI28170", ipFound != NULL);
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
				for (long i = lSize - 1; i >= 0; i--)
				{
					ISpatialStringPtr ipPage = ipPages->At(i);
					ASSERT_RESOURCE_ALLOCATION("ELI19150", ipPage != NULL);
					IIUnknownVectorPtr ipFound = ipRegExpParser->Find(ipPage->GetString(), 
						VARIANT_TRUE, VARIANT_FALSE);
					ASSERT_RESOURCE_ALLOCATION("ELI28171", ipFound != NULL);
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
			ASSERT_RESOURCE_ALLOCATION("ELI28172", ipPages != NULL);

			long lSize = ipPages->Size();
			if(m_eRegExpPageSelectionType == kSelectAllPagesWithRegExp)
			{
				for (long i = 0; i < lSize; i++)
				{
					ISpatialStringPtr ipPage = ipPages->At(i);
					ASSERT_RESOURCE_ALLOCATION("ELI09380", ipPage != NULL);
					string strPage = asString(ipPage->String);
					
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
				for (long i = 0; i < lSize; i++)
				{
					ISpatialStringPtr ipPage = ipPages->At(i);
					ASSERT_RESOURCE_ALLOCATION("ELI19151", ipPage != NULL);
					string strPage = asString(ipPage->String);
					
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
				for (long i = lSize - 1; i >= 0; i--)
				{
					ISpatialStringPtr ipPage = ipPages->At(i);
					ASSERT_RESOURCE_ALLOCATION("ELI19152", ipPage != NULL);
					string strPage = asString(ipPage->String);
					
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
ISpatialStringPtr CSelectPageRegion::getIndividualPageContent(const ISpatialStringPtr& ipOriginPage,
															  long nPageNum, long nWidth, long nHeight)
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
		}

		m_ipSpatialStringSearcher->InitSpatialStringSearcher(ipOriginPage);

		ISpatialStringPtr ipResult(NULL);

		// Check whether the string is spatial or not
		bool bHasSpatialInfo = asCppBool(ipOriginPage->HasSpatialInfo());

		// Use the spatial string to get the boundaries if the string is spatial
		// otherwise just use the width and height passed in
		if (bHasSpatialInfo)
		{
			// Get the width and height of the page using the page info
			// this will give us the real page boundaries as they relate to letter positions
			nPageNum = ipOriginPage->GetFirstPageNumber();
			ISpatialPageInfoPtr ipPageInfo = ipOriginPage->GetPageInfo(nPageNum);
			if(ipPageInfo == NULL)
			{
				UCLIDException ue("ELI10502", "Unable to obtain spatial page info.");
				ue.addDebugInfo("Page Number", nPageNum);
				throw ue;
			}

			// Get the width and height of the page
			ipPageInfo->GetWidthAndHeight(&nWidth, &nHeight);
		}

		// Compute current page's boundary
		long nLeft(0), nTop(0), nRight(nWidth), nBottom(nHeight);

		// if left and right boundaries are defined
		if (m_nHorizontalStartPercentage >= 0 && m_nHorizontalEndPercentage >= 0)
		{
			// record the left most boundary
			nLeft = (long)floor((double)nWidth * (double)m_nHorizontalStartPercentage/100.0 + 0.5);
			nRight = (long)floor((double)nWidth * (double)m_nHorizontalEndPercentage/100.0 + 0.5);
		}

		// if top and bottom boundaries are defined
		if (m_nVerticalStartPercentage >= 0 && m_nVerticalEndPercentage >= 0)
		{
			// record top most boundary
			nTop = (long)floor((double)nHeight * (double)m_nVerticalStartPercentage/100.0 + 0.5);
			nBottom = (long)floor((double)nHeight * (double)m_nVerticalEndPercentage/100.0 + 0.5);
		}
		// Set a long rectangle with the boundaries
		ILongRectanglePtr ipRect( CLSID_LongRectangle );
		ASSERT_RESOURCE_ALLOCATION("ELI08021", ipRect != NULL);
		ipRect->SetBounds(nLeft, nTop, nRight, nBottom);

		// Get extension of source file
		string strPath = asString( ipOriginPage->SourceDocName );
		string strExt = getExtensionFromFullPath( strPath );

		// calculate the region
		switch(m_eReturnType)
		{
		case kReturnText:
			{
				m_ipSpatialStringSearcher->SetIncludeDataOnBoundary(asVariantBool(m_bIncludeIntersectingText));
				m_ipSpatialStringSearcher->SetBoundaryResolution(m_eTextIntersectionType);
				if (m_bIncludeRegion)
				{
					// Rotate the rectangle per OCR results
					ipResult = m_ipSpatialStringSearcher->GetDataInRegion( ipRect, VARIANT_TRUE );
				}
				else
				{
					ipResult = m_ipSpatialStringSearcher->GetDataOutOfRegion(ipRect);
				}
				ASSERT_RESOURCE_ALLOCATION( "ELI28124", ipResult != NULL );
			}
			break;

		case kReturnReOcr:
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

				if (m_bIncludeRegion)
				{
					// Get the text from specified area after rotation
					ipResult = getOCREngine()->RecognizeTextInImageZone(strPath.c_str(), 
						nPageNum, nPageNum, ipRect, nActualRotation, kNoFilter, "", VARIANT_FALSE, 
						VARIANT_FALSE, VARIANT_TRUE, NULL );
					ASSERT_RESOURCE_ALLOCATION( "ELI12698", ipResult != NULL );
				}
				else
				{
					// Whiten the specified zone on this page - to a temporary file
					TemporaryFileName tmpFile2( NULL, strExt.c_str(), true );
					string strTempFileName2 = tmpFile2.getName();
					excludeImageZone(strPath, strTempFileName2, nPageNum, nLeft, nTop, nRight, nBottom);

					// Get the text from entire remaining area on the page
					ipResult = getOCREngine()->RecognizeTextInImageZone(strTempFileName2.c_str(), 1, 1, 
						NULL, nActualRotation, kNoFilter, "", VARIANT_FALSE, VARIANT_FALSE, VARIANT_TRUE, 
						NULL);
					ASSERT_RESOURCE_ALLOCATION( "ELI28127", ipResult != NULL );

					// Assign original filename to Spatial String
					ipResult->SourceDocName = strPath.c_str();
				}
			}
			break;

		case kReturnImageRegion:
			{
				// Create the new spatial string
				ipResult.CreateInstance(CLSID_SpatialString);
				ASSERT_RESOURCE_ALLOCATION( "ELI28125", ipResult != NULL );

				ILongToObjectMapPtr ipPageInfos = NULL;
				if (bHasSpatialInfo)
				{
					// Clone the page infos for the original object
					ICopyableObjectPtr ipCopier = ipOriginPage->SpatialPageInfos;
					ASSERT_RESOURCE_ALLOCATION("ELI28134", ipCopier != NULL);
					ipPageInfos = ipCopier->Clone();
					ASSERT_RESOURCE_ALLOCATION("ELI28135", ipPageInfos != NULL);
				}
				else
				{
					// Create a spatial page info for this page
					ISpatialPageInfoPtr ipPageInfo(CLSID_SpatialPageInfo);
					ASSERT_RESOURCE_ALLOCATION("ELI28173", ipPageInfo != NULL);
					ipPageInfo->SetPageInfo(nWidth, nHeight, kRotNone, 0.0);

					ipPageInfos.CreateInstance(CLSID_LongToObjectMap);
					ASSERT_RESOURCE_ALLOCATION("ELI28174", ipPageInfos != NULL);
					ipPageInfos->Set(nPageNum, ipPageInfo);
				}

				if (m_bIncludeRegion)
				{
					// Create a pseudo-spatial string
					IRasterZonePtr ipZone(CLSID_RasterZone);
					ASSERT_RESOURCE_ALLOCATION("ELI28136", ipZone != NULL);
					ipZone->CreateFromLongRectangle(ipRect, nPageNum);
					ipResult->CreatePseudoSpatialString(ipZone, m_strTextToAssign.c_str(),
						strPath.c_str(), ipPageInfos);
				}
				else
				{
					IIUnknownVectorPtr ipZones = buildRasterZonesForExcludedRegion(nLeft,
						nTop, nRight, nBottom, nWidth, nHeight, nPageNum);
					ASSERT_RESOURCE_ALLOCATION("ELI28137", ipZones != NULL);

					// If only 1 zone create a pseudo-spatial string
					long lSize = ipZones->Size();
					if (lSize == 1)
					{
						IRasterZonePtr ipZone = ipZones->At(0);
						ASSERT_RESOURCE_ALLOCATION("ELI28162", ipZone != NULL);
						ipResult->CreatePseudoSpatialString(ipZone,
							m_strTextToAssign.c_str(), strPath.c_str(), ipPageInfos);
					}
					// Create a hybrid string if there is more than 1 zone
					else if (lSize > 1)
					{
						// Create a new hybrid string
						ipResult->CreateHybridString(ipZones, m_strTextToAssign.c_str(),
							strPath.c_str(), ipPageInfos);
					}
					// Leave the string empty if there are no zones
				}
			}
			break;

		default:
			THROW_LOGIC_ERROR_EXCEPTION("ELI28123");
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
ISpatialStringPtr CSelectPageRegion::getRegionContent(const ISpatialStringPtr& ipPageText, 
													  bool bPageSpecified, bool bRestrictionDefined,
													  long nPageNum, long nWidth, long nHeight)
{
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
		ipSS = getIndividualPageContent( ipPageText, nPageNum, nWidth, nHeight );
	}
	else if (!bRestrictionDefined && !m_bIncludeRegion && bPageSpecified)
	{
		// No restriction, Exclude, Page Is specified ---> do NOT provide page
		ipSS = NULL;
	}
	else if (bRestrictionDefined && !m_bIncludeRegion && bPageSpecified)
	{
		// No restriction, Include, Page Is specified ---> provide entire page
		ipSS = getIndividualPageContent( ipPageText, nPageNum, nWidth, nHeight );
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
void CSelectPageRegion::excludeImageZone(const string& strSourceImage, const string& strTempImage,
										 long nPageNumber, long nLeft, long nTop, long nRight,
										 long nBottom)
{
	try
	{
		// Build the page raster zone
		PageRasterZone zone;
		zone.m_nPage = nPageNumber;
		zone.m_nStartX = nLeft;
		zone.m_nEndX = nRight;
		zone.m_nStartY = zone.m_nEndY = (nBottom - nTop) / 2;
		zone.m_nHeight = nBottom - nTop;
		zone.m_crFillColor = zone.m_crBorderColor = RGB(255,255,255);

		// Load the specified page of the image into a bitmap handle
		FILEINFO info = GetLeadToolsSizedStruct<FILEINFO>(0);
		BITMAPHANDLE hBitmap = {0};
		LeadToolsBitmapFreeer freer(hBitmap);
		loadImagePage(strSourceImage, nPageNumber, hBitmap, info);

		// Create a device context for the page to draw on
		LeadtoolsDCManager ltDC(hBitmap);

		// Draw the white box
		drawRedactionZone(ltDC.m_hDC, zone, hBitmap.YResolution, m_brushes, m_pens); 

		// Save the image page
		saveImagePage(hBitmap, strTempImage, info, 1);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28131");
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CSelectPageRegion::buildRasterZonesForExcludedRegion(long nTop, long nLeft,
																	   long nRight, long nBottom,
																	   long nWidth, long nHeight,
																	   long nPageNum)
{
	try
	{
		// Deduct one from width and height so that zones are within bounds of the image
		nWidth--;
		nHeight--;

		// Vector of zones to be returned
		IIUnknownVectorPtr ipZones(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI28138", ipZones != NULL);

		// Need to build raster zones surrounding the excluded region
		ILongRectanglePtr ipZoneRect(CLSID_LongRectangle);
		ASSERT_RESOURCE_ALLOCATION("ELI28139", ipZoneRect != NULL);

		// Build the top rectangle
		if (nTop > 0)
		{
			ipZoneRect->SetBounds(0, 0, nWidth, nTop);
			IRasterZonePtr ipZone(CLSID_RasterZone);
			ASSERT_RESOURCE_ALLOCATION("ELI28140", ipZone != NULL);
			ipZone->CreateFromLongRectangle(ipZoneRect, nPageNum);
			ipZones->PushBack(ipZone);
		}
		// Build the bottom rectangle
		if (nBottom < nHeight)
		{
			ipZoneRect->SetBounds(0, nBottom, nWidth, nHeight);
			IRasterZonePtr ipZone(CLSID_RasterZone);
			ASSERT_RESOURCE_ALLOCATION("ELI28141", ipZone != NULL);
			ipZone->CreateFromLongRectangle(ipZoneRect, nPageNum);
			ipZones->PushBack(ipZone);
		}
		// Build the left rectangle
		if (nLeft > 0)
		{
			ipZoneRect->SetBounds(0, nTop, nLeft, nBottom);
			IRasterZonePtr ipZone(CLSID_RasterZone);
			ASSERT_RESOURCE_ALLOCATION("ELI28142", ipZone != NULL);
			ipZone->CreateFromLongRectangle(ipZoneRect, nPageNum);
			ipZones->PushBack(ipZone);
		}
		// Build the right rectangle
		if (nRight < nWidth)
		{
			ipZoneRect->SetBounds(nRight, nTop, nWidth, nBottom);
			IRasterZonePtr ipZone(CLSID_RasterZone);
			ASSERT_RESOURCE_ALLOCATION("ELI28143", ipZone != NULL);
			ipZone->CreateFromLongRectangle(ipZoneRect, nPageNum);
			ipZones->PushBack(ipZone);
		}

		return ipZones;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28144");
}
//-------------------------------------------------------------------------------------------------
