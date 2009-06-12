//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	VariableRegistry.h
//
// PURPOSE:	To manage a list of variables and their corresponding values
//			
// NOTES:	
//
// AUTHORS:	Niles Bindel
//
//==================================================================================================

#pragma once

#include "stdafx.h"
#include "BaseUtils.h"
#include "UCLIDException.h"

#include <map>
#include <string>

class EXPORT_BaseUtils VariableRegistry
{
public:
	//----------------------------------------------------------------------------------------------
	// PURPOSE:  To register an entire collection of variables into the list
	// ARGS:	 mapVariables - map<strVariableName, strVariableValue>
	//			 where:
	//				strVariableName - name of variable to add
	//				strVariableValue - value of variable to add
	void addAllVariables(std::map<std::string, std::string> mapVariables);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:  To register a variable into the list of variables
	// ARGS:	 strVariableName - name of variable to add
	//			 strVariableValue - value of variable to add
	void addVariable(const std::string &strVariableName, const std::string &strVariableValue);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:  To remove an existing variable from the list of variables
	// ARGS:	 strVariableName - name of variable to remove
	void removeVariable(const std::string &strVariableName);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:  To empty out the list of variables and their values
	// PROMISE:  Clear the currently stored variables
	void clearVariables();
	//----------------------------------------------------------------------------------------------
	// PURPOSE:  To retrieve a value for a given variable
	// ARGS:	 strVariableName - name of variable to remove
	std::string getVariableValue(const std::string &strVariableName);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:  To modify and existing variable's value
	// ARGS:	 strVariableName - name of variable to remove
	void setVariableValue(const std::string &strVariableName, const std::string &strVariableValue);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:  To determine if a given name is currently a registered variable
	// ARGS:	 strVariableName - name of variable to remove
	bool isVariableName(const std::string &strVariableName);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:  To determine if a given name is currently a registered variable
	// ARGS:	 strInputString - string to replace variables in
	void replaceVariablesInString(std::string &strInputString);

private:
	// map used to store the variables and their values
	std::map<std::string, std::string> m_mapVariables;
};
