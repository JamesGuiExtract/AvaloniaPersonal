#pragma once

#include <EShortcutType.h>

#include <string>

class ShortcutInterpreter
{
public:
	ShortcutInterpreter() : m_zEscapeChar('/') {};
	// assign escape character to the interpreter
	void assignEscapeCharacter(const char& zEscapeChar) {m_zEscapeChar = zEscapeChar;}
	// interpret the pass-in string and return the shortcut command type
	EShortcutType interpretShortcutCommand(const std::string &strInput);

private:
	// escape character which usally is the first character of the input string
	// By default it is '/'
	char m_zEscapeChar;
};