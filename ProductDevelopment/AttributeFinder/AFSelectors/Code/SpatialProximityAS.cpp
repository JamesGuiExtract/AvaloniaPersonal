// SpatialProximityAS.cpp : Implementation of CSpatialProximityAS

#include "stdafx.h"
#include "SpatialProximityAS.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>
#include <MiscLeadUtils.h>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion	= 2;
const CRect grectNULL					= CRect(0, 0, 0, 0);

//--------------------------------------------------------------------------------------------------
// CSpatialProximityAS
//--------------------------------------------------------------------------------------------------
CSpatialProximityAS::CSpatialProximityAS()
: m_bDirty(false)
, m_nXResolution(0)
, m_nYResolution(0)
, m_ipDocText(NULL)
{
	try
	{
		reset();

		m_ipAFUtility.CreateInstance(CLSID_AFUtility);
		ASSERT_RESOURCE_ALLOCATION("ELI22706", m_ipAFUtility != __nullptr);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI22560");
}
//--------------------------------------------------------------------------------------------------
CSpatialProximityAS::~CSpatialProximityAS()
{
	try
	{
		m_ipDocText = __nullptr;
		m_ipAFUtility = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI22561");
}
//-------------------------------------------------------------------------------------------------
HRESULT CSpatialProximityAS::FinalConstruct()
{
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
void CSpatialProximityAS::FinalRelease()
{
}

//--------------------------------------------------------------------------------------------------
// ISpatialProximityAS
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialProximityAS::get_TargetQuery(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22603", pVal != __nullptr);

		validateLicense();

		*pVal = get_bstr_t(m_strTargetQuery).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22604")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialProximityAS::put_TargetQuery(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_strTargetQuery = asString(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22606")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialProximityAS::get_ReferenceQuery(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22607", pVal != __nullptr);

		validateLicense();

		*pVal = get_bstr_t(m_strReferenceQuery).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22608")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialProximityAS::put_ReferenceQuery(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_strReferenceQuery = asString(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22609")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialProximityAS::get_RequireCompleteInclusion(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22610", pVal != __nullptr);

		validateLicense();

		*pVal = asVariantBool(m_bRequireCompleteInclusion);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22611")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialProximityAS::put_RequireCompleteInclusion(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bRequireCompleteInclusion = asCppBool(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22612")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialProximityAS::get_TargetsMustContainReferences(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI23493", pVal != __nullptr);

		validateLicense();

		*pVal = asVariantBool(m_bTargetsMustContainReferences);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI23494")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialProximityAS::put_TargetsMustContainReferences(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bTargetsMustContainReferences = asCppBool(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI23495")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialProximityAS::get_CompareLinesSeparately(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22613", pVal != __nullptr);

		validateLicense();

		*pVal = asVariantBool(m_bCompareLinesSeparately);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22614")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialProximityAS::put_CompareLinesSeparately(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bCompareLinesSeparately = asCppBool(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22615")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialProximityAS::SetRegionBorder(EBorder eRegionBorder, EBorderRelation eRelation, 
		EBorder eRelationBorder, EBorderExpandDirection eExpandDirection, double dExpandAmount, 
		EUnits eUnits)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_mapBorderInfo[eRegionBorder].m_eRelation			= eRelation;
		m_mapBorderInfo[eRegionBorder].m_eBorder			= eRelationBorder;
		m_mapBorderInfo[eRegionBorder].m_eExpandDirection	= eExpandDirection;
		m_mapBorderInfo[eRegionBorder].m_dExpandAmount		= dExpandAmount;
		m_mapBorderInfo[eRegionBorder].m_eUnits				= eUnits;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22619")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialProximityAS::GetRegionBorder(EBorder eRegionBorder, EBorderRelation *peRelation, 
		EBorder *peRelationBorder, EBorderExpandDirection *peExpandDirection, double *pdExpandAmount, 
		EUnits *peUnits)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI22620", eRegionBorder != kNoBorder);
		ASSERT_ARGUMENT("ELI22621", peRelation != __nullptr);
		ASSERT_ARGUMENT("ELI22622", peRelationBorder != __nullptr);
		ASSERT_ARGUMENT("ELI22644", peExpandDirection != __nullptr);
		ASSERT_ARGUMENT("ELI22645", pdExpandAmount != __nullptr);
		ASSERT_ARGUMENT("ELI22646", peUnits != __nullptr);

		*peRelation			= m_mapBorderInfo[eRegionBorder].m_eRelation;
		*peRelationBorder	= m_mapBorderInfo[eRegionBorder].m_eBorder;
		*peExpandDirection	= m_mapBorderInfo[eRegionBorder].m_eExpandDirection;
		*pdExpandAmount		= m_mapBorderInfo[eRegionBorder].m_dExpandAmount;
		*peUnits			= m_mapBorderInfo[eRegionBorder].m_eUnits;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22735")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialProximityAS::get_IncludeDebugAttributes(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22694", pVal != __nullptr);

		validateLicense();

		*pVal = asVariantBool(m_bIncludeDebugAttributes);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22695")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialProximityAS::put_IncludeDebugAttributes(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bIncludeDebugAttributes = asCppBool(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22696")
}

