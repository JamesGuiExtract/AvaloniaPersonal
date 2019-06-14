// VariantVector.cpp : Implementation of CVariantVector
#include "stdafx.h"
#include "UCLIDCOMUtils.h"
#include "VariantVector.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>

#include <algorithm>
using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantVector::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IVariantVector,
		&IID_ICopyableObject,
		&IID_IShallowCopyable,
		&IID_IPersistStream,
		&IID_ILicensedComponent,
		&IID_IOCRParameters
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// CVariantVector
//-------------------------------------------------------------------------------------------------
CVariantVector::CVariantVector()
:m_bDirty(false)
{
}
//-------------------------------------------------------------------------------------------------
CVariantVector::~CVariantVector()
{
	try
	{
		m_vecVarCollection.clear();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16520");
}

//-------------------------------------------------------------------------------------------------
// IVariantVector
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantVector::get_Item(long nIndex, VARIANT *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26052", pVal != __nullptr);

		validateLicense();
		
		if ((unsigned long) nIndex < m_vecVarCollection.size())
		{
			_variant_t vtTemp = m_vecVarCollection.at(nIndex);

			return VariantCopy(pVal, &vtTemp);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03874");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantVector::get_Size(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26053", pVal != __nullptr);

		validateLicense();

		*pVal = m_vecVarCollection.size();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03875");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantVector::Clear()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_vecVarCollection.clear();
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03876");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantVector::PushBack(VARIANT vtItem)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// Validate the type of the item before inserting
		_variant_t vtAddItem(vtItem);
		validateType(vtAddItem);

		m_vecVarCollection.push_back(vtAddItem);		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03877");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantVector::Remove(long nBeginIndex, long nNumOfItems)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		long lSize = m_vecVarCollection.size();
		if (nNumOfItems >= 1 && nBeginIndex < lSize)
		{
			long nEndIndex = nBeginIndex + nNumOfItems;
			if (nEndIndex > lSize || nEndIndex <= 0)
			{
				nEndIndex = lSize;
			}
			m_vecVarCollection.erase(m_vecVarCollection.begin() + nBeginIndex, 
									 m_vecVarCollection.begin() + nEndIndex);
		}

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03878");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantVector::Contains(VARIANT vtItem, VARIANT_BOOL *bValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26054", bValue != __nullptr);

		std::vector<_variant_t>::const_iterator iter;
		iter = find(m_vecVarCollection.begin(), m_vecVarCollection.end(), vtItem);
		*bValue = asVariantBool(iter != m_vecVarCollection.end());
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04400");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantVector::Find(VARIANT vtItem, long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26055", pVal != __nullptr);

		// Get item as _variant_t
		_variant_t vtTestItem(vtItem);

		// Default the return value to -1 (not found)
		long lRetVal = -1;

		// Loop through the collection looking for the item
		long lSize = m_vecVarCollection.size();
		for (long i=0; i < lSize; i++)
		{
			// Check for item
			if (m_vecVarCollection[i] == vtTestItem)
			{
				// Item was found, set the return index and break from loop
				lRetVal = i;
				break;
			}
		}

		// Return the index
		*pVal = lRetVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04729");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantVector::Insert(long nIndex, VARIANT vtItem)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Get current size and check index
		long lCount = m_vecVarCollection.size();
		if (nIndex > lCount || nIndex < 0)
		{
			// Create and throw exception
			UCLIDException	ue( "ELI04731", "Invalid index for Insert.");
			ue.addDebugInfo( "Index", nIndex );
			ue.addDebugInfo( "Size", lCount );
			throw ue;
		}

		// Validate the type of the item
		_variant_t vtItemToInsert(vtItem);
		validateType(vtItemToInsert);

		// Is the new item being added at the end of the vector
		if (nIndex == lCount)
		{
			// Just add it and stop
			m_vecVarCollection.push_back(vtItemToInsert);
		}
		else
		{
			// Create iterator and locate index
			vector<_variant_t>::iterator iter = m_vecVarCollection.begin() + nIndex;

			// Insert the item
			m_vecVarCollection.insert( iter, vtItemToInsert );
		}// end else new item is not just being appended

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04730");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantVector::Append(IVariantVector *pVector)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// wrap pVector as smart pointer
		UCLID_COMUTILSLib::IVariantVectorPtr ipVector(pVector);
		ASSERT_RESOURCE_ALLOCATION("ELI16911", ipVector != __nullptr);

		// Get count of objects to be appended
		long lNumItems = ipVector->Size;

		// Append object references from pVector to our internal vector
		for (int i = 0; i < lNumItems; i++)
		{
			// Get the item (no need to validate type since its coming from an IVariantVector)
			_variant_t vtItem = ipVector->GetItem(i);

			// Add item to end of our vector
			m_vecVarCollection.push_back( vtItem );
		}

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08727");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantVector::InsertString(long nIndex, BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Get current size and check index
		long lCount = m_vecVarCollection.size();
		if (nIndex > lCount || nIndex < 0)
		{
			// Create and throw exception
			UCLIDException	ue( "ELI09393", "Invalid index for InsertString.");
			ue.addDebugInfo( "Index", nIndex );
			ue.addDebugInfo( "Size", lCount );
			throw ue;
		}

		// Create a VARIANT for the new string
		_variant_t vtItem = _bstr_t(newVal);

		// Is the new item being added at the end of the vector
		if (nIndex == lCount)
		{
			// Just add it and stop
			m_vecVarCollection.push_back( vtItem );
		}
		else
		{
			// Create iterator and locate index
			vector<_variant_t>::iterator iter = m_vecVarCollection.begin() + nIndex;

			// Insert the item
			m_vecVarCollection.insert( iter, vtItem );
		}// end else new item is not just being appended

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09392");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantVector::Set(long nIndex, VARIANT vtItem)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Get current size and check index
		long lCount = m_vecVarCollection.size();
		if (nIndex >= lCount || nIndex < 0)
		{
			// Create and throw exception
			UCLIDException	ue("ELI26084", "Invalid index for Replace.");
			ue.addDebugInfo( "Index", nIndex );
			ue.addDebugInfo( "Size", lCount );
			throw ue;
		}

		// Validate the type of the item
		_variant_t vtItemToInsert(vtItem);
		validateType(vtItemToInsert);

		// Replace the item at the specified index
		m_vecVarCollection[nIndex] = vtItemToInsert;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26083");
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantVector::CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		// clear the other vector object
		m_vecVarCollection.clear();

		UCLID_COMUTILSLib::IVariantVectorPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI08318", ipSource != __nullptr);

		long lSize = ipSource->Size;
		for(long i = 0; i < lSize; i++)
		{
			_variant_t vtTemp = ipSource->GetItem(i);

			// Check for types which need an explicit deep copy
			VARTYPE varType = vtTemp.vt;
			if (varType == VT_UNKNOWN || varType == VT_DISPATCH)
			{
				UCLID_COMUTILSLib::ICopyableObjectPtr ipCopier(vtTemp.punkVal);
				ASSERT_RESOURCE_ALLOCATION("ELI26056", ipCopier != __nullptr);

				IUnknownPtr ipCopy = ipCopier->Clone();
				ASSERT_RESOURCE_ALLOCATION("ELI26057", ipCopy != __nullptr);

				// _variant_t::operator=(IUnknown* pSrc) does an AddRef() so don't Detach() the copy or its ref count will never drop to zero
				// https://extract.atlassian.net/browse/ISSUE-16444
				vtTemp = (IUnknown*)ipCopy;
			}
			
			m_vecVarCollection.push_back(vtTemp);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08321");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantVector::Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		// create a new variant vector
		UCLID_COMUTILSLib::ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI08377", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04443");
}

