#pragma once

#include "BaseUtils.h"
#include "stdafx.h"

// The constructor of this class save the CWinApp mainWnd ptr 
// to a memeber variable and restore it in destructor
class EXPORT_BaseUtils AfxAppMainWindowRestorer
{
public:
	// Constructor
	AfxAppMainWindowRestorer();
	// Destructor
	~AfxAppMainWindowRestorer();

private:
	CWnd *m_pMainWnd;
};
