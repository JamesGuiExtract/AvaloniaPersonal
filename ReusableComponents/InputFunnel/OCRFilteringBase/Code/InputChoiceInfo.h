//============================================================================
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	InputChoiceInfo.h
//
// PURPOSE:	Contains info for each sub string choices, such as sub string
//			choice description, characters included inside the sub string
//			choice, etc.
//
// NOTES:	
//
// AUTHORS:	Duan Wang
//
//============================================================================
#pragma once

#include <string>
#include <map>

class InputChoiceInfo
{
public:

	// **************************
	// Member variables
	std::string m_strDescription;
	std::string m_strChars;

	// **************************
	// Methods
	//==============================================================================================
	InputChoiceInfo();
	InputChoiceInfo(const std::string& strDescription, const std::string& strChars);
	InputChoiceInfo(const InputChoiceInfo& toCopy);
	InputChoiceInfo& operator = (const InputChoiceInfo& toAssign);
};

typedef std::map<std::string, InputChoiceInfo> InputChoices;