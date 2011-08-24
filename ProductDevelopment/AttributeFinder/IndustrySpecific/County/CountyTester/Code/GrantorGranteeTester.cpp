// GrantorGranteeTester.cpp : Implementation of CGrantorGranteeTester

#include "stdafx.h"
#include "CountyTester.h"
#include "GrantorGranteeTester.h"

#include <SpecialStringDefinitions.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <CommentedTextFileReader.h>
#include <StringTokenizer.h>
#include <LicenseMgmt.h>
#include <misc.h>
#include <ComUtils.h>
#include <common.h>
#include <ComponentLicenseIDs.h>

#include <cstdio>
#include <ctime>

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
const string IMAGE_FILE_DIR_TAG = "<ImageFileDir>";

//-------------------------------------------------------------------------------------------------
// CGrantorGranteeTester
//-------------------------------------------------------------------------------------------------
CGrantorGranteeTester::CGrantorGranteeTester()
: m_ipResultLogger(NULL),
  m_ipAttrFinderEngine(CLSID_AttributeFinderEngine),
  m_ipAFUtility(CLSID_AFUtility),
  m_nTotalTestAttributes(0),
  m_nNumOfPassedAttributes(0),
  m_nNumOfOtherAttributes(0)
{
	try
	{
		ASSERT_RESOURCE_ALLOCATION("ELI06335", m_ipAttrFinderEngine != __nullptr);
		ASSERT_RESOURCE_ALLOCATION("ELI07368", m_ipAFUtility != __nullptr);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06337")
}
//-------------------------------------------------------------------------------------------------
CGrantorGranteeTester::~CGrantorGranteeTester()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16436");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CGrantorGranteeTester::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ITestableComponent,
		&IID_IOutputHandler,
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
STDMETHODIMP CGrantorGranteeTester::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI11932", pParams != __nullptr);

			IVariantVectorPtr ipParams(pParams);
			// we never expect the params vector to be empty
			if (ipParams->Size == 0)
			{
				THROW_LOGIC_ERROR_EXCEPTION("ELI11933")
			}

			// Check license
			validateLicense();

			if (m_ipResultLogger == __nullptr)
			{
				throw UCLIDException("ELI06313", "Please set ResultLogger before proceeding.");
			}

			string strTestRuleSetsFile("");
			string strTesterResultsFolder = 
				getDirectoryFromFullPath(asString(_bstr_t(ipParams->GetItem(0)))) + "\\";

			// if pParams is not empty and the second item is specified,
			// then the second item is the master dat file
			if (ipParams->Size > 1)
			{
				string strMasterDatFileName = asString(_bstr_t(ipParams->GetItem(1)));
				if (!strMasterDatFileName.empty())
				{
					strTestRuleSetsFile = ::getAbsoluteFileName( asString( strTCLFile ), 
						strMasterDatFileName, true );
				}
			}
			else
			{
				UCLIDException ue("ELI11931", "Required master testing .DAT file missing.");
				throw ue;		
			}			

			// Clear <READ_FROM_FILE_TAG> processed files
			m_setProcessedFiles.clear();

			// reset numbers
			m_strFileSucceedWithNote = "";
			m_nTotalTestAttributes = 0;
			m_nNumOfPassedAttributes = 0;
			m_nNumOfOtherAttributes = 0;

			m_mapRuleCaptureInfo.clear();
			
			// process the dat file
			processDatFile(strTestRuleSetsFile);
			
			// Get current time
			__time64_t curTime;
			time( &curTime );
			tm pTime;
			if (_localtime64_s( &pTime, &curTime ) != 0)
			{
				throw UCLIDException ("ELI12926", "Unable to retrieve local time.");
			}

			// Convert time to string and add to note text
			char szTime[32];
			if (asctime_s( szTime, sizeof(szTime), &pTime ) != 0)
			{
				throw UCLIDException ("ELI12927", "Unable to convert local time to string.");
			}
			string strTimeNote = string( " " ) + trim( szTime, "", "\r\n" );
			strTimeNote += " ";

			CString strTimeStamp;

			//create a time stamp to be added to the file's name
			strTimeStamp.Format("(%2d-%2d-%4d)-%2d%02d%2d", 
							pTime.tm_mon + 1, 
							pTime.tm_mday, 
							pTime.tm_year + 1900, 
							pTime.tm_hour, 
							pTime.tm_min, 
							pTime.tm_sec);
			//replace the spaces with 0s
			strTimeStamp.Replace(' ', '0');

			m_strPartialMatchFile = string("PartialMatches - ") + (LPCSTR)strTimeStamp + ".dat";
			string strPartialMatchPathAndFile(strTesterResultsFolder + m_strPartialMatchFile);
			
			//open PartialMatches.dat output file	
			m_PartialMatchFile.open(strPartialMatchPathAndFile.c_str(), ofstream::out | ofstream::app);
			if (!m_PartialMatchFile.is_open())
			{
				UCLIDException ue("ELI09022", "Unable to open file for writing.");
				ue.addDebugInfo("filename", m_strPartialMatchFile);
				throw ue;
			}

			m_strFullMatchFile = string("FullMatches - ") + (LPCSTR)strTimeStamp + ".dat";
			string strFullMatchPathAndFile(strTesterResultsFolder + m_strFullMatchFile);

			//open FullMatches.dat output file		
			m_FullMatchFile.open(strFullMatchPathAndFile.c_str(), ofstream::out | ofstream::app);
			if (!m_FullMatchFile.is_open())
			{
				UCLIDException ue("ELI09023", "Unable to open file for writing.");
				ue.addDebugInfo("filename", m_strFullMatchFile);
				throw ue;
			}

			////////////////////////////
			// Print the summary results
			////////////////////////////
			m_ipResultLogger->StartTestCase(_bstr_t(strTimeNote.c_str()), _bstr_t("Summary of attribute-level statistics"), kSummaryTestCase); 
			CString zTemp;

			// Test attributes - executed, passed, failed, percentage
			zTemp.Format("Total test attributes executed:  %d", m_nTotalTestAttributes);
			m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));

			if (m_nTotalTestAttributes != 0)
			{
				// add note with # passed information
				zTemp.Format("Total test attributes passed:  %d (%0.2f%%)", 
					m_nNumOfPassedAttributes, 
					m_nNumOfPassedAttributes * 100.0 / (double) m_nTotalTestAttributes);
				m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));

				// add note with # failed information
				unsigned long ulNumFailed = m_nTotalTestAttributes - m_nNumOfPassedAttributes;
				zTemp.Format("Total test attributes failed:  %d (%0.2f%%)", 
					ulNumFailed, 
					ulNumFailed * 100.0 / (double) m_nTotalTestAttributes);
				m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
			}

			// add note for # of extra attributes found
			zTemp.Format("Total extra or unmatched attributes found:  %d", m_nNumOfOtherAttributes);
			m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
			
			// if there's any final note
			if (!m_strFileSucceedWithNote.empty())
			{
				m_ipResultLogger->AddTestCaseMemo(
					_bstr_t("Files that are passed but still have associated .nte files:"),
					_bstr_t(m_strFileSucceedWithNote.c_str()));
			}

			if (!m_strFileFailedWithoutNote.empty())
			{
				m_ipResultLogger->AddTestCaseMemo(
					_bstr_t("Files that are failed and do not have any associated .nte files or notes:"),
					_bstr_t(m_strFileFailedWithoutNote.c_str()));
			}

			// set this result as failed to have the item expanded.
			bool bSuccess = m_nTotalTestAttributes == m_nNumOfPassedAttributes;

			// loop through the rule capture map and create output notes for each rule
			for (map<string, RuleCaptureData >::iterator iter = m_mapRuleCaptureInfo.begin(); iter != m_mapRuleCaptureInfo.end(); iter++)
			{
				string strOutput;
				int fullMatchCount = 0;

				vector<TestCaseData> *vecTestCases = &iter->second.m_vecTestCasesInfo;
				
				// loop through the matching test cases for a rule and add them to the output string
				for (vector<TestCaseData>::iterator testCaseIter = vecTestCases->begin(); testCaseIter != vecTestCases->end(); testCaseIter++)
				{
					string strMatchResult;

					// create a different prefix for each output string based on its match result
					switch((*testCaseIter).m_eMatchResult)
					{
						case kFullMatch:
						{
							strMatchResult = "Full Match:     ";
							fullMatchCount++;
							break;
						}
						
						case kPartialMatch:
						{
							strMatchResult = "Partial Match:  ";

							if(iter->first == "NO_RULE")
							{
								strMatchResult = strMatchResult + "(" + testCaseIter->m_strRuleID + ") - ";
							}
							break;
						}
						
						case kNoMatch:
						{
							strMatchResult = "No Match:       ";
							break;
						}

						default:
						{
							break;
						}
					}

					strOutput += (strMatchResult + (*testCaseIter).m_strUSSFilename + "\r\n");
				}

				// format the output string, if "NO_RULE" is rule matched display only the
				// relevant information
				if (iter->first == "NO_RULE")
				{
					zTemp.Format("Rule ID:  %s\r\nTotal Unmatched Files: %ld\r\nUnmatched Files:\r\n", 
								iter->first.c_str(), 
								m_mapRuleCaptureInfo[iter->first.c_str()].m_vecTestCasesInfo.size()); 
				}
				else
				{
					zTemp.Format("Rule ID:  %s\r\nTotal Document Matches: %ld\r\n-- Full Matches:      %ld\r\n-- Partial Matches:  "
								 "%ld\r\n\r\nTotal Attributes:  %ld\r\n-- Passed Attributes:    %ld\r\n-- Duplicate "
								 "Attributes: %ld\r\n-- Failed Attributes:       %ld\r\n\r\nMatching Files:\r\n", 
								iter->first.c_str(), m_mapRuleCaptureInfo[iter->first.c_str()].m_vecTestCasesInfo.size(), 
								fullMatchCount, 
								m_mapRuleCaptureInfo[iter->first.c_str()].m_vecTestCasesInfo.size() - fullMatchCount,
								m_mapRuleCaptureInfo[iter->first.c_str()].m_lNumOfPassedAttributes + m_mapRuleCaptureInfo[iter->first.c_str()].m_lNumOfFailedAttributes,
								m_mapRuleCaptureInfo[iter->first.c_str()].m_lNumOfPassedAttributes,
								m_mapRuleCaptureInfo[iter->first.c_str()].m_lNumOfDuplicateAttributes,
								m_mapRuleCaptureInfo[iter->first.c_str()].m_lNumOfFailedAttributes);
				}
				
				strOutput = (LPCTSTR)zTemp + strOutput;
				
				string strRuleID("Rule - " + iter->first);

				m_ipResultLogger->AddTestCaseDetailNote(strRuleID.c_str(), strOutput.c_str());
			}

			// output the test cases to the corresponding files
 			dumpRuleCaptureInfo(m_PartialMatchFile, m_FullMatchFile);

			m_PartialMatchFile.close();
			m_FullMatchFile.close();
			waitForFileToBeReadable(strPartialMatchPathAndFile);
			waitForFileToBeReadable(strFullMatchPathAndFile);

			//open testresults.dat output file
			string strTestResultsDat = strTesterResultsFolder + "TesterResults.dat";
			ofstream testCases(strTestResultsDat.c_str(), ofstream::out | ofstream::app);
			if (!testCases.is_open())
			{
				UCLIDException ue("ELI09024", "Unable to open file for writing.");
				ue.addDebugInfo("filename", "TesterResults.dat");
				throw ue;
			}

			// output the partial and full match files to the testresults.dat file
			createCommentLine(testCases, "-=[ TESTER RESULTS -" + strTimeNote + "]=-");
			createCommentLine(testCases, "** Partial Match Test Cases **");
			createTestFileLine(testCases, m_strPartialMatchFile, true);
			createCommentLine(testCases, "** Full Match Test Cases **");
			createTestFileLine(testCases, m_strFullMatchFile, true);
			testCases << endl;

			testCases.close();
			waitForFileToBeReadable(strTestResultsDat);

			m_ipResultLogger->EndTestCase(bSuccess ? VARIANT_TRUE : VARIANT_FALSE);

			bSuccess = false;
			// Add the stats for name + type
			m_ipResultLogger->StartTestCase(_bstr_t(strTimeNote.c_str()), _bstr_t("Summary of attribute+type-level statistics"), kSummaryTestCase); 
			
			map<string, NamePlusTypeStats>::iterator it;
			for(it = m_mapNamePlusType.begin(); it != m_mapNamePlusType.end(); it++)
			{
				string strName = it->first;
				NamePlusTypeStats stats = it->second;

				CString zTemp;

				double percent;
				if(stats.m_nExpected > 0)
				{
					percent = 100.0 * (double)stats.m_nPassed / (double)stats.m_nExpected;
				}
				else
				{
					percent = 100.0;
				}
				// Test attributes - executed, passed, failed, percentage
				zTemp.Format(	"Results for %s: (%d/%d) %.1f%% found correctly with %d extra attributes found",
								strName.c_str(), stats.m_nPassed, stats.m_nExpected, percent, stats.m_nExtra);
				m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
			}

			m_ipResultLogger->EndTestCase(VARIANT_FALSE);

		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI06314")
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
		ASSERT_RESOURCE_ALLOCATION("ELI33408", ipRuleSet != __nullptr);

		ipRuleSet->Cleanup();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI33409");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CGrantorGranteeTester::raw_RunInteractiveTests()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CGrantorGranteeTester::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		m_ipResultLogger = pLogger;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07294")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CGrantorGranteeTester::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CGrantorGranteeTester::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// Helpers
