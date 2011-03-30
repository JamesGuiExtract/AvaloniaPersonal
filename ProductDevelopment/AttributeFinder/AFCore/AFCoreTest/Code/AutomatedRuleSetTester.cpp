// AutomatedRuleSetTester.cpp : Implementation of CAutomatedRuleSetTester
#include "stdafx.h"
#include "AFCoreTest.h"
#include "AutomatedRuleSetTester.h"

#include <SpecialStringDefinitions.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <CommentedTextFileReader.h>
#include <StringTokenizer.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>
#include <TextFunctionExpander.h>

#include <cstdio>
#include <fstream>
#include <set>
#include <algorithm>
#include <cstdlib>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Local structs
//-------------------------------------------------------------------------------------------------
// PURPOSE: To store data from computing the "matching" score between an expected and a found
// attribute.  This data is important in allowing for attributes to be found in any order
// and computing which attribute is a best match to which other attribute without depending
// on the order they appear in the file (NOTE - To break a tie between matching scores, the
// order an item appears in a file will be used i.e. if found attribute 2 and 3 have the
// same "best match score" when compared against expected attribute 3, found attribute 3 will
// be the preferred attribute because it lines up horizontally with the expected attribute).
struct AttributeScoreData
{
	// Attribute match score
	long m_lScore;

	// Whether this pair is still valid
	// (can be marked invalid due to not a match or
	// if either the expected or found attribute has
	// been used)
	bool m_bValid;

	// Expected attribute index
	long m_lExpectedIndex;

	// Found attribute index
	long m_lFoundIndex;
};

//-------------------------------------------------------------------------------------------------
// Local helper methods
//-------------------------------------------------------------------------------------------------
// Sorting function used to compare attribute score data values
// We need to sort these in largest to smallest order so we return
// true if a > b
// a > b if:
// 1. a is valid and b is not OR
// 2. a and b are valid
//		A. a.score > b.score
//		B. a.score == b.score
//			a. a.expected_index == a.found_index
//				i. b.expected_index == b.found_index
//					1. a.expected_index < b.expected_index (prefer earlier index)
//				ii. true (a matched index, b didn't, prefer matched index)
//			b. false (a did not match its index, prefer b)
bool attributeScoreDataComparison(AttributeScoreData a, AttributeScoreData b)
{
	// If both are valid then check whether they have the same score or not
	if (a.m_bValid && b.m_bValid)
	{
		// Different scores, return a > b
		if (a.m_lScore != b.m_lScore)
		{
			return a.m_lScore > b.m_lScore;
		}
		else
		{
			// Same score check whether the index for a is the same
			if (a.m_lExpectedIndex == a.m_lFoundIndex)
			{
				// Now check if b has the same index
				if (b.m_lExpectedIndex == b.m_lFoundIndex)
				{
					// return a < b (prefer earlier match to later)
					return a.m_lExpectedIndex < b.m_lExpectedIndex;
				}
				else
				{
					// a matched its index but b didn't, prefer matched index
					return true;
				}
			}
			else
			{
				// a did not match its index, prefer b
				return false;
			}
		}
	}
	// Return whether a is valid or not
	else
	{
		return a.m_bValid;
	}
}
//-------------------------------------------------------------------------------------------------
// Performs a sort on the attribute score data vector
void sortAttributeScoreData(vector<AttributeScoreData>& rvecData)
{
	// Sort the vector of scores using the above comparison function
	sort(rvecData.begin(), rvecData.end(), attributeScoreDataComparison);
}
//-------------------------------------------------------------------------------------------------
// Iterates through the vector and marks each item that has either the specified expected
// or found index as invalid
void markAsInvalid(vector<AttributeScoreData>& rvecData, long lExpectedIndex, long lFoundIndex)
{
	// Walk the score vector and invalidate any entries that match either the
	// expected or found index
	for(vector<AttributeScoreData>::iterator it = rvecData.begin(); it != rvecData.end(); it++)
	{
		// If either index matches, mark that entry as invalid
		if (it->m_lExpectedIndex == lExpectedIndex || it->m_lFoundIndex == lFoundIndex)
		{
			it->m_bValid = false;
		}
	}
}
//-------------------------------------------------------------------------------------------------
void makeAttributesUpperCase(IIUnknownVectorPtr ipAttributes)
{
	ASSERT_ARGUMENT("ELI24689", ipAttributes != __nullptr);

	long lSize = ipAttributes->Size();
	for(long i = 0; i < lSize; i++)
	{
		// Get this Attribute
		IAttributePtr ipAttr = ipAttributes->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI09731", ipAttr != __nullptr);

		// Get the Value
		ISpatialStringPtr ipValue = ipAttr->Value;
		ASSERT_RESOURCE_ALLOCATION("ELI15521", ipValue != __nullptr);

		// Convert the Value to upper case
		ipValue->ToUpperCase();

		// Process any sub-attributes
		makeAttributesUpperCase(ipAttr->SubAttributes);
	}
}

