// DBInfoCombo.cpp : implementation file
//

#include "stdafx.h"
#include "FAMUtils.h"
#include "DBInfoCombo.h"
#include "DotNetUtils.h"
#include "FAMUtilsConstants.h"
#include "FileProcessingConfigMgr.h"

#include <UCLIDException.h>
#include <ExtractMFCUtils.h>
#include <RegistryPersistenceMgr.h>
#include <Win32Util.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const string gstrBROWSE_STRING = "<Browse...>";

//-------------------------------------------------------------------------------------------------
// DBInfoCombo Class
//-------------------------------------------------------------------------------------------------
DBInfoCombo::DBInfoCombo(EDBInfoType eDBInfoType)
: m_eDBInfoType(eDBInfoType),
m_strServer("")
{
	// Get the current OS platform
	m_winPlatform = GetPlatform();
}
	
//-------------------------------------------------------------------------------------------------
// Public methods
//-------------------------------------------------------------------------------------------------
void DBInfoCombo::setSQLServer(const std::string strServer)
{
	// Only set the server if it is different from the last
	if (m_eDBInfoType == kDatabaseName && m_strServer != strServer)
	{
		// Set the server
		m_strServer = strServer;

		// Only reset content if the window handle is not null
		if ( m_hWnd != NULL )
		{
			// Reset the items in the list
			ResetContent();
		}
	}
}
//-------------------------------------------------------------------------------------------------
DBInfoCombo::EDBInfoType DBInfoCombo::getDBInfoType()
{
	return m_eDBInfoType;
}

//-------------------------------------------------------------------------------------------------
// Message Map
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(DBInfoCombo, CComboBox)
	ON_CONTROL_REFLECT(CBN_SELENDOK, &DBInfoCombo::OnCbnSelendok)
	ON_CONTROL_REFLECT(CBN_DROPDOWN, &DBInfoCombo::OnCbnDropdown)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// Message handlers
//-------------------------------------------------------------------------------------------------
void DBInfoCombo::OnCbnSelendok()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Show the wait cursor
		CWaitCursor waitCursor;
		
		// Get the current selection in the list
		int iSelected = GetCurSel();
		if (iSelected >= 0)
		{
			CString zData;

			// Get the text for the selected item
			GetLBText(iSelected, zData);

			// Check for the browse string
			if ( zData == gstrBROWSE_STRING.c_str())
			{
				// Get the server or DB name list
				vector<string> vecList;

				try
				{
					switch (m_eDBInfoType)
					{
					case kServerName:
						getServerList(vecList);
						break;
					case kDatabaseName:
						getDBNameList(m_strServer, vecList);
						break;
					default:
						THROW_LOGIC_ERROR_EXCEPTION("ELI18100");
					};
				}
				catch(...)
				{
					SetCurSel(-1);
					throw;
				}

				// Load the combo box
				loadComboBoxFromVector(*this, vecList);
				if ( m_eDBInfoType == kServerName )
				{
					addLocalInstances();
				}

				// On Vista need to adjust the rectangle size manually
				if (m_winPlatform == WINVISTA)
				{
					// Get the current window rectangle
					CRect rect;
					GetWindowRect(&rect);

					// Adjust the bottom to be tall enough for at least 7 items
					// (GetItemHeight(-1) returns height of edit box which should
					// be the height of each item in the list
					rect.bottom = rect.top + GetItemHeight(-1) * 7;

					// Set the new window size
					SetWindowPos(NULL, 0, 0, rect.Width(), rect.Height(),
						SWP_NOMOVE | SWP_NOZORDER);
				}

				// need to post a message to combo box so that the list will be displayed after
				// this handler exits
				PostMessageA(CB_SHOWDROPDOWN, TRUE);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18101");
}
//-------------------------------------------------------------------------------------------------
void DBInfoCombo::OnCbnDropdown()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		int nCount = GetCount();

		// If no items in the list OR 
		// if only "(local)" is in the Server list
		if (nCount == 0 )
		{
			if ( m_eDBInfoType == kServerName )
			{
				addLocalInstances();
			}
			// Add Browse string to list
			AddString(gstrBROWSE_STRING.c_str());
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18102");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void DBInfoCombo::addLocalInstances()
{
	// if the type is Server name and there is not a string beginning with (local)
	// add the local instances - if there are any
	if (m_eDBInfoType == kServerName && 
		(FindString(-1, gstrLOCAL_STRING.c_str()) == CB_ERR) )
	{
		// Initialize vector for instance names
		vector<string> vecInstances;

		// Set up registry managers
		FileProcessingConfigMgr fpCfgMgr;

		// Get the Local SQL server instances for the local machine
		fpCfgMgr.getLocalSQLServerInstances(vecInstances);
		
		// Process instance names in reverse order
		for (long n = (long) vecInstances.size() - 1; n >= 0; n--)
		{
			// If not an empty string
			if (!vecInstances[n].empty()) 
			{
				// build the local string
				string strLocal = gstrLOCAL_STRING;

				// Check if the name is for the default instance which is used with (local)
				if (vecInstances[n] != gstrDEFAULT_SQL_INSTANCE_NAME)
				{
					// Append the instance name to the (local) string
					strLocal += "\\" + vecInstances[n];
				}
				
				// Insert at the beginning
				InsertString(0, strLocal.c_str() );
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
