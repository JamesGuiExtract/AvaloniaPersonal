// PadValue.cpp : Implementation of CPadValue
#include "stdafx.h"
#include "AFValueModifiers.h"
#include "PadValue.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CPadValue
//-------------------------------------------------------------------------------------------------
CPadValue::CPadValue()
:	m_bDirty(false),
	m_nRequiredSize(0),
	m_nPaddingCharacter('0'),
	m_bPadLeft(true)
{

}
//-------------------------------------------------------------------------------------------------
CPadValue::~CPadValue()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16362");
}
//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPadValue::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IPadValue,
		&IID_IAttributeModifyingRule,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_ILicensedComponent,
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}
//-------------------------------------------------------------------------------------------------
// IAttributeModifyingRule
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPadValue::raw_ModifyValue(IAttribute* pAttribute, IAFDocument* pOriginInput, 
										IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		IAttributePtr ipAttribute ( pAttribute );
		ASSERT_RESOURCE_ALLOCATION("ELI09689", ipAttribute != __nullptr );

		ISpatialStringPtr ipValue = ipAttribute->Value;
		ASSERT_RESOURCE_ALLOCATION("ELI09690", ipValue!= __nullptr );

		long nValueSize = ipValue->Size;
		if ( nValueSize < m_nRequiredSize )
		{
			// Calculate the amount of padding needed and build the padding string
			long nAmtToPad = m_nRequiredSize - nValueSize;
			string strPadStr(nAmtToPad, (char)m_nPaddingCharacter);

			if ( m_bPadLeft )
			{
				// Put the pad string at the beginning of the value
				ipValue->InsertString(0, strPadStr.c_str());
			}
			else
			{
				// Append the pad string to the end of the value
				ipValue->AppendString(strPadStr.c_str());
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09681");
	
	return S_OK;

}
//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPadValue::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();
		
		UCLID_AFVALUEMODIFIERSLib::IPadValuePtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI09701", ipSource != __nullptr);

		m_nRequiredSize = ipSource->RequiredSize;
		m_nPaddingCharacter = ipSource->PaddingCharacter;
		m_bPadLeft = ipSource->PadLeft == VARIANT_TRUE;

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09682");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPadValue::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_PadValue);
		ASSERT_RESOURCE_ALLOCATION("ELI09683", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09684");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPadValue::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19603", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Pad value").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09685");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPadValue::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPadValue::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_PadValue;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPadValue::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPadValue::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Clear the variables first
		m_nRequiredSize = 0;
		m_nPaddingCharacter = ' ';
		m_bPadLeft = true;

		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read( &nDataLength, sizeof(nDataLength), NULL );
		ByteStream data( nDataLength );
		pStream->Read( data.getData(), nDataLength, NULL );
		ByteStreamManipulator dataReader( ByteStreamManipulator::kRead, data );

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;
	
		// Check for newer version
		if (nDataVersion > gnCurrentVersion)
		{
			// Throw exception
			UCLIDException ue( "ELI09686", "Unable to load newer PadValue Modifier." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if ( nDataVersion >= 1 )
		{
			dataReader >> m_nRequiredSize;
			dataReader >> m_nPaddingCharacter;
			dataReader >> m_bPadLeft;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09687");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPadValue::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter << m_nRequiredSize;
		dataWriter << m_nPaddingCharacter;
		dataWriter << m_bPadLeft;
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09688");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPadValue::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	return E_NOTIMPL;
}
//-------------------------------------------------------------------------------------------------
// IPadValue
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPadValue::get_RequiredSize(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();
		
		*pVal = m_nRequiredSize;

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09692");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPadValue::put_RequiredSize(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_nRequiredSize = newVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09693");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPadValue::get_PaddingCharacter(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		*pVal = m_nPaddingCharacter;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09694");
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPadValue::put_PaddingCharacter(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		m_nPaddingCharacter = newVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09695");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPadValue::get_PadLeft(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		validateLicense();

		*pVal = m_bPadLeft ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09696");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CPadValue::put_PadLeft(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bPadLeft = newVal == VARIANT_TRUE;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19372");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CPadValue::validateLicense()
{
	static const unsigned long PAD_VALUE_RULE_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( PAD_VALUE_RULE_COMPONENT_ID, "ELI09702", "Pad Value" );
}
//-------------------------------------------------------------------------------------------------
