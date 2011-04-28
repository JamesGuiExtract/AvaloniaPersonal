// Step1.cpp : implementation file

#include "stdafx.h"
#include "UserLicense.h"
#include "Step1.h"
#include "LicenseRequest.h"

#include <Win32Util.h>
#include <UCLIDException.h>
#include <ByteStreamManipulator.h>
#include <EncryptionEngine.h>
#include <RegistryPersistenceMgr.h>
#include <LicenseMgmt.h>

DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
static const string gstrDEST_EMAIL_ADDRESS_ICOMAP = "register@extractsystems.com";
static const string gstrDEST_EMAIL_ADDRESS_NON_ICOMAP = "flex-license@extractsystems.com";
static const string gstrDEST_EMAIL_ADDRESS_IDSO  = "idso-license@extractsystems.com";
static const string gstrDEFAULT_PRODUCT_NAME = "Extract Systems software";

static const string gstrWINDOWS_INSTALL_INFO_KEY = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
static const string gstrRELATIVE_PATH_TO_ICOMAP_APP_DLL = "\\IcoMapApp.dll";

static const string gstrFLEXINDEX_GUID = "{A7DFE34D-A07E-4D57-A624-B758E42A69D4}";
static const string gstrFLEXINDEX_KEY_FILE = "\\CountyCustomComponents.dll";

static const string gstrIDSHIELD_GUID = "{158160CD-7B55-462F-8477-7E18B2937D40}";
static const string gstrIDSHIELD_KEY_FILE = "\\RedactionCC.dll";

static const string gstrLASERFICHE_GUID = "{74E1DF54-C96E-479F-915D-A0A721F9AC8B}";
static const string gstrLASERFICHE_KEY_FILE = "\\ESLaserficheCC.dll";

static const string gstrIDSO_GUID = "{A8DDFDC1-069D-42DE-AF69-A78FC232A86A}";
static const string gstrIDSO_KEY_FILE = "\\IDShieldOffice.exe";

static const string gstrLABDE_GUID = "{0E412937-E4FA-4737-A321-00AED69497C7}";
static const string gstrLABDE_KEY_FILE = "\\Extract.LabResultsCustomComponents.dll";
static const string gstrLABDE_VERSION_FILE = "\\Extract.LabDE.StandardLabDE.dll";

static const string gstrICOMAP_GUID = "{0B7B53B1-B2F3-4EA0-97F9-D1280C11A892}";
static const string gstrICOMAP_KEY_FILE = gstrRELATIVE_PATH_TO_ICOMAP_APP_DLL;

