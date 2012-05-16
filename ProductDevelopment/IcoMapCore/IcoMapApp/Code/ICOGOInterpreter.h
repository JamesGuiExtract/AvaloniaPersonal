//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ICOGOInterpreter.h
//
// PURPOSE:	To interpret COGO input according to the input type, and convert the input properly
//
// NOTES:	
//
// AUTHORS: Duan Wang (08/2001)
//
//==================================================================================================

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include <EInputType.h>
#include <EShortcutType.h>

#include <string>

class ICOGOInterpreter
{
public:
	virtual ~ICOGOInterpreter(){};
	virtual std::string interpretCOGOInput(const std::string& strInput, EInputType eInputType) = 0;
	virtual void interpretPointInput(const std::string &strInput, double &dX, double &dY) = 0;
	virtual EShortcutType interpretShortcutCommands(const std::string &strInput) = 0;
};