//-------------------------------------------------------------------------------------------------
// IShallowCopyable
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantVector::ShallowCopy(IUnknown** pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI26058", pObject != __nullptr);

		// validate license first
		validateLicense();

		UCLID_COMUTILSLib::IVariantVectorPtr ipNewVector(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI26059", ipNewVector != __nullptr);

		// Copy each item into the new vector (for reference types this will be a shallow copy)
		for (vector<_variant_t>::iterator it = m_vecVarCollection.begin();
			it != m_vecVarCollection.end(); it++)
		{
			ipNewVector->PushBack((*it));
		}

		// Return the new vector
		*pObject = ipNewVector.Detach();
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25736");
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantVector::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	try
	{
		validateLicense();
		// if validateLicense doesn't throw any exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantVector::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_VariantVector;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantVector::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantVector::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	INIT_EXCEPTION_AND_TRACING("MLI00003");

	try
	{
		validateLicense();

		// read the number of variants in the vector to the stream
		long nNumItems = 0;
		// clear the internal vector
		m_vecVarCollection.clear();

		// last code position tracing
		_lastCodePos = "10";

		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read(&nDataLength, sizeof(nDataLength), NULL);
		ByteStream data(nDataLength);
		pStream->Read(data.getData(), nDataLength, NULL);
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);
		_lastCodePos = "20";

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;
		_lastCodePos = "30";

		// Check for newer version
		if (nDataVersion > gnCurrentVersion)
		{
			// Throw exception
			UCLIDException ue( "ELI07670", "Unable to load newer VariantVector." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if (nDataVersion >= 1)
		{
			dataReader >> nNumItems;
		}
		_lastCodePos = "40";

		// read each of the variants to the stream
		for (int i = 0; i < nNumItems; i++)
		{
			// read the variant from the stream
			CComVariant vtTemp;
			HRESULT hr = vtTemp.ReadFromStream(pStream);
			if (FAILED(hr))
			{
				UCLIDException ue("ELI16912", "Failed reading from stream!");
				ue.addHresult(hr);
				throw ue;
			}

			// add to the vector of variants
			m_vecVarCollection.push_back(vtTemp);
		}
		_lastCodePos = "50";

		// set the dirty flag to false as we've just loaded the object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04569");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantVector::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	INIT_EXCEPTION_AND_TRACING("MLI00004");
	
	try
	{
		validateLicense();

		// last code position tracing
		_lastCodePos = "10";

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);
		dataWriter << gnCurrentVersion;
		_lastCodePos = "20";
		
		// write the number of variants in the vector to the stream
		long nNumItems = m_vecVarCollection.size();
		dataWriter << nNumItems;
		dataWriter.flushToByteStream();
		_lastCodePos = "30";

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);
		_lastCodePos = "40";

		// write each of the variants to the stream
		vector<_variant_t>::const_iterator iter;
		for (iter = m_vecVarCollection.begin(); iter != m_vecVarCollection.end(); iter++)
		{
			CComVariant vtTemp = *iter;
			HRESULT hr = vtTemp.WriteToStream(pStream);
			if (FAILED(hr))
			{
				UCLIDException ue("ELI16913", "Failed reading from stream!");
				ue.addHresult(hr);
				throw ue;
			}
		}
		_lastCodePos = "50";

		// clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04549");


	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CVariantVector::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
UCLID_COMUTILSLib::IVariantVectorPtr CVariantVector::getThisAsCOMPtr()
{
	UCLID_COMUTILSLib::IVariantVectorPtr ipThis(this);
	ASSERT_RESOURCE_ALLOCATION("ELI16975", ipThis != __nullptr);

	return ipThis;
}
//-------------------------------------------------------------------------------------------------
void CVariantVector::validateLicense()
{
	static const unsigned long VARVECTOR_COMPONENT_ID = gnEXTRACT_CORE_OBJECTS;

	VALIDATE_LICENSE( VARVECTOR_COMPONENT_ID, "ELI03873", "VariantVector" );
}
//-------------------------------------------------------------------------------------------------
void CVariantVector::validateType(const _variant_t& vtObject)
{
	VARTYPE varType = vtObject.vt;

	// Check that the variant type is not a pointer type or an array type (IUnknown and
	// IDispatch are supported but not pointers to IUnknown or IDispatch eg. VT_UNKNOWN | VT_BYREF)
	if ((varType & VT_BYREF) > 0 || varType == VT_SAFEARRAY || varType == VT_CARRAY)
	{
		UCLIDException ue("ELI26060", "IVariantVector does not support this variant type!");
		ue.addDebugInfo("VariantType", varType);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
