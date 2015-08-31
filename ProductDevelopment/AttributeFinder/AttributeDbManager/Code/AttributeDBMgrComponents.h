

/* this ALWAYS GENERATED file contains the definitions for the interfaces */


 /* File created by MIDL compiler version 7.00.0555 */
/* at Mon Aug 31 14:00:50 2015
 */
/* Compiler settings for AttributeDBMgrComponents.idl:
    Oicf, W1, Zp8, env=Win32 (32b run), target_arch=X86 7.00.0555 
    protocol : dce , ms_ext, c_ext, robust
    error checks: allocation ref bounds_check enum stub_data 
    VC __declspec() decoration level: 
         __declspec(uuid()), __declspec(selectany), __declspec(novtable)
         DECLSPEC_UUID(), MIDL_INTERFACE()
*/
/* @@MIDL_FILE_HEADING(  ) */

#pragma warning( disable: 4049 )  /* more than 64k source lines */


/* verify that the <rpcndr.h> version is high enough to compile this file*/
#ifndef __REQUIRED_RPCNDR_H_VERSION__
#define __REQUIRED_RPCNDR_H_VERSION__ 475
#endif

#include "rpc.h"
#include "rpcndr.h"

#ifndef __RPCNDR_H_VERSION__
#error this stub requires an updated version of <rpcndr.h>
#endif // __RPCNDR_H_VERSION__


#ifndef __AttributeDBMgrComponents_h__
#define __AttributeDBMgrComponents_h__

#if defined(_MSC_VER) && (_MSC_VER >= 1020)
#pragma once
#endif

/* Forward Declarations */ 

#ifndef __IAttributeDBMgr_FWD_DEFINED__
#define __IAttributeDBMgr_FWD_DEFINED__
typedef interface IAttributeDBMgr IAttributeDBMgr;
#endif 	/* __IAttributeDBMgr_FWD_DEFINED__ */


#ifndef __AttributeDBMgr_FWD_DEFINED__
#define __AttributeDBMgr_FWD_DEFINED__

#ifdef __cplusplus
typedef class AttributeDBMgr AttributeDBMgr;
#else
typedef struct AttributeDBMgr AttributeDBMgr;
#endif /* __cplusplus */

#endif 	/* __AttributeDBMgr_FWD_DEFINED__ */


/* header files for imported files */
#include "oaidl.h"
#include "ocidl.h"

