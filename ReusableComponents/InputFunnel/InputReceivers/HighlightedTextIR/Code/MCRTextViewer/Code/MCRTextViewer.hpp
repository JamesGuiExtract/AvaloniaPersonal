
//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	MCRTextViewer.hpp
//
// PURPOSE:	The purpose of this file is to specify the interface that needs to be supported by
//			the MCRTextViewer ActiveX control.  Because UCLID does not have developers with
//			expertise in ActiveX/COM technologies, the interface is specified here purely from a C++
//			perspective.  InfoTech is requested to determine the equivalent interface that is
//			applicable with the ActiveX/COM technology and propose the interface to UCLID for 
//			review.
//
// NOTES:	Most of the basic classes here are purposely represented as structures, as it is not
//			clear at this time as to how these classes will be implemented with the ActiveX/COM
//			technology.  The authors of this document realize that many of these clases can be
//			made more useful with member methods to perform various activities.  InfoTech is 
//			requested to convert these C++ "structural" objects into COM objects, object orientize 
//			as applicable, and propose the COM based objects to UCLID for review.
//
//			User Interface Requirements for the ActiveX Control
//			---------------------------------------------------
//			The ActiveX control must feature a toolbar with the following features:
//			  * "new" - will call MCRTextViewer::clear(), and automatically put the control
//				in "edit-text" mode.  Prior to calling the clear() method, if the contents
//				of the MCRTextViewer are associated with a file, and the contents have been
//				modified since the last open/save operation, the control shall prompt the user
//				if they would like to save the changes with an Yes/No/Cancel prompt, and respond
//				accordingly.
//			  * "open" - clicking this button should bring up the standard windows-open dialog
//				box, allow the user to select an ASCII file, call MCRTextViewer::open() with
//				the selected file name as the argument, and automatically put the control in 
//				the "view-text" mode (as opposed to the "edit-text" mode).
//			  * "save" - clicking this button should behave like clicking the save button in
//				toolbars of Microsoft Word - in other words, if the contents of the MCRTextViewer
//				are already associated with a file, the contents are re-saved to that file.  If
//				the contents are not associated with a file, pressing the "save" button should
//				behave like "save as" functionality.  Depending upon whether a file is already
//				associated with the contents of the MCRTextViewer window, either 
//				MCRTextViewer::save() or MCRTextViewer::saveAs() should be called.
//			  * "print" - clicking on this button should behave similar to the "print" command in
//				notepad, except that any text highlighted in the MCRTextViewer should also be
//				highlighted in the printed paper document.
//			  * "cut" - should behave similar to the cut functinality in Notepad.  This button
//				is enabled only when the MCRTextViewer is in "edit-text" mode.
//			  * "copy" - should behave similar to the copy functionality in Notepad.  This button
//				is always enabled.
//			  * "paste"  - should behave similar to the paste functionality in Notepad when the
//				control is in "edit-text" mode.  When the control is in "view-text" mode, if the
//				paste command is invoked, the clear() method should be called, after which the text
//				in the clipboard should be pasted into the MCRTextViewer window, and the 
//				parseMCRText() method should be called.  As with with new command, prior to calling
//				the clear() method, if the contents of the MCRTextViewer are associated with a
//				file, and the contents have been modified since the last open/save operation, the
//				control shall prompt the user if they would like to save the changes with an
//				Yes/No/Cancel prompt, and respond accordingly.
//			  * "font selector" - will allow the user to change the font used to draw the text
//				being displayed in the ActiveX control.  When the user changes the font, the
//				ConfigurationChanged event will be fired.
//			  * "increase font size" - will increase the size of the font used to display the
//				text in the ActiveX control by a programmable constant.  When the user increases
//				the font size, the ConfigurationChanged event will be fired.
//			  * "decrease font size" - will decrease the size of the font used to display the
//				text in the ActiveX control by a programmable constant, ensuring that the text
//				is never less than 6pt.  When the user decreases the font size, the 
//				ConfigurationChanged event will be fired.
//			  * "toggle edit/view mode" - will toggle the control between the "edit-text" and
//				"view-text" mode.  See the invariants & notes sections of the MCRTextViewer
//				class for more information regarding these two modes.
//			The ActiveX control must feature a status bar with the following status indicators:
//			  * "status text" - will be used to provide arbitrary information to the user.
//			The ActiveX control must support resizing gracefully.  The ActiveX control must 
//			automatically perform word-wrapping depending upon the size of the control.
//
//			Events fired by the ActiveX control
//			-----------------------------------
//			Event Name: TextSelected
//				Meaning:				The user has selected MCR'able text in the MCRTextViewer
//										window and wants to use	the selected MCR'able text in a
//										command requiring such input.
//				Number of Parameters:	1
//				Parameter 1:			ulTextEntityID (unsigned long); this is the ID of the text
//										entity that was selected.
//			Event Name: ConfigurationChanged
//				Meaning:				The user has changed one of the persistent settings of the
//										ActiveX control (the font size, or font the font name).
//				Number of Parameters:	0
//			Event Name: FileOpened
//				Meaning:				The user has opened a file in the MCRTextViewer window.
//				Number of Parameters:	1
//				Parameter 1:			strFileName (string); this is the name of the file that
//										was opened in the MCRTextViewer window.
//
// AUTHORS:	Arvind Ganesan.
//
//==================================================================================================

