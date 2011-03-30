#include "stdafx.h"
#include "Win32GlobalAtom.h"
#include "UCLIDException.h"

using std::string;

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
const long gnMaxAtomNameLength = 255;

//--------------------------------------------------------------------------------------------------
// Win32GlobalAtom
//--------------------------------------------------------------------------------------------------
Win32GlobalAtom::Win32GlobalAtom()
: m_atom(0)
{
}
//--------------------------------------------------------------------------------------------------
Win32GlobalAtom::Win32GlobalAtom(ATOM atom)
: m_atom(atom)
{
	attach(m_atom);
}
//--------------------------------------------------------------------------------------------------
Win32GlobalAtom::Win32GlobalAtom(const std::string& strAtomName)
: m_atom(0)
{
	setName(strAtomName);
}
//--------------------------------------------------------------------------------------------------
Win32GlobalAtom::~Win32GlobalAtom()
{
	try
	{
		// If the atom is 0 either it was never attached 
		// or it was detached
		if (m_atom != 0)
		{
			release();
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16417");
}
//--------------------------------------------------------------------------------------------------
void Win32GlobalAtom::attach(ATOM atom)
{
	if (m_atom != 0)
	{
		release();
	}
	m_atom = atom;
}
//--------------------------------------------------------------------------------------------------
ATOM Win32GlobalAtom::detach()
{
	if (m_atom == 0)
	{
		UCLIDException ue("ELI11857", "No currently attached ATOM.");
		throw ue;
	}
	ATOM returnAtom = m_atom;
	m_atom = 0;
	return returnAtom;
}
//--------------------------------------------------------------------------------------------------
ATOM Win32GlobalAtom::getWin32Atom() const
{
	return m_atom;
}
//--------------------------------------------------------------------------------------------------
void Win32GlobalAtom::release()
{
	if (m_atom == 0)
	{
		UCLIDException ue("ELI11855", "No currently attached ATOM.");
		throw ue;
	}
	GlobalDeleteAtom(detach());
}
//--------------------------------------------------------------------------------------------------
const std::string Win32GlobalAtom::getName() const
{
	if (m_atom == 0)
	{
		UCLIDException ue("ELI11856", "No currently attached ATOM.");
		throw ue;
	}
	
	char* buf = new char[gnMaxAtomNameLength];
	UINT nSize = 0;

	try
	{
		nSize = GlobalGetAtomName(m_atom, buf, gnMaxAtomNameLength);
	}
	catch(...)
	{
		delete [] buf;
		buf = NULL;
		throw;
	}

	if (nSize == 0)
	{
		DWORD dwErr = GetLastError();
		UCLIDException ue("ELI11841", "Unable to get Global ATOM Name.");
		ue.addDebugInfo("ATOM", m_atom);
		ue.addDebugInfo("Error", dwErr);
		throw ue;
	}

	string strAtomName = buf;

	if(buf != __nullptr)
	{
		delete [] buf;
	}

	return strAtomName;
}
//--------------------------------------------------------------------------------------------------
const void Win32GlobalAtom::setName(const string& strAtomName)
{
	// detach the currently attached atom, if any
	if (m_atom != NULL)
	{
		detach();
	}

	// create a new atom with the specified name
	ATOM atom = GlobalAddAtom(strAtomName.c_str());
	if (atom == 0)
	{
		DWORD dwErr = GetLastError();
		UCLIDException ue("ELI19398", "Unable to create Global ATOM.");
		ue.addDebugInfo("ATOM Name", strAtomName);
		ue.addDebugInfo("Error", dwErr);
		throw ue;
	}

	// attach the newly created atom
	attach(atom);
}
//--------------------------------------------------------------------------------------------------
