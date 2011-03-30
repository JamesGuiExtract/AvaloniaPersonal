// SPMFinder.cpp : Implementation of CSPMFinder
#include "stdafx.h"
#include "AFValueFinders.h"
#include "SPMFinder.h"

#include <SpecialStringDefinitions.h>
#include <Common.h>
#include <UCLIDException.h>
#include <COMUtils.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <CommentedTextFileReader.h>
#include <StringTokenizer.h>
#include <Misc.h>
#include <AFTagManager.h>
#include <ComponentLicenseIDs.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 5;
const ESPMReturnMatchType eDEFAULT_RETURN_MATCH_TYPE = kReturnFirstMatch;

//-------------------------------------------------------------------------------------------------
// CSPMFinder
//-------------------------------------------------------------------------------------------------
CSPMFinder::CSPMFinder()
: m_bDirty(false),
  m_bIsPatternsFromFile(true),
  m_bCaseSensitive(false),
  m_bMultipleWSAsOne(true),
  m_bGreedySearch(false),
  m_bStoreRuleWorked(false),
  m_strRulesFileName(""),
  m_strRulesText(""),
  m_strRuleWorkedName(""),
  m_ipSPM(NULL),
  m_ipAFUtility(NULL),
  m_eReturnMatchType(eDEFAULT_RETURN_MATCH_TYPE),
  m_nMinScoreToConsiderAsMatch(0),
  m_nMinFirstToConsiderAsMatch(0),
  m_ipDataScorer(NULL),
  m_ipPreprocessors(NULL),
  m_bIgnoreInvalidTags(true)
{
}
//-------------------------------------------------------------------------------------------------
CSPMFinder::~CSPMFinder()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16349");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IAttributeFindingRule,
		&IID_ICategorizedComponent,
		&IID_ISPMFinder,
		&IID_ICopyableObject,
		&IID_IMustBeConfiguredObject,
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
STDMETHODIMP CSPMFinder::raw_ParseText(IAFDocument* pAFDoc, IProgressStatus *pProgressStatus,
									   IIUnknownVector **pAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		IAFDocumentPtr ipAFDoc(pAFDoc);
		ASSERT_ARGUMENT("ELI07059", ipAFDoc != __nullptr);

		if (m_ipSPM == __nullptr)
		{
			m_ipSPM.CreateInstance(CLSID_StringPatternMatcher);
			ASSERT_RESOURCE_ALLOCATION("ELI07028", m_ipSPM != __nullptr);
		}

		m_ipSPM->CaseSensitive = m_bCaseSensitive ? VARIANT_TRUE : VARIANT_FALSE;
		m_ipSPM->TreatMultipleWSAsOne = m_bMultipleWSAsOne ? VARIANT_TRUE : VARIANT_FALSE;

		// return vec of attributes
		IIUnknownVectorPtr ipRetAttributes(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI07061", ipRetAttributes != __nullptr);

		string strInput(m_strRulesFileName);
		if (m_bIsPatternsFromFile)
		{
			// get the actual file name based on the current input document type
			strInput = getInputFileName(m_strRulesFileName, ipAFDoc);
			
			// if the input file name is empty, it means that the
			// document type is not determined, or this document
			// is classifed as more than one document types.
			// Return an empty attribute vec to indicate that there's no attribute found
			// or the specified document type has no SPM finder rules associated with it
			if (strInput.empty())
			{
				*pAttributes = ipRetAttributes.Detach();
				return S_OK;
			}
			else if (!isFileOrFolderValid( strInput ))
			{
				// If a tag expanded to a file that doesn't exist we won't throw
				// an exception, we will just return no attributes
				UCLIDException ue("ELI07502", "Specified file not found for String Pattern Matcher rule.");
				ue.addDebugInfo("File", strInput);
				ue.log();
				*pAttributes = ipRetAttributes.Detach();
				return S_OK;
			}

			// load pattern file
			loadPatternFile(strInput);
		}
		// if input is directly from text panel
		else
		{
			strInput = m_strRulesText;
		}

		ISpatialStringPtr ipInputText = ipAFDoc->Text;
		ASSERT_RESOURCE_ALLOCATION("ELI07060", ipInputText != __nullptr);

		PatternFileInterpreter patternInterpreter;
		if (!m_bIsPatternsFromFile)
		{
			if (m_ipPreprocessors)
			{
				patternInterpreter.setPreprocessors(m_ipPreprocessors);
			}
			patternInterpreter.readPatterns(strInput, false, true);
		}
		else
		{
			// get the pattern interpreter from the map if any
			map<string, PatternFileInterpreter>::iterator itMap
				= m_mapFileNameToInterpreter.find(strInput);
			if (itMap == m_mapFileNameToInterpreter.end())
			{
				UCLIDException ue("ELI07164", "Failed to locate a PatternFileInterpreter for this pattern file.");
				ue.addDebugInfo("Pattern File Name", strInput);
				throw ue;
			}
			
			patternInterpreter = itMap->second;
		}
		
		// for each pattern string, look for match
		string strPatternID("");
		UCLID_AFVALUEFINDERSLib::ISPMFinderPtr ipThis(this);
		if (patternInterpreter.foundPattern(m_ipSPM, ipThis, ipInputText, 
			ipRetAttributes, strPatternID))
		{
			// store the rule that works in AFDocument if required
			if (m_bStoreRuleWorked)
			{
				IStrToStrMapPtr ipStrTags = ipAFDoc->StringTags;
				if (ipStrTags)
				{
					// Obtain the name for this rule
					string strRuleName(m_strRuleWorkedName);
					if (strRuleName.empty())
					{
						strRuleName = RULE_WORKED_TAG;
					}

					// Merge the new value into the map
					ipStrTags->MergeKeyValue(
						_bstr_t(strRuleName.c_str()), _bstr_t(strPatternID.c_str()), kAppend);
				}
			}
		}
		
		*pAttributes = ipRetAttributes.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07009");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ISPMFinder
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::get_IsPatternsFromFile(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bIsPatternsFromFile ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07020");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::put_IsPatternsFromFile(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bIsPatternsFromFile = newVal==VARIANT_TRUE;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07021");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::get_RulesFileName(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = _bstr_t(m_strRulesFileName.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07022");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::put_RulesFileName(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		// checking or not checking RDT license all depends
		// on the file type.
		string strFile = asString( newVal );
		string strFileExtension = ::getExtensionFromFullPath(strFile);
		if (strFileExtension != ".etf")
		{
			// if the input file is not of ETF file type
			validateRDTLicense();
		}
		else
		{
			autoEncryptFile(strFile, gstrAF_AUTO_ENCRYPT_KEY_PATH);
		}
		// make sure the file exists
		// or if the file name contains valid <DocType> strings
		if (getAFUtility()->StringContainsInvalidTags(strFile.c_str()) == VARIANT_TRUE)
		{
			UCLIDException ue("ELI07473", "The rules file contains invalid tags.");
			ue.addDebugInfo("File", strFile);
			throw ue;
		}
		else if (getAFUtility()->StringContainsTags(strFile.c_str()) == VARIANT_FALSE)
		{
			if (!isAbsolutePath(strFile))
			{
				UCLIDException ue("ELI07501", "Specification of a relative path to the RSD/ETF file is not allowed.");
				ue.addDebugInfo("File", strFile);
				throw ue;
			}
			else if (!isValidFile(strFile))
			{
				UCLIDException ue("ELI07474", "The specified rules file does not exist.");
				ue.addDebugInfo("File", strFile);
				ue.addWin32ErrorInfo();
				throw ue;
			}
		}

		m_strRulesFileName = strFile;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07023");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::get_RulesText(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = _bstr_t(m_strRulesText.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07024");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::put_RulesText(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		validateRDTLicense();

		string strText = asString( newVal );
		// make sure the text is empty
		if (strText.empty())
		{
			throw UCLIDException("ELI07040", "No rule is defined.");
		}

		m_strRulesText = strText;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07025");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::get_StoreRuleWorked(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bStoreRuleWorked ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07072");	

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::put_StoreRuleWorked(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bStoreRuleWorked = newVal==VARIANT_TRUE;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07073");	

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::get_RuleWorkedName(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_bStoreRuleWorked && m_strRuleWorkedName.empty())
		{
			// if empty, set to default value
			m_strRuleWorkedName = "RuleWorked";
		}

		*pVal = _bstr_t(m_strRuleWorkedName.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07026");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::put_RuleWorkedName(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		string strRuleWorkedName = asString( newVal );
		if (!strRuleWorkedName.empty())
		{
			m_strRuleWorkedName = strRuleWorkedName;
			m_bDirty = true;
		}		
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07027");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::get_CaseSensitive(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bCaseSensitive ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07029");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::put_CaseSensitive(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bCaseSensitive = newVal==VARIANT_TRUE;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07030");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::get_TreatMultipleWSAsOne(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bMultipleWSAsOne ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07031");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::put_TreatMultipleWSAsOne(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bMultipleWSAsOne = newVal==VARIANT_TRUE;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07032");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::get_GreedySearch(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bGreedySearch ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07053");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::put_GreedySearch(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bGreedySearch = newVal==VARIANT_TRUE;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07054");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::get_DataScorer(IObjectWithDescription **ppObj)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// if the DataScorer object-with-description object has not yet
		// been created, do so now..
		if (m_ipDataScorer == __nullptr)
		{
			m_ipDataScorer.CreateInstance(CLSID_ObjectWithDescription);
			ASSERT_RESOURCE_ALLOCATION("ELI08593", m_ipDataScorer != __nullptr);
		}

		CComQIPtr<IObjectWithDescription> ipObj = m_ipDataScorer;
		ipObj.CopyTo(ppObj);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08586");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::put_DataScorer(IObjectWithDescription *pObj)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// update the internal pointer to the data scorer
		m_ipDataScorer = pObj;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08587");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::get_MinScoreToConsiderAsMatch(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// return the value of the member variable
		*pVal = m_nMinScoreToConsiderAsMatch;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08588");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::put_MinScoreToConsiderAsMatch(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// ensure that the new minimum match score is in [0,100]
		if (newVal < 0 || newVal > 100)
		{
			UCLIDException ue("ELI08592", "Invalid score - score must be in the range 0 to 100.");
			ue.addDebugInfo("newVal", newVal);
			throw ue;
		}

		// update the member variable
		m_nMinScoreToConsiderAsMatch = newVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08589");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::get_ReturnMatchType(ESPMReturnMatchType *peVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*peVal = m_eReturnMatchType;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08590");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::put_ReturnMatchType(ESPMReturnMatchType eNewVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// ensure that eNewVal is valid
		if (eNewVal != kReturnBestMatch && eNewVal != kReturnFirstMatch &&
			eNewVal != kReturnAllMatches && eNewVal != kReturnFirstOrBest)
		{
			UCLIDException ue("ELI08620", "Invalid return match type.");
			ue.addDebugInfo("eNewVal", (unsigned long) eNewVal);
			throw ue;
		}

		m_eReturnMatchType = eNewVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08591");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::get_Preprocessors(IVariantVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_ipPreprocessors == __nullptr)
		{
			m_ipPreprocessors.CreateInstance(CLSID_VariantVector);
			ASSERT_RESOURCE_ALLOCATION("ELI08756", m_ipPreprocessors != __nullptr);
		}

		IVariantVectorPtr ipShallowCopy = m_ipPreprocessors;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08754");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::put_Preprocessors(IVariantVector *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_ipPreprocessors = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08755");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::get_MinFirstToConsiderAsMatch(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		*pVal = m_nMinFirstToConsiderAsMatch;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09027");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::put_MinFirstToConsiderAsMatch(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_nMinFirstToConsiderAsMatch = newVal;
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09028");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::get_IgnoreInvalidTags(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bIgnoreInvalidTags ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10135");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::put_IgnoreInvalidTags(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bIgnoreInvalidTags = newVal==VARIANT_TRUE;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10136");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::raw_IsConfigured(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		bool bPatternsOK = (m_bIsPatternsFromFile && !m_strRulesFileName.empty())
			|| (!m_bIsPatternsFromFile && !m_strRulesText.empty());

		*pbValue = bPatternsOK ? VARIANT_TRUE: VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07010");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19584", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("String pattern matcher finder").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07011")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CSPMFinder::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		UCLID_AFVALUEFINDERSLib::ISPMFinderPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI08259", ipSource!=NULL);

		m_bIsPatternsFromFile = (ipSource->GetIsPatternsFromFile()==VARIANT_TRUE) ? true : false;
		m_bCaseSensitive = (ipSource->GetCaseSensitive() == VARIANT_TRUE) ? true : false;
		m_bMultipleWSAsOne = (ipSource->GetTreatMultipleWSAsOne() == VARIANT_TRUE) ? true : false;
		m_bGreedySearch = (ipSource->GetGreedySearch() == VARIANT_TRUE) ? true : false;
		m_bStoreRuleWorked = (ipSource->GetStoreRuleWorked() == VARIANT_TRUE) ? true : false;
	
		if (m_bIsPatternsFromFile)
		{
			m_strRulesFileName = ipSource->GetRulesFileName();
		}
		else
		{
			m_strRulesText = ipSource->GetRulesText();
		}

		// copy the data scorer object
		m_ipDataScorer = __nullptr;
		m_ipDataScorer.CreateInstance(CLSID_ObjectWithDescription);
		ASSERT_RESOURCE_ALLOCATION("ELI08631", m_ipDataScorer != __nullptr);
		ICopyableObjectPtr ipCopyableObj = m_ipDataScorer;
		ASSERT_RESOURCE_ALLOCATION("ELI08630", ipCopyableObj != __nullptr);
		ipCopyableObj->CopyFrom(ipSource->DataScorer);

		// copy the return match type
		m_eReturnMatchType = (ESPMReturnMatchType) ipSource->ReturnMatchType;

		// copy the min-score to consider as match
		m_nMinScoreToConsiderAsMatch = ipSource->MinScoreToConsiderAsMatch;

		m_nMinFirstToConsiderAsMatch = ipSource->MinFirstToConsiderAsMatch;

		m_strRuleWorkedName = ipSource->GetRuleWorkedName();

		m_bIgnoreInvalidTags = ipSource->IgnoreInvalidTags == VARIANT_TRUE;

		// copy preprocessors
		m_ipPreprocessors = __nullptr;
		m_ipPreprocessors.CreateInstance(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI08759", m_ipPreprocessors != __nullptr);
		ipCopyableObj = m_ipPreprocessors;
		ASSERT_RESOURCE_ALLOCATION("ELI08760", ipCopyableObj != __nullptr);
		ipCopyableObj->CopyFrom(ipSource->Preprocessors);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08261");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_SPMFinder);
		ASSERT_RESOURCE_ALLOCATION("ELI08349", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07014");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_SPMFinder;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bIsPatternsFromFile = false;
		m_bCaseSensitive = false;
		m_bMultipleWSAsOne = true;
		m_bGreedySearch = false;
		m_bStoreRuleWorked = false;
		m_strRulesFileName = "";
		m_strRulesText = "";
		m_strRuleWorkedName = "";
		m_eReturnMatchType = eDEFAULT_RETURN_MATCH_TYPE;
		m_nMinScoreToConsiderAsMatch = 0;
		m_nMinFirstToConsiderAsMatch = 0;
		m_ipDataScorer = __nullptr; // delete the current data scorer object
		m_ipPreprocessors = __nullptr;
		
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
			UCLIDException ue( "ELI07647", "Unable to load newer SPM Finder." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		if (nDataVersion >= 1)
		{
			dataReader >> m_bIsPatternsFromFile;
			dataReader >> m_bCaseSensitive;
			dataReader >> m_bMultipleWSAsOne;
			dataReader >> m_bGreedySearch;
			dataReader >> m_bStoreRuleWorked;
			dataReader >> m_strRulesFileName;
			dataReader >> m_strRulesText;
			dataReader >> m_strRuleWorkedName;
		}

		if (nDataVersion >= 2)
		{
			// read the min-score-to-consider-as-match and the return match type
			dataReader >> m_nMinScoreToConsiderAsMatch;
			unsigned long ulTemp;
			dataReader >> ulTemp;
			m_eReturnMatchType = (ESPMReturnMatchType) ulTemp;
		}

		if (nDataVersion >= 4 )
		{
			dataReader >> m_nMinFirstToConsiderAsMatch;
		}

		if(nDataVersion >= 5 )
		{
			dataReader >> m_bIgnoreInvalidTags;
		}

		// read the data scorer object
		if (nDataVersion >= 2)
		{
			// read the data scorer object
			IPersistStreamPtr ipObj;
			::readObjectFromStream(ipObj, pStream, "ELI09965");
			ASSERT_RESOURCE_ALLOCATION("ELI08627", ipObj != __nullptr);
			m_ipDataScorer = ipObj;
		}

		if (nDataVersion >= 3)
		{
			// read the list of preprocessors
			IPersistStreamPtr ipObj;
			::readObjectFromStream(ipObj, pStream, "ELI09966");
			ASSERT_RESOURCE_ALLOCATION("ELI08757", ipObj != __nullptr);
			m_ipPreprocessors = ipObj;
		}

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07016");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		dataWriter << gnCurrentVersion;
		dataWriter << m_bIsPatternsFromFile;
		dataWriter << m_bCaseSensitive;
		dataWriter << m_bMultipleWSAsOne;
		dataWriter << m_bGreedySearch;
		dataWriter << m_bStoreRuleWorked;
		dataWriter << m_strRulesFileName;
		dataWriter << m_strRulesText;
		dataWriter << m_strRuleWorkedName;
		dataWriter << m_nMinScoreToConsiderAsMatch;
		dataWriter << (unsigned long) m_eReturnMatchType;
		dataWriter << m_nMinFirstToConsiderAsMatch;
		dataWriter << m_bIgnoreInvalidTags;
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);


		UCLID_AFVALUEFINDERSLib::ISPMFinderPtr ipThis = getThisAsCOMPtr();

		// Make sure DataScorer object-with-description exists
		IObjectWithDescriptionPtr ipObjWithDesc = ipThis->DataScorer;
		ASSERT_RESOURCE_ALLOCATION("ELI08624", ipObjWithDesc != __nullptr);
		
		// write the data-scorer object to the stream
		IPersistStreamPtr ipObj = ipObjWithDesc;
		if (ipObj == __nullptr)
		{
			throw UCLIDException("ELI08625", "DataScorer object does not support persistence.");
		}
		writeObjectToStream(ipObj, pStream, "ELI09920", fClearDirty);

		// Make sure preprcessors exists
		IVariantVectorPtr ipPreprocessors = ipThis->Preprocessors;
		
		// write the object to the stream
		ipObj = ipPreprocessors;
		if (ipObj == __nullptr)
		{
			throw UCLIDException("ELI08758", "VariantVector object does not support persistence.");
		}
		writeObjectToStream(ipObj, pStream, "ELI09921", fClearDirty);

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07015");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMFinder::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
UCLID_AFVALUEFINDERSLib::ISPMFinderPtr CSPMFinder::getThisAsCOMPtr()
{
	UCLID_AFVALUEFINDERSLib::ISPMFinderPtr ipThis(this);
	ASSERT_RESOURCE_ALLOCATION("ELI16968", ipThis != __nullptr);

	return ipThis;
}
//-------------------------------------------------------------------------------------------------
IAFUtilityPtr CSPMFinder::getAFUtility()
{
	if (m_ipAFUtility == __nullptr)
	{
		m_ipAFUtility.CreateInstance( CLSID_AFUtility );
		ASSERT_RESOURCE_ALLOCATION( "ELI07324", m_ipAFUtility != __nullptr );
	}
	
	return m_ipAFUtility;
}
//-------------------------------------------------------------------------------------------------
string CSPMFinder::getInputFileName(const string& strInputFile, IAFDocumentPtr ipAFDoc)
{	

	string strResult;
	try
	{
		strResult = strInputFile;
		AFTagManager tagMgr;
		strResult = tagMgr.expandTagsAndFunctions(strResult, ipAFDoc);
	}
	catch(...)
	{
		if(m_bIgnoreInvalidTags)
		{
			return string("");
		}
		throw;
	}
	// Get the appropriate prefixed file, if available
	strResult = asString(getAFUtility()->GetPrefixedFileName(_bstr_t(strResult.c_str())));
	
	return strResult;
}
//-------------------------------------------------------------------------------------------------
void CSPMFinder::loadPatternFile(const string& strPatternFileName)
{
	// look for the industry name entry
	map<string, PatternFileInterpreter>::iterator itMap 
		= m_mapFileNameToInterpreter.find(strPatternFileName);
	if (itMap != m_mapFileNameToInterpreter.end())
	{
		// if LoadPerSession is on, then only load the pattern
		// file once per session
		if (getAFUtility()->GetLoadFilePerSession() == VARIANT_TRUE)
		{	
			return;
		}
	}

	// create a new pattern file interpreter
	PatternFileInterpreter patternInterpreter;
	if (m_ipPreprocessors)
	{
		patternInterpreter.setPreprocessors(m_ipPreprocessors);
	}
	patternInterpreter.readPatterns(strPatternFileName, true, true);
	m_mapFileNameToInterpreter[strPatternFileName] = patternInterpreter;
}
//-------------------------------------------------------------------------------------------------
void CSPMFinder::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI07017", "String Pattern Matcher Finder");
}
//-------------------------------------------------------------------------------------------------
void CSPMFinder::validateRDTLicense()
{
	static const unsigned long COMP_RDT_ID = gnRULE_DEVELOPMENT_TOOLKIT_OBJECTS;

	VALIDATE_LICENSE(COMP_RDT_ID, "ELI07350", "SPMFinder - Pattern Reader");
}
//-------------------------------------------------------------------------------------------------
