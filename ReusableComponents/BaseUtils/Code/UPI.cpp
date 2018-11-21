
#include "stdafx.h"
#include "UPI.h"

#include "cpputil.h"
#include "StringTokenizer.h"
#include "UCLIDException.h"

//--------------------------------------------------------------------------------------------------
// Statics
//--------------------------------------------------------------------------------------------------
CCriticalSection UPI::ms_criticalSection;
volatile bool UPI::ms_bInitialized = false;

//-------------------------------------------------------------------------------------------------
UPI::UPI(const string& strUPI)
{
	try
	{
		// tokenize the string
		StringTokenizer st('\\');
		vector<string> vecTokens;
		st.parse(strUPI, vecTokens);
		
		// if the correct number of tokens is available, assume the data
		// is correct and update member variables
		if (vecTokens.size() == 5)
		{
			m_strUPI = strUPI;
			m_strMachineName = vecTokens[0];
			m_strAppName = vecTokens[1];
			m_ulProcessID = asUnsignedLong(vecTokens[2]);
			m_strStartDate = vecTokens[3];
			m_strStartTime = vecTokens[4];
			m_strProcessSemaphoreName = "Local\\" + asString(vecTokens);
		}
		else
		{
			UCLIDException ue("ELI12414", "Unable to parse UPI string!");
			ue.addDebugInfo("strUPI", strUPI);
			throw ue;
		}
	}
	catch (UCLIDException& ue)
	{
		UCLIDException uexOuter("ELI12391", "Unable to construct UPI object from string!", ue);
		uexOuter.addDebugInfo("strUPI", strUPI);
		throw uexOuter;
	}
}
//-------------------------------------------------------------------------------------------------
UPI::UPI(const UPI& upiToCopy)
{
	*this = upiToCopy;
}
//-------------------------------------------------------------------------------------------------
UPI& UPI::operator=(const UPI& upiToAssign)
{
	m_strUPI = upiToAssign.m_strUPI;
	m_strMachineName = upiToAssign.m_strMachineName;
	m_strAppName = upiToAssign.m_strAppName;
	m_ulProcessID = upiToAssign.m_ulProcessID;
	m_strStartDate = upiToAssign.m_strStartDate;
	m_strStartTime = upiToAssign.m_strStartTime;
	m_strProcessSemaphoreName = upiToAssign.m_strProcessSemaphoreName;

	return *this;
}
//-------------------------------------------------------------------------------------------------
const string& UPI::getUPI() const
{
	return m_strUPI;
}
//-------------------------------------------------------------------------------------------------
const string& UPI::getMachineName() const
{
	return m_strMachineName;
}
//-------------------------------------------------------------------------------------------------
const string& UPI::getAppName() const
{
	return m_strAppName;
}
//-------------------------------------------------------------------------------------------------
unsigned long UPI::getProcessID() const
{
	return m_ulProcessID;
}
//-------------------------------------------------------------------------------------------------
const string& UPI::getStartTime() const
{
	return m_strStartTime;
}
//-------------------------------------------------------------------------------------------------
const string& UPI::getStartDate() const
{
	return m_strStartDate;
}
const std::string& UPI::getProcessSemaphoreName() const
{
	return m_strProcessSemaphoreName;
}
//-------------------------------------------------------------------------------------------------
const UPI& UPI::getCurrentProcessUPI()
{
	// create the UPI for the current process if it hasn't already been created.
	static string strUPI;
	if (!ms_bInitialized)
	{
		CSingleLock lg(&ms_criticalSection, TRUE);

		if (!ms_bInitialized)
		{
			// append the computer name
			const string strSLASH = "\\";
			strUPI += getComputerName() + strSLASH;

			// append the application name
			string strAppName;
			try
			{
				// Avoid an exception when running via nunit
				if (__argv != __nullptr)
				{
					strAppName = getFileNameWithoutExtension(__argv[0]);
				}
				else
				{
					strAppName = getFileNameWithoutExtension(getCurrentProcessEXEFullPath());
				}
			}
			catch (...)
			{
				try
				{
					strAppName = getFileNameWithoutExtension(getCurrentProcessEXEFullPath());
				}
				catch (...)
				{
					strAppName = "Unknown";
				}
			}
			strUPI += strAppName + strSLASH;

			// append the process id
			strUPI += getCurrentProcessID() + strSLASH;

			// append the current date
			CTime currentTime = CTime::GetCurrentTime();
			string strTemp;
			strTemp = LPCTSTR(currentTime.Format("%m%d%Y"));
			strUPI += strTemp + strSLASH;

			// append the current time
			strUPI += getTimeAsString();

			ms_bInitialized = true;
		}
	}

	// return the UPI
	static UPI thisProcessUPI(strUPI);
	return thisProcessUPI;
}
//-------------------------------------------------------------------------------------------------
