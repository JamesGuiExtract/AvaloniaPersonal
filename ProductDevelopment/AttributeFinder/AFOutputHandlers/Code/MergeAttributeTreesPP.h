// MergeAttributeTreesPP.h : Declaration of the CMergeAttributeTreesPP

#pragma once
#include "resource.h"
#include "AFOutputHandlers.h"

#include <string>
using namespace std;

//--------------------------------------------------------------------------------------------------
// CMergeAttributeTreesPP
//--------------------------------------------------------------------------------------------------
class ATL_NO_VTABLE CMergeAttributeTreesPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CMergeAttributeTreesPP, &CLSID_MergeAttributeTreesPP>,
	public IPropertyPageImpl<CMergeAttributeTreesPP>,
	public CDialogImpl<CMergeAttributeTreesPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CMergeAttributeTreesPP();
	~CMergeAttributeTreesPP();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

	enum {IDD = IDD_MERGEATTRIBUTETREESPP};

	DECLARE_REGISTRY_RESOURCEID(IDR_MERGEATTRIBUTETREESPP)

	BEGIN_COM_MAP(CMergeAttributeTreesPP)
		COM_INTERFACE_ENTRY(IPropertyPage)
		COM_INTERFACE_ENTRY(IDispatch)
		COM_INTERFACE_ENTRY(ILicensedComponent)
	END_COM_MAP()

	BEGIN_MSG_MAP(CMergeAttributeTreesPP)
		CHAIN_MSG_MAP(IPropertyPageImpl<CMergeAttributeTreesPP>)
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

	// Controls
	ATLControls::CEdit m_editAttributesToMergeQuery;
	ATLControls::CButton m_radMergeIntoFirst;
	ATLControls::CButton m_radMergeIntoBiggest;
	ATLControls::CEdit m_editSubAttributes;
	ATLControls::CButton m_radDiscardNonMatch;
	ATLControls::CButton m_radPreserveNonMatch;
	ATLControls::CButton m_chkCaseSensitive;
	ATLControls::CButton m_chkCompareTypeInfo;
	ATLControls::CButton m_chkCompareSubAttributes;

	/////////////
	// Methods
	/////////////

	// validate license
	void validateLicense();

	// Checks if the specified attribute query contains an invalid character
	static bool containsInvalidQueryString(const string& strAttributeQuery);
};

OBJECT_ENTRY_AUTO(__uuidof(MergeAttributeTreesPP), CMergeAttributeTreesPP)