//--------------------------------------------------------------------------------------------------
// IAttributeSelector
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialProximityAS::raw_SelectAttributes(IIUnknownVector *pAttrIn, IAFDocument *pAFDoc, 
													   IIUnknownVector **pAttrOut)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		// validate license
		validateLicense();

		IIUnknownVectorPtr ipAttributes(pAttrIn);
		ASSERT_ARGUMENT("ELI22562", ipAttributes != __nullptr);
		IAFDocumentPtr ipAFDoc(pAFDoc);
		ASSERT_ARGUMENT("ELI22563", ipAFDoc != __nullptr);
		ASSERT_ARGUMENT("ELI22564", pAttrOut != __nullptr);

		// Reset the resolution.  These will be recalculated when needed.
		m_nXResolution = 0;
		m_nYResolution = 0;

		// Store the document text so it can be used during processing.
		m_ipDocText = ipAFDoc->Text;
		ASSERT_RESOURCE_ALLOCATION("ELI22685", m_ipDocText != __nullptr);

		// Obtain the set of attributes available for selection
		IIUnknownVectorPtr ipTargetAttributes = 
			m_ipAFUtility->QueryAttributes(ipAttributes, m_strTargetQuery.c_str(), VARIANT_FALSE);
		ASSERT_RESOURCE_ALLOCATION("ELI22707", ipTargetAttributes != __nullptr);

		// Obtain the set of attributes to be used to describe the location selected attributes
		// must occupy.
		IIUnknownVectorPtr ipReferenceAttributes = 
			m_ipAFUtility->QueryAttributes(ipAttributes, m_strReferenceQuery.c_str(), VARIANT_FALSE);
		ASSERT_RESOURCE_ALLOCATION("ELI22708", ipReferenceAttributes  != __nullptr);

		// Create the collection of selected attributes that will be returned as the final result.
		IIUnknownVectorPtr ipSelectedAttributes(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI22667", ipSelectedAttributes != __nullptr);

		// Create a vector to store each pair of attributes where one is contained within the other.
		vector< pair<IAttributePtr, IAttributePtr> > vecContainmentPairs;

		if (m_bIncludeDebugAttributes)
		{
			// If debug mode is selected, just add the debug sub-attributes (don't test the regions).
			createDebugAttributes(ipReferenceAttributes);
		}
		else if (m_bTargetsMustContainReferences)
		{
			// If we are looking for attributes that completely contain a reference region.
			vecContainmentPairs = findContainmentPairs(ipTargetAttributes, ipReferenceAttributes);
		}
		else
		{
			// If we are looking for attributes either completely or partially contained in the 
			// reference regions.
			vecContainmentPairs = findContainmentPairs(ipReferenceAttributes, ipTargetAttributes);
		}

		for each (pair<IAttributePtr, IAttributePtr> containmentPair in vecContainmentPairs)
		{
			// For each pairing, select the appropriate attribute based upon the
			// m_bTargetsMustContainReferences setting.
			ipSelectedAttributes->PushBackIfNotContained(m_bTargetsMustContainReferences 
														 ? containmentPair.first
														 : containmentPair.second);
		}

		// Copy the results to pAttrOut.
		*pAttrOut = ipSelectedAttributes.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22565");
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialProximityAS::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI22566", pClassID != __nullptr);

		*pClassID = CLSID_SpatialProximityAS;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22567");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialProximityAS::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		return m_bDirty ? S_OK : S_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22568");
}
//-------------------------------------------------------------------------------------------------
// Version 2: Added m_bTargetsMustContainReferences
STDMETHODIMP CSpatialProximityAS::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		ASSERT_ARGUMENT("ELI22569", pStream != __nullptr);

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
			UCLIDException ue("ELI22570", 
				"Unable to load newer spatial proximity attribute selector rule!");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// Read data members from the stream
		dataReader >> m_strTargetQuery;
		dataReader >> m_strReferenceQuery;
		dataReader >> m_bRequireCompleteInclusion;
		if (nDataVersion > 1)
		{
			dataReader >> m_bTargetsMustContainReferences;
		}
		dataReader >> m_bCompareLinesSeparately;
		dataReader >> m_bIncludeDebugAttributes;

		for (map<EBorder, BorderInfo>::iterator iter = m_mapBorderInfo.begin(); 
			 iter != m_mapBorderInfo.end(); 
			 iter++)
		{
			long nTemp;
			dataReader >> nTemp;
			m_mapBorderInfo[iter->first].m_eBorder = (EBorder) nTemp;
			dataReader >> nTemp;
			m_mapBorderInfo[iter->first].m_eRelation = (EBorderRelation) nTemp;
			dataReader >> nTemp;
			m_mapBorderInfo[iter->first].m_eExpandDirection = (EBorderExpandDirection) nTemp;
			dataReader >> m_mapBorderInfo[iter->first].m_dExpandAmount;
			dataReader >> nTemp;
			m_mapBorderInfo[iter->first].m_eUnits = (EUnits) nTemp;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22571");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialProximityAS::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		ASSERT_ARGUMENT("ELI22572", pStream != __nullptr);

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		// Write the current version
		dataWriter << gnCurrentVersion;

		// Write the data members to the stream
		dataWriter << m_strTargetQuery;
		dataWriter << m_strReferenceQuery;
		dataWriter << m_bRequireCompleteInclusion;
		dataWriter << m_bTargetsMustContainReferences;
		dataWriter << m_bCompareLinesSeparately;
		dataWriter << m_bIncludeDebugAttributes;

		for (map<EBorder, BorderInfo>::iterator iter = m_mapBorderInfo.begin(); 
			 iter != m_mapBorderInfo.end(); 
			 iter++)
		{
			dataWriter << (long) m_mapBorderInfo[iter->first].m_eBorder;
			dataWriter << (long) m_mapBorderInfo[iter->first].m_eRelation;
			dataWriter << (long) m_mapBorderInfo[iter->first].m_eExpandDirection;
			dataWriter << m_mapBorderInfo[iter->first].m_dExpandAmount;
			dataWriter << (long) m_mapBorderInfo[iter->first].m_eUnits;
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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22573");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialProximityAS::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialProximityAS::raw_GetComponentDescription(BSTR *pbstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI22577", pbstrComponentDescription != __nullptr)

		*pbstrComponentDescription = _bstr_t("Spatial proximity attribute selector").Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22578")
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialProximityAS::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI22574", pbValue != __nullptr);

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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22575");
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialProximityAS::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license
		validateLicense();

		UCLID_AFSELECTORSLib::ISpatialProximityASPtr ipCopyThis = pObject;
		ASSERT_ARGUMENT("ELI22579", ipCopyThis != __nullptr);

		// Copy members
		m_strTargetQuery				= asString(ipCopyThis->TargetQuery);
		m_strReferenceQuery				= asString(ipCopyThis->ReferenceQuery);
		m_bRequireCompleteInclusion		= asCppBool(ipCopyThis->RequireCompleteInclusion);
		m_bTargetsMustContainReferences = asCppBool(ipCopyThis->TargetsMustContainReferences);
		m_bCompareLinesSeparately		= asCppBool(ipCopyThis->CompareLinesSeparately);
		m_bIncludeDebugAttributes		= asCppBool(ipCopyThis->IncludeDebugAttributes);

		for (map<EBorder, BorderInfo>::iterator iter = m_mapBorderInfo.begin(); 
			 iter != m_mapBorderInfo.end(); 
			 iter++)
		{
			ipCopyThis->GetRegionBorder(
				(UCLID_AFSELECTORSLib::EBorder) iter->first, 
				(UCLID_AFSELECTORSLib::EBorderRelation *) &(m_mapBorderInfo[iter->first].m_eRelation),
				(UCLID_AFSELECTORSLib::EBorder *) &(m_mapBorderInfo[iter->first].m_eBorder),
				(UCLID_AFSELECTORSLib::EBorderExpandDirection *) &(m_mapBorderInfo[iter->first].m_eExpandDirection),
				&(m_mapBorderInfo[iter->first].m_dExpandAmount),
				(UCLID_AFSELECTORSLib::EUnits *) &(m_mapBorderInfo[iter->first].m_eUnits));
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22580");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialProximityAS::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license
		validateLicense();

		ASSERT_ARGUMENT("ELI22581", pObject != __nullptr);

		// Create another instance of this object
		ICopyableObjectPtr ipObjCopy(CLSID_SpatialProximityAS);
		ASSERT_RESOURCE_ALLOCATION("ELI22582", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);
	
		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22583");
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialProximityAS::raw_IsConfigured(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Check parameter
		ASSERT_ARGUMENT("ELI22584", pbValue != __nullptr);

		*pbValue = VARIANT_TRUE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22585");
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialProximityAS::InterfaceSupportsErrorInfo(REFIID riid)
{
	try
	{
		static const IID* arr[] = 
		{
			&IID_ISpatialProximityAS,
			&IID_IAttributeSelector,
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22576")
}

//--------------------------------------------------------------------------------------------------
// Private Methods
//--------------------------------------------------------------------------------------------------
vector< pair<CRect, long> > CSpatialProximityAS::getAttributeRects(IAttributePtr ipAttribute,
																   bool bSeparateLines)
{
	ASSERT_ARGUMENT("ELI22668", ipAttribute != __nullptr);

	// Retrive the attribute's spatial string
	ISpatialStringPtr ipValue = ipAttribute->Value;
	ASSERT_RESOURCE_ALLOCATION("ELI22669", ipValue != __nullptr);

	// Get the collection of lines comprising the spatial string.
	IIUnknownVectorPtr ipLines = ipValue->GetLines();
	ASSERT_RESOURCE_ALLOCATION("ELI22670", ipLines != __nullptr);

	// Cycle through each line to obtain the rect & pagenum pairs needed to describe the attribute.
	vector< pair<CRect, long> > vecAttributeRects;
	CRect rectCompleteBounds = grectNULL;
	long nLastPage = -1;
	long nLineCount = ipLines->Size();
	for (long i = 0; i < nLineCount; i++)
	{
		ISpatialStringPtr ipLine = ipLines->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI22671", ipLine != __nullptr);

		// If the line does not have any spatial information, skip it.
		if (!asCppBool(ipLine->HasSpatialInfo()))
		{
			continue;
		}

		// Obtain the page from this line.
		long nPage = ipLine->GetFirstPageNumber();

		// Obtain a rect describing the location of this line.
		ILongRectanglePtr ipRect = ipLine->GetOCRImageBounds();
		ASSERT_RESOURCE_ALLOCATION("ELI22674", ipRect != __nullptr);

		// Copy the rect to a CRect to make rect comparisons easier.
		CRect rectBounds;
		ipRect->GetBounds(&(rectBounds.left), &(rectBounds.top),
			&(rectBounds.right), &(rectBounds.bottom));

		if (bSeparateLines)
		{
			// If using lines separately, simply add each line's rect separately
			vecAttributeRects.push_back(pair<CRect, long>(rectBounds, nPage));
		}
		else if (nPage != nLastPage)
		{
			// If using the total attribute bounds, start a new result only
			// if starting a new page.
			
			if (rectCompleteBounds != grectNULL)
			{
				// If we already were compiling a result for a previous page, add it to the results.
				vecAttributeRects.push_back(
					pair<CRect, long>(rectCompleteBounds, nLastPage));
				rectCompleteBounds = grectNULL;
			}
			
			// Start the new result.
			rectCompleteBounds = rectBounds;
			nLastPage = nPage;
		}
		else
		{
			// If using the total attribute bounds and we are still on the same page as the current
			// result, incorporate the bounds of this line into the current result.
			rectCompleteBounds.UnionRect(&rectBounds, &rectCompleteBounds);
		}
	}

	if (!bSeparateLines && rectCompleteBounds != grectNULL)
	{
		// If using the total attribute bounds, be sure to return any result that has not yet
		// been added to vecAttributeRects.
		vecAttributeRects.push_back(
			pair<CRect, long>(rectCompleteBounds, nLastPage));
	}

	return vecAttributeRects;
}
//--------------------------------------------------------------------------------------------------
bool CSpatialProximityAS::convertToTargetRegion(CRect &rrect, long nPage)
{
	// Test to see if there is text on the specified page.  If there isn't any text on this page, 
	// fail the conversion (it is possible, but too complicated for the benefit at this point.)
	ISpatialStringPtr ipPageText = m_ipDocText->GetSpecifiedPages(nPage, nPage);
	if (ipPageText == __nullptr || !asCppBool(ipPageText->HasSpatialInfo()))
	{
		return false;
	}

	ISpatialPageInfoPtr ipPageInfo = m_ipDocText->GetPageInfo(nPage);
	ASSERT_RESOURCE_ALLOCATION("ELI22684", ipPageInfo != __nullptr);

	// Create a rect to repersent the coordinates of the page as a whole
	CRect rectPage(0, 0, ipPageInfo->Width, ipPageInfo->Height);

	// Create a copy of the rect to modify so that as the borders are altered, we still know the
	// coordinates of the original.
	CRect rectOrig(rrect);

	for (map<EBorder, BorderInfo>::iterator iter = m_mapBorderInfo.begin(); 
		 iter != m_mapBorderInfo.end(); 
		 iter++)
	{
		// Obtain a reference to the border to be adjusted
		long &nBorder = getRectBorder(rrect, iter->first);

		// Re-position the border to match the reference border position (may be the same)
		// Use this result as value to set the reference of rrect's border.
		nBorder = getRectBorder((iter->second.m_eRelation == kPage) ? rectPage : rectOrig, 
								iter->second.m_eBorder);

		// Now offset the border by the amount specified.
		nBorder += getExpansionOffset(iter->second, nPage);
	}

	// Return false if we are not able to find an intersecting rect for the reference rect and
	// the page as a whole.
	return (rrect.IntersectRect(rrect, rectPage) == TRUE);
}
//--------------------------------------------------------------------------------------------------
long &CSpatialProximityAS::getRectBorder(CRect &rect, EBorder eBorder)
{
	// Return a reference to the specified border.
	switch (eBorder)
	{
		case kLeft:		return rect.left;
		case kTop:		return rect.top;
		case kRight:	return rect.right;
		case kBottom:	return rect.bottom;
		
		default: THROW_LOGIC_ERROR_EXCEPTION("ELI22677");
	}
}
//--------------------------------------------------------------------------------------------------
long CSpatialProximityAS::getExpansionOffset(const BorderInfo &borderInfo, long nPage)
{
	// Start with the expansion amount specified
	double dExpansionOffset = borderInfo.m_dExpandAmount;

	// Change the sign to reflect the direction of expansion.
	if (borderInfo.m_eExpandDirection == kExpandLeft || borderInfo.m_eExpandDirection == kExpandUp)
	{
		dExpansionOffset = -dExpansionOffset;
	}

	// Convert the expansion amount into pixels from the units specified.
	if (borderInfo.m_eUnits == kInches)
	{
		// If using inches, obtain the image's resolution so it can be used for conversion.
		if (m_nXResolution == 0 || m_nYResolution == 0)
		{
			getImageXAndYResolution(asString(m_ipDocText->SourceDocName), 
				m_nXResolution, m_nYResolution);
			if (m_nXResolution == 0 || m_nYResolution == 0)
			{
				throw UCLIDException("ELI22686", "Failed to obtain image resolution!");
			}
		}

		// Convert from inches to pixels
		if (borderInfo.m_eExpandDirection == kExpandLeft || 
			borderInfo.m_eExpandDirection == kExpandRight)
		{
			dExpansionOffset *= m_nXResolution;
		}
		else
		{
			dExpansionOffset *= m_nYResolution;
		}
	}
	else if (borderInfo.m_eUnits == kCharacters)
	{
		// Convert from the page's average character width to pixels
		ISpatialStringPtr ipPageText = m_ipDocText->GetSpecifiedPages(nPage, nPage);
		ASSERT_RESOURCE_ALLOCATION("ELI22687", ipPageText != __nullptr);

		dExpansionOffset *= ipPageText->GetAverageCharWidth();
	}
	else if (borderInfo.m_eUnits == kLines)
	{
		// Convert from the page's average line height to pixels
		ISpatialStringPtr ipPageText = m_ipDocText->GetSpecifiedPages(nPage, nPage);
		ASSERT_RESOURCE_ALLOCATION("ELI22688", ipPageText != __nullptr);

		dExpansionOffset *= ipPageText->GetAverageLineHeight();
	}
	else
	{
		THROW_LOGIC_ERROR_EXCEPTION("ELI22719");
	}

	return (long) dExpansionOffset;
}
//--------------------------------------------------------------------------------------------------
vector< pair<IAttributePtr, IAttributePtr> > CSpatialProximityAS::findContainmentPairs(
	IIUnknownVectorPtr ipContainerAttributes, IIUnknownVectorPtr ipContainedAttributes)
{
	ASSERT_ARGUMENT("ELI23498", ipContainerAttributes != __nullptr);
	ASSERT_ARGUMENT("ELI23499", ipContainedAttributes != __nullptr);

	vector< pair<IAttributePtr, IAttributePtr> > vecContainmentPairs;

	// Cycle through each container attribute to find attributes contained within
	// the container attribute.
	long nContainerAttributesCount = ipContainerAttributes->Size();
	for (long i = 0; i < nContainerAttributesCount; i++)
	{
		IAttributePtr ipContainerAttribute = ipContainerAttributes->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI22675", ipContainerAttribute != __nullptr);
		
		// Get a rect and pagenum for each area needed to describe the container attribute's
		// location.
		vector< pair<CRect, long> > vecContainerRects =
			getAttributeRects(ipContainerAttribute, m_bCompareLinesSeparately);

		// Convert each of these locations to the locations to search.
		for each (pair<CRect, long> containerRect in vecContainerRects)
		{
			// If m_bTargetsMustContainReferences is false, the container rect is
			// a reference rect and so it must be converted so that it reflects
			// the specified bounds which are not necessarily the bounds of the 
			// attribute itself.
			if (m_bTargetsMustContainReferences ||
				convertToTargetRegion(containerRect.first, containerRect.second))
			{
				// A valid area to search was found.
				long nAttributeCount = ipContainedAttributes->Size();
				for (int i = 0; i < nAttributeCount; i++)
				{
					IAttributePtr ipContainedAttribute = ipContainedAttributes->At(i);
					ASSERT_RESOURCE_ALLOCATION("ELI22704", ipContainedAttribute != __nullptr);

					// Don't allow an attribute to be selected based on itself.
					if (ipContainedAttribute == ipContainerAttribute)
					{
						continue;
					}
					
					if (isAttributeContainedIn(ipContainedAttribute, 
											   containerRect.first, containerRect.second))
					{
						// If ipContainedAttribute is indeed contained with containerRect,
						// add that attribute pair to the return result.
						vecContainmentPairs.push_back(pair<IAttributePtr, IAttributePtr>(
							ipContainerAttribute, ipContainedAttribute));
					}
				}
			}
		}
	}

	return vecContainmentPairs;
}
//--------------------------------------------------------------------------------------------------
bool CSpatialProximityAS::isAttributeContainedIn(IAttributePtr ipContainedAttribute,
												 const CRect &rectContainerArea, long nPage)
{
	ASSERT_ARGUMENT("ELI22702", ipContainedAttribute != __nullptr);

	// Obtain a rect describing the area of each of the attribute's lines.
	vector< pair<CRect, long> > vecContainedAttributeRects = 
		getAttributeRects(ipContainedAttribute, m_bCompareLinesSeparately);

	// Check each line for inclusion.
	int nLineInclusionCount = 0;
	for each (pair<CRect, long> rect in vecContainedAttributeRects)
	{
		// If m_bTargetsMustContainReferences is true, ipContainedAttribute is
		// a reference rect and so it must be converted so that it reflects
		// the specified bounds which are not necessarily the bounds of the 
		// attribute itself.
		if (!m_bTargetsMustContainReferences || convertToTargetRegion(rect.first, rect.second))
		{
			if (nPage != rect.second)
			{
				// This line is not on the same page as the specified page.

				if (m_bRequireCompleteInclusion)
				{
					// If complete inclusion is required, this attribute does not qualify. Move on
					// to the next.
					break;
				}
				else
				{
					// If only partial inclusion is required, move on to the next line.
					continue;
				}
			}

			CRect rectIntersection;
			if (rectIntersection.IntersectRect(rect.first, rectContainerArea) == TRUE)
			{
				// There is overlap between this line of the target attribute and the 
				// selection area.

				if (m_bRequireCompleteInclusion)
				{
					if (rectIntersection == rect.first)
					{
						// This line is completely included in the container area. Keep count of all
						// the lines that qualify.
						nLineInclusionCount ++;
					}
					else
					{
						// This line is not completely included, this attribute does not qualify. 
						// Move on to the next attribute.
						break;
					}
				}

				// If only partial inclusion is required or all the lines of the attribute are 
				// completely included, add the attribute to the ipSelectedAttributes collection.
				if (!m_bRequireCompleteInclusion || 
					nLineInclusionCount == vecContainedAttributeRects.size())
				{
					return true;
				}
			}
		}
	}

	return false;
}
//--------------------------------------------------------------------------------------------------
void CSpatialProximityAS::createDebugAttributes(IIUnknownVectorPtr ipReferenceAttributes)
{
	ASSERT_ARGUMENT("ELI23500", ipReferenceAttributes != __nullptr);

	long nReferenceAttributesCount = ipReferenceAttributes->Size();
	for (long i = 0; i < nReferenceAttributesCount; i++)
	{
		IAttributePtr ipReferenceAttribute = ipReferenceAttributes->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI23501", ipReferenceAttribute != __nullptr);
		
		// Get a rect and pagenum for each area needed to describe the reference attribute's
		// location.
		vector< pair<CRect, long> > vecReferenceRects =
			getAttributeRects(ipReferenceAttribute, m_bCompareLinesSeparately);

		// Convert each of these locations to the locations to search.
		for each (pair<CRect, long> referenceRect in vecReferenceRects)
		{
			if (convertToTargetRegion(referenceRect.first, referenceRect.second))
			{
				// A valid reference area was found
				createDebugAttributes(ipReferenceAttribute, referenceRect.first, referenceRect.second);
			}
		}
	}
}
//--------------------------------------------------------------------------------------------------
void CSpatialProximityAS::createDebugAttributes(IAttributePtr ipAttribute, 
												const CRect &rectReference, long nPage)
{
	ASSERT_ARGUMENT("ELI22700", ipAttribute != __nullptr);

	// Create a new raster zone based on the specified image area.
	IRasterZonePtr ipRasterZone(CLSID_RasterZone);
	ASSERT_RESOURCE_ALLOCATION("ELI22690", ipRasterZone != __nullptr);

	ILongRectanglePtr ipZoneRect(CLSID_LongRectangle);
	ASSERT_RESOURCE_ALLOCATION("ELI22691", ipZoneRect != __nullptr);
	ipZoneRect->SetBounds(rectReference.left, rectReference.top,
		rectReference.right, rectReference.bottom);

	ipRasterZone->CreateFromLongRectangle(ipZoneRect, nPage);

	// Build a spatial string using this raster zone.
	ISpatialStringPtr ipDebugString(CLSID_SpatialString);
	ASSERT_RESOURCE_ALLOCATION("ELI22689", ipDebugString != __nullptr);

	ipDebugString->CreatePseudoSpatialString(ipRasterZone, "Spatial Proximity Selector", 
									   m_ipDocText->SourceDocName, 
									   m_ipDocText->SpatialPageInfos);

	// Use this spatial string as the value for a debug attribute. 
	IAttributePtr ipDebugAttribute(CLSID_Attribute);
	ASSERT_RESOURCE_ALLOCATION("ELI22692", ipDebugAttribute != __nullptr);

	ipDebugAttribute->Value = ipDebugString;
	ipDebugAttribute->Name  = "Debug";

	// Add the debug attribute as a sub-attribute of the reference attribute it is associated with.
	IIUnknownVectorPtr ipSubAttributes = ipAttribute->SubAttributes;
	ASSERT_RESOURCE_ALLOCATION("ELI22693", ipSubAttributes != __nullptr);

	ipSubAttributes->PushBack(ipDebugAttribute);
}
//--------------------------------------------------------------------------------------------------
void CSpatialProximityAS::reset()
{
	// Reset all the rules settings.
	m_strTargetQuery.clear();
	m_strReferenceQuery.clear();
	m_bRequireCompleteInclusion = false;
	m_bTargetsMustContainReferences = false;
	m_bCompareLinesSeparately = true;
	m_bIncludeDebugAttributes = false;
	m_mapBorderInfo.clear();
	m_mapBorderInfo[kLeft] = BorderInfo(kLeft, kReferenceAttibute, kExpandLeft, 0, kInches);
	m_mapBorderInfo[kTop] = BorderInfo(kTop, kReferenceAttibute, kExpandUp, 0, kInches);
	m_mapBorderInfo[kRight] = BorderInfo(kRight, kReferenceAttibute, kExpandRight, 0, kInches);
	m_mapBorderInfo[kBottom] = BorderInfo(kBottom, kReferenceAttibute, kExpandDown, 0, kInches);
}
//--------------------------------------------------------------------------------------------------
void CSpatialProximityAS::validateLicense()
{
	VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI22586", 
		"Spatial proximity attribute selector");
}
//-------------------------------------------------------------------------------------------------