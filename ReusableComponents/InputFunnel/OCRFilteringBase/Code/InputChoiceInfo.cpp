#include "stdafx.h"
#include "InputChoiceInfo.h"

using namespace std;

//==============================================================================================
// Public Functions
//==============================================================================================
InputChoiceInfo::InputChoiceInfo()
: m_strDescription(""), 
  m_strChars("") 
{
}
//--------------------------------------------------------------------------------------------------
InputChoiceInfo::InputChoiceInfo(const std::string& strDescription, const std::string& strChars)
: m_strDescription(strDescription),
  m_strChars(strChars)
{
}
//--------------------------------------------------------------------------------------------------
InputChoiceInfo::InputChoiceInfo(const InputChoiceInfo& toCopy)
{
	m_strDescription = toCopy.m_strDescription;
	m_strChars = toCopy.m_strChars;
}
//--------------------------------------------------------------------------------------------------
InputChoiceInfo& InputChoiceInfo::operator = (const InputChoiceInfo& toAssign)
{
	m_strDescription = toAssign.m_strDescription;
	m_strChars = toAssign.m_strChars;

	return *this;
}

//==============================================================================================
// Private Functions
//==============================================================================================
