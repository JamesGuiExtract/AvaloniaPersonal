// EntityFinder.h : Declaration of the CEntityFinder

#pragma once

#include "resource.h"       // main symbols

#include "EntityFinderConfigMgr.h"
#include "..\..\AFCore\Code\AFCategories.h"

#include <common.h>
#include <CachedObjectFromFile.h>
#include <RegExLoader.h>

#include <string>

using namespace std;

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
const string gstrAFUTILS_KEY_PATH = gstrAF_REG_ROOT_FOLDER_PATH + "\\AFUtils";

/////////////////////////////////////////////////////////////////////////////
// CEntityFinder
class ATL_NO_VTABLE CEntityFinder : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CEntityFinder, &CLSID_EntityFinder>,
	public ISupportErrorInfo,
	public IDispatchImpl<IAttributeModifyingRule, &IID_IAttributeModifyingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IEntityFinder, &IID_IEntityFinder, &LIBID_UCLID_AFUTILSLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IPersistStream
{
public:
	CEntityFinder();
	~CEntityFinder();

DECLARE_REGISTRY_RESOURCEID(IDR_ENTITYFINDER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CEntityFinder)
	COM_INTERFACE_ENTRY(IEntityFinder)
	COM_INTERFACE_ENTRY2(IDispatch,IEntityFinder)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IAttributeModifyingRule)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IPersistStream)
END_COM_MAP()

BEGIN_CATEGORY_MAP(CEntityFinder)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_MODIFIERS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IEntityFinder
	STDMETHOD(FindEntities)(ISpatialString* pText);
	STDMETHOD(FindEntitiesInAttributes)(/*[in]*/ IIUnknownVector* pAttributes);

// IAttributeModifyingRule
	STDMETHOD(raw_ModifyValue)(IAttribute * pAttribute, IAFDocument* pOriginInput,
		IProgressStatus *pProgressStatus);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

