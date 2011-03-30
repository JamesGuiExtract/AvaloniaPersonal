// ImageRegionWithLines.cpp : Implementation of CImageRegionWithLines

#include "stdafx.h"
#include "ImageRegionWithLines.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>
#include <Misc.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;
const int gnMIN_ZONE_HEIGHT = 20;
const int gnMIN_ZONE_WIDTH = 20;

//--------------------------------------------------------------------------------------------------
// CImageRegionWithLines
//--------------------------------------------------------------------------------------------------
CImageRegionWithLines::CImageRegionWithLines() :
	m_ipImageLineUtility(NULL),
	m_ePageSelectionMode(kAllPages),
	m_nNumFirstPages(0),
	m_nNumLastPages(0),
	m_bIncludeLines(false)
{
}
//--------------------------------------------------------------------------------------------------
CImageRegionWithLines::~CImageRegionWithLines()
{
	try
	{
		m_ipImageLineUtility = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI18374");
}
//--------------------------------------------------------------------------------------------------
HRESULT CImageRegionWithLines::FinalConstruct()
{
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
void CImageRegionWithLines::FinalRelease()
{
}

//--------------------------------------------------------------------------------------------------
// IImageRegionWithLines
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CImageRegionWithLines::get_LineUtil(IUnknown **ppVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18842", ppVal != __nullptr);

		validateLicense();

		IImageLineUtilityPtr ipShallowCopy = getImageLineUtility();
		*ppVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18840")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageRegionWithLines::put_LineUtil(IUnknown *pNewVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_ipImageLineUtility = pNewVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18841")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageRegionWithLines::get_PageSelectionMode(EPageSelectionMode *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18917", pVal != __nullptr);

		validateLicense();

		*pVal = m_ePageSelectionMode;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18918")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageRegionWithLines::put_PageSelectionMode(EPageSelectionMode newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_ePageSelectionMode = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18919")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageRegionWithLines::get_NumFirstPages(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18920", pVal != __nullptr);

		validateLicense();

		*pVal = m_nNumFirstPages;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18921")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageRegionWithLines::put_NumFirstPages(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18959", newVal >= 0);

		validateLicense();

		m_nNumFirstPages = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18922")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageRegionWithLines::get_NumLastPages(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18923", pVal != __nullptr);

		validateLicense();

		*pVal = m_nNumLastPages;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18924")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageRegionWithLines::put_NumLastPages(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18960", newVal >= 0);

		validateLicense();

		m_nNumLastPages = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18925")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageRegionWithLines::get_SpecifiedPages(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18926", pVal != __nullptr);

		validateLicense();

		*pVal = get_bstr_t(m_strSpecifiedPages).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18927")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageRegionWithLines::put_SpecifiedPages(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		string strPages = asString(newVal);

		// If a value is provided, validate the value
		if (strPages != "")
		{
			validatePageNumbers(strPages);
		}

		m_strSpecifiedPages = strPages;
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18928")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageRegionWithLines::get_AttributeText(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18929", pVal != __nullptr);

		validateLicense();

		*pVal = get_bstr_t(m_strAttributeText).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18930")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageRegionWithLines::put_AttributeText(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_strAttributeText = asString(newVal);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18931")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageRegionWithLines::get_IncludeLines(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18932", pVal != __nullptr);

		validateLicense();

		*pVal = asVariantBool(m_bIncludeLines);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18933")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageRegionWithLines::put_IncludeLines(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bIncludeLines = asCppBool(newVal);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18934")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IAttributeFindingRule
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageRegionWithLines::raw_ParseText(IAFDocument* pAFDoc, IProgressStatus *pProgressStatus,
													IIUnknownVector **ppAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{		
		validateLicense();

		IAFDocumentPtr ipAFDoc(pAFDoc);
		ASSERT_ARGUMENT("ELI18375", ipAFDoc != __nullptr);
		ISpatialStringPtr ipAFDocText(ipAFDoc->Text);
		ASSERT_ARGUMENT("ELI19041", ipAFDocText != __nullptr);
		ASSERT_ARGUMENT("ELI18376", ppAttributes != __nullptr);

		// Create an attribute vector to store the results
		IIUnknownVectorPtr ipAttributes(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI18399", ipAttributes != __nullptr );

		// Populate a vector of ints that indicates which pages
		// to process
		vector<int> vecPages;

		// [FlexIDSCore:3069] Only populate vector of pages if the document text has spatial information.
		if (ipAFDocText->GetMode() != kNonSpatialMode)
		{
			long nLastPageNumber = ipAFDocText->GetLastPageNumber();

			if (m_ePageSelectionMode == kAllPages)
			{
				// Include all pages
				for(int i = 1; i <= nLastPageNumber; i++)
				{
					vecPages.push_back(i);
				}
			}
			else if (m_ePageSelectionMode == kFirstPages)
			{
				// Include the first [m_nNumFirstPages] pages
				for(int i = 1; i <= m_nNumFirstPages && i <= nLastPageNumber; i++)
				{
					vecPages.push_back(i);
				}
			}
			else if (m_ePageSelectionMode == kLastPages)
			{
				// Include the first [m_nNumLastPages] pages
				int nFirst = nLastPageNumber - m_nNumLastPages + 1;
				if (nFirst < 1)
				{
					nFirst = 1;
				}

				for(int i = nFirst; i <= nLastPageNumber; i++)
				{
					vecPages.push_back(i);
				}
			}
			else if (m_ePageSelectionMode == kSpecifiedPages)
			{
				// Include [m_strSpecifiedPages] pages
				vecPages = getPageNumbers(nLastPageNumber, m_strSpecifiedPages);
			}
		}

		// Process one page at a time
		for each (int nPageNum in vecPages)
		{
			IIUnknownVectorPtr ipSubLineRects = __nullptr;

			// Default to search for horizontal lines in case no page info is available.
			VARIANT_BOOL bHorizontal = VARIANT_TRUE;
			double deskew = 0.0;

			// [FlexIDSCore:3182]
			// Check to see if the specified page has any spatial text information.
			ISpatialStringPtr ipPage = ipAFDocText->GetSpecifiedPages(nPageNum, nPageNum);
			ASSERT_RESOURCE_ALLOCATION("ELI24099", ipPage != __nullptr);

			if (ipPage->String.length() > 0)
			{
				// Obtain page information (if available) and use it to get the page's orientation.
				ISpatialPageInfoPtr ipPageInfo = ipAFDocText->GetPageInfo(nPageNum);
				ASSERT_RESOURCE_ALLOCATION("ELI19440", ipPageInfo != __nullptr);

				// Determine which way to orient the search based on the page text orientation.
				EOrientation ePageOrientation = ipPageInfo->Orientation;

				// Determine whether to look for horizontal or vertical lines
				bHorizontal = asVariantBool(ePageOrientation == kRotNone ||
											ePageOrientation == kRotDown ||
											ePageOrientation == kRotFlipped ||
											ePageOrientation == kRotFlippedDown);

				deskew = ipPageInfo->Deskew;
			}
			
			// Find the image regions (and lines, if requested)
			IIUnknownVectorPtr ipGroupRects = getImageLineUtility()->FindLineRegions(
				ipAFDocText->SourceDocName, nPageNum, -deskew, bHorizontal, 
				m_bIncludeLines ? &ipSubLineRects : NULL);

			ASSERT_RESOURCE_ALLOCATION("ELI18412", ipGroupRects != __nullptr);

			if (m_bIncludeLines)
			{
				// If including lines, assert that ipSubLineRects was allocated
				ASSERT_RESOURCE_ALLOCATION("ELI19027", ipSubLineRects != __nullptr);

				// We should get a vector back for every group, plus an extra vector with all
				// lines in the document
				if (ipSubLineRects->Size() != (ipGroupRects->Size() + 1))
				{
					THROW_LOGIC_ERROR_EXCEPTION("ELI19035");
				}

				// The vector of all lines on the page is at the last position in the vector
				IIUnknownVectorPtr ipLineRects = ipSubLineRects->At(ipGroupRects->Size());
				ASSERT_RESOURCE_ALLOCATION("ELI19026", ipLineRects != __nullptr);

				// Create an attribute representing the lines on the page
				IAttributePtr ipLines = createAttributeFromRects(ipLineRects, "Lines", 
					asString(ipAFDocText->SourceDocName), ipAFDocText->SpatialPageInfos, nPageNum);

				ipAttributes->PushBack(ipLines);
			}

			// Loop to create an attribute for each image region found
			for (int i = 0; i < ipGroupRects->Size(); i++)
			{	
				ILongRectanglePtr ipRect = ipGroupRects->At(i);
				ASSERT_RESOURCE_ALLOCATION("ELI18721", ipRect != __nullptr);

				// Create an attribute to represent the image region
				IAttributePtr ipAttribute = createSpatialAttribute(ipRect, ipAFDocText, nPageNum);

				if (m_bIncludeLines)
				{
					// If lines are included, create a sub-attribute to represent the lines
					// that form the image region
					IIUnknownVectorPtr ipSubAttributes = ipAttribute->SubAttributes;
					ASSERT_RESOURCE_ALLOCATION("ELI18716", ipSubAttributes != __nullptr);

					IIUnknownVectorPtr ipLines = ipSubLineRects->At(i);
					ASSERT_RESOURCE_ALLOCATION("ELI18723", ipLines != __nullptr);

					IAttributePtr ipSubLines = createAttributeFromRects(ipLines, "...sub-lines", 
						asString(ipAFDocText->SourceDocName), ipAFDocText->SpatialPageInfos,
						nPageNum);
					ASSERT_RESOURCE_ALLOCATION("ELI18722", ipSubLines != __nullptr);

					ipSubAttributes->PushBack(ipSubLines);
				}

				ipAttributes->PushBack(ipAttribute);
			}
		}
		
		// return the vector
		*ppAttributes = ipAttributes.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18377");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageRegionWithLines::raw_IsConfigured(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Check parameter
		ASSERT_ARGUMENT("ELI18378", pbValue != __nullptr);

		// Ensure attribute text is specified.  All other settings will validated as they are set.
		if (m_strAttributeText.empty())
		{
			*pbValue = VARIANT_FALSE;
		}
		else
		{
			*pbValue = VARIANT_TRUE;
		}

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18379");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageRegionWithLines::raw_GetComponentDescription(BSTR *pbstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI18380", pbstrComponentDescription != __nullptr)

		*pbstrComponentDescription = _bstr_t("Find image region with lines").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18381")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageRegionWithLines::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license
		validateLicense();

		UCLID_AFVALUEFINDERSLib::IImageRegionWithLinesPtr ipCopyThis = pObject;
		ASSERT_ARGUMENT("ELI18382", ipCopyThis != __nullptr);

		// clone the ImageLineUtility member
		IImageLineUtilityPtr ipSourceLineUtil(ipCopyThis->LineUtil);
		if (ipSourceLineUtil)
		{
			ICopyableObjectPtr ipCopyObj(ipSourceLineUtil);
			ASSERT_RESOURCE_ALLOCATION("ELI18987", ipCopyObj != __nullptr);
			m_ipImageLineUtility = ipCopyObj->Clone();
		}

		// copy member variables
		m_ePageSelectionMode	= (EPageSelectionMode) ipCopyThis->PageSelectionMode;
		m_nNumFirstPages		= ipCopyThis->NumFirstPages;
		m_nNumLastPages			= ipCopyThis->NumLastPages;
		m_strSpecifiedPages		= asString(ipCopyThis->SpecifiedPages);
		m_strAttributeText		= asString(ipCopyThis->AttributeText);
		m_bIncludeLines			= asCppBool(ipCopyThis->IncludeLines);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18385");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageRegionWithLines::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license
		validateLicense();

		ASSERT_ARGUMENT("ELI18386", pObject != __nullptr);

		// Create another instance of this object
		ICopyableObjectPtr ipObjCopy(CLSID_ImageRegionWithLines);
		ASSERT_RESOURCE_ALLOCATION("ELI18387", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);
	
		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18388");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageRegionWithLines::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI18389", pClassID != __nullptr);

		*pClassID = CLSID_ImageRegionWithLines;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18390");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageRegionWithLines::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		return m_bDirty ? S_OK : S_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18391");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageRegionWithLines::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		ASSERT_ARGUMENT("ELI18392", pStream != __nullptr);

		//Clear the existing data
		m_ipImageLineUtility = __nullptr;
		m_ePageSelectionMode = kAllPages;
		m_nNumFirstPages = 0;
		m_nNumLastPages = 0;
		m_strSpecifiedPages = "";
		m_strAttributeText = "";
		m_bIncludeLines = false;
		
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
			UCLIDException ue("ELI18393", "Unable to load newer image region with lines rule!");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// load member variables
		long lTemp = (long) UCLID_AFVALUEFINDERSLib::kAllPages;
		dataReader >> lTemp;
		m_ePageSelectionMode = (EPageSelectionMode) lTemp;
		dataReader >> m_nNumFirstPages;
		dataReader >> m_nNumLastPages;
		dataReader >> m_strSpecifiedPages;
		dataReader >> m_strAttributeText;
		dataReader >> m_bIncludeLines;

		// read the LineUtil object from the stream
		IPersistStreamPtr ipLineUtil;
		readObjectFromStream(ipLineUtil, pStream, "ELI18955");
		m_ipImageLineUtility = ipLineUtil;

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18394");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageRegionWithLines::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		ASSERT_ARGUMENT("ELI18395", pStream != __nullptr);

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		// Write the current version
		dataWriter << gnCurrentVersion;

		// save member variables
		dataWriter << (long) m_ePageSelectionMode;
		dataWriter << m_nNumFirstPages;
		dataWriter << m_nNumLastPages;
		dataWriter << m_strSpecifiedPages;
		dataWriter << m_strAttributeText;
		dataWriter << m_bIncludeLines;

		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// Write the line util to the stream
		IPersistStreamPtr ipLineUtilStream(getImageLineUtility());
		ASSERT_RESOURCE_ALLOCATION("ELI18953", ipLineUtilStream != __nullptr);
		writeObjectToStream(ipLineUtilStream, pStream, "ELI18954", fClearDirty);

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18396");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageRegionWithLines::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageRegionWithLines::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI18397", pbValue != __nullptr);

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18398");

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CImageRegionWithLines::InterfaceSupportsErrorInfo(REFIID riid)
{
	try
	{
		static const IID* arr[] = 
		{
			&IID_IImageRegionWithLines,
			&IID_IAttributeFindingRule,
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18415")

	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
IImageLineUtilityPtr CImageRegionWithLines::getImageLineUtility()
{
	// Create image Utils object if not already created
	if (m_ipImageLineUtility == __nullptr )
	{
		m_ipImageLineUtility.CreateInstance(CLSID_ImageLineUtility);
		ASSERT_RESOURCE_ALLOCATION("ELI18409", m_ipImageLineUtility != __nullptr);
	}

	return m_ipImageLineUtility;
}
//-------------------------------------------------------------------------------------------------
IAttributePtr CImageRegionWithLines::createAttributeFromRects(IIUnknownVectorPtr ipRects, 
		const string &strText, const string &strSourceDocName, ILongToObjectMapPtr ipPageInfoMap, 
		int nPageNum)
{
	ASSERT_ARGUMENT("ELI18719", ipRects != __nullptr);
	ASSERT_ARGUMENT("ELI18720", !strText.empty());
	ASSERT_ARGUMENT("ELI19967", nPageNum > 0);

	// Create an IIUnknownVector to store the raster zones that will be used for the attribute
	IIUnknownVectorPtr ipAttributeZones(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI18698", ipAttributeZones != __nullptr);

	// For each ipRect
	for (int i = 0; i < ipRects->Size() ; i++)
	{	
		// Obtain a copy of the rect for the raster zone
		ILongRectanglePtr ipRect = ipRects->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI18696", ipRect != __nullptr);

		ILongRectanglePtr ipRectClone = ipRect->Clone();
		ASSERT_RESOURCE_ALLOCATION("ELI18709", ipRectClone != __nullptr);
		long lLeft, lTop, lRight, lBottom;
		ipRectClone->GetBounds(&lLeft, &lTop, &lRight, &lBottom);

		// Ensure that each rect has a height of at least gnMIN_ZONE_HEIGHT to be sure it is noticeable
		int nHeight = lBottom - lTop;
		if (nHeight < gnMIN_ZONE_HEIGHT)
		{
			int nPadding = gnMIN_ZONE_HEIGHT - nHeight;
			ipRectClone->Expand(0, nPadding);
			ipRectClone->Offset(0, - nPadding / 2);
		}

		// Ensure that each rect has a width of at least gnMIN_ZONE_WIDTH to be sure it is noticeable
		int nWidth = lRight - lLeft;
		if (nWidth < gnMIN_ZONE_WIDTH)
		{
			int nPadding = gnMIN_ZONE_WIDTH - nWidth;
			ipRectClone->Expand(nPadding, 0);
			ipRectClone->Offset(- nPadding / 2, 0);
		}

		// Create the raster zone
		IRasterZonePtr	ipZone(CLSID_RasterZone);
		ASSERT_RESOURCE_ALLOCATION("ELI18697", ipZone != __nullptr);

		ipZone->CreateFromLongRectangle(ipRectClone, nPageNum);

		// Store the raster zone
		ipAttributeZones->PushBack(ipZone);
	}

	// Build a SpatialString from the raster zone vector
	ISpatialStringPtr ipValue(CLSID_SpatialString);
	ASSERT_RESOURCE_ALLOCATION("ELI18700", ipValue != __nullptr);

	ipValue->CreateHybridString(ipAttributeZones, strText.c_str(), 
		strSourceDocName.c_str(), ipPageInfoMap);

	// Assign the spatial string to the attribute
	IAttributePtr ipAttribute(CLSID_Attribute);
	ASSERT_RESOURCE_ALLOCATION("ELI18699", ipAttribute != __nullptr);
	ipAttribute->Value = ipValue;

	return ipAttribute;
}
//-------------------------------------------------------------------------------------------------
IAttributePtr CImageRegionWithLines::createSpatialAttribute(ILongRectanglePtr ipRect, 
															ISpatialStringPtr ipDocText,
															int nPageNum)
{
	ASSERT_ARGUMENT("ELI19964", ipDocText != __nullptr);
	ASSERT_ARGUMENT("ELI19965", ipRect != __nullptr);
	ASSERT_ARGUMENT("ELI19966", nPageNum > 0);

	// Create the raster zone
	IRasterZonePtr	ipZone(CLSID_RasterZone);
	ASSERT_RESOURCE_ALLOCATION("ELI19971", ipZone != __nullptr);

	ipZone->CreateFromLongRectangle(ipRect, nPageNum);

	// Create the spatial string
	ISpatialStringPtr ipSpatialString(CLSID_SpatialString);
	ASSERT_RESOURCE_ALLOCATION("ELI19972", ipSpatialString);

	// We want to modify the existing page's PageInfo for the attribute, but we don't want
	// to affect the existing page, so obtain a copy.
	ICopyableObjectPtr ipCloneThis = ipDocText->GetPageInfo(nPageNum);
	ASSERT_RESOURCE_ALLOCATION("ELI25257", ipCloneThis != __nullptr);

	ISpatialPageInfoPtr ipPageInfoClone = ipCloneThis->Clone();
	ASSERT_RESOURCE_ALLOCATION("ELI25258", ipPageInfoClone != __nullptr);
	
	// [FlexIDSCore:3185]
	// The line coordinates we used to create the raster zone were based on the original orientation,
	// not rotated coordinates.  Therefore use no rotation to ensure the attribute appears in the 
	// correct location.  Do not touch the deskew value since we already accounted for skew by passing
	// the deskew value into the FindLines call.
	ipPageInfoClone->Orientation = kRotNone;

	// create a spatial page info map for the new spatial string
	ILongToObjectMapPtr ipPageInfoMap(CLSID_LongToObjectMap);
	ASSERT_RESOURCE_ALLOCATION("ELI25259", ipPageInfoMap != __nullptr);
	ipPageInfoMap->Set(nPageNum, ipPageInfoClone);

	// Build a spatial string (in spatial mode) that occupies the full extent of ipZone with
	// the letters of m_strAttributeText spread evenly across it.
	ipSpatialString->CreatePseudoSpatialString(ipZone, m_strAttributeText.c_str(), 
		ipDocText->SourceDocName, ipPageInfoMap);

	// Create the attribute
	IAttributePtr ipAttribute(CLSID_Attribute);
	ASSERT_RESOURCE_ALLOCATION("ELI19974", ipAttribute != __nullptr );

	ipAttribute->Value = ipSpatialString;

	return ipAttribute;
}
//-------------------------------------------------------------------------------------------------
void CImageRegionWithLines::validateLicense()
{
	VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI18414", "Image Region With Lines Rule");
}
//-------------------------------------------------------------------------------------------------