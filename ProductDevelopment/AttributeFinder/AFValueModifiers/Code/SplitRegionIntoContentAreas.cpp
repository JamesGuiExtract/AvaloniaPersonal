// SplitRegionIntoContentAreas.cpp : Implementation of CSplitRegionIntoContentAreas

#include "stdafx.h"
#include "SplitRegionIntoContentAreas.h"

#include <extractZoneAsImage.h>
#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>
#include <CPPLetter.h>

// add license management function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion				= 4;
const unsigned int gnDEFAULT_CHAR_WIDTH_PIXELS		= 25;
const unsigned int gnDEFAULT_CHAR_HEIGHT_PIXELS		= 60;
const double gnDEFAULT_CHAR_WIDTH_IN				= .08;
const double gnDEFAULT_CHAR_HEIGHT_IN				= .20;
const unsigned int gnMIN_CHARS_NEEDED_FOR_SIZE		= 10;
const double gdMIN_PIXEL_PERCENT_OF_AREA			= 0.05;
const int gnMIN_CHAR_SEPARATION_OF_LINES			= 4;
const CPoint gptNULL								= CPoint(-1, -1);
const long gnCONFIDENT_OCR_TEXT_SCORE				= 60;
const double gdDUPLICATE_OVERLAP					= 0.70;
const double gdREQUIRED_LINE_OVERLAP				= 0.50;
const int gnAREA_PADDING_SIZE						= 2;
const UCHAR gucFIRST_BIT							= 0x80;
const string gstrDEFAULT_ATTRIBUTE_TEXT				= "[unrecognized]";
const string gstrDEFAULT_ATTRIBUTE_NAME				= "Content";
const double gdDEFAULT_MINIMUM_WIDTH				= 3.0;
const double gdDEFAULT_MINIMUM_HEIGHT				= 0.5;
const string gstrDEFAULT_GOOD_OCR_TYPE				= "ProbableText";
const string gstrDEFAULT_POOR_OCR_TYPE				= "ProbableHandwriting";
const long	 gnDEFAULT_OCR_THRESHOLD				= 60;
const string gstrSPATIAL_STRING_ATTRIBUTE_NAME		= "SpatialString";

//-------------------------------------------------------------------------------------------------
// CSplitRegionIntoContentAreas
//-------------------------------------------------------------------------------------------------
CSplitRegionIntoContentAreas::CSplitRegionIntoContentAreas()
: m_bDirty(false)
, m_ipCurrentDoc(NULL)
, m_nCurrentPage(0)
, m_ipCurrentPageText(NULL)
, m_sizeAvgChar(gnDEFAULT_CHAR_WIDTH_PIXELS, gnDEFAULT_CHAR_HEIGHT_PIXELS)
, m_rectCurrentPage(0, 0, 0, 0)
, m_ipSpatialStringSearcher(NULL)
, m_sizeEdgeSearchDirection(0, 0)
{
	try
	{
		reset();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI22349");
}
//-------------------------------------------------------------------------------------------------
CSplitRegionIntoContentAreas::~CSplitRegionIntoContentAreas()
{
	try
	{
		m_ipCurrentDoc = __nullptr;
		m_ipCurrentPageText = __nullptr;
		m_ipSpatialStringSearcher = __nullptr;
		m_apPageBitmap.reset();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI22067");
}
//-------------------------------------------------------------------------------------------------
HRESULT CSplitRegionIntoContentAreas::FinalConstruct()
{
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
void CSplitRegionIntoContentAreas::FinalRelease()
{
}

//--------------------------------------------------------------------------------------------------
// ISplitRegionIntoContentAreas
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::get_DefaultAttributeText(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22096", pVal != __nullptr);

		validateLicense();

		*pVal = get_bstr_t(m_strDefaultAttributeText).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22097")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::put_DefaultAttributeText(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_strDefaultAttributeText = asString(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22098")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::get_AttributeName(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22355", pVal != __nullptr);

		validateLicense();

		*pVal = get_bstr_t(m_strAttributeName).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22356")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::put_AttributeName(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_strAttributeName = asString(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22357")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::get_MinimumWidth(double *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22297", pVal != __nullptr);

		validateLicense();

		*pVal = m_dMinimumWidth;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22298")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::put_MinimumWidth(double newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22299", newVal >= 0);

		validateLicense();

		m_dMinimumWidth = newVal;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22300")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::get_MinimumHeight(double *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22509", pVal != __nullptr);

		validateLicense();

		*pVal = m_dMinimumHeight;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22510")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::put_MinimumHeight(double newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22511", newVal >= 0);

		validateLicense();

		m_dMinimumHeight = newVal;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22512")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::get_IncludeGoodOCR(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22301", pVal != __nullptr);

		validateLicense();

		*pVal = asVariantBool(m_bIncludeGoodOCR);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22302")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::put_IncludeGoodOCR(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bIncludeGoodOCR = asCppBool(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22303")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::get_IncludePoorOCR(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22304", pVal != __nullptr);

		validateLicense();

		*pVal = asVariantBool(m_bIncludePoorOCR);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22305")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::put_IncludePoorOCR(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bIncludePoorOCR = asCppBool(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22306")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::get_GoodOCRType(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22513", pVal != __nullptr);

		validateLicense();

		*pVal = get_bstr_t(m_strGoodOCRType).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22514")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::put_GoodOCRType(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_strGoodOCRType = asString(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22515")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::get_PoorOCRType(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22516", pVal != __nullptr);

		validateLicense();

		*pVal = get_bstr_t(m_strPoorOCRType).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22517")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::put_PoorOCRType(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_strPoorOCRType = asString(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22518")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::get_OCRThreshold(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22307", pVal != __nullptr);

		validateLicense();

		*pVal = m_nOCRThreshold;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22308")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::put_OCRThreshold(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22309", newVal >= 0 && newVal <= 100);

		validateLicense();

		m_nOCRThreshold = newVal;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22310")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::get_UseLines(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22326", pVal != __nullptr);

		validateLicense();

		*pVal = asVariantBool(m_bUseLines);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22327")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::put_UseLines(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bUseLines = asCppBool(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22328")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::get_ReOCRWithHandwriting(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22311", pVal != __nullptr);

		validateLicense();

		*pVal = asVariantBool(m_bReOCRWithHandwriting);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22312")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::put_ReOCRWithHandwriting(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bReOCRWithHandwriting = asCppBool(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22313")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::get_IncludeOCRAsTrueSpatialString(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22545", pVal != __nullptr);

		validateLicense();

		*pVal = asVariantBool(m_bIncludeOCRAsTrueSpatialString);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22546")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::put_IncludeOCRAsTrueSpatialString(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bIncludeOCRAsTrueSpatialString = asCppBool(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22547")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::get_RequiredHorizontalSeparation(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI28020", pVal != __nullptr);

		validateLicense();

		*pVal = m_nRequiredHorizontalSeparation;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28021")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::put_RequiredHorizontalSeparation(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI28022", newVal >= 0);

		validateLicense();

		m_nRequiredHorizontalSeparation = newVal;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28023")
}

//--------------------------------------------------------------------------------------------------
// IAttributeModifyingRule
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::raw_ModifyValue(IAttribute* pAttribute, 
														   IAFDocument* pOriginInput, 
														   IProgressStatus* pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// check licensing
		validateLicense();
		
		IAttributePtr ipAttribute(pAttribute);
		ASSERT_ARGUMENT("ELI22103", ipAttribute != __nullptr);

		IAFDocumentPtr ipDocument(pOriginInput);
		ASSERT_ARGUMENT("ELI22109", ipDocument);

		addContentAreaAttributes(ipDocument, ipAttribute);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22068");
}


