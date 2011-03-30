// LimitAsLeftPart.cpp : Implementation of CLimitAsLeftPart
#include "stdafx.h"
#include "AFValueModifiers.h"
#include "LimitAsLeftPart.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 2;

//-------------------------------------------------------------------------------------------------
// CLimitAsLeftPart
//-------------------------------------------------------------------------------------------------
CLimitAsLeftPart::CLimitAsLeftPart()
: m_nNumOfChars(0),
  m_bAcceptSmallerLength(false),
  m_bExtract(true),
  m_bDirty(false)
{
}
//-------------------------------------------------------------------------------------------------
CLimitAsLeftPart::~CLimitAsLeftPart()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16359");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsLeftPart::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ILimitAsLeftPart,
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
// IAttributeModifyingRule
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsLeftPart::raw_ModifyValue(IAttribute* pAttribute, IAFDocument* pOriginInput,
											   IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		IAttributePtr	ipAttribute(pAttribute);
		ASSERT_RESOURCE_ALLOCATION( "ELI09286", ipAttribute != __nullptr );

		ISpatialStringPtr ipInputText = ipAttribute->GetValue();
		ASSERT_ARGUMENT("ELI06617", ipInputText != __nullptr);

		// Extract or Remove requires a number of characters
		if (m_nNumOfChars > 0)
		{
			// start position and end position for extracting the sub string
			long nStart=0, nEnd=0;

			// Consider Extracting
			long nLength = ipInputText->String.length();
			if (m_bExtract)
			{
				// Check string length too small for specified Extract
				if (nLength < m_nNumOfChars)
				{
					if (!m_bAcceptSmallerLength)
					{
						// remove all its contents
						ipInputText->Clear();
					}

					return S_OK;
				}

				nStart = 0;
				nEnd = m_nNumOfChars-1;
			}
			// Consider Removing
			else
			{
				// Check string length too small for specified Remove
				if (nLength < m_nNumOfChars)
				{
					if (m_bAcceptSmallerLength)
					{
						// remove all its contents
						ipInputText->Clear();
					}

					return S_OK;
				}

				// Remove the desired number of characters from the front of the string
				nStart = m_nNumOfChars;
				nEnd = nLength - 1;

				if (nEnd < nStart)
				{
					ipInputText->Clear();
					return S_OK;
				}
			}

			ISpatialStringPtr ipModifiedValue = ipInputText->GetSubString(nStart, nEnd);
			ASSERT_RESOURCE_ALLOCATION("ELI25959", ipModifiedValue != __nullptr);

			ICopyableObjectPtr ipCopier = ipInputText;
			ASSERT_RESOURCE_ALLOCATION("ELI25960", ipCopier != __nullptr);
			ipCopier->CopyFrom(ipModifiedValue);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04192");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsLeftPart::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		UCLID_AFVALUEMODIFIERSLib::ILimitAsLeftPartPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI08277", ipSource != __nullptr);
		
		m_nNumOfChars = ipSource->GetNumberOfCharacters();
		m_bAcceptSmallerLength = (ipSource->GetAcceptSmallerLength()==VARIANT_TRUE) ? true : false;
		m_bExtract = (ipSource->GetExtract()==VARIANT_TRUE) ? true : false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08278");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsLeftPart::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_LimitAsLeftPart);
		ASSERT_RESOURCE_ALLOCATION("ELI08355", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04468");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsLeftPart::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19600", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Extract or remove leftmost characters").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04193");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsLeftPart::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CLimitAsLeftPart::raw_IsConfigured(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// depends on num of chars
		*pbValue = m_nNumOfChars > 0 ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04843")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILimitAsRightPart
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsLeftPart::get_NumberOfCharacters(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_nNumOfChars;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04282");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsLeftPart::put_NumberOfCharacters(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (newVal <= 0)
		{
			UCLIDException ue("ELI05820", "Number of characters must be greater than 0.");
			ue.addDebugInfo("Number of characters", newVal);
			throw ue;
		}

		m_nNumOfChars = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04283");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsLeftPart::get_AcceptSmallerLength(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bAcceptSmallerLength?VARIANT_TRUE:VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04284");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsLeftPart::put_AcceptSmallerLength(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bAcceptSmallerLength = newVal==VARIANT_TRUE;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04285");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsLeftPart::get_Extract(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Provide setting
		*pVal = m_bExtract ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05288");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsLeftPart::put_Extract(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Store setting
		m_bExtract = (newVal == VARIANT_TRUE);

		// Set Dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05289");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsLeftPart::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_LimitAsLeftPart;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsLeftPart::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
// NOTES about versions:
// Version 1:
//   * Saved: 
//            data version,
//            number of characters to extract,
//            whether or not to accept a smaller length
// Version 2:
//   * Additionally saved:
//            whether or not characters are being extracted or removed
//   * NOTE:
//            Number of characters used in Version 1 for extraction is now also used for removal
STDMETHODIMP CLimitAsLeftPart::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Clear the variables first
		m_nNumOfChars = 0;
		m_bAcceptSmallerLength = false;
		m_bExtract = true;

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
			UCLIDException ue( "ELI07654", "Unable to load newer LimitAsLeftPart Modifier." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if (nDataVersion >= 1)
		{
			dataReader >> m_nNumOfChars;
			dataReader >> m_bAcceptSmallerLength;
		}

		// Check for Extract vs. Remove if Version >= 2
		if (nDataVersion >= 2)
		{
			dataReader >> m_bExtract;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04705");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
// Version == 2
//   Added m_bExtract
STDMETHODIMP CLimitAsLeftPart::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter << m_nNumOfChars;
		dataWriter << m_bAcceptSmallerLength;
		dataWriter << m_bExtract;
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04706");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsLeftPart::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CLimitAsLeftPart::validateLicense()
{
	static const unsigned long LIMIT_LEFT_PART_RULE_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( LIMIT_LEFT_PART_RULE_COMPONENT_ID, "ELI04191", "Limit As Left Part" );
}
//-------------------------------------------------------------------------------------------------