//-------------------------------------------------------------------------------------------------
// CAutomatedRuleSetTester
//-------------------------------------------------------------------------------------------------
CAutomatedRuleSetTester::CAutomatedRuleSetTester()
: m_ipResultLogger(NULL),
  m_ipAttrFinderEngine(CLSID_AttributeFinderEngine),
  m_ipCurrentAttributes( CLSID_IUnknownVector ),
  m_bCaseSensitive(true),
  m_bEAVMustExist(false),
  m_ipFAMTagManager(CLSID_FAMTagManager),
  m_ipAFUtility(CLSID_AFUtility)
{
	try
	{
		ASSERT_RESOURCE_ALLOCATION("ELI07427", m_ipAttrFinderEngine != __nullptr);
		ASSERT_RESOURCE_ALLOCATION("ELI07429", m_ipCurrentAttributes != __nullptr);
		ASSERT_RESOURCE_ALLOCATION("ELI15211", m_ipFAMTagManager != __nullptr );
		ASSERT_RESOURCE_ALLOCATION("ELI28477", m_ipAFUtility != __nullptr);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07430")
}
//-------------------------------------------------------------------------------------------------
CAutomatedRuleSetTester::~CAutomatedRuleSetTester()
{
	try
	{
		m_ipAttrFinderEngine = __nullptr;
		m_ipCurrentAttributes = __nullptr;
		m_ipFAMTagManager = __nullptr;
		m_ipAFUtility = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16307");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAutomatedRuleSetTester::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ITestableComponent,
		&IID_ILicensedComponent//,
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
STDMETHODIMP CAutomatedRuleSetTester::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		try
		{
			// Check license
			validateLicense();
			m_bEAVMustExist = false;
			m_bCaseSensitive = true;

			if (m_ipResultLogger == __nullptr)
			{
				throw UCLIDException("ELI06120", "Please set ResultLogger before proceeding.");
			}

			// Default to displaying entries for each test case
			m_ipResultLogger->AddEntriesToTestLogger = VARIANT_TRUE;

			string strTCLFilename = asString(strTCLFile);

			// find the master test file
			string strTestRuleSetsFile = getMasterTestFileName(pParams, strTCLFilename);
			try
			{
				validateFileOrFolderExistence(strTestRuleSetsFile);
			}
			catch (UCLIDException& ue)
			{
				UCLIDException uexOuter("ELI07336", "Unable to read RuleSet master test input file.", ue);
				throw uexOuter;
			}

			//Clear all of the maps for individual results
			m_mapTotalCorrectFound.clear();
			m_mapTotalExpected.clear();
			m_mapTotalIncorrectlyFound.clear();

			processDatFile(strTestRuleSetsFile);
			addAttributeResultCase();
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI04747")
	}
	catch(UCLIDException ue)
	{
		// Store the exception for display and continue
		m_ipResultLogger->AddComponentTestException(get_bstr_t(ue.asStringizedByteStream()));
	}

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAutomatedRuleSetTester::raw_RunInteractiveTests()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAutomatedRuleSetTester::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		// Check license
		validateLicense();

		m_ipResultLogger = pLogger;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07288")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAutomatedRuleSetTester::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAutomatedRuleSetTester::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// Private methods
//-------------------------------------------------------------------------------------------------
string CAutomatedRuleSetTester::attributeAsString(IAttributePtr ipAttribute, int nLevel)
{
	try
	{
		ASSERT_ARGUMENT("ELI24690", ipAttribute != __nullptr);

		// format the level string
		string strDots("");
		for (int i=0; i < nLevel; i++)
		{
			strDots += ".";
		}

		string strAttribute(strDots);

		strAttribute += asString(ipAttribute->Name);
		strAttribute += "|";

		// Retrieve Value object
		ISpatialStringPtr ipValue = ipAttribute->Value;
		ASSERT_RESOURCE_ALLOCATION("ELI15520", ipValue != __nullptr);

		// Get the string - convert any cpp string (ex. \r, \n, etc. )to normal string
		// (ex. \\r, \\n, etc.) for display purpose
		string strValue = asString(ipValue->String);
		convertCppStringToNormalString(strValue);

		strAttribute += strValue;

		// add type if only it's not empty
		string strType = asString(ipAttribute->Type);
		if (!strType.empty())
		{
			strAttribute += "|";
			strAttribute += strType;
		}

		// if there's any sub attributes
		IIUnknownVectorPtr ipSubAttributes(ipAttribute->SubAttributes);
		if (ipSubAttributes)
		{
			nLevel++;
			long lSize = ipSubAttributes->Size();
			for (long n=0; n < lSize; n++)
			{
				IAttributePtr ipSubAttr(ipSubAttributes->At(n));
				if (ipSubAttr)
				{
					// get the attribute string recursively
					string strSubAttributeString = attributeAsString(ipSubAttr, nLevel);
					strAttribute += "\r\n" + strSubAttributeString;
				}
			}
		}

		return strAttribute;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24738");
}
//-------------------------------------------------------------------------------------------------
string CAutomatedRuleSetTester::getAttributesCompareString(IIUnknownVectorPtr ipAttributes)
{
	try
	{
		ASSERT_ARGUMENT("ELI24691", ipAttributes != __nullptr)

			// Create the string
			string strAttributes("");

		// Iterate the vector building up the string
		long lSize = ipAttributes->Size();
		for (long i = 0; i < lSize; i++)
		{
			if (i > 0)
			{
				strAttributes += "\r\n";
			}

			IAttributePtr ipAttribute(ipAttributes->At(i));
			strAttributes += attributeAsString(ipAttribute, 0);
		}

		return strAttributes;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24739");
}
//-------------------------------------------------------------------------------------------------
string CAutomatedRuleSetTester::getQualifiedName(IAttributePtr ipAttribute,
												 const string& strQualifiedAttrName,
												 const string& strSeparator)
{
	try
	{
		ASSERT_ARGUMENT("ELI24692", ipAttribute != __nullptr);

		string strNameType = asString(ipAttribute->Name);
		string strType = asString(ipAttribute->Type);
		if ( strType.length() != 0 ) 
		{
			strNameType += "\\" + strType;
		}
		return strQualifiedAttrName + strSeparator + strNameType;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24740");
}
//-------------------------------------------------------------------------------------------------
string CAutomatedRuleSetTester::getTopLevelAttributeString(IAttributePtr ipAttribute)
{
	try
	{
		ASSERT_ARGUMENT("ELI24693", ipAttribute != __nullptr);

		// Build a string for the name
		string strAttribute = asString(ipAttribute->Name) + "|";

		// Get the value
		ISpatialStringPtr ipSS = ipAttribute->Value;
		ASSERT_RESOURCE_ALLOCATION("ELI24694", ipSS != __nullptr);
		strAttribute += asString(ipSS->String);

		// Get the type and add it if it is not empty
		string strType = asString(ipAttribute->Type);
		if (!strType.empty())
		{
			strAttribute += "|";
			strAttribute += strType;
		}

		return strAttribute;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24741");
}
//-------------------------------------------------------------------------------------------------
long CAutomatedRuleSetTester::getAttributeSize(IAttributePtr ipAttribute)
{
	try
	{
		ASSERT_ARGUMENT("ELI24695", ipAttribute != __nullptr);

		// Default count to 1
		long lCount = 1;

		// Get subattributes
		IIUnknownVectorPtr ipSubAttributes = ipAttribute->SubAttributes;
		if (ipSubAttributes != __nullptr)
		{
			// For each sub attribute, get the count
			long lSize = ipSubAttributes->Size();
			for (long i=0; i < lSize; i++)
			{
				IAttributePtr ipTemp = ipSubAttributes->At(i);
				lCount += getAttributeSize(ipTemp);
			}
		}

		return lCount;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24742");
}
//-------------------------------------------------------------------------------------------------
pair<long, bool> CAutomatedRuleSetTester::computeScore(IAttributePtr ipExpected,
													   IAttributePtr ipFound)
{
	try
	{
		ASSERT_ARGUMENT("ELI24701", ipExpected != __nullptr);
		ASSERT_ARGUMENT("ELI24702", ipFound != __nullptr);

		long lScore = 0;
		bool bMatched = false;

		// Check if the attributes match (compare Name|Value|Type)
		if (getTopLevelAttributeString(ipExpected) == getTopLevelAttributeString(ipFound))
		{
			// Attributes match, increment score and set bMatched to true
			lScore++;
			bMatched = true;

			// ---------------------------------------
			// Compute the score of the sub-attributes
			// ---------------------------------------

			// Get the sub attributes
			IIUnknownVectorPtr ipExpectedSubs = ipExpected->SubAttributes;
			ASSERT_RESOURCE_ALLOCATION("ELI24703", ipExpectedSubs != __nullptr);
			IIUnknownVectorPtr ipFoundSubs = ipFound->SubAttributes;
			ASSERT_RESOURCE_ALLOCATION("ELI24704", ipFound != __nullptr);

			// Create and initialize the map to store whether an Attribute was a match
			// and/or was a best match:
			// map<long, pair<bool, bool>>(found index, matched, best match)
			// Also create a set to store the index of a found item that was considered
			// a best match so that we can short circuit comparison on that attribute later
			map<long, pair<bool, bool>> mapFoundMatched;
			set<long> setBestMatches;
			long lFoundSize = ipFoundSubs->Size();
			for (long i=0; i < lFoundSize; i++)
			{
				mapFoundMatched[i] = pair<bool, bool>(false, false);
			}

			// Iterate through the sub attributes and compute score
			map<long, long> mapBestScores; // Holds best scores for a particular match
			long lExpectedSize = ipExpectedSubs->Size();
			for (long i=0; i < lExpectedSize; i++)
			{
				// Get the expected attribute
				IAttributePtr ipTemp1 = ipExpectedSubs->At(i);
				ASSERT_RESOURCE_ALLOCATION("ELI24706", ipTemp1 != __nullptr);

				// Compute the score for each found attribute
				// Store the scores in a vector (computeScore returns a pair
				// containing the score and whether there was a match or not:
				// vector<pair<long, bool>>(score, match)
				vector<pair<long, bool>> vecScores;
				vecScores.reserve(lFoundSize);
				for (long j=0; j < lFoundSize; j++)
				{
					// Skip any item that was already marked as a best match
					// In order to preserve the index matching from the score
					// vector to the found attribute vector add an empty entry
					// with the match value set to false to the score vector
					if (setBestMatches.find(j) != setBestMatches.end())
					{
						vecScores.push_back(pair<long, bool>(0, false));
						continue;
					}

					// Get the found attribute
					IAttributePtr ipTemp2 = ipFoundSubs->At(j);
					ASSERT_RESOURCE_ALLOCATION("ELI24707", ipTemp2 != __nullptr);

					// Compute the score and store the computed score in the
					// score vector
					pair<long, bool> prTemp = computeScore(ipTemp1, ipTemp2);
					vecScores.push_back(prTemp);

					// If this was a potential best match, mark it as such 
					if (prTemp.second)
					{
						mapFoundMatched[j].first = prTemp.second;
					}
				}

				// Look through all the computed scores and find the best score
				// NOTE: The best score may be a negative number
				long lBestScoreIndex = -1;
				long lBestScore = 0;
				long lVecSize = (long)vecScores.size();
				for(long j=0; j < lVecSize; j++)
				{
					// Check if the score is from a "successful" match
					if (vecScores[j].second)
					{
						// If the best score index has not been set yet, or the new score
						// is better than the previous best score then set the new best
						// score and index
						if (lBestScoreIndex == -1 || vecScores[j].first > lBestScore)
						{
							lBestScoreIndex = j;
							lBestScore = vecScores[j].first;
						}
					}
				}

				// Check for a best score found
				if (lBestScoreIndex != -1)
				{
					// Mark this attribute as a best match
					mapFoundMatched[lBestScoreIndex].second = true;

					// Store the score
					mapBestScores[lBestScoreIndex] = lBestScore;

					// Add this index to the set of best matches (this will
					// allow short circuit of comparison for this attribute)
					setBestMatches.insert(lBestScoreIndex);
				}
			}

			// Now compute the score
			for(map<long, pair<bool, bool>>::iterator it = mapFoundMatched.begin();
				it != mapFoundMatched.end(); it++)
			{
				long lTempScore = 0;

				// Check if this was a false positive match
				// (never matched or never returned as a best match)
				if (!it->second.first || !it->second.second)
				{
					// Get the found attribute
					IAttributePtr ipTemp = ipFoundSubs->At(it->first);
					ASSERT_RESOURCE_ALLOCATION("ELI24709", ipTemp != __nullptr);

					// False positive, score is AttributeSize * -1
					lTempScore = -1 * getAttributeSize(ipTemp);
				}
				// This was the best match, get the score
				else
				{
					// Find the score in the map
					map<long, long>::iterator score = mapBestScores.find(it->first);

					// If it is not found, throw an exception
					// (Should never have the case of a value being a best score and its
					// score not being in the collection)
					if (score == mapBestScores.end())
					{
						UCLIDException uex("ELI24705", "Best score value was not in the collection.");
						throw uex;
					}

					// Get the score from the map
					lTempScore = score->second;
				}

				// Add this value to the score
				lScore += lTempScore;
			}
		}

		// Return the pair containing the computed score and whether this was a match or not
		return pair<long, bool>(lScore, bMatched);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24745");
}
//-------------------------------------------------------------------------------------------------
bool CAutomatedRuleSetTester::compareAttributes(IIUnknownVectorPtr ipFoundAttributes, 
												IIUnknownVectorPtr ipExpectedAttributes)
{

	bool bReturn = false;

	// Do the comparison
	if ((ipFoundAttributes != __nullptr) && (ipExpectedAttributes != __nullptr))
	{
		// Compare and count all expected and found attributes
		bReturn = compareResultVectors( ipFoundAttributes, ipExpectedAttributes );

		/////////////////////////////
		// Always add Test Case Memos
		/////////////////////////////

		// get the expected and found attributes, and 
		// display their contents in note
		string strExpectedAttributes = getAttributesCompareString(ipExpectedAttributes);
		string strFoundAttributes = getAttributesCompareString(ipFoundAttributes);

		//Add Compare Attributes to the tree and the testlogger window
		m_ipResultLogger->AddTestCaseCompareData("Compare Attributes",
												 "Expected Attributes",
												 strExpectedAttributes.c_str(),
												 "Found Attributes",
												 strFoundAttributes.c_str());
	}
	else
	{
		// Throw exception, Unable to compare results
		UCLIDException	ue( "ELI05729", "Unable to compare Attributes." );
		throw ue;
	}

	return bReturn;
}
//-------------------------------------------------------------------------------------------------
const string CAutomatedRuleSetTester::getAndValidateAbsolutePath(const string& strParentFile, 
																 const string& strRelativeFile)
{
	// compute absolute path and validate its existence
	string strAbsolutePath = getAbsoluteFileName(strParentFile, strRelativeFile);
	validateFileOrFolderExistence(strAbsolutePath);
	
	// return the validated absolute path
	return strAbsolutePath;
}
//-------------------------------------------------------------------------------------------------
string CAutomatedRuleSetTester::getDocumentClassificationInfo(IAFDocumentPtr ipAFDoc)
{
	string strDocClassificationInfo("");
	
	IStrToObjectMapPtr ipObjectTags(ipAFDoc->ObjectTags);
	string strDocType("");
	if (ipObjectTags != __nullptr && ipObjectTags->Size > 0)
	{
		if (ipObjectTags->Contains(get_bstr_t(DOC_TYPE.c_str())) == VARIANT_TRUE)
		{
			IVariantVectorPtr ipVecDocTypes = ipObjectTags->GetValue(get_bstr_t(DOC_TYPE.c_str()));
			if (ipVecDocTypes)
			{
				long nSize = ipVecDocTypes->Size;
				for (long n=0; n<nSize; n++)
				{
					if (n>0)
					{
						strDocType += "|";
					}
					
					string strType = asString(_bstr_t(ipVecDocTypes->GetItem(n)));
					strDocType += strType;
				}
			}
		}
	}
	
	IStrToStrMapPtr ipStringTags = ipAFDoc->StringTags;
	if (ipStringTags)
	{
		if (ipStringTags->Contains(get_bstr_t(DOC_PROBABILITY.c_str())) == VARIANT_TRUE)
		{
			// document is classified at a certain confidence level,
			// such as Sure, Probable and Maybe
			string strDocProbabilityLevel = ipStringTags->GetValue(get_bstr_t(DOC_PROBABILITY.c_str()));
			strDocProbabilityLevel = getDocumentProbabilityString(strDocProbabilityLevel);
			
			strDocClassificationInfo = "Document Classification: " + strDocProbabilityLevel;
			if (!strDocType.empty())
			{
				strDocClassificationInfo += " - " + strDocType;
			}
		}
	}

	return strDocClassificationInfo;
}
//-------------------------------------------------------------------------------------------------
string CAutomatedRuleSetTester::getDocumentProbabilityString(const string& strProbability)
{
	static const string strDescriptions[] = { "Zero", "Maybe", "Probable", "Sure" };
	long nIndex = asLong(strProbability);

	if ((nIndex < 0) || (nIndex > sizeof(strDescriptions)))
	{
		UCLIDException ue("ELI07006", "Invalid probability.");
		ue.addDebugInfo("strProbability", strProbability);
		throw ue;
	}

	return strDescriptions[nIndex];
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CAutomatedRuleSetTester::getAttributesFromFile(const string& strAttrFileName)
{

	int iPos = strAttrFileName.find('|');
	
	if(iPos == string::npos)
	{
		// if the file is not found that means there are no attributes
		if(!isFileOrFolderValid(strAttrFileName))
		{
			IIUnknownVectorPtr ipAttributes(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI09711", ipAttributes != __nullptr);
			return ipAttributes;
		}
		else
		{
			IIUnknownVectorPtr ipAttributes =
				m_ipAFUtility->GetAttributesFromFile(get_bstr_t(strAttrFileName.c_str()));
			ASSERT_RESOURCE_ALLOCATION("ELI28452", ipAttributes != __nullptr);

			// Metadata attributes should not be considered in any test; remove them from the vector.
			m_ipAFUtility->RemoveMetadataAttributes(ipAttributes);

			return ipAttributes;
		}
	}
	else
	{
		return processInlineEAVString(strAttrFileName);
	}
}
//-------------------------------------------------------------------------------------------------
string CAutomatedRuleSetTester::getRuleID(IAFDocumentPtr ipAFDoc)
{
	string strRuleID("");

	IStrToObjectMapPtr ipObjMap = ipAFDoc->ObjectTags;
	if (ipObjMap != __nullptr && ipObjMap->Size > 0)
	{
		// before put any attributes in the grid, add record(s) to grid
		// to display the which rule is actually used to capture the data if any
		if (ipObjMap->Contains(get_bstr_t(RULE_WORKED_TAG.c_str())) == VARIANT_TRUE)
		{
			IStrToStrMapPtr ipRulesWorked = ipObjMap->GetValue(get_bstr_t(RULE_WORKED_TAG.c_str()));
			if (ipRulesWorked)
			{
				// assume there's always one rule that extracts the attributes
				CComBSTR bstrKey, bstrValue;
				ipRulesWorked->GetKeyValue(0, &bstrKey, &bstrValue);
				strRuleID = asString(bstrValue);
			}
		}
	}

	return strRuleID;
}
//-------------------------------------------------------------------------------------------------
void CAutomatedRuleSetTester::interpretLine(const string& strLineText,
											const string& strCurrentDatFileName,
											const string& strCaseNum)
{
	// *****************************************************************************
	// Note: Each line must have a tag (i.e. <FILE>, <TESTCASE> or <TESTFOLDER>)
	// to indicate the its need for different interpretation.
	// Tags:
	// <FILE>;<token1>
	//			<token1> -- Dat file name with same same structure as TestRuleSets.dat
	//
	// <TESTCASE>;<token1>;<token2>;<token3>;<token4>
	//			<token1> - RSD file to run if <token3> is empty
	//			<token2> - image or uss file to for this testcase - <SourceDocName>
	//			<token3> - expression for voa filename - this can use tags - <SourceDocName> == <token2>
	//			<token4> - expression for expected attributes filename fileneme - this can use tags - <SourceDocName> == <token2>
	//
	// <TESTFOLDER>;<token1>;<token2>;<token3>;<token4>
	//			<token1> - RSD file to run if <token3> is empty
	//			<token2> - folder that contains the images files that is searched recursively
	//			<token3> - expression for voa filename - this can use tags - <SourceDocName> == Current file in folder <token2> search
	//			<token4> - expression for expected attributes filename - this can use tags - <SourceDocName> == Current file in folder <token2> search
	//
	// <SETTING>;<SettingName>=<TRUE|FALSE>
	//			<SettingName> - Can be:
	//								CASESENSITIVE  - default = TRUE
	//								EAV_MUST_EXIST - default = FALSE
	//								OUTPUT_FINAL_STATS_ONLY - default = FALSE
	// *****************************************************************************

	// parse each line into multiple tokens with the delimiter as ";"
	vector<string> vecTokens;
	StringTokenizer::sGetTokens(strLineText, ';', vecTokens);
	int nNumOfTokens = vecTokens.size();
	if (nNumOfTokens < 2)
	{
		UCLIDException ue("ELI06239", "Invalid line of text.");
		ue.addDebugInfo("Line", strLineText);
		throw ue;
	}

	// The tag shall always be the first token
	string strTag = vecTokens[0];
	if (strTag == "<FILE>")
	{
		// in this case, only two tokens are allowed
		if (nNumOfTokens != 2)
		{
			UCLIDException ue("ELI06240", "There shall be one and only one file name follow the tag <FILE>.");
			ue.addDebugInfo("Line", strLineText);
			throw ue;
		}
		// get the name of the dat file that follows the <FILE>
		string strDatFile = getAndValidateAbsolutePath(strCurrentDatFileName, vecTokens[1]);

		// process the dat file
		processDatFile(strDatFile, strCaseNum);
	}
	else if (strTag == "<TESTCASE>")
	{
		// in this case, 5 tokens are allowed (including the <TESTCASE> tag
		if (nNumOfTokens != 5 )
		{
			UCLIDException ue("ELI06241", "Invalid line of text.");
			ue.addDebugInfo("Line", strLineText);
			throw ue;
		}
		bool bProcess = true;

		string strRSDFile;
		string strInputFile;
		string strImageFile;
		string strTextFile;
		string strEAVFile;
		string strVOAFile;

		try
		{
			// these 2 items are fully qualified paths
			strRSDFile = getAndValidateAbsolutePath(strCurrentDatFileName, vecTokens[1]);
			strInputFile = getAndValidateAbsolutePath(strCurrentDatFileName, vecTokens[2]);

			EFileType eFileType = getFileType(strInputFile);
			if (eFileType == kUSSFile || eFileType == kTXTFile)
			{
				strImageFile = getPathAndFileNameWithoutExtension(strInputFile);
				strTextFile = strInputFile;
			}
			else
			{
				strImageFile = strInputFile;
				strTextFile = "";
			}

			// These 2 items may contain tags and will be expanded below
			strVOAFile = vecTokens[3];
			strEAVFile = vecTokens[4];

			string strNote("");
		}
		catch(...)
		{
			bProcess = false;
		}

		if(bProcess)
		{
			string strEAVFileName;
			// check for inline attributes
			if(strEAVFile.find("|") == string::npos)
			{
				// Expand any tags in the EAV file expresion to get the file name
				strEAVFileName = expandTagsAndTFE(strEAVFile, strInputFile);

				// If there is no long path present, attach the current dat file's directory path
				// as a prefix to the EAV file. This allows the user to specify "Test1.eav"
				// as the expected values file.
				if(!isAbsolutePath(strEAVFileName))
				{
					strEAVFileName = getAbsoluteFileName(strCurrentDatFileName, strEAVFileName, true);
				}
			}

			// Expand tags to get the VOA file name
			string strVOAFileName = expandTagsAndTFE(strVOAFile, strInputFile);

			// If there is no long path present, attach the current dat file's directory path
			// as a prefix to the VOA file. This allows the user to specify "Test1.voa"
			// as the found values file.
			if( getDirectoryFromFullPath( strVOAFileName ) == "" )
			{
				strVOAFileName = getAbsoluteFileName(strCurrentDatFileName, strVOAFileName, false);
			}

			if ( !isValidFile(strVOAFileName))
			{
				// If voa file name is not valid set to empty string
				strVOAFileName = "";
			}
			
			// process this test case
			processTestCase(strRSDFile, strImageFile, strTextFile, strVOAFileName,
				strEAVFileName, strCurrentDatFileName, strCaseNum);
		}
	}
	else if (strTag == "<TESTFOLDER>")
	{
		// Should only be 5 tokens( includeing the <TESTFOLDER> tag
		if (nNumOfTokens != 5)
		{
			UCLIDException ue("ELI06244", "Invalid line of text.");
			ue.addDebugInfo("Line", strLineText);
			throw ue;
		}

		// Get and vaildate the rsd file name
		string strRSDFile = getAndValidateAbsolutePath(strCurrentDatFileName, vecTokens[1]);
		
		// Get and validate the input folder
		string strInputFileFolder = getAndValidateAbsolutePath(	strCurrentDatFileName, vecTokens[2]);

		// Get the expression or file name for the VOA and EAV files
		string strVOAFilesExpression = vecTokens[3];
		string strEAVFilesExpression = vecTokens[4];

		// Process the folder
		processTestFolder(strRSDFile, strInputFileFolder, strVOAFilesExpression, strEAVFilesExpression, strCurrentDatFileName, strCaseNum);
	}
	else if (strTag == "<SETTING>")
	{
		if (nNumOfTokens < 2)
		{
			UCLIDException ue("ELI09727", "Invalid line of text.");
			ue.addDebugInfo("Line", strLineText);
			throw ue;
		}
		
		for (int i = 1; i < nNumOfTokens; i++)
		{
			if (vecTokens[i] == "")
			{
				continue;
			}
			
			vector<string> vecSubTokens;

			StringTokenizer::sGetTokens(vecTokens[i], "=", vecSubTokens);

			if(vecSubTokens.size() != 2)
			{
				UCLIDException ue("ELI09728", "Invalid line of text.");
				ue.addDebugInfo("Line", strLineText);
				throw ue;
			}

			string strSetting = vecSubTokens[0];

			if (strSetting == "CASESENSITIVE")
			{
				string strValue = vecSubTokens[1];

				if (strValue == "TRUE")
				{
					m_bCaseSensitive = true;
				}
				else if (strValue == "FALSE")
				{
					m_bCaseSensitive = false;
				}
				else
				{
					UCLIDException ue("ELI09730", "Invalid Value for Setting.");
					ue.addDebugInfo("Value", strValue);
					throw ue;
				}
			}
			else if ( strSetting == "EAV_MUST_EXIST" )
			{
				string strValue = vecSubTokens[1];

				if (strValue == "TRUE")
				{
					m_bEAVMustExist = true;
				}
				else if (strValue == "FALSE")
				{
					m_bEAVMustExist = false;
				}
				else
				{
					UCLIDException ue("ELI10129", "Invalid Value for Setting.");
					ue.addDebugInfo("Value", strValue);
					throw ue;
				}
			}
			else if (strSetting == "OUTPUT_FINAL_STATS_ONLY")
			{
				string strValue = vecSubTokens[1];
				
				if (strValue == "TRUE")
				{
					m_ipResultLogger->AddEntriesToTestLogger = VARIANT_FALSE;
				}
				else if (strValue == "FALSE")
				{
					m_ipResultLogger->AddEntriesToTestLogger = VARIANT_TRUE;
				}
				else
				{
					UCLIDException ue("ELI25274",
						"Invalid Value for OUTPUT_FINAL_STATS_ONLY setting.");
					ue.addDebugInfo("Value", strValue);
					throw ue;
				}
			}
			else
			{
				UCLIDException ue("ELI09729", "Invalid Setting.");
				ue.addDebugInfo("string", strSetting);
				throw ue;
			}
		}
	}
	else
	{
		UCLIDException ue("ELI06243", "Please provide a valid tag for this line. "
			"For instance, <FILE>, <TESTCASE> or <TESTFOLDER>.");
		ue.addDebugInfo("Line", strLineText);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void CAutomatedRuleSetTester::processDatFile(const string& strDatFileName, 
											 const string& strCaseNumPrefix)
{
	// parse each line of input file into 3 part: 
	// rule set file name, input text file name, output file that has expected output result
	ifstream ifs(strDatFileName.c_str());
	
	string strLine("");
	CommentedTextFileReader fileReader(ifs, "//", true);
	long nCaseNo = 1;
	do
	{
		strLine = fileReader.getLineText();
		// skip any empty line
		if (strLine.empty()) 
		{
			continue;
		}
		
		try
		{
			try
			{
				CString zTestCaseNum("");
				CString zCaseNumFormat(strCaseNumPrefix.c_str());
				if (!zCaseNumFormat.IsEmpty())
				{
					zCaseNumFormat += "_";
				}
	
				zCaseNumFormat += "%d";
				zTestCaseNum.Format(zCaseNumFormat, nCaseNo);
	
				interpretLine(strLine, strDatFileName, (LPCSTR)zTestCaseNum);
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI06126");
		}
		catch(UCLIDException& ue)
		{
			m_ipResultLogger->AddComponentTestException(get_bstr_t(ue.asStringizedByteStream()));
		}
		nCaseNo++;
	}
	while(!ifs.eof());
}
//-------------------------------------------------------------------------------------------------
void CAutomatedRuleSetTester::processTestCase(const string& strRSDFile, 
											  const string& strImageFile,
											  const string& strTextFile,
											  const string& strVOAFile,
											  const string& strEAVFile,
											  const string& strTestCaseTitle,
											  const string& strTestCaseNo)
{
	bool bSuccess = false;
	bool bExceptionCaught = false;

	// Define strNoteFile to use for displaying Contents of NTE file
	string strNoteFile;

	// Try catch block to handle exceptions in finding the attributes
	try
	{
		try
		{	
			// Initiate a test case
			m_ipResultLogger->StartTestCase(get_bstr_t(strTestCaseNo.c_str()),
				get_bstr_t(strTestCaseTitle.c_str()), kAutomatedTestCase); 	

			// Add note for RSD file plus input filename
			m_ipResultLogger->AddTestCaseFile(get_bstr_t(strRSDFile.c_str()));

			string strInputFile;
			string strBaseFileName;

			if (!strTextFile.empty())
			{
				// If a text or uss file is provided, add it as a test case file and use it to
				// derive strBaseFileName
				m_ipResultLogger->AddTestCaseFile(get_bstr_t(strTextFile.c_str()));
				strBaseFileName = getFileNameWithoutExtension(strTextFile);

				// The input file to use for FindAttributes is the text file.
				strInputFile = strTextFile;
			}
			else
			{
				// If a text file was not provided, ensure a valid image file was.
				if ( strImageFile.empty())
				{
					THROW_LOGIC_ERROR_EXCEPTION("ELI28492");
				}

				// Use the image file sans the full path as strBaseFileName
				strBaseFileName = getFileNameFromFullPath(strImageFile);

				// The input file to use for FindAttributes is the image file.
				strInputFile = strImageFile;
			}

			// Add the image name as a test case file if it is specified and available.
			if (!strImageFile.empty() && isValidFile(strImageFile))
			{
				m_ipResultLogger->AddTestCaseFile(get_bstr_t(strImageFile.c_str()));
			}

			// Add note for EAV File
			m_ipResultLogger->AddTestCaseFile(get_bstr_t(strEAVFile.c_str()));

			// Add note for .nte file
			string strEAVFileDir = ::getDirectoryFromFullPath( strEAVFile.c_str()) + "\\";
			strNoteFile = strEAVFileDir + strBaseFileName + ".nte";
			m_ipResultLogger->AddTestCaseFile(get_bstr_t(strNoteFile.c_str()));

			// make sure current Attributes Vector is empty
			m_ipCurrentAttributes->Clear();

			// The voa filename is empty run the rules
			if ( strVOAFile.empty() )
			{
				// Make up a IAFDocument
				IAFDocumentPtr ipAFDoc(CLSID_AFDocument);
				ASSERT_RESOURCE_ALLOCATION("ELI07440", ipAFDoc != __nullptr);

				// find all attributes in the text file
				m_ipCurrentAttributes = m_ipAttrFinderEngine->FindAttributes( ipAFDoc, 
					get_bstr_t(strInputFile.c_str()), -1, get_bstr_t(strRSDFile.c_str()), 
					NULL, VARIANT_FALSE, NULL);

				// add document classification information
				string strDocType = getDocumentClassificationInfo(ipAFDoc);
				if (!strDocType.empty())
				{
					// add a note for document classification information
					m_ipResultLogger->AddTestCaseNote(get_bstr_t(strDocType.c_str()));
				}

				// add rule id that worked
				string strRuleID = getRuleID(ipAFDoc);
				if (!strRuleID.empty())
				{
					strRuleID = "Rule that captures the attributes : " + strRuleID;
					m_ipResultLogger->AddTestCaseNote(get_bstr_t(strRuleID.c_str()));
				}
			}
			else
			{
				// Add note for VOA File
				m_ipResultLogger->AddTestCaseFile(get_bstr_t(strVOAFile.c_str()));

				// VOA file exists so load the expected attributes from the voa file
				m_ipCurrentAttributes = getAttributesFromFile(strVOAFile);
			}

		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI06119");
	}
	catch(UCLIDException& uex)
	{
		bExceptionCaught = true;
		m_ipResultLogger->AddTestCaseException(_bstr_t(uex.asStringizedByteStream().c_str()), VARIANT_FALSE);
	}

	// Try catch block to catch exceptions if calculating the results
	try
	{
		try
		{
			if ( m_ipCurrentAttributes == __nullptr )
			{
				m_ipCurrentAttributes.CreateInstance( CLSID_IUnknownVector );
				ASSERT_RESOURCE_ALLOCATION("ELI09207", m_ipCurrentAttributes != __nullptr );
			}

			// get expected attributes from the file
			IIUnknownVectorPtr ipExpectedAttributes = getAttributesFromFile(strEAVFile);
			ASSERT_RESOURCE_ALLOCATION("ELI25295", ipExpectedAttributes != __nullptr);

			if(!m_bCaseSensitive)
			{
				makeAttributesUpperCase(m_ipCurrentAttributes);
				makeAttributesUpperCase(ipExpectedAttributes);
			}

			// Get the size of the expected attributes vector
			long lExpectedSize = ipExpectedAttributes->Size();

			// Count all the expected attributes
			if (lExpectedSize > 0)
			{
				countExpectedAttributes(ipExpectedAttributes);
			}

			// Check the case where no attributes were found but there were attributes expected
			if ((m_ipCurrentAttributes->Size() == 0) && (lExpectedSize > 0))
			{
				// Create string containing Expected Attributes
				string strExpectedAttributes = getAttributesCompareString(ipExpectedAttributes);

				// Add Test Case Memo for Expected Attributes
				m_ipResultLogger->AddTestCaseCompareData( "Compare Attributes",
					get_bstr_t( "Expected Attributes" ),
					get_bstr_t(strExpectedAttributes),
					"No Found Attributes", " ");

				// Create and throw exception
				UCLIDException uclidException("ELI04767", "No Found Attributes.");
				throw uclidException;
			}

			// Compare actual found attributes with expected attributes
			if (compareAttributes( m_ipCurrentAttributes, ipExpectedAttributes ))
			{
				// Comparison succeeded
				bSuccess = true;
			}

		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI09208");
	}
	catch(UCLIDException& uex)
	{
		bExceptionCaught = true;
		m_ipResultLogger->AddTestCaseException(_bstr_t(uex.asStringizedByteStream().c_str()), VARIANT_FALSE);
	}

	//Display contents of existing NTE file
	string strNote("");
	if (isValidFile(strNoteFile))
	{
		strNote = ::getTextFileContentsAsString(strNoteFile);
	}
	if ( !strNote.empty())
	{
		string strTitle = strNote;
		if ( strNote.size() > 120 )
		{
			strTitle.erase(120);
			strTitle += "...";
		}
		// Add Test Case Detail Note with Provided Information
		m_ipResultLogger->AddTestCaseDetailNote( 
			get_bstr_t(strTitle.c_str()), 
			get_bstr_t(strNote.c_str()) );
	}

	// Get test result
	bSuccess = bSuccess && !bExceptionCaught;

	// end the test case
	m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
}
//-------------------------------------------------------------------------------------------------
void CAutomatedRuleSetTester::processTestFolder(const string& strRSDFile,
												const string& strTestFolder,
												const string& strVOAFilesExpression,
												const string& strEAVFilesExpression,
												const string& strDatFileName,
												const string& strTestCaseNo)
{
	// Get all .txt and .uss files from the folder
	vector<string> vecSourceFiles;
	::getFilesInDir(vecSourceFiles, strTestFolder, "*.txt", true);
	::getFilesInDir(vecSourceFiles, strTestFolder, "*.uss", true);

	// Compile a map of unique image file names (given the files in vecSourceFiles) to their
	// associated source file.
	map<string, string> mapImageFileToSourceFile;
	for each (string strFileName in vecSourceFiles)
	{
		// If source document is a text or uss file, the image file name is the source doc name
		// minus the extension.
		string strImageFileName = getPathAndFileNameWithoutExtension(strFileName);
		mapImageFileToSourceFile[strImageFileName] = strFileName;
	}

	// get all .tif files from the folder
	vecSourceFiles.clear();
	::getFilesInDir(vecSourceFiles, strTestFolder, "*.tif", true);

	// Update the map of unique image file names to their associated source file with any images
	// that don't have an associated txt or uss file.
	for each (string strFileName in vecSourceFiles)
	{
		// The source doc is an image file.  If a uss or txt file has already been associated
		// with the image file there is nothing to do. Otherwire 
		if (mapImageFileToSourceFile.find(strFileName) == mapImageFileToSourceFile.end())
		{
			mapImageFileToSourceFile[strFileName] = strFileName;
		}
	}

	int nTestCastNum = 1;
	for each (pair<string, string> pairImageFileToSourceFile in mapImageFileToSourceFile)
	{
		// Get the input image and source file name.
		string strImageFileName = pairImageFileToSourceFile.first;
		string strSourceFileName = pairImageFileToSourceFile.second;

		// If the source filename differs from the image filename, it is the text filename;
		// otherwise there is no text filename.
		string strTextFileName = (strImageFileName != strSourceFileName) ? strSourceFileName : "";

		// Expand the eav file name
		string strEAVFileName = expandTagsAndTFE( strEAVFilesExpression, strSourceFileName);

		// Add the path relative to the dat file if necessary
		strEAVFileName = getAbsoluteFileName( strDatFileName, strEAVFileName );

		// Expand the voa file name
		string strVOAFileName = expandTagsAndTFE(strVOAFilesExpression, strSourceFileName);

		// if not empty add the path relative to the dat file
		if ( !strVOAFileName.empty() )
		{
			strVOAFileName = getAbsoluteFileName( strDatFileName, strVOAFileName );
		}

		// if the voa file is not a valid file set it to an empty string
		if ( !isValidFile(strVOAFileName) )
		{
			strVOAFileName = "";
		}

		// if this eav file exists, then process the test
		if (!m_bEAVMustExist || isValidFile(strEAVFileName))
		{
			CString zCaseNo("");
			zCaseNo.Format("%s_%d", strTestCaseNo.c_str(), nTestCastNum++);
			processTestCase(strRSDFile, strImageFileName, strTextFileName, strVOAFileName,
				strEAVFileName, strDatFileName, (LPCTSTR)zCaseNo);
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CAutomatedRuleSetTester::validateLicense()
{
	VALIDATE_LICENSE(gnFLEXINDEX_RULE_WRITING_OBJECTS, "ELI07287", "Automated Rule Set Tester" );
}
//-------------------------------------------------------------------------------------------------
const string CAutomatedRuleSetTester::getMasterTestFileName(IVariantVectorPtr ipParams, const string &strTCLFile) const
{
	// if pParams is not empty and the second item is specified,
	// then the second item is the master dat file
	if ((ipParams != __nullptr) && (ipParams->Size > 1))
	{
		// get the DAT filename
		string strMasterDatFileName = ::getAbsoluteFileName(strTCLFile, asString(_bstr_t(ipParams->GetItem(1))), true);

		// if no master file specified, throw an exception
		if(strMasterDatFileName.empty() || (getFileNameFromFullPath(strMasterDatFileName) == ""))
		{
			// Create and throw exception
			UCLIDException ue("ELI19400", "Required master testing .DAT file not found.");
			throw ue;
		}

		return strMasterDatFileName;
	}
	else
	{
		// Create and throw exception
		UCLIDException ue("ELI19401", "Required master testing .DAT file not found.");
		throw ue;	
	}
}
//-------------------------------------------------------------------------------------------------
bool CAutomatedRuleSetTester::compareResultVectors(IIUnknownVectorPtr ipFound,
												   IIUnknownVectorPtr ipExpected,
												   const string& strQualifiedAttrName)
{
	try
	{
		// setup the separator for building Qualified names the first call this function should have 
		// an empty string for the the strTotalNamePrefix and there should not be a separator
		string strSeparator = "";
		if ( strQualifiedAttrName.length() > 0 )
		{
			strSeparator = ".";
		}
		// if both found and expected are empty return true
		// bMatched is true if the found is exactly equal to expected
		bool bMatched = true;
		if (( ipFound == __nullptr || ipFound->Size() == 0 ) && 
			(ipExpected == __nullptr || ipExpected->Size() == 0 ))
		{
			//return a matched value
			return bMatched;
		}
		else if ( ipFound == __nullptr || ipFound->Size() == 0 )
		{
			// Expected attributes are counted at the beginning of the match, nothing to do here
			// just set bMatched to false
			bMatched = false;
		}
		else if ( ipExpected == __nullptr || ipExpected->Size() == 0 )
		{
			// Nothing expected but something found and need to add to count
			bMatched = false;
			long lSize = ipFound->Size();
			for (int i = 0; i < lSize; i++)
			{
				IAttributePtr ipFoundAttribute = ipFound->At( i );
				ASSERT_RESOURCE_ALLOCATION("ELI24713", ipFoundAttribute != __nullptr);

				// Get the qualified name
				string strNewQualifiedName = getQualifiedName(ipFoundAttribute, strQualifiedAttrName,
					strSeparator);

				// Mark this as incorrectly found
				m_mapTotalIncorrectlyFound[ strNewQualifiedName ]++;

				// Count all of the found subattributes recursively
				compareResultVectors( ipFoundAttribute->SubAttributes, NULL, strNewQualifiedName);
			}
		}
		else
		{
			// Get the sizes of each vector
			long lExpectedSize = ipExpected->Size();
			long lFoundSize = ipFound->Size();

			// Build a vector of pairs for each attribute.  The pair will contain
			// the found attribute and a bool to indicate whether it was used
			// to perform a comparison or not.  If it has not been used in a comparison
			// then it will be marked as incorrectly found.  The computation of the
			// incorrectly found is performed after all "Best Matches" of expected vs.
			// found attributes have been performed. When a found attribute is computed
			// to be the best match and is used in a comparison then it should have
			// its boolean flag set to true to indicate that it was used in a comparison
			vector<pair<IAttributePtr, bool>> vecFoundAttributes;
			for (long i=0; i < lFoundSize; i++)
			{
				// Get the found attribute
				IAttributePtr ipFoundAttribute = ipFound->At(i);
				ASSERT_RESOURCE_ALLOCATION("ELI24697", ipFoundAttribute != __nullptr);

				vecFoundAttributes.push_back(pair<IAttributePtr, bool>(ipFoundAttribute, false));
			}

			// Build a vector of pairs for each expected attribute.  The pair will contain
			// the expected attribute and a bool to indicate whether it was used
			// to perform a comparison or not. If it has not been used in a comparison then
			// bMatched will be set to false.
			vector<pair<IAttributePtr, bool>> vecExpectedAttributes;
			for (long i=0; i < lExpectedSize; i++)
			{
				IAttributePtr ipExpectedAttribute = ipExpected->At(i);
				ASSERT_RESOURCE_ALLOCATION("ELI24961", ipExpectedAttribute != __nullptr);

				vecExpectedAttributes.push_back(
					pair<IAttributePtr, bool>(ipExpectedAttribute, false));
			}

			// Compute match score for all expected vs. found attributes
			// and store the data in a vector (the attribute score data structure
			// holds the score, the expected attribute index, the found attribute index,
			// and a flag to indicate whether it is valid or not (valid indicates that
			// it was an actual match and that both the expected and found indices are
			// available to be used - an index is no longer usable if the attribute that
			// the index refers to has been used in a comparison, to change an attribute
			// score item from valid to invalid use the markAsInvalid function and pass
			// in the expected and found index to make invalid, you should also call
			// sortAttributeScoreData so that the invalid items are moved to the bottom
			// of the vector.
			// TODO: There is an effeciency that can be gained in the score vector
			// in the markAsInvalid function.  Instead of just marking something as
			// invalid, the invalid items could just be removed from the vector.  This
			// would cause the vector to shrink and would reduce the time needed to
			// sort the vector.
			vector<AttributeScoreData> vecAttributeScores;
			for (long i=0; i < lExpectedSize; i++)
			{
				// Get the current expected attribute
				IAttributePtr ipExpectedAttribute = vecExpectedAttributes[i].first;
				ASSERT_RESOURCE_ALLOCATION("ELI24696", ipExpectedAttribute != __nullptr);

				// Compute the scores for each found attribute vs the current expected
				// attribute.  For each score, fill in an AttributeScoreData struct
				// and add this value to the attribute scores vector.
				for (long j=0; j < lFoundSize; j++)
				{
					// Compute the score for this attribute pair
					pair<long, bool> prTemp = computeScore(ipExpectedAttribute,
						vecFoundAttributes[j].first);

					// Build an attribute data structure
					AttributeScoreData asdTemp;
					asdTemp.m_lScore = prTemp.first;
					asdTemp.m_bValid = prTemp.second;
					asdTemp.m_lExpectedIndex = i;
					asdTemp.m_lFoundIndex = j;

					// Put the attribute score data in the vector
					vecAttributeScores.push_back(asdTemp);
				}
			}
			// Sort the vector
			sortAttributeScoreData(vecAttributeScores);

			// Loop while the first item in the vector is still valid.
			// The first item in the vector should be the best match out of all expected vs. found
			// match comparisons.  We use this item to perform the comparison, then
			// need to call markAsInvalid on the score vector followed by a call to sort
			// the score vector, then get the top item from the collection again and repeat
			// the comparison
			vector<AttributeScoreData>::iterator it = vecAttributeScores.begin();
			while(it->m_bValid)
			{
				// Get the attributes from the expected and found vectors
				IAttributePtr ipExpectedAttribute =
					vecExpectedAttributes[it->m_lExpectedIndex].first;
				ASSERT_RESOURCE_ALLOCATION("ELI24698", ipExpectedAttribute != __nullptr);
				IAttributePtr ipFoundAttribute = vecFoundAttributes[it->m_lFoundIndex].first;
				ASSERT_RESOURCE_ALLOCATION("ELI24699", ipFoundAttribute != __nullptr);

				// Mark these attributes as used
				vecFoundAttributes[it->m_lFoundIndex].second = true;
				vecExpectedAttributes[it->m_lExpectedIndex].second = true;

				// Build the qualified name
				string strNewQualifiedName = getQualifiedName(ipExpectedAttribute,
					strQualifiedAttrName, strSeparator);

				// Since there was a best match, mark this attribute as correctly found
				m_mapTotalCorrectFound[ strNewQualifiedName ]++;

				// Compare all the sub attributes
				if (!compareResultVectors(ipFoundAttribute->SubAttributes,
					ipExpectedAttribute->SubAttributes, strNewQualifiedName))
				{
					// Not a perfect match, set matched to false
					bMatched = false;
				}

				// Invalidate all attribute score entries that have either the expected or found
				// index (this will move them to the bottom of the list when they are sorted)
				markAsInvalid(vecAttributeScores, it->m_lExpectedIndex, it->m_lFoundIndex);

				// Now sort the vector (this will invalidate the iterator)
				sortAttributeScoreData(vecAttributeScores);

				// Now get the top element from the vector (this should be the next best match)
				it = vecAttributeScores.begin();
			}

			// Now iterate the found attribute vector and mark each
			// not used item as incorrectly found
			for (size_t i = 0; i < vecFoundAttributes.size(); i++)
			{
				// Check if it was not used
				if (!vecFoundAttributes[i].second)
				{
					// Set matched to false
					bMatched = false;

					// Get the attribute
					IAttributePtr ipFoundAttribute = vecFoundAttributes[i].first;
					ASSERT_RESOURCE_ALLOCATION("ELI24700", ipFoundAttribute != __nullptr);

					// Get the qualified name
					string strNewQualifiedName = getQualifiedName(ipFoundAttribute,
						strQualifiedAttrName, strSeparator);

					// Mark this as incorrectly found
					m_mapTotalIncorrectlyFound[ strNewQualifiedName ]++;

					// Call compareResultVectors recursively to make sure all cases are counted
					compareResultVectors( ipFoundAttribute->SubAttributes, NULL,
						strNewQualifiedName);
				} 
			}

			// Now iterate through the expected attributes vector and if any
			// item was not marked as used set bMatched to false
			for (vector<pair<IAttributePtr, bool>>::iterator it = vecExpectedAttributes.begin();
				it != vecExpectedAttributes.end(); it++)
			{
				// Check used flag
				if (!it->second)
				{
					// Mark bMatched as false and break from loop since
					// if at least one expected attribute is not matched the
					// vectors of attributes do not match
					bMatched = false;
					break;
				}
			}
		}

		return bMatched;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24747");
}
//-------------------------------------------------------------------------------------------------
void CAutomatedRuleSetTester::addAttributeResultCase()
{
	// start the summary test case
	// get current date/time
	__time64_t currTime;
	tm pTime;
	time( &currTime );
	int iError = _localtime64_s( &pTime, &currTime );

	// Convert time to string
	char szTime[32];
	iError = asctime_s( szTime, sizeof(szTime), &pTime );
	string strTimeNote = trim( szTime, "", "\r\n" );
	m_ipResultLogger->StartTestCase(strTimeNote.c_str(), "Summary of Attribute Statistics:", kSummaryTestCase);

	map< string, long >::iterator iterCurr = m_mapTotalExpected.begin();
	bool bErrorFree = true;

	// show results for the expected attributes
	while(  iterCurr != m_mapTotalExpected.end())
	{
		string strAttrName = (*iterCurr).first;
		CString zTemp;
		long lApproxWrongFound = m_mapTotalExpected[ strAttrName ] - m_mapTotalCorrectFound[ strAttrName ];
		long lApproxExtras = 0;
		if ( m_mapTotalIncorrectlyFound[ strAttrName ] > lApproxWrongFound )
		{
			// This is an error condition, set the error free flag to false
			bErrorFree = false;
			lApproxExtras = m_mapTotalIncorrectlyFound[ strAttrName ] - lApproxWrongFound;
		}
		zTemp.Format("Total Expected: %ld\r\nTotal Correct: %ld (%0.2f%%)\r\nTotal Incorrectly Found: %ld\r\nApproximate Extras: %ld (%0.2f%%)", 
			m_mapTotalExpected[ strAttrName ],
			m_mapTotalCorrectFound[ strAttrName ],
			m_mapTotalCorrectFound[ strAttrName ] * 100 / (double) m_mapTotalExpected[ strAttrName ],
			m_mapTotalIncorrectlyFound[ strAttrName ],
			lApproxExtras,
			lApproxExtras * 100 / (double)( m_mapTotalExpected[ strAttrName] + lApproxExtras ));

		if (m_mapTotalExpected[strAttrName] != m_mapTotalCorrectFound[strAttrName])
		{
			bErrorFree = false;
		}

		m_ipResultLogger->AddTestCaseDetailNote(strAttrName.c_str(), (LPCTSTR) zTemp);
		m_mapTotalIncorrectlyFound.erase( strAttrName );
		iterCurr++;
	}

	// show results for all the found values that were not expected
	if ( m_mapTotalIncorrectlyFound.size() > 0 )
	{
		bErrorFree = false;

		iterCurr = m_mapTotalIncorrectlyFound.begin();
		while ( iterCurr != m_mapTotalIncorrectlyFound.end())
		{
			string strAttrName = (*iterCurr).first;
			CString zTemp;
			zTemp.Format("Total Expected: 0\r\nTotal Correct: 0 (0.00%%)\r\nTotal Incorrectly Found: %ld\r\nApproximate Extras: %ld (100.00%%)", 
				m_mapTotalIncorrectlyFound[ strAttrName ],
				m_mapTotalIncorrectlyFound[ strAttrName ]);

			m_ipResultLogger->AddTestCaseDetailNote(strAttrName.c_str(), (LPCTSTR) zTemp);
			iterCurr++;
		}
	}

	// end the summary test case
	m_ipResultLogger->EndTestCase(bErrorFree ? VARIANT_TRUE : VARIANT_FALSE);
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CAutomatedRuleSetTester::processInlineEAVString(const string &strInlineEAV)
{
	vector<string> vecTokens;
	string strEAV(strInlineEAV);
	int iNewLinePos = strEAV.find("\\n");
		
	// Replace all "\n" in the string with an actual carriage return and new line
	while(iNewLinePos != string::npos)
	{
		strEAV.replace(iNewLinePos, 2, "\r\n");
		iNewLinePos = strEAV.find("\\n");
	}

	return m_ipAFUtility->GetAttributesFromDelimitedString(get_bstr_t(strEAV.c_str()), get_bstr_t("*"));
}
//-------------------------------------------------------------------------------------------------
const string CAutomatedRuleSetTester::expandTagsAndTFE(const string &strInput, string &strSourceDocName)
{
	string strExpanded = m_ipFAMTagManager->ExpandTags( strInput.c_str(), strSourceDocName.c_str());

	// Expand the functions
	TextFunctionExpander tfe;
	strExpanded = tfe.expandFunctions(strExpanded); 
	
	return strExpanded;
}
//-------------------------------------------------------------------------------------------------
void CAutomatedRuleSetTester::countExpectedAttributes(IIUnknownVectorPtr ipExpected,
													  const string &strQualifiedAttrName)
{
	try
	{
		ASSERT_ARGUMENT("ELI24718", ipExpected != __nullptr);

		// setup the separator for building Qualified names the first call this function should have 
		// an empty string for the strTotalNamePrefix and there should not be a separator
		string strSeparator = "";
		if ( strQualifiedAttrName.size() > 0 )
		{
			strSeparator = ".";
		}

		long lSize = ipExpected->Size();
		for (long i = 0; i < lSize; i++)
		{
			IAttributePtr ipExpectedAttribute = ipExpected->At( i );
			ASSERT_RESOURCE_ALLOCATION("ELI24719", ipExpectedAttribute != __nullptr);

			string strNewQualifiedName = getQualifiedName(ipExpectedAttribute,
				strQualifiedAttrName, strSeparator);
			m_mapTotalExpected[ strNewQualifiedName ]++;

			// Count all the Expected subattributes recursively
			countExpectedAttributes(ipExpectedAttribute->SubAttributes, strNewQualifiedName);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24748");
}
//-------------------------------------------------------------------------------------------------