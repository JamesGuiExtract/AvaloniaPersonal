#include "stdafx.h"
#include "CountyCustomComponents.h"
#include "GrantorGranteeFinderV2.h"
#include "DatFileIterator.h"

// the file contains specific tag names for AFDocument
#include <SpecialStringDefinitions.h>
#include <CommentedTextFileReader.h>
#include <common.h>
#include <ComponentLicenseIDs.h>
#include <COMUtils.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <misc.h>
#include <RegistryPersistenceMgr.h>
#include <stringCSIS.h>
#include <UCLIDException.h>

#include <cstdlib>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 4;

// GrantorGranteeFinder folder under ComponentData folder
const static string GRANTOR_GRANTEE_FINDER = "GrantorGranteeFinder";

// Section Folder
const string CGrantorGranteeFinderV2::GGFINDERS_SECTIONNAME = "\\GrantorGranteeFinderV2";

// Key Name
const string CGrantorGranteeFinderV2::DOCTYPE_STORERULES = "StoreRulesWorked";
const string CGrantorGranteeFinderV2::DEFAULT_DOCTYPE_STORERULES = "0";

const string gstrCOUNTY_CUSTOM_COMPONENTS_KEY = gstrAF_REG_ROOT_FOLDER_PATH + string("\\IndustrySpecific\\County\\CountyCustomComponents");