//--------------------------------------------------------------------------------------------------
// IAttributeSplitter
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::raw_SplitAttribute(IAttribute * pAttribute, 
															  IAFDocument *pAFDoc, 
															  IProgressStatus *pProgressStatus)
{
	try
	{
		validateLicense();

		IAttributePtr ipAttribute(pAttribute);
		ASSERT_ARGUMENT("ELI22364", ipAttribute != __nullptr);

		IAFDocumentPtr ipDocument(pAFDoc);
		ASSERT_ARGUMENT("ELI22365", ipDocument);

		addContentAreaAttributes(ipDocument, ipAttribute);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22069")
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::raw_IsConfigured(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Check parameter
		ASSERT_ARGUMENT("ELI22070", pbValue != __nullptr);

		*pbValue = asVariantBool(!m_strDefaultAttributeText.empty() &&
								  m_dMinimumWidth >= 0 && m_dMinimumHeight >= 0 && 
								  (m_bIncludeGoodOCR || m_bIncludePoorOCR) &&
								  m_nOCRThreshold >= 0 && m_nOCRThreshold <= 100);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22071");
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::raw_GetComponentDescription(BSTR *pbstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI22072", pbstrComponentDescription != __nullptr)

		*pbstrComponentDescription = _bstr_t("Split region into content areas").Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22073")
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license
		validateLicense();

		UCLID_AFVALUEMODIFIERSLib::ISplitRegionIntoContentAreasPtr ipCopyThis = pObject;
		ASSERT_ARGUMENT("ELI22074", ipCopyThis != __nullptr);

		m_strDefaultAttributeText			= asString(ipCopyThis->DefaultAttributeText);
		m_strAttributeName					= asString(ipCopyThis->AttributeName);
		m_dMinimumWidth						= ipCopyThis->MinimumWidth;
		m_dMinimumHeight					= ipCopyThis->MinimumHeight;
		m_bIncludeGoodOCR					= asCppBool(ipCopyThis->IncludeGoodOCR);
		m_bIncludePoorOCR					= asCppBool(ipCopyThis->IncludePoorOCR);
		m_strGoodOCRType					= asString(ipCopyThis->GoodOCRType);
		m_strPoorOCRType					= asString(ipCopyThis->PoorOCRType);
		m_nOCRThreshold						= ipCopyThis->OCRThreshold;
		m_bUseLines							= asCppBool(ipCopyThis->UseLines);
		m_bReOCRWithHandwriting				= asCppBool(ipCopyThis->ReOCRWithHandwriting);
		m_bIncludeOCRAsTrueSpatialString	= asCppBool(ipCopyThis->IncludeOCRAsTrueSpatialString);
		m_nRequiredHorizontalSeparation		= ipCopyThis->RequiredHorizontalSeparation;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22075");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license
		validateLicense();

		ASSERT_ARGUMENT("ELI22076", pObject != __nullptr);

		// Create another instance of this object
		ICopyableObjectPtr ipObjCopy(CLSID_SplitRegionIntoContentAreas);
		ASSERT_RESOURCE_ALLOCATION("ELI22077", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);
	
		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22078");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI22079", pClassID != __nullptr);

		*pClassID = CLSID_SplitRegionIntoContentAreas;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22080");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		return m_bDirty ? S_OK : S_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22081");
}
//-------------------------------------------------------------------------------------------------
// Version 1:
// m_strDefaultAttributeText
// m_strAttributeName
// m_dMinimumWidth
// m_bExcludeByOCR
// m_bExcludePoorOCRAreas
// m_nOCRThreshold
// m_bUseLines
// m_bReOCRWithHandwriting
// 
// Version 2: 
// Added m_dMinimumHeight, m_strGoodOCRType and m_strPoorOCRType.
// m_bExcludeByOCR and m_bExcludePoorOCRAreas replaced by m_bIncludeGoodOCR and m_bIncludePoorOCR
//
// Version 3: 
// Added m_bIncludeOCRAsTrueSpatialString
//
// Version 4:
// Added m_nRequiredHorizontalSeparation;
STDMETHODIMP CSplitRegionIntoContentAreas::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		ASSERT_ARGUMENT("ELI22082", pStream != __nullptr);

		// Reset data members
		reset();
		
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
			UCLIDException ue("ELI22083", 
				"Unable to load newer split region into content areas rule!");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// Read data members from the stream
		dataReader >> m_strDefaultAttributeText;
		dataReader >> m_strAttributeName;
		dataReader >> m_dMinimumWidth;

		if (nDataVersion == 1)
		{
			bool bUnused;
			dataReader >> bUnused; //m_bExcludeByOCR
			dataReader >> bUnused; //m_bExcludePoorOCRAreas;
		}
		else
		{
			dataReader >> m_dMinimumHeight;
			dataReader >> m_bIncludeGoodOCR;
			dataReader >> m_bIncludePoorOCR;
			dataReader >> m_strGoodOCRType;
			dataReader >> m_strPoorOCRType;
		}

		dataReader >> m_nOCRThreshold;
		dataReader >> m_bUseLines;
		dataReader >> m_bReOCRWithHandwriting;

		if (nDataVersion >= 3)
		{
			dataReader >> m_bIncludeOCRAsTrueSpatialString;
		}

		if (nDataVersion >= 4)
		{
			dataReader >> m_nRequiredHorizontalSeparation;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22084");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		ASSERT_ARGUMENT("ELI22085", pStream != __nullptr);

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		// Write the current version
		dataWriter << gnCurrentVersion;

		// Write the data members to the stream
		dataWriter << m_strDefaultAttributeText;
		dataWriter << m_strAttributeName;
		dataWriter << m_dMinimumWidth;
		dataWriter << m_dMinimumHeight;
		dataWriter << m_bIncludeGoodOCR;
		dataWriter << m_bIncludePoorOCR;
		dataWriter << m_strGoodOCRType;
		dataWriter << m_strPoorOCRType;
		dataWriter << m_nOCRThreshold;
		dataWriter << m_bUseLines;
		dataWriter << m_bReOCRWithHandwriting;
		dataWriter << m_bIncludeOCRAsTrueSpatialString;
		dataWriter << m_nRequiredHorizontalSeparation;

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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22086");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI22066", pbValue != __nullptr);

		try
		{
			// check the license
			validateLicense();

			// If no exception, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22065");

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSplitRegionIntoContentAreas::InterfaceSupportsErrorInfo(REFIID riid)
{
	try
	{
		static const IID* arr[] = 
		{
			&IID_ISplitRegionIntoContentAreas,
			&IID_IAttributeModifyingRule,
			&IID_IAttributeSplitter,
			&IID_IPersistStream,
			&IID_ICategorizedComponent,
			&IID_ISpecifyPropertyPages,
			&IID_ICopyableObject,
			&IID_IMustBeConfiguredObject,
			&IID_ILicensedComponent
		};

		for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
		{
			if (InlineIsEqualGUID(*arr[i],riid))
				return S_OK;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22064")

	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// PixelProcessor
//--------------------------------------------------------------------------------------------------
CSplitRegionIntoContentAreas::PixelProcessor::PixelProcessor(CSplitRegionIntoContentAreas *pparent, CRect rect)
: m_parent(*pparent)
, m_rect(rect)
, m_nXPixelValue(0)
, m_nYPixelValue(0)
, m_nPixelCount(0)
{
}
//--------------------------------------------------------------------------------------------------
CSplitRegionIntoContentAreas::PixelProcessor::~PixelProcessor()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI22227");
}
//--------------------------------------------------------------------------------------------------
void CSplitRegionIntoContentAreas::PixelProcessor::process()
{
	try
	{
		// Simply don't do any processing if m_rect doesn't overlap the current page. 
		if (!m_parent.ensureRectInPage(m_rect, false))
		{
			return;
		}

		// Keep track of whether the PixelProcessor has requested that processing be aborted.
		bool bAbort = false;

		// Cycle through each row of image data
		for (int y = m_rect.top; y <= m_rect.bottom && !bAbort; y++)
		{
			int nPixelX = m_rect.left;
			int nByte = m_rect.left / 8;
			int nLastByte = m_rect.right / 8;

			// Obtain a pointer to the raw image data.
			UCHAR *pucImageRow = m_parent.m_apPageBitmap->m_hBitmap.Addr.Windows.pData;
			// Adjust the pointer to the row that contains the pixels we need.
			pucImageRow += y * m_parent.m_apPageBitmap->m_hBitmap.BytesPerLine;

			// Cycle through each byte in the row.
			while (nByte <= nLastByte && !bAbort)
			{
				// Obtain a pointer to the byte of raw image data we need.
				UCHAR *pucImageByte = pucImageRow + nByte;

				// Use a bit mask to check each pixel (bit) within the byte taking care to not read
				// bits to the left or right of m_rect.
				int nBit = nPixelX % 8;

				while (nBit < 8)
				{
					if (nPixelX > m_rect.right)
					{
						// We are now to the right of m_rect, go to the next row.
						break;
					}

					// Set a mask to read the correct pixel from the byte.
					UCHAR ucMask = gucFIRST_BIT >> nBit;
					if ((ucMask & *pucImageByte) != 0)
					{
						// This pixel is black; process it.
						int nRes = processPixel(nPixelX, y);
						if (nRes < 0)
						{
							// The processor has requested for processing to stop.
							bAbort = true;
							break;
						}

						if (nRes > 0)
						{
							// The processor has requested that nRes pixels should be skipped before
							// processing continues.
							nPixelX += nRes;

							if (nPixelX / 8 != nByte)
							{
								// Skipping ahead to a byte that follows this one
								// (subtract one to account for the fact that nByte
								// will be incremented)
								nByte = nPixelX / 8 - 1;  
								break;
							}
							else
							{
								// Skipping ahead to a bit within this same byte.
								nBit = nPixelX % 8;
								continue;
							}
						}
					}

					nPixelX ++;
					nBit ++;
				}

				nByte ++;
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26602");
}
//--------------------------------------------------------------------------------------------------
int CSplitRegionIntoContentAreas::PixelContentSearcher::processPixel(int x, int y)
{
	try
	{
		try
		{
			CPoint ptStart(x, y);

			// Start by ensuring we are to process the specified pixel.
			if (m_parent.isExcluded(ptStart))
			{
				// If ptStart was excluded, the point now indicates the next pixel not excluded.
				// Indicate to the base class how many pixels to skip.
				return ptStart.x - x;
			}
			else
			{
				// Create a rect of average character size that contains this point and is centered
				// as best as possible on black pixels.
				CRect rect(0, 0, 0, 0);
				CRect rectToExclude = m_parent.centerAreaRegionOnBlack(ptStart, rect,
					m_parent.m_rectCurrentPage);

				CPoint rectCenter(rect.CenterPoint());

				// Make sure the center position of this rect is okay to process an that the starting
				// rect has enough pixels to be worth processing.
				if (!m_parent.isExcluded(rectCenter) && 
					m_parent.hasEnoughPixels(rect))
				{
					// Attempt to expand the area horizontally to the extent of the included content
					if (m_parent.expandHorizontally(rect, m_rect))
					{
						// Don't bother processing anything in this area in the future
						m_parent.m_vecExcludedAreas.push_back(rect);

						// Add the area to the list of candidate areas only if it is big enough
						// Now that the area is expanded, now see if the center point lies within an
						// existing area.  If so, don't add an area that will likely be a duplicate,
						// just make sure the existing area is expanded to the same horizontal extent.
						CPoint ptCenter = rect.CenterPoint();
						bool bFoundExisting = false;

						for (size_t i = 0; i < m_parent.m_vecContentAreas.size(); i++)
						{
							if (m_parent.m_vecContentAreas[i].PtInRect(ptCenter) ||
								rect.PtInRect(m_parent.m_vecContentAreas[i].CenterPoint()))
							{
								bFoundExisting = true;

								// If the new area's center is within an existing area, combine the 
								// two areas into one instead of adding a separate area.
								m_parent.m_vecContentAreas[i].UnionRect(
									&(m_parent.m_vecContentAreas[i]), &rect);
							}
						}

						// Add the area to the result list if necessary.
						if (!bFoundExisting)
						{
							m_parent.m_vecContentAreas.push_back(rect);
						}
					}
				}
				else
				{	
					// Keep track of pixels that no longer need to be processed and
					// inform the base class of how many pixels can be skipped
					if (rectToExclude.bottom >= y)
					{
						if (rectToExclude.bottom > y)
						{
							m_parent.m_vecExcludedAreas.push_back(rectToExclude);
						}

						return rectToExclude.right - x;
					}
				}
			}

			return 0;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26603");
	}
	catch(UCLIDException& ue)
	{
		ue.addDebugInfo("X Coord", x);
		ue.addDebugInfo("Y Coord", y);
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
int CSplitRegionIntoContentAreas::PixelCounter::processPixel(int x, int y)
{
	// Simply keep track of how many black pixels have been found
	m_nPixelCount++;
	
	return 0;
}
//--------------------------------------------------------------------------------------------------
int CSplitRegionIntoContentAreas::PixelAverager::processPixel(int x, int y)
{
	// Increment the values in a way that can be used to calculate the average x and y pixel
	// position.
	m_nXPixelValue += x;
	m_nYPixelValue += y;
	m_nPixelCount ++;

	return 0;
}
//--------------------------------------------------------------------------------------------------
int CSplitRegionIntoContentAreas::PixelEdgeFinder::processPixel(int x, int y)
{
	try
	{
		try
		{
			// The specified pixel is from the the next pixels to be included/excluded from an area
			CPoint ptPixel(x, y);

			// Make sure this pixels is on the page.  If not, return -1 to stop processing now.
			if (!m_parent.isPointOnPage(ptPixel))
			{
				return -1;
			}

			// Make sure the previous pixel (opposite of the expansion direction), was also black.
			// If so consider this as content.
			// [FlexIDSCore #3560] - Ensure the previous pixel is on the page
			CPoint ptPreviousPixel = ptPixel - m_parent.m_sizeEdgeSearchDirection;
			if (m_parent.isPointOnPage(ptPreviousPixel)
				&& m_parent.m_apPageBitmap->isPixelBlack(ptPreviousPixel))
			{
				// Set the return value to indicate content was found
				m_nPixelCount = 1;

				// Return -1 to stop looking since we already found content.
				return -1;
			}

			return 0;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26604");
	}
	catch(UCLIDException& ue)
	{
		ue.addDebugInfo("X Coord", x);
		ue.addDebugInfo("Y Coord", y);
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
int CSplitRegionIntoContentAreas::PixelEraser::processPixel(int x, int y)
{
	try
	{
		try
		{
			// Obtain a pointer to the raw image data.
			UCHAR *pucImageData = m_parent.m_apPageBitmap->m_hBitmap.Addr.Windows.pData;
			// Adjust the pointer to the byte that contains the pixels we need.
			pucImageData += y * m_parent.m_apPageBitmap->m_hBitmap.BytesPerLine;
			pucImageData += x / 8;

			// Set a mask to obtain the bit we need.
			UCHAR ucMask = gucFIRST_BIT >> (x % 8);

			// Flip the bit (since the pixel was black for processPixel to be called,
			// we know we are flipping it to white).
			*pucImageData = *pucImageData ^ ucMask;

			return 0;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26605");
	}
	catch(UCLIDException& ue)
	{
		ue.addDebugInfo("X Coord", x);
		ue.addDebugInfo("Y Coord", y);
		throw ue;
	}
}

//--------------------------------------------------------------------------------------------------
// ContentAreaInfo
//--------------------------------------------------------------------------------------------------
CSplitRegionIntoContentAreas::ContentAreaInfo::ContentAreaInfo(const ISpatialStringPtr &ipString,
													const ILongToObjectMapPtr &ipSpatialPageInfos) 
: CRect()
, m_nOCRConfidence(0)
, m_eTopBoundaryState(kNotFound)
, m_eBottomBoundaryState(kNotFound)
{
	try
	{
		ASSERT_ARGUMENT("ELI22173", ipString != __nullptr);

		if (asCppBool(ipString->HasSpatialInfo()))
		{
			// If this spatial string has spatial information, use it to set the area bounds.	
			// Get the bounds in terms of the specified page infos not the spatial string's
			// page infos since previous rule objects may have modified the spatial infos of
			// specific attributes so that they no longer match the page.
			ILongRectanglePtr ipRect = ipString->GetTranslatedImageBounds(ipSpatialPageInfos);

			ASSERT_RESOURCE_ALLOCATION("ELI22174", ipRect != __nullptr);
			long lLeft, lTop, lRight, lBottom;
			ipRect->GetBounds(&lLeft, &lTop, &lRight, &lBottom);

			SetRect(lLeft, lTop, lRight, lBottom);

			if (ipString->GetMode() == kHybridMode)
			{
				// If a content area is based on a hybrid string, there is no confidence that can
				// be attributed to the OCR.
				m_nOCRConfidence = 0;
			}
			else
			{
				// Calculate the average OCR confidence of the string.
				ipString->GetCharConfidence(NULL, NULL, &m_nOCRConfidence);
			}
		}
		else
		{
			// If the SpatialString doesn't contain any spatial data, just set the rect to NULL.
			SetRect(0, 0, 0, 0);
		}
		
		m_rectOriginal = *this;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI22172");
}
//--------------------------------------------------------------------------------------------------
CSplitRegionIntoContentAreas::ContentAreaInfo::ContentAreaInfo(const CRect &rect)
: CRect(rect)
, m_rectOriginal(rect)
, m_nOCRConfidence(0)
, m_eTopBoundaryState(kNotFound)
, m_eBottomBoundaryState(kNotFound)
{
}
//--------------------------------------------------------------------------------------------------
bool operator < (const CRect& first, const CRect& second)
{
	// A comparison of the rectangle bounds of a ContentAreaInfo objects is all we need for the set
	// implementation (don't bother with the ContentAreaInfo specific fields).

	if (first.left != second.left)
	{
		return first.left < second.left;
	}

	if (first.top != second.top)
	{
		return first.top < second.top;
	}

	if (first.right != second.right)
	{
		return first.right < second.right;
	}
	
	if (first.bottom != second.bottom)
	{
		return first.bottom < second.bottom;
	}	

	return false;
}

//--------------------------------------------------------------------------------------------------
// Private Methods
//--------------------------------------------------------------------------------------------------
void CSplitRegionIntoContentAreas::addContentAreaAttributes(IAFDocumentPtr ipDoc, 
															IAttributePtr ipAttribute)
{
	try
	{
		ASSERT_ARGUMENT("ELI22115", ipDoc != __nullptr);
		ASSERT_ARGUMENT("ELI22116", ipAttribute != __nullptr);

		// Checks the for the handwriting OCR license if m_bReOCRWithHandwriting is true.  If the
		// license is neede but missing, and exception is logged and m_bReOCRWithHandwriting is
		// set to false.
		validateHandwritingLicense();

		// Get the spatial string from the document
		ISpatialStringPtr ipDocText = ipDoc->Text;
		ASSERT_RESOURCE_ALLOCATION("ELI22114", ipDocText != __nullptr);

		// Get the spatial string from the attribute
		ISpatialStringPtr ipValue = ipAttribute->Value;

		// If this attribute doesn't have a value or the value doesn't have any spatial information
		// or the AFDocument string does not have any spatial information,
		// there is nothing to process. [FlexIDSCore #4049]
		if (ipValue == __nullptr
			|| !asCppBool(ipValue->HasSpatialInfo())
			|| !asCppBool(ipDocText->HasSpatialInfo()))
		{
			return;
		}

		// Clear any existing results or excluded areas.
		m_vecContentAreas.clear();
		m_vecExcludedAreas.clear();
		m_setPreviouslyAddedAreas.clear();

		IIUnknownVectorPtr ipSubAttributes = ipAttribute->SubAttributes;
		ASSERT_RESOURCE_ALLOCATION("ELI22104", ipSubAttributes != __nullptr);

		ILongToObjectMapPtr ipSpatialPageInfos = ipDocText->SpatialPageInfos;
		ASSERT_RESOURCE_ALLOCATION("ELI28052", ipSpatialPageInfos != __nullptr);

		IIUnknownVectorPtr ipAttributeLines = ipValue->GetLines();
		ASSERT_RESOURCE_ALLOCATION("ELI22228", ipAttributeLines != __nullptr);

		// Process the attribute line-by-line if there is more than 1 so that
		// all pages of a given attribute are processed.
		long nAttributeLineCount = ipAttributeLines->Size();
		for (long i = 0; i < nAttributeLineCount; i++)
		{
			ISpatialStringPtr ipAttributeLine = ipAttributeLines->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI22112", ipAttributeLine != __nullptr);

			// Skip this line if it is non-spatial [FlexIDSCore #3143]
			if (ipAttributeLine->HasSpatialInfo() == VARIANT_FALSE)
			{
				continue;
			}

			long nPage = ipAttributeLine->GetFirstPageNumber();

			// Load the bitmap for this page.
			if (!loadPageBitmap(ipDoc, nPage))
			{
				continue;
			}

			// Get a spatial string searcher for the page
			ISpatialStringSearcherPtr ipSearcher = getSpatialStringSearcher(ipDoc, nPage);
			if (ipSearcher)
			{
				// Search should include words on the boundary.  In the end, only part of such
				// words will be included, but for now we want this text for clues on processing.
				ipSearcher->SetIncludeDataOnBoundary(VARIANT_TRUE);

				// Get the bounds in terms of the specified page infos not the spatial string's
				// page infos since previous rule objects may have modified the spatial infos of
				// specific attributes so that they no longer match the page.
				ILongRectanglePtr ipRect =
					ipAttributeLine->GetTranslatedImageBounds(ipSpatialPageInfos);
				ASSERT_RESOURCE_ALLOCATION("ELI22107", ipRect != __nullptr);

				CRect rectRegion;
				ipRect->GetBounds(&(rectRegion.left), &(rectRegion.top),
					&(rectRegion.right), &(rectRegion.bottom));

				// Pass VARIANT_FALSE so the rectangle does not get rotated before searching
				ISpatialStringPtr ipText = ipSearcher->GetDataInRegion(ipRect, VARIANT_FALSE);
				ASSERT_RESOURCE_ALLOCATION("ELI22101", ipText != __nullptr);

				if (m_ipCurrentPageText->Size > gnMIN_CHARS_NEEDED_FOR_SIZE)
				{
					// Set the average character height using the page text if possible
					m_sizeAvgChar = CSize(m_ipCurrentPageText->GetAverageCharWidth(), 
										  m_ipCurrentPageText->GetAverageCharHeight());
				}
				else 
				{
					// Not enough text in the page. Set m_sizeAvgChar to the default size.
					m_sizeAvgChar.cx = 
						(int) (gnDEFAULT_CHAR_WIDTH_IN * m_apPageBitmap->m_hBitmap.XResolution);
					m_sizeAvgChar.cy = 
						(int) (gnDEFAULT_CHAR_HEIGHT_IN * m_apPageBitmap->m_hBitmap.YResolution);
				}

				// Get a vector of the lines of text from the region.
				IIUnknownVectorPtr ipLines = ipText->GetLines();
				ASSERT_RESOURCE_ALLOCATION("ELI22229", ipLines != __nullptr);

				// Split these lines so that lines that are separated by enough white-space
				// are considered separate lines.
				splitLineFragments(ipLines);

				// Take each line and attempt to expand it to ensure it encapsulates all content,
				// even pixels that didn't OCR.
				long nLineCount = ipLines->Size();
				for (long i = 0; i < nLineCount; i++)
				{
					ISpatialStringPtr ipLine = ipLines->At(i);
					ASSERT_RESOURCE_ALLOCATION("ELI22102", ipLine != __nullptr);

					ContentAreaInfo area(ipLine, ipSpatialPageInfos);
					if (!area.IsRectNull())
					{
						if (expandHorizontally(area, rectRegion))
						{
							// Don't bother processing anything in this area in the future
							m_vecExcludedAreas.push_back(area);

							// add it to the result candidates.
							m_vecContentAreas.push_back(area);
						}
					}
				}

				// Now that we have attempted to expand all OCR'd text, search the pixels in
				// the region for qualifying content areas that don't contain any OCR'd text.
				processRegionPixels(rectRegion, ipSearcher);

				// Create a new subattribute for each qualifying content area.
				for each (ContentAreaInfo area in m_vecContentAreas)
				{
					IAttributePtr ipNewSubAttribute = createResult(ipDoc, nPage, area,
						ipSpatialPageInfos);
					ASSERT_RESOURCE_ALLOCATION("ELI22230", ipNewSubAttribute != __nullptr);

					ipSubAttributes->PushBack(ipNewSubAttribute);
				}
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26606");
}
//--------------------------------------------------------------------------------------------------
void CSplitRegionIntoContentAreas::splitLineFragments(IIUnknownVectorPtr ipLines)
{
	try
	{
		ASSERT_RESOURCE_ALLOCATION("ELI22178", ipLines != __nullptr);

		IIUnknownVectorPtr ipReturnValue(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI22179", ipReturnValue != __nullptr);

		// The maximum distance one character can be from another
		int maxGapBetweenChars = m_nRequiredHorizontalSeparation * m_sizeAvgChar.cx;

		// Cycle through each line and split it into multiple lines if appropriate.
		long nLineCount = ipLines->Size();
		for (int i = 0; i < nLineCount; i++)
		{
			ISpatialStringPtr ipLine = ipLines->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI22180", ipLine != __nullptr);

			// If this line doesn't have spatial info, there's nothing to do.
			if (!asCppBool(ipLine->HasSpatialInfo()))
			{
				continue;
			}

			long nLetterCount = -1;
			CPPLetter* pLetters = NULL;

			if (ipLine->GetMode() == kHybridMode)
			{
				// If the line is hybrid, there is no way to split it-- just add it to the result set.
				ipReturnValue->PushBack(ipLine);
			}
			else
			{
				// If the line is true-spatial, use the letters to split the line as appropriate.
				ipLine->GetOCRImageLetterArray(&nLetterCount, (void**)&pLetters);
				ASSERT_RESOURCE_ALLOCATION("ELI22198", pLetters != __nullptr);
			}

			// Cycle through each letter of the line and make sure it within maxGapBetweenChars of
			// the previous character and is vertically in-line with the others.
			CRect rectBounds(0, 0, 0, 0);
			for (long k = 0; k < nLetterCount; k++)
			{
				const CPPLetter& letter = pLetters[k];
				if (!letter.m_bIsSpatial)
				{
					continue;
				}

				CRect rectLetter(letter.m_ulLeft, letter.m_ulTop,
					letter.m_ulRight, letter.m_ulBottom);
				if (rectLetter.IsRectEmpty())
				{
					continue;
				}

				if (rectBounds.IsRectNull())
				{
					// rectBounds keeps running track of the line bounds.  If it isn't yet set,
					// start with the current letter.
					rectBounds = rectLetter;
				}
				else
				{
					// If this letter is too far from the previous letter or its bounds don't
					// conform with the current bounds, spawn a new spatial string and add it to
					// the result vector.
					if (rectLetter.left - rectBounds.right > maxGapBetweenChars ||
						rectLetter.top > rectBounds.bottom || rectLetter.bottom < rectBounds.top)
					{
						ISpatialStringPtr ipBegin = ipLine->GetSubString(0, k - 1);
						ASSERT_RESOURCE_ALLOCATION("ELI22199", ipBegin != __nullptr);
						ipReturnValue->PushBack(ipBegin);

						// Start over with the remaining characters in the line.
						ipLine = ipLine->GetSubString(k, nLetterCount - 1);
						ASSERT_RESOURCE_ALLOCATION("ELI22200", ipLine != __nullptr);
						nLetterCount -= k;
						k = 1;

						rectBounds = rectLetter;
					}
					else
					{
						// Update the total bounds of the line thus far.
						rectBounds.UnionRect(&rectLetter, &rectBounds);
					}
				}
			}

			ipReturnValue->PushBack(ipLine);
		}

		// Reset ipLines with the return value we've accumulated.
		ipLines->Clear();
		ipLines->Append(ipReturnValue);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26607");
}
//--------------------------------------------------------------------------------------------------
void CSplitRegionIntoContentAreas::processRegionPixels(const CRect& rect,
													   ISpatialStringSearcherPtr ipSearcher)
{
	try
	{
		// Find all content areas based on pixels using PixelContentSearcher.  This will expand
		// the areas horizontally during processing.
		PixelContentSearcher pixelContentSearcher(this, rect);
		pixelContentSearcher.process();

		// Take these areas, expand them vertically, and handle any resulting overlap of area bounds.
		expandAndMergeAreas();

		// Clean up the area bounds, ensure area qualifications, and eliminate duplicates, 
		finalizeContentAreas(rect, ipSearcher);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26608");
}
//--------------------------------------------------------------------------------------------------
void CSplitRegionIntoContentAreas::expandAndMergeAreas()
{
	try
	{
		// Iterate through each area, attempting to expand it both up and down pixel-by-pixel. As the
		// edge of content areas are found or they collide with other areas, adjust the area bounds 
		// accordingly and stop expansion of these bounds until there are no more bounds processing.
		bool bProcessing = true;
		while (bProcessing)
		{
			// Set bProcessing to false until we find a border that still needs processing.
			bProcessing = false;

			for (size_t i = 0; i < m_vecContentAreas.size(); i++)
			{
				// Delete any area with a NULL rectangle.
				if (m_vecContentAreas[i].IsRectNull())
				{
					m_vecContentAreas.erase(m_vecContentAreas.begin() + i);
					i--;
					continue;
				}

				// A vector of new areas that need to be added as a result of two areas that 
				// are found to be overlapping.
				vector<ContentAreaInfo> vecAreasToAdd;

				if (m_vecContentAreas[i].m_eTopBoundaryState == kNotFound)
				{
					// The top border needs expanding. Set bProcessing and m_sizeEdgeSearchDirection
					// accordingly.
					m_sizeEdgeSearchDirection.SetSize(0, -1);
					bProcessing = true;

					if (attemptMerge(m_vecContentAreas[i], true, vecAreasToAdd, true))
					{
						// This area was merged with another (the other is the one that will remain).
						// We can safely remove the current area.
						m_vecContentAreas.erase(m_vecContentAreas.begin() + i);
						i--;
						continue;
					}
					else if (m_vecContentAreas[i].m_eTopBoundaryState == kNotFound)
					{
						// If the area still needs expansion following the merge attempt, use
						// PixelEdgeFinder to see if we can expand it up by a pixel.
						CRect rectExpansionEdge(m_vecContentAreas[i].left, 
							m_vecContentAreas[i].top - 1, 
							m_vecContentAreas[i].right, 
							m_vecContentAreas[i].top - 1);

						PixelEdgeFinder PixelEdgeFinder(this, rectExpansionEdge);
						PixelEdgeFinder.process();
						if (PixelEdgeFinder.m_nPixelCount == 0)
						{
							// There is no more content. The top border can be considered expanded.
							m_vecContentAreas[i].m_eTopBoundaryState = kFound;
						}
						else
						{
							// Content was found, expand the area up a pixel.
							m_vecContentAreas[i].top--;
						}
					}
				}
				else if (m_vecContentAreas[i].m_eBottomBoundaryState == kNotFound)
				{
					// The bottom border needs expanding. Set bProcessing and m_sizeEdgeSearchDirection
					// accordingly.
					m_sizeEdgeSearchDirection.SetSize(0, 1);
					bProcessing = true;

					if (attemptMerge(m_vecContentAreas[i], false, vecAreasToAdd, true))
					{
						// This area was merged with another (the other is the one that will remain).
						// We can safely remove the current area.
						m_vecContentAreas.erase(m_vecContentAreas.begin() + i);
						i--;
						continue;
					}
					else if (m_vecContentAreas[i].m_eBottomBoundaryState == kNotFound)
					{
						// If the area still needs expansion following the merge attempt, use
						// PixelEdgeFinder to see if we can expand it down by a pixel.
						CRect rectExpansionEdge(m_vecContentAreas[i].left, 
							m_vecContentAreas[i].bottom + 1,
							m_vecContentAreas[i].right, 
							m_vecContentAreas[i].bottom + 1);

						PixelEdgeFinder PixelEdgeFinder(this, rectExpansionEdge);
						PixelEdgeFinder.process();
						if (PixelEdgeFinder.m_nPixelCount == 0)
						{
							// There is no more content. The bottom border can be considered expanded.
							m_vecContentAreas[i].m_eBottomBoundaryState = kFound;
						}
						else
						{
							// Content was found, expand the area down a pixel.
							m_vecContentAreas[i].bottom++;
						}
					}
				}

				// Add to the existing vector of candidate areas any new areas generated by the 
				// attemptMerge calls.
				if (!vecAreasToAdd.empty())
				{
					for (vector<ContentAreaInfo>::iterator iter = vecAreasToAdd.begin();
						 iter != vecAreasToAdd.end();
						 iter++)
					{
						// [FlexIDSCore:4570]
						// To help ensure against an infinite loop, ensure expandAndMergeAreas hasn't
						// already added an identical ContentAreaInfo.
						if (m_setPreviouslyAddedAreas.find(*iter) == m_setPreviouslyAddedAreas.end())
						{
							m_vecContentAreas.push_back(*iter);
							m_setPreviouslyAddedAreas.insert(*iter);
						}
					}
				}
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26609");
}
//--------------------------------------------------------------------------------------------------
void CSplitRegionIntoContentAreas::finalizeContentAreas(const CRect& rectRegion,
														ISpatialStringSearcherPtr ipSearcher)
{
	try
	{
		// Loop through each content area candidate to clean it up.
		for (size_t i = 0; i < m_vecContentAreas.size(); i++)
		{
			// Any areas that don't intersect with the original region can be removed. Those that do
			// intersect should be clipped to include just the intersection area.
			if (m_vecContentAreas[i].IntersectRect(&m_vecContentAreas[i], &rectRegion) == FALSE)
			{
				m_vecContentAreas.erase(m_vecContentAreas.begin() + i);
				i--;
				continue;
			}

			// Trim off any excess white space that has resulted from merging/clipping.
			shrinkToFit(m_vecContentAreas[i]);

			// If lines are involved with the area, adjust the areas accordingly.
			// Store the modified area back to the vector
			m_vecContentAreas[i] = makeFlushWithLines(m_vecContentAreas[i]);
		}

		// Merge any areas whose shared area is similar or that share similar y coordinates (in other 
		// words, that appear to represent different fragments of the same line).
		mergeAreas(ipSearcher);

		// Loop through each content area candidate to ensure it meets specified requirements to be
		// kept.
		for (size_t i = 0; i < m_vecContentAreas.size(); i++)
		{
			// Update the confidence of all remaining areas from spatial string searcher
			ContentAreaInfo& area = m_vecContentAreas[i];
			area.m_nOCRConfidence = getOcrConfidenceForRegion(area, ipSearcher);
			if (!areaMeetsSpecifications(area))
			{
				m_vecContentAreas.erase(m_vecContentAreas.begin() + i);
				i--;
			}
		}

		// Finally, sort the areas from top down.
		sort(m_vecContentAreas.begin(), m_vecContentAreas.end(), isAreaAbove);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26610");
}
//--------------------------------------------------------------------------------------------------
void CSplitRegionIntoContentAreas::mergeAreas(ISpatialStringSearcherPtr ipSearcher)
{
	try
	{
		for (size_t i = 0; i < m_vecContentAreas.size(); i++)
		{
			ContentAreaInfo areaIPadded(m_vecContentAreas[i]);
			areaIPadded.InflateRect(m_nRequiredHorizontalSeparation * m_sizeAvgChar.cx, 0);

			// Compare to all areas before this in the vector.
			for (size_t j = 0; j < i; j++)
			{
				CRect rectIntersect;
				if (rectIntersect.IntersectRect(&areaIPadded, &m_vecContentAreas[j]))
				{
					// If these areas intersect, get the height and width of the intersection.
					double dIntersectHeight = (double) rectIntersect.Height();
					double dIntersectWidth = 0;
					if (rectIntersect.IntersectRect(&m_vecContentAreas[i], &m_vecContentAreas[j]))
					{
						// Rect i was padded to ensure we merge areas level with each other but not 
						// quite overlapping. Make sure not to include this padding width-wise.
						dIntersectWidth = rectIntersect.Width();
					}
					double dIntersectionArea = dIntersectHeight * dIntersectWidth;

					// Calculate the veritcal overlap of the regions
					double dHeightIOverlap = dIntersectHeight / (double) m_vecContentAreas[i].Height();
					double dHeightJOverlap = dIntersectHeight / (double) m_vecContentAreas[j].Height();

					bool bMerge = false;

					if (m_vecContentAreas[i].m_nOCRConfidence >= gnCONFIDENT_OCR_TEXT_SCORE ||
						m_vecContentAreas[j].m_nOCRConfidence >= gnCONFIDENT_OCR_TEXT_SCORE)
					{
						// If either area is based on confidently OCR'd text, only merge based on area if
						// the intersection represents at least gdDUPLICATE_OVERLAP percent of both areas.
						if (dIntersectionArea / m_vecContentAreas[i].getArea() > gdDUPLICATE_OVERLAP &&
							dIntersectionArea / m_vecContentAreas[j].getArea() > gdDUPLICATE_OVERLAP)
						{
							bMerge = true;
						}
					}
					else if (dIntersectionArea / m_vecContentAreas[i].getArea() > gdDUPLICATE_OVERLAP ||
						dIntersectionArea / m_vecContentAreas[j].getArea() > gdDUPLICATE_OVERLAP)
					{
						// If neither area is based on confidently OCR'd text, merge if the intersection 
						// represents at least gdDUPLICATE_OVERLAP percent of either area.
						bMerge = true;
					}

					if (!bMerge)
					{
						// If area i shares mostly the same vertical positioning as area j and not based 
						// on confidently OCR'd text, merge them as two pieces of the same line of content.
						if (dHeightIOverlap > gdDUPLICATE_OVERLAP && 
							m_vecContentAreas[i].m_nOCRConfidence < gnCONFIDENT_OCR_TEXT_SCORE)
						{
							bMerge = true;
						}
						// If area j shares mostly the same vertical positioning as area i and not based
						// on confidently OCR'd text, merge them as two pieces of the same line of content.
						else if (dHeightJOverlap > gdDUPLICATE_OVERLAP && 
							m_vecContentAreas[j].m_nOCRConfidence < gnCONFIDENT_OCR_TEXT_SCORE)
						{
							bMerge = true;
						}
						// If both areas share mostly the same vertical spacing as the other, merge them
						// regardless of whether either is based on confidently OCR'd text.
						else if (dHeightIOverlap > gdDUPLICATE_OVERLAP &&
								 dHeightJOverlap > gdDUPLICATE_OVERLAP)
						{
							bMerge = true;
						}
					}

					if (bMerge)
					{
						ContentAreaInfo mergedArea = m_vecContentAreas[i];

						// These areas qualify to be merged.  Set m_vecContentAreas[j] to the union
						// of the two areas, and remove m_vecContentAreas[i].
						mergedArea.UnionRect(&mergedArea, &m_vecContentAreas[j]);
						mergedArea.m_nOCRConfidence = getOcrConfidenceForRegion(mergedArea,
							ipSearcher);

						// Ensure the resulting area meets the specified size requirements.
						if (areaMeetsSpecifications(mergedArea))
						{
							m_vecContentAreas[i] = mergedArea;

							// Recalculate the horizontally padded version of area i.
							areaIPadded = m_vecContentAreas[i];
							areaIPadded.InflateRect(m_nRequiredHorizontalSeparation * m_sizeAvgChar.cx, 0);

							// Remove the duplicate area j
							m_vecContentAreas.erase(m_vecContentAreas.begin() + j);

							// Reset counter i to force this area to be re-compared to all other zones
							// now that it has been altered.
							i -= 2;
							j = 0;
							break;
						}
					}
				}
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26611");
}
//--------------------------------------------------------------------------------------------------
long CSplitRegionIntoContentAreas::getOcrConfidenceForRegion(const CRect& rectRegion,
															 ISpatialStringSearcherPtr ipSearcher)
{
	ILongRectanglePtr ipRect(CLSID_LongRectangle);
	ASSERT_RESOURCE_ALLOCATION("ELI30329", ipRect != __nullptr);
	ipRect->SetBounds(rectRegion.left, rectRegion.top, rectRegion.right, rectRegion.bottom);

	ISpatialStringPtr ipString = ipSearcher->GetDataInRegion(ipRect, VARIANT_FALSE);
	ASSERT_RESOURCE_ALLOCATION("ELI30330", ipString != __nullptr);

	long nAvgConfidence = 0;
	ipString->GetCharConfidence(NULL, NULL, &nAvgConfidence);

	return nAvgConfidence;
}
//--------------------------------------------------------------------------------------------------
CRect CSplitRegionIntoContentAreas::centerAreaRegionOnBlack(CPoint ptStart, CRect &rrect, 
															const CRect &rectClip, int nMaxRecursions/* = 3*/)
{
	try
	{
		if (rrect.IsRectNull())
		{
			ASSERT_ARGUMENT("ELI22232", ptStart != gptNULL);

			// If searching based on a point, create rrect using ptStart as the center point.
			rrect = CRect(ptStart, ptStart);
			rrect.InflateRect(m_sizeAvgChar.cx / 2, m_sizeAvgChar.cy / 2);
		}
		else if (ptStart == gptNULL)
		{
			// Otherwise, initialize pString by using the center of rrect if necessary.
			ptStart = rrect.CenterPoint();
		}

		// Keep track of the last position of rrect and whether we have gone offscale on either axis.
		CRect rectLast(rrect);
		bool bOffscaleX = false;
		bool bOffscaleY = false;

		// If we are allowed further recursions...
		if (nMaxRecursions > 0)
		{
			nMaxRecursions--;

			// Obtain the average x and y coordinates of the pixels in rrect.
			PixelAverager pixelAverager(this, rrect);
			pixelAverager.process();

			if (pixelAverager.m_nPixelCount == 0)
			{
				// In the existing code, this shouldn't ever be reached. But in case a change is made
				// such that it is reached, just return at this point since we can't center on black
				// pixels if there are none.
				return rrect;
			}

			long nxAvg = 0;
			long nyAvg = 0;
			nxAvg = pixelAverager.m_nXPixelValue / pixelAverager.m_nPixelCount;
			nyAvg = pixelAverager.m_nYPixelValue / pixelAverager.m_nPixelCount;

			// Shift rrect so that its center point is now at the average pixel positoin.
			CPoint pointLastCenter = rrect.CenterPoint();
			rrect.OffsetRect(nxAvg - pointLastCenter.x, nyAvg - pointLastCenter.y);

			// Check to see if the new rrect is offscale either because ptStart is no longer in rrect
			// or because rrect now extends outside of rectClip.  If so flag the problem, and move 
			// rrect back onscale.
			if (ptStart.x < rrect.left)
			{
				bOffscaleX = true;
				rrect.OffsetRect(ptStart.x - rrect.left, 0);
			}
			else if (rrect.left < rectClip.left)
			{
				bOffscaleX = true;
				rrect.OffsetRect(rectClip.left - rrect.left, 0);
			}
			else if (ptStart.x > rrect.right)
			{
				bOffscaleX = true;
				rrect.OffsetRect(ptStart.x - rrect.right, 0);
			}
			else if (rrect.right > rectClip.right)
			{
				bOffscaleX = true;
				rrect.OffsetRect(rectClip.right - rrect.right, 0);
			}

			if (ptStart.y < rrect.top)
			{
				bOffscaleY = true;
				rrect.OffsetRect(0, ptStart.y - rrect.top);
			}
			else if (rrect.top < rectClip.top)
			{
				bOffscaleY = true;
				rrect.OffsetRect(0, rectClip.top - rrect.top);
			}
			else if (ptStart.y > rrect.bottom)
			{
				bOffscaleY = true;
				rrect.OffsetRect(0, ptStart.y - rrect.bottom);
			}
			else if (rrect.bottom > rectClip.bottom)
			{
				bOffscaleY = true;
				rrect.OffsetRect(0, rectClip.bottom - rrect.bottom);
			}
		}

		if (rrect != rectLast && (!bOffscaleX || !bOffscaleY))
		{
			// If rrect has moved and we are not offscale on both axis, recalculate based on the new
			// rrect position.
			return centerAreaRegionOnBlack(ptStart, rrect, rectClip, nMaxRecursions);
		}
		else
		{
			// If rrect hasn't moved, or it moved offscale on both axis, we have our final result.
			// Calculate the rectangle that lies between the current center and original position and
			// assume that for most/all we would arrive at this same position-- report this area
			// for exclusion.
			CRect rectToExclude(ptStart, rrect.CenterPoint());
			rectToExclude.NormalizeRect();

			return rectToExclude;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26612");
}
//--------------------------------------------------------------------------------------------------
bool CSplitRegionIntoContentAreas::expandHorizontally(CRect &rrect, const CRect &rectClip)
{
	try
	{
		// Before we try to expand rrect, shrink it to ensure its borders aren't including
		// unnecessary white space.
		shrinkToFit(rrect, kHorizontal);

		if (rrect.IsRectEmpty())
		{
			// If the rrect is now empty, simply return false;
			return false;
		}

		// The 3 steps to horizontal expansion:
		// 1) Expand to the extents of pixel content using the orginal rrect value.
		// 2) Recenter the expanded area vertically on the pixel y-axis average.
		// 3) In case this put the area in line with more content on either side,
		//    try again to expand it left and right.
		for (int i = 0; i < 2; i++)
		{
			CRect rectExpansionArea(0, rrect.top, m_sizeAvgChar.cx, rrect.bottom);

			int nExpansionCuttoff = m_nRequiredHorizontalSeparation * m_sizeAvgChar.cx;

			// Expand right
			rectExpansionArea.OffsetRect(rrect.right - rectExpansionArea.right, 0);
			expandArea(rrect, rectExpansionArea, CSize(1,0), nExpansionCuttoff, rectClip);

			// Expand left
			rectExpansionArea.OffsetRect(rrect.left - rectExpansionArea.left, 0);
			expandArea(rrect, rectExpansionArea, CSize(-1,0), nExpansionCuttoff, rectClip);

			if (i == 0)
			{
				// The first time around, re-center the content area vertically on the pixel
				// in case the initial position was not well centered on the line.
				centerAreaRegionOnBlack(gptNULL, rrect, 
					CRect(rrect.left, m_rectCurrentPage.top, rrect.right, m_rectCurrentPage.bottom));
			}

			shrinkToFit(rrect, kVertical);
		}

		return !rrect.IsRectEmpty();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26613");
}
//--------------------------------------------------------------------------------------------------
void CSplitRegionIntoContentAreas::expandArea(CRect &rrect, CRect rectExpansionArea,
											  const CSize &sizeExpansionDirection, 
											  int nExpansionCuttoff,
											  const CRect &rectClip)
{
	try
	{
		// rectExpanded represents total potential expansion of the rrect, but not necessarily the 
		// current state of the rect for which immediate pixel content must be found to "apply" the 
		// current possible expansion.
		CRect rectExpanded(rrect);

		// A one-pixel-wide rect (or line) representing the leading edge of the expansion area. 
		CRect rectExpansionEdge(rectExpansionArea);

		if (sizeExpansionDirection == CSize(1, 0))
		{
			rectExpansionEdge.left = rrect.right;
		}
		else if (sizeExpansionDirection == CSize(-1, 0))
		{
			rectExpansionEdge.right = rrect.left;
		}
		else if (sizeExpansionDirection == CSize(0, 1))
		{
			rectExpansionEdge.top = rrect.bottom;
		}
		else if (sizeExpansionDirection == CSize(0, -1))
		{
			rectExpansionEdge.bottom = rrect.top;
		}
		else
		{
			THROW_LOGIC_ERROR_EXCEPTION("ELI22162");
		}

		m_sizeEdgeSearchDirection = sizeExpansionDirection;

		// Calculate the pixel content of the original expansion area. In the loop below rather than
		// re-calculating the pixel content of the entire expansion area each time it is shifted a
		// pixel, instead the pixels at the front and back edge of the expansion area will be added and
		// and subtracted from a running total to keep a moving total of pixels in the expansion area.
		double dExpansionAreaArea =
			(double)(rectExpansionArea.Width() * rectExpansionArea.Height());
		PixelCounter origPixelCounter(this, rectExpansionArea);
		origPixelCounter.process();
		int nLastPixelCount = origPixelCounter.m_nPixelCount;

		// Create a rect repesenting the pixels that are no longer part of the expansion area as of
		// each iteration so they can be subtracted from the running total.
		CRect rectExpansionAreaBack(rectExpansionEdge);
		rectExpansionAreaBack -= CSize(sizeExpansionDirection.cx * (rectExpansionArea.Width() + 1),
			sizeExpansionDirection.cy * (rectExpansionArea.Height() + 1));

		// Continue to loop until we have looped more than nExpansionCuttoff without finding pixels
		// necessary to expand rrect.
		for (int i = 1; i < nExpansionCuttoff; i++)
		{	
			// Shift all expansion retangles in the direction of expansion.
			rectExpansionArea += m_sizeEdgeSearchDirection;
			rectExpansionEdge += m_sizeEdgeSearchDirection;
			rectExpansionAreaBack += m_sizeEdgeSearchDirection;

			// Expand the total potential expantion in the direction of the expansion
			rectExpanded |= (rectExpanded + m_sizeEdgeSearchDirection);

			if ((sizeExpansionDirection.cx == -1 && rectExpanded.left < rectClip.left) ||
				(sizeExpansionDirection.cx == 1 && rectExpanded.right > rectClip.right) ||
				(sizeExpansionDirection.cy == -1 && rectExpanded.top < rectClip.top) ||
				(sizeExpansionDirection.cy == 1 && rectExpanded.bottom > rectClip.bottom))
			{
				// If the expansion has extended outside of the clip rectangle,
				// break off the expansion.
				break;
			}

			// Subtract the new pixels that are no longer part of the expansion area
			PixelCounter removedPixelCounter(this, rectExpansionAreaBack);
			removedPixelCounter.process();

			// Add the new pixels from the leading edge of the expansion area
			PixelCounter addedPixelCounter(this, rectExpansionEdge);
			addedPixelCounter.process();

			// Update the total pixel count
			int nNewPixelCount = nLastPixelCount + 
				addedPixelCounter.m_nPixelCount - 
				removedPixelCounter.m_nPixelCount;

			nLastPixelCount = nNewPixelCount;

			// Calculate the current percentage of black pixels.
			double dPercent = (double) nNewPixelCount / dExpansionAreaArea;

			if (dPercent > gdMIN_PIXEL_PERCENT_OF_AREA)
			{
				// If the area currently contain enough pixels, check to see if there is currently 
				// pixel content along the leading edge.
				PixelEdgeFinder PixelEdgeFinder(this, rectExpansionEdge);
				PixelEdgeFinder.process();

				if (PixelEdgeFinder.m_nPixelCount == 1)
				{
					// If there was pixel content found along the leading edge, expand the rect
					// to the current possible expansion area and reset the counter.
					rrect = rectExpanded;
					i = 0;
				}
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26614");
}
//--------------------------------------------------------------------------------------------------
void CSplitRegionIntoContentAreas::shrinkToFit(CRect &rrect, EOrientation orientation/* = kBoth*/)
{
	try
	{
		// Define a vector of pointers to the values of each of the edges of rrect.
		vector<long *> vecEdges;
		vecEdges.push_back(&(rrect.left));
		vecEdges.push_back(&(rrect.right));
		vecEdges.push_back(&(rrect.top));
		vecEdges.push_back(&(rrect.bottom));

		// Loop through edges appropriate for the current orientation.
		for (int i = ((orientation == kVertical) ? 2 : 0);
			i < ((orientation == kHorizontal) ? 2 : 4);
			i++)
		{
			// Determine the value to add to the current edge to shrink it 
			// (positive for left and top, negative for right and bottom)
			int nDirection = (int) pow((double) -1, (double) i);

			// Create rect to represent the edge being shrunk.
			CRect rectEdge;

			if (i < 2)
			{
				// Set the search direction for the x-axis
				m_sizeEdgeSearchDirection.SetSize(nDirection, 0);

				// Set rectEdge to the left or right edge as appropriate
				rectEdge.SetRect(*(vecEdges[i]), rrect.top, *(vecEdges[i]), rrect.bottom);
			}
			else
			{
				// Set the search direction for the y-axis
				m_sizeEdgeSearchDirection.SetSize(0, nDirection);

				// Set rectEdge to the top or bottom edge as appropriate
				rectEdge.SetRect(rrect.left, *(vecEdges[i]), rrect.right, *(vecEdges[i]));
			}

			// Continue to move the edge inward until pixel content is no longer found
			// (and rrect is not empty)
			while (!rrect.IsRectEmpty())
			{
				rectEdge += m_sizeEdgeSearchDirection;

				PixelEdgeFinder PixelEdgeFinder(this, rectEdge);
				PixelEdgeFinder.process();

				if (PixelEdgeFinder.m_nPixelCount == 1)
				{
					// Pixel content was found, the rrect is shrunk as much as possible on this edge.
					break;
				}

				// Pixel content was not found, move the current edge inward.
				*(vecEdges[i]) += nDirection;
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26615");
}
//--------------------------------------------------------------------------------------------------
bool CSplitRegionIntoContentAreas::attemptMerge(ContentAreaInfo &area, bool bUp, 
												vector<ContentAreaInfo> &rvecAreasToAdd,
												bool bRecurse)
{
	try
	{
		// Cycle through each area looking for an overlapping area
		for (size_t i = 0; i < m_vecContentAreas.size(); i++)
		{
			// Don't compare area to itself.
			if (&area == &(m_vecContentAreas[i]))
			{
				continue;
			}

			CRect rectIntersection;
			if (rectIntersection.IntersectRect(&area, &m_vecContentAreas[i]))
			{
				// An overlapping area was found, make sure the the areas overlap completely
				// along the x-axis before considering a merge.
				int nNarrowestWidth = min(area.Width(), m_vecContentAreas[i].Width());

				ContentAreaInfo &areaHigher(bUp ? m_vecContentAreas[i] : area);
				ContentAreaInfo &areaLower(bUp ? area : m_vecContentAreas[i]);

				if (rectIntersection.Width() == nNarrowestWidth &&
					areaHigher.m_nOCRConfidence < gnCONFIDENT_OCR_TEXT_SCORE &&
					areaLower.m_nOCRConfidence < gnCONFIDENT_OCR_TEXT_SCORE &&
					areaHigher.m_eBottomBoundaryState != kLocked &&
					areaLower.m_eTopBoundaryState != kLocked)
				{
					// The x-axis overlap is complete and neither area is based on confident OCR
					// text or has a locked border... go ahead and combine the areas.
					m_vecContentAreas[i].UnionRect(&area, &m_vecContentAreas[i]);
					return true;
				}

				// Make sure the the zones are positioned in the correct order vertically before
				// trying find a common boundary
				if (rectIntersection.Width() > m_sizeAvgChar.cx && areaHigher.top < areaLower.top)
				{
					// If one area has already locked the boundary and the one with the unlocked
					// boundary isn't confidently OCR'd text, lock it as well.
					if (bUp && 
						areaHigher.m_eBottomBoundaryState == kLocked &&
						areaLower.m_nOCRConfidence < gnCONFIDENT_OCR_TEXT_SCORE)
					{
						areaLower.m_eTopBoundaryState = kLocked;
						continue;
					}
					else if (!bUp &&
						areaLower.m_eTopBoundaryState == kLocked &&
						areaHigher.m_nOCRConfidence < gnCONFIDENT_OCR_TEXT_SCORE)
					{
						areaHigher.m_eBottomBoundaryState = kLocked;
						continue;
					}

					// Determine if one area intersects with confidently OCR'd text from the other.
					CRect rectOCRIntersection;
					ContentAreaInfo *pConfidentArea = NULL;
					ContentAreaInfo *pImpressionableArea = NULL;

					if (areaHigher.m_nOCRConfidence > gnCONFIDENT_OCR_TEXT_SCORE &&
						areaLower.m_nOCRConfidence < areaHigher.m_nOCRConfidence)
					{
						rectOCRIntersection = areaHigher.m_rectOriginal;
						rectOCRIntersection.top = areaHigher.top;
						rectOCRIntersection.bottom = areaHigher.bottom;

						// If areaLower intersects confidently OCR'd text from areaHigher, have 
						// confidence in areaHigher's boundary and consider areaLower's impressionable.
						if (rectOCRIntersection.IntersectRect(&rectOCRIntersection, &areaLower))
						{
							pConfidentArea = &areaHigher;
							pImpressionableArea = &areaLower;
						}
					}
					else if (areaLower.m_nOCRConfidence > gnCONFIDENT_OCR_TEXT_SCORE &&
						areaHigher.m_nOCRConfidence < areaLower.m_nOCRConfidence)
					{
						rectOCRIntersection = areaLower.m_rectOriginal;
						rectOCRIntersection.top = areaLower.top;
						rectOCRIntersection.bottom = areaLower.bottom;

						// If areaHigher intersects confidently OCR'd text from areaLower, have 
						// confidence in areaLower's boundary and consider areaHigher's impressionable.
						if (rectOCRIntersection.IntersectRect(&rectOCRIntersection, &areaHigher))
						{
							pConfidentArea = &areaLower;
							pImpressionableArea = &areaHigher;
						}
					}

					// If neither area's boundary is confident or impressionable, move on.
					if (pConfidentArea == NULL || pImpressionableArea == NULL)
					{
						continue;
					}

					// Determine the needed elements of the confident and impressionable areas.
					long nNewBoundary((pConfidentArea == &areaHigher) 
						? areaHigher.m_rectOriginal.bottom
						: areaLower.m_rectOriginal.top);
					long &rnConfidentBoundary((pConfidentArea == &areaHigher) 
						? areaHigher.bottom
						: areaLower.top);
					long &rnImpressionableBoundary((pImpressionableArea == &areaHigher) 
						? areaHigher.bottom
						: areaLower.top);
					EBoundaryState &reConfidentBoundaryState((pConfidentArea == &areaHigher)
						? areaHigher.m_eBottomBoundaryState
						: areaLower.m_eTopBoundaryState);
					EBoundaryState &reImpressionableBoundaryState(
						(pImpressionableArea == &areaHigher)
						? areaHigher.m_eBottomBoundaryState
						: areaLower.m_eTopBoundaryState);

					// If the impressionable area completely spans the confident area verically, create
					// a new area on the other side of the confident area.
					if (pImpressionableArea == &areaHigher &&
						areaHigher.bottom > areaLower.bottom + m_sizeAvgChar.cy)
					{
						ContentAreaInfo newArea(*pImpressionableArea);
						newArea.top = areaLower.bottom;
						newArea.m_eTopBoundaryState = kLocked;
						rvecAreasToAdd.push_back(newArea);
					}		
					else if (pImpressionableArea == &areaLower &&
						areaLower.top < areaHigher.top - m_sizeAvgChar.cy)
					{
						ContentAreaInfo newArea(*pImpressionableArea);
						newArea.bottom = areaHigher.top;
						newArea.m_eBottomBoundaryState = kLocked;
						rvecAreasToAdd.push_back(newArea);
					}

					// Adjust and lock the impressionable area's boundary.
					reImpressionableBoundaryState = kLocked;
					rnImpressionableBoundary = nNewBoundary;

					if ((areaHigher.m_nOCRConfidence > gnCONFIDENT_OCR_TEXT_SCORE && 
						areaLower.m_nOCRConfidence > gnCONFIDENT_OCR_TEXT_SCORE) || 
						isBigEnough(*pImpressionableArea, true))
					{
						// If bRecurse == true and both areas are based on well OCR'd text or if the
						// horizontal extent of the confident area is largely shared by the
						// impressionable area and the impressionable area is not likely to thrown out
						// because it is too short, lock the confident area's boundary too.  But first,
						// attempt to merge it with all other areas to lock the boundary of any other
						// overlapping impressionable areas.
						if (bRecurse)
						{
							bool bMergeUp = (bUp == (pConfidentArea == m_vecContentAreas[i]));
							attemptMerge(*pConfidentArea, bMergeUp, rvecAreasToAdd, false);
						}

						// If the area based on confidently OCR'd text extends sufficiently beyond the 
						// left or right end of the intersection, create a new area that is a subset of
						// the original confident area that will attempt to expand up or down alongside
						// the found intersection.
						if (pConfidentArea->left < rectIntersection.left - m_sizeAvgChar.cx)
						{
							ContentAreaInfo newArea = *pConfidentArea;
							newArea.m_nOCRConfidence = 0;
							newArea.right = rectIntersection.left;
							rvecAreasToAdd.push_back(newArea);
						}
						if (pConfidentArea->right > rectIntersection.right + m_sizeAvgChar.cx)
						{
							ContentAreaInfo newArea = *pConfidentArea;
							newArea.m_nOCRConfidence = 0;
							newArea.left = rectIntersection.right;
							rvecAreasToAdd.push_back(newArea);
						}

						reConfidentBoundaryState = kLocked;
						rnConfidentBoundary = nNewBoundary;
					}

					if (pImpressionableArea->left + m_sizeAvgChar.cx < rectIntersection.left)
					{
						// The truncated area extends sufficiently beyond the left end of the
						// intersection; create a new candidate area.
						ContentAreaInfo newArea(rectIntersection);
						newArea.left = pImpressionableArea->left;
						newArea.right = rectIntersection.left;
						shrinkToFit(newArea);
						if (hasEnoughPixels(newArea) && 
							isBigEnough(newArea))
						{
							// The new candidate qualifies to be its own area. Consider its top
							// and bottom boundaries already expanded.
							newArea.m_eTopBoundaryState = kLocked;
							newArea.m_eBottomBoundaryState = kLocked;
							rvecAreasToAdd.push_back(newArea);
						}
					}
					if (pImpressionableArea->right - m_sizeAvgChar.cx > rectIntersection.right)
					{
						// The impressionable area extends sufficiently beyond the right end of the
						// intersection; create a new candidate area.
						ContentAreaInfo newArea(rectIntersection);
						newArea.right = pImpressionableArea->right;
						newArea.left = rectIntersection.right;
						shrinkToFit(newArea);
						if (hasEnoughPixels(newArea) && 
							isBigEnough(newArea))
						{
							// The new candidate qualifies to be its own area. Consider its top
							// and bottom boundaries already expanded.
							newArea.m_eTopBoundaryState = kLocked;
							newArea.m_eBottomBoundaryState = kLocked;
							rvecAreasToAdd.push_back(newArea);
						}
					}	
				}
			}
		}

		return false;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26616");
}
//--------------------------------------------------------------------------------------------------
CSplitRegionIntoContentAreas::ContentAreaInfo CSplitRegionIntoContentAreas::makeFlushWithLines(
	ContentAreaInfo area)
{
	try
	{
		// Cycle through each horizontal line looking for a line that defines the edge of this area
		for each (LineRect line in m_vecHorizontalLines)
		{
			if (area.bottom + 1 == line.top || area.top - 1 == line.bottom)
			{
				// We found a line that matches the top or bottom edge.  Make sure the line runs at
				// least 50% of the width of the area.
				int nOverlap = min(area.right, line.right) - max(area.left, line.left);
				double dOverlapPercent = (double)nOverlap / area.Width();

				if (dOverlapPercent > gdREQUIRED_LINE_OVERLAP)
				{
					// This line appears to define the edge of this area.  Align the area's edge with
					// the center of the line.
					if (area.bottom + 1 == line.top)
					{
						area.bottom = line.CenterPoint().y;
					}
					else
					{
						area.top = line.CenterPoint().y;
					}
				}
			}
			else if (line.LinePosition() >= area.top + m_sizeAvgChar.cy &&
				line.LinePosition() <= area.bottom - m_sizeAvgChar.cy)
			{
				// This line appears to split the current area with a sufficient amount of leftover on
				// either side.  If the horizontal overlap is sufficient, allow this area to be divided
				// in two along this line.
				int nOverlap = min(area.right, line.right) - max(area.left, line.left);
				double dOverlapPercent = (double)nOverlap / area.Width();

				if (dOverlapPercent > gdREQUIRED_LINE_OVERLAP)
				{
					CRect newArea(area.left, area.top, area.right, line.LinePosition());
					shrinkToFit(newArea);
					m_vecContentAreas.push_back(newArea);

					area.top = line.LinePosition();
					shrinkToFit(area);
				}
			}
		}

		// Cycle through each vertical line looking for a line that defines the edge of this area
		for each (LineRect line in m_vecVerticalLines)
		{
			if (area.right + 1 == line.left || area.left - 1 == line.right)
			{
				// We found a line that matches the left or right edge.  Make sure the line runs at
				// least 50% of the height of the area.
				int nOverlap = min(area.bottom, line.bottom) - max(area.top, line.top);
				double dOverlapPercent = (double)nOverlap / area.Height();

				if (dOverlapPercent > gdREQUIRED_LINE_OVERLAP)
				{
					// This line appears to define the edge of this area.  Align the area's edge with
					// the center of the line.
					if (area.right + 1 == line.left)
					{
						area.right = line.CenterPoint().x;
					}
					else
					{
						area.left = line.CenterPoint().x;
					}
				}
			}
			// The following code to allow an area to be divided by a vertical line seems to expose a bug in 
			// the spatial string searcher on one of the documents (9519303_001.TIF). Commenting out for the
			// time being.
			//		else if (line.LinePosition() >= area.left + (m_sizeAvgChar.cx * m_nRequiredHorizontalSeparation) &&
			//				 line.LinePosition() <= area.right - (m_sizeAvgChar.cx * m_nRequiredHorizontalSeparation))
			//		{
			//			// This line appears to split the current area with a sufficient amount of leftover on
			//			// either side.  If the vertical overlap is sufficient, allow this area to be be
			//			// divded in two along this line.
			//			int nOverlap = min(area.bottom, line.bottom) - max(area.top, line.top);
			//			double dOverlapPercent = (double)nOverlap / area.Height();
			//
			//			if (dOverlapPercent > gdREQUIRED_LINE_OVERLAP)
			//			{
			//				CRect newArea(area.left, area.top, line.LinePosition(), area.bottom);
			//				shrinkToFit(newArea);
			//				m_vecContentAreas.push_back(newArea);
			//
			//				area.left = line.LinePosition();
			//				shrinkToFit(area);
			//			}
			//		}
		}

		return area;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26617");
}
//--------------------------------------------------------------------------------------------------
bool CSplitRegionIntoContentAreas::ensureRectInPage(CRect &rrect, bool bThrowException/* = true*/)
{
	if (rrect.right < m_rectCurrentPage.left ||
		rrect.left > m_rectCurrentPage.right ||
		rrect.bottom < m_rectCurrentPage.top ||
		rrect.top > m_rectCurrentPage.bottom)
	{
		// rrect does not overlap with the current page at all.  Throw an exception or return 
		// false as directed.
		if (bThrowException)
		{
			throw UCLIDException("ELI22154", "Internal error: Image region not found!");
		}

		return false;
	}
	else
	{
		// rrect overlaps with the current page. Crop it so that it includes only the area
		// that overlaps.
		// [FlexIDSCore #3560] - Ensure the rect is (bounds - 1) for bottom and right
		// as the image is stored in a 0 based array structure
		rrect.SetRect(max(rrect.left, m_rectCurrentPage.left),
					  max(rrect.top, m_rectCurrentPage.top),
					  min(rrect.right, m_rectCurrentPage.right-1),
					  min(rrect.bottom, m_rectCurrentPage.bottom-1));
	}

	return true;
}
//--------------------------------------------------------------------------------------------------
bool CSplitRegionIntoContentAreas::isBigEnough(const ContentAreaInfo &area, 
											   bool bCheckHeightOnly/* = false*/)
{
	if (!bCheckHeightOnly && 
		area.Width() + 1 < (int) ((double) m_sizeAvgChar.cx * m_dMinimumWidth - 0.5))
	{
		// Ensure the width of the area is at least the width of the number of chars specified.
		return false;
	}
	else if (area.Height() < (int) ((double) m_sizeAvgChar.cy * m_dMinimumHeight))
	{
		// Ensure the height of the area is at least the height of the number of chars specified.
		return false;
	}
	else
	{
		// Big enough
		return true;
	}
}
//--------------------------------------------------------------------------------------------------
bool CSplitRegionIntoContentAreas::hasEnoughPixels(const CRect &rect)
{	
	try
	{
		// If the rect is empty, it doesn't have enough pixels.
		if (rect.IsRectEmpty())
		{
			return false;
		}

		// Count the pixels in this rect.
		PixelCounter pixelCounter(this, rect);
		pixelCounter.process();

		// Calculate the percentage of the rect that has black pixels.
		double dArea = (double)(rect.Width() * rect.Height());
		double dPercent = (double) pixelCounter.m_nPixelCount / dArea;

		return (dPercent > gdMIN_PIXEL_PERCENT_OF_AREA);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26618");
}
//--------------------------------------------------------------------------------------------------
bool CSplitRegionIntoContentAreas::areaMeetsSpecifications(const ContentAreaInfo &area)
{
	// If not including areas based on well-OCR'd text, ensure the area is not based on well OCR'd
	// text.
	if (!m_bIncludeGoodOCR && area.m_nOCRConfidence >= m_nOCRThreshold)
	{
		return false;
	}
	// If not including areas based on poorly-OCR'd text, ensure the area is based on well OCR'd
	// text.
	else if (!m_bIncludePoorOCR && area.m_nOCRConfidence < m_nOCRThreshold)
	{
		return false;
	}
	// Ensure the area meets the required size.
	else if (!isBigEnough(area))
	{
		return false;
	}
	// Make sure the area has enough pixels
	else if (!hasEnoughPixels(area))
	{
		return false;
	}

	return true;
}
//--------------------------------------------------------------------------------------------------
bool CSplitRegionIntoContentAreas::isExcluded(CPoint &rpoint)
{
	// See if the bounds of this point lie within any of the excluded areas.
	for each (CRect rectExcluded in m_vecExcludedAreas)
	{
		if (rpoint.x >= rectExcluded.left &&
			rpoint.x <= rectExcluded.right &&
			rpoint.y >= rectExcluded.top &&
			rpoint.y <= rectExcluded.bottom)
		{
			// Inform the the caller of the of the last pixel in the excluded area for this row.
			rpoint.x = rectExcluded.right;

			return true;
		}
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
bool CSplitRegionIntoContentAreas::isPointOnPage(const CPoint& point)
{
	return point.x >= 0 && point.x < m_rectCurrentPage.right &&
			point.y >= 0 && point.y < m_rectCurrentPage.bottom;
}
//--------------------------------------------------------------------------------------------------
bool CSplitRegionIntoContentAreas::isAreaAbove(const CRect &rect1, const CRect &rect2)
{
	CPoint ptRect1Center = rect1.CenterPoint();
	CPoint ptRect2Center = rect2.CenterPoint();

	if (rect1.top < ptRect2Center.y && rect1.bottom > ptRect2Center.y ||
		rect2.top < ptRect1Center.y && rect2.bottom > ptRect1Center.y)
	{
		// The two rectangles appear to be roughly level vertically
		if (rect1.left < ptRect2Center.x && rect1.right > ptRect2Center.x ||
			rect2.left < ptRect1Center.x && rect2.right > ptRect1Center.x)
		{
			// If the two areas also are roughly aligned horizontally, return whichever
			// is higher
			return (ptRect1Center.y < ptRect2Center.y);
		}
		else
		{
			// If the two areas are largely horizontally separate from one another, return the one
			// to the left of the other
			return (ptRect1Center.x < ptRect2Center.x);
		}
	}
	else
	{
		// If the rectangles are not close vertically, return the higher one.
		return (ptRect1Center.y < ptRect2Center.y);
	}
}
//--------------------------------------------------------------------------------------------------
IAttributePtr CSplitRegionIntoContentAreas::createResult(IAFDocumentPtr ipDoc, long nPage,
														 ContentAreaInfo area,
														 ILongToObjectMapPtr ipSpatialInfos)
{
	try
	{
		// Determine the name of the source document.
		ISpatialStringPtr ipDocText = ipDoc->Text;
		ASSERT_RESOURCE_ALLOCATION("ELI22429", ipDocText != __nullptr);

		string strSourceDocName = asString(ipDocText->SourceDocName);

		// Pad the specified rect; this helps to obtain better OCR results of the area.
		area.InflateRect(gnAREA_PADDING_SIZE, gnAREA_PADDING_SIZE);

		// Crop the rect to ensure it is completely contained in the current page.
		ensureRectInPage(area);

		// Create a new ILongRectanglePtr based on this rect
		ILongRectanglePtr ipRect(CLSID_LongRectangle);
		ASSERT_RESOURCE_ALLOCATION("ELI22153", ipRect != __nullptr);

		ipRect->SetBounds(area.left, area.top, area.right, area.bottom);

		// Search for OCR'd text within this region.
		ISpatialStringSearcherPtr ipSearcher = getSpatialStringSearcher(ipDoc, nPage);
		ASSERT_RESOURCE_ALLOCATION("ELI22182", ipSearcher != __nullptr);

		ipSearcher->SetIncludeDataOnBoundary(VARIANT_FALSE);

		ISpatialStringPtr ipValue = ipSearcher->GetDataInRegion(ipRect, VARIANT_FALSE);
		ASSERT_RESOURCE_ALLOCATION("ELI22183", ipValue != __nullptr);

		// Create a raster zone representing the entire area of the area.
		IRasterZonePtr ipNewRasterZone(CLSID_RasterZone);
		ASSERT_RESOURCE_ALLOCATION("ELI22185", ipNewRasterZone != __nullptr);
		ipNewRasterZone->CreateFromLongRectangle(ipRect, m_nCurrentPage);

		// Create a ContentAreaInfo based on the found text in order to assess the OCR confidence
		// of the text.
		ContentAreaInfo result(ipValue, ipSpatialInfos);

		// If OCR quality of the text this area is based off of or the OCR quality of text in the
		// final area is poor, re-OCR the text using handwriting recognition if so specified.
		if (m_bReOCRWithHandwriting && 
			(area.m_nOCRConfidence < gnCONFIDENT_OCR_TEXT_SCORE ||
			result.m_nOCRConfidence < gnCONFIDENT_OCR_TEXT_SCORE))
		{
			try
			{
				try
				{
					// Load the OCR engine and OCR this image region.
					IOCREnginePtr ipOCREngine = getOCREngine();

					// Re-OCR this image region from the original image (lines included)
					ISpatialStringPtr ipZoneText = ipOCREngine->RecognizeTextInImageZone(
						strSourceDocName.c_str(), nPage, nPage, ipRect, 0, kNoFilter, "",
						VARIANT_TRUE, VARIANT_TRUE, VARIANT_TRUE, NULL);

					// Create a new area based on the OCR'd text to score the OCR confidence.
					ContentAreaInfo areaReOCRd(ipZoneText, ipSpatialInfos);

					// Update to use the handwritten result if confidence is higher.
					if (areaReOCRd.m_nOCRConfidence > result.m_nOCRConfidence)
					{
						ipValue = ipZoneText;
					}
				}
				CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI22335");
			}
			catch (UCLIDException &ue)
			{
				// In testing so far, re-OCR'ing the small image region appears more likely to 
				// generate OCR exceptions such as all decompsition methods failed.  Since
				// we are only trying to improve on a result we already have, don't consider
				// this a critical problem; Just log it.
				UCLIDException uexOuter("ELI22336", 
					"Application trace: Failed to perform handwriting re-OCR attempt!", ue);
				uexOuter.log();
			}
		}

		// If a sub attribute containing the true attribute spatial info is requested, this
		// variable will store it.
		IAttributePtr ipSpatialStringSubAttribute = __nullptr;

		// Check to see if the string value of the attribute is empty
		string strValueText = asString(ipValue->String);
		strValueText = trim(strValueText, "", " \r\n");
		if (strValueText.empty() || !asCppBool(ipValue->HasSpatialInfo()))
		{
			// If so, specify create a pseudo-spatial string with the default text instead.
			ipValue->CreatePseudoSpatialString(ipNewRasterZone, m_strDefaultAttributeText.c_str(),
				m_ipCurrentPageText->SourceDocName, ipSpatialInfos);
		}
		else
		{
			// Handle the case that the user has requested the original spatial information.
			if (m_bIncludeOCRAsTrueSpatialString)
			{
				// Create the attribute to contain the spatial string
				ipSpatialStringSubAttribute.CreateInstance(CLSID_Attribute);
				ASSERT_RESOURCE_ALLOCATION("ELI22549", ipSpatialStringSubAttribute != __nullptr);

				// Create and use a copy of the originally found value.
				ICopyableObjectPtr ipCopyThis = ipValue;
				ASSERT_RESOURCE_ALLOCATION("ELI22551", ipCopyThis);

				ISpatialStringPtr ipValueCopy = ipCopyThis->Clone();
				ASSERT_RESOURCE_ALLOCATION("ELI22552", ipValueCopy);

				ipSpatialStringSubAttribute->Value = ipValueCopy;
				ipSpatialStringSubAttribute->Name = gstrSPATIAL_STRING_ATTRIBUTE_NAME.c_str();

				// Assign the type based on the character confidence of the text the area is based on.
				ipSpatialStringSubAttribute->Type = 
					(area.m_nOCRConfidence >= m_nOCRThreshold)
					? m_strGoodOCRType.c_str()
					: m_strPoorOCRType.c_str();
			}

			// Replace the new lines with spaces on the text itself
			// [FlexIDSCore #3436]
			replaceVariable(strValueText, "\r\n", " ");

			// Create the return result as a psuedo-spatial string-- a spatial string with
			// the letters it contains spread evenly throughout the found region.
			ipValue->CreatePseudoSpatialString(ipNewRasterZone, strValueText.c_str(), 
				m_ipCurrentPageText->SourceDocName, ipSpatialInfos);
		}

		// Create the new attribute and assign the newly created value.
		IAttributePtr ipNewAttribute(CLSID_Attribute);
		ASSERT_RESOURCE_ALLOCATION("ELI22184", ipNewAttribute != __nullptr);

		ipNewAttribute->Value = ipValue;
		ipNewAttribute->Name = m_strAttributeName.c_str();

		// Assign the type based on the character confidence of the text the area is based on.
		ipNewAttribute->Type = (area.m_nOCRConfidence >= m_nOCRThreshold)
			? m_strGoodOCRType.c_str()
			: m_strPoorOCRType.c_str();

		// If an attribute containing the original spatial string result is available, add it
		// as a sub-attribute.
		if (ipSpatialStringSubAttribute != __nullptr)
		{
			IIUnknownVectorPtr ipSubAttributes = ipNewAttribute->SubAttributes;
			ASSERT_RESOURCE_ALLOCATION("ELI22550", ipSubAttributes != __nullptr);

			ipSubAttributes->PushBack(ipSpatialStringSubAttribute);
		}

		return ipNewAttribute;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26619");
}
//--------------------------------------------------------------------------------------------------
bool CSplitRegionIntoContentAreas::loadPageBitmap(IAFDocumentPtr ipDoc, long nPage)
{
	try
	{
		// Call setCurrentPage which will reset m_apPageBitmap if this is a different page than for the
		// last call.
		ISpatialStringPtr ipPageText = setCurrentPage(ipDoc, nPage);

		if (m_apPageBitmap.get() == NULL)
		{
			// If this page doesn't have any text, skip it. (It would be possible to handle it, but 
			// its not worth the benefit at this point.
			if (ipPageText == __nullptr || !asCppBool(ipPageText->HasSpatialInfo()))
			{
				// Reset any previously loaded bitmap
				m_apPageBitmap.reset();

				// Reset any existing line data and attempt to find lines on this page
				m_vecHorizontalLines.clear();
				m_vecVerticalLines.clear();

				return false;
			}

			// The page bitmap needs to be loaded. Load the bitmap deskewed.
			ISpatialPageInfoPtr ipPageInfo = ipPageText->GetPageInfo(nPage);
			ASSERT_RESOURCE_ALLOCATION("ELI22125", ipPageInfo != __nullptr);

			double dRotation = ipPageInfo->Deskew;

			// Determine which way to orient the search based on the page text orientation.
			switch (ipPageInfo->Orientation)
			{
				case kRotNone: dRotation += 0; break;
				case kRotLeft: dRotation += 90; break;
				case kRotDown: dRotation += 180; break;
				case kRotRight: dRotation += 270; break;
			}

			m_apPageBitmap.reset(new LeadToolsBitmap(asString(ipPageText->SourceDocName), nPage, 
				-dRotation));
			ASSERT_RESOURCE_ALLOCATION("ELI22124", m_apPageBitmap.get() != __nullptr);

			m_rectCurrentPage.SetRect(0, 0,
				m_apPageBitmap->m_hBitmap.Width, m_apPageBitmap->m_hBitmap.Height);

			// Reset any existing line data and attempt to find lines on this page
			m_vecHorizontalLines.clear();
			m_vecVerticalLines.clear();

			if (m_bUseLines)
			{
				LeadToolsLineFinder ltLineFinder;
				
				// [FlexIDSCore:3438]
				// In most cases when trying to find lines, the emphasis is on not missing lines
				// and, therefore, having settings that will frequently produce "false-positives"
				// by identifying printed text and other markings that aren't actually lines as
				// lines. However, in the split region rule, the emphasis needs to be more on
				// ensuring content is covered and not on identifying all lines. Therefore, tighten
				// the line-finding settings for SRICA.
				// Lowering the GapLength makes it less likely that words will be counted as lines
				ltLineFinder.m_lr.iGapLength = 5;
				// Lowering the iWall decreases the chance that handwriting will be interpreted as
				// a line (and also that thick lines will be found).
				ltLineFinder.m_lr.iWall = 25;

				ltLineFinder.findLines(&(m_apPageBitmap->m_hBitmap), 
					LINEREMOVE_HORIZONTAL, m_vecHorizontalLines);
				ltLineFinder.findLines(&(m_apPageBitmap->m_hBitmap), 
					LINEREMOVE_VERTICAL, m_vecVerticalLines);

				// Erase the pixels from all lines found on the page.
				for each (CRect rect in m_vecHorizontalLines)
				{
					PixelEraser pixelEraser(this, rect);
					pixelEraser.process();
				}

				for each (CRect rect in m_vecVerticalLines)
				{
					PixelEraser pixelEraser(this, rect);
					pixelEraser.process();
				}
			}
		}
		
		return true;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26620");
}
//--------------------------------------------------------------------------------------------------
ISpatialStringSearcherPtr CSplitRegionIntoContentAreas::getSpatialStringSearcher(
																IAFDocumentPtr ipDoc, long nPage)
{
	// Lazy instantiation and initiation of a SpatialString searcher for the current page.
	ISpatialStringPtr ipPageText = setCurrentPage(ipDoc, nPage);

	if (m_ipSpatialStringSearcher == __nullptr && ipPageText != __nullptr)
	{
		m_ipSpatialStringSearcher.CreateInstance(CLSID_SpatialStringSearcher);
		ASSERT_RESOURCE_ALLOCATION("ELI22100", m_ipSpatialStringSearcher != __nullptr);

		m_ipSpatialStringSearcher->InitSpatialStringSearcher(ipPageText);
	}

	return m_ipSpatialStringSearcher;
}
//-------------------------------------------------------------------------------------------------
IOCREnginePtr CSplitRegionIntoContentAreas::getOCREngine()
{
	if (m_ipOCREngine == __nullptr)
	{
		m_ipOCREngine.CreateInstance(CLSID_ScansoftOCR);
		ASSERT_RESOURCE_ALLOCATION("ELI22253", m_ipOCREngine != __nullptr );
		
		IPrivateLicensedComponentPtr ipOCREngineLicense(m_ipOCREngine);
		ASSERT_RESOURCE_ALLOCATION("ELI22240", ipOCREngineLicense != __nullptr);
		ipOCREngineLicense->InitPrivateLicense(LICENSE_MGMT_PASSWORD.c_str());
	}

	return m_ipOCREngine;
}
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr CSplitRegionIntoContentAreas::setCurrentPage(IAFDocumentPtr ipDoc, long nPage)
{
	ASSERT_ARGUMENT("ELI22235", ipDoc != __nullptr);

	// Lazy initiation of data for the current page.
	if (ipDoc != m_ipCurrentDoc || nPage != m_nCurrentPage)
	{
		// We're using a different page than before. Reset any loaded bitmap or spatial string
		// searcher to force them to be re-loaded for the current page.
		m_ipSpatialStringSearcher = __nullptr;
		m_apPageBitmap.reset();

		m_ipCurrentDoc = ipDoc;
		m_nCurrentPage = nPage;
		m_ipCurrentPageText = __nullptr;

		ISpatialStringPtr ipDocText = ipDoc->Text;
		if (ipDocText)
		{
			m_ipCurrentPageText = ipDocText->GetSpecifiedPages(m_nCurrentPage, m_nCurrentPage);
			ASSERT_RESOURCE_ALLOCATION("ELI22111", m_ipCurrentPageText != __nullptr);
		}
	}

	return m_ipCurrentPageText;
}
//-------------------------------------------------------------------------------------------------
void CSplitRegionIntoContentAreas::reset()
{
	m_strDefaultAttributeText = gstrDEFAULT_ATTRIBUTE_TEXT;
	m_strAttributeName = gstrDEFAULT_ATTRIBUTE_NAME;
	m_dMinimumWidth = gdDEFAULT_MINIMUM_WIDTH;
	m_dMinimumHeight = gdDEFAULT_MINIMUM_HEIGHT;
	m_bIncludeGoodOCR = true;
	m_bIncludePoorOCR = true;
	m_strGoodOCRType = gstrDEFAULT_GOOD_OCR_TYPE;
	m_strPoorOCRType = gstrDEFAULT_POOR_OCR_TYPE;
	m_nOCRThreshold = gnDEFAULT_OCR_THRESHOLD;
	m_bUseLines = true;
	m_bReOCRWithHandwriting = false;
	m_bIncludeOCRAsTrueSpatialString = false;
	m_nRequiredHorizontalSeparation = gnMIN_CHAR_SEPARATION_OF_LINES;
}
//-------------------------------------------------------------------------------------------------
void CSplitRegionIntoContentAreas::validateHandwritingLicense()
{
	// If handwriting recognition is not needed, return immediately
	if (!m_bReOCRWithHandwriting)
	{
		return;
	}

	try
	{
		try
		{
			VALIDATE_LICENSE(gnHANDWRITING_RECOGNITION_FEATURE, "ELI22346", 
				"SplitRegionIntoContentAreas");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI22347")
	}
	catch (UCLIDException &ue)
	{
		m_bReOCRWithHandwriting = false;

		UCLIDException uexOuter("ELI22348", 
					"Re-OCR for handwritten areas will not be used due to missing license!", ue);
		uexOuter.log();
	}
}
//-------------------------------------------------------------------------------------------------
void CSplitRegionIntoContentAreas::validateLicense()
{
	VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI22087", 
		"Split region into content areas");
}
//-------------------------------------------------------------------------------------------------
