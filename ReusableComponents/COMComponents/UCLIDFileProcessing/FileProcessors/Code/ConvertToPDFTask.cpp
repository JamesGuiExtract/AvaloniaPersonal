// ConvertToPDFTask.cpp : Implementation of CConvertToPDFTask

#include "stdafx.h"
#include "FileProcessors.h"
#include "ConvertToPDFTask.h"
#include "FileProcessorsUtils.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <ByteStream.h>
#include <ComponentLicenseIDs.h>
#include <LicenseMgmt.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 2;

// component description
const string gstrCONVERT_TO_PDF_COMPONENT_DESCRIPTION = "Convert to searchable PDF";

// default input image filename
// NOTE: this constant is duplicated in RedactionCCConstants.h of RedactionCustomComponents.
// In a future build, these constants should be consolidated into a registry key as per [P16 #2660].
const string gstrDEFAULT_INPUT_IMAGE_FILENAME = "$InsertBeforeExt(<SourceDocName>,.redacted)";

//-------------------------------------------------------------------------------------------------
// CConvertToPDFTask
//-------------------------------------------------------------------------------------------------
CConvertToPDFTask::CConvertToPDFTask()
  : m_strInputImage(gstrDEFAULT_INPUT_IMAGE_FILENAME)
{
	try
	{
		// construct the path to ESConvertToPDF relative to the common components directory
		m_strConvertToPDFEXE = getModuleDirectory(_Module.m_hInst);
		m_strConvertToPDFEXE += "\\ESConvertToPDF.exe";
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI18737");
}
//-------------------------------------------------------------------------------------------------
CConvertToPDFTask::~CConvertToPDFTask()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI18738");
}

