// SpatialProximityASPP.cpp : Implementation of CSpatialProximityASPP

#include "stdafx.h"
#include "SpatialProximityASPP.h"
#include "..\..\AFUtils\Code\Helper.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
const string gstrCOMPLETELY	= "are completely contained in";
const string gstrPARTIALLY	= "overlap with";
const string gstrCONTAINS	= "completely contain";

// A collection of enum/name pairs representing borders that can be specified.
const pair<EBorder, string> gBorderValues[4] = {
	pair<EBorder, string>(kLeft, "left"), 
	pair<EBorder, string>(kRight, "right"),
	pair<EBorder, string>(kTop, "top"),
	pair<EBorder, string>(kBottom, "bottom")};

// A collection of enum/name pairs representing which object's border to use.
const pair<EBorderRelation, string> gRelationValues[2] = {
	pair<EBorderRelation, string>(kReferenceAttibute, "reference attribute"),
	pair<EBorderRelation, string>(kPage, "page")};

// A collection of enum/name pairs representing which way a border can be expanded.
const pair<EBorderExpandDirection, string> gExpansionValues[4] = {
	pair<EBorderExpandDirection, string>(kExpandLeft, "left"),
	pair<EBorderExpandDirection, string>(kExpandRight, "right"),
	pair<EBorderExpandDirection, string>(kExpandUp, "up"),
	pair<EBorderExpandDirection, string>(kExpandDown, "down")};

// A collection of enum/name pairs representing the units used for the expansion amount.
const pair<EUnits, string> gUnitValues[3] = {
	pair<EUnits, string>(kCharacters, "characters"),
	pair<EUnits, string>(kInches, "inches"),
	pair<EUnits, string>(kLines, "lines")};

