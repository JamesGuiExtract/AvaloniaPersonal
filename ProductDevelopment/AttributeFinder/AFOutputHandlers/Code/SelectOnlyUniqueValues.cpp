// SelectOnlyUniqueValues.cpp : Implementation of CSelectOnlyUniqueValues
#include "stdafx.h"
#include "AFOutputHandlers.h"
#include "SelectOnlyUniqueValues.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CSelectOnlyUniqueValues
//-------------------------------------------------------------------------------------------------
CSelectOnlyUniqueValues::CSelectOnlyUniqueValues()
: m_bDirty(false)
{
}
//-------------------------------------------------------------------------------------------------
CSelectOnlyUniqueValues::~CSelectOnlyUniqueValues()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16319");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectOnlyUniqueValues::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IOutputHandler,
		&IID_ICategorizedComponent,
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
STDMETHODIMP CSelectOnlyUniqueValues::raw_ProcessOutput(IIUnknownVector* pAttributes,
														IAFDocument *pAFDoc,
														IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		IIUnknownVectorPtr ipOriginAttributes(pAttributes);

		if (ipOriginAttributes != __nullptr && ipOriginAttributes->Size() > 0)
		{
			// the vector that stores indices of non-unique values 
			// in the original attributes vector
			vector<long> vecIndices;
			long nCurrentItemIndex = 0;
			long nSize = 0;
			long nIndex = 0;
			// a flag that indicates whether a given name of an attribute
			// has unique value
			bool bUnique = true;
			while (true)
			{
				// update the temp vector size
				nSize = ipOriginAttributes->Size();
				if (nSize == 0 || nCurrentItemIndex >= nSize - 1)
				{
					// if there's no more item in the vector, or there's
					// no more non-unique values, then break out of the while loop
					break;
				}

				vecIndices.clear();
				vecIndices.push_back(nCurrentItemIndex);
				bUnique = true;
				// update the current attribute
				IAttributePtr ipCurrentAttr(ipOriginAttributes->At(nCurrentItemIndex));
				// compare current item with the rest of the items
				// in the vector and find same attribute name
				// with different attribute value
				for (nIndex = nCurrentItemIndex+1; nIndex < nSize; nIndex++)
				{
					IAttributePtr ipNextAttr(ipOriginAttributes->At(nIndex));
					string strCurrentName = ipCurrentAttr->Name;
					string strNextName = ipNextAttr->Name;
					if (strCurrentName == strNextName)
					{
						// Retrieve the values
						ISpatialStringPtr ipCurrent = ipCurrentAttr->Value;
						ASSERT_RESOURCE_ALLOCATION("ELI15529", ipCurrent != __nullptr);
						ISpatialStringPtr ipNext = ipNextAttr->Value;
						ASSERT_RESOURCE_ALLOCATION("ELI15530", ipNext != __nullptr);

						// Compare the strings
						vecIndices.push_back(nIndex);
						string strCurrentValue = ipCurrent->String;
						string strNextValue = ipNext->String;
						if (strCurrentValue != strNextValue)
						{
							bUnique = false;
						}
					}
				}

				if (vecIndices.size() > 1 && !bUnique)
				{
					// if there's more than one value for the same attribute name,
					// remove them from the temp vector of attributes
					for (unsigned long ul = 0; ul < vecIndices.size(); ul++)
					{
						ipOriginAttributes->Remove(vecIndices[ul] - ul);
					}
				}
				else if (vecIndices.size() > 1 && bUnique)
				{
					// remove duplicate entries
					for (unsigned long ul = 1; ul < vecIndices.size(); ul++)
					{
						ipOriginAttributes->Remove(vecIndices[ul] - (ul - 1));
					}

					nCurrentItemIndex++;
				}
				else
				{
					// if current item is the unique value, 
					// then keep'em and check next item
					nCurrentItemIndex++;
				}
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05037")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectOnlyUniqueValues::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19555", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Select only unique attributes").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05038")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectOnlyUniqueValues::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_SelectOnlyUniqueValues;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectOnlyUniqueValues::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectOnlyUniqueValues::Load(IStream *pStream)
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
			UCLIDException ue( "ELI07762", 
				"Unable to load newer SelectOnlyUniqueValues Output Handler!" );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07763");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectOnlyUniqueValues::Save(IStream *pStream, BOOL fClearDirty)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07764");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectOnlyUniqueValues::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectOnlyUniqueValues::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CSelectOnlyUniqueValues::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSelectOnlyUniqueValues::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_SelectOnlyUniqueValues);
		ASSERT_RESOURCE_ALLOCATION("ELI05269", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05270");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CSelectOnlyUniqueValues::validateLicense()
{
	static const unsigned long SELECT_UNIQUE_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( SELECT_UNIQUE_ID, "ELI05039", 
		"Select Only Unique Attributes Output Handler" );
}
//-------------------------------------------------------------------------------------------------