//-------------------------------------------------------------------------------------------------
// IConvertToPDFTask
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConvertToPDFTask::SetOptions(BSTR bstrInputFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// validate license
		validateLicense();

		// store options
		m_strInputImage = asString(bstrInputFile);
		
		// set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18739");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConvertToPDFTask::GetOptions(BSTR *pbstrInputFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// validate license
		validateLicense();

		// ensure parameters are non-NULL
		ASSERT_ARGUMENT("ELI18740", pbstrInputFile != NULL);
		
		// set options
		*pbstrInputFile = _bstr_t(m_strInputImage.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18742");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConvertToPDFTask::raw_GetComponentDescription(BSTR* pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18743", pstrComponentDescription);
		
		*pstrComponentDescription = 
			_bstr_t(gstrCONVERT_TO_PDF_COMPONENT_DESCRIPTION.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18744");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConvertToPDFTask::raw_CopyFrom(IUnknown* pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// validate license first
		validateLicense();

		// get the ConvertToPDFTask interface
		UCLID_FILEPROCESSORSLib::IConvertToPDFTaskPtr ipConvertToPDFTask(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI18745", ipConvertToPDFTask != NULL);

		// get the options from the ConvertToPDFTask object 
		_bstr_t bstrInputFile;
		ipConvertToPDFTask->GetOptions( bstrInputFile.GetAddress() );
		
		// store the found options
		m_strInputImage = asString(bstrInputFile);

		// set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18746");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConvertToPDFTask::raw_Clone(IUnknown** ppObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// validate license first
		validateLicense();

		// ensure that the return value pointer is non-NULL
		ASSERT_ARGUMENT("ELI18747", ppObject != NULL);

		// get the copyable object interface
		ICopyableObjectPtr ipObjCopy(CLSID_ConvertToPDFTask);
		ASSERT_RESOURCE_ALLOCATION("ELI18748", ipObjCopy != NULL);

		// create a shallow copy
		IUnknownPtr ipUnknown(this);
		ASSERT_RESOURCE_ALLOCATION("ELI18749", ipUnknown != NULL);
		ipObjCopy->CopyFrom(ipUnknown);

		// return the new ConvertToPDFTask to the caller
		*ppObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18750");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IFileProcessingTask
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConvertToPDFTask::raw_Init()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// ensure the executable exists
		validateFileOrFolderExistence(m_strConvertToPDFEXE);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18751");
	
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CConvertToPDFTask::raw_ProcessFile(BSTR bstrFileFullName, long nFileID, long nActionID,
	IFAMTagManager *pTagManager, IFileProcessingDB *pDB, IProgressStatus *pProgressStatus,
	VARIANT_BOOL bCancelRequested, EFileProcessingResult *pResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		// check for NULL parameters
		ASSERT_ARGUMENT("ELI18752", bstrFileFullName != NULL);
		ASSERT_ARGUMENT("ELI18753", pTagManager != NULL);
		ASSERT_ARGUMENT("ELI18754", pResult != NULL);

		// get the source doc name
		string strSourceDoc = asString(bstrFileFullName);
		validateFileOrFolderExistence(strSourceDoc);

		// construct the full path to the input and output images
		string strInputImage = 
			CFileProcessorsUtils::ExpandTagsAndTFE(pTagManager, m_strInputImage, strSourceDoc);
		string strOutputImage = 
			getPathAndFileNameWithoutExtension(strInputImage) + ".pdf";

		// create the command line arguments, wrapping filenames in quotation marks
		string strArgs("\"");
		strArgs += strInputImage;
		strArgs += "\" \"";
		strArgs += strOutputImage;

		// ensure the remove original image option is specified
		strArgs += "\" /R";

		// execute the utility to convert the PDF
		runExtractEXE(m_strConvertToPDFEXE, strArgs, INFINITE);

		// completed successfully
		*pResult = kProcessingSuccessful;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18755")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConvertToPDFTask::raw_Cancel()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// nothing to do
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18756");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConvertToPDFTask::raw_Close()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// nothing to do
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18757");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConvertToPDFTask::raw_IsLicensed(VARIANT_BOOL* pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// ensure the return value pointer is non-NULL
		ASSERT_ARGUMENT("ELI18758", pbValue != NULL);

		try
		{
			// check license
			validateLicense();

			// if no exception was thrown, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18759");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConvertToPDFTask::raw_IsConfigured(VARIANT_BOOL* pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		// ensure the return value pointer is non-NULL
		ASSERT_ARGUMENT("ELI18760", pbValue != NULL);

		// the ConvertToPDFTask is configured if it has an input image filename
		*pbValue = asVariantBool( !m_strInputImage.empty() );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18761");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConvertToPDFTask::GetClassID(CLSID* pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18762", pClassID != NULL);

		*pClassID = CLSID_ConvertToPDFTask;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18763");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConvertToPDFTask::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
// Version 1:
//   use cleaned image if available, output image filename
// Version 2:
//   (removed) use cleaned image if available, output image filename
//   (added) input image filename
STDMETHODIMP CConvertToPDFTask::Load(IStream* pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// check license state
		validateLicense();

		// clear the options
		m_strInputImage = gstrDEFAULT_INPUT_IMAGE_FILENAME;
		
		// use a smart pointer for the IStream interface
		IStreamPtr ipStream(pStream);
		ASSERT_RESOURCE_ALLOCATION("ELI18764", ipStream != NULL);

		// read the bytestream data from the IStream object
		long nDataLength = 0;
		HANDLE_HRESULT(ipStream->Read(&nDataLength, sizeof(nDataLength), NULL), "ELI18765", 
			"Unable to read object size from stream.", ipStream, IID_IStream);
		ByteStream data(nDataLength);
		HANDLE_HRESULT(ipStream->Read(data.getData(), nDataLength, NULL), "ELI18766", 
			"Unable to read object from stream.", ipStream, IID_IStream);

		// read the data version
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// check if file is a newer version than this object can use
		if (nDataVersion > gnCurrentVersion)
		{
			// throw exception
			UCLIDException ue("ELI18767", "Unable to load newer ConvertToPDFTask.");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// read data items
		if(nDataVersion >= 2)
		{
			// read the input image filename
			dataReader >> m_strInputImage;
		}
		else
		{
			// read and ignore the cleaned image flag
			bool bTempUseCleanedImage;
			dataReader >> bTempUseCleanedImage;	

			// display warning message
			MessageBox(NULL, (gstrCONVERT_TO_PDF_COMPONENT_DESCRIPTION + 
				" no longer supports the \"use cleaned image\" option. This setting will be ignored.").c_str(),
				"Warning", MB_ICONWARNING);

			// set input image to its version 1 default
			m_strInputImage = "<SourceDocName>";

			// read and ignore the output pdf filename		
			string strTempOutputPDF;
			dataReader >> strTempOutputPDF;

			// display warning message
			MessageBox(NULL, (gstrCONVERT_TO_PDF_COMPONENT_DESCRIPTION + 
				" no longer supports the \"Output PDF filename\" option. This setting will be ignored.").c_str(),
				"Warning", MB_ICONWARNING);
		}
		
		// clear the dirty flag since a new object was loaded
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18768");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConvertToPDFTask::Save(IStream* pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// check license state
		validateLicense();

		// create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);
		dataWriter << gnCurrentVersion;
		dataWriter << m_strInputImage;
		dataWriter.flushToByteStream();

		// use a smart pointer for IStream interface
		IStreamPtr ipStream(pStream);
		ASSERT_RESOURCE_ALLOCATION("ELI18769", ipStream != NULL);

		// write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		HANDLE_HRESULT(ipStream->Write(&nDataLength, sizeof(nDataLength), NULL), "ELI18770", 
			"Unable to write object size to stream.", ipStream, IID_IStream);
		HANDLE_HRESULT(ipStream->Write(data.getData(), nDataLength, NULL), "ELI18771", 
			"Unable to write object to stream.", ipStream, IID_IStream);

		// clear the flag as specified
		if(fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18772");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConvertToPDFTask::GetSizeMax(ULARGE_INTEGER* pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConvertToPDFTask::InterfaceSupportsErrorInfo(REFIID riid)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		static const IID* arr[] = 
		{
			&IID_IConvertToPDFTask,
			&IID_ICategorizedComponent,
			&IID_ICopyableObject,
			&IID_IFileProcessingTask,
			&IID_ILicensedComponent,
			&IID_IMustBeConfiguredObject
		};

		for (int i = 0; i < sizeof(arr) / sizeof(arr[0]); i++)
		{
			if (InlineIsEqualGUID(*arr[i],riid))
			{
				return S_OK;
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18773");

	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CConvertToPDFTask::validateLicense()
{
	// ensure that creating searchable pdfs is licensed
	VALIDATE_LICENSE(gnCREATE_SEARCHABLE_PDF_FEATURE, "ELI18774", "ConvertToPDFTask");
}
//-------------------------------------------------------------------------------------------------