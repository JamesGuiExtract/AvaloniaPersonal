#include "VariableRegistry.h"

//-------------------------------------------------------------------------------------------------
void VariableRegistry::addAllVariables(std::map<std::string, std::string> mapVariables)
{
	// Iterate through map items
	std::map<std::string, std::string>::iterator iter = mapVariables.begin();
	while (iter != mapVariables.end())
	{
		// Add this entry to map
		addVariable( iter->first, iter->second );

		iter++;
	}
}
//-------------------------------------------------------------------------------------------------
void VariableRegistry::addVariable(const std::string &strVariableName, 
								   const std::string &strVariableValue)
{
	if (!isVariableName(strVariableName))
	{
		m_mapVariables[strVariableName] = strVariableValue;
	}
	else
	{
		UCLIDException ue("ELI12298", "Variable already exists!");
		ue.addDebugInfo("VariableName", strVariableName);
		throw ue;		
	}
}
//-------------------------------------------------------------------------------------------------
void VariableRegistry::removeVariable(const std::string &strVariableName)
{
	if (isVariableName(strVariableName))
	{
		m_mapVariables.erase(strVariableName);
	}
	else
	{
		UCLIDException ue("ELI12299", "Variable not found!");
		ue.addDebugInfo("VariableName", strVariableName);
		throw ue;			
	}
}
//-------------------------------------------------------------------------------------------------
void VariableRegistry::clearVariables()
{
	m_mapVariables.clear();
}
//-------------------------------------------------------------------------------------------------
std::string VariableRegistry::getVariableValue(const std::string &strVariableName)
{
	if (isVariableName(strVariableName))
	{
		return m_mapVariables[strVariableName];
	}
	else
	{
		UCLIDException ue("ELI12300", "Variable not found!");
		ue.addDebugInfo("VariableName", strVariableName);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void VariableRegistry::setVariableValue(const std::string &strVariableName, 
										const std::string &strVariableValue)
{
	if (isVariableName(strVariableName))
	{
		m_mapVariables[strVariableName] = strVariableValue;
	}
	else
	{
		UCLIDException ue("ELI12301", "Variable not found!");
		ue.addDebugInfo("VariableName", strVariableName);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void VariableRegistry::replaceVariablesInString(std::string &strInputString)
{
	std::map<std::string, std::string>::iterator variablesIter = m_mapVariables.begin();

	while (variablesIter != m_mapVariables.end())
	{
		unsigned int curPos = !std::string::npos;

		do
		{
			curPos = strInputString.find(variablesIter->first);
		
			if (curPos != std::string::npos)
			{
				strInputString.replace(curPos, variablesIter->first.size(), variablesIter->second);
			}
		}
		while (curPos != std::string::npos);

		variablesIter++;
	}
}
//-------------------------------------------------------------------------------------------------
bool VariableRegistry::isVariableName(const std::string &strVariableName)
{
	return (m_mapVariables.count(strVariableName) > 0);
}
//-------------------------------------------------------------------------------------------------
