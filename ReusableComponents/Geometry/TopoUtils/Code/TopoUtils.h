
#ifndef TOPO_UTILS_H
#define TOPO_UTILS_H

#ifdef EXPORT_TOPOUTILS_DLL
#define EXPORT_TopoUtils __declspec(dllexport)
#define EXPIMP_TEMPLATE_TOPOUTILS
#elif NO_EXPORT_IMPORT
#define EXPORT_TopoUtils 
#else
#define EXPORT_TopoUtils __declspec(dllimport)
#define EXPIMP_TEMPLATE_TOPOUTILS extern
#endif


#endif //  TOPO_UTILS_H