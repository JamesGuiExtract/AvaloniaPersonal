
// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the CURVECALCULATOR_EXPORTS
// symbol defined on the command line. this symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// CURVECALCULATOR_API functions as being imported from a DLL, wheras this DLL sees symbols
// defined with this macro as being exported.
#ifndef CURVE_CALCULATOR_H
#define CURVE_CALCULATOR_H

#ifdef CURVECALCULATOR_EXPORTS
#define EXPORT_CurveCalculator __declspec(dllexport)
#define EXPIMP_TEMPLATE_CURVE_CALCULATOR
#else
#define EXPORT_CurveCalculator __declspec(dllimport)
#define EXPIMP_TEMPLATE_CURVE_CALCULATOR extern
#endif


#endif // CURVE_CALCULATOR_H
