#ifdef EXPORT_UCLIDCOMUtils_DLL
#define EXPORT_UCLIDCOMUtils __declspec(dllexport)
#define EXPIMP_TEMPLATE_AFCORE
#elif NO_EXPORT_IMPORT
#define EXPORT_UCLIDCOMUtils
#else
#define EXPORT_UCLIDCOMUtils __declspec(dllimport)
#define EXPIMP_TEMPLATE_UCLIDCOMUtils extern
#endif