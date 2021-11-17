#pragma once

#include "FAMUtils.h"
#include "SqlApplicationRole.h"
#include "UCLIDException.h"
#include "CppApplicationRoleConnection.h"
#include "FileProcessingConfigMgr.h"

#include <memory>

using namespace std;

class FAMUTILS_API ApplicationRoleUtility
{
	FileProcessingConfigMgr m_FileProcessingConfigManager;
	bool m_bUseApplicationRoles;

public:
	ApplicationRoleUtility();

	unique_ptr<CppBaseApplicationRoleConnection> CreateAppRole(ADODB::_ConnectionPtr ipConnection, CppBaseApplicationRoleConnection::AppRoles role)
	{
		ASSERT_ARGUMENT("ELI13650", ipConnection != __nullptr);

		unique_ptr<CppBaseApplicationRoleConnection> roleInstance;

		if (m_bUseApplicationRoles)
		{
			try
			{
				switch (role)
				{
				case CppBaseApplicationRoleConnection::kNoRole:
					roleInstance.reset(new NoRoleConnection(ipConnection));
					break;
				case CppBaseApplicationRoleConnection::kExtractRole:
					roleInstance.reset(new ExtractRoleConnection(ipConnection));
					break;
				case CppBaseApplicationRoleConnection::kSecurityRole:
					roleInstance.reset(new SecurityRoleConnection(ipConnection));
					break;
				default:
					UCLIDException ue("ELI51837", "Unknown application role requested.");
					ue.addDebugInfo("ApplicationRole", (int)role);
					throw ue;
				}
			}
			catch (...)
			{
				// Try with the no role 
				roleInstance.reset(new NoRoleConnection(ipConnection));
			}
		}
		else
			roleInstance.reset(new NoRoleConnection(ipConnection));

		return roleInstance;
	}

	void RefreshSettings()
	{
		m_bUseApplicationRoles = m_FileProcessingConfigManager.getUseApplicationRoles();
	}
};

