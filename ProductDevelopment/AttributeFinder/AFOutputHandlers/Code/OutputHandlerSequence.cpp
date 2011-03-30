// OutputHandlerSequence.cpp : Implementation of COutputHandlerSequence
#include "stdafx.h"
#include "AFOutputHandlers.h"
#include "OutputHandlerSequence.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>
#include <cpputil.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 2;

//-------------------------------------------------------------------------------------------------
// COutputHandlerSequence
//-------------------------------------------------------------------------------------------------
COutputHandlerSequence::COutputHandlerSequence()
: m_bDirty(false)
{
}
//-------------------------------------------------------------------------------------------------
COutputHandlerSequence::~COutputHandlerSequence()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16314");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputHandlerSequence::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IOutputHandler,
		&IID_ILicensedComponent,
		&IID_ICategorizedComponent,
		&IID_ISpecifyPropertyPages,
		&IID_ICopyableObject,
		&IID_IMustBeConfiguredObject,
		&IID_IPersistStream,
		&IID_IMultipleObjectHolder
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
STDMETHODIMP COutputHandlerSequence::raw_ProcessOutput(IIUnknownVector* pAttributes,
													   IAFDocument *pAFDoc, 
													   IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{	
		validateLicense();

		// if no output handlers exist, then throw an exception
		if (m_ipOutputHandlers == __nullptr || m_ipOutputHandlers->Size() == 0)
		{
			throw UCLIDException("ELI08146", "No output handlers specified!");
		}

		IIUnknownVectorPtr ipReturnAttributes(pAttributes);
		ASSERT_RESOURCE_ALLOCATION("ELI08095", ipReturnAttributes != __nullptr);

		// use a smart pointer for the progress status object
		IProgressStatusPtr ipProgressStatus = pProgressStatus;

		// weighted value of progress items per individual output handler
		const long lPROGRESS_ITEMS_PER_OUTPUT_HANDLER = 1;

		// check if the caller wants progress updates
		if (ipProgressStatus)
		{
			// count the number of enabled output handlers using a MiscUtils object
			IMiscUtilsPtr ipMiscUtils(CLSID_MiscUtils);
			ASSERT_RESOURCE_ALLOCATION("ELI16263", ipMiscUtils != __nullptr);
			long lEnabledOutputHandlers = ipMiscUtils->CountEnabledObjectsIn(m_ipOutputHandlers);

			// leave without processing if there are no output handlers to process
			if (lEnabledOutputHandlers == 0)
			{
				return S_OK;
			}
			
			// initialize progress status
			ipProgressStatus->InitProgressStatus("Processing multiple output handlers", 0, 
				lEnabledOutputHandlers * lPROGRESS_ITEMS_PER_OUTPUT_HANDLER, VARIANT_TRUE);
		}

		// iterate through each output handler in the vector
		long nNumItems = m_ipOutputHandlers->Size();
		for (int i = 0; i < nNumItems; i++)
		{
			// get the outputhandler object-with-description 
			// at the current position
			IObjectWithDescriptionPtr ipObj = m_ipOutputHandlers->At(i);;
			ASSERT_RESOURCE_ALLOCATION("ELI08447", ipObj != __nullptr);

			// Check state and use this Output Handler only if Enabled
			if ( asCppBool(ipObj->Enabled) )
			{
				// update the progress status if it was requested
				if (ipProgressStatus)
				{
					ipProgressStatus->StartNextItemGroup( get_bstr_t(
						"Executing output handler " + asString(i+1) + " of " + asString(nNumItems) ),
						lPROGRESS_ITEMS_PER_OUTPUT_HANDLER);
				}

				// get the output handler inside the object-with-description
				IOutputHandlerPtr ipOutputHandler = ipObj->Object;
				ASSERT_RESOURCE_ALLOCATION("ELI08101", ipOutputHandler != __nullptr);

				// process this output handler, passing in the 
				// subprogress object if it is available
				ipOutputHandler->ProcessOutput(ipReturnAttributes, pAFDoc, 
					(ipProgressStatus ? ipProgressStatus->SubProgressStatus : NULL) );
				ASSERT_RESOURCE_ALLOCATION("ELI08096", ipReturnAttributes != __nullptr);
			}
		}

		// complete the last group if the progress status exists
		if (ipProgressStatus)
		{
			ipProgressStatus->CompleteCurrentItemGroup();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05067")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMultipleObjectHolder
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputHandlerSequence::raw_GetObjectCategoryName(BSTR *pstrCategoryName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*pstrCategoryName = get_bstr_t(AFAPI_OUTPUT_HANDLERS_CATEGORYNAME).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08103")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputHandlerSequence::raw_GetObjectType(BSTR *pstrObjectType)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*pstrObjectType = get_bstr_t("Output Handler").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08118")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputHandlerSequence::get_ObjectsVector(IIUnknownVector* *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		// if the output handlers vector has not yet been initialized, do so
		if (m_ipOutputHandlers == __nullptr)
		{
			m_ipOutputHandlers.CreateInstance(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI08441", m_ipOutputHandlers != __nullptr);
		}

		// return a reference to the internal vector
		IIUnknownVectorPtr ipShallowCopy = m_ipOutputHandlers;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08104")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputHandlerSequence::put_ObjectsVector(IIUnknownVector *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		// convert argument to smart pointer
		IIUnknownVectorPtr ipOutputHandlers(newVal);
		ASSERT_RESOURCE_ALLOCATION("ELI08442", ipOutputHandlers != __nullptr);

		// ensure that each of the items in the vector is an object with description
		// where the object is an output handler
		long nNumItems = ipOutputHandlers->Size();
		for (int i = 0; i < nNumItems; i++)
		{
			IObjectWithDescriptionPtr ipObj = ipOutputHandlers->At(i);
			if (ipObj == __nullptr)
			{
				UCLIDException ue("ELI08124", "Invalid object in vector - expecting vector of IObjectWithDescription objects!");
				ue.addDebugInfo("index", i);
				throw ue;
			}

			IOutputHandlerPtr ipOutputHandler = ipObj->Object;
			if (ipOutputHandler == __nullptr)
			{
				UCLIDException ue("ELI08091", "Invalid object in ObjectWithDescription - expecting IOutputHandler object!");
				ue.addDebugInfo("index", i);
				throw ue;
			}
		}

		// update internal variable
		m_ipOutputHandlers = ipOutputHandlers;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08105")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputHandlerSequence::raw_GetRequiredIID(IID *riid)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		*riid = IID_IOutputHandler;

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09641")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputHandlerSequence::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_OutputHandlerSequence;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputHandlerSequence::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputHandlerSequence::Load(IStream *pStream)
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
			UCLIDException ue( "ELI07768", 
				"Unable to load newer OutputHandler Sequence!" );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		// read the vector of output handlers for version >= 2
		if (nDataVersion >= 2)
		{
			IPersistStreamPtr ipObj;
			::readObjectFromStream(ipObj, pStream, "ELI09958");
			m_ipOutputHandlers = ipObj;
			ASSERT_RESOURCE_ALLOCATION("ELI08128", m_ipOutputHandlers != __nullptr);
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07769");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputHandlerSequence::Save(IStream *pStream, BOOL fClearDirty)
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

		// write the vector of output handlers to the stream
		IPersistStreamPtr ipObj = m_ipOutputHandlers;
		if (ipObj == __nullptr)
		{
			throw UCLIDException("ELI08127", "IIUnknownVector component does not support persistence!");
		}
		writeObjectToStream(ipObj, pStream, "ELI09913", fClearDirty);

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07770");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputHandlerSequence::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputHandlerSequence::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		IMultipleObjectHolderPtr ipObj = pObject;
		ASSERT_RESOURCE_ALLOCATION("ELI08083", ipObj != __nullptr);

		// get the vector of output handlers from the other object
		IIUnknownVectorPtr ipOutputHandlers = ipObj->ObjectsVector;
		ASSERT_RESOURCE_ALLOCATION("ELI08445", ipOutputHandlers != __nullptr);

		// copy the vector of output handlers
		m_ipOutputHandlers = ipOutputHandlers->Clone();
		ASSERT_RESOURCE_ALLOCATION("ELI08446", m_ipOutputHandlers != __nullptr);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08406")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputHandlerSequence::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		// create another instance of this object
		UCLID_COMUTILSLib::ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_OutputHandlerSequence);
		ASSERT_RESOURCE_ALLOCATION("ELI08084", ipObjCopy != __nullptr);

		// ask the other object to copy itself from this object
		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08085");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputHandlerSequence::raw_IsConfigured(VARIANT_BOOL * pbValue)
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

		// This object is considered configured if
		// there's at least one object in the vector
		*pbValue = asVariantBool(m_ipOutputHandlers != __nullptr && m_ipOutputHandlers->Size() != 0);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08081");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputHandlerSequence::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19546", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Select multiple output handlers").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08082")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputHandlerSequence::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// Helper functions
//-------------------------------------------------------------------------------------------------
void COutputHandlerSequence::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI05069", "Output Handler Sequence" );
}
//-------------------------------------------------------------------------------------------------
