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
#include <ComponentLicenseIDs.h>
#include <RegistryPersistenceMgr.h>
#include <mathUtil.h>
#include <MiscLeadUtils.h>
#include <StringCSIS.h>

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

// Minimum percentage of overlap consider two zones as overlapping.
// (as a percentage of the area of the smaller zone)
const string gstrOVERLAP_MINUMUM_PERCENT_KEY = "RasterZoneOverlapMinimum";
const string gstrOVERLAP_MINUMUM_DEFAULT_VALUE = "10";

const string gstrDOCTYPEQUERY = "DocumentType";

// Surrounds preliminarily expanded custom report arguments.
const string gstrPRELIM_ARG_MARKER = "$$$";

//-------------------------------------------------------------------------------------------------
// These output files will be written to "$DirOf(tcl file)\\Analysis - (Timestamp)"
//-------------------------------------------------------------------------------------------------
// Files output in handleTestCase
const string gstrFILES_WITH_EXPECTED_REDACTIONS = "\\SensitiveFiles_All.txt";
const string gstrFILES_WITH_NO_EXPECTED_REDACTIONS = "\\InsensitiveFiles_All.txt";
const string gstrFILES_WITH_FOUND_REDACTIONS = "\\Files_AtLeastOneFoundRedaction.txt";
const string gstrFILES_WITH_NO_FOUND_REDACTIONS = "\\Files_NoFoundRedactions.txt";
const string gstrFILES_WITH_OVERLAPPING_EXPECTED_REDACTIONS =
											"\\Files_AtLeastOneOverlappingExpectedRedaction.txt";

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
const string gstrUNKNOWN_DOC_TYPE_FILES = "\\DocType_Unknown.txt";
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
	ASSERT_ARGUMENT("ELI25179", pTester != __nullptr);

	// Loop through each attribute and test it
	long lAttributeCount = ipAttributes->Size();
	for (long l = 0; l < lAttributeCount; l++)
	{
		// Get next attribute
		IAttributePtr ipAttribute = ipAttributes->At(l);
		ASSERT_RESOURCE_ALLOCATION("ELI25165", ipAttribute != __nullptr);

		// Get attribute name
		string strName = asString(ipAttribute->GetName());

		// Get attribute value
		ISpatialStringPtr ipValue = ipAttribute->Value;
		ASSERT_RESOURCE_ALLOCATION("ELI25166", ipValue != __nullptr);
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
m_bOutputDirectoryInitialized(false),
m_ipTestOutputVOAVector(NULL),
m_ipAFUtility(CLSID_AFUtility),
m_ipMiscUtils(CLSID_MiscUtils),
m_ipAttrFinderEngine(CLSID_AttributeFinderEngine)
{
	try
	{
		ASSERT_RESOURCE_ALLOCATION("ELI15178", m_ipAFUtility != __nullptr);
		ASSERT_RESOURCE_ALLOCATION("ELI35184", m_ipMiscUtils != __nullptr);
		ASSERT_RESOURCE_ALLOCATION("ELI15206", m_ipAttrFinderEngine != __nullptr);

		m_ipTagUtility = m_ipMiscUtils;
		ASSERT_RESOURCE_ALLOCATION("ELI36104", m_ipTagUtility != __nullptr);

		// Map the result fields to names for use by custom reports.
		m_mapStatisticFields["TotalExpectedRedactions"] = &m_iccCounters.m_ulTotalExpectedRedactions;
		m_mapStatisticFields["NumCorrectRedactionsByDocument"] = &m_iccCounters.m_ulNumCorrectRedactionsByDocument;
		m_mapStatisticFields["NumCorrectRedactionsByPage"] = &m_iccCounters.m_ulNumCorrectRedactionsByPage;
		m_mapStatisticFields["NumOverRedactions"] = &m_iccCounters.m_ulNumOverRedactions;
		m_mapStatisticFields["NumUnderRedactions"] = &m_iccCounters.m_ulNumUnderRedactions;
		m_mapStatisticFields["NumMisses"] = &m_iccCounters.m_ulNumMisses;
		m_mapStatisticFields["TotalFilesProcessed"] = &m_iccCounters.m_ulTotalFilesProcessed;
		m_mapStatisticFields["NumFilesWithExpectedRedactions"] = &m_iccCounters.m_ulNumFilesWithExpectedRedactions;
		m_mapStatisticFields["NumFilesSelectedForReview"] = &m_iccCounters.m_ulNumFilesSelectedForReview;
		m_mapStatisticFields["NumPagesInFilesSelectedForReview"] = &m_iccCounters.m_ulNumPagesInFilesSelectedForReview;
		m_mapStatisticFields["NumPagesSelectedForReview"] = &m_iccCounters.m_ulNumPagesSelectedForReview;
		m_mapStatisticFields["NumFilesAutomaticallyRedacted"] = &m_iccCounters.m_ulNumFilesAutomaticallyRedacted;
		m_mapStatisticFields["NumExpectedRedactionsInReviewedFiles"] = &m_iccCounters.m_ulNumExpectedRedactionsInReviewedFiles;
		m_mapStatisticFields["NumExpectedRedactionsInRedactedFiles"] = &m_iccCounters.m_ulNumExpectedRedactionsInRedactedFiles;
		m_mapStatisticFields["NumFilesWithExistingVOA"] = &m_ulNumFilesWithExistingVOA;
		m_mapStatisticFields["NumFilesWithOverlappingExpectedRedactions"] = &m_iccCounters.m_ulNumFilesWithOverlappingExpectedRedactions;
		m_mapStatisticFields["TotalPages"] = &m_iccCounters.m_ulTotalPages;
		m_mapStatisticFields["NumPagesWithExpectedRedactions"] = &m_iccCounters.m_ulNumPagesWithExpectedRedactions;
		m_mapStatisticFields["DocsClassified"] = &m_iccCounters.m_ulDocsClassified;

		m_mapStatisticFields["AutomatedTotalExpectedRedactions"] = &m_iccCounters.m_automatedStatistics.m_ulTotalExpectedRedactions;
		m_mapStatisticFields["AutomatedExpectedRedactionsInSelectedFiles"] = &m_iccCounters.m_automatedStatistics.m_ulExpectedRedactionsInSelectedFiles;
		m_mapStatisticFields["AutomatedFoundRedactions"] = &m_iccCounters.m_automatedStatistics.m_ulFoundRedactions;
		m_mapStatisticFields["AutomatedNumCorrectRedactions"] = &m_iccCounters.m_automatedStatistics.m_ulNumCorrectRedactions;
		m_mapStatisticFields["AutomatedNumFalsePositives"] = &m_iccCounters.m_automatedStatistics.m_ulNumFalsePositives;
		m_mapStatisticFields["AutomatedNumOverRedactions"] = &m_iccCounters.m_automatedStatistics.m_ulNumOverRedactions;
		m_mapStatisticFields["AutomatedNumUnderRedactions"] = &m_iccCounters.m_automatedStatistics.m_ulNumUnderRedactions;
		m_mapStatisticFields["AutomatedNumMisses"] = &m_iccCounters.m_automatedStatistics.m_ulNumMisses;

		m_mapStatisticFields["VerificationTotalExpectedRedactions"] = &m_iccCounters.m_verificationStatistics.m_ulTotalExpectedRedactions;
		m_mapStatisticFields["VerificationExpectedRedactionsInSelectedFiles"] = &m_iccCounters.m_verificationStatistics.m_ulExpectedRedactionsInSelectedFiles;
		m_mapStatisticFields["VerificationExpectedRedactionsInSelectedPages"] = &m_iccCounters.m_verificationStatistics.m_ulExpectedRedactionsInSelectedPages;
		m_mapStatisticFields["VerificationFoundRedactions"] = &m_iccCounters.m_verificationStatistics.m_ulFoundRedactions;
		m_mapStatisticFields["VerificationNumCorrectRedactions"] = &m_iccCounters.m_verificationStatistics.m_ulNumCorrectRedactions;
		m_mapStatisticFields["VerificationNumFalsePositives"] = &m_iccCounters.m_verificationStatistics.m_ulNumFalsePositives;
		m_mapStatisticFields["VerificationNumOverRedactions"] = &m_iccCounters.m_verificationStatistics.m_ulNumOverRedactions;
		m_mapStatisticFields["VerificationNumUnderRedactions"] = &m_iccCounters.m_verificationStatistics.m_ulNumUnderRedactions;
		m_mapStatisticFields["VerificationNumMisses"] = &m_iccCounters.m_verificationStatistics.m_ulNumMisses;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI15179");
}
//-------------------------------------------------------------------------------------------------
CIDShieldTester::~CIDShieldTester()
{
	try
	{
		m_ipAFUtility = __nullptr;
		m_ipMiscUtils = __nullptr;
		m_ipTagUtility = __nullptr;
		m_ipAttrFinderEngine = __nullptr;
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

		*pVal = _bstr_t(m_strOutputFileDirectory.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28679")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldTester::GenerateCustomReport(BSTR bstrReportTemplate)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		generateCustomReport(asString(bstrReportTemplate));
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI34968");

	return S_OK;
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
			ASSERT_ARGUMENT("ELI14547", pParams != __nullptr);

			// Validate the license
			validateLicense();

			if (m_ipResultLogger == __nullptr)
			{
				throw UCLIDException("ELI15166", "Please set ResultLogger before proceeding.");
			}

			// Default to adding entries to the results logger
			m_ipResultLogger->AddEntriesToTestLogger = VARIANT_TRUE;

			// Create a persistence manager to get the overlap value from the registry
			RegistryPersistenceMgr rpm( HKEY_CURRENT_USER, gstrMIN_OVERLAP_REGISTRY_KEY_PATH );

			// Leniency percentage allowed before a redaction is considered an under-redaction.
			// If a key exists, use the one that is currently in the registry
			string strOverlapLeniency = "";
			if( rpm.keyExists(gstrOVERLAP_PERCENT_FOLDER, gstrOVERLAP_PERCENT_KEY) )
			{
				strOverlapLeniency = rpm.getKeyValue( gstrOVERLAP_PERCENT_FOLDER,
					gstrOVERLAP_PERCENT_KEY, gstrOVERLAP_DEFAULT_VALUE );
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

			// Excess redaction area percentage
			// if a key exists, use the one that is currently in the registry
			string strOverRedactionERAP = "";
			if (rpm.keyExists(gstrOVERLAP_PERCENT_FOLDER, gstrOVER_REDACTION_PERCENT_KEY))
			{
				strOverRedactionERAP = rpm.getKeyValue(gstrOVERLAP_PERCENT_FOLDER,
					gstrOVER_REDACTION_PERCENT_KEY, gstrOVER_REDACTION_ERAP_DEFAULT_VALUE);
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

			// Overlap minimum
			// If a key exists, use the one that is currently in the registry
			string strOverlapMinimumPercent = "";
			if (rpm.keyExists(gstrOVERLAP_PERCENT_FOLDER, gstrOVERLAP_MINUMUM_PERCENT_KEY))
			{
				strOverlapMinimumPercent = rpm.getKeyValue(gstrOVERLAP_PERCENT_FOLDER,
					gstrOVERLAP_MINUMUM_PERCENT_KEY, gstrOVERLAP_MINUMUM_DEFAULT_VALUE);
			}
			else
			{
				// key does not exist, create a key with the default value
				rpm.createKey(gstrOVERLAP_PERCENT_FOLDER, gstrOVERLAP_MINUMUM_PERCENT_KEY,
					gstrOVERLAP_MINUMUM_DEFAULT_VALUE, true);
				strOverlapMinimumPercent = gstrOVERLAP_MINUMUM_DEFAULT_VALUE;
			}

			// use the key from the registry or the default as set above
			m_dOverlapMinimumPercent = asDouble(strOverlapMinimumPercent);

			// reset all variables associated with the test
			m_mapDocTypeCounts.clear();
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
			m_iccCounters.clear();
			m_ulNumFilesWithExistingVOA = 0;
			m_bOutputHybridStats = false;
			m_bOutputAutomatedStatsOnly = false;
			m_bOutputVerificationByDocumentStats = true;
			m_bOutputVerificationByPageStats = false;
			m_apRedactionTester.reset();
			m_apVerificationTester.reset();
			m_bOutputDirectoryInitialized = false;
			m_strTestOutputPath = "$InsertBeforeExt(<FoundVoaFile>,.testoutput)";

			IVariantVectorPtr ipParams(pParams);
			ASSERT_RESOURCE_ALLOCATION("ELI15258", ipParams != __nullptr);

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

	// Ensure all accumulated USB clicks are decremented.
	try
	{
		IRuleSetPtr ipRuleSet(CLSID_RuleSet);
		ASSERT_RESOURCE_ALLOCATION("ELI33406", ipRuleSet != __nullptr);

		ipRuleSet->Cleanup();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI33407");

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
		ASSERT_RESOURCE_ALLOCATION("ELI15736", m_ipResultLogger != __nullptr);
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
		// [FlexIDSCore:5612]
		// Ensure the output file directory isn't cleared after we've already output at least one
		// folder's worth of data.
		if (!m_bOutputDirectoryInitialized)
		{
			if (isValidFolder(m_strOutputFileDirectory))
			{
				// Remove all files from the output directory if it already exists, to ensure old
				// results don't end up alongside new results.
				vector<string> vecSubDirs;
				getAllSubDirsAndDeleteAllFiles(m_strOutputFileDirectory, vecSubDirs);
			}
			else
			{
				// Create the directory if it does not yet exist.
				createDirectory(m_strOutputFileDirectory);
			}

			m_bOutputDirectoryInitialized = true;
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
	bool bOutputFilesFolderIsSet = false;

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
				ASSERT_RESOURCE_ALLOCATION("ELI18360", m_ipTestOutputVOAVector != __nullptr);
			}
		}
		// [FlexIDSCore:3039]
		else if (vecTokens[0] == "TestOutputVoaPath")
		{
			m_strTestOutputPath = vecTokens[1];
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

				// [FlexIDSCore:4005]
				// Disregard any spaces included in the list
				for (vector<string>::iterator iter = vecTemp.begin(); iter != vecTemp.end(); iter++)
				{
					*iter = trim(*iter, " ", " ");
				}

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
			if (bOutputFilesFolderIsSet)
			{
				throw UCLIDException("ELI34969", "Output directory specified multiple times.");
			}
			if (!vecTokens[1].empty())
			{
				// Update m_strOutputFileDirectory based on the specified folder.
				getOutputDirectory(vecTokens[1]);
				bOutputFilesFolderIsSet = true;
			}
		}
		else if (vecTokens[0] == "ExplicitOutputFilesFolder")
		{
			if (bOutputFilesFolderIsSet)
			{
				throw UCLIDException("ELI34970", "Output directory specified multiple times.");
			}
			if (!vecTokens[1].empty())
			{
				// Set m_strOutputFileDirectory without using a timestamp-based analysis sub-folder.
				m_strOutputFileDirectory = vecTokens[1];
				bOutputFilesFolderIsSet = true;
			}
		}
		else if (vecTokens[0] == "OutputAutomatedStatsOnly")
		{
			m_bOutputAutomatedStatsOnly = (vecTokens[1] == "1");
		}
		// [FlexIDSCore:5171]
		// Specifies if verifiers are presented every page from sensitive documents or only pages
		// that contain sensitive data (or to show stats using both methods).
		else if (vecTokens[0] == "VerificationSelection")
		{
			string strSetting = vecTokens[1];

			if (strSetting == "ByDocument")
			{
				m_bOutputVerificationByDocumentStats = true;
				m_bOutputVerificationByPageStats = false;
			}
			else if (strSetting == "ByPage")
			{
				m_bOutputVerificationByDocumentStats = false;
				m_bOutputVerificationByPageStats = true;
			}
			else if (strSetting == "ByDocumentAndPage")
			{
				m_bOutputVerificationByDocumentStats = true;
				m_bOutputVerificationByPageStats = true;
			}
			else
			{
				UCLIDException ue("ELI35297", "Invalid VerificationSelection setting. Valid options "
					"are \"ByDocument\", \"ByPage\" and \"ByDocumentAndPage\"");
				ue.addDebugInfo("Setting", strSetting);
				throw ue;
			}
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

	// Get a list of all files in the directory
	vector<string> vecDirListing;
	getFilesInDir(vecDirListing, strAbsoluteImageDir);

	// Iterate the list of files and add just the image files to the final set
	set<string> setImageFilesToTest;
	for (vector<string>::iterator iterFileName = vecDirListing.begin();
		 iterFileName != vecDirListing.end();
		 iterFileName++)
	{
		if (getFileType(*iterFileName) == kImageFile)
		{
			setImageFilesToTest.insert(*iterFileName);
		}
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

		string strFoundVOAFileWithExpandedTags =
			expandTagsAndFunctions(strFoundVOAFileWithTags, strImageFileName);
		string strExpectedVOAFileWithExpandedTags =
			expandTagsAndFunctions(strExpectedVOAFileWithTags, strImageFileName);

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
			// Create custom path tags to be used to evaluate m_strTestOutputPath
			m_ipMiscUtils->AddTag("OutputFilesFolder", m_strOutputFileDirectory.c_str());
			m_ipMiscUtils->AddTag("FoundVoaFile", strFoundVOAFile.c_str());
			m_ipMiscUtils->AddTag("ExpectedVOAFile", strExpectedVOAFile.c_str());

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
			ASSERT_RESOURCE_ALLOCATION("ELI15195", ipExpectedAttributes != __nullptr);
			if( ::isFileOrFolderValid( strExpectedVOAFile ) )
			{
				ipExpectedAttributes->LoadFrom( strExpectedVOAFile.c_str(), VARIANT_FALSE );

				// Don't include metadata, clues, DocumentType attributes or non-spatial attributes from the expected
				// voa file as expected redactions.
				// [DataEntry:3610]
				// Removing filtering of clues that was added 11/9/2009. At that time, the new
				// verification task was leaving clues in the output so in order to have the
				// RedactionTester return reasonable results, clues had to be ignored in the
				// expected VOA file. Clues are now being moved to metadata by the verification
				// task unless they are turned on during redaction. Though it should be rare that
				// clues are actually desired in the expected results, it should be allowed.
				// Currently, the SpatialProximityAS automated test is configured in a way that
				// expects clues to be included in the test.
				// https://extract.atlassian.net/browse/ISSUE-14966
				// Add removal of all non-spatial attributes
				// Change to single loop over top-level attributes because subattributes don't matter
				// for IDShield testing.
				long lSize = ipExpectedAttributes->Size();
				for (long i = 0; i < lSize; i++)
				{
					IAttributePtr ipAttribute = ipExpectedAttributes->At(i);
					ASSERT_ARGUMENT("ELI44910", ipAttribute != __nullptr);

					string strAttributeName = asString(ipAttribute->Name);
					makeLowerCase(strAttributeName);
					if (!strAttributeName.empty() && strAttributeName[0] == '_'
						|| strAttributeName == "documenttype"
						|| ipAttribute->Value->HasSpatialInfo() == VARIANT_FALSE)
					{
						ipExpectedAttributes->Remove(i);
						lSize--;
						i--;
					}
				}
			}
			// If the file does not exist, there are no expected values, throw an exception
			else
			{
				UCLIDException ue("ELI15221", "Invalid file specified for expected values.");
				ue.addDebugInfo("Filename:", strExpectedVOAFile );
				throw ue;
			}

			// [FlexIDSCore:4948]
			// If there is a type string to filter by, filter the attributes before compiling any
			// file lists.
			if (!m_setTypesToBeTested.empty())
			{
				// filter the expected and found attributes by the specified type
				ipExpectedAttributes = filterAttributesByType(ipExpectedAttributes);
			}

			// Attribute counts reported should not include the DocumentType attribute
			IIUnknownVectorPtr ipExpectedAttributesWithoutType =
				filterDocumentTypeAttributes(ipExpectedAttributes);
			ASSERT_RESOURCE_ALLOCATION("ELI36107", ipExpectedAttributesWithoutType != __nullptr);

			// Add the file to the list in the appropriate log file
			if( ipExpectedAttributesWithoutType->Size() > 0 )
			{
				appendToFile(strImageFile,
					m_strOutputFileDirectory + gstrFILES_WITH_EXPECTED_REDACTIONS);
			}
			else
			{
				appendToFile(strImageFile,
					m_strOutputFileDirectory + gstrFILES_WITH_NO_EXPECTED_REDACTIONS);
			}

			// Load the found attributes from the VOA file if it exists, otherwise run the rules to compute them
			IIUnknownVectorPtr ipFoundAttributes(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI15196", ipFoundAttributes != __nullptr);

			// This will be populated by the OCR engine during the call to FindAttributes
			IAFDocumentPtr ipAFDoc(CLSID_AFDocument);
			ASSERT_RESOURCE_ALLOCATION("ELI15205", ipAFDoc != __nullptr);
			bool bCalculatedFoundValues = false;

			// Used for keeping track of OCR confidence
			ISpatialStringPtr ipInputText = __nullptr;

			if ( ::isFileOrFolderValid( strFoundVOAFile ) )
			{
				// VOA file exists - so read found attributes from VOA file
				ipFoundAttributes->LoadFrom( strFoundVOAFile.c_str(), VARIANT_FALSE);

				// Do not include metadata attributes in any test.
				m_ipAFUtility->RemoveMetadataAttributes(ipFoundAttributes);

				// Increment the number of files that read from an existing VOA.
				m_ulNumFilesWithExistingVOA++;

				// If it exists, use the uss file to get the document text to check OCR confidence.
				string strUSSFile = strImageFile + ".uss";
				if (isValidFile(strUSSFile))
				{
					ipInputText.CreateInstance(CLSID_SpatialString);
					ASSERT_RESOURCE_ALLOCATION("ELI35112", ipInputText != __nullptr);

					ipInputText->LoadFrom(strUSSFile.c_str(), VARIANT_FALSE);
				}
			}
			// VOA file does not exist - so compute VOA file by running rules
			else
			{
				// Get the attributes by running the rules on the OCR'd document.
				ipFoundAttributes = m_ipAttrFinderEngine->FindAttributes(ipAFDoc, strSourceDoc.c_str(),
					-1, strRulesFile.c_str(), NULL, VARIANT_FALSE, "", NULL);
				ASSERT_RESOURCE_ALLOCATION("ELI15204", ipFoundAttributes != __nullptr);

				ipInputText = ipAFDoc->Text;

				// This flag means that the AFDoc contains the document types for this file.
				bCalculatedFoundValues = true;
			}

			// If there is a type string to filter by, filter the attributes
			if (!m_setTypesToBeTested.empty())
			{
				// filter the expected and found attributes by the specified type
				ipFoundAttributes = filterAttributesByType(ipFoundAttributes);
			}

			// Attribute counts reported should not include the DocumentType attribute
			IIUnknownVectorPtr ipFoundAttributesWithoutType =
				filterDocumentTypeAttributes(ipFoundAttributes);
			ASSERT_RESOURCE_ALLOCATION("ELI36108", ipFoundAttributesWithoutType != __nullptr);

			// Add this file to the list in the appropriate log file
			if(ipFoundAttributesWithoutType->Size() > 0)
			{
				appendToFile(strImageFile,
					m_strOutputFileDirectory + gstrFILES_WITH_FOUND_REDACTIONS);
			}
			else
			{
				appendToFile(strImageFile,
					m_strOutputFileDirectory + gstrFILES_WITH_NO_FOUND_REDACTIONS);
			}

			// display found and expected attributes side by side
			string strFoundAttr = m_ipAFUtility->GetAttributesAsString(ipFoundAttributes);
			string strExpectedAttr = m_ipAFUtility->GetAttributesAsString(ipExpectedAttributes);
			m_ipResultLogger->AddTestCaseCompareData("Text of Compared Spatial Strings",
				"Expected Attributes", strExpectedAttr.c_str(),
				"Found Attributes", strFoundAttr.c_str());

			// [FlexIDSCore:3039]
			// If generating a testoutput VOA file, the path should be based upon strFoundVOAFile,
			// or m_strTestOutputPath not strSourceDoc.
			string strTestOutputVOAFile = (m_ipTestOutputVOAVector == __nullptr)
				? ""
				: expandTagsAndFunctions(m_strTestOutputPath, strImageFile);

			// This is used to get the counts for the current document and will be added to the
			// total counts for the over all counts and for the document type count
			IDShieldCounterClass iccCounts;
			iccCounts.clear();

			// update internal stats and determine whether this test case passed
			bool bResult = updateStatisticsAndDetermineTestCaseResult(ipExpectedAttributes,
				ipFoundAttributes, strImageFile, strTestOutputVOAFile, iccCounts);

			// Add the counts for this file to the totals
			m_iccCounters += iccCounts;

			countDocTypes(ipFoundAttributes, strImageFile, ipAFDoc, bCalculatedFoundValues, iccCounts);

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

			m_ipResultLogger->AddTestCaseOCRConfidence(ipInputText);

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
																 const string& strSourceDoc,
																 const string& strTestOutputVOAFile,
																 IDShieldCounterClass &iccCounts)
{
	ASSERT_ARGUMENT("ELI19879", ipExpectedAttributes != __nullptr);
	ASSERT_ARGUMENT("ELI19880", ipFoundAttributes != __nullptr);

	// increment the total number of files processed
	iccCounts.m_ulTotalFilesProcessed++;

	// get size of expected attributes vector
	long lExpectedSize = ipExpectedAttributes->Size();

	// increment total number of expected redactions
	iccCounts.m_ulTotalExpectedRedactions += lExpectedSize;

	// if there is at least one expected redaction, increment the count of files
	// containing expected redactions
	if (lExpectedSize > 0)
	{
		iccCounts.m_ulNumFilesWithExpectedRedactions++;
	}

	unsigned long ulNumPages = getNumberOfPagesInImage(strSourceDoc);
	iccCounts.m_ulTotalPages += ulNumPages;

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

	iccCounts.m_ulNumPagesWithExpectedRedactions += ulNumPagesWithRedactions;

	// if there is at least 1 overlapping expected redaction increment the number of
	// files with overlapping expected redactions and add a text node indicating
	// the number of overlapping expected redactions for this test case
	if (ulNumOverlappingExpected > 0)
	{
		iccCounts.m_ulNumFilesWithOverlappingExpectedRedactions++;
		appendToFile(strSourceDoc,
			m_strOutputFileDirectory + gstrFILES_WITH_OVERLAPPING_EXPECTED_REDACTIONS);

		string strNumOverLappingExpected = "\tOverlapping sensitive data items: " +
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
			bSelectedForAutomatedProcess, strSourceDoc, strTestOutputVOAFile, iccCounts);
	}
	else
	{
		// Analyze the attributes for verification based redaction. Include documents that were
		// not otherwise selected for either process in hybrid mode.
		return analyzeDataForVerificationBasedRedaction(ipExpectedAttributes, ipFoundAttributes,
				bSelectedForVerification, strSourceDoc, strTestOutputVOAFile, ulNumPages, iccCounts);
	}
}
//-------------------------------------------------------------------------------------------------
unsigned long CIDShieldTester::countExpectedAttributesOnSelectedPages(
															IIUnknownVectorPtr ipExpectedAttributes,
															IIUnknownVectorPtr ipFoundAttributes,
															unsigned long &rulSelectedPages)
{
	// Loop to find the set of pages selected for review because sensitive data or clues have been
	// found on them.
	set<long> setSelectedPages;
	long nCount = ipFoundAttributes->Size();
	for (long i = 0; i < nCount; i++)
	{
		IAttributePtr ipAttribute = ipFoundAttributes->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI35298", ipAttribute != __nullptr);

		ISpatialStringPtr ipValue = ipAttribute->Value;
		ASSERT_RESOURCE_ALLOCATION("ELI35299", ipValue != __nullptr);

		if (asCppBool(ipValue->HasSpatialInfo()))
		{
			setSelectedPages.insert(ipValue->GetFirstPageNumber());
		}
	}
	rulSelectedPages = setSelectedPages.size();

	unsigned long ulExpectedAttributesOnSelectedPages = 0;

	// Loop through all expected attributes to find the ones that exist on any of the pages
	// selected for review.
	nCount = ipExpectedAttributes->Size();
	for (long i = 0; i < nCount; i++)
	{
		IAttributePtr ipAttribute = ipExpectedAttributes->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI35301", ipAttribute != __nullptr);

		ISpatialStringPtr ipValue = ipAttribute->Value;
		ASSERT_RESOURCE_ALLOCATION("ELI35302", ipValue != __nullptr);

		if (asCppBool(ipValue->HasSpatialInfo()))
		{
			IIUnknownVectorPtr ipPages = ipValue->GetPages(VARIANT_FALSE, "");
			ASSERT_RESOURCE_ALLOCATION("ELI35303", ipPages != __nullptr);

			// If the expected attribute spans multiple pages, check if any of the selected pages is
			// a selected page.
			long nPageCount = ipPages->Size();
			for (long j = 0; j < nPageCount; j++)
			{
				ISpatialStringPtr ipPageValue = ipPages->At(j);
				ASSERT_RESOURCE_ALLOCATION("ELI35304", ipPageValue != __nullptr);

				long nPageNum = ipPageValue->GetFirstPageNumber();
				if (setSelectedPages.find(nPageNum) != setSelectedPages.end())
				{
					ulExpectedAttributesOnSelectedPages++;
					break;
				}
			}
		}
	}

	return ulExpectedAttributesOnSelectedPages;
}
//-------------------------------------------------------------------------------------------------
bool CIDShieldTester::analyzeDataForVerificationBasedRedaction(
														IIUnknownVectorPtr ipExpectedAttributes,
														IIUnknownVectorPtr ipFoundAttributes,
														bool bSelectedForVerification,
														const string& strSourceDoc,
														const string& strTestOutputVOAFile,
														unsigned long ulNumPagesInDoc,
														IDShieldCounterClass &iccCounts)
{
	ASSERT_ARGUMENT("ELI18507", ipExpectedAttributes != __nullptr);
	ASSERT_ARGUMENT("ELI18508", ipFoundAttributes != __nullptr);

	// If m_bOutputVerificationByPageStats, find all the expected attributes that exist on selected
	// pages.
	unsigned long ulSelectedPages = ulNumPagesInDoc;
	unsigned long ulExpectedAttributesOnSelectedPages = m_bOutputVerificationByPageStats
		? countExpectedAttributesOnSelectedPages(ipExpectedAttributes, ipFoundAttributes, ulSelectedPages)
		: 0;

	// analyze the expected and found attributes
	CIDShieldTester::TestCaseStatistics testCaseStatistics = analyzeExpectedAndFoundAttributes(
		ipExpectedAttributes, ipFoundAttributes, ulExpectedAttributesOnSelectedPages,
		bSelectedForVerification, strSourceDoc, strTestOutputVOAFile);

	iccCounts.m_verificationStatistics += testCaseStatistics;

	iccCounts.m_ulNumOverRedactions += testCaseStatistics.m_ulNumOverRedactions;
	iccCounts.m_ulNumUnderRedactions += testCaseStatistics.m_ulNumUnderRedactions;
	iccCounts.m_ulNumMisses += testCaseStatistics.m_ulNumMisses;

	// Attribute counts reported should not include the DocumentType attribute
	IIUnknownVectorPtr ipExpectedAttributesWithoutType =
		filterDocumentTypeAttributes(ipExpectedAttributes);
	ASSERT_RESOURCE_ALLOCATION("ELI36109", ipExpectedAttributesWithoutType != __nullptr);

	// Get the size of the expected attribute collection
	long lExpectedSize = ipExpectedAttributesWithoutType->Size();

	if (bSelectedForVerification)
	{
		// increment the number of files and pages that would be selected for review
		iccCounts.m_ulNumFilesSelectedForReview++;
		iccCounts.m_ulNumPagesInFilesSelectedForReview += ulNumPagesInDoc;
		iccCounts.m_ulNumPagesSelectedForReview += ulSelectedPages;

		// increment the number of expected redactions in files selected for review
		iccCounts.m_ulNumExpectedRedactionsInReviewedFiles += lExpectedSize;

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
		if (lExpectedSize > 0)
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
													   const string& strSourceDoc,
													   const string& strTestOutputVOAFile,
													   IDShieldCounterClass &iccCounts)
{
	ASSERT_ARGUMENT("ELI18509", ipExpectedAttributes != __nullptr);
	ASSERT_ARGUMENT("ELI18510", ipFoundAttributes != __nullptr);

	// analyze the expected and found attributes
	CIDShieldTester::TestCaseStatistics testCaseStatistics = analyzeExpectedAndFoundAttributes(
		ipExpectedAttributes, ipFoundAttributes, 0, bSelectedForAutomatedProcess,
		strSourceDoc, strTestOutputVOAFile);

	iccCounts.m_automatedStatistics += testCaseStatistics;

	iccCounts.m_ulNumOverRedactions += testCaseStatistics.m_ulNumOverRedactions;
	iccCounts.m_ulNumUnderRedactions += testCaseStatistics.m_ulNumUnderRedactions;
	iccCounts.m_ulNumMisses += testCaseStatistics.m_ulNumMisses;

	// Attribute counts reported should not include the DocumentType attribute
	IIUnknownVectorPtr ipExpectedAttributesWithoutType =
		filterDocumentTypeAttributes(ipExpectedAttributes);
	ASSERT_RESOURCE_ALLOCATION("ELI36110", ipExpectedAttributesWithoutType != __nullptr);

	// Get the size of the expected attribute collection
	long lExpectedSize = ipExpectedAttributesWithoutType->Size();

	if (bSelectedForAutomatedProcess)
	{
		iccCounts.m_ulNumFilesAutomaticallyRedacted++;

		// increment the number of expected redactions in files selected for redaction
		iccCounts.m_ulNumExpectedRedactionsInRedactedFiles += lExpectedSize;

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
		if(lExpectedSize > 0)
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
		ASSERT_RESOURCE_ALLOCATION("ELI15186", ipExpectedRasterZones != __nullptr);
	}

	IIUnknownVectorPtr ipFoundRasterZones;
	if( ipFoundSS->HasSpatialInfo() == VARIANT_FALSE )
	{
		return false;
	}
	else
	{
		ipFoundRasterZones = ipFoundSS->GetOriginalImageRasterZones();
		ASSERT_RESOURCE_ALLOCATION("ELI15187", ipFoundRasterZones != __nullptr);
	}

	// For each expected raster zone in the vector, verify that there is some found
	// raster zone that matches it
	long lExpectedSize = ipExpectedRasterZones->Size();
	for ( int e = 0; e < lExpectedSize; e++)
	{
		// Get each individual raster zone
		IRasterZonePtr ipExpectedRZ = ipExpectedRasterZones->At( e );
		ASSERT_RESOURCE_ALLOCATION("ELI15188", ipExpectedRZ != __nullptr);

		bool bFoundMatch = false;

		// Check it against each found raster zone for a match within the limits of the overlap
		long lFoundSize = ipFoundRasterZones->Size();
		for( int f = 0; f < lFoundSize; f++)
		{
			IRasterZonePtr ipFoundRZ = ipFoundRasterZones->At( f );
			ASSERT_RESOURCE_ALLOCATION("ELI15189", ipFoundRZ != __nullptr);

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
	}// end for expected

	// If all the expected attributes had a match, return true.
	return true;
}
//-------------------------------------------------------------------------------------------------
CIDShieldTester::TestCaseStatistics CIDShieldTester::analyzeExpectedAndFoundAttributes(
	IIUnknownVectorPtr ipExpectedAttributes, IIUnknownVectorPtr ipFoundAttributes,
	unsigned long ulExpectedAttributesOnSelectedPages,
	bool bDocumentSelected, const string& strSourceDoc, const string& strTestOutputVOAFile)
{
	ASSERT_ARGUMENT("ELI18511", ipExpectedAttributes != __nullptr);
	ASSERT_ARGUMENT("ELI18512", ipFoundAttributes != __nullptr);

	CIDShieldTester::TestCaseStatistics testCaseStatistics;
	testCaseStatistics.m_ulTotalExpectedRedactions = ipExpectedAttributes->Size();

	// If the document was selected, find the attributes that would be redacted by default and use
	// those attributes as the basis for the statistics.
	IIUnknownVectorPtr ipAutoRedactedAttributes = __nullptr;
	if (bDocumentSelected)
	{
		// restrict the found attributes by the query that will be used for automated redaction
		ipAutoRedactedAttributes = m_ipAFUtility->QueryAttributes(
			ipFoundAttributes, m_strRedactionQuery.c_str(), VARIANT_FALSE);

		// If verifying all pages of sensitive docs, all expected attributes will be reviewed,
		// otherwise only those that are on selected pages will be reviewed.
		testCaseStatistics.m_ulExpectedRedactionsInSelectedFiles = ipExpectedAttributes->Size();
		testCaseStatistics.m_ulExpectedRedactionsInSelectedPages = ulExpectedAttributesOnSelectedPages;
		testCaseStatistics.m_ulFoundRedactions = ipAutoRedactedAttributes->Size();
	}
	// If the document was not selected for verification the total number of auto-redacted
	// attributes is zero.
	else
	{
		// Create an empty attribute vector, all expected attributes are a miss
		ipAutoRedactedAttributes.CreateInstance(CLSID_IUnknownVector);
	}
	ASSERT_RESOURCE_ALLOCATION("ELI29334", ipAutoRedactedAttributes != __nullptr);

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

		// Keep track of all redactions which have been tested to be an overredaction
		set<unsigned long> setCheckedForOverRedaction;

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
						// over-redaction or not.
						bool bCheckedForOverRedaction =
							setCheckedForOverRedaction.find(ulFoundIndex) != setCheckedForOverRedaction.end();

						// If not, calculate whether it is an over-redaction.
						if (!bCheckedForOverRedaction)
						{
							bool bOverRedaction = getIsOverredaction(s2dMatchInfos, ulFoundIndex,
								testCaseStatistics.m_ulTotalExpectedRedactions);

							if (bOverRedaction)
							{
								RecordStatistic(gstrTEST_OVER_REDACTED,
									s2dMatchInfos(0, ulFoundIndex).m_ipFoundAttribute, "Found",
									testCaseStatistics.m_ulNumOverRedactions);
							}

							// Update set of checked attributes
							setCheckedForOverRedaction.insert(ulFoundIndex);
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
		if (m_ipTestOutputVOAVector != __nullptr)
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
	if (!strTestOutputVOAFile.empty())
	{
		// write the testoutput.voa file
		string strStorageManagerIID = asString(CLSID_AttributeStorageManager);
		m_ipTestOutputVOAVector->SaveTo(
			strTestOutputVOAFile.c_str(), VARIANT_TRUE, strStorageManagerIID.c_str());

		// clear the vector
		m_ipTestOutputVOAVector->Clear();

		// add a link to the output data
		m_ipResultLogger->AddTestCaseFile(strTestOutputVOAFile.c_str());
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

		// Don't get the absolute value
		// https://extract.atlassian.net/browse/ISSUE-12617
		//    // get absolute value
		//    dERAP = fabs(dERAP);

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
	if (m_ipTestOutputVOAVector != __nullptr && strLabel != "")
	{
		addAttributeToTestOutputVOA(ipRelatedAttribute, strLabel, strSourceVOA);
	}

	// Increment the provided counter.
	rulCount++;
}
//-------------------------------------------------------------------------------------------------
void CIDShieldTester::displaySummaryStatistics()
{
	m_ipResultLogger->StartTestCase("", "Summary Statistics", kSummaryTestCase);

	// The string that will be put into gstrFILE_FOR_STATISTICS (statistics file) which is a textual
	// version of all the statistics
	string strStatisticSummary = "Image analysis:\r\n";

	CString zTemp;
	zTemp.Format("\tFiles tested: %d", m_iccCounters.m_ulTotalFilesProcessed);
	m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
	strStatisticSummary += zTemp + "\r\n";

	zTemp.Format("\tPages tested: %d", m_iccCounters.m_ulTotalPages);
	m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
	strStatisticSummary += zTemp + "\r\n";

	// Report the overall OCR confidence of the documents tested.
	long nOCRDocCount;
	double dOCRConfidence;
	m_ipResultLogger->GetSummaryOCRConfidenceData(&nOCRDocCount, &dOCRConfidence);

	if (nOCRDocCount > 0)
	{
		zTemp.Format("\tAverage OCR confidence of %ld documents: %.1f%%",
			nOCRDocCount, dOCRConfidence);
		m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
		strStatisticSummary += zTemp + "\r\n";
	}

	// Calculate the number of files with expected redactions
	zTemp.Format("\tFiles containing sensitive data items: %d (%0.1f%%)",
					m_iccCounters.m_ulNumFilesWithExpectedRedactions,
					getRatioAsPercentOfTwoLongs(m_iccCounters.m_ulNumFilesWithExpectedRedactions,
					m_iccCounters.m_ulTotalFilesProcessed));
	m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
	strStatisticSummary += zTemp + "\r\n";

	// Calculate the number of pages with expected redactions
	zTemp.Format("\tPages containing sensitive data items: %d (%0.1f%%)",
					m_iccCounters.m_ulNumPagesWithExpectedRedactions,
					getRatioAsPercentOfTwoLongs(m_iccCounters.m_ulNumPagesWithExpectedRedactions,
					m_iccCounters.m_ulTotalPages));
	m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
	strStatisticSummary += zTemp + "\r\n";

	zTemp.Format("\tSensitive data items: %d", m_iccCounters.m_ulTotalExpectedRedactions);
	m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
	strStatisticSummary += zTemp + "\r\n";

	zTemp.Format("\tDocuments with overlapping sensitive data items: %d ",
		m_iccCounters.m_ulNumFilesWithOverlappingExpectedRedactions);
	m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
	strStatisticSummary += zTemp + "\r\n";

	strStatisticSummary += "\r\nWorkflow:\r\n";

	if (m_bOutputAutomatedStatsOnly)
	{
		strStatisticSummary += "\tAnalysis type: Automated redaction\r\n";
	}
	else if (m_bOutputHybridStats)
	{
		strStatisticSummary += "\tAnalysis type: Hybrid\r\n";
	}
	else
	{
		strStatisticSummary += "\tAnalysis type: Standard verification\r\n";
	}

	// If only automated stats were computed, there are no verification stats to report.
	if (!m_bOutputAutomatedStatsOnly)
	{
		string strDataTypes = m_strVerificationCondition;
		replaceVariable(strDataTypes, "|", ", ");

		strStatisticSummary += "\tManually review: Files that contain "
			+ m_strVerificationQuantifier + " " + strDataTypes + "\r\n";

		// In the case that both verification selection methods are being used, it is better
		// explained simply by seeing the two separate final results sections that are output.
		if (m_bOutputVerificationByDocumentStats && !m_bOutputVerificationByPageStats)
		{
			strStatisticSummary += "\tPage review mode: All pages of documents with sensitive data.\r\n";
		}
		else if (!m_bOutputVerificationByDocumentStats && m_bOutputVerificationByPageStats)
		{
			strStatisticSummary += "\tPage review mode: Only pages with sensitive data.\r\n";
		}
	}

	if (m_bOutputAutomatedStatsOnly || m_bOutputHybridStats)
	{
		string strDataTypes = m_strRedactionCondition;
		replaceVariable(strDataTypes, "|", ", ");

		strStatisticSummary += "\tAutomatically redact: Files that contain " +
			m_strRedactionQuantifier + " " + strDataTypes + "\r\n";
	}

	string strDataTypes = m_strRedactionQuery;
	replaceVariable(strDataTypes, "|", ", ");

	strStatisticSummary += "\tDefault to redacting the following: " + strDataTypes + "\r\n";

	// [FlexIDSCore: 3798]
	// If limiting testing to specified doc types, indicates the doc types being tested.
	if (!m_bOutputAutomatedStatsOnly && !m_setDocTypesToBeVerified.empty())
	{
		strStatisticSummary += "\tDocument types to be verified: " +
			getSetAsDelimitedList(m_setDocTypesToBeVerified) + "\r\n";
	}
	if ((m_bOutputAutomatedStatsOnly || m_bOutputHybridStats) &&
		!m_setDocTypesToBeAutomaticallyRedacted.empty())
	{
		strStatisticSummary += "\tDocument types to be automatically redacted: " +
			getSetAsDelimitedList(m_setDocTypesToBeAutomaticallyRedacted) + "\r\n";
	}

	// If limiting testing to specified doc type, indicates the doc type being tested.
	if (!m_setTypesToBeTested.empty())
	{
		strStatisticSummary += "\tLimit data types to be tested to: " +
			getSetAsDelimitedList(m_setTypesToBeTested) + "\r\n";
	}

	bool bOutputBothByDocumentAndByPageStats =
		m_bOutputVerificationByDocumentStats && m_bOutputVerificationByPageStats;

	// Final results section(s):
	// Loop once for stats based on all pages of sensitive documents being reviewed and again based
	// on only sensitive pages being reviewed. One or the other iteration may be bypassed depending
	// on the VerificationSelection setting.
	for (int i = 0; i < 2; i++)
	{
		bool bOutputtingByDocumentStats = (i == 0);

		// Check if this iteration can be skipped per the VerificationSelection setting.
		if ((bOutputtingByDocumentStats && !m_bOutputVerificationByDocumentStats) ||
			(!bOutputtingByDocumentStats && !m_bOutputVerificationByPageStats))
		{
			continue;
		}

		// Compute the number of correct redactions based on whether this is hybrid stats or not
		unsigned long ulNumCorrectRedactions = m_iccCounters.m_automatedStatistics.m_ulNumCorrectRedactions;
		if (bOutputtingByDocumentStats)
		{
			ulNumCorrectRedactions += m_iccCounters.m_verificationStatistics.m_ulExpectedRedactionsInSelectedFiles;
			m_iccCounters.m_ulNumCorrectRedactionsByDocument = ulNumCorrectRedactions;
		}
		else
		{
			ulNumCorrectRedactions += m_iccCounters.m_verificationStatistics.m_ulExpectedRedactionsInSelectedPages;
			m_iccCounters.m_ulNumCorrectRedactionsByPage = ulNumCorrectRedactions;
		}

		strStatisticSummary += "\r\nFinal results of above workflow";
		if (bOutputBothByDocumentAndByPageStats)
		{
			strStatisticSummary += bOutputtingByDocumentStats
				? " (entire document with sensitive data reviewed)"
				: " (only pages with sensitive data reviewed)";
		}
		strStatisticSummary += ":\r\n";

		// Note for number of redactions found
		zTemp.Format(
			"\tSensitive data items redacted after processing: %d (%0.1f%%)",
				ulNumCorrectRedactions, getRatioAsPercentOfTwoLongs(
					ulNumCorrectRedactions, m_iccCounters.m_ulTotalExpectedRedactions));
		m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
		strStatisticSummary += zTemp + "\r\n";

		if (!m_bOutputAutomatedStatsOnly)
		{
			if (bOutputtingByDocumentStats)
			{
				// Label for number of files selected for review
				zTemp.Format("\tFiles selected for review: %d (%0.1f%%) containing %d (%0.1f%%) pages",
					m_iccCounters.m_ulNumFilesSelectedForReview, getRatioAsPercentOfTwoLongs(
						m_iccCounters.m_ulNumFilesSelectedForReview,
						m_iccCounters. m_ulTotalFilesProcessed),
					m_iccCounters.m_ulNumPagesInFilesSelectedForReview, getRatioAsPercentOfTwoLongs(
						m_iccCounters.m_ulNumPagesInFilesSelectedForReview,
						m_iccCounters.m_ulTotalPages));
			}
			else
			{
				// Label for number of pages selected for review
				zTemp.Format("\tPages selected for review: %d (%0.1f%%)",
					m_iccCounters.m_ulNumPagesSelectedForReview, getRatioAsPercentOfTwoLongs(
						m_iccCounters.m_ulNumPagesSelectedForReview,
						m_iccCounters.m_ulTotalPages));
			}

			m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
			strStatisticSummary += zTemp + "\r\n";
		}

		if (m_bOutputAutomatedStatsOnly || m_bOutputHybridStats)
		{
			// Note for the number of false positives found
			zTemp.Format("\tFalse positives in redacted images: %d ",
				m_iccCounters.m_automatedStatistics.m_ulNumFalsePositives);
			m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
			strStatisticSummary += zTemp + "\r\n";

			// if m_ulNumFalsePositives == 0, append "(ROCE = n/a)"
			if(m_iccCounters.m_automatedStatistics.m_ulNumFalsePositives <= 0)
			{
				zTemp = "\tRatio of correctly redacted items to false positives: n/a";
			}
			// else ROCE > 0, append "(ROCE = N)"
			else
			{
				// Use integer division to get a whole number ratio as a result
				zTemp.Format("\tRatio of correctly redacted items to false positives: %0.1f",
					((double)ulNumCorrectRedactions /
					 (double)m_iccCounters.m_automatedStatistics.m_ulNumFalsePositives));
			}
			m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
			strStatisticSummary += zTemp + "\r\n";
		}
	}

	if (m_bOutputHybridStats || !m_bOutputAutomatedStatsOnly)
	{
		strStatisticSummary += "\r\nVerification efficiency:\r\n";

		if (m_bOutputHybridStats)
		{
			// Make a note for the number of expected redactions in the reviewed files
			// (This number would be a duplicate statistic in a standard workflow).
			zTemp.Format("\tSensitive data items in files presented for review: %d (%0.1f%%)",
				m_iccCounters.m_verificationStatistics.m_ulExpectedRedactionsInSelectedFiles,
				getRatioAsPercentOfTwoLongs(
					m_iccCounters.m_verificationStatistics.m_ulExpectedRedactionsInSelectedFiles,
					m_iccCounters.m_ulTotalExpectedRedactions));
			m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
			strStatisticSummary += zTemp + "\r\n";
		}

		strStatisticSummary += displayStatisticsSection(m_iccCounters.m_verificationStatistics, true);
	}

	if (m_bOutputHybridStats || m_bOutputAutomatedStatsOnly)
	{
		strStatisticSummary += "\r\nAutomated redaction efficiency:\r\n";

		// Label for Number of files selected for review
		zTemp.Format("\tAutomatically redacted files: %d (%0.1f%%)",
			m_iccCounters.m_ulNumFilesAutomaticallyRedacted, getRatioAsPercentOfTwoLongs(
				m_iccCounters.m_ulNumFilesAutomaticallyRedacted,
				m_iccCounters.m_ulTotalFilesProcessed));
		m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
		strStatisticSummary += zTemp + "\r\n";

		// Make a note for the number of expected redactions in the automatically redacted files
		zTemp.Format("\tSensitive data items in automatically redacted files: %d (%0.1f%%)",
			m_iccCounters.m_automatedStatistics.m_ulExpectedRedactionsInSelectedFiles,
			getRatioAsPercentOfTwoLongs(m_iccCounters.m_automatedStatistics.m_ulExpectedRedactionsInSelectedFiles,
				m_iccCounters.m_ulTotalExpectedRedactions));
		m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
		strStatisticSummary += zTemp + "\r\n";

		strStatisticSummary += displayStatisticsSection(m_iccCounters.m_automatedStatistics, false);
	}

	// If the number of correct redactions match the number of total expected redactions, then
	// the test was 100% successful.
	bool bOneHundredPercentSuccess =
		m_iccCounters.m_automatedStatistics.m_bTestCaseResult && m_iccCounters.m_verificationStatistics.m_bTestCaseResult;
	if (m_bOutputVerificationByDocumentStats)
	{
		bOneHundredPercentSuccess &=
			(m_iccCounters.m_ulTotalExpectedRedactions == m_iccCounters.m_ulNumCorrectRedactionsByDocument);
	}
	if (m_bOutputVerificationByPageStats)
	{
		bOneHundredPercentSuccess &=
			(m_iccCounters.m_ulTotalExpectedRedactions == m_iccCounters.m_ulNumCorrectRedactionsByPage);
	}

	m_ipResultLogger->EndTestCase(bOneHundredPercentSuccess ? VARIANT_TRUE : VARIANT_FALSE);

	// Output the printed statistics to a file so they can be viewed later without running
	// the test again.
	appendToFile(strStatisticSummary, m_strOutputFileDirectory + gstrFILE_FOR_STATISTICS);

	// Display the document type statistics
	displayDocumentTypeStats();
}
//-------------------------------------------------------------------------------------------------
void CIDShieldTester::generateCustomReport(string strReportTemplate)
{
	// Use the template filename without extension as the report name.
	string strReportName = getFileNameWithoutExtension(strReportTemplate);

	m_ipResultLogger->StartTestCase("", strReportName.c_str(), kSummaryTestCase);

    string strReport;
	ifstream ifs(strReportTemplate.c_str());
	CommentedTextFileReader fileReader(ifs, "//", false);

	// Evaluate and expand field arguments and mathematical expressions in each line of the report
	// template.
	do
	{
		string strLine = evaluateCustomReportParameters(fileReader.getLineText());

		if (!strLine.empty())
		{
			m_ipResultLogger->AddTestCaseNote(strLine.c_str());
		}
		strReport += (string)strLine + "\r\n";
	}
	while (!ifs.eof());

	ifs.close();

	m_ipResultLogger->EndTestCase(VARIANT_TRUE);

	// Output the printed statistics to a file so they can be viewed later without running
	// the test again.
	appendToFile(strReport, m_strOutputFileDirectory + "\\" + strReportName + ".txt");
}
//-------------------------------------------------------------------------------------------------
string CIDShieldTester::evaluateCustomReportParameters(const string& strSourceLine)
{
	stringCSIS strLineCSIS(strSourceLine, false);

	// Iterate through the line and evaluate all fields, enclosing the fields in
	// gstrPRELIM_ARG_MARKER so that mathematical expressions can be found afterward.
	for (auto iterStatisticsFields = m_mapStatisticFields.begin();
			iterStatisticsFields != m_mapStatisticFields.end();
			iterStatisticsFields++)
	{
		string textToFind = "%" + iterStatisticsFields->first + "%";
		for (size_t pos = strLineCSIS.find(textToFind);
			pos != string::npos;
			pos =  strLineCSIS.find(textToFind, pos + 1))
		{
			string replacement = wrapPrelimArg(asString(*iterStatisticsFields->second));
			strLineCSIS = stringCSIS(
				((string)strLineCSIS).replace(pos, textToFind.length(), replacement));
		}
	}

	// No longer need to search the line case-insensitively.
	string strLine = (string)strLineCSIS;

	// Search for and evaluate any simple mathematical expression where two arguments are separated
	// by a single mathematical argument character. Floating point values are supported for all
	// types in case of a compound mathematical expression. Supported operations are *, /, +, - and %
	// where % will add a % sign to the resulting value and will be last in precedence so that it is
	// always executed last.
	size_t pos, len;
	string strResult;

	// Evaluate multiplication, division, percentage operations first.
	vector<char> vecOperations;
	vecOperations.push_back('*');
	vecOperations.push_back('/');
	while (evaluateCustomReportMathematicalExpression(strLine, vecOperations, pos, len, strResult))
	{
		strLine = strLine.replace(pos, len, wrapPrelimArg(strResult));
	}

	// Evaluate addition, subtraction operations
	vecOperations.clear();
	vecOperations.push_back('+');
	vecOperations.push_back('-');
	while (evaluateCustomReportMathematicalExpression(strLine, vecOperations, pos, len, strResult))
	{
		strLine = strLine.replace(pos, len, wrapPrelimArg(strResult));
	}

	vecOperations.clear();
	vecOperations.push_back('%');
	while (evaluateCustomReportMathematicalExpression(strLine, vecOperations, pos, len, strResult))
	{
		strLine = strLine.replace(pos, len, wrapPrelimArg(strResult));
	}

	// Now that all expressions have been evaluated, remove the gstrPRELIM_ARG_MARKERs.
	replaceVariable(strLine, gstrPRELIM_ARG_MARKER, "");

	return strLine;
}
//-------------------------------------------------------------------------------------------------
bool CIDShieldTester::evaluateCustomReportMathematicalExpression(const string &strLine,
	const vector<char> &vecAllowedOperations, size_t &rnPos, size_t &rnLen, string &strResult)
{
	char cOperation;

	// Find the first instance of any of the specified vecAllowedOperations
	size_t nFirstPosEnd = string::npos;
	for (size_t i = 0; i < vecAllowedOperations.size(); i++)
	{
		size_t pos = strLine.find(wrapPrelimArg(string(1, vecAllowedOperations[i])));
		if (pos != string::npos && (nFirstPosEnd == string::npos || pos < nFirstPosEnd))
		{
			nFirstPosEnd = pos;
			cOperation = vecAllowedOperations[i];
		}
	}

	//
	if (nFirstPosEnd != string::npos)
	{
		size_t nFirstPos = strLine.rfind("$", nFirstPosEnd - 1) + 1;
		rnPos = nFirstPos - 3;
		string strFirstValue = ((string)strLine).substr(nFirstPos, nFirstPosEnd - nFirstPos);
		double dArg1 = asDouble(strFirstValue);

		size_t nSecondPos = nFirstPosEnd + 7;
		size_t nSecondPosEnd = strLine.find("$", nSecondPos);
		string strSecondValue = ((string)strLine).substr(nSecondPos, nSecondPosEnd - nSecondPos);
		double dArg2 = asDouble(strSecondValue);
		rnLen = nSecondPosEnd - rnPos + 3;

		CString zResult;

		switch (cOperation)
		{
			case '*':
				zResult.Format("%0.2f", dArg1 * dArg2);
				break;
			case '/':
				zResult.Format("%0.2f", dArg1 / dArg2);
				break;
			case '%':
				zResult.Format("%0.1f%%", dArg1 / dArg2 * 100.0);
				break;
			case '+':
				zResult.Format("%0.2f", dArg1 + dArg2);
				break;
			case '-':
				zResult.Format("%0.2f", dArg1 - dArg2);
				break;
		}

		strResult = (LPCTSTR)zResult;

		return true;
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
string CIDShieldTester::wrapPrelimArg(const string &strArgument)
{
	return gstrPRELIM_ARG_MARKER + strArgument + gstrPRELIM_ARG_MARKER;
}
//-------------------------------------------------------------------------------------------------
string CIDShieldTester::displayStatisticsSection(
											const CIDShieldTester::TestCaseStatistics& statistics,
											bool bVerificationStatistics)
{
	string strStatisticsSection = "";
	CString zTemp;

	zTemp = CString("\tCorrect redactions") +
		(bVerificationStatistics ? " presented to reviewers" : "");
	zTemp.Format(zTemp + ": %d", statistics.m_ulNumCorrectRedactions);
	m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
	strStatisticsSection += zTemp + "\r\n";

	zTemp = CString("\tOver-redactions") +
		(bVerificationStatistics ? " presented to reviewers" : "");
	zTemp.Format(zTemp + ": %d", statistics.m_ulNumOverRedactions);
	m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
	strStatisticsSection += zTemp + "\r\n";

	zTemp = CString("\tUnder-redactions") +
		(bVerificationStatistics ? " presented to reviewers" : "");
	zTemp.Format(zTemp + ": %d", statistics.m_ulNumUnderRedactions);
	m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
	strStatisticsSection += zTemp + "\r\n";

	// Note for the number of false positives found
	zTemp = CString("\tFalse positives") +
		(bVerificationStatistics ? " presented to reviewers" : "");
	zTemp.Format(zTemp + ": %d ", statistics.m_ulNumFalsePositives);
	m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
	strStatisticsSection += zTemp + "\r\n";

	if(statistics.m_ulNumFalsePositives <= 0)
	{
		zTemp = CString("\tRatio of correctly redacted items to false positives: n/a");
	}
	else
	{
		zTemp.Format("\tRatio of correctly redacted items to false positives: %0.1f",
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
	string strDocumentTypeLabel = "Document type analysis:";
	strStatsForLogFile += strDocumentTypeLabel + "\r\n";

	map<string, IDShieldCounterClass>::const_iterator iter;
	m_iccCounters.m_ulDocsClassified = 0;

	// Iterate through the document classifier map that was created above
	for( iter = m_mapDocTypeCounts.begin(); iter != m_mapDocTypeCounts.end(); ++iter )
	{
		// Get the doc type and the number of them found from the map
		strStatsForLogFile += displayDocumentStats(iter->first, iter->second);
		int iNumber = iter->second.m_ulDocsClassified;

		// Total number of documents classified
		m_iccCounters.m_ulDocsClassified += iNumber;

	}

	// Output the printed statistics to a file so they can be viewed later without running
	// the test again.
	appendToFile( strStatsForLogFile, m_strOutputFileDirectory + gstrFILE_FOR_STATISTICS );
}
//-------------------------------------------------------------------------------------------------
void CIDShieldTester::countDocTypes(IIUnknownVectorPtr ipFoundAttributes, const string& strSourceDoc,
									IAFDocumentPtr ipAFDoc, bool bCalculatedFoundValues,
									IDShieldCounterClass iccCounters)
{
	// Get the document types from the found attributes vector
	IIUnknownVectorPtr ipDocTypes= m_ipAFUtility->QueryAttributes( ipFoundAttributes,
								gstrDOCTYPEQUERY.c_str(), VARIANT_FALSE);
	ASSERT_RESOURCE_ALLOCATION("ELI15769", ipDocTypes != __nullptr);

	if( bCalculatedFoundValues && ipDocTypes->Size() == 0)
	{
		// Get the doc types from the AFDoc and put them into attributes in the Found IIUnknownVectorPtr
		IStrToObjectMapPtr ipObjMap = ipAFDoc->GetObjectTags();
		ASSERT_RESOURCE_ALLOCATION("ELI15772", ipObjMap != __nullptr);

		// If there is at least one item in the obj map
		if( ipObjMap->Size > 0 )
		{
			if( ipObjMap->Contains( get_bstr_t(DOC_TYPE)) == VARIANT_TRUE )
			{
				// Get the variant vector for all the doc types present
				IVariantVectorPtr ipDocumentTypes = ipObjMap->GetValue(get_bstr_t(DOC_TYPE));
				ASSERT_RESOURCE_ALLOCATION("ELI15771", ipDocumentTypes != __nullptr);

				// Use the number of document types found to control the loop
				long nDocTypes = ipDocumentTypes->Size;
				for( int x = 0; x < nDocTypes; x++ )
				{
					// Get the string for the doc type
					string strDocTypeValue = asString (ipDocumentTypes->GetItem(x).bstrVal);

					// Create an attribute for this doc type and put it into the found attributes vector
					IAttributePtr ipAttr(CLSID_Attribute);
					ASSERT_RESOURCE_ALLOCATION("ELI15767", ipAttr != __nullptr);

					// Set the type of the attribute to doc type
					ipAttr->Name = get_bstr_t( "DocumentType" );

					// Create a spatial string to use as the value of the attribute
					ISpatialStringPtr ipSS(CLSID_SpatialString);
					ASSERT_RESOURCE_ALLOCATION("ELI15768", ipSS != __nullptr);
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
	ASSERT_RESOURCE_ALLOCATION("ELI15207", ipDocTypes != __nullptr);

	// Get the number of document types
	long nDocTypes = ipDocTypes->Size();

	// If there is no doc type, then add this file to the unknown doc type log file and
	// make an entry in the doc type map for the unknown type.
	if( nDocTypes == 0 || nDocTypes > 1)
	{
		// This document was classified.
		iccCounters.m_ulDocsClassified++;
		string strDocLogFilePath = m_strOutputFileDirectory + 
			((nDocTypes == 0) ? gstrUNKNOWN_DOC_TYPE_FILES : gstrFILES_CLASSIFIED_AS_MORE_THAN_ONE_DOC_TYPE);
		appendToFile( strSourceDoc, strDocLogFilePath );

		const string strDocType = "Unknown/Multiple";

		// Add the unknown doc type to the test logger as a test case note
		m_ipResultLogger->AddTestCaseNote( strDocType.c_str() );

		// If the unknown doc type does not yet exist, enter it into the table
		if (m_mapDocTypeCounts.find(strDocType) == m_mapDocTypeCounts.end())
		{
			m_mapDocTypeCounts[strDocType] = iccCounters;
		}
		else
		{
			m_mapDocTypeCounts[strDocType] += iccCounters;
		}
	}
	else
	{
		// This document was classified.
		iccCounters.m_ulDocsClassified++;

		// Get the attribute
		IAttributePtr ipAtt = ipDocTypes->At( 0 );
		ASSERT_RESOURCE_ALLOCATION("ELI15208", ipAtt != __nullptr);

		// Get the spatial string that contains the doc type info
		ISpatialStringPtr ipSpatial = ipAtt->Value;
		ASSERT_RESOURCE_ALLOCATION("ELI15209", ipSpatial != __nullptr);

		// Get the string of the doc type
		string strDocType = asString(ipSpatial->String);

		// Add the Doc Type to the test logger as a test case note
		m_ipResultLogger->AddTestCaseNote( strDocType.c_str() );

		// If this doc type does not yet exist, enter it into the table
		if (m_mapDocTypeCounts.find(strDocType) == m_mapDocTypeCounts.end())
		{
			m_mapDocTypeCounts[strDocType] = iccCounters;
		}
		else
		{
			m_mapDocTypeCounts[strDocType] += iccCounters;
		}

		// Declare the output file path to be used.
		string strDocTypeFilePath = "";

		// Open the list file for this document type
		strDocTypeFilePath = m_strOutputFileDirectory + "\\" + gstrDOC_TYPE_PREFIX +
			strDocType + ".txt";

		// Append the source doc path to the file's list
		appendToFile( strSourceDoc, strDocTypeFilePath );
	}
}
//-------------------------------------------------------------------------------------------------
void CIDShieldTester::countExpectedOverlapsAndPages(IIUnknownVectorPtr ipExpectedAttributes,
	unsigned long& rulOverlaps, unsigned long& rulNumPagesWithRedactions)
{
	ASSERT_ARGUMENT("ELI18361", ipExpectedAttributes != __nullptr);

	// get the size of the vector
	long nSize = ipExpectedAttributes->Size();
	rulOverlaps = 0;

	set<int> setPagesWithExpectedAttributes;

	// loop over each item in the vector
	for (long i = 0; i < nSize; i++)
	{
		// get the attribute from the vector
		IAttributePtr ipAttrib1 = ipExpectedAttributes->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI18362", ipAttrib1 != __nullptr);

		// get the spatial string associated with this attribute
		ISpatialStringPtr ipSS1 = ipAttrib1->Value;
		ASSERT_RESOURCE_ALLOCATION("ELI18363", ipSS1 != __nullptr);

		// now compare all of the other attributes to this attribute
		for (long j = i+1; j < nSize; j++)
		{
			// get an attribute
			IAttributePtr ipAttrib2 = ipExpectedAttributes->At(j);
			ASSERT_RESOURCE_ALLOCATION("ELI18364", ipAttrib2 != __nullptr);

			// get the spatial string from the attribute
			ISpatialStringPtr ipSS2 = ipAttrib2->Value;
			ASSERT_RESOURCE_ALLOCATION("ELI18416", ipSS2 != __nullptr);

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
			ASSERT_RESOURCE_ALLOCATION("ELI29438", ipPage != __nullptr);

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
	ASSERT_ARGUMENT("ELI18417", ipSS1 != __nullptr);
	ASSERT_ARGUMENT("ELI18418", ipSS2 != __nullptr);

	// make sure both strings are spatial, if not just return false since they cannot overlap
	// if they are not spatial
	IIUnknownVectorPtr ipRZones1;
	if (asCppBool(ipSS1->HasSpatialInfo()))
	{
		ipRZones1 = ipSS1->GetOriginalImageRasterZones();
		ASSERT_RESOURCE_ALLOCATION("ELI18419", ipRZones1 != __nullptr);
	}
	else
	{
		return false;
	}

	IIUnknownVectorPtr ipRZones2;
	if (asCppBool(ipSS2->HasSpatialInfo()))
	{
		ipRZones2 = ipSS2->GetOriginalImageRasterZones();
		ASSERT_RESOURCE_ALLOCATION("ELI18420", ipRZones2 != __nullptr);
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
		ASSERT_RESOURCE_ALLOCATION("ELI18421", ipZone1 != __nullptr);

		for (long j = 0; j < lRZones2Size; j++)
		{
			IRasterZonePtr ipZone2 = ipRZones2->At(j);
			ASSERT_RESOURCE_ALLOCATION("ELI18422", ipZone2 != __nullptr);

			// check the area of overlap, it should be zero
			// NOTE: GetAreaOverlappingWith checks the page number so no need to do it here
			double dOverlap = ipZone1->GetAreaOverlappingWith(ipZone2);
			if (!MathVars::isZero(dOverlap))
			{
				double dOverlapPercent = (dOverlap / min(ipZone1->Area, ipZone2->Area)) * 100;

				// [FlexIDSCore:4104] Ensure at least one raster zone overlaps by
				// m_dOverlapMinimumPercent before considering the attributes overlapping.
				if (dOverlapPercent >= m_dOverlapMinimumPercent)
				{
					return true;
				}
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
	ASSERT_ARGUMENT("ELI18423", ipExpected != __nullptr);
	ASSERT_ARGUMENT("ELI18424", ipFound != __nullptr);

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
	ASSERT_ARGUMENT("ELI18425", ipAttribute != __nullptr);

	// default total area to 0
	double dArea = 0.0;

	// get the spatial string for this attribute
	ISpatialStringPtr ipSS = ipAttribute->Value;
	ASSERT_RESOURCE_ALLOCATION("ELI18426", ipSS != __nullptr);

	// check for spatial info
	if (asCppBool(ipSS->HasSpatialInfo()))
	{
		// get the vector of raster zones
		IIUnknownVectorPtr ipVecRasterZones = ipSS->GetOriginalImageRasterZones();
		ASSERT_RESOURCE_ALLOCATION("ELI18427", ipVecRasterZones != __nullptr);

		// for each zone, get its area and add that to the total area
		long lSize = ipVecRasterZones->Size();
		for (long i = 0; i < lSize; i++)
		{
			IRasterZonePtr ipRZ = ipVecRasterZones->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI18428", ipRZ != __nullptr);

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
	ASSERT_ARGUMENT("ELI18429", ipExpected != __nullptr);
	ASSERT_ARGUMENT("ELI18430", ipFound != __nullptr);

	// get the spatial strings for each attribute
	ISpatialStringPtr ipExpectedSS = ipExpected->Value;
	ASSERT_RESOURCE_ALLOCATION("ELI18431", ipExpectedSS != __nullptr);

	ISpatialStringPtr ipFoundSS = ipFound->Value;
	ASSERT_RESOURCE_ALLOCATION("ELI18432", ipFoundSS != __nullptr);

	// default the overlap area to 0
	double dAreaOfOverlap = 0.0;

	// Keeps track of whether any raster zone overlaps by m_dOverlapMinimumPercent.
	bool bAttributesOverlap = false;

	// make sure both strings are spatial
	if (asCppBool(ipExpectedSS->HasSpatialInfo()) && asCppBool(ipFoundSS->HasSpatialInfo()))
	{
		// get the vector of raster zones for both spatial strings
		IIUnknownVectorPtr ipExpectedRZs = ipExpectedSS->GetOriginalImageRasterZones();
		ASSERT_RESOURCE_ALLOCATION("ELI18433", ipExpectedRZs != __nullptr);

		IIUnknownVectorPtr ipFoundRZs = ipFoundSS->GetOriginalImageRasterZones();
		ASSERT_RESOURCE_ALLOCATION("ELI18434", ipFoundRZs != __nullptr);

		// loop through each of the raster zones and add the area of overlap
		long lExpectedSize = ipExpectedRZs->Size();
		long lFoundSize = ipFoundRZs->Size();
		for (long i = 0; i < lExpectedSize; i++)
		{
			IRasterZonePtr ipERZ = ipExpectedRZs->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI18435", ipERZ != __nullptr);

			for (long j = 0; j < lFoundSize; j++)
			{
				IRasterZonePtr ipFRZ = ipFoundRZs->At(j);
				ASSERT_RESOURCE_ALLOCATION("ELI18436", ipFRZ != __nullptr);

				double dOverlap = ipERZ->GetAreaOverlappingWith(ipFRZ);

				if (!MathVars::isZero(dOverlap))
				{
					// add the area of overlap (GetArea checks page number of zone)
					dAreaOfOverlap += dOverlap;

					if (!bAttributesOverlap)
					{
						double dOverlapPercent = (dOverlap / min(ipERZ->Area, ipFRZ->Area)) * 100;

						// [FlexIDSCore:4104] Ensure at least one raster zone overlaps by
						// m_dOverlapMinimumPercent before allowing any overlap to be reported.
						if (dOverlapPercent >= m_dOverlapMinimumPercent)
						{
							bAttributesOverlap = true;
						}
					}
				}
			}
		}
	}

	return (bAttributesOverlap ? dAreaOfOverlap : 0);
}
//-------------------------------------------------------------------------------------------------
void CIDShieldTester::addAttributeToTestOutputVOA(IAttributePtr ipAttribute,
												  const string &strPrefix,
												  const string &strSourceVOA)
{
	ASSERT_ARGUMENT("ELI18437", ipAttribute != __nullptr);

	// create a new attribute
	IAttributePtr ipNewAttribute(CLSID_Attribute);
	ASSERT_RESOURCE_ALLOCATION("ELI18438", ipNewAttribute != __nullptr);

	// get the copyable object
	ICopyableObjectPtr ipCopy = ipNewAttribute;
	ASSERT_RESOURCE_ALLOCATION("ELI18439", ipCopy != __nullptr);

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
	ASSERT_ARGUMENT("ELI18504", ipFoundAttributes != __nullptr);

	// set all counts to 0
	rulHCData = rulMCData = rulLCData = rulClues = 0;

	long lSize = ipFoundAttributes->Size();

	for (long i=0; i < lSize; i++)
	{
		IAttributePtr ipAttribute = ipFoundAttributes->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI18505", ipAttribute != __nullptr);

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
	ASSERT_ARGUMENT("ELI18513", ipAttributeVector != __nullptr);

	// create a new vector
	IIUnknownVectorPtr ipNewVector(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI18514", ipNewVector != __nullptr);

	// get the current vector size
	long lSize = ipAttributeVector->Size();

	// loop through each element of the attribute vector and compare its
	// type with the specified type
	for (long i=0; i < lSize; i++)
	{
		IAttributePtr ipAttribute = ipAttributeVector->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI18515", ipAttribute != __nullptr);

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
IIUnknownVectorPtr CIDShieldTester::filterDocumentTypeAttributes(IIUnknownVectorPtr ipAttributeVector)
{
	ASSERT_ARGUMENT("ELI36105", ipAttributeVector != __nullptr);

	// create a new vector
	IIUnknownVectorPtr ipNewVector(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI36106", ipNewVector != __nullptr);

	// get the current vector size
	long lSize = ipAttributeVector->Size();

	// loop through each element of the attribute vector to look for DocumentType attributes.
	for (long i=0; i < lSize; i++)
	{
		IAttributePtr ipAttribute = ipAttributeVector->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI36112", ipAttribute != __nullptr);

		string strName = asString(ipAttribute->Name);
		makeLowerCase(strName);

		// if the attribute contains the specified type, add it to the vector
		if (strName != "documenttype")
		{
			ipNewVector->PushBack(ipAttribute);
		}
	}

	// return the new vector
	return ipNewVector;
}
//-------------------------------------------------------------------------------------------------
void CIDShieldTester::updateVerificationAttributeTester()
{
	// If an attribute tester was already created, just reset it
	if (m_apVerificationTester.get() != __nullptr)
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
	if (m_apRedactionTester.get() != __nullptr)
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
string CIDShieldTester::expandTagsAndFunctions(const string& strInput, const string& strSourceDoc)
{
	string strExpanded = asString(m_ipMiscUtils->ExpandTagsAndFunctions(
		strInput.c_str(), m_ipTagUtility, _bstr_t(strSourceDoc.c_str()).Detach(), __nullptr));

	return strExpanded;
}
//-------------------------------------------------------------------------------------------------
string  CIDShieldTester::displayDocumentStats(const string & strDocumentType, const IDShieldCounterClass& stats)
{
	string strStatisticSummary = "";

	string strType = "\t" + strDocumentType + ": ";
	strStatisticSummary += strType + "\r\n";

	m_ipResultLogger->StartTestCase("", strType.c_str(), kSummaryTestCase);

	CString zTemp;

	zTemp.Format("\t\tFiles tested: %d", stats.m_ulTotalFilesProcessed);
	m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
	strStatisticSummary += zTemp + "\r\n";

	zTemp.Format("\t\tPages tested: %d", stats.m_ulTotalPages);
	m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
	strStatisticSummary += zTemp + "\r\n";

	// Calculate the number of files with expected redactions
	zTemp.Format("\t\tFiles containing sensitive data items: %d (%0.1f%%)",
					stats.m_ulNumFilesWithExpectedRedactions,
					getRatioAsPercentOfTwoLongs(stats.m_ulNumFilesWithExpectedRedactions,
					stats.m_ulTotalFilesProcessed));
	m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
	strStatisticSummary += zTemp + "\r\n";

	// Calculate the number of pages with expected redactions
	zTemp.Format("\t\tPages containing sensitive data items: %d (%0.1f%%)",
					stats.m_ulNumPagesWithExpectedRedactions,
					getRatioAsPercentOfTwoLongs(stats.m_ulNumPagesWithExpectedRedactions,
					stats.m_ulTotalPages));
	m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
	strStatisticSummary += zTemp + "\r\n";

	zTemp.Format("\t\tSensitive data items: %d", stats.m_ulTotalExpectedRedactions);
	m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
	strStatisticSummary += zTemp + "\r\n";

	zTemp.Format("\t\tDocuments with overlapping sensitive data items: %d ",
		stats.m_ulNumFilesWithOverlappingExpectedRedactions);
	m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
	strStatisticSummary += zTemp + "\r\n";

	if (!m_bOutputAutomatedStatsOnly || m_bOutputHybridStats)
	{
		zTemp = CString("\t\tCorrect redactions presented to reviewers");
		zTemp.Format(zTemp + ": %d", stats.m_verificationStatistics.m_ulNumCorrectRedactions);
		m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
		strStatisticSummary += zTemp + "\r\n";

		zTemp = CString("\t\tOver-redactions presented to reviewers" );
		zTemp.Format(zTemp + ": %d", stats.m_verificationStatistics.m_ulNumOverRedactions);
		m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
		strStatisticSummary += zTemp + "\r\n";

		zTemp = CString("\t\tUnder-redactions presented to reviewers");
		zTemp.Format(zTemp + ": %d", stats.m_verificationStatistics.m_ulNumUnderRedactions);
		m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
		strStatisticSummary += zTemp + "\r\n";

		zTemp = CString("\t\tFalse positives presented to reviewers");
		zTemp.Format(zTemp + ": %d ", stats.m_verificationStatistics.m_ulNumFalsePositives);
		m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
		strStatisticSummary += zTemp + "\r\n";

		if(stats.m_verificationStatistics.m_ulNumFalsePositives <= 0)
		{
			zTemp = CString("\t\tRatio of correctly redacted items to false positives: n/a");
		}
		else
		{
			zTemp.Format("\t\tRatio of correctly redacted items to false positives: %0.1f",
				((double) stats.m_verificationStatistics.m_ulNumCorrectRedactions /
					(double)stats.m_verificationStatistics.m_ulNumFalsePositives));
		}
		m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
		strStatisticSummary += zTemp + "\r\n";
	}

	if (m_bOutputAutomatedStatsOnly || m_bOutputHybridStats)
	{
		zTemp = CString("\t\tCorrect redactions");
		zTemp.Format(zTemp + ": %d", stats.m_automatedStatistics.m_ulNumCorrectRedactions);
		m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
		strStatisticSummary += zTemp + "\r\n";

		zTemp = CString("\t\tOver-redactions");
		zTemp.Format(zTemp + ": %d", stats.m_automatedStatistics.m_ulNumOverRedactions);
		m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
		strStatisticSummary += zTemp + "\r\n";

		zTemp = CString("\t\tUnder-redactions");
		zTemp.Format(zTemp + ": %d", stats.m_automatedStatistics.m_ulNumUnderRedactions);
		m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
		strStatisticSummary += zTemp + "\r\n";

		zTemp = CString("\t\tFalse positives");
		zTemp.Format(zTemp + ": %d ", stats.m_automatedStatistics.m_ulNumFalsePositives);
		m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
		strStatisticSummary += zTemp + "\r\n";

		if(stats.m_automatedStatistics.m_ulNumFalsePositives <= 0)
		{
			zTemp = CString("\t\tRatio of correctly redacted items to false positives: n/a");
		}
		else
		{
			zTemp.Format("\t\tRatio of correctly redacted items to false positives: %0.1f",
				((double) stats.m_automatedStatistics.m_ulNumCorrectRedactions /
					(double)stats.m_automatedStatistics.m_ulNumFalsePositives));
		}
		m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
		strStatisticSummary += zTemp + "\r\n";
	}

	m_ipResultLogger->EndTestCase(VARIANT_TRUE);

	return strStatisticSummary;
}
//-------------------------------------------------------------------------------------------------
CIDShieldTester::TestCaseStatistics::TestCaseStatistics()
: m_bTestCaseResult(true)
, m_ulTotalExpectedRedactions(0)
, m_ulExpectedRedactionsInSelectedFiles(0)
, m_ulExpectedRedactionsInSelectedPages(0)
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
	m_ulExpectedRedactionsInSelectedPages = 0;
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
	m_ulExpectedRedactionsInSelectedPages += otherTestCase.m_ulExpectedRedactionsInSelectedPages;
	m_ulFoundRedactions += otherTestCase.m_ulFoundRedactions;
	m_ulNumCorrectRedactions += otherTestCase.m_ulNumCorrectRedactions;
	m_ulNumFalsePositives += otherTestCase.m_ulNumFalsePositives;
	m_ulNumOverRedactions += otherTestCase.m_ulNumOverRedactions;
	m_ulNumUnderRedactions += otherTestCase.m_ulNumUnderRedactions;
	m_ulNumMisses += otherTestCase.m_ulNumMisses;

	return *this;
}

//-------------------------------------------------------------------------------------------------
// CIDShieldTester::IDShieldCounterClass Class
//-------------------------------------------------------------------------------------------------
CIDShieldTester::IDShieldCounterClass::IDShieldCounterClass() :
	m_ulTotalExpectedRedactions (0),
	m_ulNumCorrectRedactionsByDocument (0),
	m_ulNumCorrectRedactionsByPage (0),
	m_ulNumOverRedactions (0),
	m_ulNumUnderRedactions (0),
	m_ulNumMisses (0),
	m_ulTotalFilesProcessed (0),
	m_ulNumFilesWithExpectedRedactions (0),
	m_ulNumFilesSelectedForReview (0),
	m_ulNumPagesInFilesSelectedForReview (0),
	m_ulNumPagesSelectedForReview (0),
	m_ulNumExpectedRedactionsInReviewedFiles (0),
	m_ulNumExpectedRedactionsInRedactedFiles (0),
	m_ulNumFilesAutomaticallyRedacted (0),
	m_ulNumFilesWithOverlappingExpectedRedactions (0),
	m_ulTotalPages (0),
	m_ulNumPagesWithExpectedRedactions (0)
{

}
//-------------------------------------------------------------------------------------------------
CIDShieldTester::IDShieldCounterClass::IDShieldCounterClass(const IDShieldCounterClass& c)
{
	m_ulTotalExpectedRedactions = c.m_ulTotalExpectedRedactions;
	m_ulNumCorrectRedactionsByDocument = c.m_ulNumCorrectRedactionsByDocument;
	m_ulNumCorrectRedactionsByPage = c.m_ulNumCorrectRedactionsByPage;
	m_ulNumOverRedactions = c.m_ulNumOverRedactions;
	m_ulNumUnderRedactions = c.m_ulNumUnderRedactions;
	m_ulNumMisses = c.m_ulNumMisses;
	m_ulTotalFilesProcessed = c.m_ulTotalFilesProcessed;
	m_ulNumFilesWithExpectedRedactions = c.m_ulNumFilesWithExpectedRedactions;
	m_ulNumFilesSelectedForReview = c.m_ulNumFilesSelectedForReview;
	m_ulNumPagesInFilesSelectedForReview = c.m_ulNumPagesInFilesSelectedForReview;
	m_ulNumPagesSelectedForReview = c.m_ulNumPagesSelectedForReview;
	m_ulNumExpectedRedactionsInReviewedFiles = c.m_ulNumExpectedRedactionsInReviewedFiles;
	m_ulNumExpectedRedactionsInRedactedFiles = c.m_ulNumExpectedRedactionsInRedactedFiles;
	m_ulNumFilesAutomaticallyRedacted = c.m_ulNumFilesAutomaticallyRedacted;
	m_ulNumFilesWithOverlappingExpectedRedactions = c.m_ulNumFilesWithOverlappingExpectedRedactions;
	m_ulTotalPages = c.m_ulTotalPages;
	m_ulNumPagesWithExpectedRedactions = c.m_ulNumPagesWithExpectedRedactions;
	m_ulDocsClassified = c.m_ulDocsClassified;
	m_automatedStatistics = c.m_automatedStatistics;
	m_verificationStatistics = c.m_verificationStatistics;
}
//-------------------------------------------------------------------------------------------------
CIDShieldTester::IDShieldCounterClass& CIDShieldTester::IDShieldCounterClass::operator +=(const IDShieldCounterClass& rhs)
{
	m_ulTotalExpectedRedactions += rhs.m_ulTotalExpectedRedactions;
	m_ulNumCorrectRedactionsByDocument += rhs.m_ulNumCorrectRedactionsByDocument;
	m_ulNumCorrectRedactionsByPage += rhs.m_ulNumCorrectRedactionsByPage;
	m_ulNumOverRedactions += rhs.m_ulNumOverRedactions;
	m_ulNumUnderRedactions += rhs.m_ulNumUnderRedactions;
	m_ulNumMisses += rhs.m_ulNumMisses;
	m_ulTotalFilesProcessed += rhs.m_ulTotalFilesProcessed;
	m_ulNumFilesWithExpectedRedactions += rhs.m_ulNumFilesWithExpectedRedactions;
	m_ulNumFilesSelectedForReview += rhs.m_ulNumFilesSelectedForReview;
	m_ulNumPagesInFilesSelectedForReview += rhs.m_ulNumPagesInFilesSelectedForReview;
	m_ulNumPagesSelectedForReview += rhs.m_ulNumPagesSelectedForReview;
	m_ulNumExpectedRedactionsInReviewedFiles += rhs.m_ulNumExpectedRedactionsInReviewedFiles;
	m_ulNumExpectedRedactionsInRedactedFiles += rhs.m_ulNumExpectedRedactionsInRedactedFiles;
	m_ulNumFilesAutomaticallyRedacted += rhs.m_ulNumFilesAutomaticallyRedacted;
	m_ulNumFilesWithOverlappingExpectedRedactions += rhs.m_ulNumFilesWithOverlappingExpectedRedactions;
	m_ulTotalPages += rhs.m_ulTotalPages;
	m_ulNumPagesWithExpectedRedactions += rhs.m_ulNumPagesWithExpectedRedactions;
	m_ulDocsClassified += rhs.m_ulDocsClassified;
	m_automatedStatistics += rhs.m_automatedStatistics;
	m_verificationStatistics += rhs.m_verificationStatistics;
	return *this;
}
//-------------------------------------------------------------------------------------------------
void CIDShieldTester::IDShieldCounterClass::clear()
{
	m_ulTotalExpectedRedactions = 0;
	m_ulNumCorrectRedactionsByDocument = 0;
	m_ulNumCorrectRedactionsByPage = 0;
	m_ulNumOverRedactions = 0;
	m_ulNumUnderRedactions = 0;
	m_ulNumMisses = 0;
	m_ulTotalFilesProcessed = 0;
	m_ulNumFilesWithExpectedRedactions = 0;
	m_ulNumFilesSelectedForReview = 0;
	m_ulNumPagesInFilesSelectedForReview = 0;
	m_ulNumPagesSelectedForReview = 0;
	m_ulNumExpectedRedactionsInReviewedFiles = 0;
	m_ulNumExpectedRedactionsInRedactedFiles = 0;
	m_ulNumFilesAutomaticallyRedacted = 0;
	m_ulNumFilesWithOverlappingExpectedRedactions = 0;
	m_ulTotalPages = 0;
	m_ulNumPagesWithExpectedRedactions = 0;
	m_ulDocsClassified = 0;
	m_automatedStatistics.reset();
	m_verificationStatistics.reset();
}