#ifdef __cplusplus
extern "C"{
#endif 



#ifndef __UCLID_AttributeDbMgrComponentsLib_LIBRARY_DEFINED__
#define __UCLID_AttributeDbMgrComponentsLib_LIBRARY_DEFINED__

/* library UCLID_AttributeDbMgrComponentsLib */
/* [helpstring][version][uuid] */ 


EXTERN_C const IID LIBID_UCLID_AttributeDbMgrComponentsLib;

#ifndef __IAttributeDBMgr_INTERFACE_DEFINED__
#define __IAttributeDBMgr_INTERFACE_DEFINED__

/* interface IAttributeDBMgr */
/* [unique][helpstring][nonextensible][dual][uuid][object] */ 


EXTERN_C const IID IID_IAttributeDBMgr;

#if defined(__cplusplus) && !defined(CINTERFACE)
    
    MIDL_INTERFACE("A92F35D7-1784-4D3B-9980-4D9AE244DA1A")
    IAttributeDBMgr : public IDispatch
    {
    public:
        virtual /* [helpstring][id][propput] */ HRESULT STDMETHODCALLTYPE put_FAMDB( 
            /* [in] */ /* external definition not present */ IFileProcessingDB *newVal) = 0;
        
    };
    
#else 	/* C style interface */

    typedef struct IAttributeDBMgrVtbl
    {
        BEGIN_INTERFACE
        
        HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
            IAttributeDBMgr * This,
            /* [in] */ REFIID riid,
            /* [annotation][iid_is][out] */ 
            __RPC__deref_out  void **ppvObject);
        
        ULONG ( STDMETHODCALLTYPE *AddRef )( 
            IAttributeDBMgr * This);
        
        ULONG ( STDMETHODCALLTYPE *Release )( 
            IAttributeDBMgr * This);
        
        HRESULT ( STDMETHODCALLTYPE *GetTypeInfoCount )( 
            IAttributeDBMgr * This,
            /* [out] */ UINT *pctinfo);
        
        HRESULT ( STDMETHODCALLTYPE *GetTypeInfo )( 
            IAttributeDBMgr * This,
            /* [in] */ UINT iTInfo,
            /* [in] */ LCID lcid,
            /* [out] */ ITypeInfo **ppTInfo);
        
        HRESULT ( STDMETHODCALLTYPE *GetIDsOfNames )( 
            IAttributeDBMgr * This,
            /* [in] */ REFIID riid,
            /* [size_is][in] */ LPOLESTR *rgszNames,
            /* [range][in] */ UINT cNames,
            /* [in] */ LCID lcid,
            /* [size_is][out] */ DISPID *rgDispId);
        
        /* [local] */ HRESULT ( STDMETHODCALLTYPE *Invoke )( 
            IAttributeDBMgr * This,
            /* [in] */ DISPID dispIdMember,
            /* [in] */ REFIID riid,
            /* [in] */ LCID lcid,
            /* [in] */ WORD wFlags,
            /* [out][in] */ DISPPARAMS *pDispParams,
            /* [out] */ VARIANT *pVarResult,
            /* [out] */ EXCEPINFO *pExcepInfo,
            /* [out] */ UINT *puArgErr);
        
        /* [helpstring][id][propput] */ HRESULT ( STDMETHODCALLTYPE *put_FAMDB )( 
            IAttributeDBMgr * This,
            /* [in] */ /* external definition not present */ IFileProcessingDB *newVal);
        
        END_INTERFACE
    } IAttributeDBMgrVtbl;

    interface IAttributeDBMgr
    {
        CONST_VTBL struct IAttributeDBMgrVtbl *lpVtbl;
    };

    

#ifdef COBJMACROS


#define IAttributeDBMgr_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define IAttributeDBMgr_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define IAttributeDBMgr_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define IAttributeDBMgr_GetTypeInfoCount(This,pctinfo)	\
    ( (This)->lpVtbl -> GetTypeInfoCount(This,pctinfo) ) 

#define IAttributeDBMgr_GetTypeInfo(This,iTInfo,lcid,ppTInfo)	\
    ( (This)->lpVtbl -> GetTypeInfo(This,iTInfo,lcid,ppTInfo) ) 

#define IAttributeDBMgr_GetIDsOfNames(This,riid,rgszNames,cNames,lcid,rgDispId)	\
    ( (This)->lpVtbl -> GetIDsOfNames(This,riid,rgszNames,cNames,lcid,rgDispId) ) 

#define IAttributeDBMgr_Invoke(This,dispIdMember,riid,lcid,wFlags,pDispParams,pVarResult,pExcepInfo,puArgErr)	\
    ( (This)->lpVtbl -> Invoke(This,dispIdMember,riid,lcid,wFlags,pDispParams,pVarResult,pExcepInfo,puArgErr) ) 


#define IAttributeDBMgr_put_FAMDB(This,newVal)	\
    ( (This)->lpVtbl -> put_FAMDB(This,newVal) ) 

#endif /* COBJMACROS */


#endif 	/* C style interface */




#endif 	/* __IAttributeDBMgr_INTERFACE_DEFINED__ */


EXTERN_C const CLSID CLSID_AttributeDBMgr;

#ifdef __cplusplus

class DECLSPEC_UUID("02F47B53-FD6A-403F-8BC3-20B96D36A9E7")
AttributeDBMgr;
#endif
#endif /* __UCLID_AttributeDbMgrComponentsLib_LIBRARY_DEFINED__ */

/* Additional Prototypes for ALL interfaces */

/* end of Additional Prototypes */

#ifdef __cplusplus
}
#endif

#endif


