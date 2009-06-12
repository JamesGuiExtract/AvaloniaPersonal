
#pragma once

extern _ATL_FUNC_INFO NotifyInputReceivedInfo;
const int INPUTMANAGER_OBJECT_ID = 1752;

class InputManagerEventHandler : public IDispEventImpl<INPUTMANAGER_OBJECT_ID,
													   InputManagerEventHandler,
													   &DIID__IInputManagerEvents,
													   &LIBID_UCLID_INPUTFUNNELLib>
{
public:
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Standard constructors and destructors
	InputManagerEventHandler();
	virtual ~InputManagerEventHandler();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To set the InputManager object from which events are to be received.
	// REQUIRE: pInputManager points to a valid implementation of IInputManager.
	// PROMISE: All subsequent events arising from pInputManager will be sent to the overridden
	//			NotifyInputReceived() method in the derived class.  Calling this method
	//			automatically disassociates this object from all previously-connected 
	//			InputManager objects.
	void SetInputManager(IInputManager *pInputManager);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To set the singleton InputManager object as the object from whichevents are to 
	//			be received.
	// REQUIRE: Nothing.
	// PROMISE: All subsequent events arising from the singleton instance of the InputManager object
	//			will be sent to the overridden NotifyInputReceived() method in the derived class.  
	//			Calling this method automatically disassociates this object from all 
	//			previously-connected InputManager objects.
	void UseSingletonInputManager();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To handle input-received event notifications from the currently connected
	//			InputManager object.
	// REQUIRE: The implementation of this virtual method must be provided in the derived class.
	// PROMISE: All input-received notifications from the currently connected InputManager
	//			object will be sent to this method in the derived class.
	virtual HRESULT __stdcall NotifyInputReceived(ITextInput* pTextInput)  = 0;
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To get an IInputManagerPtr
	// REQUIRE: The pointer returned from this method cannot be stored
	// PROMISE: Will return either the input manager set by SetInputManager() or the 
	//			Singleton Input manager.  depending on which of SetInputManager() or 
	//			UseSingletonInputManager() was more recently called
	IInputManagerPtr getInputManager();
	//---------------------------------------------------------------------------------------------

	BEGIN_SINK_MAP(InputManagerEventHandler)
		SINK_ENTRY_INFO(INPUTMANAGER_OBJECT_ID, DIID__IInputManagerEvents, 
		                1, NotifyInputReceived, &NotifyInputReceivedInfo)
	END_SINK_MAP()

private:
	// Private member variables
	IInputManagerPtr m_ipInputManager; // the currently connected InputManager object.
	bool m_bUseSingleton;
};
