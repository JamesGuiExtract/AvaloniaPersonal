// AFDocument.cpp : Implementation of CAFDocument
#include "stdafx.h"
#include "AFCore.h"
#include "AFDocument.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CAFDocument
//-------------------------------------------------------------------------------------------------
CAFDocument::CAFDocument()
: m_ipAttribute(__nullptr),
  m_ipStringTags(__nullptr),
  m_ipObjectTags(__nullptr)
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
		&IID_ICopyableObject
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

			// Clone the attribute hierarchy beneath the top-level document attribute, but not the
			// document text.
			if (bCloneAttributes)
			{
				ipDocCopy->Attribute = UCLID_AFCORELib::IAttributePtr(CLSID_Attribute);
				ICopyableObjectPtr ipCopyObj(m_ipAttribute->SubAttributes);
				ASSERT_RESOURCE_ALLOCATION("ELI36256", ipCopyObj != __nullptr);
				ipDocCopy->Attribute->SubAttributes = IIUnknownVectorPtr(ipCopyObj->Clone());
				ipDocCopy->Attribute->Value = m_ipAttribute->Value;
			}
			// Clone the document text but not the attribute hierarchy beneath the top-level
			// document attribute.
			if (bCloneText)
			{
				ipDocCopy->Attribute = UCLID_AFCORELib::IAttributePtr(CLSID_Attribute);
				ICopyableObjectPtr ipCopyObj(m_ipAttribute->Value);
				ASSERT_RESOURCE_ALLOCATION("ELI36257", ipCopyObj != __nullptr);
				ipDocCopy->Attribute->Value = ISpatialStringPtr(ipCopyObj->Clone());
				ipDocCopy->Attribute->SubAttributes = m_ipAttribute->SubAttributes;
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

		// Return the new object to the caller
		*pAFDoc = (IAFDocument *)ipDocCopy.Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36260");
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
