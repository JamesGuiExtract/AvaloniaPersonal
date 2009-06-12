// CRLFParagraphTextCorrector.h : Declaration of the CCRLFParagraphTextCorrector

#ifndef __CRLFPARAGRAPHTEXTCORRECTOR_H_
#define __CRLFPARAGRAPHTEXTCORRECTOR_H_

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CCRLFParagraphTextCorrector
class ATL_NO_VTABLE CCRLFParagraphTextCorrector : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CCRLFParagraphTextCorrector, &CLSID_CRLFParagraphTextCorrector>,
	public ISupportErrorInfo,
	public IDispatchImpl<IParagraphTextCorrector, &IID_IParagraphTextCorrector, &LIBID_UCLID_SPOTRECOGNITIONIRLib>
{
public:
	CCRLFParagraphTextCorrector();
	~CCRLFParagraphTextCorrector();

DECLARE_REGISTRY_RESOURCEID(IDR_CRLFPARAGRAPHTEXTCORRECTOR)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CCRLFParagraphTextCorrector)
//DEL 	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IParagraphTextCorrector)
END_COM_MAP()


public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);
// IParagraphTextCorrector
	STDMETHOD(raw_CorrectText)(ISpatialString *pTextToCorrect);
};

#endif //__CRLFPARAGRAPHTEXTCORRECTOR_H_
