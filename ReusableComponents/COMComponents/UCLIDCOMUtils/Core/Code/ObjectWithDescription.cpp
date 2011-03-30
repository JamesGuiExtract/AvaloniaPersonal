//-------------------------------------------------------------------------------------------------
//
// COPYRIGHT (c) 2003 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ObjectWithDescription.cpp
//
// PURPOSE:	Implementation of CObjectWithDescription class
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//-------------------------------------------------------------------------------------------------

#include "stdafx.h"
#include "UCLIDCOMUtils.h"
#include "ObjectWithDescription.h"

#include <UCLIDException.h>
#include <COMUtils.h>
#include <LicenseMgmt.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 2;

//-------------------------------------------------------------------------------------------------
// CObjectWithDescription
//-------------------------------------------------------------------------------------------------
CObjectWithDescription::CObjectWithDescription()
: m_ipObj(NULL), m_strDescription(""), m_bEnabled(true), m_bDirty(false)
{
}
//-------------------------------------------------------------------------------------------------
CObjectWithDescription::~CObjectWithDescription()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16514");
}

//-------------------------------------------------------------------------------------------------
// IObjectWithDescription
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CObjectWithDescription::get_Object(IUnknown **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		if (m_ipObj != __nullptr)
		{
			// Do a shallow copy of the smart pointer
			IUnknownPtr ipShallowCopy = m_ipObj;

			// Provide the object pointer
			*pVal = ipShallowCopy.Detach();
		}
		else
		{
			*pVal = NULL;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04308");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CObjectWithDescription::put_Object(IUnknown *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Release current object
		if (m_ipObj != __nullptr)
		{
			m_ipObj = __nullptr;
		}

		// Store the object pointer
		m_ipObj = newVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04309");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CObjectWithDescription::get_Description(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Provide the string
		_bstr_t	bstrText( m_strDescription.c_str() );
		*pVal = bstrText.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04310");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CObjectWithDescription::put_Description(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Store the new string
		m_strDescription = asString( newVal );
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04311");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CObjectWithDescription::get_Enabled(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license and return setting
		validateLicense();
		*pVal = m_bEnabled ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13612");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CObjectWithDescription::put_Enabled(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		// Save setting
		m_bEnabled = (newVal == VARIANT_TRUE);
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13613");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CObjectWithDescription::CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		UCLID_COMUTILSLib::IObjectWithDescriptionPtr ipSource = pObject;
		ASSERT_RESOURCE_ALLOCATION("ELI08313", ipSource != __nullptr);
		
		// set the other object's description to this object's description
		m_strDescription = asString(ipSource->GetDescription());

		// Set enabled flag for this object
		m_bEnabled = (ipSource->Enabled == VARIANT_TRUE);

		// set the other object's object to be a clone of this object's object.
		// while doing so, make sure that the object we are holding is copyable
		IUnknownPtr ipUnk = ipSource->GetObject();
		if(ipUnk == __nullptr)
		{
			// 1/28/08 SNK Added this block to fix [P13:4791]. If we are copying from an OWD with
			// a NULL object, this OWD's object should be set to NULL as well. In the case of
			// [P13:4791], if a user selects "<NONE>" for the skip condition, the skip condition
			// object should be removed.  If we keep the old object, it will continue to be
			// used despite the user selecting "<NONE>"
			m_ipObj = __nullptr;
		}
		else
		{
			UCLID_COMUTILSLib::ICopyableObjectPtr ipCopyableObject = ipUnk;
			ASSERT_RESOURCE_ALLOCATION("ELI08314", ipCopyableObject != __nullptr);
			
			m_ipObj = ipCopyableObject->Clone();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08315");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CObjectWithDescription::Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		// create a new IObjectWithDescription object
		UCLID_COMUTILSLib::ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_ObjectWithDescription);
		ASSERT_RESOURCE_ALLOCATION("ELI19353", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04659");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CObjectWithDescription::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_ObjectWithDescription;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CObjectWithDescription::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		HRESULT hr = m_bDirty ? S_OK : S_FALSE;
		
		if (!m_bDirty)
		{
			IPersistStreamPtr ipPersistStream(m_ipObj);
			if (m_ipObj != __nullptr && ipPersistStream == __nullptr)
			{
				UCLIDException ue("ELI04782", "Object does not support persistence!");
				ue.addDebugInfo("Description", m_strDescription);
				throw ue;
			}
			else if (ipPersistStream)
			{
				hr = ipPersistStream->IsDirty();
			}
		}

		return hr;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04779")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CObjectWithDescription::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// reset member variables
		m_strDescription = "";
		m_ipObj = __nullptr;
		m_bEnabled = true;

		// read from the stream a flag indicating whether an object exists
		bool bObjectExists = false;

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
			UCLIDException ue( "ELI07666", "Unable to load newer ObjectWithDescription." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if (nDataVersion >= 1)
		{
			dataReader >> m_strDescription;
			dataReader >> bObjectExists;
		}

		if (nDataVersion >= 2)
		{
			dataReader >> m_bEnabled;
		}

		// read the object from the stream if applicable
		if (bObjectExists)
		{
			IPersistStreamPtr ipObj;
			readObjectFromStream(ipObj, pStream, "ELI09981");
			
			if (ipObj == __nullptr)
			{
				throw UCLIDException("ELI04629", "Unable to read object from stream!");
			}
			else
			{
				m_ipObj = ipObj;
			}
		}

		// set the dirty flag to false as we've just loaded the object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04616");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
// Version 2:
//   Added Enabled/disabled persistence
STDMETHODIMP CObjectWithDescription::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);
		dataWriter << gnCurrentVersion;

		// write the description to the stream
		dataWriter << m_strDescription;

		// write to the stream a flag indicating whether an object exists
		bool bObjectExists = (m_ipObj != __nullptr);
		dataWriter << bObjectExists;

		// write the enabled/disabled state to the stream
		dataWriter << m_bEnabled;

		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		if (bObjectExists)
		{
			// write the object to the stream
			IPersistStreamPtr ipObj = m_ipObj;
			if (ipObj == __nullptr)
			{
				throw UCLIDException("ELI04628", "Object does not support persistence!");
			}
			else
			{
				writeObjectToStream(ipObj, pStream, "ELI09936", fClearDirty);
			}
		}
		
		// clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04587");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CObjectWithDescription::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CObjectWithDescription::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IObjectWithDescription,
		&IID_IPersistStream,
		&IID_ICopyableObject,
		&IID_ILicensedComponent
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
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CObjectWithDescription::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	try
	{
		validateLicense();

		// if validateLicense doesn't throw any exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CObjectWithDescription::validateLicense()
{
	static const unsigned long OBJECTWITHDESCRIPTION_COMPONENT_ID = gnEXTRACT_CORE_OBJECTS;

	VALIDATE_LICENSE( OBJECTWITHDESCRIPTION_COMPONENT_ID, "ELI04307", "ObjectWithDescription" );
}
//-------------------------------------------------------------------------------------------------
