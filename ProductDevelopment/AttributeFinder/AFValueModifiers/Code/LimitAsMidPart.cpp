// LimitAsMidPart.cpp : Implementation of CLimitAsMidPart
#include "stdafx.h"
#include "AFValueModifiers.h"
#include "LimitAsMidPart.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 2;

//-------------------------------------------------------------------------------------------------
// CLimitAsMidPart
//-------------------------------------------------------------------------------------------------
CLimitAsMidPart::CLimitAsMidPart()
: m_nStartPos(0),
  m_nEndPos(0),
  m_bAcceptSmallerLength(false),
  m_bExtract(true),
  m_bDirty(false)
{
}
//-------------------------------------------------------------------------------------------------
CLimitAsMidPart::~CLimitAsMidPart()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16360");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsMidPart::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ILimitAsMidPart,
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
STDMETHODIMP CLimitAsMidPart::raw_ModifyValue(IAttribute* pAttribute, IAFDocument* pOriginInput, 
											  IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		IAttributePtr	ipAttribute(pAttribute);
		ASSERT_RESOURCE_ALLOCATION( "ELI09287", ipAttribute != __nullptr );

		ISpatialStringPtr ipInputText = ipAttribute->GetValue();
		ASSERT_ARGUMENT("ELI06619", ipInputText != __nullptr);

		// End position must be beyond start position
		if (m_nStartPos <= m_nEndPos)
		{
			// since m_nStartPos and m_nEndPos are 1-based, let's
			// convert them to 0-based for calculation
			long nSpecifiedStartPos = m_nStartPos-1;
			long nSpecifiedEndPos = m_nEndPos - 1;
			long nLen = ipInputText->String.length();

			// Consider Extract case
			if (m_bExtract)
			{
				if (nLen <= nSpecifiedStartPos)
				{
					ipInputText->Clear();
					return S_OK;
				}

				// if specified end position is greater than the actual
				// string length and the user chooses to not to accept
				// any smaller lenght, then set the value to empty
				if (nLen <= nSpecifiedEndPos && nLen > nSpecifiedStartPos)
				{
					if (!m_bAcceptSmallerLength)
					{
						ipInputText->Clear();
						return S_OK;
					}

					// otherwise reset the end position to the end of the string
					nSpecifiedEndPos = nLen - 1;
				}

				// Continue with extraction if desired end position not beyond end of 
				// string OR if a smaller length result is acceptable
				ISpatialStringPtr ipModifiedValue = ipInputText->GetSubString(nSpecifiedStartPos, nSpecifiedEndPos);
				ASSERT_RESOURCE_ALLOCATION("ELI25961", ipModifiedValue != __nullptr);

				ICopyableObjectPtr ipCopier = ipInputText;
				ASSERT_RESOURCE_ALLOCATION("ELI25962", ipCopier != __nullptr);
				ipCopier->CopyFrom(ipModifiedValue);
			}
			// Consider Remove case
			else
			{
				// if actual length is less than the start position
				// then do nothing to the original value
				if (nLen <= nSpecifiedStartPos)
				{
					return S_OK;
				}

				// if specified end position is greater than the actual
				// string length and the user chooses to not to accept
				// any smaller lenght, then set the value to empty
				if (nLen <= nSpecifiedEndPos && nLen > nSpecifiedStartPos)
				{
					if (!m_bAcceptSmallerLength)
					{
						// if smaller lenghth can't be accept, 
						// then do nothing to the input value
						return S_OK;
					}

					// otherwise reset the end position to the end of the string
					nSpecifiedEndPos = nLen - 1;
				}

				// Continue with removal if desired end position not beyond end of 
				// string OR if a smaller length result is acceptable
				ipInputText->Remove(nSpecifiedStartPos, nSpecifiedEndPos);
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04197");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsMidPart::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		UCLID_AFVALUEMODIFIERSLib::ILimitAsMidPartPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI08279", ipSource != __nullptr);

		m_nStartPos = ipSource->GetStartPosition();
		m_nEndPos = ipSource->GetEndPosition();
		m_bAcceptSmallerLength = (ipSource->GetAcceptSmallerLength()==VARIANT_TRUE) ? true : false;
	
		m_bExtract = (ipSource->GetExtract()==VARIANT_TRUE) ? true : false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08280");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsMidPart::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_LimitAsMidPart);
		ASSERT_RESOURCE_ALLOCATION("ELI08354", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);
\
		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04471");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsMidPart::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19601", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Extract or remove middle characters").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04198");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsMidPart::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// CLimitAsMidPart
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsMidPart::get_StartPosition(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_nStartPos;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04290");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsMidPart::put_StartPosition(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (newVal <= 0)
		{
			UCLIDException ue("ELI05823", "Start position must be greater than 0.");
			ue.addDebugInfo("Start Position", newVal);
			throw ue;
		}

		m_nStartPos = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04291");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsMidPart::get_EndPosition(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_nEndPos;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04292");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsMidPart::put_EndPosition(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (newVal <= 0)
		{
			UCLIDException ue("ELI05824", "End position must be greater than 0.");
			ue.addDebugInfo("End Position", newVal);
			throw ue;
		}

		if (newVal < m_nStartPos)
		{
			UCLIDException ue("ELI05825", "End position must be greater than or equal to start position.");
			ue.addDebugInfo("Start Position", m_nStartPos);
			ue.addDebugInfo("End Position", newVal);
			throw ue;
		}

		m_nEndPos = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04293");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsMidPart::get_AcceptSmallerLength(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bAcceptSmallerLength?VARIANT_TRUE:VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04626");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsMidPart::put_AcceptSmallerLength(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bAcceptSmallerLength = newVal==VARIANT_TRUE;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04627");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsMidPart::get_Extract(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Provide setting
		*pVal = m_bExtract ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05290");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsMidPart::put_Extract(VARIANT_BOOL newVal)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05291");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsMidPart::raw_IsConfigured(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		*pbValue = (m_nStartPos > 0 && m_nEndPos >= m_nStartPos)
					? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04849")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsMidPart::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_LimitAsMidPart;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsMidPart::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
// NOTES about versions:
// Version 1:
//   * Saved: 
//            data version,
//            start position of extraction,
//            end position of extraction,
//            whether or not to accept a smaller length
// Version 2:
//   * Additionally saved:
//            whether or not characters are being extracted or removed
//   * NOTE:
//            Position information used in Version 1 for extraction is now also used for removal
STDMETHODIMP CLimitAsMidPart::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Clear the variables first
		m_nStartPos = 0;
		m_nEndPos = 0;
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
			UCLIDException ue( "ELI07655", "Unable to load newer LimitAsMidPart Modifier." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if (nDataVersion >= 1)
		{
			dataReader >> m_nStartPos;
			dataReader >> m_nEndPos;
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04707");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
// Version == 2
//   Added m_bExtract
STDMETHODIMP CLimitAsMidPart::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter << m_nStartPos;
		dataWriter << m_nEndPos;
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04708");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLimitAsMidPart::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CLimitAsMidPart::validateLicense()
{
	static const unsigned long LIMIT_MID_PART_RULE_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( LIMIT_MID_PART_RULE_COMPONENT_ID, "ELI04199", "Limit As Mid Part" );
}
//-------------------------------------------------------------------------------------------------