private:

	///////
	// Data
	///////
	// Manages keywords associated with identifying entities
	UCLID_AFUTILSLib::IEntityKeywordsPtr	m_ipKeys;

	// Misc utils pointer used to get a regular expression parser
	IMiscUtilsPtr m_ipMiscUtils;

	// Handles configuration persistence
	unique_ptr<IConfigurationSettingsPersistenceMgr> ma_pUserCfgMgr;
	unique_ptr<EntityFinderConfigMgr> ma_pEFConfigMgr;

	// Use CachedObjectFromFile so that the regular expression is re-loaded from disk only when the
	// RegEx file is modified.
	CachedObjectFromFile<string, RegExLoader> m_cachedRegExLoader;

	// Logging enabled flag
	bool	m_bLoggingEnabled;

	bool m_bDirty;

	//////////
	// Methods
	//////////
	// Performs entity finding
	void findEntities(const ISpatialStringPtr& ipText);
	//----------------------------------------------------------------------------------------------
	// Returns left-hand portion of string after intelligent trimming of blank lines
	string	doBlankLineTrimming(string strInput, IRegularExprParserPtr ipParser, long lKeywordEndPos, 
		bool bFoundTrust);
	//----------------------------------------------------------------------------------------------
	// Returns upper-case substring having period delimiter
	string	doCompanyPostProcessing(const string& strInput, long lSuffixStart, 
		long lSuffixStop, bool bAliasFound);
	//----------------------------------------------------------------------------------------------
	// Returns right-hand portion of string without any leading parentheses
	string	doGeneralTrimming(string strInput, bool bPersonFound, IRegularExprParserPtr ipParser);
	//----------------------------------------------------------------------------------------------
	// Trims string after TrustIndicator if followed by "DATED", or other TrustDates expression.
	// Returns true if TrustIndicator found, false otherwise.
	bool	doTrustTrimming(ISpatialStringPtr &ripSpatial, IRegularExprParserPtr ipParser);
	//----------------------------------------------------------------------------------------------
	// Checks ipText starting at lStartPos for Company-related keywords.
	// Alias information will be included as long as a subsequent Entity is found
	bool	findCompanyEnd(ISpatialStringPtr ipText, IRegularExprParserPtr ipParser,
		long lStartPos, long *plSuffixStart, long *plSuffixEnd, long *plEndPos,
		bool *pbFoundSuffix, bool *pbFoundAlias);
	//----------------------------------------------------------------------------------------------
	// Searches strText from specified starting character for the first word containing either
	// one or more upper-case characters OR all lower-case characters.  A word containing a 
	// digit can be either accepted or rejected, as desired
	// The word "of" is acceptable if bIsCompany == true
	long	findFirstCaseWord(const string& strText, int iStartPos, bool bUpperCase,
		bool bAcceptDigit, bool bIsCompany);
	//----------------------------------------------------------------------------------------------
	// Searches strText from specified starting character for the first LC word that should 
	// be trimmed.  "and" and "&" are accepted as separators between otherwise UC words.
	// NOTE: Neither "and" nor "&" will be accepted as trailing LC words
	// If bCompany == true, "of" is an acceptable separator
	long	findFirstLowerCaseWordToTrim(const string& strText, IRegularExprParserPtr ipParser,
		int iStartPos, bool bIsCompany);
	//----------------------------------------------------------------------------------------------
	// Searches strText from lStart, checking to see if the next word is a valid
	// separator.  Returns end position of separator, if found, otherwise -1.
	long	findSeparatorWordEnd(const string& strText, IRegularExprParserPtr ipParser,
		long lStart, bool bIsCompany);
	//----------------------------------------------------------------------------------------------
	// Searches strText1 and strText2 to find a keyword phrase that crosses from one 
	// to the other.
	bool	foundKeywordPhraseOverlap(const string& strText1, const string& strText2,
		IRegularExprParserPtr ipParser);
	//----------------------------------------------------------------------------------------------
	// Provide long, involved Address pattern as string
	string	getAddressSuffixPattern();
	//----------------------------------------------------------------------------------------------
	// Parses strText with whitespace.  Returns number of words found
	long	getWordCount(const string& strText);
	//----------------------------------------------------------------------------------------------
	// Removes paired parentheses or square brackets.  Embedded text is retained if 
	// it contains a Person Designator or Alias.  Otherwise, embedded text is removed
	// Single parentheses or brackets are replaced with spaces.
	void	handleParentheses(ISpatialStringPtr &ripText, IRegularExprParserPtr ipParser);
	//----------------------------------------------------------------------------------------------
	// Trims string after TRUST if finds TRUST DATED or TRUST DTD
	string	handleTrustDated(string strInput);
	//----------------------------------------------------------------------------------------------
	// Checks strText to see if it contains a date string
	bool	hasDateText(const string& strText, IRegularExprParserPtr ipParser);
	//----------------------------------------------------------------------------------------------
	// Checks strWord to see if it contains only punctuation characters
	bool	hasOnlyPunctuation(const string& strWord);
	//----------------------------------------------------------------------------------------------
	// Checks strWord to see if it has one or more upper-case characters.  Digit characters 
	// can be accepted or rejected, as desired
	bool	hasUpperCaseLetter(const string& strWord, bool bAcceptDigit);
	//----------------------------------------------------------------------------------------------
	// Checks strWord to see if it contains only upper-case characters and periods
	bool	isAbbreviation(const string& strWord);
	//----------------------------------------------------------------------------------------------
	// Checks to see if strText exactly matches a keyword phrase
	//   i.e. PersonDesignator or PersonTrimIdentifier
	bool	isKeywordPhrase(const string& strText, IRegularExprParserPtr ipParser);
	//----------------------------------------------------------------------------------------------
	// Logs initial and final strings to log file
	void	logResults(string strInitial, string strFinal);
	//----------------------------------------------------------------------------------------------
	// Searches ipText looking for two or more upper case letters followed by "and " or 
	// " and" followed by two or more upper case letters.  Returns the position at 
	// which to add a space or -1 if not found
	long	makeSpaceForAnd(ISpatialStringPtr ipText, IRegularExprParserPtr ipParser);
	//----------------------------------------------------------------------------------------------
	// Returns first character to be retained after trimming leading digits-only word(s)
	// If return value is zero, no trimming is needed.
	long	removeFirstDigitsWords(const string& strInput, IRegularExprParserPtr ipParser);
	//----------------------------------------------------------------------------------------------
	// look for consecutive strChars in the input string and eliminate rudundant ones
	// for example, input string is "ABBBC" and strChars is "B", then "ABC" will be returned
	string consolidateMultipleCharsIntoOne(const string& strInput, const string& strChars);
	//----------------------------------------------------------------------------------------------
	// Returns position at which to trim strInput.  Trim indications include:
	//   lower-case word, punctuation, "DATED", "UNDER"
	// If return value is zero, no trimming is needed.
	// REQUIRES: lSearchStart > 0
	long trimAfterTrust(const string& strInput, long lSearchStart);
	//----------------------------------------------------------------------------------------------
	// Trims leading non-entity text including:
	//   Non-alphanumeric characters
	//   Lower-case words
	//   Digits-only words
	//   Various nonsense words and strings from next method
	string trimLeadingNonsense(string strInput, IRegularExprParserPtr ipParser);
	//----------------------------------------------------------------------------------------------
	// Removes any embedded Address from ripText.  Returns true if found, otherwise
	// false.
	bool removeAddressText(const ISpatialStringPtr& ripText, IRegularExprParserPtr ipParser);
	//----------------------------------------------------------------------------------------------
	// Checks license state
	void	validateLicense();
};
