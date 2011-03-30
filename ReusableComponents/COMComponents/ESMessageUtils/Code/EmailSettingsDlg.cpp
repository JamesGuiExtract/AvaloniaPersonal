// EmailSettingsDlg.cpp : implementation file
//

#include "stdafx.h"
#include "ESMessageUtils.h"
#include "EmailSettingsDlg.h"

#include <UCLIDException.h>
#include <COMUtils.h>
#include <PromptDlg.h>
#include <StringTokenizer.h>
#include <ByteStreamManipulator.h>
#include <EncryptionEngine.h>

#include <vector>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif


//-------------------------------------------------------------------------------------------------
// Declaration of function defined in ESMessage.cpp
//-------------------------------------------------------------------------------------------------
void getEmailPassword(ByteStream& rPasswordBytes);

//-------------------------------------------------------------------------------------------------
// Internal function
//-------------------------------------------------------------------------------------------------
string getEncryptedString(const string& strInput)
{
	// Put the input string into the byte manipulator
	ByteStream bytes;
	ByteStreamManipulator bytesManipulator(ByteStreamManipulator::kWrite, bytes);

	bytesManipulator << strInput;

	// Convert information to a stream of bytes
	// with length divisible by 8 (in variable called 'bytes')
	bytesManipulator.flushToByteStream( 8 );

	// Get the password 'key' based on the 4 hex global variables
	ByteStream pwBS;
	getEmailPassword( pwBS );

	// Do the encryption
	ByteStream encryptedBS;
	EncryptionEngine ee;
	ee.encrypt( encryptedBS, bytes, pwBS );

	// Return the encrypted value
	return encryptedBS.asString();
}

