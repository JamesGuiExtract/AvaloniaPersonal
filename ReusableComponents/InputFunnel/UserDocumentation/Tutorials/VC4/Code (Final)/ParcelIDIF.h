// ParcelIDIF.h : Declaration of the CParcelIDIF

#ifndef __PARCELIDIF_H_
#define __PARCELIDIF_H_

#include "resource.h"       // main symbols

#include <string>

#import "UCLIDCOMUtils.dll" raw_interfaces_only, raw_native_types, no_namespace, named_guids 
#import "InputFinders.dll" raw_interfaces_only, raw_native_types, no_namespace, named_guids 

///////////////////////////////////////
// Definitions for Category ID and Name
///////////////////////////////////////
const std::string HT_IF_CATEGORYNAME = "UCLID Highlighted Text Input Finders";

// {01236221-458A-11d6-826D-0050DAD4FF55}
static const GUID CATID_HT_INPUT_FINDERS = 
{ 0x1236221, 0x458a, 0x11d6, { 0x82, 0x6d, 0x0, 0x50, 0xda, 0xd4, 0xff, 0x55 } };


/////////////////////////////////////////////////////////////////////////////
// CParcelIDIF
class ATL_NO_VTABLE CParcelIDIF : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CParcelIDIF, &CLSID_ParcelIDIF>,
	public ISupportErrorInfo,
	public IDispatchImpl<IParcelIDIF, &IID_IParcelIDIF, &LIBID_PARCELIDFINDERLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLIDCOMUTILSLib>,
	public IDispatchImpl<IInputFinder, &IID_IInputFinder, &LIBID_UCLIDINPUTFINDERSLib>
{
public:
	CParcelIDIF()
	{
	}

DECLARE_REGISTRY_RESOURCEID(IDR_PARCELIDIF)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CParcelIDIF)
	COM_INTERFACE_ENTRY(IParcelIDIF)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(IInputFinder)
END_COM_MAP()

BEGIN_CATEGORY_MAP(CIcoMapDrawingCtrl)
	IMPLEMENTED_CATEGORY(CATID_HT_INPUT_FINDERS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IInputFinder
	STDMETHOD(ParseString)(BSTR strInput, IIUnknownVector * * ippTokenPositions);

// ICategorizedComponent
	STDMETHOD(GetComponentDescription)(BSTR * pbstrComponentDescription);
};

#endif //__PARCELIDIF_H_
