// Letter.h : Declaration of the CLetter

#pragma once

#include "resource.h"       // main symbols
#include "CPPLetter.h"

/////////////////////////////////////////////////////////////////////////////
// CLetter
class ATL_NO_VTABLE CLetter : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CLetter, &CLSID_Letter>,
	public IPersistStream,
	public ISupportErrorInfo,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILetter, &IID_ILetter, &LIBID_UCLID_RASTERANDOCRMGMTLib>
{
public:
	CLetter();

DECLARE_REGISTRY_RESOURCEID(IDR_LETTER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CLetter)
	COM_INTERFACE_ENTRY(ILetter)
	COM_INTERFACE_ENTRY2(IDispatch, ILetter)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILetter
	STDMETHOD(ToUpperCase)();
	STDMETHOD(ToLowerCase)();
	STDMETHOD(get_Guess3)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_Guess3)(/*[in]*/ long newVal);
	STDMETHOD(get_Guess2)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_Guess2)(/*[in]*/ long newVal);
	STDMETHOD(get_Guess1)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_Guess1)(/*[in]*/ long newVal);
	STDMETHOD(get_Bottom)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_Bottom)(/*[in]*/ long newVal);
	STDMETHOD(get_Right)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_Right)(/*[in]*/ long newVal);
	STDMETHOD(get_Top)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_Top)(/*[in]*/ long newVal);
	STDMETHOD(get_Left)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_Left)(/*[in]*/ long newVal);
	STDMETHOD(get_IsEndOfParagraph)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IsEndOfParagraph)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_IsEndOfZone)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IsEndOfZone)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_IsSpatialChar)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IsSpatialChar)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_PageNumber)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_PageNumber)(/*[in]*/ long newVal);
	STDMETHOD(SetAsNonSpatialChar)(/*[in]*/ long nGuess1);
	STDMETHOD(Offset)(/*[in]*/ long nX, /*[in]*/ long nY);
	STDMETHOD(get_FontSize)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_FontSize)(/*[in]*/ long newVal);
	STDMETHOD(get_CharConfidence)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_CharConfidence)(/*[in]*/ long newVal);
	STDMETHOD(get_IsItalic)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IsItalic)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_IsBold)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IsBold)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_IsSansSerif)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IsSansSerif)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_IsSerif)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IsSerif)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_IsProportional)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IsProportional)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_IsUnderline)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IsUnderline)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_IsSuperScript)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IsSuperScript)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_IsSubScript)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IsSubScript)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(CreateFromCppLetter)(void* pCppLetter);
	STDMETHOD(GetCppLetter)(void* pCppLetter);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown **pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown *pObject);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

private:
	/*
	// The following variables correspond to the variables of the
	// LETTER structure in the Scansoft RecApi.
	unsigned short m_usGuess1, m_usGuess2, m_usGuess3;
	unsigned long m_ulTop, m_ulLeft, m_ulRight, m_ulBottom;
	
	// max number of pages per document is limited to 255
	unsigned char m_ucPageNumber;
	bool m_bIsEndOfParagraph, m_bIsEndOfZone;
	bool m_bIsSpatialChar;
	*/
	CPPLetter m_letter;
	
	// boolean variable to keep track of the "dirty" state of this 
	// object since the last save or load operation
	bool m_bDirty;
	
	// The reset method resets all member variables to their initial
	// default values (such as zero for longs, false, for booleans, etc)
	void reset();

	void validateLicense();

	void setFontFlag(unsigned char flag, bool bSet);
};
