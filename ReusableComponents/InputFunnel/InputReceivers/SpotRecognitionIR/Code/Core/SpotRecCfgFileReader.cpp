#include "stdafx.h"
#include "SpotRecCfgFileReader.h"
#include "SpotRecognitionDlg.h"
#include "StringTokenizer.h"
#include <cpputil.h>
#include <UCLIDException.h>
#include <CommentedTextFileReader.h>

using std::vector;
using std::string;

//--------------------------------------------------------------------------------------------------
// SpotRecCfgFileReader::SpotRecArg
//--------------------------------------------------------------------------------------------------
SpotRecArg::SpotRecArg(const std::string& strArg)
{
	m_strArg = strArg;
}
//--------------------------------------------------------------------------------------------------
const std::string SpotRecArg::getString() const
{
	return m_strArg;
}
//--------------------------------------------------------------------------------------------------
long SpotRecArg::getLong() const
{
	return asLong(m_strArg);
}
//--------------------------------------------------------------------------------------------------
// SpotRecCmd
//--------------------------------------------------------------------------------------------------
SpotRecCmd::SpotRecCmd()
{
}
//--------------------------------------------------------------------------------------------------
SpotRecCmd::~SpotRecCmd()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16498");
}
//--------------------------------------------------------------------------------------------------
void SpotRecCmd::setCommand(const std::string& strCommand)
{
	m_strCommand = strCommand;
}
//--------------------------------------------------------------------------------------------------
const std::string SpotRecCmd::getCommand() const
{
	return m_strCommand;
}
//--------------------------------------------------------------------------------------------------
void SpotRecCmd::setArgs(const std::vector<SpotRecArg>& vecArgs)
{
	m_vecArgs = vecArgs;
}
//--------------------------------------------------------------------------------------------------
long SpotRecCmd::getNumArgs() const
{
	return m_vecArgs.size();
}
//--------------------------------------------------------------------------------------------------
const SpotRecArg* SpotRecCmd::getArg(unsigned long ulIndex) const
{
	if (ulIndex < 0 || ulIndex >= m_vecArgs.size())
	{
		UCLIDException ue("ELI12065", "Invalid argument index.");
		ue.addDebugInfo("Index", ulIndex);
		throw ue;
	}
	return &(m_vecArgs[ulIndex]);
}

