#pragma once

#include <string>
#include <vector>
class SpotRecognitionDlg;

class SpotRecArg
{
public:
	SpotRecArg(const std::string& strArg);

	const std::string getString() const;
	long getLong() const;

private:
	std::string m_strArg;
};

class SpotRecCmd
{
public:
	SpotRecCmd();
	~SpotRecCmd();

	void setCommand(const std::string& strCommand);
	const std::string getCommand() const;
	
	void setArgs(const std::vector<SpotRecArg>& vecArgs);
	long getNumArgs() const;
	const SpotRecArg* getArg(unsigned long ulIndex) const;

private:
	std::string m_strCommand;
	std::vector<SpotRecArg> m_vecArgs;
};

class SpotRecCfgFileReader
{
public:
	SpotRecCfgFileReader(SpotRecognitionDlg* pSpotRecDlg);
	~SpotRecCfgFileReader();

	void loadSettingsFromFile(std::string strFileName, SpotRecognitionDlg* pSpotRecDlg);

protected:
	virtual void executeCommands(std::vector<SpotRecCmd> vecCommands);
	void executeCommand(const SpotRecCmd& cmd);

private:

	/////////////////
	// Methods
	/////////////////
	void setWindowPos(const SpotRecCmd& cmd);
	void hideButtons(const SpotRecCmd& cmd);
	void openFile(const SpotRecCmd& cmd);
	void addTempHighlight(const SpotRecCmd& cmd);
	void clearTempHighlights(const SpotRecCmd& cmd);
	void clearImage(const SpotRecCmd& cmd);
	void setCurrentPageNumber(const SpotRecCmd& cmd);
	void zoomIn(const SpotRecCmd& cmd);
	void zoomOut(const SpotRecCmd& cmd);
	void zoomExtents(const SpotRecCmd& cmd);
	void centerOnTempHighlight(const SpotRecCmd& cmd);
	void zoomToTempHighlight(const SpotRecCmd& cmd);

	/////////////////
	// Data
	/////////////////
	SpotRecognitionDlg* m_pSpotRecDlg;
};