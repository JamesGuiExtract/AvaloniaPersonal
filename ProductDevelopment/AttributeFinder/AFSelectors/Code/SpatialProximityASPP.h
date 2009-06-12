// SpatialProximityASPP.h : Declaration of the CSpatialProximityASPP

#pragma once
#include "resource.h"
#include "AFSelectors.h"

#include <string>
#include <map>
#include <utility>
using namespace std;

//--------------------------------------------------------------------------------------------------
// CSpatialProximityASPP
//--------------------------------------------------------------------------------------------------
class ATL_NO_VTABLE CSpatialProximityASPP :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CSpatialProximityASPP, &CLSID_SpatialProximityASPP>,
	public IPropertyPageImpl<CSpatialProximityASPP>,
	public CDialogImpl<CSpatialProximityASPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CSpatialProximityASPP();
	~CSpatialProximityASPP();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

	enum {IDD = IDD_SPATIALPROXIMITYASPP};

	DECLARE_REGISTRY_RESOURCEID(IDR_SPATIALPROXIMITYASPP)

	BEGIN_COM_MAP(CSpatialProximityASPP)
		COM_INTERFACE_ENTRY(IPropertyPage)
		COM_INTERFACE_ENTRY(IDispatch)
		COM_INTERFACE_ENTRY(ILicensedComponent)
	END_COM_MAP()

	BEGIN_MSG_MAP(CSpatialProximityASPP)
		CHAIN_MSG_MAP(IPropertyPageImpl<CSpatialProximityASPP>)
		MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	END_MSG_MAP()

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);

// IPropertyPage
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL *pbValue);

private:

	/////////////
	// Variables
	/////////////

	ATLControls::CEdit m_editTargetQuery;
	ATLControls::CEdit m_editReferenceQuery;
	ATLControls::CComboBox m_cmbInclusionMethod;
	ATLControls::CButton m_btnCompareLinesSeparately;
	ATLControls::CButton m_btnCompareOverallBounds;
	ATLControls::CButton m_btnIncludeDebugAttributes;

	struct BorderControls
	{
		ATLControls::CComboBox m_cmbBorder;
		ATLControls::CComboBox m_cmbRelation;
		ATLControls::CComboBox m_cmbExpandDirection;
		ATLControls::CEdit m_editExpandAmount;
		ATLControls::CComboBox m_cmbExpandUnits;
	};

	map<EBorder, BorderControls> m_mapBorderControls;

	/////////////
	// Methods
	/////////////
	
	// Set the state of the controls in m_mapBorderControls using the rule's settings.
	void loadBorderSettings(EBorder eBorder, UCLID_AFSELECTORSLib::ISpatialProximityASPtr ipRule);

	// Apply the state of the controls in m_mapBorderControls to the rule object.
	void saveBorderSettings(EBorder eBorder, UCLID_AFSELECTORSLib::ISpatialProximityASPtr ipRule);

	// Set the state of the combo box rcmbControl which applies to the border specified by
	// eBorder. The combo box should be filled with the data in the values parameter which
	// contains the number of entries specified by nValueCount.  Each entry of values a pair that 
	// maps an enum value to a name selectable by the user. The selected value should be 
	// eInitiaValue. 
	template <typename T>
	void loadComboControl(EBorder eBorder, ATLControls::CComboBox &rcmbControl, T eInitiaValue, 
						  const pair<T, string> values[], int nValueCount);

	// Retrieve the name associated with the specifed enum given the enum/name pair is
	// part of the provided array of values.
	template <typename T>
	string getValueName(T eValue, const pair<T, string> values[], int nCount);

	// Retrieve the enum associated with the specifed name given the enum/name pair is
	// part of the provided array of values.
	template <typename T>
	T getValueEnum(string strValue, const pair<T, string> values[], int nCount);

	// validate license
	void validateLicense();
};

OBJECT_ENTRY_AUTO(__uuidof(SpatialProximityASPP), CSpatialProximityASPP)