//--------------------------------------------------------------------------------------------------
// CStep1
//--------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CStep1, CLicenseWizardPage)
//--------------------------------------------------------------------------------------------------
CStep1::CStep1(CLicenseRequest &licenseRequest)
	: CLicenseWizardPage(CStep1::IDD)
	, m_licenseRequest(licenseRequest)
{
	try
	{
		// determine whether we should prompt for the registration key
		// by default, we will not prompt.  We will only prompt if 
		// we detect this to be an IcoMap installation
		string strIcoMapAppDLLFullPath = 
			getDirectoryFromFullPath(getCurrentProcessEXEFullPath());
		strIcoMapAppDLLFullPath += gstrRELATIVE_PATH_TO_ICOMAP_APP_DLL;

		// Initialize ma_pInstallRegistryMgr to the installation info area of the
		// registry.
		ma_pInstallRegistryMgr.reset(new RegistryPersistenceMgr(HKEY_LOCAL_MACHINE,
			gstrWINDOWS_INSTALL_INFO_KEY));
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI15276");
}
//--------------------------------------------------------------------------------------------------
CStep1::~CStep1()
{
	try
	{
		ma_pInstallRegistryMgr.reset();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI21565")
}
//--------------------------------------------------------------------------------------------------
void CStep1::DoDataExchange(CDataExchange* pDX)
{
	CLicenseWizardPage::DoDataExchange(pDX);
	DDX_Text(pDX, IDC_EDIT_KEY, m_licenseRequest.m_zKey);
	DDX_Text(pDX, IDC_EDIT_NAME, m_licenseRequest.m_zName);
	DDX_Text(pDX, IDC_EDIT_COMPANY, m_licenseRequest.m_zCompany);
	DDX_Text(pDX, IDC_EDIT_PHONE, m_licenseRequest.m_zPhone);
	DDX_Text(pDX, IDC_EDIT_EMAIL, m_licenseRequest.m_zEmail);
	DDX_Text(pDX, IDC_EDIT_REGISTRATION, m_licenseRequest.m_zRegistration);
	DDX_Control(pDX, IDC_STATIC_REGISTRATION, m_staticRegistrationLabel);
	DDX_Control(pDX, IDC_EDIT_REGISTRATION, m_editRegistrationKey);
	DDX_Control(pDX, IDC_COMBO_PRODUCT, m_comboProductName);
	DDX_Text(pDX, IDC_EDIT_VERSION, m_licenseRequest.m_zVersion);
	DDX_Control(pDX, IDC_COMBO_TYPE, m_comboType);
	DDX_Text(pDX, IDC_COMBO_TYPE, m_licenseRequest.m_zType);
}
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CStep1, CLicenseWizardPage)
	ON_CBN_SELCHANGE(IDC_COMBO_PRODUCT, &CStep1::OnSelChangeProduct)
	ON_EN_CHANGE(IDC_EDIT_NAME, &CStep1::OnChangeInformation)
	ON_EN_CHANGE(IDC_EDIT_COMPANY, &CStep1::OnChangeInformation)
	ON_EN_CHANGE(IDC_EDIT_PHONE, &CStep1::OnChangeInformation)
	ON_EN_CHANGE(IDC_EDIT_EMAIL, &CStep1::OnChangeInformation)
	ON_EN_CHANGE(IDC_EDIT_KEY, &CStep1::OnChangeInformation)
	ON_CBN_SELCHANGE(IDC_COMBO_TYPE, &CStep1::OnChangeInformation)
END_MESSAGE_MAP()

//--------------------------------------------------------------------------------------------------
// Message handlers
//--------------------------------------------------------------------------------------------------
void CStep1::OnSelChangeProduct()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Save the dialogs data, so that changes are not lost.
		UpdateData(TRUE);

		m_comboType.ResetContent();

		int nCurSel = m_comboProductName.GetCurSel();
		if (nCurSel != CB_ERR)
		{
			// Get the ProductInfo for the selected product.
			m_comboProductName.GetLBText(nCurSel, m_licenseRequest.m_zProductName);
			ProductInfo productInfo = m_mapProducts[m_licenseRequest.m_zProductName.GetString()];
			m_licenseRequest.m_zProductName = productInfo.strProductName.c_str();
			
			// Update the product version control
			m_licenseRequest.m_zVersion = productInfo.strVersion.c_str();

			// Display the available license types for this product
			for each (string strType in productInfo.vecLicenseTypes)
			{
				m_comboType.AddString(strType.c_str());
			}

			m_licenseRequest.m_bUseRegistrationKey = productInfo.bPromptForRegistrationKey;

			// Make the registration key related fields visible only if that it should be prompted for
			m_staticRegistrationLabel.ShowWindow(productInfo.bPromptForRegistrationKey ? SW_SHOW : SW_HIDE);
			m_editRegistrationKey.ShowWindow(productInfo.bPromptForRegistrationKey ? SW_SHOW : SW_HIDE);

			// Set the email where license requests should be sent.
			m_licenseRequest.m_strLicenseEmailAddress = productInfo.strLicenseEmail;

			UpdateData(FALSE);

			// The product type combo should be enabled only if there is a specified list of types.
			m_comboType.EnableWindow(m_comboType.GetCount() > 0);

			OnChangeInformation();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI23365");
}
//--------------------------------------------------------------------------------------------------
void CStep1::OnChangeInformation()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Save the data the user entered to the m_licenseRequest instance.
		UpdateData(TRUE);

		// Enable the next button if all the necessary information is entered
		CPropertySheet *pWizard = (CPropertySheet *)this->GetParent();
		ASSERT_RESOURCE_ALLOCATION("ELI23337", pWizard != NULL);

		pWizard->SetWizardButtons( isValidInput() ? PSWIZB_NEXT : 0);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI23364");
}

