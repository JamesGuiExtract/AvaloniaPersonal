#include "stdafx.h"

// This is shared code; the includes differ based on whether it is being compiled as part of
// AFCore or AFCppUtils
#ifdef EXPORT_AFCPPUTILS_DLL
#include "AFCppUtils.h"
#else
#include "..\..\AFCore\Code\AFCore.h"
#include "..\..\AFCore\Code\AFInternalUtils.h"
#endif

#include <UCLIDException.h>
#include <COMUtils.h>

//--------------------------------------------------------------------------------------------------
void addCurrentRSDFileToDebugInfo(UCLIDException &ue)
{
	try
	{
		// Get the currently running Ruleset
		UCLID_AFCORELib::IRuleExecutionEnvPtr ipRuleEnv(CLSID_RuleExecutionEnv);
		
		// Only add the debug info if the RuleExecutionEnv was successfully created
		if (ipRuleEnv != __nullptr)
		{
			ue.addDebugInfo("RuleFileName", asString(ipRuleEnv->GetCurrentRSDFileName()), true);
		}
	}
	catch (...)
	{
		UCLIDException ue("ELI27683", "Encountered error trying to add rsd filename to debug info");
		ue.log();
	}
}
//--------------------------------------------------------------------------------------------------