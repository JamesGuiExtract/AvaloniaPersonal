#include "stdafx.h"
#include "ApplicationRoleUtility.h"


ApplicationRoleUtility::ApplicationRoleUtility() :m_FileProcessingConfigManager()
{
	UseApplicationRoles = m_FileProcessingConfigManager.getUseApplicationRoles();
}

