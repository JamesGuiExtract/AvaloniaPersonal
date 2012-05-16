//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	IcoMapOptionsDlg.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS: Duan Wang
//
//==================================================================================================

#include "stdafx.h"
#include "resource.h"
#include "IcoMapOptionsDlg.h"

#include <IcoMapOptions.h>
#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

extern HINSTANCE gModuleResource;

/////////////////////////////////////////////////////////////////////////////
// IcoMapOptionsDlg

IMPLEMENT_DYNAMIC(IcoMapOptionsDlg, CPropertySheet)

IcoMapOptionsDlg::IcoMapOptionsDlg(LPCTSTR pszCaption, CWnd* pParentWnd, UINT iSelectPage)
: CPropertySheet(pszCaption, pParentWnd, iSelectPage)
{
	EnableAutomation();
	AddPage(&m_GeneralTab);
	m_GeneralTab.m_iGeneralSettingsPageIndex = GetPageIndex(&m_GeneralTab);

	AddPage(&m_DirectionTab);
	m_DirectionTab.m_iDirectionSettingsPageIndex = GetPageIndex(&m_DirectionTab);

	AddPage(&m_KeyboardInputTab);
	m_KeyboardInputTab.m_iKeyboardInputSettingsPageIndex = GetPageIndex(&m_KeyboardInputTab);

	AddPage( &m_ShortcutTab );
	m_ShortcutTab.m_iShortcutSettingsPageIndex = GetPageIndex(&m_ShortcutTab);

	// Load the icon from the correct DLL resource
	TemporaryResourceOverride temporaryResourceOverride( gModuleResource );
	m_hIcon = AfxGetApp()->LoadIcon( IDI_ICON_OPTIONS );
}

IcoMapOptionsDlg::~IcoMapOptionsDlg()
{
}

void IcoMapOptionsDlg::OnFinalRelease()
{
	// When the last reference for an automation object is released
	// OnFinalRelease is called.  The base class will automatically
	// deletes the object.  Add additional cleanup required for your
	// object before calling the base class.

	CPropertySheet::OnFinalRelease();
}


BEGIN_MESSAGE_MAP(IcoMapOptionsDlg, CPropertySheet)
	//{{AFX_MSG_MAP(IcoMapOptionsDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

BEGIN_DISPATCH_MAP(IcoMapOptionsDlg, CPropertySheet)
	//{{AFX_DISPATCH_MAP(IcoMapOptionsDlg)
		// NOTE - the ClassWizard will add and remove mapping macros here.
	//}}AFX_DISPATCH_MAP
END_DISPATCH_MAP()

// Note: we add support for IID_IIcoMapOptionsDlg to support typesafe binding
//  from VBA.  This IID must match the GUID that is attached to the 
//  dispinterface in the .ODL file.

// {98D869DC-7549-11D5-817A-0050DAD4FF55}
static const IID IID_IIcoMapOptionsDlg =
{ 0x98d869dc, 0x7549, 0x11d5, { 0x81, 0x7a, 0x0, 0x50, 0xda, 0xd4, 0xff, 0x55 } };

BEGIN_INTERFACE_MAP(IcoMapOptionsDlg, CPropertySheet)
	INTERFACE_PART(IcoMapOptionsDlg, IID_IIcoMapOptionsDlg, Dispatch)
END_INTERFACE_MAP()

/////////////////////////////////////////////////////////////////////////////
// IcoMapOptionsDlg message handlers

int IcoMapOptionsDlg::DoModal() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	int nResult;
	try
	{	
		// Save original value of license mode
		int iOldMode = IcoMapOptions::sGetInstance().getLicenseManagementMode();

		nResult = CPropertySheet::DoModal();
		
		if (nResult == IDOK)
		{
			// Check new state of license management
			int iNewMode = IcoMapOptions::sGetInstance().getLicenseManagementMode();

			//////////////////////////////////////////////////////
			// Display tutorial file if mode changed to Evaluation
			//////////////////////////////////////////////////////
/*			if ((iNewMode == kEvaluation) && (iNewMode != iOldMode))
			{
				// Build executable string
				string	strExe( "hh.exe " );
				string	strHelpPath = IcoMapOptions::sGetInstance().getHelpDirectory();

				// Make sure path was found in INI file
				if (strHelpPath.length() > 0)
				{
					string	strHelpFile( "\\IM4ArcGIS.chm" );

					string	strCombined = strExe + strHelpPath + strHelpFile;

					// Prepare information for CreateProcess()
					STARTUPINFO			si;
					PROCESS_INFORMATION	pi;
					memset( &si, 0, sizeof(STARTUPINFO) );
					si.cb = sizeof(STARTUPINFO);
					si.wShowWindow = SW_SHOW;

					// Start Help application
					if (!CreateProcess( NULL, (char *)strCombined.data(), 
						NULL, NULL, TRUE, DETACHED_PROCESS, NULL, NULL, 
						&si, &pi ))
					{
						UCLIDException ue("ELI02146", "Unable to run Help!");
						throw ue;
					}
				}
				else
				{
					// Display error message
					::MessageBox( NULL, 
						"Could not find path to IcoMap Evaluation help file.",
						"IcoMap", MB_ICONEXCLAMATION | MB_OK );
				}
			}
*/		}
	}
	catch (UCLIDException &uclidException)
	{
		uclidException.addHistoryRecord("ELI01155", "Failed to open IcoMap Options dialog");
		throw uclidException;
	}
	catch (...)
	{
		throw UCLIDException("ELI01107", "Unknown exception was caught.");
	}
	
	return nResult;	
}

BOOL IcoMapOptionsDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	BOOL bResult = CPropertySheet::OnInitDialog();
	
	// Setup icons
	SetIcon(m_hIcon, TRUE);
	SetIcon(m_hIcon, FALSE);

	return bResult;
}
