#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	CommandPrompt.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	John Hurd
//
//==================================================================================================

#include <string>

class CommandPrompt
{
public:
	CommandPrompt();
	virtual ~CommandPrompt();

	virtual void setCommandInput(const std::string& strInput) = 0;
	virtual void setCommandPrompt(const std::string& strPrompt, bool bShowDefaultValue = true) = 0;
};
