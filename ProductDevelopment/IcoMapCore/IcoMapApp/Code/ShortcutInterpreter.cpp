#include "stdafx.h"
#include "ShortcutInterpreter.h"

#include <IcoMapOptions.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

using namespace std;

EShortcutType ShortcutInterpreter::interpretShortcutCommand(const std::string &strInput)
{
	CString cstrTemp(strInput.c_str());
	// trim off the empty space
	cstrTemp.TrimLeft(" ");
	string strCommand(cstrTemp);

	EShortcutType eShortcutType = kShortcutNull;
	// if this string starts with an escape char, take it as a command and translate it
	if (strCommand.find(m_zEscapeChar) == 0)
	{
		// chop off the slash
		strCommand = strCommand.substr(1);
		eShortcutType = IcoMapOptions::sGetInstance().getShortcutType(strCommand);
	}
	
	return eShortcutType;
}
