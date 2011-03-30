// LocateImageRegion.cpp : Implementation of CLocateImageRegion
#include "stdafx.h"
#include "AFValueFinders.h"
#include "LocateImageRegion.h"
#include "Common.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>
#include <COMUtils.h>
#include <cppletter.h>
#include <AFTagManager.h>
#include <ComponentLicenseIDs.h>
#include <AFCppUtils.h>

#include <cmath>

// current version
const unsigned long gnCurrentVersion = 5;

//-------------------------------------------------------------------------------------------------
// CLocateImageRegion
//-------------------------------------------------------------------------------------------------
CLocateImageRegion::CLocateImageRegion()
: m_bDataInsideBoundaries(true),
  m_bIncludeIntersecting(true),
  m_bMatchMultiplePagesPerDocument(true),
  m_eIntersectingEntity(kCharacter),
  m_ipSpatialStringSearcher(CLSID_SpatialStringSearcher),
  m_eFindType(kText),
  m_strImageRegionText("")
{
	try
	{
		m_mapIndexToClueListInfo.clear();
		// set default boundary info map
		BoundaryInfo boundInfo;

		for (int n = 1; n <= 4; n++)
		{
			EBoundary eRegionBoundary = (EBoundary)n;
			boundInfo.m_eSide = eRegionBoundary;
			// default to the page edge
			boundInfo.m_eCondition = kPage;
			m_mapBoundaryToInfo[eRegionBoundary] = boundInfo;
		}

		m_mapBoundaryToInfo[kTop].m_eExpandDirection = kExpandUp;
		m_mapBoundaryToInfo[kBottom].m_eExpandDirection = kExpandDown;
		m_mapBoundaryToInfo[kLeft].m_eExpandDirection = kExpandLeft;
		m_mapBoundaryToInfo[kRight].m_eExpandDirection = kExpandRight;

		ASSERT_RESOURCE_ALLOCATION("ELI07919", m_ipSpatialStringSearcher != __nullptr);

		m_ipMisc.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI22434", m_ipMisc != __nullptr);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI07795")
}
//-------------------------------------------------------------------------------------------------
CLocateImageRegion::~CLocateImageRegion()
{
	 try
	 {
		 m_ipSpatialStringSearcher = __nullptr;
		 m_ipMisc = __nullptr;
	 }
	 CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16346");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLocateImageRegion::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ILocateImageRegion,
		&IID_IAttributeFindingRule,
		&IID_IAttributeModifyingRule,
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
// ILocateImageRegion
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLocateImageRegion::get_FindType(EFindType* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = m_eFindType;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13169")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLocateImageRegion::put_FindType(EFindType newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_eFindType = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13170")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLocateImageRegion::get_ImageRegionText(BSTR* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = _bstr_t(m_strImageRegionText.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13171")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLocateImageRegion::put_ImageRegionText(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_strImageRegionText = asString(newVal);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13172")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLocateImageRegion::get_DataInsideBoundaries(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = asVariantBool(m_bDataInsideBoundaries);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07779")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLocateImageRegion::put_DataInsideBoundaries(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bDataInsideBoundaries = asCppBool(newVal);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07780")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLocateImageRegion::get_IncludeIntersectingEntities(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = asVariantBool(m_bIncludeIntersecting);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07781")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLocateImageRegion::put_IncludeIntersectingEntities(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bIncludeIntersecting = asCppBool(newVal);
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07782")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLocateImageRegion::get_IntersectingEntityType(ESpatialEntity *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_eIntersectingEntity;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07783")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLocateImageRegion::put_IntersectingEntityType(ESpatialEntity newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_eIntersectingEntity = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07784")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLocateImageRegion::get_MatchMultiplePagesPerDocument(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = asVariantBool(m_bMatchMultiplePagesPerDocument);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16783");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLocateImageRegion::put_MatchMultiplePagesPerDocument(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// retain the new value
		m_bMatchMultiplePagesPerDocument = asCppBool(newVal);

		// set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16784");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLocateImageRegion::SetRegionBoundary(EBoundary eRegionBoundary, 
												   EBoundary eSide, 
												   EBoundaryCondition eCondition, 
												   EExpandDirection eExpandDirection, 
												   double dExpandNumber)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		BoundaryInfo boundInfo;
		boundInfo.m_eSide = eSide;
		boundInfo.m_eCondition = eCondition;
		boundInfo.m_eExpandDirection = eExpandDirection;
		boundInfo.m_dExpandNumber = dExpandNumber;

		m_mapBoundaryToInfo[eRegionBoundary] = boundInfo;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07785")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLocateImageRegion::GetRegionBoundary(EBoundary eRegionBoundary, 
												   EBoundary *peSide, 
												   EBoundaryCondition *peCondition, 
												   EExpandDirection *peExpandDirection, 
												   double *pdExpandNumber)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// look for the entry in the map
		BoundaryToInfo::iterator itMap = m_mapBoundaryToInfo.find(eRegionBoundary);
		if (itMap == m_mapBoundaryToInfo.end())
		{
			UCLIDException ue("ELI07797", "Unable to get region boundary information.");
			ue.addDebugInfo("EBoundary", eRegionBoundary);
			addCurrentRSDFileToDebugInfo(ue);
			throw ue;
		}

		BoundaryInfo boundInfo = itMap->second;
		*peSide = boundInfo.m_eSide;
		*peCondition = boundInfo.m_eCondition;
		*peExpandDirection = boundInfo.m_eExpandDirection;
		*pdExpandNumber = boundInfo.m_dExpandNumber;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07786")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLocateImageRegion::SetClueList(EClueListIndex eIndex, 
											 IVariantVector *pvecClues,
											 VARIANT_BOOL bCaseSensitive,
											 VARIANT_BOOL bAsRegExpr, 
											 VARIANT_BOOL bRestrictByBoundary)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI07800", pvecClues != __nullptr);

		ClueListInfo listInfo;
		listInfo.m_ipClues = pvecClues;
		listInfo.m_bCaseSensitive = asCppBool(bCaseSensitive);
		listInfo.m_bAsRegExpr = asCppBool(bAsRegExpr);
		listInfo.m_bRestrictByBoundary = asCppBool(bRestrictByBoundary);
		m_mapIndexToClueListInfo[eIndex] = listInfo;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07787")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLocateImageRegion::GetClueList(EClueListIndex eIndex, 
											 IVariantVector **ppvecClues,
											 VARIANT_BOOL *pbCaseSensitive,
											 VARIANT_BOOL *pbAsRegExpr, 
											 VARIANT_BOOL *pbRestrictByBoundary)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*ppvecClues = NULL;

		IndexToClueListInfo::iterator itMap = m_mapIndexToClueListInfo.find(eIndex);
		if (itMap == m_mapIndexToClueListInfo.end())
		{
			return S_OK;
		}

		ClueListInfo listInfo = itMap->second;
		IVariantVectorPtr ipShallowCopy = listInfo.m_ipClues;
		*ppvecClues = ipShallowCopy.Detach();
		*pbCaseSensitive = asVariantBool(listInfo.m_bCaseSensitive);
		*pbAsRegExpr = asVariantBool(listInfo.m_bAsRegExpr);
		*pbRestrictByBoundary = asVariantBool(listInfo.m_bRestrictByBoundary);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07788")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLocateImageRegion::ClearAllClueLists()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_mapIndexToClueListInfo.clear();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08492")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IDocumentPreprocessor
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLocateImageRegion::raw_Process(IAFDocument* pDocument, IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		IAFDocumentPtr ipDoc(pDocument);
		ASSERT_ARGUMENT("ELI07945", ipDoc != __nullptr);

		// input string
		ISpatialStringPtr ipInputText = ipDoc->Text;
		ASSERT_RESOURCE_ALLOCATION("ELI16798", ipInputText != __nullptr);
		
		// get the image regions and combine them into a single spatial string
		ipDoc->Text = combineRegions( findRegionContent(ipInputText, pDocument), 
			ipInputText->SourceDocName);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07739");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLocateImageRegion::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_LocateImageRegion;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLocateImageRegion::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLocateImageRegion::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// clear member variables
		m_bDataInsideBoundaries = true;
		m_bIncludeIntersecting = true;
		m_eIntersectingEntity = kNoEntity;
		m_mapIndexToClueListInfo.clear();
		m_mapBoundaryToInfo.clear();

		// set to behavior prior to data version 4
		m_bMatchMultiplePagesPerDocument = false;

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
			UCLIDException ue("ELI07750", "Unable to load newer versioned LocateImageRegion." );
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		if(nDataVersion >= 3)
		{
			long nTemp;
			dataReader >> nTemp;
			m_eFindType = (EFindType)nTemp;
			dataReader >> m_strImageRegionText;
		}

		// inside/outside the region
		dataReader >> m_bDataInsideBoundaries;
		// include/exclude intersecting entities
		dataReader >> m_bIncludeIntersecting;
		// intersecting entity type
		long nEntity = 0;
		dataReader >> nEntity;
		m_eIntersectingEntity = (ESpatialEntity)nEntity;

		if(nDataVersion >= 4)
		{
			// get whether to match multiple pages per document
			dataReader >> m_bMatchMultiplePagesPerDocument;
		}

		//////////////////
		// Get boundary definitions
		long nNumOfBoundaries = 0;
		dataReader >> nNumOfBoundaries;
		long n;
		for (n = 0; n < nNumOfBoundaries; n++)
		{
			long nTemp = 0;
			// read the region boundary
			dataReader >> nTemp;
			EBoundary eRegionBoundary = (EBoundary)nTemp;

			BoundaryInfo boundInfo;
			// Side of each boundary
			dataReader >> nTemp;
			boundInfo.m_eSide = (EBoundary)nTemp;
			// boundary condition
			dataReader >> nTemp;
			boundInfo.m_eCondition = (EBoundaryCondition)nTemp;
			
			if(nDataVersion <= 2)
			{
				// expand?
				bool bTemp = false;
				dataReader >> bTemp;
				
				switch (n + 1)
				{
				case kTop:
					boundInfo.m_eExpandDirection = kExpandUp;
					break;
				case kBottom:
					boundInfo.m_eExpandDirection = kExpandDown;
					break;
				case kLeft:
					boundInfo.m_eExpandDirection = kExpandLeft;
					break;
				case kRight:
					boundInfo.m_eExpandDirection = kExpandRight;
					break;
				default:
					THROW_LOGIC_ERROR_EXCEPTION("ELI13173")
					break;
				}
			}
			else
			{
				// set expand direction
				long nTemp;
				dataReader >> nTemp;
				boundInfo.m_eExpandDirection = (EExpandDirection)nTemp;
			}

			// number of spatial lines or characters to expand
			if(nDataVersion <= 4)
			{
				long lExpandNumber;
				dataReader >> lExpandNumber;
				boundInfo.m_dExpandNumber = lExpandNumber;
			}
			else
			{
				dataReader >> boundInfo.m_dExpandNumber;
			}

			// store in the map
			m_mapBoundaryToInfo[eRegionBoundary] = boundInfo;
		}

		/////////////////
		// get Clue lists
		long nNumOfClueLists = 0;
		dataReader >> nNumOfClueLists;
		// get all clue lists
		for (n = 0; n < nNumOfClueLists; n++)
		{
			nDataLength = 0;
			pStream->Read(&nDataLength, sizeof(nDataLength), NULL);
			ByteStream data2(nDataLength);
			pStream->Read(data2.getData(), nDataLength, NULL);
			ByteStreamManipulator dataReader2(ByteStreamManipulator::kRead, data2);

			long nTemp = 0;
			// clue list index
			dataReader2 >> nTemp;
			EClueListIndex eClueListIndex = (EClueListIndex)nTemp;

			ClueListInfo listInfo;
			if (nDataVersion >= 2)
			{
				dataReader2 >> listInfo.m_bCaseSensitive;
			}
			dataReader2 >> listInfo.m_bAsRegExpr;
			dataReader2 >> listInfo.m_bRestrictByBoundary;

			// Read the clue list
			IPersistStreamPtr ipObj;
			::readObjectFromStream(ipObj, pStream, "ELI09964");
			if (ipObj == __nullptr)
			{
				throw UCLIDException("ELI07802", "Variant Vector for Clue List object could not be read from stream!" );
			}

			listInfo.m_ipClues = ipObj;
			// store in the map
			m_mapIndexToClueListInfo[eClueListIndex] = listInfo;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07740");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLocateImageRegion::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		// store current version number
		dataWriter << gnCurrentVersion;

		dataWriter << (long)m_eFindType;
		dataWriter << m_strImageRegionText;

		// inside/outside the region
		dataWriter << m_bDataInsideBoundaries;
		// include/exclude intersecting entities
		dataWriter << m_bIncludeIntersecting;
		// intersecting entity type
		dataWriter << (long)m_eIntersectingEntity;

		// whether or not to match multiple pages per document
		dataWriter << m_bMatchMultiplePagesPerDocument;

		///////////////////////////
		// store boundary definitions
		// get size of the map of boundary definitions
		long nNumOfBoundaryInfos = m_mapBoundaryToInfo.size();
		dataWriter << nNumOfBoundaryInfos;
		BoundaryToInfo::iterator itBoundaries = m_mapBoundaryToInfo.begin();
		for (; itBoundaries != m_mapBoundaryToInfo.end(); itBoundaries++)
		{
			EBoundary eRegionBoundary = itBoundaries->first;
			dataWriter << (long)eRegionBoundary;
			
			BoundaryInfo boundInfo = itBoundaries->second;
			dataWriter << (long)boundInfo.m_eSide;
			dataWriter << (long)boundInfo.m_eCondition;
			dataWriter << (long)boundInfo.m_eExpandDirection;
			dataWriter << boundInfo.m_dExpandNumber;
		}

		///////////////////
		// store clue lists
		// first get total number of clue lists
		long nNumOfClueLists = m_mapIndexToClueListInfo.size();
		dataWriter << nNumOfClueLists;

		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// loop through all items in the map of clue lists
		IndexToClueListInfo::iterator itClueLists = m_mapIndexToClueListInfo.begin();
		for (; itClueLists != m_mapIndexToClueListInfo.end(); itClueLists++)
		{
			// Write the bytestream data into the IStream object
			ByteStream data2;
			ByteStreamManipulator dataWriter2(ByteStreamManipulator::kWrite, data2);

			EClueListIndex listIndex = itClueLists->first;
			dataWriter2 << (long)listIndex;

			ClueListInfo listInfo = itClueLists->second;
			// store booleans
			dataWriter2 << listInfo.m_bCaseSensitive;
			dataWriter2 << listInfo.m_bAsRegExpr;
			dataWriter2 << listInfo.m_bRestrictByBoundary;

			dataWriter2.flushToByteStream();
			nDataLength = data2.getLength();
			pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
			pStream->Write(data2.getData(), nDataLength, NULL);
			
			// store clue list first
			IPersistStreamPtr ipPersistentObj(listInfo.m_ipClues);
			if (ipPersistentObj == __nullptr)
			{
				throw UCLIDException("ELI07801", "Clue list doesn't support IPersistStream.");
			}
			::writeObjectToStream(ipPersistentObj, pStream, "ELI09919", fClearDirty);
		}

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07741");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLocateImageRegion::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IAttributeFindingRule
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLocateImageRegion::raw_ParseText(IAFDocument* pAFDoc, IProgressStatus *pProgressStatus,
											   IIUnknownVector **pAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{	
		// validate license
		validateLicense();

		IAFDocumentPtr ipAFDoc(pAFDoc);
		ASSERT_ARGUMENT("ELI07949", ipAFDoc != __nullptr);

		ISpatialStringPtr ipInputText = ipAFDoc->Text;
		
		// find regions of text
		IIUnknownVectorPtr ipVecFoundRegions = findRegionContent(ipInputText, pAFDoc);
		ASSERT_RESOURCE_ALLOCATION("ELI16816", ipVecFoundRegions != __nullptr);

		// create the attribute vector to return
		IIUnknownVectorPtr ipAttributes(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI07950", ipAttributes != __nullptr);

		// iterate through each found region
		int iSize = ipVecFoundRegions->Size();
		for(int i=0; i < iSize; i++)
		{
			// put the region into an attribute
			IAttributePtr ipAttribute(CLSID_Attribute);
			ASSERT_RESOURCE_ALLOCATION("ELI07951", ipAttribute != __nullptr);
			ipAttribute->Value = (ISpatialStringPtr) ipVecFoundRegions->At(i);

			// add the attribute to the vector of attributes
			ipAttributes->PushBack(ipAttribute);
		}

		*pAttributes = ipAttributes.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07742");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IAttributeModifyingRule
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLocateImageRegion::raw_ModifyValue(IAttribute* pAttribute, IAFDocument* pOriginInput,
												 IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		IAttributePtr	ipAttr( pAttribute );
		ASSERT_RESOURCE_ALLOCATION("ELI09303", ipAttr != __nullptr);

		ISpatialStringPtr ipInputText = ipAttr->Value;
		ASSERT_ARGUMENT("ELI07946", ipInputText != __nullptr);

		// Get the input text as a copyable object
		ICopyableObjectPtr ipCopier = ipInputText;
		ASSERT_RESOURCE_ALLOCATION("ELI25955", ipCopier != __nullptr);

		// combine all the found regions into a single result string.
		// store that result into ipInputText
		ipCopier->CopyFrom(combineRegions(findRegionContent(ipInputText, pOriginInput)));
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07743");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLocateImageRegion::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19581", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Locate image region").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07744");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLocateImageRegion::raw_IsConfigured(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// TODO: we might want to check for validity of the clue lists along
		// with defined boundaries
		bool bConfigured = !m_mapIndexToClueListInfo.empty() && !m_mapBoundaryToInfo.empty();
		*pbValue = asVariantBool(bConfigured);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07745");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLocateImageRegion::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CLocateImageRegion::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		UCLID_AFVALUEFINDERSLib::ILocateImageRegionPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI08250", ipSource != __nullptr);

		// copy find type and image region text
		m_eFindType = (EFindType)ipSource->FindType;
		m_strImageRegionText = ipSource->ImageRegionText;

		// copy general region settings
		m_bDataInsideBoundaries = asCppBool(ipSource->DataInsideBoundaries);
		m_bIncludeIntersecting = asCppBool(ipSource->IncludeIntersectingEntities);
		m_eIntersectingEntity = ipSource->IntersectingEntityType;
		m_bMatchMultiplePagesPerDocument = asCppBool(ipSource->MatchMultiplePagesPerDocument);

		// clear the map first
		m_mapIndexToClueListInfo.clear();
		int i;
		for (i = kList1; i <= kList4; i++)
		{
			IVariantVectorPtr ipClues = __nullptr;
			VARIANT_BOOL bCaseSensitive = VARIANT_FALSE;
			VARIANT_BOOL bAsRegExpr = VARIANT_FALSE;
			VARIANT_BOOL bRestrictBoundary = VARIANT_FALSE;
			ipSource->GetClueList((UCLID_AFVALUEFINDERSLib::EClueListIndex)i, &ipClues, &bCaseSensitive, &bAsRegExpr, &bRestrictBoundary);
			if (ipClues == __nullptr)
			{
				continue;
			}

			ClueListInfo listInfo;
			
			IShallowCopyablePtr ipCopyObj = ipClues;
			ASSERT_RESOURCE_ALLOCATION("ELI08251", ipCopyObj != __nullptr);
			listInfo.m_ipClues = ipCopyObj->ShallowCopy();
			listInfo.m_bCaseSensitive = asCppBool(bCaseSensitive);
			listInfo.m_bAsRegExpr = asCppBool(bAsRegExpr);
			listInfo.m_bRestrictByBoundary = asCppBool(bRestrictBoundary);
			m_mapIndexToClueListInfo[(EClueListIndex)i] = listInfo;
		}

		m_mapBoundaryToInfo.clear();
		for (i = kTop; i <= kRight; i++)
		{
			BoundaryInfo boundInfo;

			UCLID_AFVALUEFINDERSLib::EBoundaryCondition eCondition;
			UCLID_AFVALUEFINDERSLib::EBoundary eSide;
			UCLID_AFVALUEFINDERSLib::EExpandDirection eExpandDirection;
			double dExpandNumber;
		
			ipSource->GetRegionBoundary((UCLID_AFVALUEFINDERSLib::EBoundary)i, 
				&eSide, &eCondition, &eExpandDirection, &dExpandNumber);
			boundInfo.m_eSide = (EBoundary)eSide;
			boundInfo.m_eCondition = (EBoundaryCondition)eCondition;
			boundInfo.m_eExpandDirection = (EExpandDirection)eExpandDirection;
			boundInfo.m_dExpandNumber = dExpandNumber;
			m_mapBoundaryToInfo[(EBoundary)i] = boundInfo;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08252");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLocateImageRegion::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_LocateImageRegion);
		ASSERT_RESOURCE_ALLOCATION("ELI08346", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07747");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
bool CLocateImageRegion::calculateRoughBorderPosition(BoundaryToInfo mapBorderToInfo, 
													  ISpatialStringPtr ipPageText,
													  IRegularExprParserPtr ipParser)
{
	// get the boundary condition from any one of the map items since
	// all items have the same boundary condition
	EBoundaryCondition eBoundCondition = kNoCondition;
	BoundaryToInfo::iterator itBorderToInfo;
	if (!mapBorderToInfo.empty())
	{
		itBorderToInfo = mapBorderToInfo.begin();
		eBoundCondition = itBorderToInfo->second.m_eCondition;
	}

	// find the clue list associated with the boundary condition
	IndexToClueListInfo::iterator itClueLists = m_mapIndexToClueListInfo.find((EClueListIndex)eBoundCondition);
	// if m_mapBorderToPosition is not empty and with current condition, there's
	// a Restriction on the clues to be found within previous clue lists' boundaries
	if (itClueLists != m_mapIndexToClueListInfo.end())
	{
		ClueListInfo& listInfo = itClueLists->second;
		if (!m_mapBorderToPosition.empty() && listInfo.m_bRestrictByBoundary 
			&& !findCluesWithinBoundary(ipPageText, listInfo, ipParser))
		{
			return false;
		}

		// get bounding rect for the found string
		ILongRectanglePtr ipFoundStringBound = listInfo.m_ipFoundString->GetOCRImageBounds();
		ASSERT_RESOURCE_ALLOCATION("ELI07914", ipFoundStringBound != __nullptr);

		// store this clue list's found string boundary
		itBorderToInfo = mapBorderToInfo.begin();
		for (; itBorderToInfo != mapBorderToInfo.end(); itBorderToInfo++)
		{
			EBoundary eRegionBound = itBorderToInfo->first;
			BoundaryInfo boundInfo = itBorderToInfo->second;
			switch (boundInfo.m_eSide)
			{
			case kTop:
				m_mapBorderToPosition[eRegionBound] = ipFoundStringBound->Top;
				break;
			case kBottom:
				m_mapBorderToPosition[eRegionBound] = ipFoundStringBound->Bottom;
				break;
			case kLeft:
				m_mapBorderToPosition[eRegionBound] = ipFoundStringBound->Left;
				break;
			case kRight:
				m_mapBorderToPosition[eRegionBound] = ipFoundStringBound->Right;
				break;
			default:
				break;
			}
		}
	}
	else	// the boundary condition is page edges
	{
		itBorderToInfo = mapBorderToInfo.begin();
		for (; itBorderToInfo != mapBorderToInfo.end(); itBorderToInfo++)
		{
			EBoundary eRegionBound = itBorderToInfo->first;
			BoundaryInfo boundInfo = itBorderToInfo->second;
			if (boundInfo.m_eCondition == kPage)
			{
				m_mapBorderToPosition[eRegionBound] = 
					getPageBoundaryPosition(boundInfo.m_eSide, ipPageText);
			}
		}
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
long CLocateImageRegion::getPageBoundaryPosition(EBoundary eSide, ISpatialStringPtr ipPageText)
{
	switch (eSide)
	{
	case kTop:
	case kLeft:
		return 0;

	case kBottom:
	case kRight:
		{
			// Get the first spatial page info
			long lPage = ipPageText->GetFirstPageNumber();
			ISpatialPageInfoPtr ipPageInfo = ipPageText->GetPageInfo(lPage);
			ASSERT_RESOURCE_ALLOCATION("ELI25679", ipPageInfo != __nullptr);

			// Return the height and width of the OCR-rotated document [P16 #2659]
			// NOTE: ipPageInfo's height and width are relative to the original, unrotated document
			EOrientation eOrientation = ipPageInfo->Orientation;
			return (eOrientation == kRotNone || eOrientation == kRotDown) ^ (eSide == kBottom) ? 
				ipPageInfo->Width : ipPageInfo->Height;
		}

	default:
		UCLIDException ue("ELI25677", "Unexpected page boundary.");
		ue.addDebugInfo("Boundary number", eSide);
		addCurrentRSDFileToDebugInfo(ue);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CLocateImageRegion::findRegionContent(ISpatialStringPtr ipInputText, 
														 IAFDocument* pDocument)
{
	IAFDocumentPtr ipAFDoc(pDocument);
	ASSERT_RESOURCE_ALLOCATION("ELI14636", ipAFDoc != __nullptr);

	// create an IIUnknownVector to hold the results
	IIUnknownVectorPtr ipVecFoundRegions(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI16817", ipVecFoundRegions != __nullptr);

	// Create IVariantVectorPtr array to backup the clues strings.
	IVariantVectorPtr ipCopy[4];
	
	// Create a regex parser
	IRegularExprParserPtr ipParser = m_ipMisc->GetNewRegExpParserInstance("LocateImageRegion");
	ASSERT_RESOURCE_ALLOCATION("ELI22435", ipParser != __nullptr);

	// Loop through all items in the map of clue lists
	IndexToClueListInfo::iterator itClueLists = m_mapIndexToClueListInfo.begin();
	for (int i = 0; itClueLists != m_mapIndexToClueListInfo.end(); itClueLists++, i++)
	{
		// Save the current clue into ipCopy for later recovery
		// e.g. if the one of the clue is a file name, file://a.txt
		// this clue will be save in ipCopy, and the real clue will be get from this file
		// and replace this file name. But this file name will be recovered as a clue from
		// ipCopy when this method is finished.
		IVariantVectorPtr ipClues = itClueLists->second.m_ipClues;
		ASSERT_RESOURCE_ALLOCATION("ELI25235", ipClues != __nullptr);
		ipCopy[i] = ipClues;
		
		// Get a list of values that includes values from any specified files.
		itClueLists->second.m_ipClues = m_cachedListLoader.expandList(ipClues, ipAFDoc);
	}

	// Restore the clue lists to their original values when clgClues goes out of scope.
	ClueListGuard clgClues(this, ipCopy);

	// The call to GetPages() requires a spatial string in kSpatialMode.
	if (ipInputText->GetMode() != kSpatialMode)
	{
		// return empty list of regions
		return ipVecFoundRegions;
	}

	// break the input into pages
	IIUnknownVectorPtr ipPages = ipInputText->GetPages();

	// instantiate spatial strings to hold the results
	ISpatialStringPtr ipOuterRegion, ipCurrentRegion;
		
	// check if we need to locate content outside of the boundaries
	if(!m_bDataInsideBoundaries)
	{
		// instantiate the found text object
		ipOuterRegion.CreateInstance(CLSID_SpatialString);
		ASSERT_RESOURCE_ALLOCATION("ELI16777", ipOuterRegion != __nullptr);
	}

	// instantiate the index of ipPages
	int i=-1;

	// locate the clues on the first page in ipPages that matches all the clues
	long nFoundIndex = findCluesOnSamePage(ipPages, 0, ipParser);
	
	// loop as long as a page was found
	while(nFoundIndex >= 0)
	{
		// get the content of the region of the found page
		ipCurrentRegion = getPageRegionContent( ipPages->At(nFoundIndex), ipParser );

		// check if the found region should be outside of the boundaries on the page
		if(m_bDataInsideBoundaries)
		{
			// if this page contained any text, add it to the list of found content
			if(ipCurrentRegion != __nullptr && ipCurrentRegion->IsEmpty() == VARIANT_FALSE)
			{
				ipVecFoundRegions->PushBack(ipCurrentRegion);
			}
		}
		else
		{
			// we need to include the text on the pages
			// outside of the page that matched the clues

			// get the pages preceding the found page
			for(i++; i<nFoundIndex; i++)
			{
				ipOuterRegion->Append( (ISpatialStringPtr) ipPages->At(i) );
			}

			if (ipCurrentRegion == __nullptr)
			{
				// Invalid region. Since we are excluding add the current page. 
				// [FlexIDSCore #3165]
				ipCurrentRegion = ipPages->At(nFoundIndex);
				ASSERT_RESOURCE_ALLOCATION("ELI25676", ipCurrentRegion != __nullptr);
			}
			
			if (ipCurrentRegion->IsEmpty() == VARIANT_FALSE)
			{
				// Text was found, add the current page to the found text
				ipOuterRegion->Append( ipCurrentRegion );
			}
		}

		// keep finding clues on subsequent pages 
		// if m_bMatchMultiplePagesPerDocument is set
		nFoundIndex = (m_bMatchMultiplePagesPerDocument ? 
			findCluesOnSamePage(ipPages, nFoundIndex+1, ipParser) : -1);
	}

	// check if we are including text outside of the boundaries
	if (!m_bDataInsideBoundaries)
	{
		// append all the pages that come after the last page with found clues
		// (or if no page with clues were found, append all pages of input text)
		int iSize=ipPages->Size();
		for(i++; i<iSize; i++)
		{
			ipOuterRegion->Append( (ISpatialStringPtr) ipPages->At(i) );
		}

		// add all the found content to the list of found regions
		ipVecFoundRegions->PushBack(ipOuterRegion);
	}

	return ipVecFoundRegions;
}

//-------------------------------------------------------------------------------------------------
ISpatialStringPtr CLocateImageRegion::getPageRegionContent(ISpatialStringPtr ipPage,
														   IRegularExprParserPtr ipParser)
{
	// reset border to position map before each calculation
	m_mapBorderToPosition.clear();

	// we will look at each boundary definition in the order of
	// clue list 1, 2, 3, 4, page
	EBoundaryCondition eCurrentBoundCondition = kClueList1;
	// we need to get all four of the 'rough' region boundaries.
	// 'rough' means it doesn't count the expansion
	while (m_mapBorderToPosition.size() < 4)
	{
		// this map contains different border definitions with same boundary condition
		BoundaryToInfo mapBorderToInfo;
		BoundaryToInfo::iterator itBoundaries = m_mapBoundaryToInfo.begin();
		for (; itBoundaries != m_mapBoundaryToInfo.end(); itBoundaries++)
		{
			// find the earilest condition
			BoundaryInfo boundInfo = itBoundaries->second;
			// if this condition is earlier or later than the current one, 
			// continue on to the next boundary info
			if (boundInfo.m_eCondition != eCurrentBoundCondition)
			{
				continue;
			}
			
			mapBorderToInfo[itBoundaries->first] = boundInfo;
		}
		
		// get border(s) position
		if (!mapBorderToInfo.empty()
			&& !calculateRoughBorderPosition(mapBorderToInfo, ipPage, ipParser))
		{		
			// can't calculate region boundaries, return empty string
			return NULL;
		}
		
		// go to next level of boundary condition
		eCurrentBoundCondition = (EBoundaryCondition)((long)eCurrentBoundCondition + 1);
	}

	switch (m_eFindType)
	{
	case kText:
		{
			// return the found text
			return getFinalResultString(ipPage);
		}
	case kImageRegion:
		{	
			// return the found image region
			return getImageRegion(ipPage);
		}
	default:
		THROW_LOGIC_ERROR_EXCEPTION("ELI13174")
	}
}
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr CLocateImageRegion::getImageRegion(ISpatialStringPtr ipPageText)
{
	// get this page's number in its source document
	// NOTE: this is different from nFoundPage, which
	//       is the index of the page in ipPages. [P16 #2400]
	long lPage = ipPageText->GetFirstPageNumber();

	// get the spatial page info for this page
	ISpatialPageInfoPtr ipPageInfo( ipPageText->GetPageInfo(lPage) );
	ASSERT_RESOURCE_ALLOCATION("ELI20413", ipPageInfo != __nullptr);

	// convert the region into a rect
	ILongRectanglePtr ipRegionBounds = getRegionBounds(ipPageText, ipPageInfo);

	if (ipRegionBounds != __nullptr)
	{
		// Create a raster zone given the region bounds
		IRasterZonePtr	ipZone(CLSID_RasterZone);
		ASSERT_RESOURCE_ALLOCATION("ELI19885", ipZone != __nullptr);

		ipZone->CreateFromLongRectangle(ipRegionBounds, lPage);

		// Create the image region's spatial page info map
		ILongToObjectMapPtr ipPageInfoMap(CLSID_LongToObjectMap);
		ASSERT_RESOURCE_ALLOCATION("ELI20238", ipPageInfoMap != __nullptr);
		ipPageInfoMap->Set(lPage, ipPageInfo);

		// Create a spatial string that will fill the raster zone
		ISpatialStringPtr ipRet(CLSID_SpatialString);
		ASSERT_RESOURCE_ALLOCATION("ELI13175", ipRet != __nullptr);
		ipRet->CreatePseudoSpatialString(ipZone, m_strImageRegionText.c_str(), 
			ipPageText->SourceDocName, ipPageInfoMap);

		return ipRet;
	}
	else
	{
		return NULL;
	}
}
//-------------------------------------------------------------------------------------------------
long CLocateImageRegion::findCluesOnSamePage(IIUnknownVectorPtr ipPages, long lStartIndex,
											 IRegularExprParserPtr ipParser)
{
	// Narrow down the clue lists to search for. i.e. not all clue lists
	// defined in the map shall be searched as long as the boundary definitions
	// do not need'em. For instance, if clue list 1, 2, and 3 are defined, however, 
	// boundary definitions only care about clue list 1 and 3, then clue list 2 
	// shall not be searched at all. If boundary definitions require 'Page 
	// Containing Clues', then clue list 1, 2 and 3 all need to searched.
	
	// contiue only if m_mapIndexToClueListInfo is not empty
	if (m_mapIndexToClueListInfo.empty())
	{
		return -1;
	}

	// found clues on this page
	long nFoundIndex = -1;
	// if kPage is defined, this boolean will be true
	bool bSearchAll = false;
	BoundaryToInfo::iterator itBoundaries = m_mapBoundaryToInfo.begin();
	for (; itBoundaries != m_mapBoundaryToInfo.end(); itBoundaries++)
	{
		EBoundaryCondition eCondition = itBoundaries->second.m_eCondition;
		if (eCondition == kPage)
		{
			bSearchAll = true;
			break;
		}

		if (eCondition >= kClueList1 && eCondition <= kClueList4)
		{
			IndexToClueListInfo::iterator itClueLists =
				m_mapIndexToClueListInfo.find((EClueListIndex)eCondition);
			if (itClueLists != m_mapIndexToClueListInfo.end())
			{
				itClueLists->second.m_bSearchThisList = true;
			}
		}
	}

	// Keep track of any cases where found clues are non-spatial for logging purposes.
	string strDocumentWithNonSpatialClues;

	// as soon as found all clues on one page, return the page number
	long nNumOfPages = ipPages->Size();
	for (long nIndex = lStartIndex; nIndex < nNumOfPages; nIndex++)
	{
		// get each page content
		ISpatialStringPtr ipPageText = ipPages->At(nIndex);
		ASSERT_RESOURCE_ALLOCATION("ELI07889", ipPageText != __nullptr);

		long pageLen = (long)ipPageText->String.length();
		
		// flag to indicate all clues are found on the same page
		bool bAllFound = true;

		// how many clue lists are actually searched
		int nNumOfClueListsSearched = 0;

		// go through all defined clue lists in the order of list 1 to list 4
		IndexToClueListInfo::iterator itClueLists = m_mapIndexToClueListInfo.begin();
		for (; itClueLists != m_mapIndexToClueListInfo.end(); itClueLists++)
		{
			ClueListInfo &clueListInfo = itClueLists->second;

			if (bSearchAll)
			{
				clueListInfo.m_bSearchThisList = true;
			}
			
			if (clueListInfo.m_bSearchThisList)
			{
				nNumOfClueListsSearched++;
				// start and end position of found text
				long nStart(0), nEnd(0);
				// Loop in case the first clue found is not spatial; nSearchStart indicates the
				// point from which the next search should commence.
				for (long nSearchStart = 0; nStart != -1 && nSearchStart < pageLen;
					nSearchStart = nStart + 1)
				{
					if (clueListInfo.m_bAsRegExpr)
					{
						// treat vector of clues as prioritized. [P16 #1956]
						ipPageText->FindFirstItemInRegExpVector(
							clueListInfo.m_ipClues, 
							asVariantBool(clueListInfo.m_bCaseSensitive), 
							VARIANT_TRUE, nSearchStart, ipParser, &nStart, &nEnd);
					}
					else
					{
						// search for current clue lists
						// treat vector of clues as prioritized. [P16 #1956]
						ipPageText->FindFirstItemInVector(
							clueListInfo.m_ipClues, 
							asVariantBool(clueListInfo.m_bCaseSensitive), 
							VARIANT_TRUE, nSearchStart, &nStart, &nEnd);
					}
					
					// if can't find any on the page, go onto the next page
					if (nStart == -1)
					{
						// break out of the inner loop
						bAllFound = false;
						break;
					}

					ISpatialStringPtr ipFound = ipPageText->GetSubString(nStart, nEnd);
					ASSERT_RESOURCE_ALLOCATION("ELI27680", ipFound != __nullptr);

					if (asCppBool(ipFound->HasSpatialInfo()))
					{
						// Store any found string with spatial info and break out of loop.
						clueListInfo.m_ipFoundString = ipFound;
						break;
					}
					else
					{
						// [DataEntry:425]
						// If the found string is non-spatial, it can't be used to define an image region.
						// Ignore non-spatial string, but continue to look for more on this page.
						strDocumentWithNonSpatialClues = asString(ipPageText->SourceDocName);
					}
				}
			}
		}

		if (bAllFound && nNumOfClueListsSearched > 0)
		{
			// seems all clues are found on this page, break out of the outer loop
			nFoundIndex = nIndex;
			break;
		}
	}

	// Whether or not any spatial strings were finally found, log the fact that non-spatial strings
	// were found and skipped to facilitate tracking down possible problems in rulesets.
	if (!strDocumentWithNonSpatialClues.empty())
	{
		UCLIDException ue("ELI27681",
			"Application trace: Ignored one or more clues that did not contain spatial info.");
		addCurrentRSDFileToDebugInfo(ue);
		ue.addDebugInfo("File", strDocumentWithNonSpatialClues);
		ue.log();
	}
	
	return nFoundIndex;
}
//-------------------------------------------------------------------------------------------------
bool CLocateImageRegion::findCluesWithinBoundary(ISpatialStringPtr ipPageText, 
												 ClueListInfo& rlistInfo,
												 IRegularExprParserPtr ipParser)
{
	if (!m_mapBorderToPosition.empty() && rlistInfo.m_bRestrictByBoundary)
	{
		if (rlistInfo.m_ipFoundString != __nullptr)
		{
			// Get the bounds of the found clue
			ILongRectanglePtr ipFoundClueBoundary = rlistInfo.m_ipFoundString->GetOCRImageBounds();
			ASSERT_RESOURCE_ALLOCATION("ELI25632", ipFoundClueBoundary != __nullptr);

			// check if the found clue is already within the boundaries
			map<EBoundary, long>::iterator itBorderToPosition = m_mapBorderToPosition.begin();
			for (; itBorderToPosition != m_mapBorderToPosition.end(); itBorderToPosition++)
			{
				bool bOutsideBoudary = false;
				EBoundary eBorder = itBorderToPosition->first;
				long nPosition = itBorderToPosition->second;
				switch (eBorder)
				{
				case kTop:
					if (nPosition > ipFoundClueBoundary->Top)
					{
						bOutsideBoudary = true;
					}
					break;
				case kBottom:
					if (nPosition < ipFoundClueBoundary->Bottom)
					{
						bOutsideBoudary = true;
					}
					break;
				case kLeft:
					if (nPosition > ipFoundClueBoundary->Left)
					{
						bOutsideBoudary = true;
					}
					break;
				case kRight:
					if (nPosition < ipFoundClueBoundary->Right)
					{
						bOutsideBoudary = true;
					}
					break;
				default:
					break;
				}
				
				// if the found clue is actually outside the defined boundaries
				if (bOutsideBoudary)
				{
					break;
				}
				
				// as this point is reached, the found clue is acutally well
				// within the defined boundaries, no need to update the 
				// m_ipFoundString in the rlistInfo
				return true;
			}
		}

		// get string within boundaries
		ISpatialStringPtr ipStringInBound = getStringWithinBoundary(ipPageText);
		ASSERT_RESOURCE_ALLOCATION("ELI25633", ipStringInBound != __nullptr);

		// find the clue list in the provided string
		long nStart, nEnd;
		if (rlistInfo.m_bAsRegExpr)
		{
			// treat vector of clues as prioritized. [P16 #1956]
			ipStringInBound->FindFirstItemInRegExpVector(
				rlistInfo.m_ipClues, 
				asVariantBool(rlistInfo.m_bCaseSensitive), 
				VARIANT_TRUE, 0, ipParser, &nStart, &nEnd);
		}
		else
		{
			// search for current clue lists.
			// treat vector of clues as prioritized. [P16 #1956]
			ipStringInBound->FindFirstItemInVector(
				rlistInfo.m_ipClues, 
				asVariantBool(rlistInfo.m_bCaseSensitive), 
				VARIANT_TRUE, 0, &nStart, &nEnd);
		}

		// if not found, return false
		if (nStart == -1)
		{
			return false;
		}

		// update the found string
		rlistInfo.m_ipFoundString = ipStringInBound->GetSubString(nStart, nEnd);
		ASSERT_RESOURCE_ALLOCATION("ELI25634", rlistInfo.m_ipFoundString != __nullptr);
		return true;
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
long CLocateImageRegion::getExpandPixels(EBoundary eRegionBound, ISpatialStringPtr ipPageText)
{
	BoundaryToInfo::iterator itBound = m_mapBoundaryToInfo.find(eRegionBound);
	if (itBound != m_mapBoundaryToInfo.end())
	{
		BoundaryInfo boundInfo = itBound->second;
		if (boundInfo.m_dExpandNumber > 0)
		{
			// check if this is a page edge condition [P16 #2900]
			if(boundInfo.m_eCondition == kPage)
			{
				// get the expand pixels using the average line height or average character width
				// of the whole page that matched the clues
				return getExpandPixels(eRegionBound, boundInfo, ipPageText);		
			}

			// look for the boundary condition associated clue list if any
			IndexToClueListInfo::iterator itClueList = 
				m_mapIndexToClueListInfo.find((EClueListIndex)boundInfo.m_eCondition);
			if (itClueList != m_mapIndexToClueListInfo.end())
			{
				// get the expand pixels using the average line height or average character width
				// of the clue string
				return getExpandPixels(eRegionBound, boundInfo, itClueList->second.m_ipFoundString);
			}
		}
	}

	return 0;
}
//-------------------------------------------------------------------------------------------------
long CLocateImageRegion::getExpandPixels(EBoundary eRegionBound, BoundaryInfo boundInfo, 
					 ISpatialStringPtr ipSpatialString)
{	
	// get the expand number (the number of units to expand)
	double dExpandPixels = boundInfo.m_dExpandNumber;

	// calculate the unit of expansion (average line height or char width) and
	// determine pixel expansion direction by negating the expand pixels when appropriate
	switch (eRegionBound)
	{
	case kTop:
	case kBottom:
		{
			// multiply the expand number by the average line height in pixels
			dExpandPixels *= ipSpatialString->GetAverageLineHeight();

			// negate the sign if expanding upwards
			if(boundInfo.m_eExpandDirection == kExpandUp)
			{
				dExpandPixels *= -1;
			}
		}
		break;
	case kLeft:
	case kRight:
		{
			// multiply the expand number by the character width in pixels
			dExpandPixels *= ipSpatialString->GetAverageCharWidth();

			// negate the sign if expanding to the left
			if(boundInfo.m_eExpandDirection == kExpandLeft)
			{
				dExpandPixels *= -1;
			}
			break;
		}
	}

	// return the number of pixels to expand, rounded to the nearest pixel
	return (long) floor(dExpandPixels + .5);
}
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr CLocateImageRegion::getFinalResultString(ISpatialStringPtr ipPageText)
{
	// convert the region of the first page into a rect
	// This previously used a hard coded page '1' PVCS P13:4179
	long nFirstPage = ipPageText->GetFirstPageNumber();
	ILongRectanglePtr ipRegionBounds = getRegionBounds(ipPageText, 
		ipPageText->GetPageInfo(nFirstPage));

	if (ipRegionBounds != __nullptr)
	{
		// initialize the spatial searcher with found page text
		m_ipSpatialStringSearcher->InitSpatialStringSearcher(ipPageText);
		m_ipSpatialStringSearcher->SetIncludeDataOnBoundary( asVariantBool(m_bIncludeIntersecting) );
		m_ipSpatialStringSearcher->SetBoundaryResolution(m_eIntersectingEntity);

		ISpatialStringPtr ipRet(NULL);
		// get the string we are looking for
		if (m_bDataInsideBoundaries)
		{
			// Do not rotate the rectangle per the OCR
			ipRet = m_ipSpatialStringSearcher->GetDataInRegion( ipRegionBounds, VARIANT_FALSE );
		}
		else
		{
			ipRet = m_ipSpatialStringSearcher->GetDataOutOfRegion(ipRegionBounds);
		}

		return ipRet;
	}
	else
	{
		return NULL;
	}
}
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr CLocateImageRegion::getStringWithinBoundary(ISpatialStringPtr ipInput)
{
	ASSERT_ARGUMENT("ELI25635", ipInput != __nullptr);

	ISpatialStringPtr ipRet = __nullptr;
	if (!m_mapBorderToPosition.empty())
	{
		m_ipSpatialStringSearcher->InitSpatialStringSearcher(ipInput);
		m_ipSpatialStringSearcher->SetIncludeDataOnBoundary(
										asVariantBool(m_bIncludeIntersecting) );
		m_ipSpatialStringSearcher->SetBoundaryResolution(m_eIntersectingEntity);

		// create a rectangular bound
		ILongRectanglePtr ipRectBound(CLSID_LongRectangle);
		ASSERT_RESOURCE_ALLOCATION("ELI07923", ipRectBound != __nullptr);

		// first set this rect to the edges of the page
		ipRectBound->SetBounds(-1, -1, -1, -1);

		map<EBoundary, long>::iterator it = m_mapBorderToPosition.begin();
		for (; it != m_mapBorderToPosition.end(); it++)
		{
			EBoundary eRegionBound = it->first;
			long nPosition = it->second;
			switch (eRegionBound)
			{
			case kTop:
				// get anything below the top border
				ipRectBound->Top = nPosition;
				break;
			case kBottom:
				// get anything above the bottom border
				ipRectBound->Bottom = nPosition;
				break;
			case kLeft:
				// get anything left to the left border
				ipRectBound->Left = nPosition;
				break;
			case kRight:
				// get anything right to the right border
				ipRectBound->Right = nPosition;
				break;
			default:
				break;
			}
		}

		// now get the string within the bound, without rotating the rectangle
		ipRet = m_ipSpatialStringSearcher->GetDataInRegion(ipRectBound, VARIANT_FALSE);
	}

	return ipRet;
}
//-------------------------------------------------------------------------------------------------
ILongRectanglePtr CLocateImageRegion::getRegionBounds(ISpatialStringPtr ipPageText,
													  ISpatialPageInfoPtr ipPageInfo)
{
	if (!m_mapBorderToPosition.empty())
	{
		ASSERT_ARGUMENT("ELI20414", ipPageInfo != __nullptr);

		// convert the region into a rect
		ILongRectanglePtr ipRegionBounds(CLSID_LongRectangle);
		ASSERT_RESOURCE_ALLOCATION("ELI07970", ipRegionBounds != __nullptr);

		// get the height and width of the OCR-rotated document [P16 #2659]
		// NOTE: ipPageInfo's height and width are relative to the original, unrotated document
		long lHeight, lWidth;
		EOrientation eOrientation = ipPageInfo->Orientation;
		if(eOrientation == kRotNone || eOrientation == kRotDown)
		{
			// original image's height and width are the same as the OCR-rotated document
			lHeight = ipPageInfo->Height;
			lWidth = ipPageInfo->Width;
		}
		else
		{
			// the original image was rotated 90 degrees, so the height and width are transposed
			lHeight = ipPageInfo->Width;
			lWidth = ipPageInfo->Height;
		}

		map<EBoundary, long>::iterator itRegion = m_mapBorderToPosition.begin();
		for (; itRegion != m_mapBorderToPosition.end(); itRegion++)
		{
			EBoundary eRegionBound = itRegion->first;
			long nPosition = itRegion->second;
			long nExpandPixels = getExpandPixels(eRegionBound, ipPageText);

			switch (eRegionBound)
			{
				case kTop:
					{
						// ensure the region does not expand past the top of the page
						if(nPosition + nExpandPixels < 0)
						{
							ipRegionBounds->Top = 0;
						}
						else
						{
							ipRegionBounds->Top = nPosition + nExpandPixels;
						}
						break;
					}
				case kBottom:
					{
						// ensure the region does not expand past the bottom of the page
						if(nPosition + nExpandPixels >= lHeight)
						{
							ipRegionBounds->Bottom = lHeight - 1;
						}
						else
						{
							ipRegionBounds->Bottom = nPosition + nExpandPixels;
						}
						break;
					}
				case kLeft:
					{
						// ensure the region does not expand past the left of the page
						if(nPosition + nExpandPixels < 0)
						{
							// Leftmost bound is pixel 1 (P16 #1918)
							ipRegionBounds->Left = 1;
						}
						else
						{
							ipRegionBounds->Left = nPosition + nExpandPixels;
						}
						break;
					}
				case kRight:
					{
						// ensure the region does not expand past the right of the page
						if(nPosition + nExpandPixels >= lWidth)
						{
							ipRegionBounds->Right = lWidth-1;
						}
						else
						{
							ipRegionBounds->Right = nPosition + nExpandPixels;
						}
						break;
					}
				default:
					THROW_LOGIC_ERROR_EXCEPTION("ELI13180")
					break;
			}
		}

		// confirm the boundary conditions are valid
		if ((ipRegionBounds->Top < ipRegionBounds->Bottom) && (ipRegionBounds->Right > ipRegionBounds->Left))
		{
			return ipRegionBounds;
		}
	}

	return NULL;
}
//-------------------------------------------------------------------------------------------------
void CLocateImageRegion::recoverClueList(IVariantVectorPtr* ppvecClues)
{
	// Loop through all items in the map of clue lists
	IndexToClueListInfo::iterator itClueLists = m_mapIndexToClueListInfo.begin();
	for (int i = 0; itClueLists != m_mapIndexToClueListInfo.end(); itClueLists++, i++)
	{
		// If the clue has been modified because it is a file name
		if (ppvecClues[i] && itClueLists->second.m_ipClues != ppvecClues[i])
		{
			// Reset the clue to the original value
			itClueLists->second.m_ipClues = ppvecClues[i];
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CLocateImageRegion::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI07748", "Locate Image Region");
}
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr CLocateImageRegion::combineRegions(IIUnknownVectorPtr ipVecImageRegions, 
													 _bstr_t bstrSourceDocName)
{
	ASSERT_ARGUMENT("ELI16818", ipVecImageRegions != __nullptr);

	// output string
	ISpatialStringPtr ipResult(CLSID_SpatialString);
	ASSERT_RESOURCE_ALLOCATION("ELI07948", ipResult != __nullptr);

	// combine regions into single spatial string
	int iSize = ipVecImageRegions->Size();
	for(int i=0; i < iSize; i++)
	{
		ipResult->Append( (ISpatialStringPtr) ipVecImageRegions->At(i) );
	}

	// assign the source document name if one was specified. [P16 #2729]
	if(bstrSourceDocName.length() != 0)
	{
		ipResult->SourceDocName = bstrSourceDocName;
	}

	// return the combined regions
	return ipResult;
}

//-------------------------------------------------------------------------------------------------
// ClueListGuard
//-------------------------------------------------------------------------------------------------
CLocateImageRegion::ClueListGuard::ClueListGuard(CLocateImageRegion* pLocateImageRegion,
												 IVariantVectorPtr* pipVecClues)
:m_pLocateImageRegion(pLocateImageRegion)
{
	for(int i=0; i<4; i++)
	{
		m_ipVecClues[i] = pipVecClues[i];
	}
}
//-------------------------------------------------------------------------------------------------
CLocateImageRegion::ClueListGuard::~ClueListGuard()
{
	try
	{
		m_pLocateImageRegion->recoverClueList(m_ipVecClues);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16951");
}
//-------------------------------------------------------------------------------------------------
