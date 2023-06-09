// IUnknownVector.cpp : Implementation of CIUnknownVector
#include "stdafx.h"
#include "UCLIDCOMUtils.h"
#include "IUnknownVector.h"
#include "COMUtilsMethods.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <COMUtils.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>
#include <ComponentLicenseIDs.h>

#include <algorithm>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

const string gstrIUNKOWNVECTOR_FILE_SIGNATURE = "UCLID IUnknownVector File";

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IIUnknownVector,
		&IID_IPersistStream,
		&IID_ICopyableObject,
		&IID_IShallowCopyable,
		&IID_ILicensedComponent,
		&IID_IComparableObject,
		&IID_IManageableMemory,
		&IID_ICloneIdentifiableObject
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// CIUnknownVector
//-------------------------------------------------------------------------------------------------
CIUnknownVector::CIUnknownVector()
: m_bDirty(false)
, m_bstrStreamName("IUnknownVector")
, m_ipMemoryManager(__nullptr)
{
}
//-------------------------------------------------------------------------------------------------
CIUnknownVector::~CIUnknownVector()
{
	try
	{
		m_vecIUnknowns.clear();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16509");
}
//-------------------------------------------------------------------------------------------------
void CIUnknownVector::FinalRelease()
{
	try
	{
		m_vecIUnknowns.clear();

		// If memory usage has been reported, report that this instance is no longer using any
		// memory.
		RELEASE_MEMORY_MANAGER(m_ipMemoryManager, "ELI36086");
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI31145");
}

//-------------------------------------------------------------------------------------------------
// IIUnknownVector
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::Size(long *plSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26018", plSize != __nullptr);

		validateLicense();

		// return the size of the vector
		*plSize = m_vecIUnknowns.size();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01673");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::At(long lPos, IUnknown **pObj)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26019", pObj != __nullptr);

		validateLicense();

		if (m_vecIUnknowns.empty())
		{
			throw UCLIDException("ELI01514", "The IUnknown vector is empty!");
		}

		// validate index
		validateIndex(lPos);

		// find the object at the specified index, increment the reference, and return it.
		IUnknownPtr ipShallowCopy = m_vecIUnknowns[lPos];
		*pObj = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01674");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::At2(long lPos, IDispatch **pObj)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_vecIUnknowns.empty())
		{
			throw UCLIDException("ELI08975", "The IUnknown vector is empty in CIUnknownVector::At2()!");
		}

		// validate index
		validateIndex(lPos);

		// find the object at the specified index, increment the reference, and return it.
		IDispatchPtr ipDispatch = m_vecIUnknowns[lPos];
		if (ipDispatch != __nullptr)
		{
			IDispatchPtr ipShallowCopy = ipDispatch;
			*pObj = ipShallowCopy.Detach();
		}
		else
		{
			*pObj = NULL;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08976");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::PushBack(IUnknown *pObj)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		ASSERT_ARGUMENT("ELI26020", pObj != __nullptr);

		validateLicense();

		// push_back the IUnknown to the vector
		m_vecIUnknowns.push_back(pObj);
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01675");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::PushBackIfNotContained(IUnknown *pObj)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();
		
		size_t nSize = m_vecIUnknowns.size();
		for(size_t i = 0; i < nSize; i++)
		{
			// if the object is already in the vector return (don't add it)
			if(m_vecIUnknowns[i] == pObj)
			{
				return S_OK;
			}
		}

		// push_back the IUnknown to the vector
		m_vecIUnknowns.push_back(pObj);
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09522");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::Clear()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		clear();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01676");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::Front(IUnknown **pObj)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	try
	{
		ASSERT_ARGUMENT("ELI26021", pObj != __nullptr);

		validateLicense();

		if (!m_vecIUnknowns.empty())
		{
			// find the object, increment the reference, and return it.
			IUnknownPtr ipShallowCopy = m_vecIUnknowns.front();
			*pObj = ipShallowCopy.Detach();
		}
		else
		{
			throw UCLIDException("ELI01512", "The IUnknown vector is empty!");
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01677");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::Back(IUnknown **pObj)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	try
	{
		ASSERT_ARGUMENT("ELI26022", pObj != __nullptr);

		validateLicense();

		if (!m_vecIUnknowns.empty())
		{
			// find the object, increment the reference, and return it.
			IUnknownPtr ipShallowCopy = m_vecIUnknowns.back();
			*pObj = ipShallowCopy.Detach();
		}
		else
		{
			throw UCLIDException("ELI01513", "The IUnknown vector is empty!");
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI01678");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::PopBack()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		m_vecIUnknowns.pop_back();
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03327");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::Remove(long nIndex)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// validate index
		validateIndex(nIndex);

		// remove the item at nIndex
		m_vecIUnknowns.erase(m_vecIUnknowns.begin() + nIndex);
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03329");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::Append(IIUnknownVector *pVector)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_COMUTILSLib::IIUnknownVectorPtr ipVector(pVector);
		ASSERT_ARGUMENT("ELI26023", ipVector != __nullptr);

		validateLicense();

		// Append the vector to this vector
		append(ipVector);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04380");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::Insert(long lPos, IUnknown *pObj)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Check new object
		if (pObj == NULL)
		{
			// Create and throw exception
			UCLIDException	ue( "ELI04568", "Cannot insert NULL object!");
			throw ue;
		}

		// Get current size and check index
		long iCount = m_vecIUnknowns.size();

		// Is the new item being added at the end of the vector
		if (lPos == iCount)
		{
			// Append to the end
			m_vecIUnknowns.push_back( pObj );
		}
		else
		{
			// validate index
			validateIndex(lPos);

			// Create iterator and locate index
			vector<IUnknownPtr>::iterator iter = m_vecIUnknowns.begin() + lPos;

			// Insert the item
			m_vecIUnknowns.insert( iter, pObj );
		}// end else new item is not just being appended

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04566");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::InsertVector(long lPos, IIUnknownVector *pObj)
{
	try
	{
		UCLID_COMUTILSLib::IIUnknownVectorPtr ipVector(pObj);
		ASSERT_ARGUMENT("ELI26024", ipVector != __nullptr);

		// Check license state
		validateLicense();

		// Get current size and check index
		int iCount = m_vecIUnknowns.size();
		if (lPos == iCount)
		{
			// caller wants to actually do an append operation
			append(ipVector); // Dirty flag is set in append method
		}
		else
		{
			// caller wants to do an insert, validate index
			validateIndex(lPos);

			// insert the vector, item by item, at the correct position
			long nNumItemsToInsert = ipVector->Size();

			// find the iterator that points to the position we need to insert at
			// guaranteed that lPos is valid index because of call to validateIndex
			vector<IUnknownPtr>::iterator iter = m_vecIUnknowns.begin() + lPos;

			for (long i = 0; i < nNumItemsToInsert; i++)
			{
				// get the object to insert
				IUnknownPtr ipUnknown = ipVector->At(i);
				ASSERT_RESOURCE_ALLOCATION("ELI06455", ipUnknown != __nullptr);

				// insert the object at the correct position
				iter = m_vecIUnknowns.insert(iter, ipUnknown);

				// increment the iterator for next insertion
				iter++;
			}

			// Set the dirty flag
			m_bDirty = true;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06452");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::RemoveRange(long nStart, long nEnd)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing state
		validateLicense();

		// validate indexes
		validateIndex(nStart);
		validateIndex(nEnd);
		if (nEnd < nStart)
		{
			UCLIDException ue("ELI28784", "Invalid index range!");
			ue.addDebugInfo("start", nStart);
			ue.addDebugInfo("end", nEnd);
			throw ue;
		}

		vector<IUnknownPtr>::iterator iterStart, iterEnd;

		// get the iterStart iterator to the correct location
		iterStart = m_vecIUnknowns.begin() + nStart;

		// get the iterEnd iterator to the correct location
		// [LegacyRCAndUtils:5569] Add one since stl doesn't include the end iterator in the
		// deletions.
		iterEnd = m_vecIUnknowns.begin() + nEnd + 1;

		// erase the entries in the specified range
		m_vecIUnknowns.erase(iterStart, iterEnd);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06464");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::RemoveValue(IUnknown *pObj)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing state
		validateLicense();

		vector<IUnknownPtr>::iterator iter;
		vector<vector<IUnknownPtr>::iterator> vecEraseIter;

		// get iterators pointing to all matches in the vector
		for (iter = m_vecIUnknowns.begin(); iter != m_vecIUnknowns.end(); iter++)
		{
			if (pObj == *iter)
			{
				vecEraseIter.push_back(iter);
			}
		}

		// erase all matches in the vector to pObj
		// The vecEraseIter vector should be traversed in reverse order so the
		// iterators it contains are valid after items are erased from the m_vecIUnknowns vector
		vector<vector<IUnknownPtr>::iterator>::reverse_iterator iter2;
		for (iter2 = vecEraseIter.rbegin(); iter2 != vecEraseIter.rend(); ++iter2)
		{
			m_vecIUnknowns.erase(*iter2);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07971");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::Set(long lPos, IUnknown *pObj)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing state
		validateLicense();

		// Validate the index
		validateIndex(lPos);

		// Cast object as smart pointer
		IUnknownPtr ipObj(pObj);

		// Check new object
		if (ipObj == __nullptr)
		{
			// Create and throw exception
			UCLIDException	ue( "ELI04765", "Cannot insert NULL object!" );
			throw ue;
		}

		// Insert the new object
		m_vecIUnknowns[lPos] = ipObj;

		// Set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04762");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::Swap(long lPos1, long lPos2)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing state
		validateLicense();

		// Validate the indices
		validateIndex(lPos1);
		validateIndex(lPos2);

		IUnknownPtr ipTemp1 = m_vecIUnknowns[lPos1];
		ASSERT_RESOURCE_ALLOCATION("ELI26025", ipTemp1 != __nullptr);
		IUnknownPtr ipTemp2 = m_vecIUnknowns[lPos2];
		ASSERT_RESOURCE_ALLOCATION("ELI26026", ipTemp2 != __nullptr);

		m_vecIUnknowns[lPos1] = ipTemp2;
		m_vecIUnknowns[lPos2] = ipTemp1;

		// Set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04763");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::IsOrderFreeEqualTo(IIUnknownVector *pVector, VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI06308", pVector != __nullptr);

		// default the value to false
		*pbValue = VARIANT_FALSE;

		UCLID_COMUTILSLib::IIUnknownVectorPtr ipOtherVec(pVector);
		if (ipOtherVec == __nullptr)
		{
			// if pVector is not of type IIUnknownVector, then return false
			return S_OK;
		}

		long nThisVecSize = m_vecIUnknowns.size();
		long nOtherVecSize = ipOtherVec->Size();
		if (nThisVecSize == 0 && nOtherVecSize == 0)
		{
			// if they are both empty, then return true
			*pbValue = VARIANT_TRUE;
			return S_OK;
		}

		// first compare the size of two vectors
		if (nThisVecSize != nOtherVecSize)
		{
			// if size is different, return immediately
			return S_OK;
		}

		// make a copy of these two vectors
		vector<IUnknownPtr> vecCopyThisVec = m_vecIUnknowns;
		UCLID_COMUTILSLib::IShallowCopyablePtr ipCopier = ipOtherVec;
		ASSERT_RESOURCE_ALLOCATION("ELI26027", ipCopier != __nullptr);
		UCLID_COMUTILSLib::IIUnknownVectorPtr ipCopyOtherVec = ipCopier->ShallowCopy();
		ASSERT_RESOURCE_ALLOCATION("ELI26028", ipCopyOtherVec != __nullptr);
		
		long nTotalSize = vecCopyThisVec.size();

		// look at each item and find its equal object(s)
		while (nTotalSize > 0)
		{
			// get the first item of the other vector
			UCLID_COMUTILSLib::IComparableObjectPtr ipOtherObj(ipCopyOtherVec->At(0));
			if (ipOtherObj == __nullptr)
			{
				throw UCLIDException("ELI06310", "Object in IUnknownVector must implement IComparableObject in order to make the comparison.");
			}
				
			// ...and look for the equal object from the "other" vector
			for (long n = 0; n < nTotalSize; n++)
			{
				UCLID_COMUTILSLib::IComparableObjectPtr ipThisObj(vecCopyThisVec[n]);
				if (ipThisObj == __nullptr)
				{
					throw UCLIDException("ELI06309", "Object in IUnknownVector must implement IComparableObject in order to make the comparison.");
				}

				// found one match
				if (ipThisObj->IsEqualTo(ipOtherObj) == VARIANT_TRUE)
				{
					// remove the first object from the "other" vector
					ipCopyOtherVec->Remove(0);
					
					// remove the equal object from the "this" vector
					vecCopyThisVec.erase(vecCopyThisVec.begin() + n);
					nTotalSize = vecCopyThisVec.size();

					// break out of the inner loop
					break;
				}
			}

			// if new size of the vector is the same as before, then
			// it means there's no equal object found, then return false;
			int nNewSize = vecCopyThisVec.size();
			if (nNewSize == nTotalSize)
			{
				return S_OK;
			}

			// reset the total size
			nTotalSize = nNewSize;
		}

		// once this point is reached, it means that 
		// these two vectors have the same objects
		*pbValue = VARIANT_TRUE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06307");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::FindByValue(IUnknown *pObj, long nStartIndex, long *plIndex)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Check start index
		validateIndex(nStartIndex);

		// Default to not found
		long lFoundIndex = -1;
		*plIndex = lFoundIndex;

		// Test object must implement IComparableObject to be found
		UCLID_COMUTILSLib::IComparableObjectPtr ipTestObj( pObj );
		if (ipTestObj == __nullptr)
		{
			throw UCLIDException( "ELI06426", 
				"Object must implement IComparableObject in order to be found." );
		}

		// Step through vector looking for the specified object
		long lSize = m_vecIUnknowns.size();
		for (long i = nStartIndex; i < lSize; i++)
		{
			// Retrieve this item
			UCLID_COMUTILSLib::IComparableObjectPtr ipMyObj( m_vecIUnknowns[i] );
			if (ipMyObj == __nullptr)
			{
				throw UCLIDException( "ELI06415", 
					"Object in IUnknownVector must implement IComparableObject in order to be found." );
			}

			// Compare the objects
			if (ipMyObj->IsEqualTo(ipTestObj) == VARIANT_TRUE)
			{
				// Equal, store the index and return
				*plIndex = i;

				return S_OK;
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06414");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::FindByReference(IUnknown *pObj, long nStartPos, long *pRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Default to not found
		long nFoundIndex = -1;
		*pRetVal = nFoundIndex;

		// If there are no items in the vector just return
		if (m_vecIUnknowns.size() == 0)
		{
			return S_OK;
		}
			
		// Check start index
		validateIndex(nStartPos);

		// Step through vector looking for the specified object
		for (unsigned int i = nStartPos; i < m_vecIUnknowns.size(); i++)
		{
			// Compare the objects
			if (m_vecIUnknowns[i] == pObj)
			{
				// Equal, store the index and return
				*pRetVal = i;

				return S_OK;
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09523");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::LoadFrom(BSTR strFullFileName, VARIANT_BOOL bSetDirtyFlagToTrue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();
		
		UCLID_COMUTILSLib::IIUnknownVectorPtr ipThis = getThisAsCOMPtr();

		// Load the vector from the file
		IPersistStreamPtr ipPersistStream = ipThis;
		ASSERT_RESOURCE_ALLOCATION("ELI16908", ipPersistStream != __nullptr);
		readObjectFromFile(ipPersistStream, strFullFileName, m_bstrStreamName, false, 
			gstrIUNKOWNVECTOR_FILE_SIGNATURE);

		// Mark this object as dirty depending upon bSetDirtyFlagToTrue
		m_bDirty = (bSetDirtyFlagToTrue == VARIANT_TRUE);

		// Wait for the file to be accessible
		waitForFileAccess(asString(strFullFileName), giMODE_READ_ONLY);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06969");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP 
CIUnknownVector::PrepareForStorage(BSTR bstrStorageManager, IIUnknownVector** ppClonedVector)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_RESOURCE_ALLOCATION("ELI38815", SysStringLen(bstrStorageManager) != 0);

		GUID guidStorageManager;
		IIDFromString(bstrStorageManager, &guidStorageManager);
	
		UCLID_COMUTILSLib::IStorageManagerPtr ipStorageManager(guidStorageManager);
		ASSERT_RESOURCE_ALLOCATION("ELI36346", ipStorageManager != __nullptr);
		
		// We don't want the preparation to have side-effects on the data, so clone the vector
		// first.
		UCLID_COMUTILSLib::ICloneIdentifiableObjectPtr ipCopySource(getThisAsCOMPtr());
		ASSERT_RESOURCE_ALLOCATION("ELI36347", ipCopySource != __nullptr);
		UCLID_COMUTILSLib::IIUnknownVectorPtr ipClone = ipCopySource->CloneIdentifiableObject();
		ASSERT_RESOURCE_ALLOCATION("ELI36348", ipClone != __nullptr);
	
		ipStorageManager->PrepareForStorage(ipClone);
	
		// After preparing the cloned data for storage, include the storage manager itself in
		// the vector to be persisted if it implements IPersistStream. This is so that it can
		// be used to perform special initialization on the data as it is loaded back from disk.
		IPersistStreamPtr ipPersistStream(ipStorageManager);
		if (ipPersistStream != __nullptr)
		{
			ipClone->PushBack(ipStorageManager);
		}
	
		*ppClonedVector = (IIUnknownVector*)ipClone.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38814");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::SaveTo(BSTR strFullFileName, VARIANT_BOOL bClearDirty,
									 BSTR bstrStorageManager)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		// If a storage manager was specified, use it to prepare the data before persisting it.
		if (SysStringLen(bstrStorageManager) != 0)
		{
			UCLID_COMUTILSLib::IIUnknownVectorPtr ipClone;
			ipClone = getThisAsCOMPtr()->PrepareForStorage(bstrStorageManager);

			// Write this prepared data to the file
			writeObjectToFile(ipClone, strFullFileName, m_bstrStreamName, asCppBool(bClearDirty), 
				gstrIUNKOWNVECTOR_FILE_SIGNATURE);
		}
		else
		{
			// Write this object to the file
			writeObjectToFile(this, strFullFileName, m_bstrStreamName, asCppBool(bClearDirty), 
				gstrIUNKOWNVECTOR_FILE_SIGNATURE);
		}

		// Mark this object as dirty depending upon bClearDirty
		if (bClearDirty == VARIANT_TRUE)
		{
			m_bDirty = false;
		}

		// Wait until the file is readable
		waitForStgFileAccess(strFullFileName);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06972");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::Sort(ISortCompare* pSortCompare)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		vector<IUnknownPtr> tmpVec = m_vecIUnknowns;

		SortCompare sort;
		sort.m_ipSortCompare = pSortCompare;
		std::sort(m_vecIUnknowns.begin(), m_vecIUnknowns.end(), sort);

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11269");

}
//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		// Ensure that the object is a Vector
		UCLID_COMUTILSLib::IIUnknownVectorPtr ipSource = pObject;
		ASSERT_RESOURCE_ALLOCATION("ELI08207", ipSource != __nullptr);
	
		copyFrom(ipSource, false);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08209");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		// create a new variant vector
		UCLID_COMUTILSLib::ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI19454", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04449");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IShallowCopyable
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::ShallowCopy(IUnknown** pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI25983", pObject != __nullptr);

		// Create a new vector to hold the objects
		UCLID_COMUTILSLib::IIUnknownVectorPtr ipNewVector(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI25982", ipNewVector != __nullptr);

		// Place a shallow copy of each object in the new vector
		for (vector<IUnknownPtr>::iterator it = m_vecIUnknowns.begin();
			it != m_vecIUnknowns.end(); it++)
		{
			ipNewVector->PushBack((*it));
		}

		// Return the new vector
		*pObject = ipNewVector.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25725");
}

//-------------------------------------------------------------------------------------------------
// IManageableMemory
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::ReportMemoryUsage()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		if (m_ipMemoryManager == __nullptr)
		{
			m_ipMemoryManager.CreateInstance(MEMORY_MANAGER_CLASS);
		}
		
		long nSize = m_vecIUnknowns.size();
		m_ipMemoryManager->ReportUnmanagedMemoryUsage(sizeof(*this) + nSize);

		for (long i = 0; i < nSize; i++)
		{
			if (m_vecIUnknowns[i] != __nullptr)
			{
				UCLID_COMUTILSLib::IManageableMemoryPtr ipManageableMemory = m_vecIUnknowns[i];
				ASSERT_RESOURCE_ALLOCATION("ELI36020", ipManageableMemory != __nullptr);
			
				ipManageableMemory->ReportMemoryUsage();
			}
		}		

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36021");
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_IUnknownVector;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		HRESULT hr = m_bDirty ? S_OK : S_FALSE;
		
		if (!m_bDirty)
		{
			// iterate through all objects
			vector<IUnknownPtr>::iterator itVec = m_vecIUnknowns.begin();
			for (; itVec != m_vecIUnknowns.end(); itVec++)
			{
				IPersistStreamPtr ipPersistStream(*itVec);
				if (ipPersistStream==NULL)
				{
					throw UCLIDException("ELI04783", "Object does not support persistence!");
				}
				
				hr = ipPersistStream->IsDirty();
				if (hr == S_OK)
				{
					// if object is dirty, break out of the loop
					break;
				}
			}
		}

		return hr;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04778")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// Clear the variables first
		long nNumItems = 0;
		// clear the internal vector
		m_vecIUnknowns.clear();

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
			UCLIDException ue( "ELI07665", "Unable to load newer IUnknownVector." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if (nDataVersion >= 1)
		{
			dataReader >> nNumItems;
		}
		
		// read each of the objects to the stream
		for (int i = 0; i < nNumItems; i++)
		{
			IPersistStreamPtr ipObj;
			readObjectFromStream(ipObj, pStream, "ELI09979");
			if (ipObj == __nullptr)
			{
				throw UCLIDException("ELI04630", "Unable to read object from stream!");
			}
			else
			{			
				// add to the vector of objects
				m_vecIUnknowns.push_back(ipObj);
			}
		}

		// If there was at least one item loaded, attempt to cast the last item in the vector as an
		// IStorageManager. If the last item is an IStorageManager instance, use it to prepare the
		// just loaded data.
		if (m_vecIUnknowns.size() > 0)
		{
			UCLID_COMUTILSLib::IStorageManagerPtr ipStorageManager = m_vecIUnknowns.back();
			if (ipStorageManager != __nullptr)
			{
				// If the last instance was an IStorageManager, it is only to be used to
				// initialize the data and should not remain as part of the loaded data.
				m_vecIUnknowns.pop_back();

				ipStorageManager->InitFromStorage(getThisAsCOMPtr());
			}
		}

		// set the dirty flag to false as we've just loaded the object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04570");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);
		dataWriter << gnCurrentVersion;
		// write the number of objects in the vector to the stream
		long nNumItems = m_vecIUnknowns.size();
		dataWriter << nNumItems;
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);
		
		// write each of the objects to the stream
		vector<IUnknownPtr>::const_iterator iter;
		for (iter = m_vecIUnknowns.begin(); iter != m_vecIUnknowns.end(); iter++)
		{
			// make sure the object supports persistence
			IPersistStreamPtr ipObj = *iter;
			if (ipObj == __nullptr)
			{
				throw UCLIDException("ELI04572", "Object in vector does not support persistence!");
			}
			else
			{
				// object supports persistence -> write it to stream
				writeObjectToStream(ipObj, pStream, "ELI09934", fClearDirty);
			}
		}
		
		// clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04571");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// IComparableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::IsEqualTo(IUnknown * pObj, VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		ASSERT_ARGUMENT("ELI06311", pObj != __nullptr);

		// default to false
		*pbValue = VARIANT_FALSE;

		// Comparison object must be an IIUnknownVector
		UCLID_COMUTILSLib::IIUnknownVectorPtr ipOtherVector = pObj;
		if (ipOtherVector == __nullptr)
		{
			// if pObj is not of type IIUnknownVector, return false
			return S_OK;
		}

		// Compare number of elements in vectors
		long nOtherVecSize = ipOtherVector->Size();
		long nThisVecSize = m_vecIUnknowns.size();
		if (nThisVecSize == 0 && nOtherVecSize == 0)
		{
			// if they are both empty, then return true
			*pbValue = VARIANT_TRUE;
			return S_OK;
		}

		if (nOtherVecSize != nThisVecSize)
		{
			// if size doesn't match return false;
			return S_OK;
		}
		
		/////////////////////////////////////////
		// Check all elements
		/////////////////////////////////////////
		for (long n = 0; n < nThisVecSize; n++)
		{
			// Retrieve Ith elements
			UCLID_COMUTILSLib::IComparableObjectPtr ipThisObj(m_vecIUnknowns.at(n));
			UCLID_COMUTILSLib::IComparableObjectPtr ipOtherObj(ipOtherVector->At(n));
			
			// Make sure that both elements were retrieved
			if ((ipThisObj == __nullptr) || (ipOtherObj == __nullptr))
			{
				throw UCLIDException("ELI06312", "Object in IUnknownVector must implement IComparableObject in order to make the comparison.");
			}
						
			// Compare the objects
			if (ipThisObj->IsEqualTo(ipOtherObj) == VARIANT_FALSE)
			{
				// not equal, return false;
				return S_OK;
			}
		}		

		// if this point is reached, it means that the vectors are equal
		*pbValue = VARIANT_TRUE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05589");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICloneIdentifiableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::CloneIdentifiableObject(IUnknown ** pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		UCLID_COMUTILSLib::ICloneIdentifiableObjectPtr ipCloneIdentifiable;
		ipCloneIdentifiable.CreateInstance(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI38479", ipCloneIdentifiable != __nullptr);

		IUnknownPtr ipUnk = this;
		ipCloneIdentifiable->CopyFromIdentifiableObject(ipUnk);

		*pObject = ipCloneIdentifiable.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38478");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIUnknownVector::CopyFromIdentifiableObject(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		copyFrom(pObject, true);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38480");
}

//-------------------------------------------------------------------------------------------------
// Helper function
//-------------------------------------------------------------------------------------------------
UCLID_COMUTILSLib::IIUnknownVectorPtr CIUnknownVector::getThisAsCOMPtr()
{
	UCLID_COMUTILSLib::IIUnknownVectorPtr ipThis(this);
	ASSERT_RESOURCE_ALLOCATION("ELI16972", ipThis != __nullptr);

	return ipThis;
}
//-------------------------------------------------------------------------------------------------
void CIUnknownVector::validateLicense()
{
	static const unsigned long IUNKNOWNVECTOR_COMPONENT_ID = gnEXTRACT_CORE_OBJECTS;

	VALIDATE_LICENSE( IUNKNOWNVECTOR_COMPONENT_ID, "ELI02606", "IUnknownVector" );
}
//-------------------------------------------------------------------------------------------------
void CIUnknownVector::validateIndex(long nIndex)
{
	// throw an exception if nIndex is not a valid index for 
	// the m_vecIUnknowns vector
	if (nIndex < 0 || (unsigned long) nIndex >= m_vecIUnknowns.size())
	{
		UCLIDException ue("ELI06465", "Invalid index!");
		ue.addDebugInfo("nIndex", nIndex);
		ue.addDebugInfo("Vector size", m_vecIUnknowns.size());
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
bool CIUnknownVector::SortCompare::operator()(IUnknownPtr& rpObj1, IUnknownPtr& rpObj2)
{
	return m_ipSortCompare->LessThan(rpObj1, rpObj2) == VARIANT_TRUE;
}
//-------------------------------------------------------------------------------------------------
void CIUnknownVector::append(UCLID_COMUTILSLib::IIUnknownVectorPtr ipVector)
{
	try
	{
		// Check the argument
		ASSERT_ARGUMENT("ELI26029", ipVector != __nullptr);

		// Add each item from the vector to this vector
		long nNumItems = ipVector->Size();
		for (long i = 0; i < nNumItems; i++)
		{
			IUnknownPtr ipUnknown = ipVector->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI16938", ipUnknown != __nullptr)

			m_vecIUnknowns.push_back(ipUnknown);
		}

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26030");
}
//-------------------------------------------------------------------------------------------------
void CIUnknownVector::clear()
{
	try
	{
		// clear the vector of IUnknowns
		m_vecIUnknowns.clear();
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26061");
}
//-------------------------------------------------------------------------------------------------
void CIUnknownVector::copyFrom(UCLID_COMUTILSLib::IIUnknownVectorPtr ipSource, bool bWithCloneIdentifiableObject)
{
	// Ensure that the object is a Vector
	ASSERT_ARGUMENT("ELI40287", ipSource != __nullptr);
	
	// Clear this vector
	clear();

	long lSize = ipSource->Size();
	for (long i = 0; i < lSize; i++)
	{
		IUnknownPtr ipUnknownClone = cloneObject("ELI38484", ipSource->At(i), bWithCloneIdentifiableObject);

		m_vecIUnknowns.push_back(ipUnknownClone);
	}
}
