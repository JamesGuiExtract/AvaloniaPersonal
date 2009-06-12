
#include "stdafx.h"
#include "SPMTokenInfo.h"

//------------------------------------------------------------------------------------------------
SPMTokenInfo::SPMTokenInfo()
{
	m_eTokenType = kInvalidTokenType;
	m_ulMaxIgnoreChars = 0;
	m_nMatchStartPos = -1;
	m_nMatchEndPos = -1;
	m_bMatchGreedyOnLeft = false;
	m_bMatchGreedyOnRight = false;
}
//------------------------------------------------------------------------------------------------
SPMTokenInfo::SPMTokenInfo(const SPMTokenInfo& ti)
{
	*this = ti;
}
//------------------------------------------------------------------------------------------------
SPMTokenInfo& SPMTokenInfo::operator=(const SPMTokenInfo& ti)
{
	// copy member variables
	m_eTokenType = ti.m_eTokenType;
	m_ulMaxIgnoreChars = ti.m_ulMaxIgnoreChars;
	m_nMatchStartPos = ti.m_nMatchStartPos;
	m_nMatchEndPos = ti.m_nMatchEndPos;
	m_strExprOrVariableName = ti.m_strExprOrVariableName;
	m_strToken = ti.m_strToken;
	m_bMatchGreedyOnLeft = ti.m_bMatchGreedyOnLeft;
	m_bMatchGreedyOnRight = ti.m_bMatchGreedyOnRight;

	return *this;
}
//------------------------------------------------------------------------------------------------
bool SPMTokenInfo::isMatchVariable() const
{
	return m_eTokenType == kDesiredMatch;
}
//------------------------------------------------------------------------------------------------
