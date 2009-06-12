//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	OEStatusUpdate.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan
//
//==================================================================================================

#ifndef OE_STATUS_UPDATE_H
#define OE_STATUS_UPDATE_H

#include "BaseUtils.h"
#include "ObservableEvent.h"
#include "EventID.h"

class EXPORT_BaseUtils OEStatusUpdate : public ObservableEvent
{
public:
	static EventID ID;

	OEStatusUpdate(const string& strStatusUpdateText);

	const string& getStatusUpdateText() const;

private:
	string m_strStatusUpdateText;
};

#endif // OE_STATUS_UPDATE_H