// include necessary header files
#include <afxwin.h>
#include <string>
using namespace std;

//==================================================================================================
//
// CLASS:	MCRTextViewer
//
// PURPOSE:	To provide the functionality that allows the user to select MCR text from a notepad like
//			window that automatically highlights MCR'able text making it easier for the user to 
//			spot MCR'able text.
//
// REQUIRE:	Nothing.
// 
// INVARIANTS:
//			The ActiveX control is always in one of two modes - "edit-text" mode, or "view-text" 
//			mode.  Whenever the ActiveX control is in the "edit-text" mode, the text cursor is
//			visible and is availble for user manipulation.  Whenever the ActiveX control is in 
//			the "edit-text" mode, the MCRTextViewer displays the text just like Notepad would - 
//			the MCR'able text is not highlighted (and neither does the ActiveX control spend time
//			identifying MCR'able text).  Whenever the ActiveX control is in "view-text" mode, all
//			MCR'able text is automatically highlighted in yellow color (which is an internal
//			programmable constant).
//
// EXTENSIONS:
//			None.
//
// NOTES:	When in "edit-text" mode the user shall be able to edit the text in the MCRTextViewer
//			window just like the user will be able to edit text in Notepad.  When in "view-text"
//			mode, the ActiveX control shall operate in "read-only" mode, and shall not allow the
//			user to modify the text in the MCRTextViewer window.  Further, when the user switches
//			the control from the "edit-text" mode to the "view-text" mode, the ActiveX control
//			will use the UCLIDMCRTextFinder ActiveX control to automatically identify MCR'able text
//			in the MCRTextViewer window, and shall automatically highlight the identified MCR'able
//			text.
//
class MCRTextViewer
{
public:
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To initialize the ActiveX control.
	// REQUIRE: Nothing.
	// PROMISE: The ActiveX control will be initialized to be in "edit-text" mode.
	MCRTextViewer();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To open an ASCII file
	// REQUIRE: The specified file is readable and contains ASCII data.
	// PROMISE: To read the ASCII file, replace the contents of the MCRTextViewer window with
	//			the contents of the file, and then call parseMCRText().  
	void open(const string& strFileName);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To save the contents of the MCRTextViewer window.
	// REQUIRE: open() must have been successfully called after the last call to clear().
	// PROMISE: To call saveAs() with the same parameter that was passed to the last open() call.
	void save();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To save the contents of the MCRTextViewer window.
	// REQUIRE: The specified file name is writable.
	// PROMISE: To save the contents of teh MCRTextViewer window to the specified file.  Note that
	//			only the textual contents of the MCRTextViewer window is written to the target file 
	//			- i.e. no special tags regarding the starting ending positions of MCR'able text, 
	//			etc. will be written to the file.  The file is written similar to the way Notepad
	//			will save a file.
	void saveAs(const string& strFileName);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To clear the the contents of the MCRTextViewer window.
	// REQUIRE: Nothing.
	// PROMISE: To clear the contents of the MCRTextViewer window, and disassociate the contents
	//			of the MCRTextViewer window with any file.
	void clear();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To parse the the contents of the MCRTextViewer window and highlight MCR'able text.
	// REQUIRE: Nothing.
	// PROMISE: The ActiveX control will be set to operate in the "view-text" mode.
	//			Any currently highlighted MCR'able text in the MCRTextViewer window will be
	//			unhighlighted and the UCLIDMCRTextFinder ActiveX control will be used to re-identify
	//			MCR'able text in the contents of the MCRTextViewer window.  Each newly identified
	//			MCR'able text will be highlighted, and will be assigned a unique ID internally,
	//			which will be sent as the argument to the TextSelected event when the highlighted
	//			text is selected.  Any unique text entity ID's sent as arguments to previous
	//			TextSelected events will now be invalid.  There is no guarantee that calling this
	//			method multiple times on the same textual content will result in the various
	//			MCR'able texts getting assigned the same unique ID's - in other words, the same
	//			MCR'able text entity may be assigned a different ID each time this method is
	//			called.
	void parseMCRText();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To enable the user to select on text entities.
	// REQUIRE: Nothing.
	// PROMISE: To turn on or off the text selection mode depending upon bValue.  If text
	//			selection is enabled, and the ActiveX control is in "view-text" mode, then the 
	//			ActiveX control's mouse cursor will change to the "SelectTextCursor" whenever the 
	//			mouse is on top of a highlighted text (i.e. any MCR'able text).  Further, when 
	//			text selection is enabled, if the user clicks on highlighted text, the ActiveX
	//			control will send out a TextSelected event with the ID of the selected text as the
	//			event parameter.
	void enableTextSelection(bool bValue);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To retrieve the text of a highlighted text entity.
	// REQUIRE: ulTextEntityID must have been obtained as the paramter to a fired "TextSelected"
	//			event.
	// PROMISE:	To return the text of the highlighted text entity associated with the unique ID
	//			ulTextEntityID
	string getText(unsigned long ulTextEntityID);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To modify the text of a highlighted text entity.
	// REQUIRE: Same requirements as the getText() method.  In addition, strNewText != "".
	// PROMISE: To modify the text associated with the highlighted entity associated with the unique
	//			ID ulTextEntityID.  The new text associated with the specified highlighted text
	//			entity will be strNewText.  No checking will be done to ensure that the new text
	//			strNewText is infact MCR'able text - the old MCR'able text will merely be 
	//			replaced with the specified new text.
	string setText(unsigned long ulTextEntityID, const string& strNewText);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To modify the highlight color of highlighted text in the MCRTextViewer.
	// REQUIRE: Same requirements as the getText() method.
	// PROMISE: To highlighted text entity associated with the unique ID ulTextEntityID will be
	//			highlighted in the color represented by newTextHighlightColor.
	string setTextHighlightColor(unsigned long ulTextEntityID, COLORREF newTextHighlightColor);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To store configuration information to an INI file.
	// REQUIRE: The specified file is an INI file, and is writable.
	// PROMISE: To store the name of the font used to display the text under the INI file section 
	//			"MCRTextViewer", and key "Font".  To store the font size used to display the text
	//			under the INI file section "MCRTextViewer" and key "FontSize".
	void saveConfiguration(const string& strINIFileName);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To load configuration information from an INI file.
	// REQUIRE: The specified file is an INI file to which configuration information has been
	//			written by a call to saveConfiguration().
	// PROMISE: To read the configuration information stored in the INI file as per the format
	//			described in the PROMISE section of saveConfiguration(), and to apply those settings
	//			to the ActiveX control.
	void loadConfiguration(const string& strINIFileName);
	//----------------------------------------------------------------------------------------------
};
//==================================================================================================
