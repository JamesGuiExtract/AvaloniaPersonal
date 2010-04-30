#pragma once

#include "BaseUtils.h"

#include <vector>
#include <string>

class EXPORT_BaseUtils QuickMenuChooser
{
// Construction
public:
	QuickMenuChooser();
	// creates the QMC and sets the choices as per setChoices()
	QuickMenuChooser(const std::vector<std::string>& vecChoices);
	~QuickMenuChooser();

	// Reset the popup menu choices
	// a string with the value "" will be treated as a separator
	void setChoices(const std::vector<std::string>& vecChoices);

	// Displays a popup menu with the choices at position x, y
	// it will return the index of the selected menu item or
	// -1 if no item was selected.
	// Separators do count as menu items
	long getChoice(CWnd* pParent, long x, long y);

	// Displays a popup menu with the choices at position x, y
	// the string of the selected menu item is returned or ""
	// if no item was selected
	const std::string getChoiceString(CWnd* pParent, long x, long y);

private:
	std::vector< std::string > m_vecChoices;
	CMenu m_menu;
	bool m_bInit;
};