//-------------------------------------------------------------------------------------------------
// EmailSettingsDlg dialog
//-------------------------------------------------------------------------------------------------
EmailSettingsDlg::EmailSettingsDlg(ESMESSAGEUTILSLib::IEmailSettingsPtr ipEmailSettings, CWnd* pParent /*=NULL*/)
	: CDialog(EmailSettingsDlg::IDD, pParent), m_ipEmailSettings(ipEmailSettings)
{
	ASSERT_ARGUMENT("ELI12279", ipEmailSettings != __nullptr );
}
//-------------------------------------------------------------------------------------------------
EmailSettingsDlg::~EmailSettingsDlg()
{
	try
	{
		m_ipEmailSettings = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI25194");
}
//-------------------------------------------------------------------------------------------------
void EmailSettingsDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(EmailSettingsDlg)
	DDX_Control(pDX, IDC_USERNAME, m_editUserName);
	DDX_Control(pDX, IDC_SMTP_SERVER, m_editSMTPServer);
	DDX_Control(pDX, IDC_PASSWORD, m_editPassword);
	DDX_Control(pDX, IDC_AUTHENTICATION, m_checkAuthentication);
	DDX_Control(pDX, IDC_SENDER_DISPLAY_NAME, m_editSenderDisplayName);
	DDX_Control(pDX, IDC_SENDER_EMAIL_ADDR, m_editSenderEmailAddr);
	DDX_Control(pDX, IDC_SENDER_SIGNATURE, m_editSignature);

	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
// Message Map
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(EmailSettingsDlg, CDialog)
	//{{AFX_MSG_MAP(EmailSettingsDlg)
	ON_BN_CLICKED(IDC_AUTHENTICATION, OnAuthentication)
	ON_BN_CLICKED(IDC_BUTTON_SEND_TEST_EMAIL, &EmailSettingsDlg::OnBnClickedButtonSendTestEmail)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// EmailSettingsDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL EmailSettingsDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		CDialog::OnInitDialog();
		string strUserName = m_ipEmailSettings->SMTPUserName;
		if ( !strUserName.empty() )
		{
			m_checkAuthentication.SetCheck(BST_CHECKED);
		}
		else
		{
			m_checkAuthentication.SetCheck(BST_UNCHECKED);
		}
		m_editSMTPServer.SetWindowText(m_ipEmailSettings->SMTPServer);
		m_editUserName.SetWindowText(m_ipEmailSettings->SMTPUserName);

		// Get the stored password ( if it exists)
		string strEncryptedPassword =  m_ipEmailSettings->SMTPPassword;
		string strDecryptedPassword = "";

		if (!strEncryptedPassword.empty())
		{
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
			bsm >> strDecryptedPassword;
		}	

		m_editPassword.SetWindowText(strDecryptedPassword.c_str());
		m_editSenderDisplayName.SetWindowText( m_ipEmailSettings->SenderName);
		m_editSenderEmailAddr.SetWindowText( m_ipEmailSettings->SenderEmailAddr );
		m_editSignature.SetWindowText( m_ipEmailSettings->EmailSignature );
			
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18599");
	
	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
void EmailSettingsDlg::OnOK() 
{
	try
	{
		// Read settings from the dialog
		getSettingsFromDialog(m_ipEmailSettings);

		CDialog::OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12448");
}
//-------------------------------------------------------------------------------------------------
void EmailSettingsDlg::OnAuthentication() 
{
	try
	{
		updateControls();	
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI25186");
}
//-------------------------------------------------------------------------------------------------
void EmailSettingsDlg::OnBnClickedButtonSendTestEmail()
{
	try
	{
		// Setup PromptDlg
		PromptDlg dlgEmailAddresses("Send test email", "Enter email addresses separated by , or ;");

		// Display dialog
		if (dlgEmailAddresses.DoModal() == IDOK)
		{
			// Display wait cursor
			CWaitCursor cw;

			// Create EmailSettings object
			ESMESSAGEUTILSLib::IEmailSettingsPtr ipEmailSettings(CLSID_EmailSettings);
			ASSERT_RESOURCE_ALLOCATION("ELI25193", ipEmailSettings != __nullptr);

			// Get the settings from the controls
			getSettingsFromDialog(ipEmailSettings);
		
			// Create a new message
			ESMESSAGEUTILSLib::IESMessagePtr ipMessage(CLSID_ESMessage);
			ASSERT_RESOURCE_ALLOCATION("ELI25190", ipMessage != __nullptr);

			// Set the message settings
			ipMessage->EmailSettings = ipEmailSettings;

			// Set the message subject
			ipMessage->Subject = "This is a test!";

			// Get the recipient addresses
			ipMessage->Recipients = parseRecipientAddress((LPCSTR)dlgEmailAddresses.m_zInput);

			// Send the message
			ipMessage->Send();

			// Display dialog to indicate the message was sent successfully
			MessageBox("Test message sent successfully.", "Message Status",  MB_OK | MB_ICONINFORMATION);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI25187");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void EmailSettingsDlg::updateControls()
{
	if ( m_checkAuthentication.GetCheck() == BST_CHECKED )
	{
		m_editUserName.EnableWindow();
		m_editPassword.EnableWindow();
	}
	else
	{
		m_editUserName.EnableWindow(FALSE);
		m_editPassword.EnableWindow(FALSE);
	}

}
//-------------------------------------------------------------------------------------------------
void EmailSettingsDlg::getSettingsFromDialog(ESMESSAGEUTILSLib::IEmailSettingsPtr ipEmailSettings)
{
	try
	{
		ASSERT_ARGUMENT("ELI25204", ipEmailSettings != __nullptr);

		CString szSMTPServer;
		CString szUserName;
		CString szPassword;
		
		m_editSMTPServer.GetWindowText(szSMTPServer);
		
		// Make sure the email server name is not empty
		if ( szSMTPServer.IsEmpty() )
		{
			m_editSMTPServer.SetFocus();
			UCLIDException ue("ELI12447", "SMTP Server name must not be blank.");
			ue.addDebugInfo("SMTP Server", szSMTPServer.operator LPCTSTR() );
			throw ue;
		}
		ipEmailSettings->SMTPServer = get_bstr_t(szSMTPServer);
		
		if ( m_checkAuthentication.GetCheck() == BST_CHECKED )
		{
			m_editUserName.GetWindowText(szUserName);
			ipEmailSettings->SMTPUserName = get_bstr_t(szUserName);
			
			m_editPassword.GetWindowText(szPassword);
			ipEmailSettings->SMTPPassword = get_bstr_t(getEncryptedString(LPCSTR(szPassword)));
		}
		else
		{
			ipEmailSettings->SMTPUserName = "";
			ipEmailSettings->SMTPPassword = "";
		}
		CString szSenderDisplayName;
		m_editSenderDisplayName.GetWindowText(szSenderDisplayName );
		// Make sure the display name is not empty
		if (szSenderDisplayName.IsEmpty())
		{
			m_editSenderDisplayName.SetFocus();
			UCLIDException ue("ELI12449", "Sender display name must not be blank.");
			ue.addDebugInfo( "Sender display name", szSenderDisplayName.operator LPCTSTR() );
			throw ue;
		}
		ipEmailSettings->SenderName = get_bstr_t( szSenderDisplayName );
		
		CString szSenderAddr;
		m_editSenderEmailAddr.GetWindowText( szSenderAddr );
		// Get the position of the @ symbol that must be in a valid emailaddress
		int iPos = szSenderAddr.Find("@");
		// Make sure the address is not empty and contains a @ before the last character
		if ( szSenderAddr.IsEmpty() || (iPos <= 0)  || (iPos >= (szSenderAddr.GetLength() - 1)))
		{
			m_editSenderEmailAddr.SetFocus();
			UCLIDException ue("ELI12450", "Sender email address is not valid.");
			ue.addDebugInfo("Email Address", szSenderAddr.operator LPCTSTR());
			throw ue;
		}
		ipEmailSettings->SenderEmailAddr = get_bstr_t( szSenderAddr );
		
		CString szSignature;
		m_editSignature.GetWindowText( szSignature );
		ipEmailSettings->EmailSignature = get_bstr_t( szSignature );
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25189");
}
//-------------------------------------------------------------------------------------------------
IVariantVectorPtr EmailSettingsDlg::parseRecipientAddress(const string &strEmailAddresses)
{
	vector<string> vecAddresses;

	// Tokenize the addresses
	StringTokenizer::sGetTokens(strEmailAddresses, ",;", vecAddresses, true);

	// Put recipient on list of Recipients
	IVariantVectorPtr ipRecipients(CLSID_VariantVector);
	ASSERT_RESOURCE_ALLOCATION("ELI12605", ipRecipients != __nullptr );

	// Put addresses in return VariantVector
	int nSize = vecAddresses.size();
	for (int i = 0; i < nSize; i++)
	{
		// Only add non empty strings
		if ( !vecAddresses[i].empty())
		{
			ipRecipients->PushBack(vecAddresses[i].c_str());
		}
	}

	// Return list of recipients
	return ipRecipients;
}
//-------------------------------------------------------------------------------------------------
