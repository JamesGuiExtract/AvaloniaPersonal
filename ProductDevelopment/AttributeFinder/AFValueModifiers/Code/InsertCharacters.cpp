// InsertCharacters.cpp : Implementation of CInsertCharacters
#include "stdafx.h"
#include "AFValueModifiers.h"
#include "InsertCharacters.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CInsertCharacters
//-------------------------------------------------------------------------------------------------
CInsertCharacters::CInsertCharacters()
: m_eLengthType(kAnyLength),
  m_nNumOfCharsLong(0),
  m_strCharsToInsert(""),
  m_bAppendToEnd(true),
  m_nPositionToInsert(0),
  m_bDirty(false)
{
}
//-------------------------------------------------------------------------------------------------
CInsertCharacters::~CInsertCharacters()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16357");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInsertCharacters::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IInsertCharacters,
		&IID_IAttributeModifyingRule,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_ILicensedComponent,
		&IID_IMustBeConfiguredObject
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IInsertCharacters
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInsertCharacters::get_LengthType(EInsertCharsLengthType *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_eLengthType;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04968");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInsertCharacters::put_LengthType(EInsertCharsLengthType newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_eLengthType = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04969");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInsertCharacters::get_NumOfCharsLong(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_nNumOfCharsLong;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04970");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInsertCharacters::put_NumOfCharsLong(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (newVal <= 0)
		{
			UCLIDException ue("ELI05804", "Please specify length of the input text.");
			ue.addDebugInfo("Length of input text", newVal);
			throw ue;
		}

		m_nNumOfCharsLong = newVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04971");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInsertCharacters::get_CharsToInsert(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = _bstr_t(m_strCharsToInsert.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04972");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInsertCharacters::put_CharsToInsert(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		string strToInsert = asString( newVal );
		if (strToInsert.empty())
		{
			UCLIDException ue("ELI05803", "Please specify non-empty text as characters for insertion.");
			ue.addDebugInfo("Characters for insertion", strToInsert);
			throw ue;
		}

		m_strCharsToInsert = strToInsert;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04973");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInsertCharacters::get_AppendToEnd(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bAppendToEnd ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04983");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInsertCharacters::put_AppendToEnd(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bAppendToEnd = newVal==VARIANT_TRUE;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04984");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInsertCharacters::get_InsertAt(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_nPositionToInsert;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04974");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInsertCharacters::put_InsertAt(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (newVal <= 0)
		{
			UCLIDException ue("ELI05802", "Invalid position number.");
			ue.addDebugInfo("Position", newVal);
			throw ue;
		}

		m_nPositionToInsert = newVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04975");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IAttributeModifyingRule
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInsertCharacters::raw_ModifyValue(IAttribute* pAttribute, IAFDocument* pOriginInput,
												IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		IAttributePtr	ipAttribute(pAttribute);
		ASSERT_RESOURCE_ALLOCATION( "ELI09283", ipAttribute != __nullptr );

		ISpatialStringPtr ipInputText = ipAttribute->GetValue();
		ASSERT_RESOURCE_ALLOCATION( "ELI09284", ipInputText != __nullptr);

		long nInputLength = (ipInputText->String).length();

		// a 0-based actual insertion position
		long nActualInsertPosition = m_nPositionToInsert-1;

		if (m_bAppendToEnd)
		{
			nActualInsertPosition = nInputLength;
		}
		else if (nActualInsertPosition > nInputLength)
		{
			// if only the position to insert is less than or 
			// equal to the input text length
			return S_OK;
		}

		switch (m_eLengthType)
		{
		case kEqual:
			if (nInputLength != m_nNumOfCharsLong)
			{
				return S_OK;
			}
			break;
		case kLessThanEqual:
			if (nInputLength > m_nNumOfCharsLong)
			{
				return S_OK;
			}
			break;
		case kLessThan:
			if (nInputLength >= m_nNumOfCharsLong)
			{
				return S_OK;
			}
			break;
		case kGreaterThanEqual:
			if (nInputLength < m_nNumOfCharsLong)
			{
				return S_OK;
			}
			break;
		case kGreaterThan:
			if (nInputLength <= m_nNumOfCharsLong)
			{
				return S_OK;
			}
			break;
		case kNotEqual:
			if (nInputLength == m_nNumOfCharsLong)
			{
				return S_OK;
			}
			break;
		}

		// Insert the characters at the specified location
		ipInputText->InsertString(nActualInsertPosition, m_strCharsToInsert.c_str());
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04961");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInsertCharacters::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		UCLID_AFVALUEMODIFIERSLib::IInsertCharactersPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI08275", ipSource != __nullptr);

		m_eLengthType = (EInsertCharsLengthType)ipSource->GetLengthType();
		m_nNumOfCharsLong = ipSource->GetNumOfCharsLong();
		
		m_strCharsToInsert = asString(ipSource->GetCharsToInsert());
		m_bAppendToEnd = (ipSource->GetAppendToEnd()==VARIANT_TRUE) ? true : false;
		m_nPositionToInsert = ipSource->GetInsertAt();

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08276");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInsertCharacters::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_InsertCharacters);
		ASSERT_RESOURCE_ALLOCATION("ELI08356", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04963");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInsertCharacters::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19599", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Insert characters").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04964");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInsertCharacters::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInsertCharacters::raw_IsConfigured(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		bool bConfigured = m_strCharsToInsert.empty() ? false : true;
		if (!m_bAppendToEnd)
		{
			bConfigured = bConfigured && m_nPositionToInsert>0;
		}

		if (m_eLengthType > kAnyLength)
		{
			bConfigured = bConfigured && m_nNumOfCharsLong>=0;
		}

		*pbValue = bConfigured ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04965")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInsertCharacters::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_InsertCharacters;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInsertCharacters::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInsertCharacters::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Clear the variables first
		m_eLengthType = kAnyLength;
		m_nNumOfCharsLong = 0;
		m_strCharsToInsert = "";
		m_nPositionToInsert = 0;

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
			UCLIDException ue( "ELI07653", "Unable to load newer InsertCharacters Modifier." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if (nDataVersion >= 1)
		{
			long nLengthType = 0;
			dataReader >> nLengthType;
			m_eLengthType = (EInsertCharsLengthType)nLengthType;
			dataReader >> m_nNumOfCharsLong;
			dataReader >> m_strCharsToInsert;
			dataReader >> m_bAppendToEnd;
			dataReader >> m_nPositionToInsert;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04966");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInsertCharacters::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter << (long)m_eLengthType;
		dataWriter << m_nNumOfCharsLong;
		dataWriter << m_strCharsToInsert;
		dataWriter << m_bAppendToEnd;
		dataWriter << m_nPositionToInsert;
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04967");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInsertCharacters::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CInsertCharacters::validateLicense()
{
	static const unsigned long INSERT_CHARS_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( INSERT_CHARS_COMPONENT_ID, "ELI04960", "Insert Characters" );
}
//-------------------------------------------------------------------------------------------------
