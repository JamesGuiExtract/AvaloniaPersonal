// EmailSettings.cpp : Implementation of CEmailSettings
#include "stdafx.h"
#include "ESMessageUtils.h"
#include "EmailSettings.h"

#include <UCLIDException.h>
#include <COMUtils.h>
#include <cpputil.h>
#include <RegistryPersistenceMgr.h>
#include "EmailSettingsDlg.h"

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const long gnDEFAULT_PORT = 25;
const long gnDEFAULT_SMTP_TIMEOUT = 120;

//-------------------------------------------------------------------------------------------------
// Construction/Destruction
//-------------------------------------------------------------------------------------------------
CEmailSettings::CEmailSettings()
:	m_strSMTPServer(""), 
	m_lSMTPPort(gnDEFAULT_PORT), 
	m_strSMTPUserName(""), 
	m_strSMTPPassword(""),
	m_nSMTPTimeout(gnDEFAULT_SMTP_TIMEOUT)
{
}
//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEmailSettings::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IEmailSettings
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}
//-------------------------------------------------------------------------------------------------
// IObjectSettings
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEmailSettings::raw_LoadFromRegistry(BSTR bstrKey)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		string strKeyFull = asString( bstrKey );
		string strRegPath = "";
		HKEY hKey = parseKey( strKeyFull, strRegPath );

		RegistryPersistenceMgr UserCfgMgr(hKey, strRegPath);
		m_strSMTPServer = "";
		if ( UserCfgMgr.keyExists("", "SMTPServer" ))
		{
			m_strSMTPServer = UserCfgMgr.getKeyValue( "", "SMTPServer", "" );
		}
		// default value is port gnDEFAULT_PORT
		m_lSMTPPort = gnDEFAULT_PORT;
		if ( UserCfgMgr.keyExists( "", "SMTPPort" ))
		{
			string strSMTPPort = UserCfgMgr.getKeyValue( "", "SMTPPort", "" );
			if ( !strSMTPPort.empty() )
			{
				m_lSMTPPort = asLong (UserCfgMgr.getKeyValue( "", "SMTPPort", "" ).c_str());
			}
		}
		m_strSMTPUserName = "";
		if ( UserCfgMgr.keyExists( "", "SMTPUserName" ) ) 
		{
			m_strSMTPUserName = UserCfgMgr.getKeyValue( "", "SMTPUserName", "" );
		}
		m_strSMTPPassword = "";
		if ( UserCfgMgr.keyExists( "", "SMTPPassword" ) )
		{
			m_strSMTPPassword = UserCfgMgr.getKeyValue( "", "SMTPPassword", "" );
			//TODO: Decrypt password if encrypted
		}
		m_strSenderName = "";
		if ( UserCfgMgr.keyExists( "", "SenderName" ))
		{
			m_strSenderName = UserCfgMgr.getKeyValue( "", "SenderName", "" );
		}
		m_strSenderEmailAddr = "";
		if ( UserCfgMgr.keyExists( "", "SenderEmailAddr" ) )
		{
			m_strSenderEmailAddr = UserCfgMgr.getKeyValue( "", "SenderEmailAddr", "" );
		}
		m_strEmailSignature = "";
		if ( UserCfgMgr.keyExists( "", "EmailSignature" ))
		{
			m_strEmailSignature = UserCfgMgr.getKeyValue( "", "EmailSignature", "" );
		}
		m_nSMTPTimeout = gnDEFAULT_SMTP_TIMEOUT;
		if ( UserCfgMgr.keyExists( "", "SMTPTimeout" ))
		{
			m_nSMTPTimeout = asLong(UserCfgMgr.getKeyValue( "", "SMTPTimeout", "" ));
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12273")
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEmailSettings::raw_SaveToRegistry(BSTR bstrKey)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		string strKeyFull = asString( bstrKey );
		string strRegPath = "";
		HKEY hKey = parseKey( strKeyFull, strRegPath );

		RegistryPersistenceMgr UserCfgMgr(hKey, strRegPath);

		UserCfgMgr.setKeyValue("", "SMTPServer", m_strSMTPServer, true );
		UserCfgMgr.setKeyValue("", "SMTPPort", asString(m_lSMTPPort), true );
		UserCfgMgr.setKeyValue("", "SMTPUserName", m_strSMTPUserName, true );
		//TODO: Encrypt password
		UserCfgMgr.setKeyValue("", "SMTPPassword", m_strSMTPPassword, true );
		UserCfgMgr.setKeyValue("", "SenderName", m_strSenderName, true );
		UserCfgMgr.setKeyValue("", "SenderEmailAddr", m_strSenderEmailAddr, true );
		UserCfgMgr.setKeyValue("", "EmailSignature", m_strEmailSignature, true );
		UserCfgMgr.setKeyValue("", "SMTPTimeout", asString(m_nSMTPTimeout));
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12274")
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
// IObjectUserInterface
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEmailSettings::raw_DisplayReadOnly()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		UCLIDException ue("ELI12254", "Read Only Display not implemented.");
		throw ue;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12253");
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEmailSettings::raw_DisplayReadWrite()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		string strSaveServer = m_strSMTPServer;
		long lSavePort = m_lSMTPPort;
		string strSaveUserName = m_strSMTPUserName;
		string strSavePassword = m_strSMTPPassword;
		string strSaveSenderName = m_strSenderName;
		string strSaveSenderEmailAddr = m_strSenderEmailAddr;
		string strSaveEmailSignature = m_strEmailSignature;
		long lSaveSMTPTimeout = m_nSMTPTimeout;

		EmailSettingsDlg esd(this);
		if ( esd.DoModal() != IDOK )
		{
			m_strSMTPServer = strSaveServer;
			m_lSMTPPort = lSavePort;
			m_strSMTPUserName = strSaveUserName;
			m_strSMTPPassword = strSavePassword;
			m_strSenderName = strSaveSenderName;
			m_strSenderEmailAddr = strSaveSenderEmailAddr;
			m_strEmailSignature = strSaveEmailSignature;
			m_nSMTPTimeout = lSaveSMTPTimeout;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12305");
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEmailSettings::raw_SupportsReadOnly(VARIANT_BOOL * bSupportsReadOnly)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		ASSERT_ARGUMENT("ELI12251", bSupportsReadOnly );

		*bSupportsReadOnly = VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12250");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEmailSettings::raw_SupportsReadWrite(VARIANT_BOOL * bSupportsReadWrite)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		ASSERT_ARGUMENT("ELI12252", bSupportsReadWrite);

		*bSupportsReadWrite = VARIANT_TRUE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12249");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
// IEmailSettings
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEmailSettings::get_SMTPServer(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI12275", pVal );
		*pVal = get_bstr_t( m_strSMTPServer.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12256");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEmailSettings::put_SMTPServer(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_strSMTPServer = asString( newVal );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12257");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEmailSettings::get_SMTPPort(LONG *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI12276", pVal );
		*pVal = m_lSMTPPort;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12258");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEmailSettings::put_SMTPPort(LONG newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_lSMTPPort = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12259");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEmailSettings::get_SMTPUserName(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI12277", pVal );
		*pVal = get_bstr_t( m_strSMTPUserName.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12260");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEmailSettings::put_SMTPUserName(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_strSMTPUserName = asString( newVal );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12261");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEmailSettings::get_SMTPPassword(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI12278", pVal );
		*pVal = get_bstr_t( m_strSMTPPassword.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12262");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEmailSettings::put_SMTPPassword(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_strSMTPPassword = asString( newVal );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12263");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEmailSettings::get_SenderName(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI12316", pVal );
		*pVal = get_bstr_t( m_strSenderName.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12315");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEmailSettings::put_SenderName(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_strSenderName = asString( newVal );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12314");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEmailSettings::get_SenderEmailAddr(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI12313", pVal );
		*pVal = get_bstr_t( m_strSenderEmailAddr.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12309");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEmailSettings::put_SenderEmailAddr(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_strSenderEmailAddr = asString( newVal );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12308");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEmailSettings::get_EmailSignature(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI12312", pVal );
		*pVal = get_bstr_t( m_strEmailSignature.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12311");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEmailSettings::put_EmailSignature(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_strEmailSignature = asString( newVal );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12310");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEmailSettings::get_SMTPTimeout(LONG *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI25063", pVal != __nullptr );
		*pVal = m_nSMTPTimeout;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25062");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEmailSettings::put_SMTPTimeout(LONG newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_nSMTPTimeout = newVal;
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25061");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
HKEY CEmailSettings::parseKey( const string strRegFullPath, string & strRegPath )
{
	int nPos = strRegFullPath.find_first_of("\\/");
	if ( nPos != 0 ) 
	{
		strRegPath = strRegFullPath.substr(nPos + 1 );
		string strRegRoot = strRegFullPath.substr(0, nPos );
		makeUpperCase(strRegRoot);
		if ( strRegRoot == "HKLM" || strRegRoot == "HKEY_LOCAL_MACHINE" )
		{
			return HKEY_LOCAL_MACHINE;
		}
		else if ( strRegRoot == "HKCU" || strRegRoot == "HKEY_CURRENT_USER" )
		{
			return HKEY_CURRENT_USER;
		}
	}
	UCLIDException ue ("ELI12272", "Registry Key is not in the correct format." );
	throw ue;
}
//-------------------------------------------------------------------------------------------------
