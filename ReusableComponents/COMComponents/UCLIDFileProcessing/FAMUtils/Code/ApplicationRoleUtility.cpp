#include "stdafx.h"
#include "ApplicationRoleUtility.h"


ApplicationRoleUtility::ApplicationRoleUtility() :m_FileProcessingConfigManager()
{
	m_bUseApplicationRoles = m_FileProcessingConfigManager.getUseApplicationRoles();
}

