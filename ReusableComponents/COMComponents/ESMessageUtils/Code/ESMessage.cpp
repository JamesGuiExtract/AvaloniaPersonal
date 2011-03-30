// ESMessage.cpp : Implementation of CESMessage
#include "stdafx.h"
#include "ESMessageUtils.h"
#include "ESMessage.h"

#include <UCLIDException.h>
#include <COMUtils.h>
#include <ByteStreamManipulator.h>
#include <EncryptionEngine.h>

#include "LTEmailErr.h"

//Win32Mutex CESMessage::ms_mutex;
CMutex CESMessage::ms_mutex;

// Define four passwords used for encrypting the password
// NOTE: These passwords were not exposed at the header file level because
//		 no user of this class needs to know that these passwords exist
// These passwords are also uses in the EmailSettings.cpp
const unsigned long	gulEmailKey1 = 0x3EFF3899;
const unsigned long	gulEmailKey2 = 0x3A7B3AF7;
const unsigned long	gulEmailKey3 = 0x70890E74;
const unsigned long	gulEmailKey4 = 0xDC105DD;

//--------------------------------------------------------------------------------------------------
// FILE-SCOPE FUNCTIONS
//--------------------------------------------------------------------------------------------------
// NOTE: This function is purposely not exposed at header file level as a class
//		 method, as no user of this class needs to know that such a function
//		 exists.
void getEmailPassword(ByteStream& rPasswordBytes)
{
	ByteStreamManipulator bsm(ByteStreamManipulator::kWrite, rPasswordBytes);
	
	bsm << gulEmailKey1;
	bsm << gulEmailKey2;
	bsm << gulEmailKey3;
	bsm << gulEmailKey4;
	bsm.flushToByteStream( 8 );
}

//-------------------------------------------------------------------------------------------------
// Construction/Destruction
//-------------------------------------------------------------------------------------------------
CESMessage::CESMessage()
:	m_ipEmailSettings(NULL),
	m_ipltSMTP(NULL),
	m_ipFileAttachments(NULL),
	m_ipRecipients(NULL)
{
	try
	{
		m_ipFileAttachments.CreateInstance(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI12424", m_ipFileAttachments != __nullptr);

		m_ipRecipients.CreateInstance(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI12425", m_ipRecipients != __nullptr);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI12423")
}
//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CESMessage::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IESMessage
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}
//-------------------------------------------------------------------------------------------------
// IObjectUserInterface
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CESMessage::raw_DisplayReadOnly()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		CSingleLock slg(&ms_mutex, TRUE );
		UCLIDException ue("ELI12352", "Read Only Display not implemented.");
		throw ue;
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12353");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CESMessage::raw_DisplayReadWrite()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		CSingleLock slg(&ms_mutex, TRUE );

		// TODO: check if sender is set if it is do not use the default
		// if no sender is set use the default stored in the registry
		setupSender();

		addAttachments();
		addRecipients();

		// Set up body text with the email signature
		string strNew;
		string strSignature = m_ipEmailSettings->EmailSignature;

		if ( !strSignature.empty() )
		{
			strNew = m_strBodyText + "\r\n\r\n" + strSignature;
		}
		else
		{
			strNew = m_strBodyText;
		}
		getLtSMTP()->TextualBody = strNew.c_str();

		short nEmailStatus = getLtSMTP()->ShowSendDialog(NULL, "Message", CDLG_ENABLE_UPDATE);
		// Only concered with the errors that start at EML_ERROR_ALLOC_MEMORY_FAILED
		// other status indicate a different degree of success
		if ( nEmailStatus >= EML_ERROR_ALLOC_MEMORY_FAILED	)
		{
			UCLIDException ue("ELI12369", "Error opening eamil message." );
			loadLTEmailErrInfo(ue, nEmailStatus );
			throw ue;
		}
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12368");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CESMessage::raw_SupportsReadOnly(VARIANT_BOOL * bSupportsReadOnly)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		CSingleLock slg(&ms_mutex, TRUE );

		ASSERT_ARGUMENT("ELI12354", bSupportsReadOnly );

		*bSupportsReadOnly = VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12355");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CESMessage::raw_SupportsReadWrite(VARIANT_BOOL * bSupportsReadWrite)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		CSingleLock slg(&ms_mutex, TRUE );
		ASSERT_ARGUMENT("ELI12356", bSupportsReadWrite);

		*bSupportsReadWrite = VARIANT_TRUE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12357");
}
//-------------------------------------------------------------------------------------------------
// IESMessage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CESMessage::Send()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		CSingleLock slg(&ms_mutex, TRUE );
		
		// TODO: check if sender is set if it is do not use the default
		setupSender();

		addAttachments();
		addRecipients();
		
		// Set up body text with the email signature
		string strNew;
		string strSignature = m_ipEmailSettings->EmailSignature;

		if ( !strSignature.empty() )
		{
			strNew = m_strBodyText + "\r\n\r\n" + strSignature;
		}
		else
		{
			strNew = m_strBodyText;
		}
		getLtSMTP()->TextualBody = strNew.c_str();

		// This flag will be used to retry after a reconnection attempt
		bool bSendAgain = false;

		// Send the message
		short nEmailStatus;

		do
		{
			nEmailStatus = getLtSMTP()->LTSendMessage();
			if ( nEmailStatus == EML_ERROR_TIMEOUT )
			{
				// if already tried again after reconnect throw exception
				if (bSendAgain)
				{
					UCLIDException ue("ELI25064", "Unable to send message." );
					loadLTEmailErrInfo(ue, nEmailStatus );
					string strServer = asString(m_ipEmailSettings->SMTPServer);
					ue.addDebugInfo("SMTP Server", strServer);
					ue.addDebugInfo("SMTP Port", m_ipEmailSettings->SMTPPort);
					throw ue;
				}

				// Reconnect
				nEmailStatus = m_ipltSMTP->Connect ();
				if ( nEmailStatus > EML_ERROR )
				{
					UCLIDException ue("ELI12412", "Error Connecting to SMTP Server." );
					loadLTEmailErrInfo(ue, nEmailStatus );
					string strServer = asString(m_ipEmailSettings->SMTPServer);
					ue.addDebugInfo("SMTP Server", strServer);
					ue.addDebugInfo("SMTP Port", m_ipEmailSettings->SMTPPort);
					throw ue;
				}
				bSendAgain = true;
			}
			else if ( nEmailStatus != EML_SUCCESS)
			{
				UCLIDException ue("ELI12366", "Error Sending Email." );
				loadLTEmailErrInfo(ue, nEmailStatus );
				throw ue;
			}
			else
			{
				// Successful send
				bSendAgain = false;
			}
		}
		while (bSendAgain);

		// Clear the MessageContents
		nEmailStatus = getLtSMTP()->ClearMessageContents(MSG_CLEAR_ALL);
		if ( nEmailStatus != EML_SUCCESS)
		{
			UCLIDException ue("ELI12370", "Error Clearing message." );
			loadLTEmailErrInfo(ue, nEmailStatus );
			throw ue;
		}
		m_strBodyText = "";
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12367");
}

