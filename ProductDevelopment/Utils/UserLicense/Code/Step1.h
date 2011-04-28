#pragma once

#include "LicenseWizardPage.h"
#include <ByteStream.h>

#include <string>
#include <vector>
#include <map>
using namespace std;

class CLicenseRequest;
class IConfigurationSettingsPersistenceMgr;

//--------------------------------------------------------------------------------------------------
// CStep1
//--------------------------------------------------------------------------------------------------
class CStep1 : public CLicenseWizardPage
{
	DECLARE_DYNAMIC(CStep1)

public:
	CStep1(CLicenseRequest &licenseRequest);
	virtual ~CStep1();

// Dialog Data
	enum { IDD = IDD_STEP1 };

// Overrides
	virtual BOOL OnInitDialog();
	virtual BOOL OnSetActive();

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	DECLARE_MESSAGE_MAP()

// Message handlers
	afx_msg void OnSelChangeProduct();
	afx_msg void OnChangeInformation();

private:

	//////////////
	// Variables
	//////////////

	// Struct to encapsulate information about an Extract Systems product
	struct ProductInfo
	{
		// The GUID associated with the product's Window's installation
		string strGUID;

		// The product name as it appears in Window's Add/Remove programs list
		string strProductName;

		// The product version
		string strVersion;

		// The email to which license requests should be emailed.
		string strLicenseEmail;

		// Whether to prompt for a registration key for this product.
		bool bPromptForRegistrationKey;

		// The license types available for this product
		vector<string> vecLicenseTypes;
	};

	// Used to access Windows install information via the registry
	unique_ptr<IConfigurationSettingsPersistenceMgr> ma_pInstallRegistryMgr;

	// Used to keep track of the ProductInfo associated with each entry in the
	// products drop-down
	map<string, ProductInfo> m_mapProducts;

	// The current license request information.
	CLicenseRequest &m_licenseRequest;

	// Control variables
	CStatic m_staticRegistrationLabel;
	CEdit m_editRegistrationKey;
	CString m_zEmailAddress;
	CComboBox m_comboProductName;
	CComboBox m_comboType;
	
	//////////////
	// Methods
	//////////////

	//=======================================================================
	// PURPOSE: Validates the Name, Company Name, Phone Number, and Email
	//			edit boxes. In order to be valid, the edit box must contain
	//			non-empty data. Registration Key is optional and not validated.
	// REQUIRE: Nothing
	// PROMISE: If invalid, throws a UE and returns false. If all 4 are valid, 
	//			returns true.
	// ARGS:	None
	bool isValidInput();

	//=======================================================================
	// PURPOSE: To determine the user license key of the current machine
	//			and update the UI.
	void updateUserLicenseKeyInUI();

	//=======================================================================
	// PURPOSE: Searches for an installed product that uses the specified
	//			GUID and file.
	// ARGS:    strGUID- The product GUID under which the product should
	//				be installed
	//			strKeyFile- A core file required by the product which can be
	//				validated and from which version information can be read.
	//			strEmail- The email address to which license requests should 
	//				be sent for this product.
	//			bPromptForRegistrationKey- Whether the registration key
	//				edit box should be displayed for this product
	//			vecTypes- If provided, it will present the user with the
	//				contained entries as possible license types.
	void checkForProduct(const string &strGUID, const string &strKeyFile,
						 const string &strEmail, bool bPromptForRegistrationKey,
						 const vector<string> &vecTypes = vector<string>());
};
