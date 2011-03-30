// SpatialContentBasedAS.cpp : Implementation of CSpatialContentBasedAS

#include "stdafx.h"
#include "SpatialContentBasedAS.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CSpatialContentBasedAS
//-------------------------------------------------------------------------------------------------
CSpatialContentBasedAS::CSpatialContentBasedAS()
:	m_bDirty(false),
	m_lConsecutiveRows(0),
	m_lMinPercent(0),
	m_lMaxPercent(0),
	m_bContains(true),
	m_ipImageUtils(NULL),
	m_bIncludeNonSpatial(false)
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialContentBasedAS::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ISpatialContentBasedAS,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_IPersistStream,
		&IID_ILicensedComponent,
		&IID_IAttributeSelector
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialContentBasedAS::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_SpatialContentBasedAS;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialContentBasedAS::IsDirty()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialContentBasedAS::Load(IStream * pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

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
			UCLIDException ue( "ELI13339", 
				"Unable to load newer Spatial Content Based Attribute Selector" );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		// load data here
		dataReader >> m_bContains;
		dataReader >> m_lConsecutiveRows;
		dataReader >> m_lMinPercent;
		dataReader >> m_lMaxPercent;
		dataReader >> m_bIncludeNonSpatial;

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13268");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialContentBasedAS::Save(IStream * pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())	
	
	try
	{
		// validate license
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter( ByteStreamManipulator::kWrite, data );
		dataWriter << gnCurrentVersion;
		dataWriter << m_bContains;
		dataWriter << m_lConsecutiveRows;
		dataWriter << m_lMinPercent;
		dataWriter << m_lMaxPercent;
		dataWriter << m_bIncludeNonSpatial;
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13276");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialContentBasedAS::GetSizeMax(_ULARGE_INTEGER * pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())	
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IAttributeSelector Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialContentBasedAS::raw_SelectAttributes(IIUnknownVector * pAttrIn, IAFDocument * pAFDoc, IIUnknownVector * * pAttrOut)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())	
	
	try
	{
		// validate license
		validateLicense();

		IIUnknownVectorPtr ipIn( pAttrIn);
		IIUnknownVectorPtr ipFound(CLSID_IUnknownVector);

		selectMatchingAttrs ( ipIn, ipFound );
		
		CComQIPtr<IIUnknownVector> ipOut(ipFound);
		ipOut.CopyTo(pAttrOut);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13278");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialContentBasedAS::raw_IsConfigured(VARIANT_BOOL * bConfigured)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())	

	try
	{
		bool bCfg = m_lConsecutiveRows > 0;
		bCfg = bCfg && (m_lMinPercent <= m_lMaxPercent);
		bCfg = bCfg && (m_lMaxPercent <= 100);

		*bConfigured = bCfg ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13350");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialContentBasedAS::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())	
	
	try
	{
		ASSERT_ARGUMENT("ELI19632", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Spatial content attribute selector").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13279");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialContentBasedAS::raw_Clone(IUnknown * * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())	
	
	try
	{
		// validate license
		validateLicense();

		// create a new instance of the EntityNameDataScorer
		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_SpatialContentBasedAS);
		ASSERT_RESOURCE_ALLOCATION("ELI13282", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13280");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialContentBasedAS::raw_CopyFrom(IUnknown * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())	
	
	try
	{
		// validate license
		validateLicense();
		UCLID_AFSELECTORSLib::ISpatialContentBasedASPtr ipFrom(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI13338", ipFrom != __nullptr );

		m_bContains = ipFrom->Contains == VARIANT_TRUE;
		m_lConsecutiveRows = ipFrom->ConsecutiveRows;
		m_lMinPercent = ipFrom->MinPercent;
		m_lMaxPercent = ipFrom->MaxPercent;
		m_bIncludeNonSpatial = ipFrom->IncludeNonSpatial == VARIANT_TRUE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13281");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialContentBasedAS::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// ISpatialContentBasedAS Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialContentBasedAS::get_ConsecutiveRows(long* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		*pVal = m_lConsecutiveRows;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13330");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialContentBasedAS::put_ConsecutiveRows(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();
		
		m_lConsecutiveRows = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13331");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialContentBasedAS::get_MinPercent(long* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		*pVal = m_lMinPercent;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13332");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialContentBasedAS::put_MinPercent(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		m_lMinPercent = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13333");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialContentBasedAS::get_MaxPercent(long* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		*pVal = m_lMaxPercent;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13334");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialContentBasedAS::put_MaxPercent(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		m_lMaxPercent = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13335");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialContentBasedAS::get_Contains(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		*pVal = (m_bContains) ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13336");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialContentBasedAS::put_Contains(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		m_bContains = (newVal == VARIANT_TRUE);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13337");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialContentBasedAS::get_IncludeNonSpatial(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		*pVal = (m_bIncludeNonSpatial) ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13344");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialContentBasedAS::put_IncludeNonSpatial(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		m_bIncludeNonSpatial = (newVal == VARIANT_TRUE);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13345");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CSpatialContentBasedAS::validateLicense()
{
	static const unsigned long SPATIAL_CONTENT_BASED_AS_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( SPATIAL_CONTENT_BASED_AS_ID, "ELI13357", 
		"Spatial Content Based Attribute Selector" );
}
//-------------------------------------------------------------------------------------------------
IImageUtilsPtr CSpatialContentBasedAS::getImageUtils()
{
	if ( m_ipImageUtils == __nullptr )
	{
		m_ipImageUtils.CreateInstance(CLSID_ImageUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI13340", m_ipImageUtils != __nullptr );
	}
	return m_ipImageUtils;
}
//-------------------------------------------------------------------------------------------------
void CSpatialContentBasedAS::selectMatchingAttrs(IIUnknownVectorPtr ipAttributes, 
												 IIUnknownVectorPtr ipSelected)
{
	ASSERT_ARGUMENT("ELI13342", ipSelected != __nullptr );

	if ( ipAttributes != __nullptr )
	{
		long lAttributesSize = ipAttributes->Size();
		for (long i = 0; i < lAttributesSize; i++)
		{
			bool bProcessSubs = false;

			// Retrieve this Attribute
			IAttributePtr ipAttribute = ipAttributes->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI13341", ipAttribute != __nullptr );

			// Retrieve the associated Spatial String
			ISpatialStringPtr ipValue = ipAttribute->Value;
			ASSERT_RESOURCE_ALLOCATION("ELI15838", ipValue != __nullptr );

			// This Attribute contains spatial information
			if ( ipValue->HasSpatialInfo() == VARIANT_TRUE )
			{
				// Retrieve the collected raster zones
				IIUnknownVectorPtr ipRasterZones = ipValue->GetOriginalImageRasterZones();
				ASSERT_RESOURCE_ALLOCATION("ELI15842", ipRasterZones != __nullptr );

				// Process each raster zone
				long lSize = ipRasterZones->Size();
				for ( long z = 0; z < lSize; z ++ )
				{
					// Retrieve this raster zone
					IRasterZonePtr ipZone = ipRasterZones->At(z);
					ASSERT_RESOURCE_ALLOCATION("ELI13343", ipZone != __nullptr );

					// Get the image statistics for this raster zone
					IImageStatsPtr ipImageStats = getImageUtils()->GetImageStats(
						ipValue->SourceDocName, ipZone );
					ASSERT_RESOURCE_ALLOCATION("ELI13426", ipImageStats != __nullptr );

					// Check for "text" contained within the zone
					if ( getImageUtils()->IsTextInZone( ipImageStats, m_lConsecutiveRows, 
						m_lMinPercent, m_lMaxPercent ) == VARIANT_TRUE )
					{
						// Save this Attribute
						if ( m_bContains )
						{
							ipSelected->PushBack( ipAttribute );

							// Also process the sub-attributes, if any exist
							bProcessSubs = true;
							break;
						}
					}
					// No "text" is contained within the zone
					else
					{
						// Save this Attribute
						if ( !m_bContains )
						{
							ipSelected->PushBack( ipAttribute );

							// Also process the sub-attributes, if any exist
							bProcessSubs = true;
							break;
						}
					}
				}
			}
			// This Attribute does not contain spatial information
			else
			{
				if ( m_bIncludeNonSpatial )
				{
					// Save this Attribute
					ipSelected->PushBack( ipAttribute );

					// Also process the sub-attributes, if any exist
					bProcessSubs = true;
				}
			}

			// Do sub-attributes need to be processed
			if (bProcessSubs)
			{
				// Retrieve the sub-attributes
				IIUnknownVectorPtr ipSubs = ipAttribute->SubAttributes;
				ASSERT_RESOURCE_ALLOCATION("ELI15843", ipSubs != __nullptr );
				if (ipSubs->Size() > 0)
				{
					// Create collection for filtered sub-attributes
					IIUnknownVectorPtr ipFiltered( CLSID_IUnknownVector );
					ASSERT_RESOURCE_ALLOCATION("ELI15844", ipFiltered != __nullptr );

					// Process the sub-attributes
					selectMatchingAttrs( ipAttribute->SubAttributes, ipFiltered );

					// Overwrite the original sub-attributes with the 
					// newly filtered sub-attributes
					ipAttribute->SubAttributes = ipFiltered;
				}
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
