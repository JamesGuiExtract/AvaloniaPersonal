// QuickMenuChooser.cpp : implementation file
//

#include "stdafx.h"
#include "BaseUtils.h"
#include "QuickMenuChooser.h"
#include "UCLIDException.h"

using namespace std;

static const long gnBASE_MENU_ID = 10546;

//--------------------------------------------------------------------------------------------------
// QuickMenuChooser
//--------------------------------------------------------------------------------------------------
QuickMenuChooser::QuickMenuChooser() 
: m_bInit(false)
{
}
//--------------------------------------------------------------------------------------------------
QuickMenuChooser::QuickMenuChooser(const std::vector<std::string>& vecChoices)
: m_bInit(false)
{
	setChoices(vecChoices);
}
//--------------------------------------------------------------------------------------------------
QuickMenuChooser::~QuickMenuChooser()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16396");
}
//--------------------------------------------------------------------------------------------------
void QuickMenuChooser::setChoices(const std::vector<std::string>& vecChoices)
{
	if (!m_bInit)
	{
		m_menu.CreatePopupMenu();
		m_bInit = true;
	}

	while(m_menu.GetMenuItemCount() > 0)
	{
		m_menu.RemoveMenu(0, MF_BYPOSITION);
	}
	m_vecChoices = vecChoices;

	unsigned long ulID = gnBASE_MENU_ID;
	for(unsigned int n = 0; n < vecChoices.size(); n++)
	{
		const string& strChoice = m_vecChoices[n];
	
		if (strChoice != "")
		{
			m_menu.InsertMenu(-1, MF_BYPOSITION | MF_STRING, ulID, strChoice.c_str());
		}
		else
		{
			m_menu.InsertMenu(-1, MF_BYPOSITION | MF_SEPARATOR, 0, "");
		}
		ulID++;
	}
}
//--------------------------------------------------------------------------------------------------
long QuickMenuChooser::getChoice(CWnd* pParent, long x, long y)
{
	if (!m_bInit)
	{
		m_menu.CreatePopupMenu();
		m_bInit = true;
	}

	long nIndex = -1;
	int command = m_menu.TrackPopupMenu( TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_VERTICAL | TPM_NONOTIFY | TPM_RETURNCMD,
		x, y, pParent);

	if (command > 0)
	{
		nIndex = command - gnBASE_MENU_ID;
	}
	return nIndex;
}
//--------------------------------------------------------------------------------------------------
const std::string QuickMenuChooser::getChoiceString(CWnd* pParent, long x, long y)
{
	string str = "";
	long nIndex = getChoice(pParent, x, y);
	if (nIndex >= 0)
	{
		str = m_vecChoices[nIndex];
	}
	return str;
}
//--------------------------------------------------------------------------------------------------
