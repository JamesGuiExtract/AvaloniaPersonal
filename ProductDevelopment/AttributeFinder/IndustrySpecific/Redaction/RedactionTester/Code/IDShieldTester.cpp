// IDShieldTester.cpp : Implementation of CIDShieldTester

#include "stdafx.h"
#include "IDShieldTester.h"

#include <Common.h>
#include <SpecialStringDefinitions.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <CommentedTextFileReader.h>
#include <StringTokenizer.h>
#include <ComUtils.h>
#include <LicenseMgmt.h>
#include <TextFunctionExpander.h>
#include <ComponentLicenseIDs.h>
#include <RegistryPersistenceMgr.h>
#include <mathUtil.h>
#include <MiscLeadUtils.h>

#include <cmath>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Path to the registry key for minimum overlap of raster zone that will 'pass' the test
const string gstrMIN_OVERLAP_REGISTRY_KEY_PATH = gstrAF_REG_ROOT_FOLDER_PATH + 
									"\\IndustrySpecific\\Redaction";
const string gstrOVERLAP_PERCENT_FOLDER = "\\RedactionTester";
const string gstrOVERLAP_PERCENT_KEY = "RasterZoneOverlapLeniency";
const string gstrOVERLAP_DEFAULT_VALUE = "80";

// ERAP - Excess Redaction Area Percentage
const string gstrOVER_REDACTION_PERCENT_KEY = "OverRedactionERAP";
const string gstrOVER_REDACTION_ERAP_DEFAULT_VALUE = "30";

const string gstrDOCTYPEQUERY = "DocumentType";

//-------------------------------------------------------------------------------------------------
// These output files will be written to "$DirOf(tcl file)\\Analysis - (Timestamp)"
//-------------------------------------------------------------------------------------------------
// Files output in handleTestCase
const string gstrFILES_WITH_EXPECTED_REDACTIONS = "\\SensitiveFiles_All.txt";
const string gstrFILES_WITH_NO_EXPECTED_REDACTIONS = "\\InsensitiveFiles_All.txt";
const string gstrFILES_WITH_FOUND_REDACTIONS = "\\Files_AtLeastOneFoundRedaction.txt";
const string gstrFILES_WITH_NO_FOUND_REDACTIONS = "\\Files_NoFoundRedactions.txt";

// Files output in analyzeDataForVerificationBasedRedaction
const string gstrFILES_CORRECTLY_SELECTED_FOR_REVIEW = 
	"\\SensitiveFiles_SelectedForReview.txt";
const string gstrFILES_INCORRECTLY_SELECTED_FOR_REVIEW = 
	"\\InsensitiveFiles_SelectedForReview.txt";
const string gstrFILES_MISSED_BEING_SELECTED_FOR_REVIEW = 
	"\\SensitiveFiles_NotSelectedForReview.txt";

// File output in analyzeDataForAutomatedRedaction
// These files added as per P16 #2552 and modified as per P16 #2811
const string gstrFILES_WITH_HCDATA = "\\Files_AtLeastOneFoundHCData.txt";
const string gstrFILES_WITH_MCDATA = "\\Files_AtLeastOneFoundMCData.txt";
const string gstrFILES_WITH_LCDATA = "\\Files_AtLeastOneFoundLCData.txt";
const string gstrFILES_WITH_CLUES = "\\Files_AtLeastOneFoundClue.txt";

const string gstrFILES_CORRECTLY_SELECTED_FOR_REDACTION =
	"\\SensitiveFiles_AutomaticallyRedacted.txt";
const string gstrFILES_INCORRECTLY_SELECTED_FOR_REDACTION = 
	"\\InsensitiveFiles_AutomaticallyRedacted.txt";
const string gstrFILES_MISSED_BEING_SELECTED_FOR_REDACTION =
	"\\SensitiveFiles_NotSelectedForRedaction.txt";

// Files output in countDocTypes
const string gstrFILES_CLASSIFIED_AS_MORE_THAN_ONE_DOC_TYPE = 
	"\\DocType_MultipleDocTypes.txt";
const string gstrUNCLASSIFIED_DOC_TYPE_FILES = "\\DocType_Unclassified.txt";
// Known DocType files are written to DocType - TypeName.txt (P16 #2769)
const string gstrDOC_TYPE_PREFIX = "\\DocType_";

// Files output in analyzeExpectedAndFoundAttributes
// These files added as per P16 #2606
const string gstrFILES_WITH_CORRECT_REDACTIONS = "\\SensitiveFiles_AtLeastOneCorrectRedaction.txt";
const string gstrFILES_WITH_OVER_REDACTIONS = "\\SensitiveFiles_AtLeastOneOverRedaction.txt";
const string gstrFILES_WITH_UNDER_REDACTIONS = "\\SensitiveFiles_AtLeastOneUnderRedaction.txt";
const string gstrFILES_WITH_FALSE_POSITIVES = "\\Files_AtLeastOneFalsePositive.txt";
const string gstrFILES_WITH_MISSED_REDACTIONS = "\\SensitiveFiles_AtLeastOneMissedRedaction.txt";

// Files output in displaySummaryStatistics and displayDocumentTypeStats
const string gstrFILE_FOR_STATISTICS = "\\Statistics.txt";

const string gstrTEST_UNDER_REDACTED = "UnderRedaction_";
const string gstrTEST_OVER_REDACTED = "OverRedaction_";
const string gstrTEST_CORRECT_REDACTED = "Correct_";
const string gstrTEST_MISSED_REDACTED = "Missed_";
const string gstrTEST_FALSE_POSITIVE = "FalsePositive_";

// attribute names
const string gstrATTRIBUTE_HCDATA	= "HCData";
const string gstrATTRIBUTE_MCDATA	= "MCData";
const string gstrATTRIBUTE_LCDATA	= "LCData";
const string gstrATTRIBUTE_MANUAL	= "Manual";
const string gstrATTRIBUTE_CLUES	= "Clues";