STDMETHODIMP CESMessage::put_EmailSettings(IEmailSettings * newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		CSingleLock slg(&ms_mutex, TRUE );
		m_ipEmailSettings = newVal;
		m_ipltSMTP = __nullptr;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12361");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CESMessage::get_Subject(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		CSingleLock slg(&ms_mutex, TRUE );
		ASSERT_ARGUMENT("ELI12375", pVal );
		*pVal = getLtSMTP()->Subject.copy();
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12376");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CESMessage::put_Subject(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		CSingleLock slg(&ms_mutex, TRUE );
		getLtSMTP()->Subject = newVal;
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12379");

}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CESMessage::get_BodyText(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		CSingleLock slg(&ms_mutex, TRUE );
		*pVal = get_bstr_t(m_strBodyText.c_str()).copy();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12383");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CESMessage::put_BodyText(BSTR newVal)
{
	
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		CSingleLock slg(&ms_mutex, TRUE );
		m_strBodyText = asString( newVal );
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12386");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CESMessage::get_FileAttachments(IVariantVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		CSingleLock slg(&ms_mutex, TRUE );

		IVariantVectorPtr ipShallowCopy = m_ipFileAttachments;	
		*pVal = ipShallowCopy.Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12387");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CESMessage::put_FileAttachments(IVariantVector *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());


	try
	{
		CSingleLock slg(&ms_mutex, TRUE );
		m_ipFileAttachments = newVal;
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12393");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CESMessage::get_Recipients(IVariantVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	
	try
	{
		CSingleLock slg(&ms_mutex, TRUE );
		IVariantVectorPtr ipShallowCopy = m_ipRecipients;
		*pVal = ipShallowCopy.Detach();
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12394");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CESMessage::put_Recipients(IVariantVector *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		CSingleLock slg(&ms_mutex, TRUE );
		m_ipRecipients = newVal;
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12395");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CESMessage::get_Sender(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		CSingleLock slg(&ms_mutex, TRUE );
		*pVal = getLtSMTP()->SenderAddress.copy();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12398");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CESMessage::put_Sender(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		CSingleLock slg(&ms_mutex, TRUE );
		getLtSMTP()->SenderAddress = newVal;
		getLtSMTP()->SenderName = newVal;
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12399");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CESMessage::Clear()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		CSingleLock slg(&ms_mutex, TRUE );
		short nEmailStatus = getLtSMTP()->ClearMessageContents(MSG_CLEAR_ALL);
		if ( nEmailStatus != EML_SUCCESS)
		{
			UCLIDException ue("ELI19410", "Error Clearing message." );
			loadLTEmailErrInfo(ue, nEmailStatus );
			throw ue;
		}
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25262");
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
ILEADSmtpPtr CESMessage::getLtSMTP()
{
	CSingleLock slg(&ms_mutex, TRUE );
	if ( m_ipltSMTP == __nullptr )
	{
		m_ipltSMTP.CreateInstance(__uuidof(LEADSmtp));
		ASSERT_RESOURCE_ALLOCATION("ELI12362", m_ipltSMTP != __nullptr );

		if ( m_ipEmailSettings == __nullptr )
		{
			UCLIDException ue("ELI12363", "Invalid Email Settings." );
			throw ue;
		}
		// Connect to the SMTP server
		m_ipltSMTP->ServerAddress = m_ipEmailSettings->SMTPServer;
		m_ipltSMTP->ServerPort = m_ipEmailSettings->SMTPPort;
		m_ipltSMTP->Timeout = m_ipEmailSettings->SMTPTimeout;
		short nEmailStatus = m_ipltSMTP->Connect ();
		if ( nEmailStatus > EML_ERROR )
		{
			UCLIDException ue("ELI12364", "Error Connecting to SMTP Server." );
			loadLTEmailErrInfo(ue, nEmailStatus );
			string strServer = asString(m_ipEmailSettings->SMTPServer);
			ue.addDebugInfo("SMTP Server", strServer);
			ue.addDebugInfo("SMTP Port", m_ipEmailSettings->SMTPPort);
			throw ue;
		}
		string strUserName = m_ipEmailSettings->SMTPUserName;
		if ( !strUserName.empty())
		{
			// If the SMTP server requires authentication
			m_ipltSMTP->UserName =m_ipEmailSettings->SMTPUserName;

			// Get the stored password ( if it exists)
			string strEncryptedPassword =  m_ipEmailSettings->SMTPPassword;

			// Get the password 'key' based on the 4 hex global variables
			ByteStream pwBS;
			getEmailPassword(pwBS);

			// Stream to hold the decrypted PW
			ByteStream decryptedPW;

			// Decrypt the stored, encrypted PW
			EncryptionEngine ee;
			ee.decrypt(decryptedPW, strEncryptedPassword, pwBS);
			ByteStreamManipulator bsm( ByteStreamManipulator::kRead, decryptedPW );

			// Get the decrypted password from byte stream
			string strDecryptedPassword = "";
			bsm >> strDecryptedPassword;

			m_ipltSMTP->Password = get_bstr_t(strDecryptedPassword);
			nEmailStatus = m_ipltSMTP->Login();
			if ( nEmailStatus > EML_ERROR )
			{
				UCLIDException ue("ELI12365", "Error Logging into SMTP Server." );
				loadLTEmailErrInfo(ue, nEmailStatus );
				ue.addDebugInfo("UserName",  strUserName);
				throw ue;
			}
		}
	}
	return m_ipltSMTP;
}
//-------------------------------------------------------------------------------------------------
void CESMessage::setupSender()
{
	CSingleLock slg(&ms_mutex, TRUE );
	short nEmailStatus = getLtSMTP()->ClearMessageContents(MSG_CLEAR_AUTHORS);
	if ( nEmailStatus != EML_SUCCESS)
	{
		UCLIDException ue("ELI12372", "Unable to clear sender." );
		loadLTEmailErrInfo(ue, nEmailStatus );
		throw ue;
	}

	nEmailStatus = getLtSMTP()->AddAuthor(m_ipEmailSettings->SenderName, m_ipEmailSettings->SenderEmailAddr );
	if ( nEmailStatus != EML_SUCCESS)
	{
		UCLIDException ue("ELI12371", "Unable to set sender." );
		loadLTEmailErrInfo(ue, nEmailStatus );
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void CESMessage::addAttachments()
{
	CSingleLock slg(&ms_mutex, TRUE );
	long nNumAttachments = m_ipFileAttachments->Size;
	for ( long i = 0; i < nNumAttachments; i++ )
	{
		getLtSMTP()->AddAttachment(  _bstr_t(m_ipFileAttachments->GetItem(i)),  ATTACH_ENC_BASE64,  _bstr_t(m_ipFileAttachments->GetItem(i)),  "");
	}
}
//-------------------------------------------------------------------------------------------------
void CESMessage::addRecipients()
{
	CSingleLock slg(&ms_mutex, TRUE );
	long nNumRecipients = m_ipRecipients->Size;
	for ( long i = 0; i < nNumRecipients; i++ )
	{
		getLtSMTP()->AddRecipient(  _bstr_t(m_ipRecipients->GetItem(i)),  _bstr_t(m_ipRecipients->GetItem(i)), RCPT_TO);
	}
}
//-------------------------------------------------------------------------------------------------
