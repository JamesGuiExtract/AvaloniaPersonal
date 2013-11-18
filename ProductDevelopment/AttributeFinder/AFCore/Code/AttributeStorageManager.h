#pragma once
#include "resource.h"

#include <string>
#include <map>
#include <set>
using namespace std;

//--------------------------------------------------------------------------------------------------
// CAttributeStorageManager
//--------------------------------------------------------------------------------------------------
class ATL_NO_VTABLE CAttributeStorageManager :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CAttributeStorageManager, &CLSID_AttributeStorageManager>,
	public IDispatchImpl<IStorageManager, &IID_IStorageManager, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IPersistStream,
	public ISupportErrorInfo
{
	public:
	CAttributeStorageManager();
	~CAttributeStorageManager();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

	DECLARE_REGISTRY_RESOURCEID(IDR_ATTRIBUTESTORAGEMANAGER)

	BEGIN_COM_MAP(CAttributeStorageManager)
		COM_INTERFACE_ENTRY(IStorageManager)
		COM_INTERFACE_ENTRY(ILicensedComponent)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY(IPersistStream)
	END_COM_MAP()

	// IStorageManager
	STDMETHOD(raw_PrepareForStorage)(IIUnknownVector *pDataToStore);
	STDMETHOD(raw_InitFromStorage)(IIUnknownVector *pDataToInit);

	// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL *pbValue);

	// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStream);
	STDMETHOD(Save)(IStream *pStream, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

	// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

private:

	/////////////////
	// Variables
	/////////////////

	// Keeps track of spatial page info data that has been collected by PrepareForStorage to persist
	// to disk.
	map<ILongToObjectMapPtr, set<long>> m_mapPageInfosToPersist;

	// Keeps track of spatial page info data loaded from disk to be assigned to the loaded
	// attributes.
	map<long, ILongToObjectMapPtr> m_mapLoadedPageInfos;

	bool m_bDirty;

	/////////////////
	// Methods
	/////////////////

	// Given the specified IUnknownVector of attributes, collects all duplicate spatial page info
	// copies into m_mapPageInfosToPersist such that they can be persisted in a way that avoids
	// persisting the same instances multiple times.
	void prepareAttributesForStorage(IIUnknownVectorPtr ipAttributes, long &rnAttributeIndex);

	// Given the specified IUnknownVector of attributes that have been loaded from disk, prepare
	// the spatial page infos for each give the data loaded into m_mapLoadedPageInfos.
	void initAttributesFromStorage(IIUnknownVectorPtr ipAttributes, long &rnAttributeIndex);

	// Validate license.
	void validateLicense();	
};

OBJECT_ENTRY_AUTO(__uuidof(AttributeStorageManager), CAttributeStorageManager)