//-------------------------------------------------------------------------------------------------
// Local helper functions
//-------------------------------------------------------------------------------------------------
// Validates the specified condition string and throws an exception with the
// specified ELI code if the string is invalid. Returns the QueryCondition for the string
// if it is valid.
EAttributeQuantifier validateConditionString(const string& strCondition, const string& strELICode)
{
	// Make the string lower case
	string strTemp = strCondition;
	makeLowerCase(strTemp);

	if (strTemp == "any")
	{
		return kAny;
	}
	else if (strTemp == "only any")
	{
		return kOnlyAny;
	}
	else if (strTemp == "at least one of each")
	{
		return kOneOfEach;
	}
	else if(strTemp == "none")
	{
		return kNone;
	}
	else
	{
		UCLIDException ue(strELICode, "Invalid condition string.");
		ue.addDebugInfo("Condition String", strCondition);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void buildConditionSet(const string& strConditions, set<string>& rsetConditions)
{
	// Tokenize the string by the '|' character
	vector<string> vecTemp;
	StringTokenizer::sGetTokens(strConditions, '|', vecTemp);

	// Iterate through the settings and build the set
	for(vector<string>::iterator it = vecTemp.begin(); it != vecTemp.end(); it++)
	{
		if(!it->empty())
		{
			string strTemp = *it;
			makeLowerCase(strTemp);

			rsetConditions.insert(strTemp);
		}
	}
}
//-------------------------------------------------------------------------------------------------
bool testAttributeCondition(IIUnknownVectorPtr ipAttributes,
											 AttributeTester* pTester)
{
	ASSERT_ARGUMENT("ELI25179", pTester != NULL);

	// Loop through each attribute and test it
	long lAttributeCount = ipAttributes->Size();
	for (long l = 0; l < lAttributeCount; l++)
	{
		// Get next attribute
		IAttributePtr ipAttribute = ipAttributes->At(l);
		ASSERT_RESOURCE_ALLOCATION("ELI25165", ipAttribute != NULL);

		// Get attribute name
		string strName = asString(ipAttribute->GetName());

		// Get attribute value
		ISpatialStringPtr ipValue = ipAttribute->Value;
		ASSERT_RESOURCE_ALLOCATION("ELI25166", ipValue != NULL);
		string strValue = asString(ipValue->String);

		// Test to see if this attribute fulfills the condition
		if (pTester->test(strName, strValue))
		{
			// No need to test any further attributes
			break;
		}
	}

	return pTester->getResult();
}

//-------------------------------------------------------------------------------------------------
// CIDShieldTester
//-------------------------------------------------------------------------------------------------
CIDShieldTester::CIDShieldTester() :
m_bOutputAttributeNameFilesList(false),
m_ipTestOutputVOAVector(NULL)
{
	try
	{
		m_ipAFUtility.CreateInstance(CLSID_AFUtility);
		ASSERT_RESOURCE_ALLOCATION("ELI15178", m_ipAFUtility != NULL);

		m_ipAttrFinderEngine.CreateInstance(CLSID_AttributeFinderEngine);
		ASSERT_RESOURCE_ALLOCATION("ELI15206", m_ipAttrFinderEngine != NULL);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI15179");
}
//-------------------------------------------------------------------------------------------------
CIDShieldTester::~CIDShieldTester()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16555");
}
//-------------------------------------------------------------------------------------------------
HRESULT CIDShieldTester::FinalConstruct()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
void CIDShieldTester::FinalRelease()
{
}

//-------------------------------------------------------------------------------------------------
// IIDShieldTester
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldTester::get_OutputFileDirectory(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		*pVal = _bstr_t(m_strOutputFileDirectory.c_str());

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28679")
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldTester::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IIDShieldTester,
		&IID_ITestableComponent,
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
// ITestableComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldTester::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI14547", pParams != NULL);

			// Validate the license
			validateLicense();

			if (m_ipResultLogger == NULL)
			{
				throw UCLIDException("ELI15166", "Please set ResultLogger before proceeding.");
			}

			// Default to adding entries to the results logger
			m_ipResultLogger->AddEntriesToTestLogger = VARIANT_TRUE;

			// Create a persistence manager to get the overlap value from the registry
			RegistryPersistenceMgr rpm( HKEY_CURRENT_USER, gstrMIN_OVERLAP_REGISTRY_KEY_PATH );

			// If a key exists, use the one that is currently in the registry
			string strOverlapLeniency = "";
			if( rpm.keyExists(gstrOVERLAP_PERCENT_FOLDER, gstrOVERLAP_PERCENT_KEY) )
			{
				strOverlapLeniency = rpm.getKeyValue( gstrOVERLAP_PERCENT_FOLDER, gstrOVERLAP_PERCENT_KEY );
			}
			else
			{
				// Key does not exist, create a key with the default value and 
				// overwrite the key if an invalid one exists
				rpm.createKey(gstrOVERLAP_PERCENT_FOLDER, gstrOVERLAP_PERCENT_KEY, gstrOVERLAP_DEFAULT_VALUE, true);
				strOverlapLeniency = gstrOVERLAP_DEFAULT_VALUE;
			}

			// Use the key that was in the registry, or the default one as the above blocks determine
			m_dOverlapLeniencyPercent = asDouble( strOverlapLeniency );
			
			// if a key exists, use the one that is currently in the registry
			string strOverRedactionERAP = "";
			if (rpm.keyExists(gstrOVERLAP_PERCENT_FOLDER, gstrOVER_REDACTION_PERCENT_KEY))
			{
				strOverRedactionERAP = rpm.getKeyValue(gstrOVERLAP_PERCENT_FOLDER, 
					gstrOVER_REDACTION_PERCENT_KEY);
			}
			else
			{
				// key does not exist, create a key with the default value
				rpm.createKey(gstrOVERLAP_PERCENT_FOLDER, gstrOVER_REDACTION_PERCENT_KEY, 
					gstrOVER_REDACTION_ERAP_DEFAULT_VALUE, true);
				strOverRedactionERAP = gstrOVER_REDACTION_ERAP_DEFAULT_VALUE;
			}

			// use the key from the registry or the default as set above
			m_dOverRedactionERAP = asDouble(strOverRedactionERAP);

			// reset all variables associated with the test
			m_mapDocTypeCount.clear();
			m_setVerificationCondition.clear();
			m_strVerificationCondition = "";
			m_strVerificationQuantifier = "";
			m_eaqVerificationQuantifier = kAny;
			m_setRedactionCondition.clear();
			m_strRedactionCondition = "";
			m_strRedactionQuantifier = "";
			m_eaqRedactionQuantifier = kAny;
			m_strRedactionQuery = "";
			m_setTypesToBeTested.clear();
			m_setDocTypesToBeVerified.clear();
			m_setDocTypesToBeAutomaticallyRedacted.clear();
			m_ulTotalExpectedRedactions = 0;
			m_ulNumCorrectRedactions = 0;
			m_ulNumOverRedactions = 0;
			m_ulNumUnderRedactions = 0;
			m_ulNumMisses = 0;
			m_ulTotalFilesProcessed = 0;
			m_ulNumFilesWithExpectedRedactions = 0;
			m_ulNumFilesSelectedForReview = 0;
			m_ulNumFilesAutomaticallyRedacted = 0;
			m_ulNumExpectedRedactionsInReviewedFiles = 0;
			m_ulNumExpectedRedactionsInRedactedFiles = 0;
			m_iNumFilesWithExistingVOA = 0;
			m_ulNumFilesWithOverlappingExpectedRedactions = 0;
			m_ulTotalPages = 0;
			m_ulNumPagesWithExpectedRedactions = 0;
			m_bOutputHybridStats = false;
			m_bOutputAutomatedStatsOnly = false;
			m_apRedactionTester.reset();
			m_apVerificationTester.reset();
			automatedStatistics.reset();
			verificationStatistics.reset();

			IVariantVectorPtr ipParams(pParams);
			ASSERT_RESOURCE_ALLOCATION("ELI15258", ipParams != NULL);

			// The params vector should never be empty
			if (ipParams->Size == 0)
			{
				THROW_LOGIC_ERROR_EXCEPTION("ELI14548")
			}
			
			// Get the .dat file
			// if pParams is not empty and the second item is specified,
			// then the second item is the master dat file
			string strMasterDatFileName = "";
			string strTestRuleSetsFile("");
			if (ipParams->Size > 1)
			{
				strMasterDatFileName = asString(_bstr_t(ipParams->GetItem(1)));
				if (!strMasterDatFileName.empty())
				{
					strTestRuleSetsFile = ::getAbsoluteFileName( asString( strTCLFile ), 
						strMasterDatFileName, true );
				}
			}
			else
			{
				UCLIDException ue("ELI15167", "Required master testing .DAT file missing.");
				throw ue;
			}	

			// Determine the Analysis folder to create the log files in
			getOutputDirectory(getDirectoryFromFullPath(strTestRuleSetsFile));

			// Process the DAT file
			processDatFile(strTestRuleSetsFile);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI15230")
	}
	catch(UCLIDException ue)
	{
		// Store the exception for display and continue
		m_ipResultLogger->AddComponentTestException(get_bstr_t(ue.asStringizedByteStream()));
	}

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldTester::raw_RunInteractiveTests()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldTester::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_ipResultLogger = pLogger;
		ASSERT_RESOURCE_ALLOCATION("ELI15736", m_ipResultLogger != NULL);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14546");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldTester::raw_SetInteractiveTestExecuter(
	IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	// Intentionally empty method
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldTester::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	try
	{
		// Check license
		validateLicense();

		// If validateLicense doesn't throw any exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CIDShieldTester::validateLicense()
{
	VALIDATE_LICENSE( gnIDSHIELD_CORE_OBJECTS, "ELI15192", "ID Shield Tester" );
}
//-------------------------------------------------------------------------------------------------
void CIDShieldTester::processDatFile(const string& strDatFileName)
{
	// Create a file stream for the dat file
	ifstream ifs(strDatFileName.c_str());

	if( ! ifs.is_open() )
	{
		UCLIDException ue("ELI15168", "Error while opening DAT file.");
		ue.addDebugInfo("File", strDatFileName);
		throw ue;
	}

	// Create a commented text file reader from the file stream
	CommentedTextFileReader fileReader(ifs, "//", true);

	// Set the initial case number
	string strLine = "";
	do
	{
		// Read a single line of text
		strLine = "";
		strLine = fileReader.getLineText();

		// skip any empty line
		if (strLine.empty())
		{
			continue;
		}

		// Process the line from the DAT file
		interpretLine(strLine, strDatFileName);
	}
	// Continue to the end of file
	while(!ifs.eof());

	// wrap up the test by displaying summary stats and generating file lists
	displaySummaryStatistics();
}
//-------------------------------------------------------------------------------------------------
void CIDShieldTester::interpretLine(const string& strLineText,
									const string& strCurrentDatFileName)
{
	/*******************************************************************************
	// Each line must have a prefix of <SETTING> or <TESTFOLDER> prefix.
	*******************************************************************************/

	// parse each line into multiple tokens with the delimiter as ";"
	vector<string> vecTokens;
	StringTokenizer::sGetTokens(strLineText, ';', vecTokens);
	int nNumTokens = static_cast<int>( vecTokens.size() );
	if (nNumTokens < 2)
	{
		UCLIDException ue("ELI15171", "Invalid line of text in DAT file.");
		ue.addDebugInfo("Line", strLineText);
		throw ue;
	}

	// The first token is the tag
	// Check for <SETTING>, <TESTFOLDER>
	if( vecTokens[0] == "<SETTING>" )
	{
		// Verify the correct number of arguments for the <SETTING> case.
		if( nNumTokens != 2 )
		{
			UCLIDException ue("ELI15172", "Invalid number of arguments for <SETTING> in DAT file.");
			ue.addDebugInfo("Line Text", strLineText);
			ue.addDebugInfo("Number of args", nNumTokens);
			throw ue;
		}

		// Handle Settings line
		handleSettings(vecTokens[1]);
	}
	else if( vecTokens[0] == "<TESTFOLDER>" )
	{
		// Create the output directory if this is the first test folder to run.
		if (!isValidFolder(m_strOutputFileDirectory))
		{
			createDirectory(m_strOutputFileDirectory);
		}

		// Verify the correct number of tokens for the TESTFOLDER case and that a verification
		// condition has been properly set (if required) and the automated redaction query have
		// been set to something useful.
		if(!m_bOutputAutomatedStatsOnly &&
			(m_setVerificationCondition.empty() || m_strVerificationQuantifier.empty()) &&
			m_setDocTypesToBeVerified.empty())
		{
			// Both condition settings must be set before test folder can take place
			UCLIDException ue("ELI25180", 
				"Either a verification attribute condition or doc type condition must be set.");
			throw ue;
		}
		else if ((m_bOutputAutomatedStatsOnly || m_bOutputHybridStats) &&
			(m_setRedactionCondition.empty() || m_strRedactionQuantifier.empty()) &&
			m_setDocTypesToBeAutomaticallyRedacted.empty())
		{
			// Both condition settings must be set before test folder can take place
			UCLIDException ue("ELI25162", 
				"Either an automated redaction condition or doc type condition must be set.");
			throw ue;
		}
		else if(nNumTokens != 5)
		{
			UCLIDException ue("ELI15173", 
				"Invalid number of arguments for <TESTFOLDER> in DAT file.");
			ue.addDebugInfo("Line Text", strLineText);
			ue.addDebugInfo("Number of Args", nNumTokens);
			throw ue;
		}
		else
		{
			// Handle Test Folder line
			handleTestFolder(vecTokens[1], vecTokens[2], vecTokens[3], 
				vecTokens[4], strCurrentDatFileName);
		}
	}
	else
	{
		UCLIDException ue("ELI15175", "Invalid prefix in DAT file.");
		ue.addDebugInfo("Tag value", vecTokens[0]);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void CIDShieldTester::handleSettings(const string& strSettingsText)
{
	// Tokenize on the =
	vector<string> vecTokens;
	StringTokenizer::sGetTokens(strSettingsText, '=', vecTokens);

	if ( vecTokens.size() == 2 )
	{
		// Check which type of setting this is, and verify that the specified setting has not
		// yet been set. 
		// [FlexIDSCore #3359 - JDS 03/30/2009]
		if ( vecTokens[0] == "VerificationCondition")
		{
			// If the setting for Verification has already been set, it cannot be set again
			if (! m_setVerificationCondition.empty())
			{
				throw UCLIDException("ELI25158", "Verification condition can only be set once.");
			}

			m_strVerificationCondition = vecTokens[1];
			buildConditionSet(m_strVerificationCondition, m_setVerificationCondition);
		}
		// [FlexIDSCore #3359 - JDS 03/30/2009]
		else if (vecTokens[0] == "VerificationConditionQuantifier")
		{
			// Get the condition quantifier
			m_strVerificationQuantifier = vecTokens[1];

			m_eaqVerificationQuantifier = validateConditionString(m_strVerificationQuantifier,
				"ELI25156");
		}
		else if (vecTokens[0] == "QueryForAutomatedRedaction")
		{
			if (! m_strRedactionQuery.empty())
			{
				throw UCLIDException("ELI15236", 
					"Query for automated redaction can only be set once.");
			}

			m_strRedactionQuery = vecTokens[1];
		}
		// [FlexIDSCore #3359 - JDS 03/30/2009]
		else if (vecTokens[0] == "AutomatedCondition")
		{
			// If the setting for Automated redaction has already been set, it cannot be set again
			if (! m_setRedactionCondition.empty())
			{
				throw UCLIDException("ELI25155",
					"Automated redaction condition can only be set once.");
			}

			m_strRedactionCondition = vecTokens[1];
			buildConditionSet(m_strRedactionCondition, m_setRedactionCondition);
		}
		// [FlexIDSCore #3359 - JDS 03/30/2009]
		else if (vecTokens[0] == "AutomatedConditionQuantifier")
		{
			// Get the condition quantifier
			m_strRedactionQuantifier = vecTokens[1];

			m_eaqRedactionQuantifier = validateConditionString(m_strRedactionQuantifier, "ELI25157");
		}
		// [p16 #2606 - JDS] 
		else if (vecTokens[0] == "CreateTestOutputVOAFiles")
		{
			if (vecTokens[1] == "1")
			{
				m_ipTestOutputVOAVector.CreateInstance(CLSID_IUnknownVector);
				ASSERT_RESOURCE_ALLOCATION("ELI18360", m_ipTestOutputVOAVector != NULL);
			}
		}
		// [p16 #2552 - JDS]
		else if (vecTokens[0] == "OutputAttributeNamesFileLists")
		{
			m_bOutputAttributeNameFilesList = vecTokens[1] == "1";
		}
		// [p16 #2385 - JDS]
		// [FlexIDSCore:3752] Multiple types can now be tested at once... but allow legacy support
		// of "TypeToBeTested" setting name.
		else if (vecTokens[0] == "TypeToBeTested" ||
				 vecTokens[0] == "TypesToBeTested")
		{
			if (!m_setTypesToBeTested.empty())
			{
				throw UCLIDException("ELI18506", "Type to be tested may only be set once.");
			}

			// check for the "all"
			string strLowerTypes = vecTokens[1];
			makeLowerCase(strLowerTypes);

			if (strLowerTypes != "all")
			{
				// Support both comma and pipe delimiters.
				vector<string> vecTemp;
				StringTokenizer::sGetTokens(vecTokens[1], "|,", vecTemp, true);

				// Insert the doc types into the set of doc types
				m_setTypesToBeTested.insert(vecTemp.begin(), vecTemp.end());
			}
		}
		// [FlexIDSCore #3347 - JDS 03/30/2009]
		else if (vecTokens[0] == "DocTypesToVerify")
		{
			if (!m_setDocTypesToBeVerified.empty())
			{
				throw UCLIDException("ELI25149", "Doc types to verify may only be set once.");
			}

			// Tokenize the doc types to be verified by the '|' character
			vector<string> vecTemp;
			StringTokenizer::sGetTokens(vecTokens[1], '|', vecTemp);

			// Insert the doc types into the set of doc types
			m_setDocTypesToBeVerified.insert(vecTemp.begin(), vecTemp.end());
		}
		// [FlexIDSCore:3980]
		else if (vecTokens[0] == "DocTypesToRedact")
		{
			if (!m_setDocTypesToBeAutomaticallyRedacted.empty())
			{
				throw UCLIDException("ELI29401",
					"Doc types to automatically redact may only be set once.");
			}

			// Tokenize the doc types to be automatically redacted by the '|' character
			vector<string> vecTemp;
			StringTokenizer::sGetTokens(vecTokens[1], '|', vecTemp);

			// Insert the doc types into the set of doc types
			m_setDocTypesToBeAutomaticallyRedacted.insert(vecTemp.begin(), vecTemp.end());
		}
		// [FlexIDSCore #3358 - JDS 03/30/2009]
		else if (vecTokens[0] == "OutputHybridStats")
		{
			m_bOutputHybridStats = vecTokens[1] == "1";
		}
		else if (vecTokens[0] == "OutputFinalStatsOnly")
		{
			// Set whether to display entries for each test case or not
			m_ipResultLogger->AddEntriesToTestLogger = asVariantBool(vecTokens[1] != "1");
		}
		else if (vecTokens[0] == "OutputFilesFolder")
		{
			// Update m_strOutputFileDirectory based on the specified folder.
			getOutputDirectory(vecTokens[1]);
		}
		else if (vecTokens[0] == "OutputAutomatedStatsOnly")
		{
			m_bOutputAutomatedStatsOnly = (vecTokens[1] == "1");
		}
		else
		{
			UCLIDException ue("ELI15176", "Invalid Settings token in DAT file.");
			ue.addDebugInfo("Text", strSettingsText);
			throw ue;
		}
	}
	else
	{
		UCLIDException ue("ELI15177", "Invalid <SETTINGS> line in DAT file.");
		ue.addDebugInfo("Text", strSettingsText);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void CIDShieldTester::handleTestFolder(const string& strRulesFile, const string& strImageDir, 
										string strFoundVOAFile, string strExpectedVOAFile,
										const string& strCurrentDatFileName)
{
	// allow rules file to be defined as relative path [p16 #2343]
	string strAbsoluteRulesFile = getAbsoluteFileName(strCurrentDatFileName, strRulesFile, true);

	// check for relative path to image directory, 
	// and if relative make absolute [p16 #2343]
	string strAbsoluteImageDir = strImageDir;
	if (!isAbsolutePath(strAbsoluteImageDir))
	{
		string strTemp = getDirectoryFromFullPath(strCurrentDatFileName);
		strAbsoluteImageDir = strTemp + "\\" + strAbsoluteImageDir;
		simplifyPathName(strAbsoluteImageDir);
	}

	// ensure the folder of images exists
	validateFileOrFolderExistence(strAbsoluteImageDir);

	// Get a list of the .tif or .uss files in the directory in order to find images based on the
	// presense of either a tif file or a uss file (but not require a uss file).
	vector<string> vecDirListing;
	getFilesInDir(vecDirListing, strAbsoluteImageDir, "*.uss", false);
	getFilesInDir(vecDirListing, strAbsoluteImageDir, "*.tif", false);

	// If the directory contains tif and uss files, there will be duplicates. Extract the image name
	// from each listing and add it to a set to eliminate duplicates.
	set<string> setImageFilesToTest;
	for (vector<string>::iterator iterFileName = vecDirListing.begin();
		 iterFileName != vecDirListing.end();
		 iterFileName++)
	{
		string strFileName = *iterFileName;
		EFileType eFileType = getFileType(strFileName);
		if (eFileType == kUSSFile)
		{
			strFileName = getPathAndFileNameWithoutExtension(strFileName);
		}
		setImageFilesToTest.insert(strFileName);
	}
	
	// Process each file in the folder
	int nTestCastNum = 1;
	for (set<string>::iterator iterImageFileName = setImageFilesToTest.begin();
		 iterImageFileName != setImageFilesToTest.end();
		 iterImageFileName++)
	{
		string strImageFileName = *iterImageFileName;

		// Items to replace in the full path
		string strFoundVOAFileWithTags = strFoundVOAFile;
		string strExpectedVOAFileWithTags = strExpectedVOAFile;

		// Get the .uss file name from the directory listing
		string strOCRResults = strImageFileName + ".uss";
		if (!isFileOrFolderValid(strOCRResults))
		{
			strOCRResults = strImageFileName;
		}
		
		// Replace <SourceDocName> with the image file name for the ExpectedVOA and FoundVOA files
		replaceVariable(strFoundVOAFileWithTags, "<SourceDocName>", strImageFileName);
		replaceVariable(strExpectedVOAFileWithTags, "<SourceDocName>", strImageFileName);

		// Run a TextFunctionExpander to replace the remaining $DirOf style tags
		TextFunctionExpander tfe;
		string strFoundVOAFileWithExpandedTags = tfe.expandFunctions( strFoundVOAFileWithTags );
		string strExpectedVOAFileWithExpandedTags = tfe.expandFunctions( strExpectedVOAFileWithTags );

		// Execute the test case.
		handleTestCase( strAbsoluteRulesFile, strImageFileName, strOCRResults, 
			strFoundVOAFileWithExpandedTags, strExpectedVOAFileWithExpandedTags, nTestCastNum++, 
			strCurrentDatFileName);
	}// end for
}
//-------------------------------------------------------------------------------------------------
void CIDShieldTester::handleTestCase(const string& strRulesFile, const string& strImageFile, 
									 const string& strSourceDoc, string& strFoundVOAFile, 
									 string& strExpectedVOAFile, const int iTestCaseNum,
									 const string& strCurrentDatFileName)
{
	try
	{
		try
		{
			string strNoteFile = strImageFile + ".nte";

			// Start the test case labeling it as the .dat file
			m_ipResultLogger->StartTestCase(asString( iTestCaseNum ).c_str(), 
				strCurrentDatFileName.c_str(), kAutomatedTestCase);

			// Add the image file as an executable (via double click) note
			m_ipResultLogger->AddTestCaseFile( strImageFile.c_str() );

			// Add the strSourceDoc if it differs from the image file 
			if (strSourceDoc != strImageFile)
			{
				m_ipResultLogger->AddTestCaseFile( strSourceDoc.c_str() );
			}

			// the expected VOA file must always be there
			if (strExpectedVOAFile.empty())
			{
				throw UCLIDException("ELI15246", "Expected VOA file must not be empty.");
			}
			else
			{
				m_ipResultLogger->AddTestCaseFile( strExpectedVOAFile.c_str() );
			}

			// the Found VOA file may or may not be defined.  If it is not defined, that means we have to 
			// run the rules.
			if (!strFoundVOAFile.empty())
			{
				m_ipResultLogger->AddTestCaseFile( strFoundVOAFile.c_str() );
			}

			// Add a note file. If this file exists in the directory and this test case 
			// fails, the node will automatically collapse. 
			m_ipResultLogger->AddTestCaseFile(get_bstr_t(strNoteFile.c_str()));

			// If the expected file exists, load the data from it
			IIUnknownVectorPtr ipExpectedAttributes(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI15195", ipExpectedAttributes != NULL);
			if( ::isFileOrFolderValid( strExpectedVOAFile ) )
			{
				ipExpectedAttributes->LoadFrom( strExpectedVOAFile.c_str(), VARIANT_FALSE );

				// Don't include metadata, clues or DocumentType attributes from the expected
				// voa file as expected redactions.
				m_ipAFUtility->RemoveMetadataAttributes(ipExpectedAttributes);

				// [DataEntry:3610]
				// Removing filtering of clues that was added 11/9/2009. At that time, the new
				// verification task was leaving clues in the output so in order to have the
				// RedactionTester return reasonable results, clues had to be ignored in the
				// expected VOA file. Clues are now being moved to metadata by the verification
				// task unless they are turned on during redaction. Though it should be rare that
				// clues are actually desired in the expected results, it should be allowed.
				// Currently, the SpatialProximityAS automated test is configured in a way that
				// expects clues to be included in the test.
				m_ipAFUtility->QueryAttributes(ipExpectedAttributes, "DocumentType",
					VARIANT_TRUE);
			}
			// If the file does not exist, there are no expected values, throw an exception
			else
			{
				UCLIDException ue("ELI15221", "Invalid file specified for expected values.");
				ue.addDebugInfo("Filename:", strExpectedVOAFile );
				throw ue;
			}

			// Add the file to the list in the appropriate log file
			if( ipExpectedAttributes->Size() > 0 )
			{
				appendToFile( strImageFile, 
					m_strOutputFileDirectory + gstrFILES_WITH_EXPECTED_REDACTIONS );
			}
			else
			{
				appendToFile( strImageFile, 
					m_strOutputFileDirectory + gstrFILES_WITH_NO_EXPECTED_REDACTIONS );
			}

			// Load the found attributes from the VOA file if it exists, otherwise run the rules to compute them
			IIUnknownVectorPtr ipFoundAttributes(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI15196", ipFoundAttributes != NULL);

			// This will be populated by the OCR engine during the call to FindAttributes
			IAFDocumentPtr ipAFDoc(CLSID_AFDocument);
			ASSERT_RESOURCE_ALLOCATION("ELI15205", ipAFDoc != NULL);
			bool bCalculatedFoundValues = false;

			if ( ::isFileOrFolderValid( strFoundVOAFile ) )
			{
				// VOA file exists - so read found attributes from VOA file
				ipFoundAttributes->LoadFrom( strFoundVOAFile.c_str(), VARIANT_FALSE);

				// Do not include metadata attributes in any test.
				m_ipAFUtility->RemoveMetadataAttributes(ipFoundAttributes);

				// Increment the number of files that read from an existing VOA.
				m_iNumFilesWithExistingVOA++;
			}
			// VOA file does not exist - so compute VOA file by running rules
			else
			{
				// Get the attributes by running the rules on the OCR'd document.
				ipFoundAttributes = m_ipAttrFinderEngine->FindAttributes(ipAFDoc, strSourceDoc.c_str(), 
					-1, strRulesFile.c_str(), NULL, VARIANT_FALSE, NULL);
				ASSERT_RESOURCE_ALLOCATION("ELI15204", ipFoundAttributes != NULL);

				// This flag means that the AFDoc contains the document types for this file.
				bCalculatedFoundValues = true;
			}

			// Add this file to the list in the appropriate log file
			if( ipFoundAttributes->Size() > 0 )
			{
				appendToFile( strImageFile, 
					m_strOutputFileDirectory + gstrFILES_WITH_FOUND_REDACTIONS );
			}
			else
			{
				appendToFile( strImageFile, 
					m_strOutputFileDirectory + gstrFILES_WITH_NO_FOUND_REDACTIONS );
			}

			// display found and expected attributes side by side
			string strFoundAttr = m_ipAFUtility->GetAttributesAsString(ipFoundAttributes);
			string strExpectedAttr = m_ipAFUtility->GetAttributesAsString(ipExpectedAttributes);
			m_ipResultLogger->AddTestCaseCompareData("Text of Compared Spatial Strings", 
				"Expected Attributes", strExpectedAttr.c_str(),
				"Found Attributes", strFoundAttr.c_str());

			countDocTypes(ipFoundAttributes, strImageFile, ipAFDoc, bCalculatedFoundValues);

			// update internal stats and determine whether this test case passed
			bool bResult = updateStatisticsAndDetermineTestCaseResult(ipExpectedAttributes, 
				ipFoundAttributes, strImageFile);

			// Append the text of the note at the end of the test case
			string strNote = "";
			if( isFileOrFolderValid( strNoteFile ))
			{
				strNote = ::getTextFileContentsAsString( strNoteFile );
			}
			if( !strNote.empty() )
			{
				// Chop off the end of the note if it extends past 120 characters. 
				// The note file's text will be displayed in it's entirety as a detail note.
				string strTitle = strNote;
				if( strNote.size() > 120 )
				{
					strTitle.erase(120);
					strTitle += "...";
				}

				// Add Test case detail with the note info
				m_ipResultLogger->AddTestCaseDetailNote( get_bstr_t( strTitle.c_str() ),
					get_bstr_t( strNote.c_str() ) ) ;
			}

			m_ipResultLogger->EndTestCase(bResult ? VARIANT_TRUE : VARIANT_FALSE);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI15233");
	}
	catch(UCLIDException& ue)
	{
		m_ipResultLogger->AddTestCaseException(ue.asStringizedByteStream().c_str(), VARIANT_TRUE);
	}
}
//-------------------------------------------------------------------------------------------------
bool CIDShieldTester::updateStatisticsAndDetermineTestCaseResult(IIUnknownVectorPtr ipExpectedAttributes,
																 IIUnknownVectorPtr ipFoundAttributes,
																 const string& strSourceDoc)
{
	ASSERT_ARGUMENT("ELI19879", ipExpectedAttributes != NULL);
	ASSERT_ARGUMENT("ELI19880", ipFoundAttributes != NULL);

	// increment the total number of files processed
	m_ulTotalFilesProcessed++;

	// if there is a type string to filter by, filter the attributes
	if (!m_setTypesToBeTested.empty())
	{
		// filter the expected and found attributes by the specified type
		ipExpectedAttributes = filterAttributesByType(ipExpectedAttributes);
		ipFoundAttributes = filterAttributesByType(ipFoundAttributes);
	}

	// get size of expected attributes vector
	long lExpectedSize = ipExpectedAttributes->Size();

	// increment total number of expected redactions
	m_ulTotalExpectedRedactions += lExpectedSize;

	// if there is at least one expected redaction, increment the count of files
	// containing expected redactions
	if (lExpectedSize > 0)
	{
		m_ulNumFilesWithExpectedRedactions++;
	}

	m_ulTotalPages += getNumberOfPagesInImage(strSourceDoc);

	// if user wants a list of files with HCData, MCData, LCData, etc. then
	// gather the attribute names from the found attributes and
	// output the appropriate files ([p16 #2552] - JDS)
	if (m_bOutputAttributeNameFilesList)
	{
		// counters for HCData, MCData, LCData, and Clues
		unsigned long ulHCData(0), ulMCData(0), ulLCData(0), ulClues(0);

		// check for HCData, MCData, LCData, and clues
		countAttributeNames(ipFoundAttributes, ulHCData, ulMCData, ulLCData, ulClues);

		// if at least 1 HCData append to FilesWithHCData
		if (ulHCData > 0)
		{
			appendToFile(strSourceDoc, m_strOutputFileDirectory + gstrFILES_WITH_HCDATA);
		}

		// if at least 1 MCData append to FilesWithMCData
		if (ulMCData > 0)
		{
			appendToFile(strSourceDoc, m_strOutputFileDirectory + gstrFILES_WITH_MCDATA);
		}

		// if at least 1 LCData append to FilesWithLCData
		if (ulLCData > 0)
		{
			appendToFile(strSourceDoc, m_strOutputFileDirectory + gstrFILES_WITH_LCDATA);
		}

		// if at least 1 Clues append to FilesWithClues
		if (ulClues > 0)
		{
			appendToFile(strSourceDoc, m_strOutputFileDirectory + gstrFILES_WITH_CLUES);
		}
	}

	// compute the number of overlapping expected redactions and the number of pages with expected
	// redactions.
	unsigned long ulNumOverlappingExpected;
	unsigned long ulNumPagesWithRedactions;
	countExpectedOverlapsAndPages(ipExpectedAttributes, ulNumOverlappingExpected,
		ulNumPagesWithRedactions);

	m_ulNumPagesWithExpectedRedactions += ulNumPagesWithRedactions;

	// if there is at least 1 overlapping expected redaction increment the number of
	// files with overlapping expected redactions and add a text node indicating
	// the number of overlapping expected redactions for this test case
	if (ulNumOverlappingExpected > 0)
	{
		m_ulNumFilesWithOverlappingExpectedRedactions++;

		string strNumOverLappingExpected = "Number of overlapping sensitive data items: " +
			asString(ulNumOverlappingExpected);
		m_ipResultLogger->AddTestCaseNote(strNumOverLappingExpected.c_str());
	}

	// Determine if the file is selected for automatic redaction
	bool bSelectedForAutomatedProcess = false;
	bool bSelectedForVerification = false;
	
	if (m_bOutputAutomatedStatsOnly)
	{
		// Ensure the redaction condition is refreshed
		updateRedactionAttributeTester();

		// Test the automated redaction condition
		bSelectedForAutomatedProcess =
			testAttributeCondition(ipFoundAttributes, m_apRedactionTester.get());
	}
	else
	{
		// Ensure the verification condition is refreshed
		updateVerificationAttributeTester();

		bSelectedForVerification =
			testAttributeCondition(ipFoundAttributes, m_apVerificationTester.get());

		// In hybrid mode, analyze the file as an automated file only if it is not
		// also selected for verification.
		if (!bSelectedForVerification && m_bOutputHybridStats)
		{
			// Ensure the redaction condition is refreshed
			updateRedactionAttributeTester();

			// Test the automated redaction condition
			bSelectedForAutomatedProcess =
				testAttributeCondition(ipFoundAttributes, m_apRedactionTester.get());
		}
	}

	if (m_bOutputAutomatedStatsOnly || bSelectedForAutomatedProcess)
	{
		// Analyze the attributes for automated redaction. Also process files not selected for
		// review if computing only automated stats
		return analyzeDataForAutomatedRedaction(ipExpectedAttributes, ipFoundAttributes,
			bSelectedForAutomatedProcess, strSourceDoc);
	}
	else
	{
		// Analyze the attributes for verification based redaction. Include documents that were
		// not otherwise selected for either process in hybrid mode.
		return analyzeDataForVerificationBasedRedaction(ipExpectedAttributes, ipFoundAttributes,
				bSelectedForVerification, strSourceDoc);
	}
}
//-------------------------------------------------------------------------------------------------
bool CIDShieldTester::analyzeDataForVerificationBasedRedaction(
														IIUnknownVectorPtr ipExpectedAttributes,
														IIUnknownVectorPtr ipFoundAttributes,
														bool bSelectedForVerification,
														const string& strSourceDoc)
{
	ASSERT_ARGUMENT("ELI18507", ipExpectedAttributes != NULL);
	ASSERT_ARGUMENT("ELI18508", ipFoundAttributes != NULL);

	// analyze the expected and found attributes 
	CIDShieldTester::TestCaseStatistics testCaseStatistics = analyzeExpectedAndFoundAttributes(
		ipExpectedAttributes, ipFoundAttributes, bSelectedForVerification, strSourceDoc);

	// update total statistics
	verificationStatistics += testCaseStatistics;

	if (bSelectedForVerification)
	{
		// increment the number of files that would be selected for review
		m_ulNumFilesSelectedForReview++;

		// Get the size of the expected attribute collection
		long lExpectedSize = ipExpectedAttributes->Size();

		// increment the number of expected redactions in files selected for review
		m_ulNumExpectedRedactionsInReviewedFiles += lExpectedSize;

		// If there are one or more redactions and at least one filtered attribute, then add
		// this file to the FilesCorrectlySelectedForReview.txt file.
		if(lExpectedSize > 0)
		{
			appendToFile( strSourceDoc, m_strOutputFileDirectory + gstrFILES_CORRECTLY_SELECTED_FOR_REVIEW );
		}
		else
		{
			// There are one or more found redactions, but no expected redactions
			appendToFile( strSourceDoc, m_strOutputFileDirectory + gstrFILES_INCORRECTLY_SELECTED_FOR_REVIEW);
		}
	}
	else
	{
		// The document has no found attributes, but it does have an expected attribute
		if (ipExpectedAttributes->Size() > 0)
		{
			appendToFile( strSourceDoc, m_strOutputFileDirectory + gstrFILES_MISSED_BEING_SELECTED_FOR_REVIEW);
		}
		else
		{
			// The case of no expected and no found attributes is intentionally not tracked.
		}
	}

	return testCaseStatistics.m_bTestCaseResult;
}
//-------------------------------------------------------------------------------------------------
bool CIDShieldTester::analyzeDataForAutomatedRedaction(IIUnknownVectorPtr ipExpectedAttributes,
													   IIUnknownVectorPtr ipFoundAttributes,
													   bool bSelectedForAutomatedProcess,
													   const string& strSourceDoc)
{
	ASSERT_ARGUMENT("ELI18509", ipExpectedAttributes != NULL);
	ASSERT_ARGUMENT("ELI18510", ipFoundAttributes != NULL);

	// analyze the expected and found attributes 
	CIDShieldTester::TestCaseStatistics testCaseStatistics = analyzeExpectedAndFoundAttributes(
		ipExpectedAttributes, ipFoundAttributes, bSelectedForAutomatedProcess, strSourceDoc);

	// update total statistics
	automatedStatistics += testCaseStatistics;

	if (bSelectedForAutomatedProcess)
	{
		m_ulNumFilesAutomaticallyRedacted++;
		
		// Get the size of the expected attribute collection
		long lExpectedSize = ipExpectedAttributes->Size();

		// increment the number of expected redactions in files selected for redaction
		m_ulNumExpectedRedactionsInRedactedFiles += lExpectedSize;

		// If there are one or more redactions and at least one filtered attribute, then add
		// this file to the FilesCorrectlySelectedForReview.txt file.
		if(lExpectedSize > 0)
		{
			appendToFile(strSourceDoc,
				m_strOutputFileDirectory + gstrFILES_CORRECTLY_SELECTED_FOR_REDACTION);
		}
		else
		{
			// There are one or more found redactions, but no expected redactions
			appendToFile(strSourceDoc,
				m_strOutputFileDirectory + gstrFILES_INCORRECTLY_SELECTED_FOR_REDACTION);
		}
	}
	else
	{
		// The document has no found attributes, but it does have an expected attribute
		// and we are not outputting hybrid statistics
		if(ipExpectedAttributes->Size() > 0)
		{
			appendToFile(strSourceDoc,
				m_strOutputFileDirectory + gstrFILES_MISSED_BEING_SELECTED_FOR_REDACTION);
		}
		else
		{
			// The case of no expected and no found attributes is intentionally not tracked.
		}
	}

	return testCaseStatistics.m_bTestCaseResult;
}
//-------------------------------------------------------------------------------------------------
bool CIDShieldTester::spatiallyMatches(ISpatialStringPtr ipExpectedSS,
									   ISpatialStringPtr ipFoundSS)
{
	// If the expected Spatial string object is not spatial, then there is no reason to test 
	// for a found raster zone that matches it. No raster zone will be able to match a non-spatial
	// spatial string. However, since this is a non-spatial string, it is counted as a failure and 
	// the tests should move on to the next document.
	IIUnknownVectorPtr ipExpectedRasterZones;

	if( ipExpectedSS->HasSpatialInfo() == VARIANT_FALSE )
	{
		return false;
	}
	else
	{
		ipExpectedRasterZones = ipExpectedSS->GetOriginalImageRasterZones();
		ASSERT_RESOURCE_ALLOCATION("ELI15186", ipExpectedRasterZones != NULL);		
	}

	IIUnknownVectorPtr ipFoundRasterZones;
	if( ipFoundSS->HasSpatialInfo() == VARIANT_FALSE )
	{
		return false;
	}
	else
	{
		ipFoundRasterZones = ipFoundSS->GetOriginalImageRasterZones();
		ASSERT_RESOURCE_ALLOCATION("ELI15187", ipFoundRasterZones != NULL);
	}

	// For each expected raster zone in the vector, verify that there is some found
	// raster zone that matches it
	long lExpectedSize = ipExpectedRasterZones->Size();
	for ( int e = 0; e < lExpectedSize; e++)
	{
		// Get each individual raster zone
		IRasterZonePtr ipExpectedRZ = ipExpectedRasterZones->At( e );
		ASSERT_RESOURCE_ALLOCATION("ELI15188", ipExpectedRZ != NULL);

		bool bFoundMatch = false;
		
		// Check it against each found raster zone for a match within the limits of the overlap
		long lFoundSize = ipFoundRasterZones->Size();
		for( int f = 0; f < lFoundSize; f++)
		{
			IRasterZonePtr ipFoundRZ = ipFoundRasterZones->At( f );
			ASSERT_RESOURCE_ALLOCATION("ELI15189", ipFoundRZ != NULL);

			// Get the area of the overlap
			double dOverlapArea = ipExpectedRZ->GetAreaOverlappingWith( ipFoundRZ );

			// Get the area of the expected zone
			double dExpectedArea = ipExpectedRZ->GetArea();

			// Prevent division by zero
			double dPercentageOverlap = 0;
			if( dExpectedArea > 0 )
			{
				// Calculate the percentage of the overlap
				dPercentageOverlap = ( dOverlapArea / dExpectedArea ) * 100;
			}

			// If the overlap percentage is within the specified bounds, the raster matches
			if( dPercentageOverlap >= m_dOverlapLeniencyPercent)
			{
				bFoundMatch = true;
				break;
			}
		}// end for found
		
		// If there was an expected value that did not have a matching found value, then these two 
		// attributes are not spatially equivalent, return false.
		if(! bFoundMatch )
		{
			return false;
		}
	}// end for expeceted

	// If all the expected attributes had a match, return true.
	return true;
}
//-------------------------------------------------------------------------------------------------
CIDShieldTester::TestCaseStatistics CIDShieldTester::analyzeExpectedAndFoundAttributes(
	IIUnknownVectorPtr ipExpectedAttributes, IIUnknownVectorPtr ipFoundAttributes,
	bool bDocumentSelected, const string& strSourceDoc)
{
	ASSERT_ARGUMENT("ELI18511", ipExpectedAttributes != NULL);
	ASSERT_ARGUMENT("ELI18512", ipFoundAttributes != NULL);

	CIDShieldTester::TestCaseStatistics testCaseStatistics;
	testCaseStatistics.m_ulTotalExpectedRedactions = ipExpectedAttributes->Size();

	// If the document was selected, find the attributes that would be redacted by default and use
	// those attributes as the basis for the statistics.
	IIUnknownVectorPtr ipAutoRedactedAttributes = NULL;
	if (bDocumentSelected)
	{
		// restrict the found attributes by the query that will be used for automated redaction
		ipAutoRedactedAttributes = m_ipAFUtility->QueryAttributes(
			ipFoundAttributes, m_strRedactionQuery.c_str(), VARIANT_FALSE);

		testCaseStatistics.m_ulExpectedRedactionsInSelectedFiles = ipExpectedAttributes->Size();
		testCaseStatistics.m_ulFoundRedactions = ipAutoRedactedAttributes->Size();
	}
	// If the document was not selected for verification the total number of auto-redacted
	// attributes is zero.
	else
	{
		// Create an empty attribute vector, all expected attributes are a miss
		ipAutoRedactedAttributes.CreateInstance(CLSID_IUnknownVector);
	}
	ASSERT_RESOURCE_ALLOCATION("ELI29334", ipAutoRedactedAttributes != NULL);
	
	if (testCaseStatistics.m_ulFoundRedactions != 0 &&
		testCaseStatistics.m_ulTotalExpectedRedactions != 0)
	{
		// declare an array of MatchInfos
		SafeTwoDimensionalArray<MatchInfo> s2dMatchInfos(
			testCaseStatistics.m_ulTotalExpectedRedactions, testCaseStatistics.m_ulFoundRedactions);

		//--------------------------------------------------------
		// Compute match info section
		//--------------------------------------------------------
		for (unsigned long ulExpectedIndex = 0;
			 ulExpectedIndex < testCaseStatistics.m_ulTotalExpectedRedactions;
			 ulExpectedIndex++)
		{
			for (unsigned long ulFoundIndex = 0;
				 ulFoundIndex < testCaseStatistics.m_ulFoundRedactions;
				 ulFoundIndex++)
			{
				getMatchInfo(s2dMatchInfos[ulExpectedIndex][ulFoundIndex],
					ipExpectedAttributes->At(ulExpectedIndex), 
					ipAutoRedactedAttributes->At(ulFoundIndex));
			}
		}

		// Keep track of all found redactions which overlap and expected redaction
		set<unsigned long> setOverlappingFounds;

		// Keep track of all redactions which are over redactions to at least one expected redaction.
		set<unsigned long> setOverRedactions;

		//--------------------------------------------------------
		// Compute data from the expected attribute perspective
		//--------------------------------------------------------
		// compute correct redactions, over-redactions, under-redactions, and missed redactions
		for (unsigned long ulExpectedIndex = 0;
			 ulExpectedIndex < testCaseStatistics.m_ulTotalExpectedRedactions;
			 ulExpectedIndex++)
		{
			// Only one "correct" redaction count is allowed per expected redaction. Keep track of
			// whether one has been found for this expected.
			bool bFoundCorrectRedaction = false;

			for (unsigned long ulFoundIndex = 0;
				 ulFoundIndex < testCaseStatistics.m_ulFoundRedactions;
				 ulFoundIndex++)
			{
				// get the match info
				MatchInfo miTemp = s2dMatchInfos(ulExpectedIndex, ulFoundIndex);

				// check if there is any area of overlap
				if (!MathVars::isZero(miTemp.m_dAreaOfOverlap))
				{
					// Keep track of overlapping founds (anything not marked overlapping will be
					// marked as a false positive).
					setOverlappingFounds.insert(ulFoundIndex);
					
					// check the area of overlap
					if ((miTemp.getPercentOfExpectedAreaRedacted() < m_dOverlapLeniencyPercent))
					{
						// less than the leniency then it is under redacted
						RecordStatistic(gstrTEST_UNDER_REDACTED,
							s2dMatchInfos(0, ulFoundIndex).m_ipFoundAttribute, "Found",
							testCaseStatistics.m_ulNumUnderRedactions);
					}
					else
					{
						// The expected redaction is completely covered. Record a "correct" redaction
						// unless one has already been recorded for this expected redaction.
						if (!bFoundCorrectRedaction)
						{
							bFoundCorrectRedaction = true;

							RecordStatistic(gstrTEST_CORRECT_REDACTED,
								s2dMatchInfos(ulExpectedIndex, 0).m_ipExpectedAttribute, "Expected",
								testCaseStatistics.m_ulNumCorrectRedactions);
						}
						
						// Look to see if we have already determined the found attribute to be an
						// over-redaction.
						bool bOverRedaction =
							setOverRedactions.find(ulFoundIndex) != setOverRedactions.end();

						// If not, calculate whether it is an over-redaction.
						if (!bOverRedaction)
						{
							bOverRedaction = getIsOverredaction(s2dMatchInfos, ulFoundIndex,
								testCaseStatistics.m_ulTotalExpectedRedactions);
						}

						if (bOverRedaction)
						{
							// Record an over-redaction.
							setOverRedactions.insert(ulFoundIndex);

							RecordStatistic(gstrTEST_OVER_REDACTED,
								s2dMatchInfos(0, ulFoundIndex).m_ipFoundAttribute, "Found",
								testCaseStatistics.m_ulNumOverRedactions);
						}
					}
				}
			}

			if (!bFoundCorrectRedaction)
			{
				// Record a missed redaction.
				RecordStatistic(gstrTEST_MISSED_REDACTED,
					s2dMatchInfos(ulExpectedIndex, 0).m_ipExpectedAttribute, "Expected",
					testCaseStatistics.m_ulNumMisses);
			}
		}

		//--------------------------------------------------------
		// Record all false positives
		//--------------------------------------------------------
		for (unsigned long ulFoundIndex = 0;
			 ulFoundIndex < testCaseStatistics.m_ulFoundRedactions;
			 ulFoundIndex++)
		{
			// A found redaction is a false positive if it was not already found to overlap an
			// expected redaction.
			if (setOverlappingFounds.find(ulFoundIndex) == setOverlappingFounds.end())
			{
				RecordStatistic(gstrTEST_FALSE_POSITIVE,
					s2dMatchInfos(0, ulFoundIndex).m_ipFoundAttribute, "Found",
					testCaseStatistics.m_ulNumFalsePositives);
			}
		}
	}
	// m_ulTotalExpectedRedactions == 0 || m_ulFoundRedactions == 0
	else
	{
		// if lFoundSize is 0 then all expected attributes are misses
		// if lExpectedSize is 0 then all found attributes are false positives
		testCaseStatistics.m_ulNumMisses = testCaseStatistics.m_ulTotalExpectedRedactions;
		testCaseStatistics.m_ulNumFalsePositives = testCaseStatistics.m_ulFoundRedactions;

		// check for the CreateTestOutputVOAFile
		if (m_ipTestOutputVOAVector != NULL)
		{
			// if lExpectedSize > 0 then all expected attributes are misses
			for (unsigned long i = 0; i < testCaseStatistics.m_ulTotalExpectedRedactions; i++)
			{
				addAttributeToTestOutputVOA(ipExpectedAttributes->At(i),
					gstrTEST_MISSED_REDACTED, "Expected");
			}

			// if lFoundSize > 0 then all found attributes are false positives
			for (unsigned long i = 0; i < testCaseStatistics.m_ulFoundRedactions; i++)
			{
				addAttributeToTestOutputVOA(ipAutoRedactedAttributes->At(i),
					gstrTEST_FALSE_POSITIVE, "Found");
			}
		}
	}

	//--------------------------------------------------------
	// File output section 
	//--------------------------------------------------------
	// if at least 1 correctly found then output to files with correct redactions
	if (testCaseStatistics.m_ulNumCorrectRedactions > 0)
	{
		appendToFile(strSourceDoc,
			m_strOutputFileDirectory + gstrFILES_WITH_CORRECT_REDACTIONS);
	}

	// if at least 1 over redaction then output to files with over redactions
	if (testCaseStatistics.m_ulNumOverRedactions > 0)
	{
		appendToFile(strSourceDoc, 
			m_strOutputFileDirectory + gstrFILES_WITH_OVER_REDACTIONS);
	}

	// if at least 1 under redaction then output to files with under redactions
	if (testCaseStatistics.m_ulNumUnderRedactions > 0)
	{
		appendToFile(strSourceDoc, 
			m_strOutputFileDirectory + gstrFILES_WITH_UNDER_REDACTIONS);
	}

	// if at least 1 false positive then output to files with false positives
	if (testCaseStatistics.m_ulNumFalsePositives > 0)
	{
		appendToFile(strSourceDoc,
			m_strOutputFileDirectory + gstrFILES_WITH_FALSE_POSITIVES);
	}

	// if at least 1 redaction missed then output to files with missed redactions
	if (testCaseStatistics.m_ulNumMisses > 0)
	{
		appendToFile(strSourceDoc,
			m_strOutputFileDirectory + gstrFILES_WITH_MISSED_REDACTIONS);
	}

	// check for output test voa file
	if (m_ipTestOutputVOAVector != NULL)
	{
		// write the testoutput.voa file
		string strTestOutputFile = strSourceDoc + ".testoutput.voa";
		m_ipTestOutputVOAVector->SaveTo(strTestOutputFile.c_str(), VARIANT_TRUE);

		// clear the vector
		m_ipTestOutputVOAVector->Clear();

		// add a link to the output data
		m_ipResultLogger->AddTestCaseFile(strTestOutputFile.c_str());
	}

	// test case is considered failed if:
	// 1 or more redactions was missed 
	// OR if there are 1 or more under redactions
	// OR if there are 1 or more over redactions
	// OR if there are 1 or more false positives
	if (testCaseStatistics.m_ulNumMisses > 0
		|| testCaseStatistics.m_ulNumUnderRedactions > 0
		|| testCaseStatistics.m_ulNumOverRedactions > 0
		|| testCaseStatistics.m_ulNumFalsePositives > 0)
	{
		testCaseStatistics.m_bTestCaseResult = false;
	}
	else
	{
		testCaseStatistics.m_bTestCaseResult = true;
	}	
	
	return testCaseStatistics;
}
//-------------------------------------------------------------------------------------------------
bool CIDShieldTester::getIsOverredaction(const SafeTwoDimensionalArray<MatchInfo>& s2dMatchInfos,
										 unsigned long ulFoundIndex, unsigned long ulTotalExpected)
{
	// Iterate through all expected redactions to determine the total area of overlap with the
	// specified found redaction.
	double dTotalOverlapArea = 0.0;
	for (unsigned long ulExpectedIndex = 0; ulExpectedIndex < ulTotalExpected; ulExpectedIndex++)
	{
		// get the match info for these two attributes and add the area of
		// overlap
		MatchInfo miOverlap = s2dMatchInfos(ulExpectedIndex, ulFoundIndex);
		dTotalOverlapArea += miOverlap.m_dAreaOfOverlap;
	}

	// get the area of the found redaction
	double dAreaOfFoundRedaction = 
		s2dMatchInfos(0, ulFoundIndex).m_dAreaOfFoundRedaction;

	double dERAP = 0.0;

	// protect against divide by zero
	if (!MathVars::isZero(dAreaOfFoundRedaction))
	{
		// compute the excess redaction area percentage
		dERAP = (dAreaOfFoundRedaction - dTotalOverlapArea) / 
			dAreaOfFoundRedaction;
		dERAP *= 100.0;

		// get absolute value
		dERAP = fabs(dERAP);

		if (dERAP >= m_dOverRedactionERAP)
		{
			return true;
		}
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
void CIDShieldTester::RecordStatistic(const string& strLabel, IAttributePtr ipRelatedAttribute,
									  const string &strSourceVOA, unsigned long& rulCount)
{
	// Output the provided attribute as long as an output vector and label are available.
	if (m_ipTestOutputVOAVector != NULL && strLabel != "")
	{
		addAttributeToTestOutputVOA(ipRelatedAttribute, strLabel, strSourceVOA);
	}

	// Incremement the provided counter.
	rulCount++;
}
//-------------------------------------------------------------------------------------------------
void CIDShieldTester::displaySummaryStatistics()
{
	m_ipResultLogger->StartTestCase("", "Summary Statistics", kOtherTestCase);

	// The string that will be put into gstrFILE_FOR_STATISTICS (statistics file) which is a textual
	// version of all the statistics
	string strStatisticSummary = "Image analysis:\r\n";

	CString zTemp;
	zTemp.Format("Number of files tested: %d", m_ulTotalFilesProcessed);
	m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
	strStatisticSummary += zTemp + "\r\n";

	zTemp.Format("Number of pages tested: %d", m_ulTotalPages);
	m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
	strStatisticSummary += zTemp + "\r\n";

	// Calculate the number of files with expected redactions
	zTemp.Format("Number of files containing sensitive data items: %d (%0.1f%%)",	
					m_ulNumFilesWithExpectedRedactions, 
					getRatioAsPercentOfTwoLongs(m_ulNumFilesWithExpectedRedactions, m_ulTotalFilesProcessed));	
	m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
	strStatisticSummary += zTemp + "\r\n";

	// Calculate the number of pages with expected redactions
	zTemp.Format("Number of pages containing sensitive data items: %d (%0.1f%%)",	
					m_ulNumPagesWithExpectedRedactions, 
					getRatioAsPercentOfTwoLongs(m_ulNumPagesWithExpectedRedactions, m_ulTotalPages));	
	m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
	strStatisticSummary += zTemp + "\r\n";

	zTemp.Format("Number of sensitive data items: %d", m_ulTotalExpectedRedactions);
	m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
	strStatisticSummary += zTemp + "\r\n";

	zTemp.Format("Number of documents with overlapping sensitive data items: %d ", 
		m_ulNumFilesWithOverlappingExpectedRedactions);
	m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
	strStatisticSummary += zTemp + "\r\n";

	strStatisticSummary += "\r\nWorkflow:\r\n";

	if (m_bOutputAutomatedStatsOnly)
	{
		strStatisticSummary += "Analysis type: Automated redaction\r\n";
	}
	else if (m_bOutputHybridStats)
	{
		strStatisticSummary += "Analysis type: Hybrid\r\n";
	}
	else
	{
		strStatisticSummary += "Analysis type: Standard verification\r\n";
	}

	// If only automated stats were computed, there are no verification stats to report.
	if (!m_bOutputAutomatedStatsOnly)
	{
		string strDataTypes = m_strVerificationCondition;
		replaceVariable(strDataTypes, "|", ", ");

		strStatisticSummary += "Manually review: Files that contain "
			+ m_strVerificationQuantifier + " " + strDataTypes + "\r\n";
	}

	if (m_bOutputAutomatedStatsOnly || m_bOutputHybridStats)
	{
		string strDataTypes = m_strRedactionCondition;
		replaceVariable(strDataTypes, "|", ", ");

		strStatisticSummary += "Automatically redact: Files that contain " +
			m_strRedactionQuantifier + " " + strDataTypes + "\r\n";
	}

	string strDataTypes = m_strRedactionQuery;
	replaceVariable(strDataTypes, "|", ", ");

	strStatisticSummary += "Default to redacting the following: " + strDataTypes + "\r\n";

	// [FlexIDSCore: 3798]
	// If limiting testing to specified doc types, indicates the doc types being tested.
	if (!m_bOutputAutomatedStatsOnly && !m_setDocTypesToBeVerified.empty())
	{
		strStatisticSummary += "Document types to be verified: " +
			getSetAsDelimitedList(m_setDocTypesToBeVerified) + "\r\n";
	}
	if ((m_bOutputAutomatedStatsOnly || m_bOutputHybridStats) &&
		!m_setDocTypesToBeAutomaticallyRedacted.empty())
	{
		strStatisticSummary += "Document types to be automatically redacted: " +
			getSetAsDelimitedList(m_setDocTypesToBeAutomaticallyRedacted) + "\r\n";
	}

	// If limiting testing to specified doc type, indicates the doc type being tested.
	if (!m_setTypesToBeTested.empty())
	{
		strStatisticSummary += "Limit data types to be tested to: " + 
			getSetAsDelimitedList(m_setTypesToBeTested) + "\r\n";
	}

	// Compute the number of correct redactions based on whether this is hyrbid stats or not
	unsigned long ulTotalNumberOfCorrectRedactions = automatedStatistics.m_ulNumCorrectRedactions +
		verificationStatistics.m_ulExpectedRedactionsInSelectedFiles;

	strStatisticSummary += "\r\nFinal results of above workflow:\r\n";

	// Note for number of redactions found
	zTemp.Format(
		"Number of sensitive data items redacted after processing: %d (%0.1f%%)",
			ulTotalNumberOfCorrectRedactions, getRatioAsPercentOfTwoLongs(
				ulTotalNumberOfCorrectRedactions, m_ulTotalExpectedRedactions));
	m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
	strStatisticSummary += zTemp + "\r\n";

	if (m_bOutputAutomatedStatsOnly || m_bOutputHybridStats)
	{
		// Note for the number of false positives found
		zTemp.Format("Number of false positives in redacted images: %d ",
			automatedStatistics.m_ulNumFalsePositives);
		m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
		strStatisticSummary += zTemp + "\r\n";

		// if m_ulNumFalsePositives == 0, append "(ROCE = n/a)"
		if(automatedStatistics.m_ulNumFalsePositives <= 0)
		{
			zTemp = "Ratio of correctly redacted items to false positives: n/a";
		}
		// else ROCE > 0, append "(ROCE = N)"
		else
		{
			// Use integer division to get a whole number ratio as a result
			zTemp.Format("Ratio of correctly redacted items to false positives: %0.1f",
				((double)ulTotalNumberOfCorrectRedactions /
				 (double)automatedStatistics.m_ulNumFalsePositives));
		}
		m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
		strStatisticSummary += zTemp + "\r\n";
	}

	if (m_bOutputHybridStats || !m_bOutputAutomatedStatsOnly)
	{
		strStatisticSummary += "\r\nVerification efficiency:\r\n";

		// Label for Number of files selected for review
		zTemp.Format("Number of files selected for review: %d (%0.1f%%)",
			m_ulNumFilesSelectedForReview, getRatioAsPercentOfTwoLongs(
				m_ulNumFilesSelectedForReview, m_ulTotalFilesProcessed));
		m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
		strStatisticSummary += zTemp + "\r\n";

		if (m_bOutputHybridStats)
		{
			// Make a note for the number of expected redactions in the reviewed files
			// (This number would be a duplicate statistic in a standard workflow).
			zTemp.Format("Number of sensitive data items in files presented for review: %d (%0.1f%%)",
				verificationStatistics.m_ulExpectedRedactionsInSelectedFiles,
				getRatioAsPercentOfTwoLongs(
					verificationStatistics.m_ulExpectedRedactionsInSelectedFiles,
					m_ulTotalExpectedRedactions));
			m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
			strStatisticSummary += zTemp + "\r\n";
		}

		strStatisticSummary += displayStatisticsSection(verificationStatistics, true);
	}

	if (m_bOutputHybridStats || m_bOutputAutomatedStatsOnly)
	{
		strStatisticSummary += "\r\nAutomated redaction efficiency:\r\n";
		
		// Label for Number of files selected for review
		zTemp.Format("Number of automatically redacted files: %d (%0.1f%%)",
			m_ulNumFilesAutomaticallyRedacted, getRatioAsPercentOfTwoLongs(
				m_ulNumFilesAutomaticallyRedacted, m_ulTotalFilesProcessed));
		m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
		strStatisticSummary += zTemp + "\r\n";

		// Make a note for the number of expected redactions in the automatically redacted files
		zTemp.Format("Number of sensitive data items in automatically redacted files: %d (%0.1f%%)",
			automatedStatistics.m_ulExpectedRedactionsInSelectedFiles, 
			getRatioAsPercentOfTwoLongs(automatedStatistics.m_ulExpectedRedactionsInSelectedFiles,
				m_ulTotalExpectedRedactions));
		m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
		strStatisticSummary += zTemp + "\r\n";

		strStatisticSummary += displayStatisticsSection(automatedStatistics, false);
	}

	// If the number of correct redactions match the number of total expected redactions, then 
	// the test was 100% successful.
	bool bOneHundredPercentSuccess = false;
	bOneHundredPercentSuccess = 
		automatedStatistics.m_bTestCaseResult && verificationStatistics.m_bTestCaseResult &&
		(m_ulTotalExpectedRedactions == ulTotalNumberOfCorrectRedactions);

	m_ipResultLogger->EndTestCase(bOneHundredPercentSuccess ? VARIANT_TRUE : VARIANT_FALSE);

	// Output the printed statistics to a file so they can be viewed later without running
	// the test again.
	appendToFile(strStatisticSummary, m_strOutputFileDirectory + gstrFILE_FOR_STATISTICS);

	// Display the document type statistics
	displayDocumentTypeStats();
}
//-------------------------------------------------------------------------------------------------
string CIDShieldTester::displayStatisticsSection(
											const CIDShieldTester::TestCaseStatistics& statistics,
											bool bVerificationStatistics)
{
	string strStatisticsSection = "";
	CString zTemp;

	zTemp = CString("Number of correct redactions") +
		(bVerificationStatistics ? " presented to reviewers" : "");
	zTemp.Format(zTemp + ": %d", statistics.m_ulNumCorrectRedactions);
	m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
	strStatisticsSection += zTemp + "\r\n";

	zTemp = CString("Number of over-redactions") +
		(bVerificationStatistics ? " presented to reviewers" : "");
	zTemp.Format(zTemp + ": %d", statistics.m_ulNumOverRedactions);
	m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
	strStatisticsSection += zTemp + "\r\n";

	zTemp = CString("Number of under-redactions") +
		(bVerificationStatistics ? " presented to reviewers" : "");
	zTemp.Format(zTemp + ": %d", statistics.m_ulNumUnderRedactions);
	m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
	strStatisticsSection += zTemp + "\r\n";

	// Note for the number of false positives found
	zTemp = CString("Number of false positives") +
		(bVerificationStatistics ? " presented to reviewers" : "");
	zTemp.Format(zTemp + ": %d ", statistics.m_ulNumFalsePositives);
	m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
	strStatisticsSection += zTemp + "\r\n";

	if(statistics.m_ulNumFalsePositives <= 0)
	{
		zTemp = CString("Ratio of correctly redacted items to false positives: n/a");
	}
	else
	{
		zTemp.Format("Ratio of correctly redacted items to false positives: %0.1f",
			((double)statistics.m_ulNumCorrectRedactions /
				(double)statistics.m_ulNumFalsePositives));
	}
	m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
	strStatisticsSection += zTemp + "\r\n";

	return strStatisticsSection;
}
//-------------------------------------------------------------------------------------------------
double CIDShieldTester::getRatioAsPercentOfTwoLongs(unsigned long ulNumerator, 
													unsigned long ulDenominator)
{
	if( ulDenominator > 0 )
	{
		return (100.0* ((double)ulNumerator / (double)ulDenominator) );
	}
	else
	{
		return 0.0;
	}
}
//-------------------------------------------------------------------------------------------------
void CIDShieldTester::displayDocumentTypeStats()
{
	// Create a string to piece together for the statistics log file
	string strStatsForLogFile = "";
	
	// Display the number of documents classified for each type
	string strDocumentTypeLabel = "Document type distribution:";
	strStatsForLogFile += strDocumentTypeLabel + "\r\n";

	m_ipResultLogger->StartTestCase("", strDocumentTypeLabel.c_str(), kOtherTestCase);
	map<string,int>::const_iterator iter;
	long nDocsClassified = 0;

	// Iterate through the document classifier map that was created above
	for( iter = m_mapDocTypeCount.begin(); iter != m_mapDocTypeCount.end(); ++iter )
	{
		// Get the doc type and the number of them found from the map
		string strType = iter->first;
		int iNumber = iter->second;

		// Create the string to display
		strType.append( ": " + asString(iNumber) );

		// Make a node in the result tree for this doc type
		m_ipResultLogger->AddTestCaseNote( strType.c_str() );
		strStatsForLogFile += strType + "\r\n";

		// Total number of documents classified
		nDocsClassified+= iNumber;
	}

	// Output the printed statistics to a file so they can be viewed later without running
	// the test again.
	appendToFile( strStatsForLogFile, m_strOutputFileDirectory + gstrFILE_FOR_STATISTICS );
	
	// End the test case
	m_ipResultLogger->EndTestCase(VARIANT_TRUE);
}
//-------------------------------------------------------------------------------------------------
void CIDShieldTester::countDocTypes(IIUnknownVectorPtr ipFoundAttributes, const string& strSourceDoc,
									IAFDocumentPtr ipAFDoc, bool bCalculatedFoundValues)
{
	// Get the document types from the found attributes vector
	IIUnknownVectorPtr ipDocTypes= m_ipAFUtility->QueryAttributes( ipFoundAttributes, 
								gstrDOCTYPEQUERY.c_str(), VARIANT_FALSE);
	ASSERT_RESOURCE_ALLOCATION("ELI15769", ipDocTypes != NULL);
	
	if( bCalculatedFoundValues && ipDocTypes->Size() == 0)
	{	
		// Get the doc types from the AFDoc and put them into attributes in the Found IIUnknownVectorPtr
		IStrToObjectMapPtr ipObjMap = ipAFDoc->GetObjectTags();
		ASSERT_RESOURCE_ALLOCATION("ELI15772", ipObjMap != NULL);

		// If there is at least one item in the obj map
		if( ipObjMap->Size > 0 )
		{
			if( ipObjMap->Contains( get_bstr_t(DOC_TYPE)) == VARIANT_TRUE )
			{
				// Get the variant vector for all the doc types present
				IVariantVectorPtr ipDocumentTypes = ipObjMap->GetValue(get_bstr_t(DOC_TYPE));
				ASSERT_RESOURCE_ALLOCATION("ELI15771", ipDocumentTypes != NULL);

				// Use the number of document types found to control the loop
				long nDocTypes = ipDocumentTypes->Size;
				for( int x = 0; x < nDocTypes; x++ )
				{
					// Get the string for the doc type
					string strDocTypeValue = asString (ipDocumentTypes->GetItem(x).bstrVal);
					
					// Create an attribute for this doc type and put it into the found attributes vector
					IAttributePtr ipAttr(CLSID_Attribute);
					ASSERT_RESOURCE_ALLOCATION("ELI15767", ipAttr != NULL);

					// Set the type of the attribute to doc type
					ipAttr->Name = get_bstr_t( "DocumentType" );
					
					// Create a spatial string to use as the value of the attribute
					ISpatialStringPtr ipSS(CLSID_SpatialString);
					ASSERT_RESOURCE_ALLOCATION("ELI15768", ipSS != NULL);
					ipSS->CreateNonSpatialString(strDocTypeValue.c_str(), "");

					// Apply the spatial string as the attribute's value
					ipAttr->Value = ipSS;

					// Insert the document type attribute into the found attributes vector.
					ipFoundAttributes->PushBackIfNotContained((IUnknownPtr)ipAttr );
				}
			}
		}
	}

	// Get the document types from the found attributes vector
	ipDocTypes= m_ipAFUtility->QueryAttributes( ipFoundAttributes, 
							gstrDOCTYPEQUERY.c_str(), VARIANT_FALSE);
	ASSERT_RESOURCE_ALLOCATION("ELI15207", ipDocTypes != NULL);

	// Get the number of document types
	long nDocTypes = ipDocTypes->Size();

	// If there is no doc type, then add this file to the unclassified log file and 
	// make an entry in the doc type map for the unclassified type.
	if( nDocTypes == 0 )
	{
		string strDocLogFilePath = m_strOutputFileDirectory + gstrUNCLASSIFIED_DOC_TYPE_FILES;
		appendToFile( strSourceDoc, strDocLogFilePath );

		const string strDocType = "Unclassified";

		// Add the Unclassified Doc Type to the test logger as a test case note
		m_ipResultLogger->AddTestCaseNote( strDocType.c_str() );

		// If the unclassified doc type does not yet exist, enter it into the table
		if (m_mapDocTypeCount.find(strDocType) == m_mapDocTypeCount.end())
		{
			m_mapDocTypeCount[strDocType] = 1;
		}
		else
		{
			m_mapDocTypeCount[strDocType]++;
		}
	}
	else
	{
		// For each type this document is classified as
		for( int i = 0; i < nDocTypes; i++ )
		{
			// Get the attribute
			IAttributePtr ipAtt = ipDocTypes->At( i );
			ASSERT_RESOURCE_ALLOCATION("ELI15208", ipAtt != NULL);

			// Get the spatial string that contains the doc type info
			ISpatialStringPtr ipSpatial = ipAtt->Value;
			ASSERT_RESOURCE_ALLOCATION("ELI15209", ipSpatial != NULL);

			// Get the string of the doc type
			string strDocType = asString(ipSpatial->String);

			// Add the Doc Type to the test logger as a test case note
			m_ipResultLogger->AddTestCaseNote( strDocType.c_str() );

			// If this doc type does not yet exist, enter it into the table
			if (m_mapDocTypeCount.find(strDocType) == m_mapDocTypeCount.end())
			{
				m_mapDocTypeCount[strDocType] = 1;
			}
			else
			{
				m_mapDocTypeCount[strDocType]++;
			}

			// Declare the output file path to be used.
			string strDocTypeFilePath = "";

			// If there is more than one doc type, the file should be added to the log file
			// ONE time and one time only.  
			if( i == 1 )
			{
				strDocTypeFilePath = m_strOutputFileDirectory + gstrFILES_CLASSIFIED_AS_MORE_THAN_ONE_DOC_TYPE;

				// Append the source doc path to the file's list
				appendToFile( strSourceDoc, strDocTypeFilePath );
			}

			// Open the list file for this document type
			strDocTypeFilePath = m_strOutputFileDirectory + "\\" + gstrDOC_TYPE_PREFIX + 
				strDocType + ".txt";

			// Append the source doc path to the file's list
			appendToFile( strSourceDoc, strDocTypeFilePath );
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CIDShieldTester::countExpectedOverlapsAndPages(IIUnknownVectorPtr ipExpectedAttributes,
	unsigned long& rulOverlaps, unsigned long& rulNumPagesWithRedactions)
{
	ASSERT_ARGUMENT("ELI18361", ipExpectedAttributes != NULL);

	// get the size of the vector
	long nSize = ipExpectedAttributes->Size();
	rulOverlaps = 0;

	set<int> setPagesWithExpectedAttributes;

	// loop over each item in the vector
	for (long i = 0; i < nSize; i++)
	{
		// get the attribute from the vector
		IAttributePtr ipAttrib1 = ipExpectedAttributes->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI18362", ipAttrib1 != NULL);

		// get the spatial string associated with this attribute
		ISpatialStringPtr ipSS1 = ipAttrib1->Value;
		ASSERT_RESOURCE_ALLOCATION("ELI18363", ipSS1 != NULL);

		// now compare all of the other attributes to this attribute
		for (long j = i+1; j < nSize; j++)
		{
			// get an attribute
			IAttributePtr ipAttrib2 = ipExpectedAttributes->At(j);
			ASSERT_RESOURCE_ALLOCATION("ELI18364", ipAttrib2 != NULL);

			// get the spatial string from the attribute
			ISpatialStringPtr ipSS2 = ipAttrib2->Value;
			ASSERT_RESOURCE_ALLOCATION("ELI18416", ipSS2 != NULL);

			// check if the spatial strings overlap
			if (spatialStringsOverlap(ipSS1, ipSS2))
			{
				// strings overlap, increment our overlap counter
				rulOverlaps++;
			}
		}

		long lLastPageNumber = ipSS1->GetLastPageNumber();
		for (long lPage = ipSS1->GetFirstPageNumber(); lPage <= lLastPageNumber; lPage++)
		{
			// Retrieve the specific page we need.
			ISpatialStringPtr ipPage = ipSS1->GetSpecifiedPages(lPage, lPage);
			ASSERT_RESOURCE_ALLOCATION("ELI29438", ipPage != NULL);

			if (!asCppBool(ipPage->HasSpatialInfo()))
			{
				// If this page has no spatial information, do not include it as a page with
				// expected attributes.
				continue;
			}

			setPagesWithExpectedAttributes.insert(lPage);
		}
	}

	rulNumPagesWithRedactions = (unsigned long)setPagesWithExpectedAttributes.size();
}
//-------------------------------------------------------------------------------------------------
bool CIDShieldTester::spatialStringsOverlap(ISpatialStringPtr ipSS1, ISpatialStringPtr ipSS2)
{
	ASSERT_ARGUMENT("ELI18417", ipSS1 != NULL);
	ASSERT_ARGUMENT("ELI18418", ipSS2 != NULL);

	// make sure both strings are spatial, if not just return false since they cannot overlap
	// if they are not spatial
	IIUnknownVectorPtr ipRZones1;
	if (asCppBool(ipSS1->HasSpatialInfo()))
	{
		ipRZones1 = ipSS1->GetOriginalImageRasterZones();
		ASSERT_RESOURCE_ALLOCATION("ELI18419", ipRZones1 != NULL);
	}
	else
	{
		return false;
	}

	IIUnknownVectorPtr ipRZones2;
	if (asCppBool(ipSS2->HasSpatialInfo()))
	{
		ipRZones2 = ipSS2->GetOriginalImageRasterZones();
		ASSERT_RESOURCE_ALLOCATION("ELI18420", ipRZones2 != NULL);
	}
	else
	{
		return false;
	}

	// loop through each of the zones for the first string and compare them to the second string
	long lRZones1Size = ipRZones1->Size();
	long lRZones2Size = ipRZones2->Size();
	for (long i = 0; i < lRZones1Size; i++)
	{
		// get the first zone
		IRasterZonePtr ipZone1 = ipRZones1->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI18421", ipZone1 != NULL);

		for (long j = 0; j < lRZones2Size; j++)
		{
			IRasterZonePtr ipZone2 = ipRZones2->At(j);
			ASSERT_RESOURCE_ALLOCATION("ELI18422", ipZone2 != NULL);

			// check the area of overlap, it should be zero
			// NOTE: GetAreaOverlappingWith checks the page number so no need to do it here
			if (!MathVars::isZero(ipZone1->GetAreaOverlappingWith(ipZone2)))
			{
				// area of overlap is not zero - strings overlap so return true
				return  true;
			}
		}
	}

	// if we got here no zones overlap, return false
	return false;
}
//-------------------------------------------------------------------------------------------------
void CIDShieldTester::getMatchInfo(MatchInfo& rMatchInfo, IAttributePtr ipExpected, 
										IAttributePtr ipFound)
{
	ASSERT_ARGUMENT("ELI18423", ipExpected != NULL);
	ASSERT_ARGUMENT("ELI18424", ipFound != NULL);

	// store the expected and found attribute pointers
	rMatchInfo.m_ipExpectedAttribute = ipExpected;
	rMatchInfo.m_ipFoundAttribute = ipFound;

	// get the total area of each attribute
	rMatchInfo.m_dAreaOfExpectedRedaction = getTotalArea(ipExpected);
	rMatchInfo.m_dAreaOfFoundRedaction = getTotalArea(ipFound);

	// get the area of overlap
	rMatchInfo.m_dAreaOfOverlap = computeTotalAreaOfOverlap(ipExpected, ipFound);
}
//-------------------------------------------------------------------------------------------------
double CIDShieldTester::getTotalArea(IAttributePtr ipAttribute)
{
	ASSERT_ARGUMENT("ELI18425", ipAttribute != NULL);

	// default total area to 0
	double dArea = 0.0;

	// get the spatial string for this attribute
	ISpatialStringPtr ipSS = ipAttribute->Value;
	ASSERT_RESOURCE_ALLOCATION("ELI18426", ipSS != NULL);

	// check for spatial info
	if (asCppBool(ipSS->HasSpatialInfo()))
	{
		// get the vector of raster zones
		IIUnknownVectorPtr ipVecRasterZones = ipSS->GetOriginalImageRasterZones();
		ASSERT_RESOURCE_ALLOCATION("ELI18427", ipVecRasterZones != NULL);

		// for each zone, get its area and add that to the total area
		long lSize = ipVecRasterZones->Size();
		for (long i = 0; i < lSize; i++)
		{
			IRasterZonePtr ipRZ = ipVecRasterZones->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI18428", ipRZ != NULL);

			dArea += (double) ipRZ->Area;
		}
	}

	// return the computed area (0 if no spatial info)
	return dArea;
}
//-------------------------------------------------------------------------------------------------
double CIDShieldTester::computeTotalAreaOfOverlap(IAttributePtr ipExpected, IAttributePtr ipFound)
{
	// check arguments
	ASSERT_ARGUMENT("ELI18429", ipExpected != NULL);
	ASSERT_ARGUMENT("ELI18430", ipFound != NULL);

	// get the spatial strings for each attribute
	ISpatialStringPtr ipExpectedSS = ipExpected->Value;
	ASSERT_RESOURCE_ALLOCATION("ELI18431", ipExpectedSS != NULL);

	ISpatialStringPtr ipFoundSS = ipFound->Value;
	ASSERT_RESOURCE_ALLOCATION("ELI18432", ipFoundSS != NULL);

	// default the overlap area to 0
	double dAreaOfOverlap = 0.0;

	// make sure both strings are spatial
	if (asCppBool(ipExpectedSS->HasSpatialInfo()) && asCppBool(ipFoundSS->HasSpatialInfo()))
	{
		// get the vector of raster zones for both spatial strings
		IIUnknownVectorPtr ipExpectedRZs = ipExpectedSS->GetOriginalImageRasterZones();
		ASSERT_RESOURCE_ALLOCATION("ELI18433", ipExpectedRZs != NULL);

		IIUnknownVectorPtr ipFoundRZs = ipFoundSS->GetOriginalImageRasterZones();
		ASSERT_RESOURCE_ALLOCATION("ELI18434", ipFoundRZs != NULL);

		// loop through each of the raster zones and add the area of overlap
		long lExpectedSize = ipExpectedRZs->Size();
		long lFoundSize = ipFoundRZs->Size();
		for (long i = 0; i < lExpectedSize; i++)
		{
			IRasterZonePtr ipERZ = ipExpectedRZs->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI18435", ipERZ != NULL);

			for (long j = 0; j < lFoundSize; j++)
			{
				IRasterZonePtr ipFRZ = ipFoundRZs->At(j);
				ASSERT_RESOURCE_ALLOCATION("ELI18436", ipFRZ != NULL);

				// add the area of overlap (GetArea checks page number of zone)
				dAreaOfOverlap += ipERZ->GetAreaOverlappingWith(ipFRZ);
			}
		}
	}

	return dAreaOfOverlap;
}
//-------------------------------------------------------------------------------------------------
void CIDShieldTester::addAttributeToTestOutputVOA(IAttributePtr ipAttribute, 
												  const string &strPrefix,
												  const string &strSourceVOA)
{
	ASSERT_ARGUMENT("ELI18437", ipAttribute != NULL);

	// create a new attribute
	IAttributePtr ipNewAttribute(CLSID_Attribute);
	ASSERT_RESOURCE_ALLOCATION("ELI18438", ipNewAttribute != NULL);

	// get the copyable object
	ICopyableObjectPtr ipCopy = ipNewAttribute;
	ASSERT_RESOURCE_ALLOCATION("ELI18439", ipCopy != NULL);

	// copy the expected attribute to the new attribute
	ipCopy->CopyFrom(ipAttribute);

	// prefix the name with the prefix string
	string strName = strPrefix + asString(ipNewAttribute->Name);
	ipNewAttribute->Name = strName.c_str();

	if (!strSourceVOA.empty())
	{
		ipNewAttribute->AddType(get_bstr_t(strSourceVOA));
	}

	// add this attribute to the output VOA vector
	m_ipTestOutputVOAVector->PushBack(ipNewAttribute);
}
//-------------------------------------------------------------------------------------------------
void CIDShieldTester::countAttributeNames(IIUnknownVectorPtr ipFoundAttributes, 
										  unsigned long &rulHCData, unsigned long &rulMCData, 
										  unsigned long &rulLCData, unsigned long &rulClues)
{
	ASSERT_ARGUMENT("ELI18504", ipFoundAttributes != NULL);

	// set all counts to 0
	rulHCData = rulMCData = rulLCData = rulClues = 0;

	long lSize = ipFoundAttributes->Size();

	for (long i=0; i < lSize; i++)
	{
		IAttributePtr ipAttribute = ipFoundAttributes->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI18505", ipAttribute != NULL);

		string strName = asString(ipAttribute->Name);

		if (strName == "HCData")
		{
			rulHCData++;
		}
		else if (strName == "MCData")
		{
			rulMCData++;
		}
		else if (strName == "LCData")
		{
			rulLCData++;
		}
		else if (strName == "Clues")
		{
			rulClues++;
		}
	}
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CIDShieldTester::filterAttributesByType(IIUnknownVectorPtr ipAttributeVector)
{
	ASSERT_ARGUMENT("ELI18513", ipAttributeVector != NULL);

	// create a new vector
	IIUnknownVectorPtr ipNewVector(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI18514", ipNewVector != NULL);

	// get the current vector size
	long lSize = ipAttributeVector->Size();

	// loop through each element of the attribute vector and compare its
	// type with the specified type
	for (long i=0; i < lSize; i++)
	{
		IAttributePtr ipAttribute = ipAttributeVector->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI18515", ipAttribute != NULL);

		// Always filter by document type [FlexIDSCore #3449]
		string strName = asString(ipAttribute->Name);
		makeLowerCase(strName);

		// if the attribute contains the specified type, add it to the vector
		if (strName == "documenttype")
		{
			ipNewVector->PushBack(ipAttribute);
		}
		else
		{
			for (set<string>::iterator iterType = m_setTypesToBeTested.begin();
				 iterType != m_setTypesToBeTested.end();
				 iterType++)
			{
				if (asCppBool(ipAttribute->ContainsType(iterType->c_str())))
				{
					ipNewVector->PushBack(ipAttribute);
					break;
				}
			}
		}
	}

	// return the new vector
	return ipNewVector;
}
//-------------------------------------------------------------------------------------------------
void CIDShieldTester::updateVerificationAttributeTester()
{
	// If an attribute tester was already created, just reset it
	if (m_apVerificationTester.get() != NULL)
	{
		m_apVerificationTester->reset();
		return;
	}

	// Create a new attribute tester
	m_apVerificationTester.reset(new AttributeTester());

	switch(m_eaqVerificationQuantifier)
	{
	case kAny:
		{
			m_apVerificationTester->addTester(
				new AnyDataTypeAttributeTester(m_setVerificationCondition));
		}
		break;
	case kOnlyAny:
		{
			m_apVerificationTester->addTester(
				new OnlyAnyDataTypeAttributeTester(m_setVerificationCondition));
		}
		break;
	case kOneOfEach:
		{
			m_apVerificationTester->addTester(
				new OneOfEachDataTypeAttributeTester(m_setVerificationCondition));
		}
		break;
	case kNone:
		{
			m_apVerificationTester->addTester(
				new NoneDataTypeAttributeTester(m_setVerificationCondition));
		}
		break;
	default:
		THROW_LOGIC_ERROR_EXCEPTION("ELI25163");
	}

	// If there are doc types that should be selected for verification
	// add a tester for the doc types
	if (!m_setDocTypesToBeVerified.empty())
	{
		m_apVerificationTester->addTester(
			new DocTypeAttributeTester(m_setDocTypesToBeVerified));
	}
}
//-------------------------------------------------------------------------------------------------
void CIDShieldTester::updateRedactionAttributeTester()
{
	// If an attribute tester was already created, just reset it
	if (m_apRedactionTester.get() != NULL)
	{
		m_apRedactionTester->reset();
		return;
	}

	// Create a new attribute tester
	m_apRedactionTester.reset(new AttributeTester());

	switch(m_eaqRedactionQuantifier)
	{
	case kAny:
		{
			m_apRedactionTester->addTester(
				new AnyDataTypeAttributeTester(m_setRedactionCondition));
		}
		break;
	case kOnlyAny:
		{
			m_apRedactionTester->addTester(
				new OnlyAnyDataTypeAttributeTester(m_setRedactionCondition));
		}
		break;
	case kOneOfEach:
		{
			m_apRedactionTester->addTester(
				new OneOfEachDataTypeAttributeTester(m_setRedactionCondition));
		}
		break;
	case kNone:
		{
			m_apRedactionTester->addTester(
				new NoneDataTypeAttributeTester(m_setRedactionCondition));
		}
		break;
	default:
		THROW_LOGIC_ERROR_EXCEPTION("ELI25164");
	}

	// If there are doc types that should be selected for automatic redaction add a tester for the
	// doc types
	if (!m_setDocTypesToBeAutomaticallyRedacted.empty())
	{
		m_apRedactionTester->addTester(
			new DocTypeAttributeTester(m_setDocTypesToBeAutomaticallyRedacted));
	}
}
//-------------------------------------------------------------------------------------------------
void CIDShieldTester::getOutputDirectory(string rootDirectory)
{
	// Determine the Analysis folder to create the log files in
	m_strOutputFileDirectory = rootDirectory + "\\Analysis - " + getTimeStamp();
}
//-------------------------------------------------------------------------------------------------
string CIDShieldTester::getSetAsDelimitedList(const set<string>& setValues)
{
	string strList;

	bool bFirst = true;
	for (set<string>::const_iterator iterValue = setValues.begin();
		 iterValue != setValues.end();
		 iterValue++)
	{
		if (!bFirst)
		{
			strList += ", ";
		}

		strList += *iterValue;
		bFirst = false;
	}

	return strList;
}
//-------------------------------------------------------------------------------------------------
CIDShieldTester::TestCaseStatistics::TestCaseStatistics()
: m_bTestCaseResult(true)
, m_ulTotalExpectedRedactions(0)
, m_ulExpectedRedactionsInSelectedFiles(0)
, m_ulFoundRedactions(0)
, m_ulNumCorrectRedactions(0)
, m_ulNumFalsePositives(0)
, m_ulNumOverRedactions(0)
, m_ulNumUnderRedactions(0)
, m_ulNumMisses(0)
{
}
//-------------------------------------------------------------------------------------------------
void CIDShieldTester::TestCaseStatistics::reset()
{
	m_bTestCaseResult = true;
	m_ulTotalExpectedRedactions = 0;
	m_ulExpectedRedactionsInSelectedFiles = 0;
	m_ulFoundRedactions = 0;
	m_ulNumCorrectRedactions = 0;
	m_ulNumFalsePositives = 0;
	m_ulNumOverRedactions = 0;
	m_ulNumUnderRedactions = 0;
	m_ulNumMisses = 0;
}
//-------------------------------------------------------------------------------------------------
CIDShieldTester::TestCaseStatistics& CIDShieldTester::TestCaseStatistics::operator += (
															const TestCaseStatistics& otherTestCase)
{
	m_bTestCaseResult = (m_bTestCaseResult && otherTestCase.m_bTestCaseResult);
	m_ulTotalExpectedRedactions += otherTestCase.m_ulTotalExpectedRedactions;
	m_ulExpectedRedactionsInSelectedFiles += otherTestCase.m_ulExpectedRedactionsInSelectedFiles;
	m_ulFoundRedactions += otherTestCase.m_ulFoundRedactions;
	m_ulNumCorrectRedactions += otherTestCase.m_ulNumCorrectRedactions;
	m_ulNumFalsePositives += otherTestCase.m_ulNumFalsePositives;
	m_ulNumOverRedactions += otherTestCase.m_ulNumOverRedactions;
	m_ulNumUnderRedactions += otherTestCase.m_ulNumUnderRedactions;
	m_ulNumMisses += otherTestCase.m_ulNumMisses;

	return *this;
}
//-------------------------------------------------------------------------------------------------