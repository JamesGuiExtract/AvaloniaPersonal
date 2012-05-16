#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	CurrentCurveTool.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	John Hurd
//
//==================================================================================================

#include "CurveTool.h"
#include "EInputType.h"

#include <Singleton.h>

#include <map>
#include <string>

class CurrentCurveTool  : public CurveTool,
						  public Singleton<CurrentCurveTool>
{
	ALLOW_SINGLETON_ACCESS(CurrentCurveTool);
public:
	EInputType getInputType(int iParameterID) const;
	std::string getPrompt(int iParameterID) const;

protected:
	CurrentCurveTool();
	// following required for all singletons in general
	CurrentCurveTool(const CurrentCurveTool& toCopy);
	CurrentCurveTool& operator = (const CurrentCurveTool& toAssign);

	~CurrentCurveTool();

private:
	typedef std::map<ECurveParameterType,EInputType> InputTypeMap;	
	InputTypeMap m_mapInputType;								// associates the input type with a curve parameter type
	typedef std::map<ECurveParameterType,std::string> PromptMap;
	PromptMap m_mapPrompt;										// associates a prompt with a curve parameter type

	void initializePromptMap(void);
	void initializeInputTypeMap(void);

};