//--------------------------------------------------------------------------------------------------
// SpotRecCfgFileReader
//--------------------------------------------------------------------------------------------------
SpotRecCfgFileReader::SpotRecCfgFileReader(SpotRecognitionDlg* pSpotRecDlg)
{
	m_pSpotRecDlg = pSpotRecDlg;
}
//--------------------------------------------------------------------------------------------------
SpotRecCfgFileReader::~SpotRecCfgFileReader()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16499");
}
//--------------------------------------------------------------------------------------------------
void SpotRecCfgFileReader::loadSettingsFromFile(std::string strFileName, SpotRecognitionDlg* pSpotRecDlg)
{
//	validateFileOrFolderName(strFileName);

	m_pSpotRecDlg = pSpotRecDlg;

	ifstream ifs;
	ifs.open(strFileName.c_str());

	CommentedTextFileReader ctf(ifs);

	vector<SpotRecCmd> vecCommands;

	while (!ctf.reachedEndOfStream())
	{
		string strLine = ctf.getLineText();

		if (strLine == "")
		{
			continue;
		}

		long nFirstWhiteSpace = strLine.find_first_of(" \t");

		SpotRecCmd cmd;
		cmd.setCommand(strLine.substr(0, nFirstWhiteSpace));

		if (nFirstWhiteSpace != string::npos)
		{
			long nEndWhiteSpace = strLine.find_first_not_of(" \t", nFirstWhiteSpace);
			strLine = strLine.substr(nEndWhiteSpace, string::npos);	
			
			vector<string> vecArgTokens;
			StringTokenizer::sGetTokens(strLine, ",", "\"", "\"",  vecArgTokens);

			// create the arguments
			vector<SpotRecArg> vecArgs;
			unsigned int ui;
			for (ui = 0; ui < vecArgTokens.size(); ui++)
			{
				SpotRecArg arg( vecArgTokens[ui] );
				vecArgs.push_back(arg);
			}
		
			// erase the first element
			cmd.setArgs(vecArgs);
		}

		vecCommands.push_back(cmd);
	}

	ifs.close();

	executeCommands(vecCommands);
	
}
//--------------------------------------------------------------------------------------------------
void SpotRecCfgFileReader::executeCommands(std::vector<SpotRecCmd> vecCommands)
{
	unsigned int ui;
	for (ui = 0; ui < vecCommands.size(); ui++)
	{
		// if a command fails (throws an exception) we will display it and move to the
		// next command
		try
		{
			executeCommand( vecCommands[ui] );	
		}
		CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12245");
	}
}
//--------------------------------------------------------------------------------------------------
void SpotRecCfgFileReader::executeCommand(const SpotRecCmd& cmd)
{
	string strCommand = cmd.getCommand();

	// case insensitive compare
	makeLowerCase(strCommand);

	if (strCommand == "setwindowpos")
	{
		setWindowPos(cmd);
	}
	else if(strCommand == "hidebuttons")
	{
		hideButtons(cmd);
	}
	else if(strCommand == "openfile")
	{
		openFile(cmd);
	}
	else if(strCommand == "addtemphighlight")
	{
		addTempHighlight(cmd);
	}
	else if(strCommand == "cleartemphighlights")
	{
		clearTempHighlights(cmd);
	}
	else if(strCommand == "clearimage")
	{
		clearImage(cmd);
	}
	else if(strCommand == "setcurrentpagenumber")
	{
		setCurrentPageNumber(cmd);
	}
	else if(strCommand == "zoomin")
	{
		zoomIn(cmd);
	}
	else if(strCommand == "zoomout")
	{
		zoomOut(cmd);
	}
	else if(strCommand == "zoomextents")
	{
		zoomExtents(cmd);
	}
	else if(strCommand == "centerontemphighlight")
	{
		centerOnTempHighlight(cmd);
	}
	else if(strCommand == "zoomtotemphighlight")
	{
		zoomToTempHighlight(cmd);
	}
	else
	{
		UCLIDException ue("ELI12072", "Invalid SpotRecognitionIR command.");
		ue.addDebugInfo("Command", strCommand);
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
void SpotRecCfgFileReader::setWindowPos(const SpotRecCmd& cmd)
{
	if (cmd.getNumArgs() != 1 && cmd.getNumArgs() != 4)
	{
		UCLIDException ue("ELI12076", "Invalid number of command arguments.");
		ue.addDebugInfo("NumArgs", cmd.getNumArgs());
		ue.addDebugInfo("Command", cmd.getCommand());
		throw ue;
	}

	if (cmd.getNumArgs() == 1)
	{
		RECT displayRect;
		SystemParametersInfo(SPI_GETWORKAREA, 0, &displayRect, 0);
		int nScreenWidth = displayRect.right - displayRect.left;
		int nScreenHeight = displayRect.bottom - displayRect.top;

		RECT rect;
		memset(&rect, 0, sizeof(RECT));
		rect = displayRect;

		string strPos = cmd.getArg(0)->getString();
		if (strPos == "Full")
		{
		}
		else if(strPos == "Left")
		{
			rect.right = rect.left + nScreenWidth / 2;
		}
		else if(strPos == "Right")
		{
			rect.left = rect.left + nScreenWidth / 2;
		}
		else if(strPos == "Top")
		{
			rect.bottom = rect.top + nScreenHeight / 2;
		}
		else if(strPos == "Bottom")
		{
			rect.top = rect.top + nScreenHeight / 2;
		}
		else
		{
			UCLIDException ue("ELI19457", "Invalid window position specification.");
			ue.addDebugInfo("Position", strPos);
			throw ue;
		}
		m_pSpotRecDlg->MoveWindow(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
	}

	if (cmd.getNumArgs() == 4)
	{
		RECT rect;
		rect.left = cmd.getArg(0)->getLong();
		rect.right = cmd.getArg(1)->getLong();
		rect.top = cmd.getArg(2)->getLong();
		rect.bottom = cmd.getArg(3)->getLong();

		RECT oldRect;
		m_pSpotRecDlg->GetWindowRect(&oldRect);

		if (oldRect.left != rect.left ||
			oldRect.top != rect.top ||
			oldRect.right != rect.right ||
			oldRect.bottom != rect.bottom)
		{
			m_pSpotRecDlg->MoveWindow(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
		}
	}

}
//--------------------------------------------------------------------------------------------------
void SpotRecCfgFileReader::hideButtons(const SpotRecCmd& cmd)
{
	if (cmd.getNumArgs() < 1)
	{
		UCLIDException ue("ELI19403", "Invalid number of command arguments.");
		ue.addDebugInfo("NumArgs", cmd.getNumArgs());
		ue.addDebugInfo("Command", cmd.getCommand());
		throw ue;
	}

	int i;
	for (i = 0; i < cmd.getNumArgs(); i++)
	{
		ESRIRToolbarCtrl eCtrl = (ESRIRToolbarCtrl)cmd.getArg(i)->getLong();
		m_pSpotRecDlg->showToolbarCtrl(eCtrl, false);
	}
}
//--------------------------------------------------------------------------------------------------
void SpotRecCfgFileReader::openFile(const SpotRecCmd& cmd)
{
	if (cmd.getNumArgs() != 1)
	{
		UCLIDException ue("ELI19404", "Invalid number of command arguments.");
		ue.addDebugInfo("NumArgs", cmd.getNumArgs());
		ue.addDebugInfo("Command", cmd.getCommand());
		throw ue;
	}

	// only open the image if it is not already open
	if (m_pSpotRecDlg->getImageFileName() != cmd.getArg(0)->getString())
	{
		m_pSpotRecDlg->openFile2(cmd.getArg(0)->getString());
	}
}
//--------------------------------------------------------------------------------------------------
void SpotRecCfgFileReader::addTempHighlight(const SpotRecCmd& cmd)
{
	if (cmd.getNumArgs() != 6)
	{
		UCLIDException ue("ELI19405", "Invalid number of command arguments.");
		ue.addDebugInfo("NumArgs", cmd.getNumArgs());
		ue.addDebugInfo("Command", cmd.getCommand());
		throw ue;
	}

	long nStartX = cmd.getArg(0)->getLong();
	long nStartY = cmd.getArg(1)->getLong();
	long nEndX = cmd.getArg(2)->getLong();
	long nEndY = cmd.getArg(3)->getLong();
	long nHeight = cmd.getArg(4)->getLong();
	long nPageNum = cmd.getArg(5)->getLong();

	m_pSpotRecDlg->addTemporaryHighlight(nStartX, nStartY, nEndX, nEndY, nHeight, nPageNum); 
}
//--------------------------------------------------------------------------------------------------
void SpotRecCfgFileReader::clearTempHighlights(const SpotRecCmd& cmd)
{
	if (cmd.getNumArgs() > 0)
	{
		UCLIDException ue("ELI12106", "Invalid number of command arguments.");
		ue.addDebugInfo("NumArgs", cmd.getNumArgs());
		ue.addDebugInfo("Command", cmd.getCommand());
		throw ue;
	}
	m_pSpotRecDlg->deleteTemporaryHighlight();
}
//--------------------------------------------------------------------------------------------------
void SpotRecCfgFileReader::clearImage(const SpotRecCmd& cmd)
{
	if (cmd.getNumArgs() > 0)
	{
		UCLIDException ue("ELI12107", "Invalid number of command arguments.");
		ue.addDebugInfo("NumArgs", cmd.getNumArgs());
		ue.addDebugInfo("Command", cmd.getCommand());
		throw ue;
	}
	m_pSpotRecDlg->openFile2("");
}
//--------------------------------------------------------------------------------------------------
void SpotRecCfgFileReader::setCurrentPageNumber(const SpotRecCmd& cmd)
{
	if (cmd.getNumArgs() != 1)
	{
		UCLIDException ue("ELI12108", "Invalid number of command arguments.");
		ue.addDebugInfo("NumArgs", cmd.getNumArgs());
		ue.addDebugInfo("Command", cmd.getCommand());
		throw ue;
	}

	long nPageNumber = cmd.getArg(0)->getLong();

	// only change the page if it is not already the current page
	if (nPageNumber != m_pSpotRecDlg->getCurrentPageNumber())
	{
		m_pSpotRecDlg->setCurrentPageNumber(nPageNumber);
	}
}
//--------------------------------------------------------------------------------------------------
void SpotRecCfgFileReader::zoomIn(const SpotRecCmd& cmd)
{
	if (cmd.getNumArgs() > 0)
	{
		UCLIDException ue("ELI12109", "Invalid number of command arguments.");
		ue.addDebugInfo("NumArgs", cmd.getNumArgs());
		ue.addDebugInfo("Command", cmd.getCommand());
		throw ue;
	}
	m_pSpotRecDlg->zoomIn();
}
//--------------------------------------------------------------------------------------------------
void SpotRecCfgFileReader::zoomOut(const SpotRecCmd& cmd)
{
	if (cmd.getNumArgs() > 0)
	{
		UCLIDException ue("ELI12110", "Invalid number of command arguments.");
		ue.addDebugInfo("NumArgs", cmd.getNumArgs());
		ue.addDebugInfo("Command", cmd.getCommand());
		throw ue;
	}
	m_pSpotRecDlg->zoomOut();
}
//--------------------------------------------------------------------------------------------------
void SpotRecCfgFileReader::zoomExtents(const SpotRecCmd& cmd)
{
	if (cmd.getNumArgs() > 0)
	{
		UCLIDException ue("ELI12111", "Invalid number of command arguments.");
		ue.addDebugInfo("NumArgs", cmd.getNumArgs());
		ue.addDebugInfo("Command", cmd.getCommand());
		throw ue;
	}
	m_pSpotRecDlg->zoomExtents();
}
//--------------------------------------------------------------------------------------------------
void SpotRecCfgFileReader::centerOnTempHighlight(const SpotRecCmd& cmd)
{
	if (cmd.getNumArgs() > 0)
	{
		UCLIDException ue("ELI12243", "Invalid number of command arguments.");
		ue.addDebugInfo("NumArgs", cmd.getNumArgs());
		ue.addDebugInfo("Command", cmd.getCommand());
		throw ue;
	}
	m_pSpotRecDlg->centerOnTemporaryHighlight();
}
//--------------------------------------------------------------------------------------------------
void SpotRecCfgFileReader::zoomToTempHighlight(const SpotRecCmd& cmd)
{
	if (cmd.getNumArgs() > 0)
	{
		UCLIDException ue("ELI19407", "Invalid number of command arguments.");
		ue.addDebugInfo("NumArgs", cmd.getNumArgs());
		ue.addDebugInfo("Command", cmd.getCommand());
		throw ue;
	}
	m_pSpotRecDlg->zoomToTemporaryHighlight();
}
//--------------------------------------------------------------------------------------------------