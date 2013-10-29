#pragma once
#include "resource.h"
#include "AFUtils.h"

#include <string>
#include <vector>
#include <map>
#include <set>
using namespace std;

//--------------------------------------------------------------------------------------------------
// CAFExpressionFormatter
//--------------------------------------------------------------------------------------------------
class ATL_NO_VTABLE CAFExpressionFormatter :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CAFExpressionFormatter, &CLSID_AFExpressionFormatter>,
	public IDispatchImpl<IAFExpressionFormatter, &IID_IAFExpressionFormatter, &LIBID_UCLID_AFUTILSLib>,
	public IDispatchImpl<IExpressionFormatter, &IID_IExpressionFormatter, &LIBID_UCLID_COMUTILSLib>,
	public ISupportErrorInfo
{
	public:
	CAFExpressionFormatter();
	~CAFExpressionFormatter();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

	DECLARE_REGISTRY_RESOURCEID(IDR_AFEXPRESSIONFORMATTER)

	BEGIN_COM_MAP(CAFExpressionFormatter)
		COM_INTERFACE_ENTRY(IAFExpressionFormatter)
		COM_INTERFACE_ENTRY2(IDispatch, IAFExpressionFormatter)
		COM_INTERFACE_ENTRY(IExpressionFormatter)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
	END_COM_MAP()

	// IAFExpressionFormatter
	STDMETHOD(get_AFDocument)(IAFDocument **ppDoc);
	STDMETHOD(put_AFDocument)(IAFDocument *pDoc);

	// IExpressionFormatter
	STDMETHOD(raw_FormatExpression)(BSTR bstrExpression, BSTR* pbstrFormatedExpression);

	// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

private:

	/////////////////
	// Variables
	/////////////////

	// The AFDocument will provide the referenceable attributes for the format string.
	IAFDocumentPtr m_ipAFDocument;

	// Provides the ExpandFormatString method to expand format strings.
	UCLID_AFUTILSLib::IAFUtilityPtr m_ipAFUtility;

	/////////////////
	// Methods
	/////////////////

	// Validate license.
	void validateLicense();	
};

OBJECT_ENTRY_AUTO(__uuidof(AFExpressionFormatter), CAFExpressionFormatter)