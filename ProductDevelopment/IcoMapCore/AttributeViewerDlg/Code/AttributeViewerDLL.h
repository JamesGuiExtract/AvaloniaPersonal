#if !defined(AFX_ATTRIBUTEVIEWDLL_H__01E93C2D_3B7F_47B4_AD45_F35904445333__INCLUDED_)
#define AFX_ATTRIBUTEVIEWDLL_H__01E93C2D_3B7F_47B4_AD45_F35904445333__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	AttributeViewerDLL.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//==================================================================================================
#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifdef AttributeDLL_IMPL
#define CLASS_DECL_AttributeViewerDLL _declspec(dllexport)
#else
#define CLASS_DECL_AttributeViewerDLL _declspec(dllimport)
#endif

#endif
