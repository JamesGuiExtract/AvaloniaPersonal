// DynamicFileListFS.cpp : Implementation of CDynamicFileListFS

#include "stdafx.h"
#include "DynamicFileListFS.h"
#include "ESFileSuppliers.h"
#include "FileSupplierUtils.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <CommentedTextFileReader.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//--------------------------------------------------------------------------------------------------
// CDynamicFileListFS
//--------------------------------------------------------------------------------------------------
CDynamicFileListFS::CDynamicFileListFS()
{
}
//--------------------------------------------------------------------------------------------------
CDynamicFileListFS::~CDynamicFileListFS()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16557");
}

//--------------------------------------------------------------------------------------------------
// IDynamicFileListFS
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDynamicFileListFS::get_FileName(BSTR *strFileName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*strFileName = _bstr_t(m_strFileName.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13949")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDynamicFileListFS::put_FileName(BSTR strFileName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		string strFile = asString(strFileName);

		// Create a local IFAMTagManagerPtr object
		UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr ipFAMTagManager;
		ipFAMTagManager.CreateInstance(CLSID_FAMTagManager);
		ASSERT_RESOURCE_ALLOCATION("ELI14437", ipFAMTagManager != __nullptr);

		// make sure the file name contains valid string tags
		if (ipFAMTagManager->StringContainsInvalidTags(strFile.c_str()) == VARIANT_TRUE)
		{

			UCLIDException ue("ELI14438", "The file name contains invalid tags!");
			ue.addDebugInfo("File", strFile);
			throw ue;
		}

		m_strFileName = strFile;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13950")

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
//  ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CDynamicFileListFS::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ILicensedComponent,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_IPersistStream,
		&IID_IMustBeConfiguredObject,
		&IID_IFileSupplier,
		&IID_IDynamicFileListFS
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
STDMETHODIMP CDynamicFileListFS::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (pbValue == NULL)
	{
		return E_POINTER;
	}

	try
	{
		// Validate license
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
STDMETHODIMP CDynamicFileListFS::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19633", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Files from dynamic list").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13951")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDynamicFileListFS::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		EXTRACT_FILESUPPLIERSLib::IDynamicFileListFSPtr ipCopyThis(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI13952", ipCopyThis != __nullptr);

		// Get the file name string
		m_strFileName = asString(ipCopyThis->FileName);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13953");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDynamicFileListFS::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		if(pObject != __nullptr)
		{
			// Validate license first
			validateLicense();

			ICopyableObjectPtr ipObjCopy;
			ipObjCopy.CreateInstance(CLSID_DynamicFileListFS);
			ASSERT_RESOURCE_ALLOCATION("ELI13954", ipObjCopy != __nullptr);

			IUnknownPtr ipUnk = this;
			ipObjCopy->CopyFrom(ipUnk);

			// Return the new object to the caller
			*pObject = ipObjCopy.Detach();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13955");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDynamicFileListFS::raw_IsConfigured(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		bool bConfigured = true;
		if (m_strFileName.empty())
		{
			bConfigured = false;
		}

		*pbValue = bConfigured ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13956");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDynamicFileListFS::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_DynamicFileListFS;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDynamicFileListFS::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDynamicFileListFS::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
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
			UCLIDException ue("ELI13957", "Unable to load newer DynamicFileListFileSupplier component.");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}
		
		// Read the file name string
		dataReader >> m_strFileName;
				
		// Clear the dirty flag since we loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13958");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDynamicFileListFS::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter << m_strFileName;

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13959");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDynamicFileListFS::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IFileSupplier
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDynamicFileListFS::raw_Start( IFileSupplierTarget *pTarget, IFAMTagManager *pFAMTM)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Wrap the file supplying target in a smart pointer
		IFileSupplierTargetPtr ipTarget(pTarget);
		ASSERT_RESOURCE_ALLOCATION("ELI13960", ipTarget != __nullptr );

		// reset events
		m_stopEvent.reset();
		m_pauseEvent.reset();
		m_supplyingDoneOrStoppedEvent.reset();
		m_resumeEvent.reset();

		// The string contain one file name
		_bstr_t bstrItem;

		try
		{
			try
			{
				// Call ExpandTagsAndTFE() to expand tags and functions
				// Pass an empty string as the second parameter because file supplier doesn't support <SourceDocName> tag 
				// and we also don't have a source doc name to expand it [P13: 3901]
				string strFileName = CFileSupplierUtils::ExpandTagsAndTFE(pFAMTM, m_strFileName, "");

				// Read each line of the input file
				ifstream ifs( strFileName.c_str() );
				CommentedTextFileReader fileReader( ifs, "//", true );
				if (!ifs.is_open())
				{
					// Can not open the file, will be considered as a 
					// file supplier failure
					UCLIDException ue("ELI13961", "Can not open the image list file for supplying.");
					ue.addDebugInfo ("File name", m_strFileName);
					throw ue;
				}

				// A flag to indicate if there are some files in the list or not
				bool bNoFilesInList = true;

				do
				{
					// Retrieve this line from the input file
					bstrItem = _bstr_t(fileReader.getLineText().c_str());

					// Send the item to the FileSupplierTarget
					if (bstrItem.length() != 0)
					{
						// Set the flag to false if there are files in the list
						if (bNoFilesInList)
						{
							bNoFilesInList = false;
						}

						// Add the file
						ipTarget->NotifyFileAdded(bstrItem, this);
					}

					// Check for stop event
					if (m_stopEvent.isSignaled())
					{
						break;
					}

					// Check for pause event
					if (m_pauseEvent.isSignaled())
					{
						m_resumeEvent.wait();
						m_pauseEvent.reset();
					}
				}
				while (!ifs.eof());

				// Check if we got here because we have reached the end of the list file
				// and call NotifyFileSupplyingDone if it is true
				if(ifs.eof())
				{
					//Signal the UI that we have supplied all our files.
					ipTarget->NotifyFileSupplyingDone(this);

					if (bNoFilesInList)
					{
						// The list contains no files, log the excpetion [P13: 3722]
						UCLIDException ue("ELI15726", "The dynamic list file is empty.");
						ue.addDebugInfo ("File name", m_strFileName);
						ue.log();
					}
				}
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI14277");
		}
		catch (UCLIDException ue)
		{
			// Notify the supplying target that the supplying failed
			ipTarget->NotifyFileSupplyingFailed(this, ue.asStringizedByteStream().c_str());
		}

		// Regardless of how we get here, we signal that we are done.
		m_supplyingDoneOrStoppedEvent.signal();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13962");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDynamicFileListFS::raw_Stop()
{
	try
	{
		// NOTE: Do not call validateLicense here because we want to be able 
		//   to gracefully stop processing even if the license state is corrupted.
		// validateLicense();

		// Signal that we are stopped.
		m_stopEvent.signal();

		// Wait for notification that the for loop in raw_start has kicked out
		m_supplyingDoneOrStoppedEvent.wait();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13963");
		
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDynamicFileListFS::raw_Pause()
{
	try
	{
		// Check license
		validateLicense();

		// Reset resume to gurantee that the loop will be waiting for the next
		// resume.
		m_resumeEvent.reset();
		m_pauseEvent.signal();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13964");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDynamicFileListFS::raw_Resume()
{
	try
	{
		// Check license
		validateLicense();

		//Allow the waiting for-loop to continue
		m_resumeEvent.signal();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13965");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CDynamicFileListFS::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI13966", "Dynamic List File Supplier");
}
//-------------------------------------------------------------------------------------------------