//--------------------------------------------------------------------------------------------------
// CSpatialProximityASPP
//--------------------------------------------------------------------------------------------------
CSpatialProximityASPP::CSpatialProximityASPP()
{
}
//--------------------------------------------------------------------------------------------------
CSpatialProximityASPP::~CSpatialProximityASPP()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI22587");
}
//--------------------------------------------------------------------------------------------------
HRESULT CSpatialProximityASPP::FinalConstruct()
{
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
void CSpatialProximityASPP::FinalRelease()
{
}

//--------------------------------------------------------------------------------------------------
// Windows message handlers
//--------------------------------------------------------------------------------------------------
LRESULT CSpatialProximityASPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFSELECTORSLib::ISpatialProximityASPtr ipRule = m_ppUnk[0];
		ASSERT_RESOURCE_ALLOCATION("ELI22588", ipRule);

		// Map controls to member variables
		m_editTargetQuery				= GetDlgItem(IDC_EDIT_TARGET_QUERY);
		m_editReferenceQuery			= GetDlgItem(IDC_EDIT_REFERENCE_QUERY);
		m_cmbInclusionMethod			= GetDlgItem(IDC_COMBO_INCLUSION_METHOD);
		m_btnCompareLinesSeparately		= GetDlgItem(IDC_RADIO_SEPARATE_LINES);
		m_btnCompareOverallBounds		= GetDlgItem(IDC_RADIO_OVERALL_BOUNDS);
		m_btnIncludeDebugAttributes		= GetDlgItem(IDC_CHECK_INCLUDE_DEBUG_ATTRIBUTES);

		m_mapBorderControls[kLeft] = BorderControls();
		m_mapBorderControls[kLeft].m_cmbBorder			= GetDlgItem(IDC_COMBO_BORDER_LEFT);
		m_mapBorderControls[kLeft].m_cmbRelation		= GetDlgItem(IDC_COMBO_REFERENCE_LEFT);
		m_mapBorderControls[kLeft].m_cmbExpandDirection = GetDlgItem(IDC_COMBO_EXPAND_DIR_LEFT);
		m_mapBorderControls[kLeft].m_editExpandAmount	= GetDlgItem(IDC_EDIT_EXPAND_AMOUNT_LEFT);
		m_mapBorderControls[kLeft].m_cmbExpandUnits		= GetDlgItem(IDC_COMBO_EXPAND_UNITS_LEFT);

		m_mapBorderControls[kTop] = BorderControls();
		m_mapBorderControls[kTop].m_cmbBorder			= GetDlgItem(IDC_COMBO_BORDER_TOP);
		m_mapBorderControls[kTop].m_cmbRelation			= GetDlgItem(IDC_COMBO_REFERENCE_TOP);
		m_mapBorderControls[kTop].m_cmbExpandDirection	= GetDlgItem(IDC_COMBO_EXPAND_DIR_TOP);
		m_mapBorderControls[kTop].m_editExpandAmount	= GetDlgItem(IDC_EDIT_EXPAND_AMOUNT_TOP);
		m_mapBorderControls[kTop].m_cmbExpandUnits		= GetDlgItem(IDC_COMBO_EXPAND_UNITS_TOP);

		m_mapBorderControls[kRight] = BorderControls();
		m_mapBorderControls[kRight].m_cmbBorder				= GetDlgItem(IDC_COMBO_BORDER_RIGHT);
		m_mapBorderControls[kRight].m_cmbRelation			= GetDlgItem(IDC_COMBO_REFERENCE_RIGHT);
		m_mapBorderControls[kRight].m_cmbExpandDirection	= GetDlgItem(IDC_COMBO_EXPAND_DIR_RIGHT);
		m_mapBorderControls[kRight].m_editExpandAmount		= GetDlgItem(IDC_EDIT_EXPAND_AMOUNT_RIGHT);
		m_mapBorderControls[kRight].m_cmbExpandUnits		= GetDlgItem(IDC_COMBO_EXPAND_UNITS_RIGHT);

		m_mapBorderControls[kBottom] = BorderControls();
		m_mapBorderControls[kBottom].m_cmbBorder			= GetDlgItem(IDC_COMBO_BORDER_BOTTOM);
		m_mapBorderControls[kBottom].m_cmbRelation			= GetDlgItem(IDC_COMBO_REFERENCE_BOTTOM);
		m_mapBorderControls[kBottom].m_cmbExpandDirection	= GetDlgItem(IDC_COMBO_EXPAND_DIR_BOTTOM);
		m_mapBorderControls[kBottom].m_editExpandAmount		= GetDlgItem(IDC_EDIT_EXPAND_AMOUNT_BOTTOM);
		m_mapBorderControls[kBottom].m_cmbExpandUnits		= GetDlgItem(IDC_COMBO_EXPAND_UNITS_BOTTOM);

		// Load the rule values into the property page.
		m_editTargetQuery.SetWindowText(asString(ipRule->TargetQuery).c_str());
		m_editReferenceQuery.SetWindowText(asString(ipRule->ReferenceQuery).c_str());

		m_cmbInclusionMethod.AddString(gstrCOMPLETELY.c_str());
		m_cmbInclusionMethod.AddString(gstrPARTIALLY.c_str());
		m_cmbInclusionMethod.AddString(gstrCONTAINS.c_str());

		if (asCppBool(ipRule->TargetsMustContainReferences))
		{
			m_cmbInclusionMethod.SelectString(-1, gstrCONTAINS.c_str());
		}
		else if (asCppBool(ipRule->RequireCompleteInclusion))
		{
			m_cmbInclusionMethod.SelectString(-1, gstrCOMPLETELY.c_str());
		}
		else
		{
			m_cmbInclusionMethod.SelectString(-1, gstrPARTIALLY.c_str());
		}

		m_btnCompareLinesSeparately.SetCheck(asBSTChecked(ipRule->CompareLinesSeparately));
		m_btnCompareOverallBounds.SetCheck(
			asBSTChecked(ipRule->CompareLinesSeparately == VARIANT_FALSE));
		m_btnIncludeDebugAttributes.SetCheck(
			asBSTChecked(ipRule->IncludeDebugAttributes == VARIANT_TRUE));

		loadBorderSettings(kLeft, ipRule);
		loadBorderSettings(kTop, ipRule);
		loadBorderSettings(kRight, ipRule);
		loadBorderSettings(kBottom, ipRule);
		
		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI22589");

	return 0;
}

