// ChangeCase.cpp : Implementation of CChangeCase
#include "stdafx.h"
#include "AFValueModifiers.h"
#include "ChangeCase.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 2;

//-------------------------------------------------------------------------------------------------
// CChangeCase
//-------------------------------------------------------------------------------------------------
CChangeCase::CChangeCase()
: m_eCaseType(kNoChangeCase),
  m_bDirty(false)
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CChangeCase::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IChangeCase,
		&IID_IAttributeModifyingRule,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_ILicensedComponent,
		&IID_IOutputHandler
	};

	for (int i = 0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
		{
			return S_OK;
		}
	}

	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IChangeCase
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CChangeCase::get_CaseType(EChangeCaseType *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Provide setting
		*pVal = m_eCaseType;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06381");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CChangeCase::put_CaseType(EChangeCaseType newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Save setting
		m_eCaseType = newVal;

		// Set flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06382");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IAttributeModifyingRule
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CChangeCase::raw_ModifyValue(IAttribute * pAttribute, IAFDocument* pOriginInput,
										  IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		IAttributePtr	ipAttribute(pAttribute);
		ASSERT_RESOURCE_ALLOCATION( "ELI09279", ipAttribute != __nullptr );

		ISpatialStringPtr ipValue = ipAttribute->GetValue();
		ASSERT_ARGUMENT("ELI06611", ipValue != __nullptr);

		// Modify string based on case type
		switch (m_eCaseType)
		{
		case kNoChangeCase:
			// Do nothing
			return S_OK;
			break;
		case kMakeUpperCase:
			ipValue->ToUpperCase();
			break;
		case kMakeLowerCase:
			ipValue->ToLowerCase();
			break;
		case kMakeTitleCase:
			ipValue->ToTitleCase();
			break;
		default:
			// Error condition
			UCLIDException ue( "ELI06393", "Unknown case." );
			ue.addDebugInfo( "Chosen case", m_eCaseType );
			throw ue;
			break;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06383");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IOutputHandler
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CChangeCase::raw_ProcessOutput(IIUnknownVector * pAttributes, IAFDocument * pDoc,
											IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Create AFUtility object
		IAFUtilityPtr ipAFUtility( CLSID_AFUtility );
		ASSERT_RESOURCE_ALLOCATION( "ELI08690", ipAFUtility != __nullptr );

		// Use Attributes as smart pointer
		IIUnknownVectorPtr ipAttributes( pAttributes );
		ASSERT_RESOURCE_ALLOCATION("ELI08693", ipAttributes != __nullptr);

		// Apply Attribute Modification
		ipAFUtility->ApplyAttributeModifier( ipAttributes, pDoc, this, VARIANT_TRUE );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08692");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CChangeCase::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CChangeCase::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19597", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Change case").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06385");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CChangeCase::raw_CopyFrom(IUnknown * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		UCLID_AFVALUEMODIFIERSLib::IChangeCasePtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI08273", ipSource != __nullptr);

		m_eCaseType = (EChangeCaseType)ipSource->GetCaseType();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08274");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CChangeCase::raw_Clone(IUnknown * * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Create a new ChangeCase object
		ICopyableObjectPtr ipObjCopy( CLSID_ChangeCase );
		ASSERT_RESOURCE_ALLOCATION("ELI08357", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06390");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CChangeCase::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_ChangeCase;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CChangeCase::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
// Version 1:
//   * Saved: 
//            case type
// Version 2:
//   * Additionally saved:
//            data version
//   * NOTE:
//            data version is read from file in version 1 but not saved, this is an error
STDMETHODIMP CChangeCase::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Clear variables first
		m_eCaseType = kNoChangeCase;
		
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
			UCLIDException ue( "ELI07652", "Unable to load newer ChangeCase Modifier." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if (nDataVersion >= 1)
		{
			// NOTE: The following code does extra work in order to support 
			//       backward compatibility for old RSD files

			// In old files, the case type will be the data version and 
			// an exception will be thrown here
			long lCaseType = 0;
			try
			{
				// Read stored case type
				dataReader >> lCaseType;
			}
			catch(...)
			{
				// Use the data version as the case type
				lCaseType = nDataVersion;
			}

			// Apply the retrieved case type
			m_eCaseType = (EChangeCaseType)lCaseType;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06386");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CChangeCase::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter << (long)m_eCaseType;
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06387");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CChangeCase::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CChangeCase::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI06384", "Change Case" );
}
//-------------------------------------------------------------------------------------------------
