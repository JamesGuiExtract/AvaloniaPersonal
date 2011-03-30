// SelectUsingMajority.cpp : Implementation of CSelectUsingMajority
#include "stdafx.h"
#include "AFOutputHandlers.h"
#include "SelectUsingMajority.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CSelectUsingMajority
//-------------------------------------------------------------------------------------------------
CSelectUsingMajority::CSelectUsingMajority()
: m_bDirty(false)
{
}
//-------------------------------------------------------------------------------------------------
CSelectUsingMajority::~CSelectUsingMajority()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16320");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectUsingMajority::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IOutputHandler,
		&IID_ICategorizedComponent,
		&IID_IPersistStream,
		&IID_ICopyableObject,
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
// IOutputHandler
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectUsingMajority::raw_ProcessOutput(IIUnknownVector* pAttributes, IAFDocument *pAFDoc,
													 IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{	
		validateLicense();
		IIUnknownVectorPtr ipOriginAttributes(pAttributes);
		ASSERT_RESOURCE_ALLOCATION("ELI10490", ipOriginAttributes != __nullptr);
		IIUnknownVectorPtr ipReturnAttributes(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI10491", ipReturnAttributes != __nullptr);

		// vector will contain unique attributes with number 
		// of occurrence of same value. 
		vector<AttributeAndNumber> vecAttrAndNum;
		// For example, original attributes are 
		// Name		Value
		// State	 WI
		// State	 IL
		// State	 WI
		// State	 MN
		// City		Madison
		// City		Beloit
		// City		Madison
		// City		Chicago
		// ..................
		// The vector will have
		// Step (1)
		// Attribute of State - WI, number as 2
		// Attribute of State - IL, number as 1
		// Attribute of State - MN, number as 1
		// Attribute of City - Madison, number as 2
		// Attribute of City - Beloit, number as 1
		// Attribute of City - Chicago, number as 1
		// ....................
		// And final result shall be 
		// Step (2)
		// Attribute of State - WI, number as 2
		// Attribute of City - Madison, number as 2

		// Step(1)
		bool bSameNameValueFound = false;
		long nSize = ipOriginAttributes->Size();
		for (long nIndex=0; nIndex<nSize; nIndex++)
		{
			bSameNameValueFound = false;

			IAttributePtr ipCurrentAttribute(ipOriginAttributes->At(nIndex));
			// compare name/value of current attribute with each attribute in the vector
			for (unsigned int ui = 0; ui < vecAttrAndNum.size(); ui++)
			{
				// Retrieve the strings
				ISpatialStringPtr ipCurr = ipCurrentAttribute->Value;
				ASSERT_RESOURCE_ALLOCATION("ELI15531", ipCurr != __nullptr);
				ISpatialStringPtr ipAttr = vecAttrAndNum[ui].ipAttribute->Value;
				ASSERT_RESOURCE_ALLOCATION("ELI15532", ipAttr != __nullptr);

				string strCurName = ipCurrentAttribute->Name;
				string strCurValue = ipCurr->String;
				string strAttrName = vecAttrAndNum[ui].ipAttribute->Name;
				string strAttrValue = ipAttr->String;

				// Compare the names and values
				if (strCurName == strAttrName && strCurValue == strAttrValue)
				{	
					vecAttrAndNum[ui].nNumOfSameValue++;
					bSameNameValueFound = true;
					break;
				}
			}
			
			if (!bSameNameValueFound)
			{
				AttributeAndNumber structAttrAndNum;
				structAttrAndNum.ipAttribute = ipCurrentAttribute;
				structAttrAndNum.nNumOfSameValue = 1;
				vecAttrAndNum.push_back(structAttrAndNum);
			}
		}

		// Step(2)
		// only keep those attributes that have most frequent occurrence
		int nCurrentIndex = 0;
		int nVecSize = vecAttrAndNum.size();
		while (nCurrentIndex < nVecSize)
		{
			// if current item is marked as dirty already, go to next one
			if (vecAttrAndNum[nCurrentIndex].bDirty)
			{
				nCurrentIndex++;
				continue;
			}
			// iterate through vecAttrAndNum and find each attribute with the largest
			// repeating number of values along with it.
			for (int i=nCurrentIndex+1; i<nVecSize; i++)
			{
				if (vecAttrAndNum[i].bDirty)
				{
					continue;
				}
				// alway compare the attribute with same name
				string strCurrentName = vecAttrAndNum[nCurrentIndex].ipAttribute->Name;
				string strAttrName = vecAttrAndNum[i].ipAttribute->Name;
				if (strCurrentName == strAttrName)
				{
					if (vecAttrAndNum[nCurrentIndex].nNumOfSameValue < vecAttrAndNum[i].nNumOfSameValue)
					{
						// if current item has less num
						// bDirty flag indicates whether current item can be removed
						vecAttrAndNum[nCurrentIndex].bDirty = true;
					}
					else if (vecAttrAndNum[nCurrentIndex].nNumOfSameValue == vecAttrAndNum[i].nNumOfSameValue)
					{
						// if they are identical, both can be moved
						vecAttrAndNum[nCurrentIndex].bDirty = true;
						vecAttrAndNum[i].bDirty = true;
					}
					else
					{
						// only remove this
						vecAttrAndNum[i].bDirty = true;
					}
				}
			}

			// now increment the current index 
			nCurrentIndex++;
		}

		// store every item that's not dirty in the vec into the returning vec
		for (unsigned int ui = 0; ui < vecAttrAndNum.size(); ui++)
		{
			if (!vecAttrAndNum[ui].bDirty)
			{
				ipReturnAttributes->PushBack(vecAttrAndNum[ui].ipAttribute);
			}
		}

		// clear the in/out vector
		ipOriginAttributes->Clear();
		long nReturnSize = ipReturnAttributes->Size();
		// Fill it with the values we want to return
		for (int i = 0; i < nReturnSize; i++)
		{
			ipOriginAttributes->PushBack(ipReturnAttributes->At(i));
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05040")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectUsingMajority::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19556", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Select using majority").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05041")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectUsingMajority::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_SelectUsingMajority;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectUsingMajority::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectUsingMajority::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

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
			UCLIDException ue( "ELI07765", 
				"Unable to load newer SelectUsingMajority Output Handler!" );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07766");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectUsingMajority::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter( ByteStreamManipulator::kWrite, data );
		dataWriter << gnCurrentVersion;
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), NULL );
		pStream->Write( data.getData(), nDataLength, NULL );

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07767");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectUsingMajority::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectUsingMajority::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pbValue == NULL)
		{
			return E_POINTER;
		}

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
STDMETHODIMP CSelectUsingMajority::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectUsingMajority::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_SelectUsingMajority);
		ASSERT_RESOURCE_ALLOCATION("ELI05271", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05272");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CSelectUsingMajority::validateLicense()
{
	static const unsigned long SELECT_MAJORITY_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( SELECT_MAJORITY_ID, "ELI05042", "Select Majority Output Handler" );
}
//-------------------------------------------------------------------------------------------------