//-------------------------------------------------------------------------------------------------
// CGrantorGranteeFinderV2
//-------------------------------------------------------------------------------------------------
CGrantorGranteeFinderV2::CGrantorGranteeFinderV2()
: m_ipEntityFinder(NULL),
  m_ipDocPreprocessor(NULL),
  m_ipAFUtility(NULL),
  m_bUseSelectedDatFiles(false),
  m_ipDocTypeToFileMap(NULL),
  m_bDocTypeToFileMapLoaded(false),
  m_bValidDocTypesLoaded(false)
{
	try
	{
		// Instantiate the settings object
		ma_pUserCfgMgr = unique_ptr<IConfigurationSettingsPersistenceMgr>(
			new RegistryPersistenceMgr( HKEY_CURRENT_USER, gstrCOUNTY_CUSTOM_COMPONENTS_KEY));
		ASSERT_RESOURCE_ALLOCATION( "ELI09217", ma_pUserCfgMgr.get() != __nullptr );
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI09218")
}
//-------------------------------------------------------------------------------------------------
CGrantorGranteeFinderV2::~CGrantorGranteeFinderV2()
{
	try
	{
		m_mapStringToVecSPMFinders.clear();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16433");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CGrantorGranteeFinderV2::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IAttributeFindingRule,
		&IID_IPersistStream,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_ILicensedComponent
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IAttributeFindingRule
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CGrantorGranteeFinderV2::raw_ParseText(IAFDocument* pAFDoc, IProgressStatus *pProgressStatus,
													IIUnknownVector **pAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// This finder is obsolete so throw exception if this method is called
		UCLIDException ue("ELI28705", "Grantor-Grantee finder is obsolete.");
		throw ue;

		validateLicense();

		// ensure pre-requisites
		ASSERT_ARGUMENT("ELI09219", pAFDoc != __nullptr);

		// first process the AFDocument to get proper tags for the document
		IAFDocumentPtr ipAFDoc(pAFDoc);
		processAFDcoument(ipAFDoc);

		// given that the document has been processed, by this time,
		// the string tags collection MUST ABSOLUTELY contain 
		// the DOC PROBABILITY tag, otherwise something's wrong with
		// our logic.
		IStrToStrMapPtr ipStringTags(ipAFDoc->StringTags);
		ASSERT_RESOURCE_ALLOCATION("ELI09220", ipStringTags != __nullptr);
		if (ipStringTags->Contains(DOC_PROBABILITY.c_str()) == VARIANT_FALSE)
		{
			// something wrong in our program logic
			THROW_LOGIC_ERROR_EXCEPTION("ELI09221");
		}

		// get the spatial string from the AFDocument
		ISpatialStringPtr ipInputText = ipAFDoc->Text;
		ASSERT_RESOURCE_ALLOCATION("ELI09222", ipInputText != __nullptr);

		// the vector for IAttribute
		IIUnknownVectorPtr ipAttributes(NULL);

		string strDocType("");
		// if the doc type is not unique, simply return without 
		// further processing
		if (!getDocType(ipAFDoc, strDocType))
		{
			// create an empty vector
			ipAttributes.CreateInstance(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI09671", ipAttributes != __nullptr);
			*pAttributes = ipAttributes.Detach();
			return S_OK;
		}

		// Check for and extract document sub-type
		string strDocSubType("");
		bool bSubType = getDocSubType( ipAFDoc, strDocSubType );

		///////////////////////////////////////
		// Search for optional GGMaster.rsd.etf file inside the DocType folder (P16 #1301)
		///////////////////////////////////////

		// Build name of file
		string strRSDFile;

		// Check inside sub-type folder, if present
		if (bSubType)
		{
			// Look for file within sub-folder
			strRSDFile = getRulesFolder() + "\\" + GRANTOR_GRANTEE_FINDER + "\\" + 
				strDocType + "\\" + strDocSubType + "\\" + "GGMaster.rsd.etf";

			if (!isFileOrFolderValid( strRSDFile.c_str() ))
			{
				// File not found, build path to main folder
				strRSDFile = getRulesFolder() + "\\" + GRANTOR_GRANTEE_FINDER + "\\" + 
					strDocType + "\\" + "GGMaster.rsd.etf";
			}
		}
		else
		{
			strRSDFile = getRulesFolder() + "\\" + GRANTOR_GRANTEE_FINDER + "\\" + 
				strDocType + "\\" + "GGMaster.rsd.etf";
		}

		// Check for presence of file
		autoEncryptFile( strRSDFile, gstrAF_AUTO_ENCRYPT_KEY_PATH.c_str() );
		if (isFileOrFolderValid( strRSDFile.c_str() ))
		{
			// Load the Rule Set object
			IRuleSetPtr ipRuleSet( CLSID_RuleSet );
			ASSERT_RESOURCE_ALLOCATION( "ELI10503", ipRuleSet != __nullptr );
			ipRuleSet->LoadFrom( _bstr_t( strRSDFile.c_str() ), VARIANT_FALSE );

			// Exercise the rules - for all attributes
			ipAttributes = ipRuleSet->ExecuteRulesOnText( ipAFDoc, NULL, NULL );
		}

		// Get the finders for this doc type
		vector<ISPMFinderPtr>& vecFinders = getDocTypeSPMFinders( strDocType, strDocSubType );

		// Find the attributes for each of the finders
		int nNumFinders = vecFinders.size();
		for ( int i = 0; i < nNumFinders; i++ )
		{
			// Get the finder
			IAttributeFindingRulePtr ipFinder = vecFinders[i];
			ASSERT_RESOURCE_ALLOCATION("ELI09252", ipFinder != __nullptr );

			// Get the attributes
			IIUnknownVectorPtr ipTempAttrs = ipFinder->ParseText(ipAFDoc, NULL);
			ASSERT_RESOURCE_ALLOCATION("ELI09251", ipTempAttrs != __nullptr );
			
			// Add the found attributes to the list of all found attributes
			if ( ipAttributes == __nullptr )
			{
				// if ipTempAttrs is NULL ipAttributes will still be NULL and handled on the next pass
				ipAttributes = ipTempAttrs;
			}
			else
			{
				ipAttributes->Append( ipTempAttrs );
			}
		}

		// If any attributes were found apply the modifiers
		if (ipAttributes)
		{
			// find entities for each attribute
			getEntityFinder()->FindEntitiesInAttributes(ipAttributes);
		}

		if (ipAttributes == __nullptr)
		{
			// create an empty vector
			ipAttributes.CreateInstance(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI09224", ipAttributes != __nullptr);
		}
		else
		{
			m_MERSFinder.findMERS( ipAttributes, ipAFDoc );
		}

		*pAttributes = ipAttributes.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09226");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IGrantorGranteeFinderV2
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CGrantorGranteeFinderV2::get_UseSelectedDatFiles(/*[out, retval]*/ BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*pVal = m_bUseSelectedDatFiles ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09314");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CGrantorGranteeFinderV2::put_UseSelectedDatFiles(/*[in]*/ BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		m_bUseSelectedDatFiles = newVal == VARIANT_TRUE;
		if ( !m_bUseSelectedDatFiles)
		{
			// If not selected dat files then the DocTypeToFileMap should have all files
			initDocTypeToFileMap();
		}
		m_bDirty = true;

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09313");
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CGrantorGranteeFinderV2::get_DocTypeToFileMap(/*[out, retval]*/ IStrToObjectMap* *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		// if value is null create it and return the new value
		if ( m_ipDocTypeToFileMap == __nullptr )
		{
			m_ipDocTypeToFileMap.CreateInstance(CLSID_StrToObjectMap);
			ASSERT_RESOURCE_ALLOCATION("ELI09322", m_ipDocTypeToFileMap != __nullptr );
		}
		IStrToObjectMapPtr ipShallowCopy = m_ipDocTypeToFileMap;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09317");
	return S_OK;

}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CGrantorGranteeFinderV2::put_DocTypeToFileMap(/*[in]*/ IStrToObjectMap *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_ipDocTypeToFileMap = newVal;
		
		// Map of SPMFinders is now invalid
		m_mapStringToVecSPMFinders.clear();
		
		m_bDocTypeToFileMapLoaded = true;
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19361");
	return S_OK;

}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CGrantorGranteeFinderV2::GetAFUtility(IAFUtility **ppAFUtil)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		IAFUtilityPtr ipShallowCopy = getAFUtility();
		*ppAFUtil = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09230");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CGrantorGranteeFinderV2::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19609", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Grantor-Grantee finder version 2").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12862");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CGrantorGranteeFinderV2::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CGrantorGranteeFinderV2::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		// Copy GrantorGranteeFinderV2 objects
		UCLID_COUNTYCUSTOMCOMPONENTSLib::IGrantorGranteeFinderV2Ptr ipSourceV2(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI09324", ipSourceV2 != __nullptr );

		ICopyableObjectPtr ipCopyableObj;
		ipCopyableObj = ipSourceV2->DocTypeToFileMap;
		ASSERT_RESOURCE_ALLOCATION("ELI09323", ipCopyableObj != __nullptr );

		// create clone of list and copy it
		m_ipDocTypeToFileMap = ipCopyableObj->Clone();
		ASSERT_RESOURCE_ALLOCATION("ELI09325", m_ipDocTypeToFileMap != __nullptr );
		// set the value of UseSelectedDatFiles
		m_bUseSelectedDatFiles = ipSourceV2->UseSelectedDatFiles == VARIANT_TRUE;
		
		// If UseSelectedDatFiles is true then the m_ipDocTypeToFileMap will
		// contain the files for each doc type if it is false
		// the All option will require the files be determined
		m_bDocTypeToFileMapLoaded = m_bUseSelectedDatFiles;

		// Map of SPMFinders is invalid
		m_mapStringToVecSPMFinders.clear();

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09234");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CGrantorGranteeFinderV2::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_GrantorGranteeFinderV2);
		ASSERT_RESOURCE_ALLOCATION("ELI09235", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09236");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CGrantorGranteeFinderV2::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*pClassID = CLSID_GrantorGranteeFinderV2;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12863");
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CGrantorGranteeFinderV2::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12864");

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CGrantorGranteeFinderV2::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();
		
		m_bUseSelectedDatFiles = false;
		m_ipDocTypeToFileMap = __nullptr;
		m_mapStringToVecSPMFinders.clear();

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
			UCLIDException ue( "ELI09237", "Unable to load newer Grantor-Grantee Finder ver 2." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if ( nDataVersion >= 2)
		{
			dataReader >> m_bUseSelectedDatFiles;
		}

		if (nDataVersion >= 1 && nDataVersion <= 3)
		{
			// read the list of preprocessors
			IPersistStreamPtr ipObj;
			::readObjectFromStream(ipObj, pStream, "ELI09974");
			ASSERT_RESOURCE_ALLOCATION("ELI09238", ipObj != __nullptr);
			// ignore the pre-processors...it is no longer used by the
			// current version of the object file anyway.
		}

		// Only load if m_bUsesSelectedDatFiles is true
		if ( nDataVersion >= 2 && m_bUseSelectedDatFiles )
		{
			// Save list of Selected dat files to use for doctypes
			IPersistStreamPtr ipObj;
			::readObjectFromStream(ipObj, pStream, "ELI09975");
			ASSERT_RESOURCE_ALLOCATION("ELI09353", ipObj != __nullptr);
			m_ipDocTypeToFileMap = ipObj;
			
			removeInvalidDocMappings();
		}

		// if UseSelectedDatFiles is false load all Dat files
		if ( !m_bUseSelectedDatFiles )
		{
			initDocTypeToFileMap();
		}
		m_bDocTypeToFileMapLoaded = true;

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09239");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CGrantorGranteeFinderV2::Save(IStream *pStream, BOOL fClearDirty)
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
		dataWriter << m_bUseSelectedDatFiles;

		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// only save if UseSelectedDatFiles is true
		if ( m_bUseSelectedDatFiles )
		{
			IPersistStreamPtr ipObj = m_ipDocTypeToFileMap;
			
			if ( ipObj == __nullptr )
			{
				throw UCLIDException("ELI09352", "StrToObjectMap object does not support persistence.");
			}
			
			writeObjectToStream(ipObj, pStream, "ELI09930", fClearDirty);
		}
		
		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09241");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CGrantorGranteeFinderV2::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
IAFUtilityPtr CGrantorGranteeFinderV2::getAFUtility()
{
	if (m_ipAFUtility == __nullptr)
	{
		m_ipAFUtility.CreateInstance(CLSID_AFUtility);
		ASSERT_RESOURCE_ALLOCATION("ELI09242", m_ipAFUtility != __nullptr);
	}

	return m_ipAFUtility;
}
//-------------------------------------------------------------------------------------------------
IEntityFinderPtr CGrantorGranteeFinderV2::getEntityFinder()
{
	if (m_ipEntityFinder == __nullptr)
	{
		m_ipEntityFinder.CreateInstance(CLSID_EntityFinder);
		ASSERT_RESOURCE_ALLOCATION("ELI09243", m_ipEntityFinder != __nullptr);
	}

	return m_ipEntityFinder;
}
//-------------------------------------------------------------------------------------------------
string CGrantorGranteeFinderV2::getRulesFolder()
{
	// get component data folder
	string strRulesFolder = getAFUtility()->GetComponentDataFolder();

	return strRulesFolder;
}
//-------------------------------------------------------------------------------------------------
void CGrantorGranteeFinderV2::processAFDcoument(IAFDocumentPtr ipAFDoc)
{
	// first check to see if the document has already been processed
	IStrToStrMapPtr ipStringTags(ipAFDoc->StringTags);
	// if string tags doesn't contain "DocProbability", 
	// then the AFDoc needs to be processed
	if (ipStringTags->Size == 0 
		||ipStringTags->Contains(_bstr_t(DOC_PROBABILITY.c_str())) == VARIANT_FALSE)
	{
		// use county document classifier to distinguish the document type
		if (m_ipDocPreprocessor == __nullptr)
		{
			m_ipDocPreprocessor.CreateInstance(CLSID_DocumentClassifier);
			ASSERT_RESOURCE_ALLOCATION("ELI09247", m_ipDocPreprocessor != __nullptr);
			// set industry category name
			IDocumentClassifierPtr ipDocClassifier(m_ipDocPreprocessor);
			ipDocClassifier->IndustryCategoryName = "County Document";
		}
		m_ipDocPreprocessor->Process(ipAFDoc, NULL);
	}
}
//-------------------------------------------------------------------------------------------------
bool CGrantorGranteeFinderV2::storeRulesWorked()
{
	// Check key existence
	if (!ma_pUserCfgMgr->keyExists(GGFINDERS_SECTIONNAME, DOCTYPE_STORERULES))
	{
		// Default setting is OFF
		string strStore(DEFAULT_DOCTYPE_STORERULES);
		ma_pUserCfgMgr->createKey( GGFINDERS_SECTIONNAME, DOCTYPE_STORERULES, strStore );

		return (strStore == "1");
	}

	return ma_pUserCfgMgr->getKeyValue(GGFINDERS_SECTIONNAME, DOCTYPE_STORERULES,
		DEFAULT_DOCTYPE_STORERULES) == "1";
}
//-------------------------------------------------------------------------------------------------
void CGrantorGranteeFinderV2::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFLEXINDEX_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI09248", "Grantor-Grantee Finding Rule V2");
}
//-------------------------------------------------------------------------------------------------
vector<ISPMFinderPtr> CGrantorGranteeFinderV2::getDocTypeSPMFinders(const string& strDocType, 
																	const string& strDocSubType)
{
	// Build the doc type in question as "strDocType\strDocSubType"
	stringCSIS strTest ( strDocType, false );
	stringCSIS strISDocType (strDocType, false);

	if (strDocSubType.length() > 0)
	{
		strTest += "\\";
		strTest += strDocSubType;
	}

	// Make sure that map has been loaded
	if (!m_bDocTypeToFileMapLoaded)
	{
		// if it hasn't been loaded by here do the default of all files
		initDocTypeToFileMap();
	}

	// if doc type has been used before return existing SPMFinder
	map<stringCSIS, vector<ISPMFinderPtr> >::iterator iterCurr = m_mapStringToVecSPMFinders.end();

	iterCurr = m_mapStringToVecSPMFinders.find( strTest );
	
	if (iterCurr != m_mapStringToVecSPMFinders.end())
	{
		if (((*iterCurr).second).size() > 0)
		{
			return (*iterCurr).second;
		}
		// No SPM Finders for this sub-type, move to the main type
		else if (((*iterCurr).second).size() == 0)
		{
			iterCurr = m_mapStringToVecSPMFinders.find( strISDocType );

			if (iterCurr != m_mapStringToVecSPMFinders.end())
			{
				return (*iterCurr).second;
			}
		}
	}

	// Check doc type with sub-type then without sub-type, if necessary
	bool bInMap = false;
	string strTemp( strTest );
	if ( m_ipDocTypeToFileMap->Contains( strTemp.c_str() ) == VARIANT_TRUE )
	{
		bInMap = true;
	}
	else
	{
		strTemp = strDocType;
		if ( m_ipDocTypeToFileMap->Contains( strTemp.c_str() ) == VARIANT_TRUE )
		{
			bInMap = true;
		}
	}

//	if (!m_bDocTypeToFileMapLoaded)
//	{
//		// if it hasn't been loaded by here do the default of all files
//		initDocTypeToFileMap();
//	}
	// Load new SPMFinder if doc type is in file list
	if (bInMap)
	{
		// Get List of files for the document type
		IVariantVectorPtr ipDatFiles = m_ipDocTypeToFileMap->GetValue( strTemp.c_str() );
		ASSERT_RESOURCE_ALLOCATION("ELI09360", ipDatFiles != __nullptr );

		long nNumDatFiles = ipDatFiles->Size;
		// 11/1/07 SNK [P16:2499] Removed "ELI09263","No Rules Files Found." 
		// m_ipDocTypeToFileMap represents the user-configured settings.  If the map is empty,
		// it means no rules were selected by the user, not that the files are missing.  Missing
		// files are reported via ELI 07474 as the rule is loaded.

		// Create list of SPMFinders for doc type
		vector<ISPMFinderPtr> vecFinders;
		for ( int i = 0; i < nNumDatFiles; i++ )
		{
			// Create SPMFinder
			ISPMFinderPtr ipSPMFinder(CLSID_SPMFinder );
			ASSERT_RESOURCE_ALLOCATION("ELI09249", ipSPMFinder != __nullptr );
	
			// Get file Name for dat file list
			string strFileName = asString(_bstr_t(ipDatFiles->GetItem(i)));

			// Build name of dat file
			string strRuleFile = getRulesFolder() + "\\" + GRANTOR_GRANTEE_FINDER + "\\" + 
				strTemp + "\\" + strFileName + ".dat.etf";

			// If the rules file doesn't exist then ipSPMFinder->RulesFileName
			// will throw an exception.  In that case we just want to eat(log) the exception 
			// and continue processing
			try
			{
				try
				{
					// Set rules file name in SPM Finder
					ipSPMFinder->RulesFileName = strRuleFile.c_str();
				}
				CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI10545");
			}
			catch(UCLIDException ue)
			{
				ue.log();
				continue;
			}

			// set properties
			bool bStoreRules = storeRulesWorked();
			ipSPMFinder->StoreRuleWorked = bStoreRules ? VARIANT_TRUE : VARIANT_FALSE;
			if (bStoreRules)
			{
				// Create different name for each dat file used
				string strRuleWorked = "RuleWorked" + asString(i);
				ipSPMFinder->RuleWorkedName = strRuleWorked.c_str();
			}
			ipSPMFinder->IsPatternsFromFile = VARIANT_TRUE;
			
			// enable using of the entity name data scorer with
			// find-best-match and a minimum score of 5.
			IObjectWithDescriptionPtr ipDataScorerObjWithDesc = 
				ipSPMFinder->DataScorer;
			ASSERT_RESOURCE_ALLOCATION("ELI09245", ipDataScorerObjWithDesc != __nullptr);
			
			// set the data scorer
			ipDataScorerObjWithDesc->Object = getDataScorer();
			
			// set the minimum-match-score
			ipSPMFinder->MinScoreToConsiderAsMatch = 5;
			// set the score for first match
			ipSPMFinder->MinFirstToConsiderAsMatch = 10;
			
			// set the return match type
			ipSPMFinder->ReturnMatchType = kReturnFirstOrBest;
			vecFinders.push_back( ipSPMFinder );
		}
		m_mapStringToVecSPMFinders[ strISDocType ] = vecFinders;
		return m_mapStringToVecSPMFinders[ strISDocType ]; 
	}
	
	// Document type not defined so log exception -- don't display 
	UCLIDException ue("ELI09361", "No Rules Files Found." );
	ue.addDebugInfo( "Document Type", strDocType);
	ue.addDebugInfo( "Document Sub-Type", strDocSubType);
	ue.log();
	//throw ue;	
	
	vector<ISPMFinderPtr> tmpVec;
	return tmpVec;
}
//-------------------------------------------------------------------------------------------------
IDataScorerPtr CGrantorGranteeFinderV2::getDataScorer()
{
	if (m_ipDataScorer == __nullptr )
	{
		m_ipDataScorer.CreateInstance(CLSID_EntityNameDataScorer);
		ASSERT_RESOURCE_ALLOCATION("ELI09250", m_ipDataScorer != __nullptr );
	}
	return m_ipDataScorer;
}
//-------------------------------------------------------------------------------------------------
bool CGrantorGranteeFinderV2::getDocSubType(IAFDocumentPtr ipAFDoc, string& strDocSubType)
{
	// get the object tags associated with the document
	IStrToObjectMapPtr ipObjTags(ipAFDoc->ObjectTags);
	ASSERT_RESOURCE_ALLOCATION("ELI09344", ipObjTags != __nullptr);
	
	// check to see if a string tag for the document type exists.
	if (ipObjTags->Contains(DOC_TYPE.c_str()) == VARIANT_FALSE)
	{
		return false;
	}
	
	// get the vector of document type names
	IVariantVectorPtr ipVecDocTypes = ipObjTags->GetValue(_bstr_t(DOC_TYPE.c_str()));
	ASSERT_RESOURCE_ALLOCATION("ELI09346", ipVecDocTypes != __nullptr);
	
	// If there's no doc type found or there are more than one 
	// type found, throw an exception
	long nSize = ipVecDocTypes->Size;
	// if the the document type is not unique, return false
	if (nSize == 0 || nSize > 1)
	{
		return false;
	}
	
	// get the string value for the document type tag
	string strText = asString(_bstr_t(ipVecDocTypes->GetItem(0)));

	// extract second part, if present
	long lLength = strText.length();
	long lPos = strText.find( '.', 0 );
	if ((lPos == string::npos) || (lPos == lLength - 1))
	{
		// No sub-type defined
		strDocSubType = "";
		return false;
	}
	else
	{
		// Extract the defined sub-type
		strDocSubType = strText.substr( lPos + 1, lLength - lPos - 1 );
		return true;
	}
}
//-------------------------------------------------------------------------------------------------
bool CGrantorGranteeFinderV2::getDocType(IAFDocumentPtr ipAFDoc, string& strDocType)
{
	// get the object tags associated with the document
	IStrToObjectMapPtr ipObjTags(ipAFDoc->ObjectTags);
	ASSERT_RESOURCE_ALLOCATION("ELI19362", ipObjTags != __nullptr);
	
	// check to see if a string tag for the document type exists.
	if (ipObjTags->Contains(DOC_TYPE.c_str()) == VARIANT_FALSE)
	{
		return false;
	}
	
	// get the vector of document type names
	IVariantVectorPtr ipVecDocTypes = ipObjTags->GetValue(_bstr_t(DOC_TYPE.c_str()));
	ASSERT_RESOURCE_ALLOCATION("ELI19363", ipVecDocTypes != __nullptr);
	
	// If there's no doc type found or there are more than one 
	// type found, throw an exception
	long nSize = ipVecDocTypes->Size;
	// if the the document type is not unique, return false
	if (nSize == 0 || nSize > 1)
	{
		return false;
	}
	
	// get the string value for the document type tag
	string strText = asString(_bstr_t(ipVecDocTypes->GetItem(0)));

	// extract first part, if two parts are present
	long lLength = strText.length();
	long lPos = strText.find( '.', 0 );
	if ((lPos == string::npos) || (lPos == lLength - 1))
	{
		// No sub-type defined
		strDocType = strText;
	}
	else
	{
		// Extract the defined type
		strDocType = strText.substr( 0, lPos );
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
vector< stringCSIS > &CGrantorGranteeFinderV2::getValidDocTypes()
{
	if (( getAFUtility()->GetLoadFilePerSession() == VARIANT_TRUE) && m_bValidDocTypesLoaded) 
	{
		// Already loaded 
		return m_vecValidDocTypes;
	}
	string strComponentDataFolder = getAFUtility()->GetComponentDataFolder();
	string strIndustrySpecificFolder = strComponentDataFolder 
								+ "\\" + DOC_CLASSIFIERS_FOLDER 
								+ "\\" + "County Document";
	// get doc type index file based on the industry category name
	string strDocTypeIndexFile = strIndustrySpecificFolder + "\\" + DOC_TYPE_INDEX_FILE;

	// open the file
	ifstream ifs(strDocTypeIndexFile.c_str());
	// use CommentedTextFileReader to read the file line by line
	CommentedTextFileReader fileReader(ifs, "//", true);
	string strLine("");
	while (!fileReader.reachedEndOfStream())
	{
		strLine = fileReader.getLineText();
		strLine = ::trim(strLine, " \t", " \t");
		if (strLine.empty())
		{
			continue;
		}
		
		// store each document type name
		m_vecValidDocTypes.push_back( stringCSIS(strLine, false));
	}
	m_bValidDocTypesLoaded = true;
	return m_vecValidDocTypes;
}
//-------------------------------------------------------------------------------------------------
void CGrantorGranteeFinderV2::initDocTypeToFileMap()
{
	// Create new list if doesn't already exist
	if ( m_ipDocTypeToFileMap == __nullptr )
	{
		m_ipDocTypeToFileMap.CreateInstance(CLSID_StrToObjectMap);
		ASSERT_RESOURCE_ALLOCATION("ELI09356", m_ipDocTypeToFileMap != __nullptr );
	}
	m_ipDocTypeToFileMap->Clear();
	m_ipDocTypeToFileMap->CaseSensitive = VARIANT_FALSE;

	vector<stringCSIS>& vecDocTypes = getValidDocTypes();
	if ( vecDocTypes.size() == 0 )
	{
		m_bDocTypeToFileMapLoaded = true;
		return;
	}

	string strDatFolder = getRulesFolder() + "\\" + GRANTOR_GRANTEE_FINDER;

	vector<stringCSIS>::iterator iterCurr;
	for (iterCurr = vecDocTypes.begin(); iterCurr != vecDocTypes.end(); iterCurr++ )
	{
		// Store dat files in variant vector
		IVariantVectorPtr ipDatFiles(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI09357", ipDatFiles != __nullptr);

		// Get all the dat files for this doc type
		DatFileIterator datFileIter(strDatFolder, *iterCurr);
		while (datFileIter.moveNext())
		{
			ipDatFiles->PushBack(datFileIter.getFileName().c_str());
		}
		
		// Add mapping from doctype to list of dat files
		m_ipDocTypeToFileMap->Set( (*iterCurr).c_str(), ipDatFiles );
	}
	m_bDocTypeToFileMapLoaded = true;
}
//-------------------------------------------------------------------------------------------------
bool CGrantorGranteeFinderV2::isValidDocType ( stringCSIS strDocType )
{
	getValidDocTypes();
	long nNumTypes = m_vecValidDocTypes.size();

	for ( long i = 0; i < nNumTypes; i++ )
	{
		if ( strDocType == m_vecValidDocTypes[i] )
		{
			return true;
		}
	}
	return false;
}
//-------------------------------------------------------------------------------------------------
void CGrantorGranteeFinderV2::removeInvalidDocMappings()
{
	IStrToObjectMapPtr ipNewMap( CLSID_StrToObjectMap );
	ASSERT_RESOURCE_ALLOCATION ("ELI10479", ipNewMap != __nullptr );
	
	ipNewMap->CaseSensitive = VARIANT_FALSE;
	
	long nNumKeys = m_ipDocTypeToFileMap->Size;
	CComBSTR bstrValue;

	for ( long i = 0; i < nNumKeys; i++ )
	{
		bstrValue.Empty();
		IUnknownPtr ipUnknown;
		m_ipDocTypeToFileMap->GetKeyValue( i, &bstrValue, &ipUnknown );
		
		stringCSIS strDocType( asString( bstrValue ), false);

		if ( isValidDocType ( strDocType ) ) 
		{
			// if the key does not exist in the new map  add it if it does skip it
			if (ipNewMap->Contains( _bstr_t( bstrValue ) ) == VARIANT_FALSE )
			{
				ipNewMap->Set( _bstr_t( bstrValue ), ipUnknown );
			}
			else
			{
				UCLIDException ue("ELI10480", "Application trace: Duplicate Document type Removed." );
				ue.addDebugInfo( "Document Type", static_cast<string>(strDocType) );
				ue.log();
			}
		}
		else
		{
			UCLIDException ue("ELI10484", "Doc type not valid." );
			ue.addDebugInfo( "Document Type", static_cast<string>(strDocType) );
			ue.log();
		}
	}

	m_ipDocTypeToFileMap = ipNewMap;
}
//-------------------------------------------------------------------------------------------------
