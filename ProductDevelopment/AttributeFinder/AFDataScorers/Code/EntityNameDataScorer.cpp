// EntityNameDataScorer.cpp : Implementation of CEntityNameDataScorer
#include "stdafx.h"
#include "AFDataScorers.h"
#include "EntityNameDataScorer.h"
#include "..\\..\\AFCore\\Code\\Common.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <RegistryPersistenceMgr.h>
#include <COMUtils.h>
#include <StringTokenizer.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>
#include <CommentedTextFileReader.h>
#include <EncryptedFileManager.h>
#include <cpputil.h>
#include <misc.h>
#include <ComponentLicenseIDs.h>

#include <algorithm>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const string gstrDATA_SCORER_LOGGING_ENABLED = "LoggingEnabled";
const string gstrAF_DATA_SCORERS = "AFDataScorers";
const string gstrAF_DATA_SCORERS_PATH = gstrAF_REG_ROOT_FOLDER_PATH + string("\\") + gstrAF_DATA_SCORERS;
const string gstrENDS = "\\EntityNameDataScorer";
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CEntityNameDataScorer
//-------------------------------------------------------------------------------------------------
CEntityNameDataScorer::CEntityNameDataScorer()
: m_bDirty(false),
m_ipAFUtility(NULL),
m_strCommonWords(""),
m_bIsCommonWordsLoaded(false),
m_bLoggingEnabled(false),
m_bIsInvalidPersonWordsLoaded(false),
m_cachedRegExLoader(gstrAF_AUTO_ENCRYPT_KEY_PATH.c_str())
{
	try
	{
		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI12971", m_ipMiscUtils != __nullptr );

		ma_pUserCfgMgr = unique_ptr<IConfigurationSettingsPersistenceMgr>(
			new RegistryPersistenceMgr( HKEY_CURRENT_USER, gstrAF_DATA_SCORERS_PATH ) );
		ASSERT_RESOURCE_ALLOCATION( "ELI09036", ma_pUserCfgMgr.get() != __nullptr );

		m_bLoggingEnabled = getLoggingEnabled() == 1;
		loadInvalidPersonVector();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI08962");
}
//-------------------------------------------------------------------------------------------------
CEntityNameDataScorer::~CEntityNameDataScorer()
{
	try
	{
		m_ipMiscUtils = __nullptr;
		m_ipAFUtility = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI29471");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameDataScorer::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IDataScorer,
		&IID_ICategorizedComponent,
		&IID_ICopyableObject,
		&IID_IPersistStream,
		&IID_ILicensedComponent

	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
		{
			return S_OK;
		}
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IDataScorer
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameDataScorer::raw_GetDataScore1(IAttribute * pAttribute, LONG * pScore)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		// validate License
		validateLicense();
		
		ASSERT_ARGUMENT("ELI08597", pScore != __nullptr );

		ICopyableObjectPtr ipFrom ( pAttribute );
		ASSERT_RESOURCE_ALLOCATION("ELI08646", ipFrom != __nullptr );

		// Original Attribute for logging if required
		IAttributePtr ipOriginal = ipFrom->Clone();
		ASSERT_RESOURCE_ALLOCATION("ELI09037", ipOriginal != __nullptr );

		IAttributePtr ipAttribute ( pAttribute );
		ASSERT_ARGUMENT("ELI08598", ipAttribute != __nullptr );

		*pScore = getAttrScore( ipAttribute );
		if ( m_bLoggingEnabled )
		{
			ISpatialStringPtr ipValue = ipOriginal->Value;
			ASSERT_RESOURCE_ALLOCATION("ELI09035", ipValue != __nullptr );
			string strValue = ipValue->String;
			logResults( *pScore, strValue, true );
		}

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08602")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameDataScorer::raw_GetDataScore2(IIUnknownVector * pAttributes, LONG * pScore)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		// validate License
		validateLicense();

		ASSERT_ARGUMENT("ELI08599", pScore != __nullptr );
		IIUnknownVectorPtr ipAttributes(pAttributes);
		ASSERT_ARGUMENT("ELI08600", ipAttributes != __nullptr );
		
		long nTotalScore = 0;
		long nNumAttr = ipAttributes->Size();
		for (int i = 0; i < nNumAttr; i++ )
		{
			ICopyableObjectPtr ipFrom = ipAttributes->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI08647", ipFrom != __nullptr );
			
			IAttributePtr ipCurrAttr ( ipFrom );
			ASSERT_RESOURCE_ALLOCATION("ELI08604", ipCurrAttr != __nullptr );

			// Original attribute for logging if required
			IAttributePtr ipOriginal = ipFrom->Clone();
			ASSERT_RESOURCE_ALLOCATION("ELI19132", ipOriginal != __nullptr );
			
			// Add Attributes score to total
			long nAttrScore = getAttrScore( ipCurrAttr );
			
			if ( m_bLoggingEnabled )
			{
				ISpatialStringPtr ipValue = ipOriginal->Value;
				ASSERT_RESOURCE_ALLOCATION("ELI19133", ipValue != __nullptr );
				string strValue = ipValue->String;
				logResults( nAttrScore, strValue );
			}
			nTotalScore += nAttrScore;
		}
		if ( nTotalScore > 100 )
		{
			nTotalScore = 100;
		}
		*pScore = nTotalScore;
		if ( m_bLoggingEnabled )
		{
			logResults( *pScore, "TotalGroupScore", true);
		}

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08601")
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameDataScorer::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19538", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Entity name data scorer").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08650")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameDataScorer::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*pClassID = CLSID_EntityNameDataScorer;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameDataScorer::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameDataScorer::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate License
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

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08651");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameDataScorer::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate License
		validateLicense();
		
		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter( ByteStreamManipulator::kWrite, data );
		dataWriter << gnCurrentVersion;
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08652");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameDataScorer::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameDataScorer::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate License
		validateLicense();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08655");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameDataScorer::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate License
		validateLicense();
		
		// create a new instance of the EntityNameDataScorer
		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_EntityNameDataScorer);
		ASSERT_RESOURCE_ALLOCATION("ELI08656", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08653");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameDataScorer::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// Private / Helper methods
//-------------------------------------------------------------------------------------------------
long CEntityNameDataScorer::getAttrScore( IAttributePtr ipAttribute )
{
	ASSERT_ARGUMENT("ELI08603", ipAttribute != __nullptr );

	ISpatialStringPtr ipAttrValue = ipAttribute->Value;
	// if there is no value return score of 0
	if ( ipAttrValue  == NULL )
	{
		return 0;
	}
	if ( ipAttrValue->Size == 0 )
	{
		return 0;
	}

	// Get the regex parser
	IRegularExprParserPtr ipParser = getParser();

	long nScore = 0;
	string strAttrName = asString(ipAttribute->Name);
	string strValue = asString(ipAttrValue->String);

	// if the Top level attribute name is a split subattribute name get the score of top level
	if ( strAttrName == "Person" || strAttrName == "PersonAlias" )
	{
		nScore = getPersonScore( ipAttribute, strValue, ipParser );
		return nScore;
	}
	else if ( strAttrName == "Company" || strAttrName == "CompanyAlias" ||
			strAttrName == "RelatedCompany" || strAttrName == "Trust" )
	{
		nScore = getCompanyScore( strValue, strValue, ipParser );
		return nScore;
	}


	// If not already Split split the attribute
	IIUnknownVectorPtr ipSubAttr = ipAttribute->SubAttributes;
	IEntityFinderPtr ipEntityFinder(CLSID_EntityFinder);
	ASSERT_RESOURCE_ALLOCATION("ELI08648", ipEntityFinder != __nullptr );
	
	// Entity Splitter assumes previous call to EFA
	ipEntityFinder->FindEntities( ipAttrValue );
	
	// Split the attribute value with Entity Splitter and score the results
	IAttributeSplitterPtr ipEntitySplitter(CLSID_EntityNameSplitter);
	ASSERT_RESOURCE_ALLOCATION("ELI08610", ipEntitySplitter != __nullptr );
	IAFDocumentPtr ipAFDoc (CLSID_AFDocument );
	ASSERT_RESOURCE_ALLOCATION("ELI08611", ipAFDoc != __nullptr );
	ipAFDoc->Text = ipAttribute->Value;
	ipEntitySplitter->SplitAttribute( ipAttribute, ipAFDoc, NULL );

	// if no subattributes score is 0 or the size is 0
	if (ipSubAttr == __nullptr )
	{
		return 0;
	}

	long nNumSubAttr = ipSubAttr->Size();	
	for ( int i = 0; i < nNumSubAttr; i++ )
	{
		// Get the split attribute
		IAttributePtr ipCurrAttr = ipSubAttr->At(i);
		string strName = ipCurrAttr->Name;
		ISpatialStringPtr ipValue = ipCurrAttr->Value;
		string strSubValue = ipValue->String;
	
		// Score based on the company or the person
		if (( strName == "Company" ) || ( strName == "Trust" ))
		{
			nScore += getCompanyScore( strSubValue, strValue, ipParser );
		}
		if ( strName == "Person")
		{
			nScore += getPersonScore( ipCurrAttr, strValue, ipParser );
		}
		if ( nScore > 100 )
		{
			nScore = 100;
			// no need to continue checking
			break;
		}
	}
	// if no sub-attributes exist, return 0
	if (nNumSubAttr == 0)
	{
		return 0;
	}
	return nScore;
}
//-------------------------------------------------------------------------------------------------
long CEntityNameDataScorer::getCompanyScore( string strCompanyString, string strOriginal,
											IRegularExprParserPtr ipParser)
{
	// min size of a valid Company is 4 char
	if ( strCompanyString.size() < 4 ) 
	{
		return 0;
	}

	// Get local string for parsing
	string strCompany = strCompanyString;

	// Replace any \r \n characters with spaces
	replaceVariable( strCompany, "\r", " ", kReplaceAll );
	replaceVariable( strCompany, "\n", " ", kReplaceAll );

	// Divide string into words
	StringTokenizer tokenizer(" ");
	vector<string> vecWords;

	tokenizer.parse( strCompany, vecWords);

	// Base score is 2 since this was split as company
	long nScore = 1;
	if ( isAllCommonWords( strCompany, ipParser ) )
	{
		return nScore;
	}
	if ( !noInvalidChars( strCompany, "/#\\r\\n0123456789&* -,.'ABCDEFGHIJKLMNOPQRSTUVWXYZ" ))
	{
		return nScore;
	}

	nScore++;
	// check for 50% > first letter capitalized
	if ( isTitleCase( vecWords ))
	{
		nScore += 2;
	}
	if ( hasVowelsAndConsonants( strCompany ))
	{
		nScore += 2;
	}
//	if ( noInvalidChars( strCompany, "0123456789&* -,.'ABCDEFGHIJKLMNOPQRSTUVWXYZ" ))
//	{
//		nScore += 2;
//	}

	// Check for too many vowels or consonants together
	if ( countOfRegExpInInput( "[aeiou]{4,}", strCompany, ipParser ) == 0
		&& countOfRegExpInInput( "[^aeiouy\\s]{4,}", strCompany, ipParser ) == 0 )
	{
		nScore += 2;
	}
	// Count the number embedded numbers
	if ( countOfRegExpInInput ("[a-z]*?\\d+?[a-z]+", strCompany, ipParser ) <= 2)
	{
		nScore += 2;
	}

	// score higher for more than 1 word but less than 10
	if ( vecWords.size() > 1  && vecWords.size() < 10)
	{
		nScore += 2;
	}
	if ( nScore >= 8 )
	{
		nScore = 10;
	}
	return nScore;
}
//-------------------------------------------------------------------------------------------------
long CEntityNameDataScorer::getPersonScore( IAttributePtr ipAttribute, string strOriginal,
										   IRegularExprParserPtr ipParser)
{

	if (ipAttribute == __nullptr )
	{
		return 0;
	}
	StringTokenizer tokenizer(" ");
	vector<string> vecWords;
	long nScore = 0;

	// Get person subattribute
	IIUnknownVectorPtr ipPersonSubAttr = ipAttribute->SubAttributes;
	ASSERT_RESOURCE_ALLOCATION("ELI08707", ipPersonSubAttr != __nullptr );
	ISpatialStringPtr ipPerson = ipAttribute->Value;
	ASSERT_RESOURCE_ALLOCATION("ELI09726", ipPerson != __nullptr );

	// Get local string for parsing
	string strPerson = ipPerson->String;

	// Replace any /r/n characters with spaces
	replace(strPerson.begin(), strPerson.end(), '\r', ' ');
	replace(strPerson.begin(), strPerson.end(), '\n', ' ');

	// min size of a valid person is 4 char
	if ( strPerson.size() < 4 ) 
	{
		return  0;
	}

	// divide into words
	tokenizer.parse( strPerson, vecWords);

	// if only one word score is 1
	if (	vecWords.size() < 2 || 
			!noInvalidChars( strPerson, "\\r\\n -,.'ABCDEFGHIJKLMNOPQRSTUVWXYZ" ) ||
			containsInvalidPersonWords( vecWords ))
	{
		return 0;
	}

	nScore++;
	// check if most are Title case
	if ( isTitleCase( vecWords) )
	{
		nScore++;
	}
	if ( hasVowelsAndConsonants( strPerson ))
	{
		nScore++;
	}
		// Check for too many vowels or consonants together
	if ( countOfRegExpInInput( "[aeiou]{4,}", strPerson, ipParser ) == 0
		&& countOfRegExpInInput( "[^aeiouy]{4,}", strPerson, ipParser ) == 0 )
	{
		nScore++;
	}

	long nNumPersonSubAttr = ipPersonSubAttr->Size();
	// All extra words for person go in Middle name
	bool bMiddleNameFound = false;
	bool bFirstNameFound = false;
	bool bLastNameFound = false;
	bool bSuffixFound = false;
	bool bTitleFound = false;
	bool bAliasFound = false;
	for ( int i = 0; i < nNumPersonSubAttr; i++ )
	{
		IAttributePtr ipPersonSub = ipPersonSubAttr->At(i);
		ASSERT_RESOURCE_ALLOCATION( "ELI09710", ipPersonSub != __nullptr );

		string strAttrName = ipPersonSub->Name;
		ISpatialStringPtr ipValue = ipPersonSub->Value;
		ASSERT_RESOURCE_ALLOCATION( "ELI09709", ipValue != __nullptr );
		string strValue = ipValue->String;
		
		// bAboveMinSize allows first last and title to be checked for min to be valid
		long nValueSize = ipValue->Size;
		bool bAboveMinSize = nValueSize > 1;

		if ( bAboveMinSize  && strAttrName == "Title" )
		{
			bTitleFound = true;
		}
		else if ( strAttrName == "Suffix" )
		{
			bSuffixFound = true;
		}
		else if (  strAttrName == "First" && !isAllCommonWords (strValue, ipParser) )
		{
			bFirstNameFound = true;
		}
		else if ( bAboveMinSize  && strAttrName == "Last" && !isAllCommonWords (strValue, ipParser))
		{
			bLastNameFound = true;
		}
		else if ( strAttrName == "Middle" )
		{
			bMiddleNameFound = true;
			tokenizer.parse( strValue, vecWords );	
			if ( vecWords.size() < 3 )
			{
				nScore++;
			}
		}
		else if ( strAttrName == "PersonAlias" )
		{
			bAliasFound = true;
		}

	}
	if ( !bMiddleNameFound )
	{
		nScore++;
	}
	if ( bFirstNameFound )
	{
		nScore++;
	}
	if ( bLastNameFound )
	{
		nScore++;
	}
	if ( bTitleFound || bSuffixFound )
	{
		nScore++;
	}

	// Get Person Designators pattern
	IEntityKeywordsPtr ipEntityKeywords ( CLSID_EntityKeywords );
	ASSERT_RESOURCE_ALLOCATION("ELI08723", ipEntityKeywords != __nullptr );
	IVariantVectorPtr ipPersonDesignators = ipEntityKeywords->PersonDesignators;
	ASSERT_RESOURCE_ALLOCATION("ELI08725", ipPersonDesignators != __nullptr );

	// Search original string for Person designators
	VARIANT_BOOL bFound = ipParser->StringContainsPatterns(strOriginal.c_str(), 
															ipPersonDesignators, FALSE);
	if ( bFound || bAliasFound )
	{
		nScore++;
	}
	// Determine all common words
	if ( !isAllCommonWords (strPerson, ipParser) )
	{
		nScore += 2;
	}
	if ( nScore >= 8 )
	{
		nScore = 10;
	}
	return nScore;

}
//-------------------------------------------------------------------------------------------------
bool CEntityNameDataScorer::isTitleCase( vector<string> &vecWords )
{
	long nNumWords = vecWords.size();
	long nCapCount = 0;
	for ( int i = 0; i < nNumWords; i++ )
	{
		long nCapPos;
		string strValue = trim ( vecWords[i], " .,'", " .,'" );
		nCapPos = strValue.find_first_of("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
		if ( nCapPos == 0)
		{
			nCapCount++;
		}
	}

	if ( nCapCount > nNumWords / 2 ) 
	{
		return true;
	}
	return false;
}
//-------------------------------------------------------------------------------------------------
void CEntityNameDataScorer::validateLicense()
{
	static const unsigned long ENTITY_NAME_DATA_SCORER_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( ENTITY_NAME_DATA_SCORER_ID, "ELI08849", "Entity Data Scorer" );
}
//-------------------------------------------------------------------------------------------------
bool CEntityNameDataScorer::isAllCommonWords( const string& strInput,
											 IRegularExprParserPtr ipParser)
{
	if ( strInput.size() == 0 ) 
	{
		return false;
	}
	// Load common words regular expression
	string strPattern = getCommonWordsPattern();
	ipParser->IgnoreCase = VARIANT_TRUE;
	ipParser->Pattern = strPattern.c_str();
	string strWithoutWords = asString(ipParser->ReplaceMatches(strInput.c_str(), "", VARIANT_FALSE));
	
	// remove punctuation
	ipParser->Pattern = "[\\.,/?';:""\\[\\]\\(\\)\\s\\\\]";
	string strWithoutPunct =
		asString(ipParser->ReplaceMatches(strWithoutWords.c_str(), "", VARIANT_FALSE));
	
	//  if anything is left return score of 2
	if ( strWithoutPunct.size() > 0 ) 
	{
		return false;
	}

	// nothing left score 0
	return true;
}
//-------------------------------------------------------------------------------------------------
string &CEntityNameDataScorer::getCommonWordsPattern()
{
	VARIANT_BOOL bLoadFilePerSession = getAFUtility()->GetLoadFilePerSession() ;

	// Pattern is loaded and loadFilePerSession is set return member pattern
	if ( bLoadFilePerSession && m_bIsCommonWordsLoaded )
	{
		return m_strCommonWords;
	}
	// setup file name to read pattern from
	string strComponentDataDir = getAFUtility()->GetComponentDataFolder();
	string strFileName =  strComponentDataDir + "\\EntityNameDataScorer\\" + "\\" + "CommonWords.dat.etf";

	// [FlexIDSCore:3643] Load the regular expression from disk if necessary.
	m_cachedRegExLoader.loadObjectFromFile(strFileName);

	// Retrieve the pattern
	m_strCommonWords = (string)m_cachedRegExLoader.m_obj;

	m_bIsCommonWordsLoaded = true;
	return m_strCommonWords;
}

//-------------------------------------------------------------------------------------------------
IAFUtilityPtr CEntityNameDataScorer::getAFUtility()
{
	if (m_ipAFUtility == __nullptr)
	{
		m_ipAFUtility.CreateInstance( CLSID_AFUtility );
		ASSERT_RESOURCE_ALLOCATION( "ELI08950", m_ipAFUtility != __nullptr );
	}
	
	return m_ipAFUtility;
}
//-------------------------------------------------------------------------------------------------
void CEntityNameDataScorer::logResults(long nScore, string strItemScored, bool bLineAfter)
{
	// Convert each string to single-line string
	::convertCppStringToNormalString( strItemScored );
	
	// Get path to log file
	string strLogFile = ::getModuleDirectory(_Module.m_hInst) + "\\" + string( "gstrENDSLog.dat" );

	// Open the log file
	ofstream ofsLogFile( strLogFile.c_str(), (ios::out | ios::app) );

	// Get the output string
	string	strPipe( "|" );
	string	strOut = asString(nScore) + strPipe + strItemScored;

	// Write string to log file
	ofsLogFile << strOut << endl;
	if ( bLineAfter )
	{
		ofsLogFile << endl;
	}

	// Close the log file
	ofsLogFile.close();
	waitForFileToBeReadable(strLogFile);
}
//-------------------------------------------------------------------------------------------------
long CEntityNameDataScorer::getLoggingEnabled()
{
	long lResult = 0;

	// Check for existence of key
	if (!ma_pUserCfgMgr->keyExists( gstrENDS, gstrDATA_SCORER_LOGGING_ENABLED ))
	{
		// Key does not exist, set and return default (NOT ENABLED)
		ma_pUserCfgMgr->createKey( gstrENDS, gstrDATA_SCORER_LOGGING_ENABLED, "0" );
	}
	else
	{
		// Key found - return its value
		string strResult = ma_pUserCfgMgr->getKeyValue( gstrENDS, gstrDATA_SCORER_LOGGING_ENABLED );
		lResult = ::asLong( strResult );
	}

	return lResult;
}
//-------------------------------------------------------------------------------------------------
bool CEntityNameDataScorer::hasVowelsAndConsonants( const string& strItem )
{
	string strTemp = strItem;
	makeUpperCase( strTemp );
	long nPos = strTemp.find_first_of("BCDFGHJKLMNPQRSTVWXYZ");
	if ( nPos != string::npos )
	{
		nPos = strTemp.find_first_of("AEIOUY");
		if (nPos != string::npos )
		{
			return true;
		}
	}
	return false;
}
//-------------------------------------------------------------------------------------------------
bool CEntityNameDataScorer::noInvalidChars( const string& strItem, const string& strValidChars )
{
	string strTemp = strItem;
	makeUpperCase( strTemp );
	long nPos = strTemp.find_first_not_of(strValidChars);
	if ( nPos != string::npos )
	{
		return false;
	}
	return true;
}
//-------------------------------------------------------------------------------------------------
void CEntityNameDataScorer::loadInvalidPersonVector()
{
	VARIANT_BOOL bLoadFilePerSession = getAFUtility()->GetLoadFilePerSession() ;

	// Pattern is loaded and loadFilePerSession is set return member pattern
	if ( bLoadFilePerSession && m_bIsInvalidPersonWordsLoaded )
	{
		return;
	}
	// setup file name to read pattern from
	string strComponentDataDir = getAFUtility()->GetComponentDataFolder();
	string strFileName =  strComponentDataDir + "\\EntityNameDataScorer\\" + "\\" + "InvalidPersonWords.dat.etf";
	
	// make sure the encryption is current
	autoEncryptFile(strFileName, gstrAF_AUTO_ENCRYPT_KEY_PATH.c_str());

	// Clear the vector
	m_vecInvalidPersonWords.clear();
	// Get the lines of the file
	vector<string> vecLines = convertFileToLines(strFileName);
	CommentedTextFileReader fileReader(vecLines, "//", true);
	while (!fileReader.reachedEndOfStream())
	{
		// Retrieve this line
		string strLine = ::trim(fileReader.getLineText(), " \t", "");
		if (strLine.empty())
		{
			continue;
		}
		// Make sure all words are lower case
		makeLowerCase(strLine);
		// put word in list
		m_vecInvalidPersonWords.push_back(strLine);
	}
	// sort the list
	sort(m_vecInvalidPersonWords.begin(), m_vecInvalidPersonWords.end());
	m_bIsInvalidPersonWordsLoaded = true;
	return;
}
//-------------------------------------------------------------------------------------------------
bool CEntityNameDataScorer::containsInvalidPersonWords( const vector<string> &vecWords )
{
	bool bResult = false;
	long nNumWords = vecWords.size();
	for ( int i = 0; !bResult && i < nNumWords; i++ )
	{
		string strWord = vecWords[i];
		// Compare as lower case
		makeLowerCase( strWord);
		bResult = binary_search(	m_vecInvalidPersonWords.begin(), 
									m_vecInvalidPersonWords.end(), 
									strWord);
	}
	return bResult;
};
//-------------------------------------------------------------------------------------------------
long CEntityNameDataScorer::countOfRegExpInInput( const string &strRegExpToFind,
												 const string &strInput,
												 IRegularExprParserPtr ipParser)
{
	if ( strInput.size() == 0 ) 
	{
		return false;
	}
	// Set pattern for Multiple vowels
	ipParser->IgnoreCase = VARIANT_TRUE;
	ipParser->Pattern = strRegExpToFind.c_str();

	IIUnknownVectorPtr ipFound;
	ipFound =	ipParser->Find( strInput.c_str(), VARIANT_TRUE, VARIANT_FALSE );

	if (ipFound != __nullptr )
	{
		return ipFound->Size();
	}
	return 0;
}
//-------------------------------------------------------------------------------------------------
IRegularExprParserPtr CEntityNameDataScorer::getParser()
{
	try
	{
		IRegularExprParserPtr ipParser =
			m_ipMiscUtils->GetNewRegExpParserInstance("EntityNameDataScorer");
		ASSERT_RESOURCE_ALLOCATION("ELI08724", ipParser != __nullptr );

		return ipParser;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29472");
}
//-------------------------------------------------------------------------------------------------