//--------------------------------------------------------------------------------------------------
// Overrides
//--------------------------------------------------------------------------------------------------
BOOL CStep1::OnInitDialog()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		CLicenseWizardPage::OnInitDialog();

		// Default email address to the non-IcoMap email address
		m_zEmailAddress = gstrDEST_EMAIL_ADDRESS_NON_ICOMAP.c_str();

		// update the user license key in the dialog
		updateUserLicenseKeyInUI();

		// Create a vector to offer a choice between a Client or Server license types for
		// FLEX Index & ID Shield
		vector<string> vecClientServerTypes;
		vecClientServerTypes.push_back("Client");
		vecClientServerTypes.push_back("Server");

		// [FlexIDSIntegrations:99] The Laserfiche integration now has 3 available license levels.
		vector<string> vecLaserficheTypes;
		vecLaserficheTypes.push_back("Verification only");
		vecLaserficheTypes.push_back("Desktop redaction");
		vecLaserficheTypes.push_back("Server redaction");

		// Check for IcoMap, FLEX Index, ID Shield and ID Shield for Laserfiche, 
		// ID Shield Office and LabDE installations
		checkForProduct(gstrICOMAP_GUID, gstrICOMAP_KEY_FILE, gstrDEST_EMAIL_ADDRESS_ICOMAP, 
						true);
		checkForProduct(gstrFLEXINDEX_GUID, gstrFLEXINDEX_KEY_FILE, gstrDEST_EMAIL_ADDRESS_NON_ICOMAP,
						false, vecClientServerTypes);
		checkForProduct(gstrIDSHIELD_GUID, gstrIDSHIELD_KEY_FILE, gstrDEST_EMAIL_ADDRESS_NON_ICOMAP,
						false, vecClientServerTypes);
		checkForProduct(gstrLASERFICHE_GUID, gstrLASERFICHE_KEY_FILE, gstrDEST_EMAIL_ADDRESS_NON_ICOMAP,
						false, vecLaserficheTypes);
		checkForProduct(gstrIDSO_GUID, gstrIDSO_KEY_FILE, gstrDEST_EMAIL_ADDRESS_IDSO, false);
		checkForProduct(gstrLABDE_GUID, gstrLABDE_KEY_FILE, gstrDEST_EMAIL_ADDRESS_NON_ICOMAP, false,
						vecClientServerTypes);
		
		// If no products appear to be installed, create a generic default entry
		if (m_comboProductName.GetCount() == 0)
		{
			ProductInfo productInfo;
			productInfo.strLicenseEmail = gstrDEST_EMAIL_ADDRESS_NON_ICOMAP;
			productInfo.bPromptForRegistrationKey = false;
			productInfo.strProductName = gstrUNKNOWN;
			productInfo.strVersion = gstrUNKNOWN;

		    m_mapProducts[gstrDEFAULT_PRODUCT_NAME] = productInfo;
			m_comboProductName.AddString(gstrDEFAULT_PRODUCT_NAME.c_str());
		}
		
		m_comboProductName.SetCurSel(0);


		// Initialize the data displayed in the product controls
		OnSelChangeProduct();

		//Set the focus to Name edit box.
		CEdit *pEdit;
		pEdit = (CEdit *)GetDlgItem( IDC_EDIT_NAME );
		ASSERT_RESOURCE_ALLOCATION("ELI13452", pEdit != NULL);
		pEdit->SetFocus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15273");

	// return FALSE because you set the focus to a control
	return FALSE;
}
//--------------------------------------------------------------------------------------------------
BOOL CStep1::OnSetActive()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Enable the next button if all the necessary information is entered
		CPropertySheet *pWizard = (CPropertySheet *)this->GetParent();
		ASSERT_RESOURCE_ALLOCATION("ELI23366", pWizard != NULL);

		pWizard->SetWizardButtons(isValidInput() ? PSWIZB_NEXT : 0);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI23367");

	return CLicenseWizardPage::OnSetActive();
}

//--------------------------------------------------------------------------------------------------
// Private Methods
//--------------------------------------------------------------------------------------------------
void CStep1::updateUserLicenseKeyInUI() 
{
	m_licenseRequest.m_zKey = LicenseManagement::getUserLicense(LICENSE_MGMT_PASSWORD).c_str();
	UpdateData(FALSE);
}
//--------------------------------------------------------------------------------------------------
bool CStep1::isValidInput()
{
	//trim the white space to prevent "   " type entries
	m_licenseRequest.m_zName.Trim(" ");
	//check if there is data in the edit box
	if( m_licenseRequest.m_zName.IsEmpty() )
	{		
		//the name is not valid, so return false
		return false;		
	}

	m_licenseRequest.m_zCompany.Trim(" ");
	if( m_licenseRequest.m_zCompany.IsEmpty() )
	{
		return false;
	}

	m_licenseRequest.m_zPhone.Trim(" ");
	if( m_licenseRequest.m_zPhone.IsEmpty() )
	{
		return false;
	}

	m_licenseRequest.m_zEmail.Trim(" ");
	if( m_licenseRequest.m_zEmail.IsEmpty() )
	{
		return false;
	}

	if (m_comboType.GetCount() > 0 && m_comboType.GetCurSel() == CB_ERR)
	{
		return false;
	}

	return true;
}
//--------------------------------------------------------------------------------------------------
void CStep1::checkForProduct(const string &strGUID, const string &strKeyFile,
									  const string &strEmail, bool bPromptForRegistrationKey,
									  const vector<string> &vecTypes/* = vector<string>()*/)
{
	try
	{
		// Retrieve the product name from the registry
		string strProductName = ma_pInstallRegistryMgr->getKeyValue("\\" + strGUID, "DisplayName");

		// If we found a product name, the product is installed according to Windows.
		if (!strProductName.empty())
		{
			ProductInfo productInfo;
			productInfo.strGUID = strGUID;
			productInfo.strProductName = strProductName;
			productInfo.strLicenseEmail = strEmail;
			productInfo.bPromptForRegistrationKey = bPromptForRegistrationKey;
			productInfo.vecLicenseTypes = vecTypes;

			string strPath = getDirectoryFromFullPath(getAppFullPath());
			string strKeyFileFullPath = strPath + strKeyFile;

			// Verify we can find the specified key file.
			validateFileOrFolderExistence(strKeyFileFullPath);

			// For LabDE, use the version of DataEntry dll instead.
			if (strKeyFile == gstrLABDE_KEY_FILE)
			{
				strKeyFileFullPath = strPath + gstrLABDE_VERSION_FILE;
			}

			// Retrieve the product version from the key file.
			productInfo.strVersion = ::getFileVersion(strKeyFileFullPath);
			if (productInfo.strVersion.empty())
			{
				productInfo.strVersion = gstrUNKNOWN;
			}

			m_mapProducts[strProductName] = productInfo;
			m_comboProductName.AddString(strProductName.c_str());
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI21564")
}
//--------------------------------------------------------------------------------------------------
