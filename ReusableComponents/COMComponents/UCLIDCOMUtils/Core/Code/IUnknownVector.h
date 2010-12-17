// IUnknownVector.h : Declaration of the CIUnknownVector

#pragma once

#include "resource.h"       // main symbols

#include <comdef.h>
#include <vector>

/////////////////////////////////////////////////////////////////////////////
// CIUnknownVector
class ATL_NO_VTABLE CIUnknownVector : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CIUnknownVector, &CLSID_IUnknownVector>,
	public IPersistStream,
	public ISupportErrorInfo,
	public IDispatchImpl<IIUnknownVector, &IID_IIUnknownVector, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IComparableObject, &IID_IComparableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IShallowCopyable, &IID_IShallowCopyable, &LIBID_UCLID_COMUTILSLib>
{
public:
	CIUnknownVector();
	~CIUnknownVector();

	void FinalRelease();

DECLARE_REGISTRY_RESOURCEID(IDR_IUNKNOWNVECTOR)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CIUnknownVector)
	COM_INTERFACE_ENTRY(IIUnknownVector)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IIUnknownVector)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IShallowCopyable)
	COM_INTERFACE_ENTRY(IComparableObject)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ICopyableObject
	STDMETHOD(Clone)(/*[out, retval]*/ IUnknown* *pObject);
	STDMETHOD(CopyFrom)(/*[in]*/ IUnknown *pObject);

// IShallowCopyable
	STDMETHOD(ShallowCopy)(IUnknown** pObject);

// IIUnknownVector
	STDMETHOD(SaveTo)(/*[in]*/ BSTR strFullFileName, /*[in]*/ VARIANT_BOOL bClearDirty);
	STDMETHOD(LoadFrom)(/*[in]*/ BSTR strFullFileName, /*[in]*/ VARIANT_BOOL bSetDirtyFlagToTrue);
	STDMETHOD(Append)(/*[in]*/ IIUnknownVector *pVector);
	STDMETHOD(Remove)(/*[in]*/ long nIndex);
	STDMETHOD(PopBack)();
	STDMETHOD(Back)(/*[out, retval]*/ IUnknown **pObj);
	STDMETHOD(Front)(/*[out, retval]*/ IUnknown **pObj);
	STDMETHOD(Clear)();
	STDMETHOD(PushBack)(/*[in]*/ IUnknown *pObj);
	STDMETHOD(At)(/*[in]*/ long lPos, /*[out, retval]*/ IUnknown **pObj);
	STDMETHOD(Size)(/*[out, retval]*/ long *plSize);
	STDMETHOD(Insert)(/*[in]*/ long lPos, /*[in]*/ IUnknown *pObj);
	STDMETHOD(Set)(/*[in]*/ long lPos, /*[in]*/ IUnknown* pObj);
	STDMETHOD(Swap)(/*[in]*/ long lPos1, /*[in]*/ long lPos2);
	STDMETHOD(IsOrderFreeEqualTo)(IIUnknownVector *pVector, VARIANT_BOOL * pbValue);
	STDMETHOD(FindByValue)(/*[in]*/ IUnknown *pObj, /*[in]*/ long nStartIndex, /*[out, retval]*/ long *plIndex);
	STDMETHOD(InsertVector)(/*[in]*/ long lPos, /*[in]*/ IIUnknownVector *pObj);
	STDMETHOD(RemoveRange)(/*[in]*/ long nStart, /*[in]*/ long nEnd);
	STDMETHOD(RemoveValue)(/*[in]*/ IUnknown *pObj);
	STDMETHOD(At2)(/*[in]*/ long lPos, /*[out, retval]*/ IDispatch **pObj);
	STDMETHOD(PushBackIfNotContained)(/*[in]*/ IUnknown *pObj);
	STDMETHOD(FindByReference)(/*[in]*/ IUnknown *pObj, /*[in]*/ long nStartPos, /*[out, retval]*/ long *pRetVal);
	STDMETHOD(Sort)(/*[in]*/ ISortCompare* pSortCompare);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

//IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// IComparableObject
	STDMETHOD(IsEqualTo)(IUnknown * pObj, VARIANT_BOOL * pbValue);

private:
	////////////////
	// Classes
	////////////////
	class SortCompare
	{
	public:
		bool operator()(IUnknownPtr& rpObj1, IUnknownPtr& rpObj2);
		UCLID_COMUTILSLib::ISortComparePtr m_ipSortCompare;
	};
	////////////////
	// Methods
	////////////////
	//----------------------------------------------------------------------------------------------
	UCLID_COMUTILSLib::IIUnknownVectorPtr getThisAsCOMPtr();
	//----------------------------------------------------------------------------------------------
	// check license
	void validateLicense();
	//----------------------------------------------------------------------------------------------
	// throw exception if nIndex is not a valid index for m_vecIUnknowns
	void validateIndex(long nIndex);
	//----------------------------------------------------------------------------------------------
	// Appends the values in the specified vector to this vector
	void append(UCLID_COMUTILSLib::IIUnknownVectorPtr ipVector);
	//----------------------------------------------------------------------------------------------
	// Clears this vector
	void clear();
	//----------------------------------------------------------------------------------------------

	////////////////
	// Data
	////////////////
	// flag to store whether the object has been modified since the last
	// store-to-stream operation
	bool m_bDirty;

	std::vector<IUnknownPtr> m_vecIUnknowns;

	// the stream name used to store the vector within a storage object
	_bstr_t m_bstrStreamName;

};
