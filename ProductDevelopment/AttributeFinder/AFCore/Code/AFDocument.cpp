// AFDocument.cpp : Implementation of CAFDocument
#include "stdafx.h"
#include "AFCore.h"
#include "AFDocument.h"

#include <UCLIDException.h>
#include <ComUtils.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CAFDocument
//-------------------------------------------------------------------------------------------------
CAFDocument::CAFDocument()
: m_ipAttribute(__nullptr),
  m_ipStringTags(__nullptr),
  m_ipObjectTags(__nullptr),
  m_nVersionNumber(gnCurrentVersion),
  m_ipRSDFileStack(__nullptr),
  m_strFKBVersion(""),
  m_strAlternateComponentDataDir(""),
  m_eParallelRunMode(kUnspecifiedParallelization),
  m_ipOCRParameters(__nullptr)
{
}
//-------------------------------------------------------------------------------------------------
CAFDocument::~CAFDocument()
{
	try
	{
		m_ipAttribute = __nullptr;
		m_ipStringTags = __nullptr;
		m_ipObjectTags = __nullptr;
		m_ipRSDFileStack = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16298");
}
//-------------------------------------------------------------------------------------------------
HRESULT CAFDocument::FinalConstruct()
{
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
void CAFDocument::FinalRelease()
{
	try
	{
		m_ipAttribute = __nullptr;
		m_ipStringTags = __nullptr;
		m_ipObjectTags = __nullptr;
		m_ipRSDFileStack = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI36206");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IAFDocument,
		&IID_ILicensedComponent,
		&IID_ICopyableObject,
		&IID_IPersistStream,
		&IID_IHasOCRParameters
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IAFDocument
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::get_Text(ISpatialString **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ISpatialStringPtr ipText = getAttribute()->Value;
		ASSERT_RESOURCE_ALLOCATION("ELI34817", ipText != __nullptr);

		ISpatialStringPtr ipShallowCopy = ipText;
		*pVal = ipShallowCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05845");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::put_Text(ISpatialString *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		getAttribute()->Value = newVal;
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05846");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::get_StringTags(IStrToStrMap **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_ipStringTags == __nullptr)
		{
			m_ipStringTags.CreateInstance(CLSID_StrToStrMap);
			ASSERT_RESOURCE_ALLOCATION("ELI05871", m_ipStringTags != __nullptr);
		}

		IStrToStrMapPtr ipShallowCopy = m_ipStringTags;
		*pVal = ipShallowCopy.Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05847");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::put_StringTags(IStrToStrMap *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_ipStringTags = newVal;
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05848");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::get_ObjectTags(IStrToObjectMap **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_ipObjectTags == __nullptr)
		{
			m_ipObjectTags.CreateInstance(CLSID_StrToObjectMap);
			ASSERT_RESOURCE_ALLOCATION("ELI05872", m_ipObjectTags != __nullptr);
		}

		IStrToObjectMapPtr ipShallowCopy = m_ipObjectTags;
		*pVal = ipShallowCopy.Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05849");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::put_ObjectTags(IStrToObjectMap *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_ipObjectTags = newVal;
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05850");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::get_Attribute(IAttribute **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		UCLID_AFCORELib::IAttributePtr ipShallowCopy = getAttribute();
		*pVal = (IAttribute*)ipShallowCopy.Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI34803");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::put_Attribute(IAttribute *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_ipAttribute = newVal;
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI34804");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::get_RSDFileStack(IVariantVector* *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		IVariantVectorPtr ipShallowCopy(m_ipRSDFileStack);
		*pVal = (IVariantVector*)ipShallowCopy.Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41953");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::put_RSDFileStack(IVariantVector *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_ipRSDFileStack = newVal;
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41957");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::IsRSDFileExecuting(BSTR bstrFileName, VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*pbValue = asVariantBool(m_ipRSDFileStack->Contains(bstrFileName));

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41954")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::PartialClone(VARIANT_BOOL vbCloneAttributes, VARIANT_BOOL vbCloneText,
									   IAFDocument **pAFDoc)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_RESOURCE_ALLOCATION("ELI36254", pAFDoc != __nullptr);

		// Create a new IAFDocument object
		UCLID_AFCORELib::IAFDocumentPtr ipDocCopy(CLSID_AFDocument);
		ASSERT_RESOURCE_ALLOCATION("ELI36255", ipDocCopy != __nullptr);
	
		if (m_ipAttribute != __nullptr)
		{
			bool bCloneAttributes = asCppBool(vbCloneAttributes);
			bool bCloneText = asCppBool(vbCloneText);

			// Copy references to document and attribute hierarchy
			ipDocCopy->Attribute = UCLID_AFCORELib::IAttributePtr(CLSID_Attribute);
			ipDocCopy->Attribute->Value = m_ipAttribute->Value;
			ipDocCopy->Attribute->SubAttributes = m_ipAttribute->SubAttributes;

			// Clone the attribute hierarchy beneath the top-level document attribute, but not the
			// document text.
			if (bCloneAttributes)
			{
				ICopyableObjectPtr ipCopyObj(m_ipAttribute->SubAttributes);
				ASSERT_RESOURCE_ALLOCATION("ELI36256", ipCopyObj != __nullptr);
				ipDocCopy->Attribute->SubAttributes = IIUnknownVectorPtr(ipCopyObj->Clone());
			}
			// Clone the document text but not the attribute hierarchy beneath the top-level
			// document attribute.
			if (bCloneText)
			{
				ICopyableObjectPtr ipCopyObj(m_ipAttribute->Value);
				ASSERT_RESOURCE_ALLOCATION("ELI36257", ipCopyObj != __nullptr);
				ipDocCopy->Attribute->Value = ISpatialStringPtr(ipCopyObj->Clone());
			}
		}
		if (m_ipStringTags != __nullptr)
		{
			ICopyableObjectPtr ipCopyObj(m_ipStringTags);
			ASSERT_RESOURCE_ALLOCATION("ELI36258", ipCopyObj != __nullptr);
			ipDocCopy->StringTags = IStrToStrMapPtr(ipCopyObj->Clone());
		}
		if (m_ipObjectTags != __nullptr)
		{
			ICopyableObjectPtr ipCopyObj(m_ipObjectTags);
			ASSERT_RESOURCE_ALLOCATION("ELI36259", ipCopyObj != __nullptr);
			ipDocCopy->ObjectTags = IStrToObjectMapPtr(ipCopyObj->Clone());
		}
		if (m_ipRSDFileStack != __nullptr)
		{
			ICopyableObjectPtr ipCopyObj(m_ipRSDFileStack);
			ASSERT_RESOURCE_ALLOCATION("ELI41956", ipCopyObj != __nullptr);
			ipDocCopy->RSDFileStack = IVariantVectorPtr(ipCopyObj->Clone());
		}

		ipDocCopy->ParallelRunMode = (UCLID_AFCORELib::EParallelRunMode)m_eParallelRunMode;
		ipDocCopy->FKBVersion = get_bstr_t(m_strFKBVersion);
		ipDocCopy->AlternateComponentDataDir = get_bstr_t(m_strAlternateComponentDataDir);


		// Shallow copy OCR parameters because they are not expected to be
		// modified during rule execution
		if (m_ipOCRParameters != __nullptr)
		{
			IHasOCRParametersPtr ipOCRParams(ipDocCopy);
			ipOCRParams->OCRParameters = m_ipOCRParameters;
		}

		// Return the new object to the caller
		*pAFDoc = (IAFDocument *)ipDocCopy.Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36260");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::PushRSDFileName(BSTR strFileName, long *pnStackSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate arguments
		ASSERT_ARGUMENT("ELI41941", pnStackSize != __nullptr);

		// store the specified file as associated with
		// the currently executing thread
		string strFile = asString(strFileName);

		// if strFile is defined, make sure it is an absolute path
		if (!strFile.empty())
		{
			// create a dummy file in the current directory
			string strCurrentDirFile = getCurrentDirectory() + "\\dummy.dat";
			// if strFile has no path before call after call it will have the current Directory as path
			strFile = getAbsoluteFileName(strCurrentDirFile, strFile);
		}

		if (m_ipRSDFileStack == __nullptr)
		{
			m_ipRSDFileStack.CreateInstance(CLSID_VariantVector);
		}

		// push the RSD file on the stack
		m_ipRSDFileStack->PushBack(_bstr_t(strFile.c_str()));

		long nStackSize = m_ipRSDFileStack->Size;

		// return the stack size
		*pnStackSize = nStackSize;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41942")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::PopRSDFileName(long *pnStackSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate arguments
		ASSERT_ARGUMENT("ELI41943", pnStackSize != __nullptr);

		// pop the stack
		m_ipRSDFileStack->Remove(m_ipRSDFileStack->Size - 1, 1);

		// return the stack size
		*pnStackSize = m_ipRSDFileStack->Size;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41944")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::get_FKBVersion(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI41945", pVal != __nullptr);

		*pVal = _bstr_t(m_strFKBVersion.c_str()).Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41946")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::put_FKBVersion(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		string newFKBVersion = asString(newVal);

		// If this is a nested ruleset, ensure it is not a different FKB version than that which
		// has already been specified.
		if (m_ipRSDFileStack != __nullptr &&
			m_ipRSDFileStack->Size > 1 &&
			!m_strFKBVersion.empty() &&
			 _strcmpi(newFKBVersion.c_str(), m_strFKBVersion.c_str()) != 0)
		{
			UCLIDException ue("ELI41947", "Conflicting FKB version numbers!");
			ue.addDebugInfo("Original version", m_strFKBVersion);
			ue.addDebugInfo("Conflicting version", newFKBVersion);
			throw ue;
		}

		m_strFKBVersion = newFKBVersion;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41948")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::get_AlternateComponentDataDir(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI41949", pVal != __nullptr);

		*pVal = _bstr_t(m_strAlternateComponentDataDir.c_str()).Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41950")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::put_AlternateComponentDataDir(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_strAlternateComponentDataDir = asString(newVal);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41951")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::GetCurrentRSDFileDir(BSTR *pstrRSDFileDir)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		if (m_ipRSDFileStack == __nullptr || m_ipRSDFileStack->Size == 0)
		{
			*pstrRSDFileDir = _bstr_t("").Detach();

			return S_OK;
		}

		string strRSDFile = _bstr_t(m_ipRSDFileStack->Item[m_ipRSDFileStack->Size - 1]);

		// the entry was found - return the directory associated
		// with the corresponding RSD file
		string strRSDFileDir = getDirectoryFromFullPath(strRSDFile);
		*pstrRSDFileDir = get_bstr_t(strRSDFileDir).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI49955")

}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::get_ParallelRunMode(EParallelRunMode *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		*pVal = m_eParallelRunMode;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI42039")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::put_ParallelRunMode(EParallelRunMode newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		if (m_eParallelRunMode != newVal)
		{
			ASSERT_RUNTIME_CONDITION("ELI42062",
				newVal >= kUnspecifiedParallelization
				&& newVal <= kGreedyParallelization,
				"ParallelRunMode value out of range");
			m_eParallelRunMode = newVal;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI42040")
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::raw_IsLicensed(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
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
STDMETHODIMP CAFDocument::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		UCLID_AFCORELib::IAFDocumentPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI08201", ipSource != __nullptr);
	
		UCLID_AFCORELib::IAttributePtr ipAttribute = ipSource->Attribute;
		if (ipAttribute)
		{
			ICopyableObjectPtr ipCopyObj(ipAttribute);
			ASSERT_RESOURCE_ALLOCATION("ELI34806", ipCopyObj != __nullptr);
			m_ipAttribute = ipCopyObj->Clone();
		}
		IStrToStrMapPtr ipSTS = ipSource->GetStringTags();
		if (ipSTS)
		{
			ICopyableObjectPtr ipCopyObj(ipSTS);
			ASSERT_RESOURCE_ALLOCATION("ELI08327", ipCopyObj != __nullptr);
			m_ipStringTags = ipCopyObj->Clone();
		}
		IStrToObjectMapPtr ipSTOM = ipSource->GetObjectTags();
		if (ipSTOM)
		{
			ICopyableObjectPtr ipCopyObj(ipSTOM);
			ASSERT_RESOURCE_ALLOCATION("ELI08328", ipCopyObj != __nullptr);
			m_ipObjectTags = ipCopyObj->Clone();
		}
		m_strFKBVersion = ipSource->FKBVersion;
		m_strAlternateComponentDataDir = ipSource->AlternateComponentDataDir;
		IVariantVectorPtr ipRSDStack = ipSource->RSDFileStack;
		if (ipRSDStack != __nullptr)
		{
			ICopyableObjectPtr ipCopyObj(ipRSDStack);
			ASSERT_RESOURCE_ALLOCATION("ELI41955", ipCopyObj != __nullptr);
			m_ipRSDFileStack = ipCopyObj->Clone();
		}
		m_eParallelRunMode = (EParallelRunMode)ipSource->ParallelRunMode;

		// Shallow copy OCR parameters because they are not expected to be
		// modified during rule execution
		IHasOCRParametersPtr ipOCRParams(pObject);
		m_ipOCRParameters = ipOCRParams->OCRParameters;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08202");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		// Create a new IAFDocument object
		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_AFDocument);
		ASSERT_RESOURCE_ALLOCATION("ELI05853", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);
	
		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05854");
}
//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	*pClassID = CLSID_AFDocument;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// TODO: track dirty state
		HRESULT hr = S_OK;

		return hr;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI49954");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license state
		validateLicense();

		// Reset all the member variables
		m_ipAttribute = __nullptr;
		m_ipStringTags = __nullptr;
		m_ipObjectTags = __nullptr;
		m_ipRSDFileStack = __nullptr;
		m_strFKBVersion = "";
		m_strAlternateComponentDataDir = "";

		bool bHasAttribute = false;
		bool bHasStringTags = false;
		bool bHasObjectTags = false;
		bool bHasRSDFileStack = false;
		bool bHasOCRParameters = false;

		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read( &nDataLength, sizeof(nDataLength), __nullptr );
		ByteStream data( nDataLength );
		pStream->Read( data.getData(), nDataLength, __nullptr );
		ByteStreamManipulator dataReader( ByteStreamManipulator::kRead, data );

		// Read the individual data items from the bytestream
		dataReader >> m_nVersionNumber;

		// Check for newer version
		if (m_nVersionNumber > gnCurrentVersion)
		{
			// Throw exception
			UCLIDException ue( "ELI41963", "Unable to load newer AFDocument." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", m_nVersionNumber );
			throw ue;
		}

		// Read the FKB version
		dataReader >> m_strFKBVersion;

		// Read the AlternateComponentDataDir
		dataReader >> m_strAlternateComponentDataDir;

		long lParallelRunMode;
		dataReader >> lParallelRunMode;
		m_eParallelRunMode = (EParallelRunMode) lParallelRunMode;

		dataReader >> bHasAttribute;
		dataReader >> bHasStringTags;
		dataReader >> bHasObjectTags;
		dataReader >> bHasRSDFileStack;
		dataReader >> bHasOCRParameters;

		// Read attribute from the stream
		if (bHasAttribute)
		{
			IPersistStreamPtr ipObj;
			readObjectFromStream(ipObj, pStream, "ELI41964");
			m_ipAttribute = ipObj;
			if (m_ipAttribute == __nullptr)
			{
				throw UCLIDException("ELI41965", "Attribute could not be read from stream!");
			}
		}

		// Read string tags
		if (bHasStringTags)
		{
			IPersistStreamPtr ipObj;
			readObjectFromStream(ipObj, pStream, "ELI41966");
			m_ipStringTags = ipObj;
			if (m_ipStringTags == __nullptr)
			{
				throw UCLIDException("ELI49999", "String tags could not be read from stream!");
			}
		}

		// Read object tags
		if (bHasObjectTags)
		{
			IPersistStreamPtr ipObj;
			readObjectFromStream(ipObj, pStream, "ELI41967");
			m_ipObjectTags = ipObj;
			if (m_ipObjectTags == __nullptr)
			{
				throw UCLIDException("ELI41968", "Object tags could not be read from stream!");
			}
		}

		// Read rsd file stack
		if (bHasRSDFileStack)
		{
			IPersistStreamPtr ipObj;
			readObjectFromStream(ipObj, pStream, "ELI41969");
			m_ipRSDFileStack = ipObj;
			if (m_ipRSDFileStack == __nullptr)
			{
				throw UCLIDException("ELI41970", "RSD file stack could not be read from stream!");
			}
		}

		// Read the OCR parameters
		if (bHasOCRParameters)
		{
			IPersistStreamPtr ipObj;

			::readObjectFromStream(ipObj, pStream, "ELI45906");
			ASSERT_RESOURCE_ALLOCATION("ELI45907", ipObj != __nullptr);
			m_ipOCRParameters = ipObj;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41971");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license state
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter( ByteStreamManipulator::kWrite, data );
		dataWriter << gnCurrentVersion;
		dataWriter << m_strFKBVersion;
		dataWriter << m_strAlternateComponentDataDir;
		dataWriter << (long) m_eParallelRunMode;

		bool bHasAttribute = (m_ipAttribute != __nullptr);
		bool bHasStringTags = (m_ipStringTags != __nullptr);
		bool bHasObjectTags = (m_ipObjectTags != __nullptr);
		bool bHasRSDFileStack = (m_ipRSDFileStack != __nullptr);
		bool bHasOCRParameters = (m_ipOCRParameters != __nullptr);
		dataWriter << bHasAttribute;
		dataWriter << bHasStringTags;
		dataWriter << bHasObjectTags;
		dataWriter << bHasRSDFileStack;
		dataWriter << bHasOCRParameters;

		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write( &nDataLength, sizeof(nDataLength), __nullptr );
		pStream->Write( data.getData(), nDataLength, __nullptr );

		// Separately write the Attribute to the stream
		IPersistStreamPtr ipPersistentObj;
		if (bHasAttribute)
		{
			ipPersistentObj = m_ipAttribute;
			writeObjectToStream(ipPersistentObj, pStream, "ELI41972", fClearDirty);
		}

		// Write the string tags
		if (bHasStringTags)
		{
			ipPersistentObj = m_ipStringTags;
			writeObjectToStream(ipPersistentObj, pStream, "ELI41979", fClearDirty);
		}

		// Write the object tags
		if (bHasObjectTags)
		{
			ipPersistentObj = m_ipObjectTags;
			writeObjectToStream(ipPersistentObj, pStream, "ELI41974", fClearDirty);
		}

		// Write the RSD file stack
		if (bHasRSDFileStack)
		{
			ipPersistentObj = m_ipRSDFileStack;
			writeObjectToStream(ipPersistentObj, pStream, "ELI41975", fClearDirty);
		}

		// Write the OCR parameters
		if (bHasOCRParameters)
		{
			IPersistStreamPtr ipPIObj = m_ipOCRParameters;
			ASSERT_RESOURCE_ALLOCATION("ELI45908", ipPIObj != __nullptr);
			writeObjectToStream(ipPIObj, pStream, "ELI45909", fClearDirty);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41976");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
UCLID_AFCORELib::IAttributePtr CAFDocument::getAttribute()
{
	if (m_ipAttribute == __nullptr)
	{
		m_ipAttribute.CreateInstance(CLSID_Attribute);
		ASSERT_RESOURCE_ALLOCATION("ELI34802", m_ipAttribute != __nullptr);
	}

	return m_ipAttribute;
}
//-------------------------------------------------------------------------------------------------
void CAFDocument::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI05851", "AF Document" );
}
//-------------------------------------------------------------------------------------------------
// IHasOCRParameters
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::get_OCRParameters(IOCRParameters** ppOCRParameters)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*ppOCRParameters = getOCRParameters().Detach();
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI45911");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::put_OCRParameters(IOCRParameters* pOCRParameters)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI45912", pOCRParameters != __nullptr);
		m_ipOCRParameters = pOCRParameters;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI45913");
}
//-------------------------------------------------------------------------------------------------
IOCRParametersPtr CAFDocument::getOCRParameters()
{
	if (m_ipOCRParameters != __nullptr)
	{
		return m_ipOCRParameters;
	}

	// If none specified directly, e.g., by a ruleset, then just return the parameters
	// associated with the input value (SpatialString will create the collection if needed)
	IHasOCRParametersPtr ipInputParams(getAttribute()->Value);
	IOCRParametersPtr ipOCRParameters = ipInputParams->OCRParameters;
	ASSERT_RESOURCE_ALLOCATION("ELI45910", ipOCRParameters != __nullptr);

	return ipOCRParameters;
}
//-------------------------------------------------------------------------------------------------
