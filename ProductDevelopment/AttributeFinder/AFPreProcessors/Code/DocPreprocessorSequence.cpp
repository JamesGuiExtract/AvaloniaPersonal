// DocPreprocessorSequence.cpp : Implementation of CDocPreprocessorSequence
#include "stdafx.h"
#include "AFPreProcessors.h"
#include "DocPreprocessorSequence.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>
#include <cpputil.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CDocPreprocessorSequence
//-------------------------------------------------------------------------------------------------
CDocPreprocessorSequence::CDocPreprocessorSequence()
: m_bDirty(false)
{
}
//-------------------------------------------------------------------------------------------------
CDocPreprocessorSequence::~CDocPreprocessorSequence()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16324");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocPreprocessorSequence::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IDocumentPreprocessor,
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
// IDocumentPreprocessor
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocPreprocessorSequence::raw_Process(IAFDocument* pDocument, IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{	
		validateLicense();

		// if no document preprocessors exist, then throw an exception
		if (m_ipDocPreprocessors == __nullptr || m_ipDocPreprocessors->Size() == 0)
		{
			throw UCLIDException("ELI08147", "No document preprocessors specified!");
		}

		// use a smart pointer for the progress status object
		IProgressStatusPtr ipProgressStatus = pProgressStatus;

		// weighted value of progress items per individual preprocessor
		const long lPROGRESS_ITEMS_PER_PREPROCESSOR = 1;

		// check if the caller wants progress status updates
		if (ipProgressStatus)
		{
			// count the number of enabled preprocessors using a MiscUtils object
			IMiscUtilsPtr ipMiscUtils(CLSID_MiscUtils);
			ASSERT_RESOURCE_ALLOCATION("ELI16262", ipMiscUtils != __nullptr);
			long lEnabledPreprocessors = ipMiscUtils->CountEnabledObjectsIn(m_ipDocPreprocessors);

			// leave without processing if there are no preprocessors to process
			if (lEnabledPreprocessors == 0)
			{
				return S_OK;
			}

			// intialize the progress status object
			ipProgressStatus->InitProgressStatus("Processing multiple preprocessors", 
				0, lEnabledPreprocessors * lPROGRESS_ITEMS_PER_PREPROCESSOR, VARIANT_TRUE);
		}

		// iterate through each document preprocessor in the vector
		long nNumItems = m_ipDocPreprocessors->Size();
		for (int i = 0; i < nNumItems; i++)
		{
			// get the DocPreprocessor object-with-description 
			// at the current position
			IObjectWithDescriptionPtr ipObj = m_ipDocPreprocessors->At(i);;
			ASSERT_RESOURCE_ALLOCATION("ELI08448", ipObj != __nullptr);

			// Check state and use this Preprocessor only if Enabled
			if ( asCppBool(ipObj->Enabled) )
			{
				// update the progress status if it was requested
				if (ipProgressStatus)
				{
					ipProgressStatus->StartNextItemGroup(get_bstr_t(
						"Executing preprocessor " + asString(i+1) + " of " + asString(nNumItems) ),
						lPROGRESS_ITEMS_PER_PREPROCESSOR);
				}

				// get the DocPreprocessor at the current position
				IDocumentPreprocessorPtr ipDP = ipObj->Object;
				ASSERT_RESOURCE_ALLOCATION("ELI08449", ipDP != __nullptr);

				// execute the document preprocessor & pass in the 
				// sub progress object if progress status updates were requested
				ipDP->Process(pDocument, 
					(ipProgressStatus ? ipProgressStatus->SubProgressStatus : NULL) );
			}
		}

		// complete the last group if the progress status exists
		if (ipProgressStatus)
		{
			ipProgressStatus->CompleteCurrentItemGroup();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08149")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMultipleObjectHolder
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocPreprocessorSequence::raw_GetObjectCategoryName(BSTR *pstrCategoryName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*pstrCategoryName = get_bstr_t(AFAPI_DOCUMENT_PREPROCESSORS_CATEGORYNAME).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08150")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocPreprocessorSequence::raw_GetObjectType(BSTR *pstrObjectType)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*pstrObjectType = get_bstr_t("Document Preprocessor").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08151")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocPreprocessorSequence::get_ObjectsVector(IIUnknownVector* *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		// if the document preprocessors vector has not yet been initialized, do so
		if (m_ipDocPreprocessors == __nullptr)
		{
			m_ipDocPreprocessors.CreateInstance(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI08154", m_ipDocPreprocessors != __nullptr);
		}

		// return a reference to the internal vector
		IIUnknownVectorPtr ipShallowCopy = m_ipDocPreprocessors;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08152")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocPreprocessorSequence::put_ObjectsVector(IIUnknownVector *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		// convert argument to smart pointer
		IIUnknownVectorPtr ipDocPreprocessors(newVal);
		ASSERT_RESOURCE_ALLOCATION("ELI08155", ipDocPreprocessors != __nullptr);

		// ensure that each of the items in the vector is a document preprocessor
		long nNumItems = ipDocPreprocessors->Size();
		for (int i = 0; i < nNumItems; i++)
		{
			IObjectWithDescriptionPtr ipObj = ipDocPreprocessors->At(i);
			if (ipObj == __nullptr)
			{
				UCLIDException ue("ELI08156", "Invalid object in vector - expecting vector of IObjectWithDescription objects!");
				ue.addDebugInfo("index", i);
				throw ue;
			}

			IDocumentPreprocessorPtr ipDP = ipObj->Object;
			if (ipDP == __nullptr)
			{
				UCLIDException ue("ELI08157", "Invalid object in ObjectWithDescription - expecting IDocumentPreprocessor object!");
				ue.addDebugInfo("index", i);
				throw ue;
			}
		}

		// update internal variable
		m_ipDocPreprocessors = ipDocPreprocessors;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08153")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocPreprocessorSequence::raw_GetRequiredIID(IID *riid)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		*riid = IID_IDocumentPreprocessor;

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09642")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocPreprocessorSequence::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_DocPreprocessorSequence;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocPreprocessorSequence::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocPreprocessorSequence::Load(IStream *pStream)
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
			UCLIDException ue( "ELI08158", 
				"Unable to load newer DocPreprocessor Sequence!" );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		// read the vector of document preprocessors for version >= 2
		if (nDataVersion >= 1)
		{
			IPersistStreamPtr ipObj;
			::readObjectFromStream(ipObj, pStream, "ELI09961");
			m_ipDocPreprocessors = ipObj;
			ASSERT_RESOURCE_ALLOCATION("ELI08159", m_ipDocPreprocessors != __nullptr);
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08160");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocPreprocessorSequence::Save(IStream *pStream, BOOL fClearDirty)
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

		// write the vector of document preprocessors to the stream
		IPersistStreamPtr ipObj = m_ipDocPreprocessors;
		if (ipObj == __nullptr)
		{
			throw UCLIDException("ELI08161", "IIUnknownVector component does not support persistence!");
		}
		writeObjectToStream(ipObj, pStream, "ELI09916", fClearDirty);

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08162");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocPreprocessorSequence::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocPreprocessorSequence::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		IMultipleObjectHolderPtr ipObj = pObject;
		ASSERT_RESOURCE_ALLOCATION("ELI08163", ipObj != __nullptr);

		// get the vector of doc preprocessors from the other object
		IIUnknownVectorPtr ipDocPreprocessors = ipObj->ObjectsVector;
		ASSERT_RESOURCE_ALLOCATION("ELI08429", ipDocPreprocessors != __nullptr);

		// get a copy of the vector of doc preprocessors
		m_ipDocPreprocessors = ipDocPreprocessors->Clone();
		ASSERT_RESOURCE_ALLOCATION("ELI08430", m_ipDocPreprocessors != __nullptr);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08431")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocPreprocessorSequence::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		// create another instance of this object
		UCLID_COMUTILSLib::ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_DocPreprocessorSequence);
		ASSERT_RESOURCE_ALLOCATION("ELI08427", ipObjCopy != __nullptr);

		// ask the other object to copy itself from this object
		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08428");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocPreprocessorSequence::raw_IsConfigured(VARIANT_BOOL * pbValue)
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
		// there is at least one object in the vector
		*pbValue = asVariantBool(
			m_ipDocPreprocessors != __nullptr && m_ipDocPreprocessors->Size() != 0 );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08169");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocPreprocessorSequence::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19559", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Select multiple document preprocessors").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08170")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocPreprocessorSequence::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
void CDocPreprocessorSequence::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI08171", 
		"Document Preprocessor Sequence" );
}
//-------------------------------------------------------------------------------------------------
