// SpatialStringInterfaces.cpp : Implementation of CSpatialString interfaces 
#include "stdafx.h"
#include "SpatialString.h"
#include "UCLIDRasterAndOCRMgmt.h"

#include <UCLIDException.h>
#include <COMUtils.h>
#include <ByteStreamManipulator.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 12;
const string gstrRASTER_OCR_SECTION = "\\ReusableComponents\\COMComponents\\UCLIDRasterAndOCRMgmt";
const string gstrCONVERT_LEGACY_STRINGS_KEY = "ConvertLegacyHybridStrings";

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ISpatialString,
		&IID_IPersistStream,
		&IID_IComparableObject,
		&IID_ICopyableObject,
		&IID_ILicensedComponent
	};

	for (int i = 0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i], riid))
		{
			return S_OK;
		}
	}

	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::raw_CopyFrom(IUnknown * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		// verify valid object
		UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipSource = pObject;
		ASSERT_RESOURCE_ALLOCATION("ELI08205", ipSource != NULL);

		// Copy from the specified string
		copyFromSpatialString(ipSource);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08206");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::raw_Clone(IUnknown * * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI25779", pObject != NULL);

		// Validate license first
		validateLicense();

		// Create a new ISpatialString object
		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_SpatialString);
		ASSERT_RESOURCE_ALLOCATION("ELI08372", ipObjCopy != NULL);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05866");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IComparableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::raw_IsEqualTo(IUnknown * pObj, VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI25780", pbValue != NULL);
		UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipObj(pObj);
		ASSERT_ARGUMENT("ELI05878", ipObj != NULL);

		validateLicense();

		// NOTE: this function does not compare the letter vector or the SpatialPageInfo map

		string strObjString = asString(ipObj->String);

		*pbValue = VARIANT_TRUE;

		if (m_strString != strObjString
			|| m_strSourceDocName != asString(ipObj->SourceDocName)
			|| m_eMode != ipObj->GetMode())
		{
			*pbValue = VARIANT_FALSE;
			return S_OK;
		}

		// Now both objects must have the same mode, so it is possible to compare the items in
		// each object based upon their spatial mode.
		// NonSpatialMode has no other relevant items to compare
		if( m_eMode == kNonSpatialMode)
		{
			return S_OK;
		}
		else if ( m_eMode == kSpatialMode )
		{
			// TODO: Compare letters vector
		}
		else if (m_eMode == kHybridMode )
		{
			// Compare raster zones vector
			IIUnknownVectorPtr ipObjZones = ipObj->GetOCRImageRasterZones();
			ASSERT_RESOURCE_ALLOCATION("ELI25781", ipObjZones != NULL);

			long lZonesSize = ipObjZones->Size();
			
			if (lZonesSize != m_vecRasterZones.size())
			{
				*pbValue = VARIANT_FALSE;
			}
			else
			{
				for (long i=0; i < lZonesSize; i++)
				{
					UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipObjZone = ipObjZones->At(i);
					ASSERT_RESOURCE_ALLOCATION("ELI25782", ipObjZone != NULL);

					UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone = m_vecRasterZones[i];
					ASSERT_RESOURCE_ALLOCATION("ELI25783", ipZone != NULL);

					if (ipZone->Equals(ipObjZone) == VARIANT_FALSE)
					{
						*pbValue = VARIANT_FALSE;
						return S_OK;
					}
				}
			}
		}
		else
		{
			// Should never get here
			UCLIDException ue("ELI15077", "Spatial string mode is invalid!");
			ue.addDebugInfo("Mode:", asString( m_eMode ) );
			throw ue;
		}
		// For both spatialmode and hybridmode, compare pageinfomap
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05867");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI25784", pbValue != NULL);

		try
		{
			validateLicense();

			// If validateLicense doesn't throw any exception, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25785");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	try
	{
		ASSERT_ARGUMENT("ELI25786", pClassID != NULL);

		// Validate license first
		validateLicense();

		*pClassID = CLSID_SpatialString;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25787");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// validate license
		validateLicense();

		// if the internal dirty flag is set, then return S_OK
		if (m_bDirty)
		{
			return S_OK;
		}

		// if this is not a spatial string, there's nothing else to check
		// so, return S_FALSE
		if (m_eMode == kNonSpatialMode)
		{
			return S_FALSE;
		}
	
		// For hybrid mode, each raster zone needs to be checked as well.
		if (m_eMode == kHybridMode)
		{
			for (vector<UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr>::iterator it = m_vecRasterZones.begin();
				it != m_vecRasterZones.end(); it++)
			{
				IPersistStreamPtr ipZone = (*it);
				ASSERT_RESOURCE_ALLOCATION("ELI25788", ipZone != NULL);
				if (ipZone->IsDirty() == S_OK)
				{
					return S_OK;
				}
			}
		}
		
		// The LongToObject map object will check each item's dirty flag and return 
		// S_OK if any item is dirty.
		IPersistStreamPtr ipPersistStream = getPageInfoMap();
		ASSERT_RESOURCE_ALLOCATION("ELI15082", ipPersistStream != NULL);
		
		if( ipPersistStream->IsDirty() == S_OK )
		{
			return S_OK;
		}

		// At this point, the object must not be dirty
		return S_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06649");
}
//-------------------------------------------------------------------------------------------------
// Version 8 - Replaced m_bIsSpatial with an enumeration named 'm_eMode'
// Version 9 - Size of CPPLetter object changed for m_usPageNumber
// Version 10 - Added m_bIsForcedRotation
// Version 11 - Removed m_bIsForcedRotation
// Version 12 - Added OCR Engine Version information
STDMETHODIMP CSpatialString::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		// Read the bytestream data from the IStream object
		long nDataLength = 0;

		pStream->Read( &nDataLength, sizeof(nDataLength), NULL );
		ByteStream data( nDataLength );
		pStream->Read( data.getData(), nDataLength, NULL );
		ByteStreamManipulator dataReader( ByteStreamManipulator::kRead, data );

		// reset the data members
		reset(true, true);

		// Read the individual data items from the bytestream
		// read the data version
		unsigned long nDataVersion = 0;

		dataReader >> nDataVersion;
		
		// Check for newer version
		if (nDataVersion > gnCurrentVersion)
		{
			// Throw exception
			UCLIDException ue( "ELI07672", "Unable to load newer SpatialString." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if (nDataVersion >= 2)
		{
			dataReader >> m_strSourceDocName;
		}

		string strOCREngineVersion = "";
		if (nDataVersion >= 12)
		{
			dataReader >> strOCREngineVersion;
		}

		// Read spatialness setting
		if (nDataVersion >= 3)
		{
			// Version 8 made m_bIsSpatial obsolete, now an enumeration named
			// m_eMode is used to keep track of the spatial mode of this spatial string
			if( nDataVersion < 8 )
			{
				bool bIsSpatial;
				dataReader >> bIsSpatial;

				if( bIsSpatial )
				{
					// Set the mode to spatial
					m_eMode = kSpatialMode;
				}
				else
				{
					// Set the mode to non-spatial
					m_eMode = kNonSpatialMode;

					// read the string from the stream if the object is not a spatial string
					std::string strTemp("");
					dataReader >> strTemp;
					
					// Use updateString to put the new string in place.
					updateString( strTemp );
				}
			}
			else
			{
				// Version 8 writes the enumerated spatial mode out as an unsigned long
				unsigned long uleMode = 0;
				dataReader >> uleMode;
				m_eMode = static_cast<ESpatialStringMode>( uleMode );

				// Version 8 writes the string every time no matter what
				dataReader >> m_strString;
			}
		}

		// Read forced rotation flag (no longer used)
		if (nDataVersion == 10)
		{
			bool bTemp;
			dataReader >> bTemp;
		}

		// version 5 has no fontsize or char confidence in the CppLetter
		if (nDataVersion == 5)
		{
			struct Version5Letter
			{
				unsigned short m_usGuess1;
				unsigned short m_usGuess2;
				unsigned short m_usGuess3;
				unsigned short m_usTop;
				unsigned short m_usLeft;
				unsigned short m_usRight;
				unsigned short m_usBottom;
				unsigned char m_ucPageNumber;
				bool m_bIsEndOfParagraph;
				bool m_bIsEndOfZone;
				bool m_bIsSpatial;
			};
			
			long nNumLetters = 0;

			dataReader >> nNumLetters;

			if (nNumLetters > 0)
			{
				ByteStream bs;

				bs.setSize(nNumLetters * sizeof(Version5Letter));
				dataReader.read(bs);

				Version5Letter* letters = (Version5Letter*)bs.getData();
				// convert the Version5letters into current CppLetters
				vector<CPPLetter> vecLetters;
				
				for (int i = 0; i < nNumLetters; i++)
				{
					CPPLetter letter;

					// Copy each element from Version5Letter to CPPLetter
					letter.m_usGuess1 = letters[i].m_usGuess1; 
					letter.m_usGuess2 = letters[i].m_usGuess2;
					letter.m_usGuess3 = letters[i].m_usGuess3;
					letter.m_usTop = letters[i].m_usTop;
					letter.m_usBottom = letters[i].m_usBottom;
					letter.m_usLeft = letters[i].m_usLeft;
					letter.m_usRight = letters[i].m_usRight;
					letter.m_usPageNumber = letters[i].m_ucPageNumber;
					letter.m_bIsEndOfParagraph = letters[i].m_bIsEndOfParagraph;
					letter.m_bIsEndOfZone = letters[i].m_bIsEndOfZone;
					letter.m_bIsSpatial = letters[i].m_bIsSpatial;

					// Copy default values for new fields
					letter.m_ucFontSize = 0;
					letter.m_ucCharConfidence = 100;
					letter.m_ucFont = 0;

					// Add this new CPP letter into the collection
					vecLetters.push_back(letter);
				}

				// Note: this call will build the string
				updateLetters(&vecLetters[0], nNumLetters);
			}
			else
			{
				updateString("");
			}
		}

		if (nDataVersion == 6)
		{
			struct Version6Letter
			{
				unsigned short m_usGuess1;
				unsigned short m_usGuess2;
				unsigned short m_usGuess3;
				unsigned short m_usTop;
				unsigned short m_usLeft;
				unsigned short m_usRight;
				unsigned short m_usBottom;
				unsigned char m_ucPageNumber;
				bool m_bIsEndOfParagraph;
				bool m_bIsEndOfZone;
				bool m_bIsSpatial;
				unsigned char m_ucFontSize;
				unsigned char m_ucCharConfidence;
			};

			long nNumLetters = 0;

			dataReader >> nNumLetters;

			if (nNumLetters > 0)
			{
				ByteStream bs;

				bs.setSize(nNumLetters*sizeof(Version6Letter));
				dataReader.read(bs);

				Version6Letter* letters = (Version6Letter*)bs.getData();
				// convert the Version6letters into current CppLetters
				vector<CPPLetter> vecLetters;
			
				for(int i = 0; i < nNumLetters; i++)
				{
					CPPLetter letter;

					// Copy each element from Version6Letter to CPPLetter
					letter.m_usGuess1 = letters[i].m_usGuess1; 
					letter.m_usGuess2 = letters[i].m_usGuess2;
					letter.m_usGuess3 = letters[i].m_usGuess3;
					letter.m_usTop = letters[i].m_usTop;
					letter.m_usBottom = letters[i].m_usBottom;
					letter.m_usLeft = letters[i].m_usLeft;
					letter.m_usRight = letters[i].m_usRight;
					letter.m_usPageNumber = letters[i].m_ucPageNumber;
					letter.m_bIsEndOfParagraph = letters[i].m_bIsEndOfParagraph;
					letter.m_bIsEndOfZone = letters[i].m_bIsEndOfZone;
					letter.m_bIsSpatial = letters[i].m_bIsSpatial;
					letter.m_ucFontSize = letters[i].m_ucFontSize;
					letter.m_ucCharConfidence = letters[i].m_ucCharConfidence;

					// Copy default value for new field
					letter.m_ucFont = 0;

					// Add this new CPP letter into the collection
					vecLetters.push_back(letter);
				}

				// Note: this call will build the string
				updateLetters(&vecLetters[0], nNumLetters);
			}
			else
			{
				updateString("");
			}
		}

		if (nDataVersion == 7 || nDataVersion == 8)
		{
			long nNumLetters = 0;

			if (nDataVersion == 7)
			{
				// nNumLetters is output even if not spatial for version 7
				dataReader >> nNumLetters;
			}

			if( m_eMode == kSpatialMode )
			{
				// If the version is 7, we've already read the number of letters. Do not read again.
				if( nDataVersion != 7 )
				{
					// nNumLetters is only output for spatial objects for version > 7
					dataReader >> nNumLetters;
				}

				if (nNumLetters > 0)
				{
					struct Version7n8Letter
					{
						unsigned short m_usGuess1;
						unsigned short m_usGuess2;
						unsigned short m_usGuess3;
						unsigned short m_usTop;
						unsigned short m_usLeft;
						unsigned short m_usRight;
						unsigned short m_usBottom;
						unsigned char m_ucPageNumber;
						bool m_bIsEndOfParagraph;
						bool m_bIsEndOfZone;
						bool m_bIsSpatial;
						unsigned char m_ucFontSize;
						unsigned char m_ucCharConfidence;
						unsigned char m_ucFont;
					};

					// Read the letter objects as one chunk
					ByteStream bs;
					bs.setSize(nNumLetters*sizeof(Version7n8Letter));
					dataReader.read(bs);

					// Convert the Version7n8Letters into current CppLetters
					Version7n8Letter* letters = (Version7n8Letter*)bs.getData();
					vector<CPPLetter> vecLetters;

					for (int i = 0; i < nNumLetters; i++)
					{
						CPPLetter letter;

						// Copy each element from Version7n8Letter to CPPLetter
						letter.m_usGuess1 = letters[i].m_usGuess1; 
						letter.m_usGuess2 = letters[i].m_usGuess2;
						letter.m_usGuess3 = letters[i].m_usGuess3;
						letter.m_usTop = letters[i].m_usTop;
						letter.m_usBottom = letters[i].m_usBottom;
						letter.m_usLeft = letters[i].m_usLeft;
						letter.m_usRight = letters[i].m_usRight;
						letter.m_usPageNumber = letters[i].m_ucPageNumber;
						letter.m_bIsEndOfParagraph = letters[i].m_bIsEndOfParagraph;
						letter.m_bIsEndOfZone = letters[i].m_bIsEndOfZone;
						letter.m_bIsSpatial = letters[i].m_bIsSpatial;
						letter.m_ucFontSize = letters[i].m_ucFontSize;
						letter.m_ucCharConfidence = letters[i].m_ucCharConfidence;
						letter.m_ucFont = letters[i].m_ucFont;

						// Add this new CPP letter into the collection
						vecLetters.push_back(letter);
					}

					// Note: this call will build the string
					updateLetters( &vecLetters[0], nNumLetters );
				}
			}
			else if( m_eMode == kHybridMode )
			{
				// Read the raster zone vector
				IPersistStreamPtr ipObj;
				::readObjectFromStream(ipObj, pStream, "ELI14802");

				IIUnknownVectorPtr ipVecRasterZones = ipObj;
				ASSERT_RESOURCE_ALLOCATION("ELI25789", ipVecRasterZones != NULL);

				long lSize = ipVecRasterZones->Size();
				m_vecRasterZones.reserve(lSize);
				for (long i=0; i < lSize; i++)
				{
					UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone = ipVecRasterZones->At(i);
					ASSERT_RESOURCE_ALLOCATION("ELI25790", ipZone != NULL);

					m_vecRasterZones.push_back(ipZone);
				}
			}
		}
		else if (nDataVersion >= 9)
		{
			long nNumLetters = 0;

			if (m_eMode == kSpatialMode)
			{
				// nNumLetters is only output for spatial objects for version > 7
				dataReader >> nNumLetters;

				if (nNumLetters > 0)
				{
					// Determine size of chunk of Letters objects
					ByteStream bs;
					bs.setSize( nNumLetters * sizeof(CPPLetter) );

					// Read the chunk of letters and hold as an array
					dataReader.read( bs );
					CPPLetter* letters = (CPPLetter*)bs.getData();

					// Note: this call will build the string
					updateLetters( letters, nNumLetters );
				}
			}
			else if (m_eMode == kHybridMode)
			{
				// Read and store the vector of raster zones
				IPersistStreamPtr ipObj;
				::readObjectFromStream(ipObj, pStream, "ELI15481");

				IIUnknownVectorPtr ipVecRasterZones = ipObj;
				ASSERT_RESOURCE_ALLOCATION("ELI25791", ipVecRasterZones != NULL);

				long lSize = ipVecRasterZones->Size();
				m_vecRasterZones.reserve(lSize);
				for (long i=0; i < lSize; i++)
				{
					UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone = ipVecRasterZones->At(i);
					ASSERT_RESOURCE_ALLOCATION("ELI25792", ipZone != NULL);

					m_vecRasterZones.push_back(ipZone);
				}
			}
		}

		// load the vector of letter objects if the object is a spatial string
		// NOTE: Versions 1 and 2 always wrote the letter objects, and the string
		// was always computed from the letter objects.
		// Beginning with version 3, the letter objects were written only if the
		// string was spatial.
		if (nDataVersion < 3 || (nDataVersion < 5 && m_eMode == kSpatialMode))
		{
			IPersistStreamPtr ipObj;

			::readObjectFromStream(ipObj, pStream, "ELI19468");

			IIUnknownVectorPtr ipLetters = ipObj;
			ASSERT_RESOURCE_ALLOCATION("ELI25793", ipLetters != NULL);

			// NOTE: if the loaded letter objects were actually not spatial 
			// (such as in versions 1 and 2), then this string will automatically
			// be downgraded to a non-spatial string in updateLetters
			
			vector<CPPLetter> vecLetters;
			long nSize = ipLetters->Size();
			vecLetters.reserve(nSize);
			for(long i = 0; i < nSize; i++)
			{
				UCLID_RASTERANDOCRMGMTLib::ILetterPtr ipLetter = ipLetters->At(i);
				ASSERT_RESOURCE_ALLOCATION("ELI25794", ipLetter != NULL);

				CPPLetter letter;
				ipLetter->GetCppLetter(&letter);
				vecLetters.push_back(letter);
			}

			if(vecLetters.size() > 0)
			{
				updateLetters(&vecLetters[0], vecLetters.size());
			}
			else
			{
				updateString("");
			}
		}

		if (nDataVersion >= 4 && (m_eMode == kSpatialMode || m_eMode == kHybridMode))
		{
			// Load the page info for spatial mode or hybrid mode
			IPersistStreamPtr ipObj;

			::readObjectFromStream(ipObj, pStream, "ELI09984");
			ASSERT_RESOURCE_ALLOCATION("ELI15279", ipObj != NULL);
			m_ipPageInfoMap = ipObj;
		}
		
		// after we're done loading the string, do a consistency check
		performConsistencyCheck();	

		if (nDataVersion < 4 && m_eMode == kSpatialMode)
		{
			// Fake all of the page info data
			m_ipPageInfoMap.CreateInstance(CLSID_LongToObjectMap);
			ASSERT_RESOURCE_ALLOCATION("ELI09163", m_ipPageInfoMap != NULL);

			long nCurrPage = -1;

			for (unsigned int i = 0; i < m_vecLetters.size(); i++)
			{
				CPPLetter& letter = m_vecLetters[i];

				if (letter.m_bIsSpatial)
				{
					long nPage = letter.m_usPageNumber;

					if (nPage != nCurrPage)
					{
						UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipPageInfo(CLSID_SpatialPageInfo);
						ASSERT_RESOURCE_ALLOCATION("ELI19466", ipPageInfo != NULL);
						ipPageInfo->SetPageInfo(0, 0,
							(UCLID_RASTERANDOCRMGMTLib::EOrientation)0, 0.0);

						m_ipPageInfoMap->Set(nPage, ipPageInfo);
						nCurrPage = nPage;
					}
				}
			}
		}
		
		// [FlexIDSCore:4006]
		// Hybrid SpatialStrings through version 7.0 (SpatialString version 11) are saved in image
		// coordinates not in OCR coordinates as is the case starting in 8.0 (SpatialString
		// version 12). To allow for backward compatibility, adjust the zones so that they are
		// stored in OCR coordinates if specified via registry setting.
		if (nDataVersion < 12 && m_eMode == kHybridMode)
		{
			// get registry persistance manager
			RegistryPersistenceMgr rpmRasterOCR(HKEY_CURRENT_USER, gstrREG_ROOT_KEY);

			bool bConvertLegacyHybridString = true;

			// check for the convert legacy strings key
			if (rpmRasterOCR.keyExists(gstrRASTER_OCR_SECTION, gstrCONVERT_LEGACY_STRINGS_KEY))
			{
				// key exists, so read from the registry
				string strConvertLegacyStringsKey = rpmRasterOCR.getKeyValue(
					gstrRASTER_OCR_SECTION, gstrCONVERT_LEGACY_STRINGS_KEY);

				// Is conversion enabled?
				bConvertLegacyHybridString = (strConvertLegacyStringsKey == "1");
			}

			// If conversion is turned on, perform the conversion.
			if (bConvertLegacyHybridString)
			{
				autoConvertLegacyHybridString();
			}
		}

		// Save the OCR engine version
		m_strOCREngineVersion = strOCREngineVersion;

		// clear the dirty flag as we just loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06650");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter( ByteStreamManipulator::kWrite, data );
		
		// Write the data version
		dataWriter << gnCurrentVersion;
		
		// Write the non-COM data members associated with this object

		// Write the source document name
		dataWriter << m_strSourceDocName;

		// Write the OCR Engine version
		dataWriter << m_strOCREngineVersion;
		
		// Write the current mode
		// Changed for Version 8. 
		//Version 7 or less will have m_bIsSpatial in this spot - 12/5/06 - RM
		unsigned long nm_eMode = m_eMode;
		dataWriter << nm_eMode;

		// Write the string to the stream 
		dataWriter << m_strString;

		ByteStream bs;

		// write the letters vector
		if (m_eMode == kSpatialMode)
		{
			// Configure the bytestream object to handle the letters vector
			unsigned long ulLetterSize = sizeof(CPPLetter);
			unsigned long ulNumLetters = m_vecLetters.size();

			// write the number of letters
			dataWriter << (long)m_vecLetters.size();

			if (ulNumLetters > 0)
			{
				long lSize = m_vecLetters.size() * sizeof(CPPLetter);

				// Set the size of the bytestream
				bs.setSize(lSize);

				memcpy_s(bs.getData(), lSize, &(m_vecLetters[0]), lSize);
				dataWriter.write(bs);
			}
		}

		// MUST flush the bytestream before writing out the COM object(s)
		// flush the bytestream
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();

		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// If this is a hybrid object write the raster zone vector
		if( m_eMode == kHybridMode )
		{
			IIUnknownVectorPtr ipZones(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI25795", ipZones != NULL);

			vector<UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr>::iterator it = m_vecRasterZones.begin();
			for(; it != m_vecRasterZones.end(); it++)
			{
				ipZones->PushBack((*it));
			}

			// Save the page information to the stream
			IPersistStreamPtr ipPIObj = ipZones;
			ASSERT_RESOURCE_ALLOCATION("ELI14780", ipPIObj != NULL);

			writeObjectToStream(ipPIObj, pStream, "ELI14781", fClearDirty);
		}

		// If this is a kSpatialMode or a kHybridMode object also save the page information
		if ( (m_eMode == kSpatialMode) || (m_eMode == kHybridMode) )
		{
			// Save the page information to the stream
			IPersistStreamPtr ipPIObj = getPageInfoMap();

			ASSERT_RESOURCE_ALLOCATION("ELI09127", ipPIObj != NULL);
			writeObjectToStream(ipPIObj, pStream, "ELI09939", fClearDirty);
		}

		// clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06651");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())	
	return E_NOTIMPL;
}
//-------------------------------------------------------------------------------------------------
