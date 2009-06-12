
#pragma once

#include "BaseUtils.h"

#include <string>

//	class to represent the Unique Process Identifier (UPI)
//	The Unique Process Identifier (UPI) is a string of the form
//	"MachineName/AppName/ProcessID/StartTime"

class EXPORT_BaseUtils UPI
{
public:
	// ctor, copy ctor, and assignment operator
	UPI(const std::string& strUPI);
	UPI(const UPI& upiToCopy);
	UPI& operator=(const UPI& upiToAssign);

	// data access methods
	const std::string& getUPI() const;
	const std::string& getMachineName() const;
	const std::string& getAppName() const;
	unsigned long getProcessID() const;
	const std::string& getStartDate() const;
	const std::string& getStartTime() const;

	// get the UPI associated with the currently executing process
	static const UPI& getCurrentProcessUPI();

private:
	std::string m_strUPI;
	std::string m_strMachineName;
	std::string m_strAppName;
	unsigned long m_ulProcessID;
	std::string m_strStartDate;
	std::string m_strStartTime;
};