//--------------------------------------------------------------------------------------------------
// IPropertyPage
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialProximityASPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ATLTRACE(_T("CSpatialProximityASPP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			// Obtain interface pointer to the IImageRegionWithLines class
			UCLID_AFSELECTORSLib::ISpatialProximityASPtr ipRule = m_ppUnk[i];
			ASSERT_RESOURCE_ALLOCATION("ELI22590", ipRule != __nullptr);

			// Apply settings to rule
			CComBSTR bstrTargetQuery;
			m_editTargetQuery.GetWindowText(&bstrTargetQuery);
			ipRule->TargetQuery = bstrTargetQuery.m_str;

			CComBSTR bstrReferenceQuery;
			m_editReferenceQuery.GetWindowText(&bstrReferenceQuery);
			ipRule->ReferenceQuery = bstrReferenceQuery.m_str;
			
			CComBSTR bstrInclusionMethod;
			m_cmbInclusionMethod.GetWindowText(&bstrInclusionMethod);
			
			if (asString(bstrInclusionMethod.m_str) == gstrCONTAINS)
			{
				ipRule->RequireCompleteInclusion = VARIANT_TRUE;
				ipRule->TargetsMustContainReferences = VARIANT_TRUE;
			}
			else
			{
				ipRule->RequireCompleteInclusion  =
					 asVariantBool(asString(bstrInclusionMethod.m_str) == gstrCOMPLETELY);
				ipRule->TargetsMustContainReferences = VARIANT_FALSE;
			}

			ipRule->CompareLinesSeparately =
				asVariantBool(m_btnCompareLinesSeparately.GetCheck() == BST_CHECKED);

			ipRule->IncludeDebugAttributes = 
				asVariantBool(m_btnIncludeDebugAttributes.GetCheck() == BST_CHECKED);
	
			saveBorderSettings(kLeft, ipRule);
			saveBorderSettings(kTop, ipRule);
			saveBorderSettings(kRight, ipRule);
			saveBorderSettings(kBottom, ipRule);
		}
		
		SetDirty(FALSE);

		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI22591");

	// If we reached here, it's because of an exception
	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialProximityASPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI22592", pbValue != __nullptr);

		try
		{
			// check the license
			validateLicense();

			// If no exception, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22593");
}

//--------------------------------------------------------------------------------------------------
// Private Methods
//--------------------------------------------------------------------------------------------------
void CSpatialProximityASPP::loadBorderSettings(EBorder eBorder,
											   UCLID_AFSELECTORSLib::ISpatialProximityASPtr ipRule)
{
	ASSERT_ARGUMENT("ELI22653", ipRule != __nullptr);

	// These are the settings pertaining to eBorder that need to be retrieved from the rule.
	EBorderRelation eBorderRelation;
	EBorder eReferenceBorder;
	EBorderExpandDirection eBorderExpandDirection;
	double dExpansionAmount;
	EUnits eUnits;

	// Retrive the settings.
	ipRule->GetRegionBorder((UCLID_AFSELECTORSLib::EBorder) eBorder,
							(UCLID_AFSELECTORSLib::EBorderRelation *) &eBorderRelation, 
							(UCLID_AFSELECTORSLib::EBorder *) &eReferenceBorder, 
							(UCLID_AFSELECTORSLib::EBorderExpandDirection *) &eBorderExpandDirection,
							&dExpansionAmount,
							(UCLID_AFSELECTORSLib::EUnits *) &eUnits);

	// Use the eBorder setting to initialize the border selection combo box
	loadComboControl(eBorder, m_mapBorderControls[eBorder].m_cmbBorder, eReferenceBorder, 
		&(gBorderValues[(eBorder == kLeft || eBorder == kRight) ? 0 : 2]), 2);
	// Use the eBorderRelation setting to initialize the border relation combo box
	loadComboControl(eBorder, m_mapBorderControls[eBorder].m_cmbRelation, eBorderRelation,
		gRelationValues, 2);
	// Use the eBorderExpandDirection setting to initialize the expansion direction combo box.
	loadComboControl(eBorder, m_mapBorderControls[eBorder].m_cmbExpandDirection, 
		eBorderExpandDirection, 
		&(gExpansionValues[(eBorder == kLeft || eBorder == kRight) ? 0 : 2]), 2);
	// Use the eUnits setting to initialize the expansion units combo box.
	loadComboControl(eBorder, m_mapBorderControls[eBorder].m_cmbExpandUnits, 
		eUnits, &(gUnitValues[(eBorder == kLeft || eBorder == kRight) ? 0 : 1]), 2);

	// Load dExpansionAmount into the expansion amount edit box
	m_mapBorderControls[eBorder].m_editExpandAmount.SetWindowText(
		asString(dExpansionAmount, 1, 5).c_str());
}
//--------------------------------------------------------------------------------------------------
void CSpatialProximityASPP::saveBorderSettings(EBorder eBorder,
											   UCLID_AFSELECTORSLib::ISpatialProximityASPtr ipRule)
{
	ASSERT_ARGUMENT("ELI22656", ipRule != __nullptr);

	// Retrive the settings from the combo boxes associated with eBorder.
	CComBSTR bstrBorder;
	m_mapBorderControls[eBorder].m_cmbBorder.GetWindowText(&bstrBorder);

	CComBSTR bstrBorderRelation;
	m_mapBorderControls[eBorder].m_cmbRelation.GetWindowText(&bstrBorderRelation);
	
	CComBSTR bstrExpandDirection;
	m_mapBorderControls[eBorder].m_cmbExpandDirection.GetWindowText(&bstrExpandDirection);

	CComBSTR bstrExpandUnits;
	m_mapBorderControls[eBorder].m_cmbExpandUnits.GetWindowText(&bstrExpandUnits);

	// Convert these selections into their corresponding enums.
	EBorder eReferenceBorder	= getValueEnum(asString(bstrBorder), gBorderValues, 4);
	EBorderRelation eBorderRelation
								= getValueEnum(asString(bstrBorderRelation), gRelationValues, 2);
	EBorderExpandDirection eExpandDirection
								= getValueEnum(asString(bstrExpandDirection), gExpansionValues, 4);
	EUnits eUnits				= getValueEnum(asString(bstrExpandUnits), gUnitValues, 3);

	// Set dExpansionAmount using the expansion amount edit box.
	string strErrorIfBlank = "Please specify the distance to expand the " + 
							 getValueName(eBorder, gBorderValues, 4) + 
							 " border in relation to the " + asString(bstrBorderRelation) +
							 " border.";
	string strErrorIfNegative = "Expansion amount must be a positive number.";
	double dExpansionAmount = verifyControlValueAsDouble(
		m_mapBorderControls[eBorder].m_editExpandAmount, 0, 9999999, strErrorIfNegative, 
		0, strErrorIfBlank);

	// Apply these settings to the rule object.
	ipRule->SetRegionBorder((UCLID_AFSELECTORSLib::EBorder) eBorder,
							(UCLID_AFSELECTORSLib::EBorderRelation) eBorderRelation, 
							(UCLID_AFSELECTORSLib::EBorder) eReferenceBorder, 
							(UCLID_AFSELECTORSLib::EBorderExpandDirection)eExpandDirection,
							dExpansionAmount,
							(UCLID_AFSELECTORSLib::EUnits) eUnits);
}
//--------------------------------------------------------------------------------------------------
template <typename T>
void CSpatialProximityASPP::loadComboControl(EBorder eBorder, ATLControls::CComboBox &rcmbControl, 
											 T eInitiaValue, const pair<T, string> values[], 
											 int nValueCount)
{
	// Remove any existing values.
	rcmbControl.Clear();
	for (int i = 0; i < nValueCount; i++)
	{
		// Add each option name from the values array.
		rcmbControl.AddString(values[i].second.c_str());
	}

	// Select the name that corresponds to the eInitiaValue enum.
	rcmbControl.SelectString(-1, getValueName(eInitiaValue, values, nValueCount).c_str());
}
//--------------------------------------------------------------------------------------------------
template <typename T>
string CSpatialProximityASPP::getValueName(T eValue, const pair<T, string> values[], int nCount)
{
	// Search each option for the one whose enum matches eValue
	for (int i = 0; i < nCount; i++)
	{
		if (eValue == values[i].first)
		{
			// Return the name of this option.
			return values[i].second;
		}
	}

	THROW_LOGIC_ERROR_EXCEPTION("ELI22657");
}
//--------------------------------------------------------------------------------------------------
template <typename T>
T CSpatialProximityASPP::getValueEnum(string strValue, const pair<T, string> values[], int nCount)
{
	// Search each option for the one whose name matches strValue
	for (int i = 0; i < nCount; i++)
	{
		if (strValue == values[i].second)
		{
			// Return the enum for this option.
			return values[i].first;
		}
	}

	THROW_LOGIC_ERROR_EXCEPTION("ELI22658");
}
//--------------------------------------------------------------------------------------------------
void CSpatialProximityASPP::validateLicense()
{
	VALIDATE_LICENSE(gnRULESET_EDITOR_UI_OBJECT, "ELI22594", 
		"Spatial proximity attribute selector PP");
}
//--------------------------------------------------------------------------------------------------
