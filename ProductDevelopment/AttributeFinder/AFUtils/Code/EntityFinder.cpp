// EntityFinder.cpp : Implementation of CEntityFinder
#include "stdafx.h"
#include "AFUtils.h"
#include "EntityFinder.h"

#include <UCLIDException.h>
#include <RegistryPersistenceMgr.h>
#include <ByteStreamManipulator.h>
#include <cpputil.h>
#include <comutils.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Version 2: Added CIdentifiableObject
const unsigned long gnCurrentVersion = 2;

// constants moved to the header file so that they would be accessible to both
// EntityFinder and EntityFinder_Internal
//const string gstrAFUTILS_KEY_PATH = gstrAF_REG_ROOT_FOLDER_PATH + string("\\AFUtils");

//-------------------------------------------------------------------------------------------------
// CEntityFinder
//-------------------------------------------------------------------------------------------------
CEntityFinder::CEntityFinder()
: m_bLoggingEnabled(false)
, m_cachedRegExLoader()
{
	try
	{
		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI13036", m_ipMiscUtils != __nullptr );

		// Instantiate the Entity Keywords object
		m_ipKeys.CreateInstance( CLSID_EntityKeywords );
		ASSERT_RESOURCE_ALLOCATION( "ELI06027", m_ipKeys != __nullptr );

		// Create pointer to Registry Persistence Manager
		ma_pUserCfgMgr = unique_ptr<IConfigurationSettingsPersistenceMgr>(
			new RegistryPersistenceMgr( HKEY_CURRENT_USER, gstrAFUTILS_KEY_PATH ) );
		ASSERT_RESOURCE_ALLOCATION( "ELI06162", ma_pUserCfgMgr.get() != __nullptr );

		// Create pointer to Entity Finder settings
		ma_pEFConfigMgr = unique_ptr<EntityFinderConfigMgr>(
			new EntityFinderConfigMgr( ma_pUserCfgMgr.get(), "\\EntityFinder" ) );
		ASSERT_RESOURCE_ALLOCATION( "ELI06163", ma_pEFConfigMgr.get() != __nullptr );

		// Check logging flag
		m_bLoggingEnabled = (ma_pEFConfigMgr->getLoggingEnabled() > 0) ? true : false;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI05953");
}
//-------------------------------------------------------------------------------------------------
CEntityFinder::~CEntityFinder()
{
	try
	{
		m_ipKeys = __nullptr;
		m_ipMiscUtils = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI29442");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityFinder::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IEntityFinder,
		&IID_IAttributeModifyingRule,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_ILicensedComponent,
		&IID_IIdentifiableObject,
		&IID_IPersistStream
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IEntityFinder
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityFinder::FindEntities(ISpatialString* pText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// Check licensing
		validateLicense();

		// This is such an obscure method (only used by MERS modifier)
		// that it doesn't seem worth it to ensure that a specific FKB is used.
		// This will result in the use of the entity finder from the Latest FKB
		// https://extract.atlassian.net/browse/ISSUE-15361
		IAFDocumentPtr ipAFDoc(CLSID_AFDocument);
		ASSERT_RESOURCE_ALLOCATION( "ELI45674", ipAFDoc != __nullptr);
		m_ipKeys->Init(ipAFDoc);

		// Copy text to local SpatialString and string
		ISpatialStringPtr ipInputText( pText );
		ASSERT_RESOURCE_ALLOCATION( "ELI09399", ipInputText != __nullptr );

		findEntities(ipAFDoc, ipInputText);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05879")
		
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityFinder::FindEntitiesInAttributes(IIUnknownVector *pAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		// This method is called only by the grantor grantee finder, which is obsolete
		UCLIDException ue("ELI45671", "This method is obsolete");
		throw ue;

		// make sure the vec of attributes are not null
		IIUnknownVectorPtr ipAttributes(pAttributes);
		ASSERT_ARGUMENT("ELI07081", ipAttributes != __nullptr);

		// go through all attributes in the vec
		long nSize = ipAttributes->Size();
		for (long n=0; n<nSize; n++)
		{
			IAttributePtr ipAttr = ipAttributes->At(n);
			// make sure the vector contains IAttribute
			if (ipAttr == __nullptr)
			{
				throw UCLIDException("ELI07082", "The IUnknownVector shall contain objects of IAttribute.");
			}

			// get spatial string from each attribute
			ISpatialStringPtr ipSpatialString = ipAttr->Value;
			ASSERT_RESOURCE_ALLOCATION("ELI26596", ipSpatialString != __nullptr);

			// find proper entity from the string
			findEntities(__nullptr, ipSpatialString);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07080")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityFinder::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// IAttributeModifyingRule
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityFinder::raw_ModifyValue(IAttribute * pAttribute, IAFDocument* pOriginInput,
											IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		IAttributePtr	ipAttribute(pAttribute);
		ASSERT_RESOURCE_ALLOCATION( "ELI09297", ipAttribute != __nullptr );

		ISpatialStringPtr ipInputText = ipAttribute->Value;
		ASSERT_RESOURCE_ALLOCATION( "ELI09298", ipInputText != __nullptr);

		// Initialize the keywords object (loads lists from component data)
		IAFDocumentPtr ipAFDoc(pOriginInput);
		ASSERT_RESOURCE_ALLOCATION( "ELI45672", ipAFDoc != __nullptr);
		m_ipKeys->Init(ipAFDoc);

		findEntities(ipAFDoc, ipInputText);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07064");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityFinder::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19569", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Find company or person(s)").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07065");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityFinder::raw_CopyFrom(IUnknown * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08242");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityFinder::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Create a new EntityFinder object
		ICopyableObjectPtr ipObjCopy(CLSID_EntityFinder);
		ASSERT_RESOURCE_ALLOCATION("ELI08341", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07068");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityFinder::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_EntityFinder;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityFinder::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityFinder::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
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
			UCLIDException ue( "ELI07642", "Unable to load newer EntityFinder." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if (nDataVersion >= 2)
		{
			// Load the GUID for the IIdentifiableObject interface.
			loadGUID(pStream);
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07069");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityFinder::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// Save the GUID for the IIdentifiableObject interface.
		saveGUID(pStream);

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07070");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityFinder::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IIdentifiableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityFinder::get_InstanceGUID(GUID *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = getGUID();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33520")
}