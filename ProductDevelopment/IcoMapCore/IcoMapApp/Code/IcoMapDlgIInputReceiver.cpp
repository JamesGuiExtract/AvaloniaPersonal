
#include "stdafx.h"
#include "IcoMapDlg.h"
#include "IcoMapApp.h"

#include "DrawingToolFSM.h"

#include <UCLIDException.h>
#include <TemporaryResourceOverride.h>

using namespace std;

extern HINSTANCE gModuleResource;

void IcoMapDlg::connectCommandInputReceiver(IInputReceiver *pInputReceiver)
{
	IInputManagerPtr ipInputManager = getInputManager();
	if (ipInputManager)
	{
		// only connect once
		static const long lIcoMapCommandIR = ipInputManager->ConnectInputReceiver(pInputReceiver);
		m_lIcoMapCommandIR = lIcoMapCommandIR;
	}
}

void IcoMapDlg::disconnectCommandInputReceiver()
{
	IInputManagerPtr ipInputManager = getInputManager();
	if (ipInputManager)
	{
		static bool bDisconnected = false;
		if (!bDisconnected)
		{
			// only disconnect once
			ipInputManager->DisconnectInputReceiver(m_lIcoMapCommandIR);
			bDisconnected = true;
		}
	}
}

void IcoMapDlg::enableCommandInputReceiver(bool bEnable)
{
	// Now, the command can only be enabled if bEnable is true,
	// and icomap is current tool and feature creation is enabled
	// and feature selection is disabled
	bool bEnableCommandLine = bEnable && m_bFeatureCreationEnabled && !m_bViewEditFeatureSelected;
	
	// before disable DIG, make sure it doesn't have the focus
	if (!bEnableCommandLine)
	{
		switchFocusFromDIGToCommandLine();
	}

	m_commandLine.EnableWindow(bEnableCommandLine ? TRUE : FALSE);
	ma_pDIGWnd->EnableWindow(bEnableCommandLine ? TRUE : FALSE);

	m_bTextInputEnabled = bEnableCommandLine;
	m_bPointInputEnabled = false;

	if (bEnableCommandLine)
	{		
		if (ma_pDrawingTool.get() != NULL)
		{		
			// only enable point input when the expected input type is
			// point and the input is enabled
			if (ma_pDrawingTool->getCurrentExpectedInputType() == kPoint)
			{
				m_bPointInputEnabled = true;
			}
		}
	}

	// NOTE: DO NOT call enableInput()/disableInput() since these calls
	// made inside this method will cause infinite loop
}

void IcoMapDlg::fireOnInputReceived(const string& strTextInput)
{
	IInputEntityPtr ipInputEntity;
	ipInputEntity.CreateInstance(__uuidof(InputEntity));
	// there's only one input entity for the command line
	ipInputEntity->InitInputEntity(m_ipInputEntityMgr, _bstr_t("").copy());

	ITextInputPtr ipTextInput;
	ipTextInput.CreateInstance(__uuidof(TextInput));
	// put meat into the textinput object
	ipTextInput->InitTextInput(/*NULL*/ipInputEntity, _bstr_t(strTextInput.c_str()));

	if (m_ipIREventHandler)
	{
		// notify input manager the input text is received 
		m_ipIREventHandler->NotifyInputReceived(ipTextInput);
	}
}

HRESULT IcoMapDlg::NotifyInputReceived(ITextInput* pTextInput) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		// Check if the IcoMap dialog is ready for input
		// If there is a child dialog for IcoMap dialog
		if (::IsWindowEnabled(m_hWnd) == FALSE)
		{
			::Beep(200, 100);
			HWND childHandle = ::GetNextWindow(m_hWnd, GW_HWNDPREV);
			if (childHandle)
			{
				// Bring the IcoMap and its child dialog to the front as a 
				// reminder to the users.
				::SetForegroundWindow(m_hWnd);
				::SetForegroundWindow(childHandle);
			}
			
			// get input entity
			IInputEntityPtr ipInputEntity(pTextInput->GetInputEntity() );
			// if there is an input entity object, set it to unused color and delete it
			if (ipInputEntity != NULL)
			{
				if (ipInputEntity->IsMarkedAsUsed() )
				{
					ipInputEntity->MarkAsUsed(VARIANT_FALSE);
				}
				if (ipInputEntity->CanBeDeleted() )
				{
					ipInputEntity->Delete();
				}
			}

			return S_OK;
		}

		if (ma_pDrawingTool.get() != NULL)
		{
			// notify drawingtoolfsm to process the text input if appropriate
			if (m_bIcoMapIsActiveInputTarget)
			{
				ma_pDrawingTool->processExpectedInput(pTextInput);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02836")
	
	return S_OK;
}

void IcoMapDlg::setInputEntityManager(IInputEntityManager *ipInputEntityMgr)
{
	m_ipInputEntityMgr = ipInputEntityMgr;
}

void IcoMapDlg::setIREventHandler(IIREventHandler *pEventHandler)
{
	m_ipIREventHandler = pEventHandler;
}

