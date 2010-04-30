// StringPatternMatcher.h : Declaration of the CStringPatternMatcher

#pragma once

#include "resource.h"       // main symbols
#include "SPMLiteralSearchData.h"
#include "SPMTokenInfo.h"

#include <string>
#include <vector>
#include <map>

#include <stringCSIS.h>

// NOTE: The syntax of the String Pattern matching rules is stored in SPM-Syntax.txt
// in the Req-Design folder

// forward declarations
class StringTokenizer;

/////////////////////////////////////////////////////////////////////////////
// CStringPatternMatcher
class ATL_NO_VTABLE CStringPatternMatcher : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CStringPatternMatcher, &CLSID_StringPatternMatcher>,
	public ISupportErrorInfo,
	public IDispatchImpl<IStringPatternMatcher, &IID_IStringPatternMatcher, &LIBID_UCLID_COMUTILSLib>
{
public:
	CStringPatternMatcher();
	~CStringPatternMatcher();

DECLARE_REGISTRY_RESOURCEID(IDR_STRINGPATTERNMATCHER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CStringPatternMatcher)
	COM_INTERFACE_ENTRY(IStringPatternMatcher)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IStringPatternMatcher
	STDMETHOD(get_CaseSensitive)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_CaseSensitive)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_TreatMultipleWSAsOne)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_TreatMultipleWSAsOne)(/*[in]*/ VARIANT_BOOL newVal);
	//---------------------------------------------------------------------------------------------
	// REQUIRE: strPattern must adhere to the syntax documented above.
	STDMETHOD(Match1)(/*[in]*/ BSTR strText, /*[in]*/ BSTR strPattern, 
		/*[in]*/ IStrToStrMap *pExprMap, /*[in]*/VARIANT_BOOL bGreedy,
		/*[out, retval]*/ IStrToObjectMap **pMatches);
	//---------------------------------------------------------------------------------------------
	// REQUIRE: strPattern must adhere to the syntax documented above.
	STDMETHOD(Match2)(/*[in]*/ BSTR strText, /*[in]*/ BSTR strPattern, 
		/*[in]*/ IStrToStrMap *pExprMap, /*[in]*/VARIANT_BOOL bGreedy,
		/*[in]*/ long *pnPatternStartPos, /*[in]*/ long *pnPatternEndPos, 
		/*[out, retval]*/ IStrToObjectMap **pMatches);
	//---------------------------------------------------------------------------------------------

