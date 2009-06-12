#pragma once

#include "BaseUtils.h"

#include <string>

class EXPORT_BaseUtils ClipboardManager
{
public:
	// PURPOSE: Create a ClipboardManger
	// REQUIRE: pWnd is not NULL and is a vaild window
	// PROMISE:
	ClipboardManager(CWnd* pWnd);
	~ClipboardManager();

	// PURPOSE: To write text to the clipboard
	// REQUIRE: The CWnd* passed to the constructor is a valid
	//			window
	// PROMISE: If the text cannot be sucessfully written to
	//			the cliboard for any reason an exception will
	//			be thrown
	void writeText(const std::string& strText);
	// PURPOSE: To read text to the clipboard
	// REQUIRE: The CWnd* passed to the constructor is a valid
	//			window
	// PROMISE: If the text is read successfully true will be
	//			returned
	//			If the current clipboard data is not text
	//			false will be returned
	//			If the data cannot be read for any other reason
	//			an exception will be thrown
	bool readText(std::string& strText) const;

private:
	CWnd* m_pWnd;
};