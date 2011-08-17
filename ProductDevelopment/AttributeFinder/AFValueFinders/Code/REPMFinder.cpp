// REPMFinder.cpp : Implementation of CREPMFinder
#include "stdafx.h"
#include "AFValueFinders.h"
#include "REPMFinder.h"

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
const unsigned long gnCurrentVersion = 1;
const EPMReturnMatchType eDEFAULT_RETURN_MATCH_TYPE = kReturnFirstMatch;

//-------------------------------------------------------------------------------------------------
// CREPMFinder
//-------------------------------------------------------------------------------------------------
CREPMFinder::CREPMFinder()
: m_bDirty(false),
  m_bCaseSensitive(false),
  m_bStoreRuleWorked(false),
  m_strRulesFileName(""),
  m_strRuleWorkedName(""),
  m_ipMiscUtils(__nullptr),
  m_ipRegExpParser(__nullptr),
  m_ipAFUtility(__nullptr),
  m_eReturnMatchType(eDEFAULT_RETURN_MATCH_TYPE),
  m_nMinScoreToConsiderAsMatch(0),
  m_nMinFirstToConsiderAsMatch(0),
  m_ipDataScorer(NULL),
  m_bIgnoreInvalidTags(true)
{
}
//-------------------------------------------------------------------------------------------------
CREPMFinder::~CREPMFinder()
{
	try
	{
		m_ipMiscUtils = __nullptr;
		m_ipRegExpParser = __nullptr;
		m_ipAFUtility = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI33222");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CREPMFinder::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IAttributeFindingRule,
		&IID_ICategorizedComponent,
		&IID_IREPMFinder,
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
STDMETHODIMP CREPMFinder::raw_ParseText(IAFDocument* pAFDoc, IProgressStatus *pProgressStatus,
									   IIUnknownVector **pAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		IAFDocumentPtr ipAFDoc(pAFDoc);
		ASSERT_ARGUMENT("ELI33223", ipAFDoc != __nullptr);

		getRegExParser()->IgnoreCase = m_bCaseSensitive ? VARIANT_FALSE : VARIANT_TRUE;

		// return vec of attributes
		IIUnknownVectorPtr ipRetAttributes(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI33225", ipRetAttributes != __nullptr);

		// get the actual file name based on the current input document type
		string strInput = getInputFileName(m_strRulesFileName, ipAFDoc);
			
		// if the input file name is empty, it means that the
		// document type is not determined, or this document
		// is classifed as more than one document types.
		// Return an empty attribute vec to indicate that there's no attribute found
		// or the specified document type has no REPM finder rules associated with it
		if (strInput.empty())
		{
			*pAttributes = ipRetAttributes.Detach();
			return S_OK;
		}
		else if (!isFileOrFolderValid( strInput ))
		{
			// If a tag expanded to a file that doesn't exist we won't throw
			// an exception, we will just return no attributes
			UCLIDException ue("ELI33226",
				"Specified file not found for Regular Expression Pattern Matcher rule.");
			ue.addDebugInfo("File", strInput);
			ue.log();
			*pAttributes = ipRetAttributes.Detach();
			return S_OK;
		}

		// load pattern file
		loadPatternFile(strInput);

		ISpatialStringPtr ipInputText = ipAFDoc->Text;
		ASSERT_RESOURCE_ALLOCATION("ELI33227", ipInputText != __nullptr);

		RegExPatternFileInterpreter patternInterpreter;

		// get the pattern interpreter from the map if any
		map<string, RegExPatternFileInterpreter>::iterator itMap
			= m_mapFileNameToInterpreter.find(strInput);
		if (itMap == m_mapFileNameToInterpreter.end())
		{
			UCLIDException ue("ELI33228",
				"Failed to locate a RegExPatternFileInterpreter for this pattern file.");
			ue.addDebugInfo("Pattern File Name", strInput);
			throw ue;
		}
			
		patternInterpreter = itMap->second;
		
		// for each pattern string, look for match
		string strPatternID("");
		UCLID_AFVALUEFINDERSLib::IREPMFinderPtr ipThis(this);
		if (patternInterpreter.foundPattern(getRegExParser(), ipThis, ipInputText,
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33229");
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IREPMFinder
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CREPMFinder::get_RulesFileName(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = _bstr_t(m_strRulesFileName.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33232");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CREPMFinder::put_RulesFileName(BSTR newVal)
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
			UCLIDException ue("ELI33233", "The rules file contains invalid tags.");
			ue.addDebugInfo("File", strFile);
			throw ue;
		}
		else if (getAFUtility()->StringContainsTags(strFile.c_str()) == VARIANT_FALSE)
		{
			if (!isAbsolutePath(strFile))
			{
				UCLIDException ue("ELI33234", "Specification of a relative path to the RSD/ETF file is not allowed.");
				ue.addDebugInfo("File", strFile);
				throw ue;
			}
			else if (!isValidFile(strFile))
			{
				UCLIDException ue("ELI33235", "The specified rules file does not exist.");
				ue.addDebugInfo("File", strFile);
				ue.addWin32ErrorInfo();
				throw ue;
			}
		}

		m_strRulesFileName = strFile;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33236");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CREPMFinder::get_StoreRuleWorked(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bStoreRuleWorked ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33240");	

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CREPMFinder::put_StoreRuleWorked(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bStoreRuleWorked = newVal==VARIANT_TRUE;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33241");	

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CREPMFinder::get_RuleWorkedName(BSTR *pVal)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33242");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CREPMFinder::put_RuleWorkedName(BSTR newVal)
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33243");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CREPMFinder::get_CaseSensitive(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bCaseSensitive ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33244");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CREPMFinder::put_CaseSensitive(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bCaseSensitive = newVal==VARIANT_TRUE;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33245");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CREPMFinder::get_DataScorer(IObjectWithDescription **ppObj)
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
			ASSERT_RESOURCE_ALLOCATION("ELI33250", m_ipDataScorer != __nullptr);
		}

		CComQIPtr<IObjectWithDescription> ipObj = m_ipDataScorer;
		ipObj.CopyTo(ppObj);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33251");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CREPMFinder::put_DataScorer(IObjectWithDescription *pObj)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// update the internal pointer to the data scorer
		m_ipDataScorer = pObj;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33252");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CREPMFinder::get_MinScoreToConsiderAsMatch(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// return the value of the member variable
		*pVal = m_nMinScoreToConsiderAsMatch;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33253");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CREPMFinder::put_MinScoreToConsiderAsMatch(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// ensure that the new minimum match score is in [0,100]
		if (newVal < 0 || newVal > 100)
		{
			UCLIDException ue("ELI33254", "Invalid score - score must be in the range 0 to 100.");
			ue.addDebugInfo("newVal", newVal);
			throw ue;
		}

		// update the member variable
		m_nMinScoreToConsiderAsMatch = newVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33255");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CREPMFinder::get_ReturnMatchType(EPMReturnMatchType *peVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*peVal = m_eReturnMatchType;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33256");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CREPMFinder::put_ReturnMatchType(EPMReturnMatchType eNewVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// ensure that eNewVal is valid
		if (eNewVal != kReturnBestMatch && eNewVal != kReturnFirstMatch &&
			eNewVal != kReturnAllMatches && eNewVal != kReturnFirstOrBest)
		{
			UCLIDException ue("ELI33257", "Invalid return match type.");
			ue.addDebugInfo("eNewVal", (unsigned long) eNewVal);
			throw ue;
		}

		m_eReturnMatchType = eNewVal;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33258");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CREPMFinder::get_MinFirstToConsiderAsMatch(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		*pVal = m_nMinFirstToConsiderAsMatch;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33262");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CREPMFinder::put_MinFirstToConsiderAsMatch(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_nMinFirstToConsiderAsMatch = newVal;
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33263");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CREPMFinder::get_IgnoreInvalidTags(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bIgnoreInvalidTags ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33264");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CREPMFinder::put_IgnoreInvalidTags(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bIgnoreInvalidTags = newVal==VARIANT_TRUE;
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33265");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CREPMFinder::raw_IsConfigured(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		bool bPatternsOK = !m_strRulesFileName.empty();

		*pbValue = bPatternsOK ? VARIANT_TRUE: VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33266");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CREPMFinder::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI33267", pstrComponentDescription != __nullptr)

		*pstrComponentDescription = _bstr_t("Regular expression pattern matcher finder").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33268")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CREPMFinder::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CREPMFinder::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		UCLID_AFVALUEFINDERSLib::IREPMFinderPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI33269", ipSource!=NULL);

		m_bCaseSensitive = (ipSource->GetCaseSensitive() == VARIANT_TRUE) ? true : false;
		m_bStoreRuleWorked = (ipSource->GetStoreRuleWorked() == VARIANT_TRUE) ? true : false;

		m_strRulesFileName = ipSource->GetRulesFileName();

		// copy the data scorer object
		m_ipDataScorer = __nullptr;
		m_ipDataScorer.CreateInstance(CLSID_ObjectWithDescription);
		ASSERT_RESOURCE_ALLOCATION("ELI33270", m_ipDataScorer != __nullptr);
		ICopyableObjectPtr ipCopyableObj = m_ipDataScorer;
		ASSERT_RESOURCE_ALLOCATION("ELI33271", ipCopyableObj != __nullptr);
		ipCopyableObj->CopyFrom(ipSource->DataScorer);

		// copy the return match type
		m_eReturnMatchType = (EPMReturnMatchType) ipSource->ReturnMatchType;

		// copy the min-score to consider as match
		m_nMinScoreToConsiderAsMatch = ipSource->MinScoreToConsiderAsMatch;

		m_nMinFirstToConsiderAsMatch = ipSource->MinFirstToConsiderAsMatch;

		m_strRuleWorkedName = ipSource->GetRuleWorkedName();

		m_bIgnoreInvalidTags = ipSource->IgnoreInvalidTags == VARIANT_TRUE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33274");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CREPMFinder::raw_Clone(IUnknown* *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ICopyableObjectPtr ipObjCopy(CLSID_REPMFinder);
		ASSERT_RESOURCE_ALLOCATION("ELI33275", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// return the new variant vector to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33276");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CREPMFinder::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_REPMFinder;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CREPMFinder::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CREPMFinder::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bCaseSensitive = false;
		m_bStoreRuleWorked = false;
		m_strRulesFileName = "";
		m_strRuleWorkedName = "";
		m_eReturnMatchType = eDEFAULT_RETURN_MATCH_TYPE;
		m_nMinScoreToConsiderAsMatch = 0;
		m_nMinFirstToConsiderAsMatch = 0;
		m_ipDataScorer = __nullptr; // delete the current data scorer object
		
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
			UCLIDException ue( "ELI33277", "Unable to load newer REPM Finder." );
			ue.addDebugInfo( "Current Version", gnCurrentVersion );
			ue.addDebugInfo( "Version to Load", nDataVersion );
			throw ue;
		}

		dataReader >> m_bCaseSensitive;
		dataReader >> m_bStoreRuleWorked;
		dataReader >> m_strRulesFileName;
		dataReader >> m_strRuleWorkedName;

		// read the min-score-to-consider-as-match and the return match type
		dataReader >> m_nMinScoreToConsiderAsMatch;
		unsigned long ulTemp;
		dataReader >> ulTemp;
		m_eReturnMatchType = (EPMReturnMatchType) ulTemp;

		dataReader >> m_nMinFirstToConsiderAsMatch;
		dataReader >> m_bIgnoreInvalidTags;

		// read the data scorer object
		IPersistStreamPtr ipObj;
		::readObjectFromStream(ipObj, pStream, "ELI33278");
		ASSERT_RESOURCE_ALLOCATION("ELI33279", ipObj != __nullptr);
		m_ipDataScorer = ipObj;

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33282");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CREPMFinder::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		dataWriter << gnCurrentVersion;
		dataWriter << m_bCaseSensitive;
		dataWriter << m_bStoreRuleWorked;
		dataWriter << m_strRulesFileName;
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

		UCLID_AFVALUEFINDERSLib::IREPMFinderPtr ipThis = getThisAsCOMPtr();

		// Make sure DataScorer object-with-description exists
		IObjectWithDescriptionPtr ipObjWithDesc = ipThis->DataScorer;
		ASSERT_RESOURCE_ALLOCATION("ELI33283", ipObjWithDesc != __nullptr);
		
		// write the data-scorer object to the stream
		IPersistStreamPtr ipObj = ipObjWithDesc;
		if (ipObj == __nullptr)
		{
			throw UCLIDException("ELI33284", "DataScorer object does not support persistence.");
		}
		writeObjectToStream(ipObj, pStream, "ELI33285", fClearDirty);

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33288");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CREPMFinder::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
UCLID_AFVALUEFINDERSLib::IREPMFinderPtr CREPMFinder::getThisAsCOMPtr()
{
	UCLID_AFVALUEFINDERSLib::IREPMFinderPtr ipThis(this);
	ASSERT_RESOURCE_ALLOCATION("ELI33289", ipThis != __nullptr);

	return ipThis;
}
//-------------------------------------------------------------------------------------------------
IAFUtilityPtr CREPMFinder::getAFUtility()
{
	if (m_ipAFUtility == __nullptr)
	{
		m_ipAFUtility.CreateInstance( CLSID_AFUtility );
		ASSERT_RESOURCE_ALLOCATION( "ELI33290", m_ipAFUtility != __nullptr );
	}
	
	return m_ipAFUtility;
}
//-------------------------------------------------------------------------------------------------
IRegularExprParserPtr CREPMFinder::getRegExParser()
{
	if (m_ipMiscUtils == __nullptr)
	{
		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI33353", m_ipMiscUtils != __nullptr);
	}

	if (m_ipRegExpParser == __nullptr)
	{
		m_ipRegExpParser = m_ipMiscUtils->GetNewRegExpParserInstance("REPMFinder");
		ASSERT_RESOURCE_ALLOCATION("ELI33224", m_ipRegExpParser != __nullptr);
	}

	return m_ipRegExpParser;
}
//-------------------------------------------------------------------------------------------------
string CREPMFinder::getInputFileName(const string& strInputFile, IAFDocumentPtr ipAFDoc)
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
void CREPMFinder::loadPatternFile(const string& strPatternFileName)
{
	// look for the industry name entry
	map<string, RegExPatternFileInterpreter>::iterator itMap 
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
	RegExPatternFileInterpreter patternInterpreter;
	patternInterpreter.readPatterns(strPatternFileName, true);
	m_mapFileNameToInterpreter[strPatternFileName] = patternInterpreter;
}
//-------------------------------------------------------------------------------------------------
void CREPMFinder::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI33291", "Regular Expression Pattern Matcher Finder");
}
//-------------------------------------------------------------------------------------------------
void CREPMFinder::validateRDTLicense()
{
	static const unsigned long COMP_RDT_ID = gnRULE_DEVELOPMENT_TOOLKIT_OBJECTS;

	VALIDATE_LICENSE(COMP_RDT_ID, "ELI33292", "REPMFinder - Pattern Reader");
}
//-------------------------------------------------------------------------------------------------
