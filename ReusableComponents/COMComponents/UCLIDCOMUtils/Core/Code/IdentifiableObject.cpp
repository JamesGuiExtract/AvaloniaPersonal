#include "stdafx.h"
#include "UCLIDCOMUtils.h"
#include "IdentifiableObject.h"

#include <UCLIDException.h>
#include <COMUtils.h>
#include <ByteStreamManipulator.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//--------------------------------------------------------------------------------------------------
// CIdentifiableObject
//--------------------------------------------------------------------------------------------------
CIdentifiableObject::CIdentifiableObject()
: m_upGUID(__nullptr)
{
	try
	{
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI33624");
}
//--------------------------------------------------------------------------------------------------
CIdentifiableObject::~CIdentifiableObject()
{
	try
	{
		m_upGUID.reset(__nullptr);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI33625");
}
//--------------------------------------------------------------------------------------------------
GUID CIdentifiableObject::getGUID(bool bRegenerate/* = false*/)
{
	try
	{
		validateLicense();

		// Create the GUID if bRegenerate is set or it has not yet been created.
		if (bRegenerate || m_upGUID.get() == __nullptr)
		{
			m_upGUID.reset(new GUID());
			CoCreateGuid(m_upGUID.get());
		}

		return *m_upGUID.get();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI33626");
}
//--------------------------------------------------------------------------------------------------
void CIdentifiableObject::loadGUID(IStream *pStream)
{
	try
	{
		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read(&nDataLength, sizeof(nDataLength), __nullptr);
		ByteStream data(nDataLength);
		pStream->Read(data.getData(), nDataLength, __nullptr);
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;
		
		// Check for newer version
		if (nDataVersion > gnCurrentVersion)
		{
			// Throw exception
			UCLIDException ue("ELI33627", "Unable to load newer IdentifiableObject!");
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		// Read the GUID as four DWORDS.
		m_upGUID.reset(new GUID());
		DWORD* pdwGUIDData = (DWORD*)m_upGUID.get();
		for (int i = 0; i < 4; i++)
		{
			dataReader >> pdwGUIDData[i];
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI33628");
}
//--------------------------------------------------------------------------------------------------
void CIdentifiableObject::saveGUID(IStream * pStream)
{
	try
	{
		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		dataWriter << gnCurrentVersion;
	
		// Create the GUID if it has not yet been created.
		if (m_upGUID.get() == __nullptr)
		{
			m_upGUID.reset(new GUID());
			CoCreateGuid(m_upGUID.get());
		}

		// Write the GUID as four DWORDS.
		DWORD* pdwGUIDData = (DWORD*)m_upGUID.get();
		for (int i = 0; i < 4; i++)
		{
			dataWriter << pdwGUIDData[i];
		}

		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI33629");
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CIdentifiableObject::validateLicense()
{
	VALIDATE_LICENSE(gnEXTRACT_CORE_OBJECTS, "ELI33519", "Identifiable Object");
}