private:
	// get the token type associated with strToken, and if the token type
	// involves an expression, return the expression name via
	// rstrExprName
	ETokenType analyzeToken(stringCSIS& rstrToken,
		stringCSIS& rstrExprName, unsigned long &rnMaxIgnoreChars);

	//---------------------------------------------------------------------------------------------
	// both Match1() and Match2() basically do the same thing....and they both
	// end up calling this method to do the real work.
	void match(BSTR bstrText, BSTR bstrPattern, IStrToStrMap *pExprMap, 
		VARIANT_BOOL bGreedy, long *pnPatternStartPos, long *pnPatternEndPos, 
		IStrToObjectMap **pMatches);
	//---------------------------------------------------------------------------------------------
	// PURPOSE:	Process each of the tokens in m_vecTokens, and store match information in 
	//			m_vecTokenInfo
	// REQUIRE: nProcessingStartPos >= 0 && nProcessingStartPos < m_strText.length();
	// PROMISE: To return true if the processing was successful and false otherwise.
	//			If true is returned, then the found matches are returned via ipMatches
	//			and the start and end position of the overall pattern are returned
	//			via nProcessingStartPos and rnPatternStartPos.
	//			If false is retuned, rbStopSearching will indicate if further
	//			searching should be stopped (i.e. no matter how we resume the search
	//			it's not going to be successful)
	bool processTokens(unsigned long nStartToken, 
		long nProcessingStartPos, long& rnPatternStartPos, long& rnPatternEndPos,
		bool& rbStopSearching);
	//---------------------------------------------------------------------------------------------
	// PURPOSE:	Process each of the tokens in m_vecTokens, and update the
	//			match variable values to adhere to the greedy specification
	//			that is uniquely specified for them (or using the default
	//			m_bGreedyByDefault flag if no unique setting is specified at
	//			tha match variable level)
	// PROMISE:	To iterate through each of the tokens that are match varibles
	//			and convert the match to adhere to the left/right greedy
	//			specification for that match variable.
	void convertMatchesToGreedySpecification();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To find the token specified by index nStartToken, by searching
	//			beginning at the position nProcessingStartPos.  If the token is
	//			found, the start and end position of the token will be returned
	//			via the 2 reference arguments and true will be returned.  If the
	//			token is not found, then false is returned.  If true is returned,
	//			then rbConstraintMet will indicate if any constraints associated
	//			with the token (such as the max distance from the end of the last
	//			token) were met.
	// NOTE:	nCompareStartPos is the position that is used to determine if the
	//			max-ignore-chars constraint was met.
	bool findToken(unsigned long nStartToken, long nProcessingStartPos, 
		long nCompareStartPos, long& rnThisTokenStartPos, 
		long& rnThisTokenEndPos, bool& rbConstraintMet);
	//---------------------------------------------------------------------------------------------
	// PURPOSE:	To trim the left side of the match variable's value to conform to
	//			the non-greedy specification.
	// REQUIRE:	nToken is the index of a match variable
	void performNonGreedyLeftTrim(long nToken);
	//---------------------------------------------------------------------------------------------
	// PURPOSE:	To extend the right side of the match variable's value to conform to
	//			the greedy specification.
	// REQUIRE:	nToken is the index of a match variable
	void performGreedyRightExtend(long nToken);
	//---------------------------------------------------------------------------------------------
	// PURPOSE:	To iterate through the internal m_vecTokens and determine the
	//			matches and return the match information via the Token COM objects.
	UCLID_COMUTILSLib::IStrToObjectMapPtr getMatches() const;
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Methods to process the various types of tokens
	// PROMISE: To return true/false depending upon whether the token's representation in
	//			m_strText was found.  If the token's representation could not be 
	//			found in false is returned.  If the token's representation is found,
	//			true is returned and rnTokenStartPos and rnTokenEndPos are updated with the
	//			token position.  All searches for token representations are 
	//			executed beginning at nStartSearchPos
	bool processLiteralOrList(const stringCSIS& strPattern, 
		long nStartSearchPos, long& rnTokenStartPos, long& rnTokenEndPos);
	bool processCharMustMatchToken(long nToken, long nStartSearchPos, 
		long& rnTokenStartPos, long& rnTokenEndPos) const;
	bool processCharMustNotMatchToken(long nToken, long nStartSearchPos, 
		long& rnTokenStartPos, long& rnTokenEndPos) const;
	bool processExpressionToken(long nToken, long nStartSearchPos, 
		long& rnTokenStartPos, long& rnTokenEndPos);
	//---------------------------------------------------------------------------------------------
	// PROMISE: To return true if cChar is either '[' or ']'
	bool isWordBoundaryChar(char cChar) const;
	//---------------------------------------------------------------------------------------------
	// PROMISE: To return true if cChar is any whitespace character or
	//			one of several other characters (like periods, commas, semicolons,
	//			parenthesis, etc) which indicate word boundary.
	bool isWordBoundaryConstraintChar(char cChar) const;
	//---------------------------------------------------------------------------------------------
	// REQUIRE: vecTokens contains the tokens from the pattern string
	//			This method also assumes that m_mapExpressions contains all valid
	//			expression names/values.
	// PROMISE: To not throw any exceptions if all the tokens available from
	//			pTokenizer are valid tokens with regards the pattern string
	void validateTokens(const std::vector<std::string>& vecTokens);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To find the next instance of a literal, where a literal is defined
	//			as a string that needs to be found with optional word-boundary-required
	//			indicating characters at the beginning or end
	// PROMISE:	To return the first position in m_strText beginning at nStartSearchPos
	//			where strLiteral's representation is found.  If strLiteral's 
	//			representation is not found in m_strText beginning at nStartSearchPos,
	//			then false will be returned.
	//			If the search was successful, then true will be returned, and rnTokenStartPos and
	//			rnTokenEndPos will be updated to the positions representing the found match.
	bool findNextInstanceOfLiteral(stringCSIS strLiteral, long nStartSearchPos, 
		long& rnTokenStartPos, long& rnTokenEndPos);
	//---------------------------------------------------------------------------------------------
	// PURPOSE:	To find the next instance of a literal, but taking the 
	//			'TreatMultipleWSAsOne' attribute value into account.
	// PROMISE: If strLiteral has no spaces or if TreatMultipleWSAsOne == False, then
	//			this function will behave just like findNextInstanceOfLiteral().
	//			If strLiteral has spaces in it, and TreatMultipleWSAsOne == True,
	//			then this function will behave just like findNextInstanceOfLiteral()
	//			with the exception of the fact that any space character in strLiteral
	//			will be attempted for matching with a sequence of whitespace chars
	//			in the original input string.
	// NOTE:	MWS = Multiple White Space
	bool findNextInstanceOfLiteralIgnoreMWS(const stringCSIS& strLiteral, long nStartSearchPos, 
		long& rnTokenStartPos, long& rnTokenEndPos);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To find the next instance of a one of the literals in a literal or-list.
	//			A literal is defined as a string that needs to be found with 
	//			optional word-boundary-required indicating characters at the 
	//			beginning or end.
	// REQUIRE: Literals in a literal or-list are required to be separated by 
	//			the | (pipe) character.
	//			There must be at least two literals in strLiteralOrList
	// PROMISE:	To return the earliest position in m_strText beginning at nStartSearchPos
	//			where the representation of one of the literals in the literal-or-list 
	//			is found.  If no such match was found, then false will be returned.
	//			If the search was successful, then true is returned, and rnTokenStartPos and 
	//			rnTokenEndPos will be updated to the position representing the found match.
	bool findNextMatchForLiteralOrList(const stringCSIS& strLiteralOrList, long nStartSearchPos, 
		long& rnTokenStartPos, long& rnTokenEndPos);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Validate different parts of the pattern string
	// PROMISE: If the data passed into these methods is not valid, an exception
	//			will be thrown with the relevant information
	void validateLiteralOrList(const stringCSIS& strLiteralOrList,
		const stringCSIS& strToken, const int iTokenNum);
	void validateExpressionName(const stringCSIS& strExpressionName,
		const stringCSIS& strToken, const int iTokenNum);
	void validateCharList(const stringCSIS& strCharList,
		const stringCSIS& strToken, const int iTokenNum);
	//---------------------------------------------------------------------------------------------
	// PROMISE: To return true only if one of the characters in strCharsToCheck
	//			is found in strInput
	bool stringContainsCharacters(const stringCSIS& strInput, 
		const stringCSIS& strCharsToCheck);
	//---------------------------------------------------------------------------------------------
	// PROMISE: To throw an exception if this component is not licensed.
	void validateLicense();
	//---------------------------------------------------------------------------------------------

	// the current text that is being searched to find matches associated
	// with a pattern
	// NOTE: we are using stringCSIS here so that we can specify case-sensitivy
	// when we do our searches
	stringCSIS m_strText;

	// the token in the input pattern that is currently being processed.
	stringCSIS m_strCurrentToken;

	// if the current token is of a type that is associated with an
	// expression, the name of the current expression is stored in this variable
	stringCSIS m_strCurrentExprName;

	// the names of all match variables in the input pattern
	// is stored in the following vector.  The leading ? is
	// not part of the variable name stored in this vector
	std::vector<SPMTokenInfo> m_vecTokenInfo;

	// cache # of tokens so that it doesn't need to be queried again and again
	unsigned long m_ulNumTokens;

	// the positions of the start of the first token and the end of the
	// last token (i.e. the start/end positions of the pattern)
	long m_nFirstTokenStartPos, m_nLastTokenEndPos;

	// vector to store the match results as we find them
	UCLID_COMUTILSLib::IIUnknownVectorPtr m_ipMatches;

	// map that stores the expression value for each named expression
	std::map<stringCSIS, stringCSIS> m_mapExpressions;

	// flag to indicate whether the comparisons are done
	// in a case-sensitive way
	bool m_bCaseSensitive;

	// flag to indicate whether each space in the pattern
	// represents a sequence of whitespace chars in the orignal input string.
	// If this flag is set to false, then each space in the pattern represents
	// exactly a space character in the input string.
	bool m_TreatMultipleWSAsOne;

	// we keep track of the last input for optimization purposes.  If the
	// input has not changed, and we are re-searching tokens that we have
	// already searched before, then we just look up the match results
	// from the search results in memory
	stringCSIS m_strLastInput;

	// keep track of at which positions we find what literals in the 
	// document so that we don't keep re-searching it as different
	// patterns are executed on the same document
	std::map<stringCSIS, SPMLiteralSearchData> m_mapLiteralToSearchData;

	// the following flag indicates if the search is greedy
	// by default.  If value is false, it indicates that search
	// is non-greedy by default
	bool m_bGreedyByDefault;

	// semaphore object to ensure that this object is not accessed simultaneously
	// from multiple threads
	CMutex m_lock;
};
