// OutputToVOA.cpp : Implementation of COutputToVOA
#include "stdafx.h"
#include "AFOutputHandlers.h"
#include "OutputToVOA.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <comutils.h>
#include <AFTagManager.h>
#include <ComponentLicenseIDs.h>
#include <TemporaryFileName.h>

#include <io.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// COutputToVOA
//-------------------------------------------------------------------------------------------------
COutputToVOA::COutputToVOA()
:m_bDirty(false)
{
	try
	{
		m_ipAFUtils.CreateInstance(CLSID_AFUtility);
		ASSERT_RESOURCE_ALLOCATION("ELI08881", m_ipAFUtils != __nullptr);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI08882")
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToVOA::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IOutputToVOA,
		&IID_IOutputHandler,
		&IID_IPersistStream,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_ILicensedComponent
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
		{
			return S_OK;
		}
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IOutputToVOA
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToVOA::get_FileName(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license and arguments
		validateLicense();
		ASSERT_ARGUMENT("ELI08853", pVal != __nullptr);

		// return the filename
		*pVal = _bstr_t(m_strFileName.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08854")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToVOA::put_FileName(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		string strFile = asString(newVal);

		// make sure the file exists
		// or that it contains valid string tags
		if (m_ipAFUtils->StringContainsInvalidTags(strFile.c_str()) == VARIANT_TRUE)
		{
			UCLIDException ue("ELI08855", "The rules file contains invalid tags!");
			ue.addDebugInfo("File", strFile);
			throw ue;
		}
		else if (m_ipAFUtils->StringContainsTags(strFile.c_str()) == VARIANT_FALSE)
		{
			// if no tags are defined, make sure the filename represents
			// an absolute path that can be written to
			if (!isAbsolutePath(strFile))
			{
				UCLIDException ue("ELI08856", "Specification of a relative path is not allowed!");
				ue.addDebugInfo("File", strFile);
				throw ue;
			}
			else if (!canCreateFile(strFile)) 
			{
				UCLIDException ue("ELI08857", "The specified file cannot be written to!");
				ue.addDebugInfo("File", strFile);
				throw ue;
			}
		}

		// store the filename
		m_strFileName = strFile;
		//m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08858")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IOutputHandler
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToVOA::raw_ProcessOutput(IIUnknownVector *pAttributes, IAFDocument *pAFDoc,
											 IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// expand ny tags in the filename
		AFTagManager tagMgr;
		string strFileName = tagMgr.expandTagsAndFunctions(m_strFileName, pAFDoc);

		// ensure  valid parameters
		ASSERT_ARGUMENT("ELI10488", pAttributes != __nullptr);

		// the output filename may be associated with a folder that does not
		// exist.  If so, try to create that folder
		string strFolder = getDirectoryFromFullPath(strFileName);
		if (!directoryExists(strFolder))
		{
			createDirectory(strFolder);
		}

		// Save the Attributes to the file
		pAttributes->SaveTo( _bstr_t(strFileName.c_str()), VARIANT_TRUE );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08869")
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToVOA::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToVOA::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19547", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Output data to VOA file").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08860")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToVOA::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		
		UCLID_AFOUTPUTHANDLERSLib::IOutputToVOAPtr ipSource = pObject;
		ASSERT_RESOURCE_ALLOCATION("ELI08861", ipSource != __nullptr);
		
		m_strFileName = asString(ipSource->GetFileName());
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12815");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToVOA::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		// create another instance of this object
		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_OutputToVOA);
		ASSERT_RESOURCE_ALLOCATION("ELI08862", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);
	
		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08863");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToVOA::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_OutputToVOA;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToVOA::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToVOA::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// reset member variables
		m_strFileName = "";

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
			UCLIDException ue("ELI08864", 
				"Unable to load newer OutputToVOA Output Handler!" );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if (nDataVersion >= 1)
		{
			// read the filename
			dataReader >> m_strFileName;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08865");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToVOA::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter << m_strFileName;
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08867");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToVOA::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToVOA::raw_IsConfigured(VARIANT_BOOL * pbValue)
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

		// This object is considered configured 
		//	if the filename is specified
		*pbValue = m_strFileName.empty() ? VARIANT_FALSE : VARIANT_TRUE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08880");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper Functions
//-------------------------------------------------------------------------------------------------
void COutputToVOA::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI08859", "OutputToVOA Output Handler" );
}
//-------------------------------------------------------------------------------------------------