//-------------------------------------------------------------------------------------------------
bool CGrantorGranteeTester::compareAttributes(IIUnknownVectorPtr ipFoundAttributes, 
											  IIUnknownVectorPtr ipExpectedAttributes,
											  const string& strRuleID)
{
	bool bReturn = false;

	// always add expected attributes as a test case memo no matter
	// if the test case succeeds or fails
	string strExpectedAttr = m_ipAFUtility->GetAttributesAsString(ipExpectedAttributes);

	// Get count of expected attributes
	long lExpectedCount = ipExpectedAttributes->Size();

	if (ipFoundAttributes->Size() != 0)
	{
		// only add found attributes to the memo if it's not empty
		string strFoundAttr = m_ipAFUtility->GetAttributesAsString(ipFoundAttributes);
		//Add Compare Attributes to the tree and the dialog.
		m_ipResultLogger->AddTestCaseCompareData("Compare Attributes", 
												 "Expected Attributes", _bstr_t(strExpectedAttr.c_str()),
												 "Found Attributes", _bstr_t(strFoundAttr.c_str()));
	}
	else
	{	
		m_ipResultLogger->AddTestCaseMemo(_bstr_t("Expected Attributes"),
						_bstr_t(strExpectedAttr.c_str()));

		// Update Attribute counter
		m_nTotalTestAttributes += lExpectedCount;

		int i;
		for(i = 0; i < lExpectedCount; i++)
		{
			IAttributePtr ipExpected = ipExpectedAttributes->At( i );
			// each expected attribute needs its increment the name + type 
			string strNamePlusType = "";
			strNamePlusType = strNamePlusType + asString(ipExpected->Name) + "+" + asString(ipExpected->Type);
			m_mapNamePlusType[strNamePlusType].m_nExpected++;
		}

		// if there's no found attributes, throw exception
		throw UCLIDException("ELI06317", "No attribute is found.");
	}

	// Make a local vector of flags to indicate Used vs. Not Used
	vector<bool>	vecUsedAttributes;

	// Fill vector with "False" for each Found Attribute
	long lFoundSize = ipFoundAttributes->Size();
	int i;
	for (i = 0; i < lFoundSize; i++)
	{
		vecUsedAttributes.push_back( false );
	}

	// Reset local count of Passed attributes
	long lLocalPassed = 0;

	// Check each expected Attribute
	int iFoundIndex = -1;
	for (i = 0; i < lExpectedCount; i++)
	{
		bool	bDoneLooking = false;
		long	lStartIndex = 0;

		// Increment Attribute counter
		m_nTotalTestAttributes++;

		IAttributePtr ipExpected = ipExpectedAttributes->At( i );
		// each expected attribute needs its increment the name + type 
		string strNamePlusType = "";
		strNamePlusType = strNamePlusType + asString(ipExpected->Name) + "+" + asString(ipExpected->Type);
		m_mapNamePlusType[strNamePlusType].m_nExpected++;

		while (!bDoneLooking)
		{
			// Look for this Attribute
			iFoundIndex = ipFoundAttributes->FindByValue( ipExpected, lStartIndex );

			if (iFoundIndex > -1)
			{
				// Check Used flag
				if (vecUsedAttributes.at( iFoundIndex ) == false)
				{
					// Increment local Passed counter
					lLocalPassed++;

					m_mapNamePlusType[strNamePlusType].m_nPassed++;

					// Set the used flag and stop looking
					vecUsedAttributes.at( iFoundIndex ) = true;
					bDoneLooking = true;
				}
				else
				{
					// This Attribute has already been Used

					// Update the StartIndex
					lStartIndex = iFoundIndex + 1;
					if (lStartIndex >= lFoundSize)
					{
						// No more room for another matching Attribute
						// Stop looking
						bDoneLooking = true;
					}

					m_mapRuleCaptureInfo[strRuleID].m_lNumOfDuplicateAttributes++;
					// else keep looking for another match
				}	// end else Attribute already Used
			}		// end if Attribute found
			else
			{
				// Not found, quit looking for this Attribute
				bDoneLooking = true;
			}		// end else Attribute NOT found
		}			// end while checking Found Attributes
	}				// end for each Expected Attribute

	// Count number of Found Attributes that were not Used
	for (i = 0; i < lFoundSize; i++)
	{
		if (vecUsedAttributes.at( i ) == false)
		{
			// Increment counter
			m_mapRuleCaptureInfo[strRuleID].m_lNumOfFailedAttributes++;
			m_nNumOfOtherAttributes++;

			IAttributePtr ipFound = ipFoundAttributes->At(i);
			string strNamePlusType = "";
			strNamePlusType = strNamePlusType + asString(ipFound->Name) + "+" + asString(ipFound->Type);
			m_mapNamePlusType[strNamePlusType].m_nExtra++;
		}
	}

	// Update member Passed count
	m_nNumOfPassedAttributes += lLocalPassed;
	m_mapRuleCaptureInfo[strRuleID].m_lNumOfPassedAttributes += lLocalPassed;

	// Check for complete match
	if (lExpectedCount == lFoundSize && lLocalPassed == lExpectedCount)
	{
		// Set flag
		bReturn = true;
	}

	return bReturn;
}
//-------------------------------------------------------------------------------------------------
string CGrantorGranteeTester::getDocumentClassificationInfo(IAFDocumentPtr ipAFDoc)
{
	string strDocClassificationInfo("");
	
	IStrToObjectMapPtr ipObjectTags(ipAFDoc->ObjectTags);
	string strDocType("");
	if (ipObjectTags != __nullptr && ipObjectTags->Size > 0)
	{
		if (ipObjectTags->Contains(_bstr_t(DOC_TYPE.c_str())) == VARIANT_TRUE)
		{
			IVariantVectorPtr ipVecDocTypes = ipObjectTags->GetValue(_bstr_t(DOC_TYPE.c_str()));
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
		if (ipStringTags->Contains(_bstr_t(DOC_PROBABILITY.c_str())) == VARIANT_TRUE)
		{
			// document is classified at a certain confidence level,
			// such as Sure, Probable and Maybe
			string strDocProbabilityLevel = asString(ipStringTags->GetValue(_bstr_t(DOC_PROBABILITY.c_str())));
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
string CGrantorGranteeTester::getDocumentProbabilityString(const string& strProbability)
{
	static string strDescriptions[] = { "Zero", "Maybe", "Probable", "Sure" };
	string strRet("");

	long nIndex = asLong(strProbability);
	if (nIndex < 0 || nIndex > sizeof(strDescriptions))
	{
		UCLIDException ue("ELI19334", "Invalid probability.");
		ue.addDebugInfo("strProbability", strProbability);
		throw ue;
	}

	return strDescriptions[nIndex];
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CGrantorGranteeTester::getExpectedAttributes(const string& strExpectedAttrFileName)
{
	IIUnknownVectorPtr ipExpectedAttributes(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI06327", ipExpectedAttributes != __nullptr);

	// get all attributes from the file
	IIUnknownVectorPtr ipAllAttributes = 
		m_ipAFUtility->GenerateAttributesFromEAVFile(_bstr_t(strExpectedAttrFileName.c_str()));

	// iterate throught the vector and only store the 
	// attribute with name "Grantor-Grantee"
	long nSize = ipAllAttributes->Size();
	for (long n=0; n<nSize; n++)
	{
		IAttributePtr ipAttribute = ipAllAttributes->At(n);
		string strAttrName = ipAttribute->Name;
		if (strAttrName == "GrantorGrantee")
		{
			ipExpectedAttributes->PushBack(ipAttribute);
		}
	}

	return ipExpectedAttributes;
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CGrantorGranteeTester::getGrantorGranteeAttributes(IAttributePtr ipAttribute)
{
	IIUnknownVectorPtr ipRetVec(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI19320", ipRetVec != __nullptr);

	// retain the attribute name and type
	string strName = ipAttribute->Name;
	string strType = ipAttribute->Type;

	// now take a look at the first level sub attributes and find 
	// the ones with the names as "Person" or "Company"
	IIUnknownVectorPtr ipSubAttributes = ipAttribute->SubAttributes;
	if (ipSubAttributes != __nullptr && ipSubAttributes->Size() > 0)
	{
		long nSize = ipSubAttributes->Size();
		for (long n=0; n<nSize; n++)
		{
			IAttributePtr ipSubAttr = ipSubAttributes->At(n);
			ICopyableObjectPtr ipCopyAttr(ipSubAttr);
			if (ipCopyAttr)
			{
				string strSubAttrName = ipSubAttr->Name;
				// if the name is "Person" or "Company"...
				if (strSubAttrName == "Person" 
					|| strSubAttrName == "Company"
					|| strSubAttrName == "Nominee"
					|| strSubAttrName == "NomineeFor")
				{
					IAttributePtr ipClonedAttr = ipCopyAttr->Clone();
					// then modify its name and type and 
					// leave the value unchanged
					ipClonedAttr->Name = _bstr_t(strName.c_str());
					ipClonedAttr->Type = _bstr_t(strType.c_str());
					// now wipe out all it input validator and 
					// sub attributes and splitter if any
					ipClonedAttr->InputValidator = NULL;
					ipClonedAttr->SubAttributes = NULL;
					ipClonedAttr->AttributeSplitter = NULL;

					// and put the modified attribute in the returning vector
					ipRetVec->PushBack(ipClonedAttr);
				}
			}
		}
	}

	return ipRetVec;
}
//-------------------------------------------------------------------------------------------------
string CGrantorGranteeTester::getRuleID(IAFDocumentPtr ipAFDoc)
{
	string strRuleID("");

	IStrToObjectMapPtr ipObjMap = ipAFDoc->ObjectTags;
	if (ipObjMap != __nullptr && ipObjMap->Size > 0)
	{
		// before put any attributes in the grid, add record(s) to grid
		// to display the which rule is actually used to capture the data if any
		if (ipObjMap->Contains(_bstr_t(RULE_WORKED_TAG.c_str())) == VARIANT_TRUE)
		{
			IStrToStrMapPtr ipRulesWorked = ipObjMap->GetValue(_bstr_t(RULE_WORKED_TAG.c_str()));
			if (ipRulesWorked)
			{
				// Search through all the rules that capture an attribute and append them to
				// the rule id
				long lSize = ipRulesWorked->Size;
				for(int i = 0; i < lSize; i++)
				{
					CComBSTR bstrKey, bstrValue;
					ipRulesWorked->GetKeyValue(i, &bstrKey, &bstrValue);
					
					// if this is the first rule don't prepend an '&', otherwise prepend '&'
					(i == 0) ? strRuleID = asString(bstrValue) : strRuleID += string(" & ") + asString(bstrValue);
				}
			}
		}
	}

	return strRuleID;
}
//-------------------------------------------------------------------------------------------------
void CGrantorGranteeTester::interpretLine(const string& strLineText,
											const string& strCurrentDatFileName,
											const string& strCaseNum)
{
	// *****************************************************************************
	// Note: Each line must have a tag (i.e. <FILE>, <TESTCASE>, or <TESTFOLDER>
	// to indicate the its need for different interpretation.
	// Tags:
	// <FILE> -- followed by a .dat file that has similiar structure 
	//			 as TestGrantorGrantee.dat
	// <TESTCASE> -- followed by an .rsd file, an input text file and a .eav file
	// <TESTFOLDER> -- followed by an .rsd file, a folder name containing input text files 
	//				   and their associated .eav files
	// <READ_FROM_FILE_TAG> -- followed by MapFile name, ReadFromFileName, Tag
	//							MapFile will contain lines with Path; RsdFileName, Relative folder Path for EAV
	//							ReadFromFileName is a Rules file with comment line of format
	//								// <TAG>TestCaseFilename;Optional EAV File Name
	// *****************************************************************************

	// parse each line into multiple tokens with the delimiter as ";"
	vector<string> vecTokens;
	StringTokenizer::sGetTokens(strLineText, ';', vecTokens);
	int nNumOfTokens = vecTokens.size();
	if (nNumOfTokens < 2)
	{
		UCLIDException ue("ELI06321", "Invalid line of text.");
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
			UCLIDException ue("ELI06322", "There shall be one and only one file name follow the tag <FILE>.");
			ue.addDebugInfo("Line", strLineText);
			throw ue;
		}

		// get the name of the dat file that follows the <FILE>
		string strDatFile = getAbsoluteFileName(strCurrentDatFileName,
			vecTokens[1], true);

		// process the dat file
		processDatFile(strDatFile, strCaseNum);
	}
	else if (strTag == "<TESTCASE>")
	{
		// in this case, 4 tokens are allowed
		if (nNumOfTokens != 4)
		{
			UCLIDException ue("ELI06323", "<TESTCASE> line takes an RSD file, an input file and an EAV file.");
			ue.addDebugInfo("Line", strLineText);
			throw ue;
		}

		string strRSDFile = getAbsoluteFileName(strCurrentDatFileName, vecTokens[1], true);
		string strInputFile = getAbsoluteFileName(strCurrentDatFileName,
			vecTokens[2], true);
		string strEAVFile = getAbsoluteFileName(strCurrentDatFileName,
			vecTokens[3], true);

		// process this test case
		processTestCase(strRSDFile, strInputFile, strEAVFile, strCurrentDatFileName, strCaseNum);
	}
	else if (strTag == "<TESTFOLDER>")
	{
		// in this case, only 3 or 4 tokens allowed
		if (nNumOfTokens != 3 && nNumOfTokens != 4)
		{
			UCLIDException ue("ELI06328", "<TESTCASE> line takes an RSD file, an input files folder and an optional EAV files folder.");
			ue.addDebugInfo("Line", strLineText);
			throw ue;
		}

		string strRSDFile = getAbsoluteFileName(strCurrentDatFileName, vecTokens[1], true);
		string strInputFileFolder = getAbsoluteFileName(strCurrentDatFileName,
										vecTokens[2], true);

		// third parameter is the EAV files folder if exists
		string strEAVFilesFolder("");
		if (nNumOfTokens == 4)
		{
			strEAVFilesFolder = getAbsoluteFileName(strCurrentDatFileName,
										vecTokens[3], true);
		}

		processTestFolder(strRSDFile, strInputFileFolder, strEAVFilesFolder, strCurrentDatFileName, strCaseNum);
	}
	else if ( strTag == "<READ_FROM_FILE_TAG>" )
	{
		if ( nNumOfTokens != 4 )
		{
			UCLIDException ue("ELI09409", "<READ_FROM_FILE_TAG> line takes a Map file, a ReadFrom file  and a Tag name.");
			ue.addDebugInfo("Line", strLineText);
			ue.addDebugInfo("Number of Tokens", nNumOfTokens );
			throw ue;
		}
		string strMapFileName = getAbsoluteFileName(strCurrentDatFileName,
									vecTokens[1], true );

		IAFDocumentPtr ipAFDoc(CLSID_AFDocument);
		ASSERT_RESOURCE_ALLOCATION("ELI09533", ipAFDoc != __nullptr );
		string strReadFromFileName = m_ipAFUtility->ExpandTags( vecTokens[2].c_str(), ipAFDoc );
		strReadFromFileName = getAbsoluteFileName(strCurrentDatFileName,
									strReadFromFileName, false );
		
		string strCommentTag = vecTokens[3];
		
		// Get the Input file name from the read from file and tag with optional EAV File name
		vector<TestCaseData>& vecTestFiles = getTestCaseFilesUsingTag( strReadFromFileName, strCommentTag );
		int nNumCases = vecTestFiles.size();
		int nSubCaseNo = 0;
		for ( int i = 0; i < nNumCases; i++ )
		{
			getRSDAndEAVFileName( strMapFileName, vecTestFiles[i]);
			//validateFileOrFolderExistence( vecTestFiles[i].m_strRSDFilename);
			// if the EAV file does not exist don't run
//			if (access(vecTestFiles[i].m_strEAVFilename.c_str(), 00) == 00)
//			{
				nSubCaseNo++;
				
				// update the test case no
				CString zCaseNo("");
				zCaseNo.Format("%s_%d", strCaseNum.c_str(), i + 1 /*nSubCaseNo*/);
				
				processTestCase(vecTestFiles[i].m_strRSDFilename, 
					vecTestFiles[i].m_strUSSFilename,
					vecTestFiles[i].m_strEAVFilename,
					strCurrentDatFileName, LPCTSTR(zCaseNo),
					vecTestFiles[i].m_strRuleID );
				makeLowerCase(vecTestFiles[i].m_strUSSFilename );
				m_setProcessedFiles.insert(vecTestFiles[i].m_strUSSFilename);
//			}
		}
	}
	else
	{
		UCLIDException ue("ELI06324", "Please provide a valid tag for this line. "
			"For instance, <FILE>, <TESTCASE>, <TESTFOLDER> or <READ_FROM_FILE_TAG>.");
		ue.addDebugInfo("Line", strLineText);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void CGrantorGranteeTester::processDatFile(const string& strDatFileName, 
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
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI06325")
		}
		catch(UCLIDException ue)
		{
			// Store the exception for display and continue
			m_ipResultLogger->AddComponentTestException(get_bstr_t(ue.asStringizedByteStream()));
		}
		nCaseNo++;
	}
	while(!ifs.eof());
}
//-------------------------------------------------------------------------------------------------
void CGrantorGranteeTester::processTestCase(const string& strRSDFile,
											const string& strInputTextFile,
											const string& strEAVFile,
											const string& strTestCaseTitle,
											const string& strTestCaseNo,
											const string& strExpectedRule)
{
	// calculate the note file's path
	string strFolderName = ::getDirectoryFromFullPath(strInputTextFile) + "\\";
	string strImageFileName = ::getFileNameWithoutExtension(strInputTextFile);
	// Note file should be in same directory as EAVfile
	string strEAVFileDir = ::getDirectoryFromFullPath(strEAVFile) + "\\";
	string strNoteFile = strEAVFileDir + strImageFileName + ".nte";

	// Pre declare string for Attached note
	string strAttachedNote("");	

	// Initiate a test case
	m_ipResultLogger->StartTestCase(_bstr_t(strTestCaseNo.c_str()),
					_bstr_t(strTestCaseTitle.c_str()), kAutomatedTestCase);

	bool bSuccess = false;
	bool bExceptionCaught = false;
	// Pre declare string for rule id
	string strRuleID;
	
	// Use 2 try blocks to catch exceptions that occur before the results or expected results are counted
	// The inner try catch block will catch exceptions that occur while obtaining the found results 
	// The outer try catch block will catch the exceptions that occur in the processing of the expected 
	// results.
	
	// Use 2 try blocks to catch exceptions that occur before the results or expected results are counted
	// Try catch block will catch exceptions that occur while obtaining the found results 
	try
	{
		m_ipResultLogger->AddTestCaseFile(_bstr_t(strInputTextFile.c_str()));
		
		// create a filename with the same name as the text file
		// but without the extension ".txt".  Since all our test
		// files are named a.tif.txt, removing the .txt leaves us with
		// the image name
		string strImageFile = getDirectoryFromFullPath(strInputTextFile) + "\\";
		strImageFile += getFileNameWithoutExtension(strInputTextFile);
		m_ipResultLogger->AddTestCaseFile(_bstr_t(strImageFile.c_str()));
		// add the eav file to the logger
		m_ipResultLogger->AddTestCaseFile(_bstr_t(strEAVFile.c_str()));
		// add the nte file to the logger (regardless of whether the file exists)
		m_ipResultLogger->AddTestCaseFile(_bstr_t(strNoteFile.c_str()));

		if ( strExpectedRule != "" )
		{
			string strExpected = "The Expected Rule ID is: " + strExpectedRule;
			m_ipResultLogger->AddTestCaseNote(_bstr_t(strExpected.c_str()));
		}
		
		// Validate RSD File and Input file
		string strExpectedFile = "";
		try
		{
			strExpectedFile = "Input File Missing";
			validateFileOrFolderExistence( strInputTextFile );
			strExpectedFile = "Rules File Missing";
			validateFileOrFolderExistence( strRSDFile );
		}
		catch ( UCLIDException ue )
		{
			// Add additional Debug info
			ue.addDebugInfo ( "Expected File", strExpectedFile );
			throw ue;
		}

		// Make up a SpatialString
		IAFDocumentPtr ipAFDoc(CLSID_AFDocument);
		ASSERT_RESOURCE_ALLOCATION("ELI07439", ipAFDoc != __nullptr);
		ISpatialStringPtr ipInputText = ipAFDoc->Text;
		ipInputText->LoadFrom(_bstr_t(strInputTextFile.c_str()), VARIANT_TRUE);
		// find all attributes in the text file
		if (m_ipCurrentAttributes != __nullptr )
		{
			m_ipCurrentAttributes->Clear();
		}
		IIUnknownVectorPtr ipFoundAttributes = m_ipAttrFinderEngine->FindAttributes( ipAFDoc, _bstr_t(""), -1, 
			_bstr_t( strRSDFile.c_str() ), NULL, VARIANT_TRUE, NULL );
		ASSERT_RESOURCE_ALLOCATION("ELI08799", ipFoundAttributes != __nullptr );

		m_ipCurrentAttributes = ipFoundAttributes;
		
		// add document classification information
		string strDocType = getDocumentClassificationInfo(ipAFDoc);
		if (!strDocType.empty())
		{
			// add a note for document classification information
			m_ipResultLogger->AddTestCaseNote(_bstr_t(strDocType.c_str()));
		}
		
		// add rule id that worked
		strRuleID = getRuleID(ipAFDoc);
		if (!strRuleID.empty())
		{
			strRuleID = "Rule that captures the attributes : " + strRuleID;
			m_ipResultLogger->AddTestCaseNote(_bstr_t(strRuleID.c_str()));
		}
		//reset the rule ID string
		strRuleID = getRuleID(ipAFDoc);
		
		if (strRuleID.empty()) // push file into "NO_RULE" if it had no partial match
		{
			m_mapRuleCaptureInfo["NO_RULE"].m_vecTestCasesInfo.push_back(TestCaseData(strRSDFile, strInputTextFile, strEAVFile, strRuleID, kNoMatch));
		}
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI09205", m_ipResultLogger, bExceptionCaught, VARIANT_FALSE);
	
	// try catch block will catch the exceptions that occur in the processing of the expected 
	// results.
	try
	{
		// Validate EAV file Existence
		try
		{
			validateFileOrFolderExistence( strEAVFile );
		}
		catch ( UCLIDException ue )
		{
			// Add additional Debug info
			ue.addDebugInfo ( "Expected File", "EAV File Missing" );
			throw ue;
		}

		// Make sure there is something to compare against otherwise compareAttributes will throw 
		// exception and not count the expected attributes
		if ( m_ipCurrentAttributes == __nullptr )
		{
			m_ipCurrentAttributes.CreateInstance(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI09206", m_ipCurrentAttributes != __nullptr );
		}
		// get expected attributes from the file
		IIUnknownVectorPtr ipExpectedAttributes = getExpectedAttributes(strEAVFile);
				
		// Since this is also an output handler, the current set attributes
		// found by the rule set is obtained after FindAttributesInText() call finished.		
		// Compare actual found attributes with expected attributes
		if (compareAttributes(m_ipCurrentAttributes, ipExpectedAttributes, strRuleID))
		{
			// Comparison succeeded
			bSuccess = true;
			
			// get the note if any
			if (isFileOrFolderValid( strNoteFile ))
			{
				strAttachedNote = fileAsString(strNoteFile);
			}

			// add to the final note if this input text file has
			// associated .nte file
			if (!strAttachedNote.empty())
			{
				if (!m_strFileSucceedWithNote.empty())
				{
					m_strFileSucceedWithNote += "\r\n";
				}
				m_strFileSucceedWithNote += strInputTextFile;
			}
		}

		// Store matching and partially matching files into the 
		// map for files matching by a rule
		if (!strRuleID.empty())
		{
			// if match was successful, push full match into output
			// else push partial match
			if (bSuccess)
			{
				m_mapRuleCaptureInfo[strRuleID].m_vecTestCasesInfo.push_back(TestCaseData(strRSDFile, strInputTextFile, strEAVFile, strRuleID, kFullMatch));
			}
			else
			{
				m_mapRuleCaptureInfo[strRuleID].m_vecTestCasesInfo.push_back(TestCaseData(strRSDFile, strInputTextFile, strEAVFile, strRuleID, kPartialMatch));
			}
		}

	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI06326", m_ipResultLogger, bExceptionCaught, VARIANT_FALSE);

	// Check for success
	bSuccess = bSuccess && !bExceptionCaught;
	VARIANT_BOOL bRet = bSuccess ? VARIANT_TRUE : VARIANT_FALSE;
	
	// Check for potential Detail Note
	if (!bSuccess)
	{
		if (!strAttachedNote.empty())
		{
			// truncate the note to a reasonable displayable length for the result logger
			string strTitle = strAttachedNote.substr(0, 100);
			// Trim leading and trailing whitespace from truncated note
			strTitle = trim( strTitle.c_str(), " \r\n", " \r\n" );
			// Add Test Case Detail Note with Provided Information
			m_ipResultLogger->AddTestCaseDetailNote( 
				_bstr_t(strTitle.c_str()), 
				_bstr_t(strAttachedNote.c_str()));
		}
		else
		{
			// if the file failed and with no related note, list it
			if (!m_strFileFailedWithoutNote.empty())
			{
				m_strFileFailedWithoutNote += "\r\n";
			}

			m_strFileFailedWithoutNote += strInputTextFile;
		}
	}

	// Clear previously found results
	m_ipCurrentAttributes->Clear();

	// end the test case
	m_ipResultLogger->EndTestCase(bRet);
}
//-------------------------------------------------------------------------------------------------
void CGrantorGranteeTester::processTestFolder(const string& strRSDFile,
											  const string& strTestFolder,
											  const string& strEAVFilesFolder,
											  const string& strTestCaseTitle,
											  const string& strTestCaseNo)
{
	// get all .txt file from the folder
	vector<string> vecTextFiles;
	::getFilesInDir(vecTextFiles, strTestFolder, "*.uss", true);
	if (vecTextFiles.size() == 0)
	{
		// if there's no uss file at all, get .txt files
		::getFilesInDir(vecTextFiles, strTestFolder, "*.txt", true);
	}

	for (unsigned int ui = 0; ui < vecTextFiles.size(); ui++)
	{
		string strInputTextFileName = vecTextFiles[ui];
		string strFolderName = ::getDirectoryFromFullPath(strInputTextFileName) + "\\";
		// by default, retrieve eav file from the same folder as the image file
		string strEAVFolderName = strFolderName;
		if (!strEAVFilesFolder.empty())
		{
			strEAVFolderName = strEAVFilesFolder + "\\";
		}
		// look for the .eav file with the same image file name
		string strImageFileName = ::getFileNameWithoutExtension(strInputTextFileName);
		// get eav file name
		string strEAVFileName = strEAVFolderName + strImageFileName + ".eav";

		// if this eav file exists, then process the test
		if (isValidFile(strEAVFileName))
		{
			// further more, there could be a note file related 
			// to this text input file. We shall list the note if there's any
			string strNoteFile = strEAVFolderName + strImageFileName + ".nte";
			
			// update the test case no
			CString zCaseNo("");
			zCaseNo.Format("%s_%d", strTestCaseNo.c_str(), ui + 1);

			processTestCase(strRSDFile, strInputTextFileName, strEAVFileName, 
				strTestCaseTitle, (LPCTSTR) zCaseNo);
		}
	}
}
//-------------------------------------------------------------------------------------------------
string CGrantorGranteeTester::fileAsString(const string& strInputFileName)
{
	// make sure the file is valid
	if (!isValidFile(strInputFileName))
	{
		// if the test file doesn't exist
		UCLIDException uclidException("ELI06412", "File doesn't exist.");
		uclidException.addDebugInfo("File name", strInputFileName);
		uclidException.addWin32ErrorInfo();
		throw uclidException;
	}

	return getTextFileContentsAsString(strInputFileName);
}
//-------------------------------------------------------------------------------------------------
void CGrantorGranteeTester::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnRULE_DEVELOPMENT_TOOLKIT_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI07293", "Grantor-Grantee Finder Tester" );
}
//-------------------------------------------------------------------------------------------------
void CGrantorGranteeTester::dumpRuleCaptureInfo(ostream& partialMatches, ostream& fullMatches)
{
	// loop through all the rules and output their corresponding test cases
	for (map<string, RuleCaptureData >::iterator iter = m_mapRuleCaptureInfo.begin(); iter != m_mapRuleCaptureInfo.end(); iter++)
	{
		//these bools ensure that the comment header for each rule was displayed only once 
		//for each matching result type's listing of test cases found under that rule
		bool bPartialMatchRuleIDComment = false;
		bool bFullMatchRuleIDComment = false;
		bool bNoMatchRuleIDComment = false;

		vector<TestCaseData> *vecTestCases = &iter->second.m_vecTestCasesInfo;

		// loop through the file names for a particular rule and create a testcase for that file
		for (vector<TestCaseData>::iterator testCaseIter = vecTestCases->begin(); testCaseIter != vecTestCases->end(); testCaseIter++)
		{
			// checks for the type of result (full, partial, or no match)
			if ((*testCaseIter).m_eMatchResult == kFullMatch)
			{
				if (!bFullMatchRuleIDComment)
				{
					// Output the start of rule ID comment
					createCommentLine(fullMatches, " -=[ " + iter->first + " ]=- -=[ START ]=-");
					bFullMatchRuleIDComment = true;
				}

				createTestCaseLine(fullMatches, (*testCaseIter).m_strRSDFilename, (*testCaseIter).m_strUSSFilename, (*testCaseIter).m_strEAVFilename);
			}
			else if ((*testCaseIter).m_eMatchResult == kPartialMatch)
			{
				if (iter->first != "NO_RULE")
				{
					if (!bPartialMatchRuleIDComment)
					{
						// Output the start of rule ID comment
						createCommentLine(partialMatches, " -=[ " + iter->first + " ]=- -=[ START ]=-");
						bPartialMatchRuleIDComment = true;
					}

					createTestCaseLine(partialMatches, (*testCaseIter).m_strRSDFilename, (*testCaseIter).m_strUSSFilename, (*testCaseIter).m_strEAVFilename);
				}
			}
			else if ((*testCaseIter).m_eMatchResult == kNoMatch)
			{
				if (!bNoMatchRuleIDComment)
				{
					// Output the start of rule ID comment
					createCommentLine(partialMatches, " -=[ NO MATCHING RULE ]=- -=[ START ]=-");
					bNoMatchRuleIDComment = true;
				}

				createTestCaseLine(partialMatches, (*testCaseIter).m_strRSDFilename, (*testCaseIter).m_strUSSFilename, (*testCaseIter).m_strEAVFilename);
			}
		}

		// Output the end of rule ID comment
		if (bFullMatchRuleIDComment)
		{
			createCommentLine(fullMatches, " -=[ " + iter->first + " ]=- -=[  END  ]=-");
			fullMatches << endl;
		}

		if (bPartialMatchRuleIDComment)
		{
			createCommentLine(partialMatches, " -=[ " + iter->first + " ]=- -=[  END  ]=-");
			partialMatches << endl;
		}

		if (bNoMatchRuleIDComment)
		{
			createCommentLine(partialMatches, " -=[ NO MATCHING RULE ]=- -=[  END  ]=-");
			partialMatches << endl;
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CGrantorGranteeTester::createCommentLine(ostream& output, const string& strComment)
{
	output << "//" << strComment << endl;
}
//-------------------------------------------------------------------------------------------------
void CGrantorGranteeTester::createTestCaseLine(ostream& output, const string& strRSDFile, const string& strUSSFile, const string& strEAVFile)
{
	output << "<TESTCASE>;" << strRSDFile << ";" << strUSSFile << ";" << strEAVFile << endl;
}
//-------------------------------------------------------------------------------------------------
void CGrantorGranteeTester::createTestFileLine(ostream& output, const string& strFilename, const bool bCommented)
{
	if(bCommented)
	{
		output << "//<FILE>;" << strFilename << endl;
	}
	else
	{
		output << "<FILE>;" << strFilename << endl;
	}

}
//-------------------------------------------------------------------------------------------------
vector<CGrantorGranteeTester::TestCaseData> CGrantorGranteeTester::getTestCaseFilesUsingTag( const string& strFileName, const string& strTag )
{
	string strETFFile;
	string strExt = getExtensionFromFullPath(strFileName, true );
	if ( strExt != ".etf" )
	{
		strETFFile = strFileName + ".etf";
	}
	else
	{
		strETFFile = strFileName;
	}
	autoEncryptFile(strETFFile, gstrAF_AUTO_ENCRYPT_KEY_PATH);

	// Load file into vector;
	vector<string>& vecLines = convertFileToLines( strETFFile );
	// Search comments for tag
	long nNumLines = vecLines.size();
	vector<TestCaseData> vecTestCaseData;
	long nNumSinceLastRule = 0;
	for ( int i = 0; i < nNumLines; i++ )
	{
		string& strLine = vecLines[i];
		// Look for comment lines
		long nCommentPos = strLine.find( "//" );
		if ( nCommentPos != string::npos )
		{
			// look for the Tag
			long nTagPos = strLine.find( strTag );
			if ( nTagPos != string::npos )
			{
				// parse each line into multiple tokens with the delimiter as ";"
				vector<string> vecTokens;
				StringTokenizer::sGetTokens(strLine, ';', vecTokens);
				TestCaseData testCase;
				if ( vecTokens.size() > 1 )
				{
					testCase.m_strUSSFilename = trim(vecTokens[1], " ", " ");
					// if no uss file extension add it
					string strExt = getExtensionFromFullPath( vecTokens[1], true );
					if ( strExt != ".uss" )
					{
						testCase.m_strUSSFilename += ".uss";
					}
				}
				// if there is a 3rd token it is the eav file
				if ( vecTokens.size() > 2  && vecTokens[2].size() > 0)
				{
					// Get eav file and if no path give it same path as ussfile
					testCase.m_strEAVFilename = getAbsoluteFileName ( testCase.m_strUSSFilename, vecTokens[2] );
				}
				// if uss file is in the list of processed files don't put in the list of files to run
				makeLowerCase(testCase.m_strUSSFilename );
				set<string>::iterator iterFiles = m_setProcessedFiles.find(testCase.m_strUSSFilename);
				if ( iterFiles == m_setProcessedFiles.end() )
				{
					vecTestCaseData.push_back( testCase );
					nNumSinceLastRule++;
				}
			}
		}
		else
		{
			// No comment found so either blank or rule
			// look for = indicating end of ruleID
			long nEqualPos = strLine.find("=");
			if ( nEqualPos != string::npos )
			{
				// Get rule id 
				string strRuleID = strLine.substr(0, nEqualPos );
				// Set ruleID in all test cases since last ruleID
				int nNumCases = vecTestCaseData.size();
				for ( int c = nNumCases - nNumSinceLastRule; c < nNumCases ; c++ )
				{
					TestCaseData& testCase = vecTestCaseData[c];
					testCase.m_strRuleID = strRuleID;
				}
				// Reset count of uss files since last RuleID
				nNumSinceLastRule = 0;
			}
		}
	}

	return vecTestCaseData;
}
//-------------------------------------------------------------------------------------------------
void CGrantorGranteeTester::getRSDAndEAVFileName ( const string& strMapFile, TestCaseData& testCaseData )
{
	// Load file into vector;
	vector<string>& vecLines = convertFileToLines( strMapFile );
	CommentedTextFileReader readerMapFile( vecLines, "//", true );
	
	// Get USS File path
	string strUSSPath = getDirectoryFromFullPath ( testCaseData.m_strUSSFilename );
	// Get Extentions of filename ( may not be uss )
	string strExt = getExtensionFromFullPath( testCaseData.m_strUSSFilename, true );

	// Get the Image File Name
	string strImageFileName;
	if ( strExt != ".uss" )
	{
		// if not uss get the image file name with extension
		strImageFileName = getFileNameFromFullPath( testCaseData.m_strUSSFilename );
	}
	else
	{
		// if uss file then remove uss extension for the image file name
		strImageFileName = getFileNameWithoutExtension( testCaseData.m_strUSSFilename );
	}
	// Read map file  
	string strLine("");
	while ( !readerMapFile.reachedEndOfStream() )
	{
		strLine = readerMapFile.getLineText();
		if ( strLine.empty())
		{
			continue;
		}
		// parse each line into multiple tokens with the delimiter as ";"
		vector<string> vecTokens;
		StringTokenizer::sGetTokens(strLine, ';', vecTokens);
		int nNumTokens = vecTokens.size();
		if ( nNumTokens < 2)
		{
			continue;
		}
		// Check path of image file to path in map file
		string strMatchPath = vecTokens[0];
		makeLowerCase( strMatchPath );
		makeLowerCase( strUSSPath );
		long nPathPos = strUSSPath.find( strMatchPath );
		if ( nPathPos != string::npos )
		{
			// Set RSDFile if no path give it the path of the map file
			testCaseData.m_strRSDFilename = getAbsoluteFileName(strMapFile, vecTokens[1]);
			// if there is a third token it is the path of the EAV file
			if ( nNumTokens > 2 )
			{
				// if there is no eav file set or the one set does not exist
				if ( testCaseData.m_strEAVFilename == "" 
					||  !isValidFile(testCaseData.m_strEAVFilename))
				{
					string strRelEAVPath = vecTokens[2];
					strRelEAVPath +=  "\\" + strImageFileName + ".eav";
					int nTagPos = strRelEAVPath.find(IMAGE_FILE_DIR_TAG);
					if ( nTagPos != string::npos )
					{
						strRelEAVPath.replace(nTagPos, IMAGE_FILE_DIR_TAG.size(), "");
						testCaseData.m_strEAVFilename = getAbsoluteFileName( testCaseData.m_strUSSFilename, strRelEAVPath );
					}
					else
					{
						testCaseData.m_strEAVFilename +=  strImageFileName + ".eav";
					}
				}
			}
			else
			{
				// if no eav path token default to the path of the image file
				if ( testCaseData.m_strEAVFilename == "" 
					||  !isValidFile(testCaseData.m_strEAVFilename))
				{
					testCaseData.m_strEAVFilename = getAbsoluteFileName( testCaseData.m_strUSSFilename, strImageFileName ) + ".eav";
				}

			}
			// If the eav file is found stop looking
			if  (isValidFile(testCaseData.m_strEAVFilename))
			{
				break;
			}
			else
			{
				testCaseData.m_strEAVFilename = "";
			}
		}
	}
}

//-------------------------------------------------------------------------------------------------
// CGrantorGranteeTester::TestCaseData
//-------------------------------------------------------------------------------------------------
CGrantorGranteeTester::TestCaseData::TestCaseData(const string& strRSDFile, const string& strUSSFile, const string& strEAVFile, const string &strRuleID, const ETestCaseResult eResult)
{
	m_strRSDFilename = strRSDFile;
	m_strUSSFilename = strUSSFile;	
	m_strEAVFilename = strEAVFile;
	m_strRuleID		 = strRuleID;
	m_eMatchResult   = eResult;
}
//-------------------------------------------------------------------------------------------------
CGrantorGranteeTester::TestCaseData::TestCaseData()
{
	m_strRSDFilename = "";
	m_strUSSFilename = "";	
	m_strEAVFilename = "";
	m_strRuleID		 = "";
	m_eMatchResult   = kNoMatch;
}

//-------------------------------------------------------------------------------------------------
// CGrantorGranteeTester::RuleCaptureData
//-------------------------------------------------------------------------------------------------
CGrantorGranteeTester::RuleCaptureData::RuleCaptureData()
{
	m_lNumOfPassedAttributes = 0;
	m_lNumOfFailedAttributes = 0;
	m_lNumOfDuplicateAttributes = 0;
}

//-------------------------------------------------------------------------------------------------
// CGrantorGranteeTester::NamePlusTypeStats
//-------------------------------------------------------------------------------------------------
CGrantorGranteeTester::NamePlusTypeStats::NamePlusTypeStats()
:
m_nExpected(0),
m_nPassed(0),
m_nExtra(0)
{
}
//-------------------------------------------------------------------------------------------------
