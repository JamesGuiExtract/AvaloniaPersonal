#pragma once

#include "stdafx.h"
#include "UCLIDException.h"

namespace uex
{
	// Convert the current exception into a UCLIDException
	// Copied logic from the CATCH_AND_DISPLAY_ALL_EXCEPTIONS macro
	inline UCLIDException fromCurrent(const std::string& eliCode)
	{
		try
		{
			// rethrow the current exception so that its type can be tested
			rethrow_exception(current_exception());
		}
		catch (UCLIDException& ue)
		{
			ue.addDebugInfo("catchID", eliCode);

			return ue;
		}
		catch (_com_error& e)
		{
			UCLIDException ue;
			_bstr_t _bstrDescription = e.Description();
			char* pszDescription = _bstrDescription;

			if (pszDescription)
			{
				ue.createFromString(eliCode, pszDescription);
			}
			else
			{
				ue.createFromString(eliCode, "COM exception caught!");
			}

			ue.addHresult(e.Error());
			ue.addDebugInfo("err.WCode", e.WCode());

			return ue;
		}
		catch (COleDispatchException* pEx)
		{
			string strDesc = (LPCTSTR)pEx->m_strDescription;
			UCLIDException ue;
			ue.createFromString(eliCode, strDesc.empty() ? "OLE dispatch exception caught." : strDesc);
			ue.addDebugInfo("Error Code", pEx->m_wCode);
			pEx->Delete();

			return ue;
		}
		catch (COleDispatchException& ex)
		{
			string strDesc = (LPCTSTR)ex.m_strDescription;
			UCLIDException ue;
			ue.createFromString(eliCode, strDesc.empty() ? "OLE dispatch exception caught." : strDesc);
			ue.addDebugInfo("Error Code", ex.m_wCode);

			return ue;
		}
		catch (COleException& ex)
		{
			char pszCause[256] = { 0 };

			ex.GetErrorMessage(pszCause, 255);
			UCLIDException ue;
			ue.createFromString(eliCode, *pszCause == '\0' ? "OLE exception caught." : pszCause);
			ue.addDebugInfo("Status Code", ex.m_sc);

			return ue;
		}
		catch (CException* pEx)
		{
			char pszCause[256] = { 0 };
			pEx->GetErrorMessage(pszCause, 255);
			pEx->Delete();
			UCLIDException ue;
			ue.createFromString(eliCode, *pszCause == '\0' ? "C Exception caught." : pszCause);

			return ue;
		}
		catch (...)
		{
			return UCLIDException(eliCode, "Unexpected exception caught.");
		};
	}
	//--------------------------------------------------------------------------------------------------
	// Convert the current exception into a UCLIDException
	// with optional LastCodePosition info
	inline UCLIDException fromCurrent(const std::string& eliCode,
		const LastCodePosition* lastCodePos)
	{
		UCLIDException ue = fromCurrent(eliCode);

		if (lastCodePos)
		{
			ue.addDebugInfo(*lastCodePos);
		}

		return ue;
	}
	//--------------------------------------------------------------------------------------------------
	// Convert the current exception into a UCLIDException and log it
	inline void logCurrent(const std::string& eliCode, const LastCodePosition* lastCodePos = __nullptr)
	{
		uex::fromCurrent(eliCode, lastCodePos).log();
	}
	//--------------------------------------------------------------------------------------------------
	// Convert the current exception into a UCLIDException and display it
	inline void displayCurrent(const std::string& eliCode, const LastCodePosition* lastCodePos = __nullptr)
	{
		uex::fromCurrent(eliCode, lastCodePos).display();
	}
	//--------------------------------------------------------------------------------------------------
	// Convert the current exception into a UCLIDException and log or display it
	inline void logOrDisplayCurrent(const std::string& eliCode, bool display,
		const LastCodePosition* lastCodePos = __nullptr)
	{
		if (display)
		{
			displayCurrent(eliCode, lastCodePos);
		}
		else
		{
			logCurrent(eliCode, lastCodePos);
		}
	}
}
