
#pragma once

#include <stringCSIS.h>

// enum of different types of tokens used by the syntax of the
// pattern string passed to the Match() method
enum ETokenType
{
	kInvalidTokenType,
	kLiteralOrList,			// "grant|assign|convey" or "grant"
	kDesiredMatch,			// "?MatchVariableName"
	kCharMustMatch,			// "@+Digits"
	kCharMustNotMatch,		// "@-NonTerminatingChars"
	kExpression				// "@GrantKeywords"
};

struct SPMTokenInfo
{
	//---------------------------------------------------------------------------------------------
	// ctor, copy ctor, and assignment operator
	SPMTokenInfo();
	SPMTokenInfo(const SPMTokenInfo& ti);
	SPMTokenInfo& operator=(const SPMTokenInfo& ti);
	//---------------------------- -----------------------------------------------------------------
	// PROMISE: To return true if m_eTokenType == kMatchVariable
	bool isMatchVariable() const;
	//---------------------------------------------------------------------------------------------

	stringCSIS m_strToken;

	// this enum will keep track of the token type
	ETokenType m_eTokenType;

	// this variable will keep track of the expression associated with
	// a token.  This could be the name of a mapped expression (such as
	// the NonTP in @NonTP, or the name of a variable such as Grantor
	// in ?Grantor
	stringCSIS m_strExprOrVariableName;
	
	// the maximum chars that can be ignored before this
	// token can be considered as a match
	unsigned long m_ulMaxIgnoreChars;

	long m_nMatchStartPos, m_nMatchEndPos;

	// flags to indicate whether the match variable is
	// greedy or non-greedy at the left and right sides
	// REQUIRE: These flags must be read only if isMatchVariable() == true
	bool m_bMatchGreedyOnLeft, m_bMatchGreedyOnRight;
};
