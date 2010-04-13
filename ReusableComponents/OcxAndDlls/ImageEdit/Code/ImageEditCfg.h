
#pragma once

#include <IConfigurationSettingsPersistenceMgr.h>
#include <memory>

class ImageEditCtrlCfg
{
public:
	//---------------------------------------------------------------------------------------------
	ImageEditCtrlCfg();
	~ImageEditCtrlCfg();

	//---------------------------------------------------------------------------------------------
	// PROMISE:	To get or set the anti-aliasing functionality
	bool isAntiAliasingEnabled();
	void setAntiAliasing(bool bEnable);

	//---------------------------------------------------------------------------------------------
	// PROMISE:	To get or set the annotation functionality
	bool isAnnotationEnabled();
	void setAnnotation(bool bEnable);

private:
	// True iff the Anti-Aliasing functionality is licensed
	bool	isAntiAliasingLicensed();

	// Throws an exception if the Anti-Aliasing component is not licensed
	void	validateAALicense();

	// Pointer to the persistence manager for registry access
	std::auto_ptr<IConfigurationSettingsPersistenceMgr> m_apCfgMgr;
};
