#ifdef EXPORT_AFCORE_DLL
#define EXPORT_AFCore __declspec(dllexport)
#define EXPIMP_TEMPLATE_AFCORE
#elif NO_EXPORT_IMPORT
#define EXPORT_AFCore
#else
#define EXPORT_AFCore __declspec(dllimport)
#define EXPIMP_TEMPLATE_AFCORE extern
#endif