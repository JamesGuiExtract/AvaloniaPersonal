
#pragma once

class PersistStreamDefaultImpl : public IPersistStream
{
protected:
	PersistStreamDefaultImpl(CLSID clsID)
	:m_clsID(clsID)
	{
	}

	STDMETHOD(GetClassID)(CLSID *pClassID)
	{
		*pClassID = m_clsID;
		return S_OK;
	}

	STDMETHOD(IsDirty)(void)
	{
		return S_OK;
	}

	STDMETHOD(Load)(IStream *pStm)
	{
		return S_OK;
	}

	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty)
	{
		return S_OK;
	}

	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize)
	{
		return E_NOTIMPL;
	}

	CLSID m_clsID;
};
