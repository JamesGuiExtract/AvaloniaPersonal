// EventSink.cpp : Implementation of CEventSink
#include "stdafx.h"
#include "GridGenerator.h"
#include "EventSink.h"

#include <UCLIDException.h>

//-------------------------------------------------------------------------------------------------
// CEventSink
//-------------------------------------------------------------------------------------------------
CEventSink::CEventSink()
: m_bIsEditEnabled(false)
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEventSink::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IEventSink
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IEditEvents
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEventSink::raw_OnSelectionChanged()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

/*	try
	{

	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS()*/

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEventSink::raw_OnCurrentLayerChanged()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

/*	try
	{

	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS()*/

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEventSink::raw_OnCurrentTaskChanged()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_pDrawGridCtrl->m_bEnbleTool = m_bIsEditEnabled && isCurrentTaskRight();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08192")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEventSink::raw_OnSketchModified()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

/*	try
	{		
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS()*/

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEventSink::raw_OnSketchFinished()
{
	return E_NOTIMPL;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEventSink::raw_AfterDrawSketch(IDisplay *pDpy)
{
	return E_NOTIMPL;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEventSink::raw_OnStartEditing()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// if current task is 'Create New Feature'
		m_bIsEditEnabled = true;
		m_pDrawGridCtrl->m_bEnbleTool = m_bIsEditEnabled && isCurrentTaskRight();
		m_pDrawGridCtrl->createAllFields();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08190");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEventSink::raw_OnStopEditing(VARIANT_BOOL Save)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_bIsEditEnabled = false;
		m_pDrawGridCtrl->m_bEnbleTool = m_bIsEditEnabled;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08191")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEventSink::raw_OnConflictsDetected()
{
	return E_NOTIMPL;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEventSink::raw_OnUndo()
{
	return E_NOTIMPL;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEventSink::raw_OnRedo()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return E_NOTIMPL;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEventSink::raw_OnCreateFeature(IObject *obj)
{
	return E_NOTIMPL;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEventSink::raw_OnChangeFeature(IObject *obj)
{
	return E_NOTIMPL;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEventSink::raw_OnDeleteFeature(IObject *obj)
{
	return E_NOTIMPL;
}

//--------------------------------------------------------------------------------------------------
// IEventSink
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEventSink::SetApplicationHook(IApplication *pApp)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_ipApp = pApp;
		
		// set m_ipEditor
		_bstr_t sName("ESRI Object Editor");
		IExtensionPtr ipExtension(m_ipApp->FindExtensionByName(sName));
		m_ipEditor = ipExtension;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08106")

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
bool CEventSink::isCurrentTaskRight()
{
	if (m_ipEditor)
	{
		IEditTaskPtr ipEditTask(m_ipEditor->CurrentTask);
		if (ipEditTask)
		{
			//get current task name
			string strCurrentTaskName(_bstr_t(ipEditTask->Name));
			if (strCurrentTaskName == "Create New Feature")
			{
				return true;
			}
		}
	}

	return false;
}
//--------------------------------------------------------------------------------------------------
