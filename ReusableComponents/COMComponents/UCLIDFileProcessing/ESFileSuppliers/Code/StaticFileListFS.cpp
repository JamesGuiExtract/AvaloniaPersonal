// StaticFileListFS.cpp : Implementation of CStaticFileListFS

#include "stdafx.h"
#include "StaticFileListFS.h"
#include "ESFileSuppliers.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <cpputil.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//--------------------------------------------------------------------------------------------------
// CStaticFileListFS
//--------------------------------------------------------------------------------------------------
CStaticFileListFS::CStaticFileListFS()
{
}
//--------------------------------------------------------------------------------------------------
CStaticFileListFS::~CStaticFileListFS()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16559");
}

//--------------------------------------------------------------------------------------------------
// IStaticFileListFS
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStaticFileListFS::get_FileList(IVariantVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		IVariantVectorPtr ipFiles(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI13596", ipFiles != __nullptr);

		for (unsigned int n = 0; n < m_vecFileList.size(); n++)
		{
			_bstr_t _bstrFile = get_bstr_t(m_vecFileList[n].c_str());
			ipFiles->PushBack(_bstrFile);
		}

		*pVal = ipFiles.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13597")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStaticFileListFS::put_FileList(IVariantVector *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		IVariantVectorPtr ipFiles(newVal);
		ASSERT_ARGUMENT("ELI13598", ipFiles != __nullptr);

		m_vecFileList.clear();

		long nSize = ipFiles->Size;
		for (long n=0; n<nSize; n++)
		{
			string strFile = asString(_bstr_t(ipFiles->GetItem(n)));
			m_vecFileList.push_back(strFile);
		}
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13599")

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
//  ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CStaticFileListFS::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ILicensedComponent,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_IPersistStream,
		&IID_IMustBeConfiguredObject,
		&IID_IFileSupplier,
		&IID_IStaticFileListFS
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CStaticFileListFS::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (pbValue == NULL)
		return E_POINTER;

	try
	{
		// validate license
		validateLicense();
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// ICategorizedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CStaticFileListFS::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19635", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Files from static list").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13587")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStaticFileListFS::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		EXTRACT_FILESUPPLIERSLib::IStaticFileListFSPtr ipCopyThis(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI13588", ipCopyThis != __nullptr);

		IVariantVectorPtr ipList = ipCopyThis->FileList;
		ASSERT_RESOURCE_ALLOCATION("ELI15643", ipList != __nullptr);
		long lSize = ipList->GetSize();

		_variant_t varType;
		_bstr_t bstrType;
		string strTemp;

		// clear the vector before we copying into it
		m_vecFileList.clear();

		for(int i=0; i < lSize; i++)
		{
			varType = ipList->GetItem(i);
			bstrType = varType.bstrVal;
			strTemp = bstrType;

			m_vecFileList.push_back(strTemp);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13589");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStaticFileListFS::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())


	try
	{
		if(pObject != __nullptr)
		{
			// Validate license first
			validateLicense();

			ICopyableObjectPtr ipObjCopy;
			ipObjCopy.CreateInstance(CLSID_StaticFileListFS);
			ASSERT_RESOURCE_ALLOCATION("ELI13601", ipObjCopy != __nullptr);

			IUnknownPtr ipUnk = this;
			ipObjCopy->CopyFrom(ipUnk);

			// Return the new object to the caller
			*pObject = ipObjCopy.Detach();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13602");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStaticFileListFS::raw_IsConfigured(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		bool bConfigured = true;
		if (m_vecFileList.size() == 0)
		{
			bConfigured = false;
		}
		*pbValue = bConfigured ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13590");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStaticFileListFS::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_StaticFileListFS;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStaticFileListFS::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStaticFileListFS::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// reset member variables
		clear();

		// Read the bytestream data from the IStream object
		long lDataLength = 0;
		pStream->Read(&lDataLength, sizeof(lDataLength), NULL);
		ByteStream data(lDataLength);
		pStream->Read(data.getData(), lDataLength, NULL);
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// Check for newer version
		if (nDataVersion > gnCurrentVersion)
		{
			// Throw exception
			UCLIDException ue("ELI13608", "Unable to load newer StaticFileListFileSupplier component.");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// Read how many strings are in the file
		long lcount = 0;
		dataReader >> lcount;
		
		// Read the strings themselves
		std::string strTmp;
		for(int i = 0; i < lcount; i++)
		{
			dataReader >> strTmp;
			m_vecFileList.push_back( strTmp );
		}
		
		// Clear the dirty flag since we loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13609");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStaticFileListFS::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter << (long)m_vecFileList.size();
		for(unsigned int i = 0; i < m_vecFileList.size(); i++)
		{
			dataWriter << m_vecFileList[i];
		}

		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long lDataLength = data.getLength();
		pStream->Write(&lDataLength, sizeof(lDataLength), NULL);
		pStream->Write(data.getData(), lDataLength, NULL);

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13610");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStaticFileListFS::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IFileSupplier
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStaticFileListFS::raw_Start( IFileSupplierTarget *pTarget, IFAMTagManager *pFAMTM )
{
	try
	{
		if(pTarget == NULL)
		{
			UCLIDException ue( "ELI13727", "NULL FileSupplierTarget." );
			throw ue;
		}

		// Check license
		validateLicense();

		// reset events
		m_stopEvent.reset();
		m_pauseEvent.reset();
		m_supplyingDoneOrStoppedEvent.reset();
		m_resumeEvent.reset();

		std::string strItem;
		_bstr_t bstrItem;

		try
		{
			try
			{
				// start supplying files
				unsigned long ulSize = static_cast<unsigned long>(m_vecFileList.size());
				for (unsigned long n = 0; n < ulSize; n++)
				{
					//get an item
					strItem = (m_vecFileList[n]);
					bstrItem = strItem.c_str();

					//send the item to the FileSupplierTarget
					pTarget->NotifyFileAdded(bstrItem, this);

					// check for stop event
					if (m_stopEvent.isSignaled())
					{
						break;
					}

					// check for pause event
					if (m_pauseEvent.isSignaled())
					{
						m_resumeEvent.wait();
						m_pauseEvent.reset();
					}
					if(n+1 == ulSize)
					{
						//Signal the UI that we have supplied all our files.
						pTarget->NotifyFileSupplyingDone(this);
					}
				}
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI14126");
		}
		catch ( UCLIDException ue)
		{
			// Notify the supplying target that the supplying failed
			pTarget->NotifyFileSupplyingFailed(this, ue.asStringizedByteStream().c_str());
		}

		//regardless of how we get here, we signal that we are done.
		m_supplyingDoneOrStoppedEvent.signal();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13717");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStaticFileListFS::raw_Stop()
{
	try
	{
		// NOTE: Do not call validateLicense here because we want to be able 
		//   to gracefully stop processing even if the license state is corrupted.
		// validateLicense();

		//Signal that we are stopped.
		m_stopEvent.signal();

		//Wait for notification that the for loop in raw_start has kicked out
		m_supplyingDoneOrStoppedEvent.wait();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13718");
		
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStaticFileListFS::raw_Pause()
{
	try
	{
		// Check license
		validateLicense();

		//reset resume to gurantee that the loop will be waiting for the next
		//resume.
		m_resumeEvent.reset();
		m_pauseEvent.signal();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13719");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStaticFileListFS::raw_Resume()
{
	try
	{
		// Check license
		validateLicense();

		//Allow the waiting for-loop to continue
		m_resumeEvent.signal();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13720");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CStaticFileListFS::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI13600", "Static List File Supplier");
}
//-------------------------------------------------------------------------------------------------
void CStaticFileListFS::clear()
{
	m_vecFileList.clear();
}
//-------------------------------------------------------------------------------------------------
