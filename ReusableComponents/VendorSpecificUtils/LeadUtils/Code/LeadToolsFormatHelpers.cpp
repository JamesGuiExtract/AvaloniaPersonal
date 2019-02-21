#include "stdafx.h"
#include "LeadToolsFormatHelpers.h"

#include <UCLIDException.h>

#include <l_bitmap.h>

#include <afxmt.h>

#include <map>

using namespace std;

//-------------------------------------------------------------------------------------------------
int getFormatFromString(const string& strFormat)
{
	// Map of strings to formats
	static map<string, int> smapStringToFormat;
	static CCriticalSection criticalSection;
	static bool bInitialized = false;

	try
	{
		// Ensure the string is upper case
		string strKey = strFormat;
		makeUpperCase(strKey);

		// If the format string starts with "FILE_" remove it
		size_t nPos = strKey.find("FILE_");
		if (nPos == 0)
		{
			strKey = strKey.substr(5);
		}

		// If the map has not been filled yet, fill it
		if (!bInitialized)
		{
			CSingleLock lock(&criticalSection, TRUE);
			if (!bInitialized)
			{
				smapStringToFormat["PCX"] = FILE_PCX;
				smapStringToFormat["GIF"] = FILE_GIF;
				smapStringToFormat["TIF"] = FILE_TIF;
				smapStringToFormat["TGA"] = FILE_TGA;
				smapStringToFormat["CMP"] = FILE_CMP;
				smapStringToFormat["BMP"] = FILE_BMP;
				smapStringToFormat["BITMAP"] = FILE_BITMAP;
				smapStringToFormat["JPEG"] = FILE_JPEG;
				smapStringToFormat["TIF_JPEG"] = FILE_TIF_JPEG;
				smapStringToFormat["OS2"] = FILE_OS2;
				smapStringToFormat["WMF"] = FILE_WMF;
				smapStringToFormat["EPS"] = FILE_EPS;
				smapStringToFormat["TIFLZW"] = FILE_TIFLZW;
				smapStringToFormat["LEAD"] = FILE_LEAD;
				smapStringToFormat["JPEG_411"] = FILE_JPEG_411;
				smapStringToFormat["TIF_JPEG_411"] = FILE_TIF_JPEG_411;
				smapStringToFormat["JPEG_422"] = FILE_JPEG_422;
				smapStringToFormat["TIF_JPEG_422"] = FILE_TIF_JPEG_422;
				smapStringToFormat["CCITT"] = FILE_CCITT;
				smapStringToFormat["LEAD1BIT"] = FILE_LEAD1BIT;
				smapStringToFormat["CCITT_GROUP3_1DIM"] = FILE_CCITT_GROUP3_1DIM;
				smapStringToFormat["CCITT_GROUP3_2DIM"] = FILE_CCITT_GROUP3_2DIM;
				smapStringToFormat["CCITT_GROUP4"] = FILE_CCITT_GROUP4;
				smapStringToFormat["LEAD1BITA"] = FILE_LEAD1BITA;
				smapStringToFormat["ABC"] = FILE_ABC;
				smapStringToFormat["CALS"] = FILE_CALS;
				smapStringToFormat["MAC"] = FILE_MAC;
				smapStringToFormat["IMG"] = FILE_IMG;
				smapStringToFormat["MSP"] = FILE_MSP;
				smapStringToFormat["WPG"] = FILE_WPG;
				smapStringToFormat["RAS"] = FILE_RAS;
				smapStringToFormat["PCT"] = FILE_PCT;
				smapStringToFormat["PCD"] = FILE_PCD;
				smapStringToFormat["DXF"] = FILE_DXF;
				smapStringToFormat["FLI"] = FILE_FLI;
				smapStringToFormat["CGM"] = FILE_CGM;
				smapStringToFormat["EPSTIFF"] = FILE_EPSTIFF;
				smapStringToFormat["EPSWMF"] = FILE_EPSWMF;
				smapStringToFormat["CMPNOLOSS"] = FILE_CMPNOLOSS;
				smapStringToFormat["FAX_G3_1D"] = FILE_FAX_G3_1D;
				smapStringToFormat["FAX_G3_2D"] = FILE_FAX_G3_2D;
				smapStringToFormat["FAX_G4"] = FILE_FAX_G4;
				smapStringToFormat["WFX_G3_1D"] = FILE_WFX_G3_1D;
				smapStringToFormat["WFX_G4"] = FILE_WFX_G4;
				smapStringToFormat["ICA_G3_1D"] = FILE_ICA_G3_1D;
				smapStringToFormat["ICA_G3_2D"] = FILE_ICA_G3_2D;
				smapStringToFormat["ICA_G4"] = FILE_ICA_G4;
				smapStringToFormat["ICA_IBM_MMR"] = FILE_ICA_IBM_MMR;
				smapStringToFormat["ICA_UNCOMPRESSED"] = FILE_ICA_UNCOMPRESSED;
				smapStringToFormat["ICA_ABIC"] = FILE_ICA_ABIC;
				smapStringToFormat["OS2_2"] = FILE_OS2_2;
				smapStringToFormat["PNG"] = FILE_PNG;
				smapStringToFormat["PSD"] = FILE_PSD;
				smapStringToFormat["RAWICA_G3_1D"] = FILE_RAWICA_G3_1D;
				smapStringToFormat["RAWICA_G3_2D"] = FILE_RAWICA_G3_2D;
				smapStringToFormat["RAWICA_G4"] = FILE_RAWICA_G4;
				smapStringToFormat["RAWICA_IBM_MMR"] = FILE_RAWICA_IBM_MMR;
				smapStringToFormat["RAWICA_UNCOMPRESSED"] = FILE_RAWICA_UNCOMPRESSED;
				smapStringToFormat["RAWICA_ABIC"] = FILE_RAWICA_ABIC;
				smapStringToFormat["FPX"] = FILE_FPX;
				smapStringToFormat["FPX_SINGLE_COLOR"] = FILE_FPX_SINGLE_COLOR;
				smapStringToFormat["FPX_JPEG"] = FILE_FPX_JPEG;
				smapStringToFormat["FPX_JPEG_QFACTOR"] = FILE_FPX_JPEG_QFACTOR;
				smapStringToFormat["BMP_RLE"] = FILE_BMP_RLE;
				smapStringToFormat["TIF_CMYK"] = FILE_TIF_CMYK;
				smapStringToFormat["TIFLZW_CMYK"] = FILE_TIFLZW_CMYK;
				smapStringToFormat["TIF_PACKBITS"] = FILE_TIF_PACKBITS;
				smapStringToFormat["TIF_PACKBITS_CMYK"] = FILE_TIF_PACKBITS_CMYK;
				smapStringToFormat["DICOM_GRAY"] = FILE_DICOM_GRAY;
				smapStringToFormat["DICOM_COLOR"] = FILE_DICOM_COLOR;
				smapStringToFormat["WIN_ICO"] = FILE_WIN_ICO;
				smapStringToFormat["WIN_CUR"] = FILE_WIN_CUR;
				smapStringToFormat["TIF_YCC"] = FILE_TIF_YCC;
				smapStringToFormat["TIFLZW_YCC"] = FILE_TIFLZW_YCC;
				smapStringToFormat["TIF_PACKBITS_YCC"] = FILE_TIF_PACKBITS_YCC;
				smapStringToFormat["EXIF"] = FILE_EXIF;
				smapStringToFormat["EXIF_YCC"] = FILE_EXIF_YCC;
				smapStringToFormat["EXIF_JPEG_422"] = FILE_EXIF_JPEG_422;
				smapStringToFormat["AWD"] = FILE_AWD;
				smapStringToFormat["FASTEST"] = FILE_FASTEST;
				smapStringToFormat["EXIF_JPEG_411"] = FILE_EXIF_JPEG_411;
				smapStringToFormat["PBM_ASCII"] = FILE_PBM_ASCII;
				smapStringToFormat["PBM_BINARY"] = FILE_PBM_BINARY;
				smapStringToFormat["PGM_ASCII"] = FILE_PGM_ASCII;
				smapStringToFormat["PGM_BINARY"] = FILE_PGM_BINARY;
				smapStringToFormat["PPM_ASCII"] = FILE_PPM_ASCII;
				smapStringToFormat["PPM_BINARY"] = FILE_PPM_BINARY;
				smapStringToFormat["CUT"] = FILE_CUT;
				smapStringToFormat["XPM"] = FILE_XPM;
				smapStringToFormat["XBM"] = FILE_XBM;
				smapStringToFormat["IFF_ILBM"] = FILE_IFF_ILBM;
				smapStringToFormat["IFF_CAT"] = FILE_IFF_CAT;
				smapStringToFormat["XWD"] = FILE_XWD;
				smapStringToFormat["CLP"] = FILE_CLP;
				smapStringToFormat["JBIG"] = FILE_JBIG;
				smapStringToFormat["EMF"] = FILE_EMF;
				smapStringToFormat["ANI"] = FILE_ANI;
				smapStringToFormat["LASERDATA"] = FILE_LASERDATA;
				smapStringToFormat["INTERGRAPH_RLE"] = FILE_INTERGRAPH_RLE;
				smapStringToFormat["INTERGRAPH_VECTOR"] = FILE_INTERGRAPH_VECTOR;
				smapStringToFormat["DWG"] = FILE_DWG;
				smapStringToFormat["DICOM_RLE_GRAY"] = FILE_DICOM_RLE_GRAY;
				smapStringToFormat["DICOM_RLE_COLOR"] = FILE_DICOM_RLE_COLOR;
				smapStringToFormat["DICOM_JPEG_GRAY"] = FILE_DICOM_JPEG_GRAY;
				smapStringToFormat["DICOM_JPEG_COLOR"] = FILE_DICOM_JPEG_COLOR;
				smapStringToFormat["CALS4"] = FILE_CALS4;
				smapStringToFormat["CALS2"] = FILE_CALS2;
				smapStringToFormat["CALS3"] = FILE_CALS3;
				smapStringToFormat["XWD10"] = FILE_XWD10;
				smapStringToFormat["XWD11"] = FILE_XWD11;
				smapStringToFormat["FLC"] = FILE_FLC;
				smapStringToFormat["KDC"] = FILE_KDC;
				smapStringToFormat["DRW"] = FILE_DRW;
				smapStringToFormat["PLT"] = FILE_PLT;
				smapStringToFormat["TIF_CMP"] = FILE_TIF_CMP;
				smapStringToFormat["TIF_JBIG"] = FILE_TIF_JBIG;
				smapStringToFormat["TIF_DXF_R13"] = FILE_TIF_DXF_R13;
				smapStringToFormat["TIF_DXF_R12"] = FILE_TIF_DXF_R12;
				smapStringToFormat["TIF_UNKNOWN"] = FILE_TIF_UNKNOWN;
				smapStringToFormat["SGI"] = FILE_SGI;
				smapStringToFormat["SGI_RLE"] = FILE_SGI_RLE;
				smapStringToFormat["VECTOR_DUMP"] = FILE_VECTOR_DUMP;
				smapStringToFormat["DWF"] = FILE_DWF;
				smapStringToFormat["RAS_PDF"] = FILE_RAS_PDF;
				smapStringToFormat["RAS_PDF_G3_1D"] = FILE_RAS_PDF_G3_1D;
				smapStringToFormat["RAS_PDF_G3_2D"] = FILE_RAS_PDF_G3_2D;
				smapStringToFormat["RAS_PDF_G4"] = FILE_RAS_PDF_G4;
				smapStringToFormat["RAS_PDF_JPEG"] = FILE_RAS_PDF_JPEG;
				smapStringToFormat["RAS_PDF_JPEG_422"] = FILE_RAS_PDF_JPEG_422;
				smapStringToFormat["RAS_PDF_JPEG_411"] = FILE_RAS_PDF_JPEG_411;
				smapStringToFormat["RAS_PDF_LZW"] = FILE_RAS_PDF_LZW;
				smapStringToFormat["RAS_PDF_JBIG2"] = FILE_RAS_PDF_JBIG2;
				smapStringToFormat["RAW"] = FILE_RAW;
				smapStringToFormat["RASTER_DUMP"] = FILE_RASTER_DUMP;
				smapStringToFormat["TIF_CUSTOM"] = FILE_TIF_CUSTOM;
				smapStringToFormat["RAW_RGB"] = FILE_RAW_RGB;
				smapStringToFormat["RAW_RLE4"] = FILE_RAW_RLE4;
				smapStringToFormat["RAW_RLE8"] = FILE_RAW_RLE8;
				smapStringToFormat["RAW_BITFIELDS"] = FILE_RAW_BITFIELDS;
				smapStringToFormat["RAW_PACKBITS"] = FILE_RAW_PACKBITS;
				smapStringToFormat["RAW_JPEG"] = FILE_RAW_JPEG;
				smapStringToFormat["FAX_G3_1D_NOEOL"] = FILE_FAX_G3_1D_NOEOL;
				smapStringToFormat["RAW_LZW"] = FILE_RAW_LZW;
				smapStringToFormat["JP2"] = FILE_JP2;
				smapStringToFormat["J2K"] = FILE_J2K;
				smapStringToFormat["CMW"] = FILE_CMW;
				smapStringToFormat["TIF_J2K"] = FILE_TIF_J2K;
				smapStringToFormat["TIF_CMW"] = FILE_TIF_CMW;
				smapStringToFormat["MRC"] = FILE_MRC;
				smapStringToFormat["GERBER"] = FILE_GERBER;
				smapStringToFormat["WBMP"] = FILE_WBMP;
				smapStringToFormat["JPEG_LAB"] = FILE_JPEG_LAB;
				smapStringToFormat["JPEG_LAB_411"] = FILE_JPEG_LAB_411;
				smapStringToFormat["JPEG_LAB_422"] = FILE_JPEG_LAB_422;
				smapStringToFormat["GEOTIFF"] = FILE_GEOTIFF;
				smapStringToFormat["TIF_LEAD1BIT"] = FILE_TIF_LEAD1BIT;
				smapStringToFormat["TIF_MRC"] = FILE_TIF_MRC;
				smapStringToFormat["TIF_ABC"] = FILE_TIF_ABC;
				smapStringToFormat["NAP"] = FILE_NAP;
				smapStringToFormat["JPEG_RGB"] = FILE_JPEG_RGB;
				smapStringToFormat["JBIG2"] = FILE_JBIG2;
				smapStringToFormat["ABIC"] = FILE_ABIC;
				smapStringToFormat["TIF_ABIC"] = FILE_TIF_ABIC;
				smapStringToFormat["TIF_JBIG2"] = FILE_TIF_JBIG2;
				smapStringToFormat["TIF_ZIP"] = FILE_TIF_ZIP;
				smapStringToFormat["SVG"] = FILE_SVG;
				smapStringToFormat["NITF"] = FILE_NITF;
				smapStringToFormat["PTOCA"] = FILE_PTOCA;
				smapStringToFormat["SCT"] = FILE_SCT;
				smapStringToFormat["PCL"] = FILE_PCL;
				smapStringToFormat["AFP"] = FILE_AFP;
				smapStringToFormat["SHP"] = FILE_SHP;
				smapStringToFormat["SMP"] = FILE_SMP;
				smapStringToFormat["SMP_G3_1D"] = FILE_SMP_G3_1D;
				smapStringToFormat["SMP_G3_2D"] = FILE_SMP_G3_2D;
				smapStringToFormat["SMP_G4"] = FILE_SMP_G4;
				smapStringToFormat["VWPG"] = FILE_VWPG;
				smapStringToFormat["VWPG1"] = FILE_VWPG1;
				smapStringToFormat["CMX"] = FILE_CMX;
				smapStringToFormat["TGA_RLE"] = FILE_TGA_RLE;
				smapStringToFormat["KDC_120"] = FILE_KDC_120;
				smapStringToFormat["KDC_40"] = FILE_KDC_40;
				smapStringToFormat["KDC_50"] = FILE_KDC_50;
				smapStringToFormat["DCS"] = FILE_DCS;
				smapStringToFormat["PSP"] = FILE_PSP;
				smapStringToFormat["PSP_RLE"] = FILE_PSP_RLE;
				smapStringToFormat["TIFX_JBIG"] = FILE_TIFX_JBIG;
				smapStringToFormat["TIFX_JBIG_T43"] = FILE_TIFX_JBIG_T43;
				smapStringToFormat["TIFX_JBIG_T43_ITULAB"] = FILE_TIFX_JBIG_T43_ITULAB;
				smapStringToFormat["TIFX_JBIG_T43_GS"] = FILE_TIFX_JBIG_T43_GS;
				smapStringToFormat["TIFX_FAX_G4"] = FILE_TIFX_FAX_G4;
				smapStringToFormat["TIFX_FAX_G3_1D"] = FILE_TIFX_FAX_G3_1D;
				smapStringToFormat["TIFX_FAX_G3_2D"] = FILE_TIFX_FAX_G3_2D;
				smapStringToFormat["TIFX_JPEG"] = FILE_TIFX_JPEG;
				smapStringToFormat["ECW"] = FILE_ECW;
				smapStringToFormat["RAS_RLE"] = FILE_RAS_RLE;
				smapStringToFormat["SVG_EMBED_IMAGES"] = FILE_SVG_EMBED_IMAGES;
				smapStringToFormat["DXF_R13"] = FILE_DXF_R13;
				smapStringToFormat["CLP_RLE"] = FILE_CLP_RLE;
				smapStringToFormat["DCR"] = FILE_DCR;
				smapStringToFormat["DICOM_J2K_GRAY"] = FILE_DICOM_J2K_GRAY;
				smapStringToFormat["DICOM_J2K_COLOR"] = FILE_DICOM_J2K_COLOR;
				smapStringToFormat["FIT"] = FILE_FIT;
				smapStringToFormat["CRW"] = FILE_CRW;
				smapStringToFormat["DWF_TEXT_AS_POLYLINE"] = FILE_DWF_TEXT_AS_POLYLINE;
				smapStringToFormat["CIN"] = FILE_CIN;
				smapStringToFormat["PCL_TEXT_AS_POLYLINE"] = FILE_PCL_TEXT_AS_POLYLINE;
				smapStringToFormat["EPSPOSTSCRIPT"] = FILE_EPSPOSTSCRIPT;
				smapStringToFormat["INTERGRAPH_CCITT_G4"] = FILE_INTERGRAPH_CCITT_G4;
				smapStringToFormat["SFF"] = FILE_SFF;
				smapStringToFormat["IFF_ILBM_UNCOMPRESSED"] = FILE_IFF_ILBM_UNCOMPRESSED;
				smapStringToFormat["IFF_CAT_UNCOMPRESSED"] = FILE_IFF_CAT_UNCOMPRESSED;
				smapStringToFormat["RTF_RASTER"] = FILE_RTF_RASTER;
				smapStringToFormat["WMZ"] = FILE_WMZ;
				smapStringToFormat["AFPICA_G3_1D"] = FILE_AFPICA_G3_1D;
				smapStringToFormat["AFPICA_G3_2D"] = FILE_AFPICA_G3_2D;
				smapStringToFormat["AFPICA_G4"] = FILE_AFPICA_G4;
				smapStringToFormat["AFPICA_UNCOMPRESSED"] = FILE_AFPICA_UNCOMPRESSED;
				smapStringToFormat["AFPICA_IBM_MMR"] = FILE_AFPICA_IBM_MMR;
				smapStringToFormat["AFPICA_ABIC"] = FILE_AFPICA_ABIC;
				smapStringToFormat["LEAD_MRC"] = FILE_LEAD_MRC;
				smapStringToFormat["TIF_LEAD_MRC"] = FILE_TIF_LEAD_MRC;
				smapStringToFormat["TXT"] = FILE_TXT;
				smapStringToFormat["PDF_LEAD_MRC"] = FILE_PDF_LEAD_MRC;
				smapStringToFormat["HDP"] = FILE_HDP;
				smapStringToFormat["HDP_GRAY"] = FILE_HDP_GRAY;
				smapStringToFormat["HDP_CMYK"] = FILE_HDP_CMYK;
				smapStringToFormat["PNG_ICO"] = FILE_PNG_ICO;
				smapStringToFormat["XPS"] = FILE_XPS;
				smapStringToFormat["JPX"] = FILE_JPX;
				smapStringToFormat["XPS_JPEG"] = FILE_XPS_JPEG;
				smapStringToFormat["XPS_JPEG_422"] = FILE_XPS_JPEG_422;
				smapStringToFormat["XPS_JPEG_411"] = FILE_XPS_JPEG_411;
				smapStringToFormat["MNG"] = FILE_MNG;
				smapStringToFormat["MNG_GRAY"] = FILE_MNG_GRAY;
				smapStringToFormat["MNG_JNG"] = FILE_MNG_JNG;
				smapStringToFormat["MNG_JNG_411"] = FILE_MNG_JNG_411;
				smapStringToFormat["MNG_JNG_422"] = FILE_MNG_JNG_422;
				smapStringToFormat["RAS_PDF_CMYK"] = FILE_RAS_PDF_CMYK;
				smapStringToFormat["RAS_PDF_LZW_CMYK"] = FILE_RAS_PDF_LZW_CMYK;
				smapStringToFormat["MIF"] = FILE_MIF;
				smapStringToFormat["E00"] = FILE_E00;
				smapStringToFormat["TDB"] = FILE_TDB;
				smapStringToFormat["TDB_VISTA"] = FILE_TDB_VISTA;
				smapStringToFormat["SNP"] = FILE_SNP;
				smapStringToFormat["JTIF"] = FILE_TIF_JPEG;
				smapStringToFormat["LEAD1JTIF"] = FILE_TIF_JPEG_411;
				smapStringToFormat["LEAD2JTIF"] = FILE_TIF_JPEG_422;
				smapStringToFormat["JFIF"] = FILE_JPEG;
				smapStringToFormat["LEAD1JFIF"] = FILE_JPEG_411;
				smapStringToFormat["LEAD2JFIF"] = FILE_JPEG_422;

				bInitialized = true;
			}
		}

		map<string, int>::iterator it = smapStringToFormat.find(strKey);
		if (it != smapStringToFormat.end())
		{
			return it->second;
		}

		UCLIDException uex("ELI25416", "Unknown file format string.");
		uex.addDebugInfo("File format", strFormat);
		uex.addDebugInfo("Format key", strKey);
		throw uex;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25423");
}
//-------------------------------------------------------------------------------------------------
string getStringFromFormat(int nFormat)
{
	try
	{
		switch (nFormat)
		{
		case FILE_PCX:
			return "PCX";
		case FILE_GIF:
			return "GIF";
		case FILE_TIF:
			return "TIF";
		case FILE_TGA:
			return "TGA";
		case FILE_CMP:
			return "CMP";
		case FILE_BMP:
			return "BMP";
		case FILE_BITMAP:
			return "BITMAP";
		case FILE_JPEG:
			return "JPEG";
		case FILE_TIF_JPEG:
			return "TIF_JPEG";
		case FILE_OS2:
			return "OS2";
		case FILE_WMF:
			return "WMF";
		case FILE_EPS:
			return "EPS";
		case FILE_TIFLZW:
			return "TIFLZW";
		case FILE_LEAD:
			return "LEAD";
		case FILE_JPEG_411:
			return "JPEG_411";
		case FILE_TIF_JPEG_411:
			return "TIF_JPEG_411";
		case FILE_JPEG_422:
			return "JPEG_422";
		case FILE_TIF_JPEG_422:
			return "TIF_JPEG_422";
		case FILE_CCITT:
			return "CCITT";
		case FILE_LEAD1BIT:
			return "LEAD1BIT";
		case FILE_CCITT_GROUP3_1DIM:
			return "CCITT_GROUP3_1DIM";
		case FILE_CCITT_GROUP3_2DIM:
			return "CCITT_GROUP3_2DIM";
		case FILE_CCITT_GROUP4:
			return "CCITT_GROUP4";
		case FILE_LEAD1BITA:
			return "LEAD1BITA";
		case FILE_ABC:
			return "ABC";
		case FILE_CALS:
			return "CALS";
		case FILE_MAC:
			return "MAC";
		case FILE_IMG:
			return "IMG";
		case FILE_MSP:
			return "MSP";
		case FILE_WPG:
			return "WPG";
		case FILE_RAS:
			return "RAS";
		case FILE_PCT:
			return "PCT";
		case FILE_PCD:
			return "PCD";
		case FILE_DXF:
			return "DXF";
		case FILE_FLI:
			return "FLI";
		case FILE_CGM:
			return "CGM";
		case FILE_EPSTIFF:
			return "EPSTIFF";
		case FILE_EPSWMF:
			return "EPSWMF";
		case FILE_CMPNOLOSS:
			return "CMPNOLOSS";
		case FILE_FAX_G3_1D:
			return "FAX_G3_1D";
		case FILE_FAX_G3_2D:
			return "FAX_G3_2D";
		case FILE_FAX_G4:
			return "FAX_G4";
		case FILE_WFX_G3_1D:
			return "WFX_G3_1D";
		case FILE_WFX_G4:
			return "WFX_G4";
		case FILE_ICA_G3_1D:
			return "ICA_G3_1D";
		case FILE_ICA_G3_2D:
			return "ICA_G3_2D";
		case FILE_ICA_G4:
			return "ICA_G4";
		case FILE_ICA_IBM_MMR:
			return "ICA_IBM_MMR";
		case FILE_ICA_UNCOMPRESSED:
			return "ICA_UNCOMPRESSED";
		case FILE_ICA_ABIC:
			return "ICA_ABIC";
		case FILE_OS2_2:
			return "OS2_2";
		case FILE_PNG:
			return "PNG";
		case FILE_PSD:
			return "PSD";
		case FILE_RAWICA_G3_1D:
			return "RAWICA_G3_1D";
		case FILE_RAWICA_G3_2D:
			return "RAWICA_G3_2D";
		case FILE_RAWICA_G4:
			return "RAWICA_G4";
		case FILE_RAWICA_IBM_MMR:
			return "RAWICA_IBM_MMR";
		case FILE_RAWICA_UNCOMPRESSED:
			return "RAWICA_UNCOMPRESSED";
		case FILE_RAWICA_ABIC:
			return "RAWICA_ABIC";
		case FILE_FPX:
			return "FPX";
		case FILE_FPX_SINGLE_COLOR:
			return "FPX_SINGLE_COLOR";
		case FILE_FPX_JPEG:
			return "FPX_JPEG";
		case FILE_FPX_JPEG_QFACTOR:
			return "FPX_JPEG_QFACTOR";
		case FILE_BMP_RLE:
			return "BMP_RLE";
		case FILE_TIF_CMYK:
			return "TIF_CMYK";
		case FILE_TIFLZW_CMYK:
			return "TIFLZW_CMYK";
		case FILE_TIF_PACKBITS:
			return "TIF_PACKBITS";
		case FILE_TIF_PACKBITS_CMYK:
			return "TIF_PACKBITS_CMYK";
		case FILE_DICOM_GRAY:
			return "DICOM_GRAY";
		case FILE_DICOM_COLOR:
			return "DICOM_COLOR";
		case FILE_WIN_ICO:
			return "WIN_ICO";
		case FILE_WIN_CUR:
			return "WIN_CUR";
		case FILE_TIF_YCC:
			return "TIF_YCC";
		case FILE_TIFLZW_YCC:
			return "TIFLZW_YCC";
		case FILE_TIF_PACKBITS_YCC:
			return "TIF_PACKBITS_YCC";
		case FILE_EXIF:
			return "EXIF";
		case FILE_EXIF_YCC:
			return "EXIF_YCC";
		case FILE_EXIF_JPEG_422:
			return "EXIF_JPEG_422";
		case FILE_AWD:
			return "AWD";
		case FILE_FASTEST:
			return "FASTEST";
		case FILE_EXIF_JPEG_411:
			return "EXIF_JPEG_411";
		case FILE_PBM_ASCII:
			return "PBM_ASCII";
		case FILE_PBM_BINARY:
			return "PBM_BINARY";
		case FILE_PGM_ASCII:
			return "PGM_ASCII";
		case FILE_PGM_BINARY:
			return "PGM_BINARY";
		case FILE_PPM_ASCII:
			return "PPM_ASCII";
		case FILE_PPM_BINARY:
			return "PPM_BINARY";
		case FILE_CUT:
			return "CUT";
		case FILE_XPM:
			return "XPM";
		case FILE_XBM:
			return "XBM";
		case FILE_IFF_ILBM:
			return "IFF_ILBM";
		case FILE_IFF_CAT:
			return "IFF_CAT";
		case FILE_XWD:
			return "XWD";
		case FILE_CLP:
			return "CLP";
		case FILE_JBIG:
			return "JBIG";
		case FILE_EMF:
			return "EMF";
		case FILE_ANI:
			return "ANI";
		case FILE_LASERDATA:
			return "LASERDATA";
		case FILE_INTERGRAPH_RLE:
			return "INTERGRAPH_RLE";
		case FILE_INTERGRAPH_VECTOR:
			return "INTERGRAPH_VECTOR";
		case FILE_DWG:
			return "DWG";
		case FILE_DICOM_RLE_GRAY:
			return "DICOM_RLE_GRAY";
		case FILE_DICOM_RLE_COLOR:
			return "DICOM_RLE_COLOR";
		case FILE_DICOM_JPEG_GRAY:
			return "DICOM_JPEG_GRAY";
		case FILE_DICOM_JPEG_COLOR:
			return "DICOM_JPEG_COLOR";
		case FILE_CALS4:
			return "CALS4";
		case FILE_CALS2:
			return "CALS2";
		case FILE_CALS3:
			return "CALS3";
		case FILE_XWD10:
			return "XWD10";
		case FILE_XWD11:
			return "XWD11";
		case FILE_FLC:
			return "FLC";
		case FILE_KDC:
			return "KDC";
		case FILE_DRW:
			return "DRW";
		case FILE_PLT:
			return "PLT";
		case FILE_TIF_CMP:
			return "TIF_CMP";
		case FILE_TIF_JBIG:
			return "TIF_JBIG";
		case FILE_TIF_DXF_R13:
			return "TIF_DXF_R13";
		case FILE_TIF_DXF_R12:
			return "TIF_DXF_R12";
		case FILE_TIF_UNKNOWN:
			return "TIF_UNKNOWN";
		case FILE_SGI:
			return "SGI";
		case FILE_SGI_RLE:
			return "SGI_RLE";
		case FILE_VECTOR_DUMP:
			return "VECTOR_DUMP";
		case FILE_DWF:
			return "DWF";
		case FILE_RAS_PDF:
			return "RAS_PDF";
		case FILE_RAS_PDF_G3_1D:
			return "RAS_PDF_G3_1D";
		case FILE_RAS_PDF_G3_2D:
			return "RAS_PDF_G3_2D";
		case FILE_RAS_PDF_G4:
			return "RAS_PDF_G4";
		case FILE_RAS_PDF_JPEG:
			return "RAS_PDF_JPEG";
		case FILE_RAS_PDF_JPEG_422:
			return "RAS_PDF_JPEG_422";
		case FILE_RAS_PDF_JPEG_411:
			return "RAS_PDF_JPEG_411";
		case FILE_RAS_PDF_LZW:
			return "RAS_PDF_LZW";
		case FILE_RAS_PDF_JBIG2:
			return "RAS_PDF_JBIG2";
		case FILE_RAW:
			return "RAW";
		case FILE_RASTER_DUMP:
			return "RASTER_DUMP";
		case FILE_TIF_CUSTOM:
			return "TIF_CUSTOM";
		case FILE_RAW_RGB:
			return "RAW_RGB";
		case FILE_RAW_RLE4:
			return "RAW_RLE4";
		case FILE_RAW_RLE8:
			return "RAW_RLE8";
		case FILE_RAW_BITFIELDS:
			return "RAW_BITFIELDS";
		case FILE_RAW_PACKBITS:
			return "RAW_PACKBITS";
		case FILE_RAW_JPEG:
			return "RAW_JPEG";
		case FILE_FAX_G3_1D_NOEOL:
			return "FAX_G3_1D_NOEOL";
		case FILE_RAW_LZW:
			return "RAW_LZW";
		case FILE_JP2:
			return "JP2";
		case FILE_J2K:
			return "J2K";
		case FILE_CMW:
			return "CMW";
		case FILE_TIF_J2K:
			return "TIF_J2K";
		case FILE_TIF_CMW:
			return "TIF_CMW";
		case FILE_MRC:
			return "MRC";
		case FILE_GERBER:
			return "GERBER";
		case FILE_WBMP:
			return "WBMP";
		case FILE_JPEG_LAB:
			return "JPEG_LAB";
		case FILE_JPEG_LAB_411:
			return "JPEG_LAB_411";
		case FILE_JPEG_LAB_422:
			return "JPEG_LAB_422";
		case FILE_GEOTIFF:
			return "GEOTIFF";
		case FILE_TIF_LEAD1BIT:
			return "TIF_LEAD1BIT";
		case FILE_TIF_MRC:
			return "TIF_MRC";
		case FILE_TIF_ABC:
			return "TIF_ABC";
		case FILE_NAP:
			return "NAP";
		case FILE_JPEG_RGB:
			return "JPEG_RGB";
		case FILE_JBIG2:
			return "JBIG2";
		case FILE_ABIC:
			return "ABIC";
		case FILE_TIF_ABIC:
			return "TIF_ABIC";
		case FILE_TIF_JBIG2:
			return "TIF_JBIG2";
		case FILE_TIF_ZIP:
			return "TIF_ZIP";
		case FILE_SVG:
			return "SVG";
		case FILE_NITF:
			return "NITF";
		case FILE_PTOCA:
			return "PTOCA";
		case FILE_SCT:
			return "SCT";
		case FILE_PCL:
			return "PCL";
		case FILE_AFP:
			return "AFP";
		case FILE_SHP:
			return "SHP";
		case FILE_SMP:
			return "SMP";
		case FILE_SMP_G3_1D:
			return "SMP_G3_1D";
		case FILE_SMP_G3_2D:
			return "SMP_G3_2D";
		case FILE_SMP_G4:
			return "SMP_G4";
		case FILE_VWPG:
			return "VWPG";
		case FILE_VWPG1:
			return "VWPG1";
		case FILE_CMX:
			return "CMX";
		case FILE_TGA_RLE:
			return "TGA_RLE";
		case FILE_KDC_120:
			return "KDC_120";
		case FILE_KDC_40:
			return "KDC_40";
		case FILE_KDC_50:
			return "KDC_50";
		case FILE_DCS:
			return "DCS";
		case FILE_PSP:
			return "PSP";
		case FILE_PSP_RLE:
			return "PSP_RLE";
		case FILE_TIFX_JBIG:
			return "TIFX_JBIG";
		case FILE_TIFX_JBIG_T43:
			return "TIFX_JBIG_T43";
		case FILE_TIFX_JBIG_T43_ITULAB:
			return "TIFX_JBIG_T43_ITULAB";
		case FILE_TIFX_JBIG_T43_GS:
			return "TIFX_JBIG_T43_GS";
		case FILE_TIFX_FAX_G4:
			return "TIFX_FAX_G4";
		case FILE_TIFX_FAX_G3_1D:
			return "TIFX_FAX_G3_1D";
		case FILE_TIFX_FAX_G3_2D:
			return "TIFX_FAX_G3_2D";
		case FILE_TIFX_JPEG:
			return "TIFX_JPEG";
		case FILE_ECW:
			return "ECW";
		case FILE_RAS_RLE:
			return "RAS_RLE";
		case FILE_SVG_EMBED_IMAGES:
			return "SVG_EMBED_IMAGES";
		case FILE_DXF_R13:
			return "DXF_R13";
		case FILE_CLP_RLE:
			return "CLP_RLE";
		case FILE_DCR:
			return "DCR";
		case FILE_DICOM_J2K_GRAY:
			return "DICOM_J2K_GRAY";
		case FILE_DICOM_J2K_COLOR:
			return "DICOM_J2K_COLOR";
		case FILE_FIT:
			return "FIT";
		case FILE_CRW:
			return "CRW";
		case FILE_DWF_TEXT_AS_POLYLINE:
			return "DWF_TEXT_AS_POLYLINE";
		case FILE_CIN:
			return "CIN";
		case FILE_PCL_TEXT_AS_POLYLINE:
			return "PCL_TEXT_AS_POLYLINE";
		case FILE_EPSPOSTSCRIPT:
			return "EPSPOSTSCRIPT";
		case FILE_INTERGRAPH_CCITT_G4:
			return "INTERGRAPH_CCITT_G4";
		case FILE_SFF:
			return "SFF";
		case FILE_IFF_ILBM_UNCOMPRESSED:
			return "IFF_ILBM_UNCOMPRESSED";
		case FILE_IFF_CAT_UNCOMPRESSED:
			return "IFF_CAT_UNCOMPRESSED";
		case FILE_RTF_RASTER:
			return "RTF_RASTER";
		case FILE_WMZ:
			return "WMZ";
		case FILE_AFPICA_G3_1D:
			return "AFPICA_G3_1D";
		case FILE_AFPICA_G3_2D:
			return "AFPICA_G3_2D";
		case FILE_AFPICA_G4:
			return "AFPICA_G4";
		case FILE_AFPICA_UNCOMPRESSED:
			return "AFPICA_UNCOMPRESSED";
		case FILE_AFPICA_IBM_MMR:
			return "AFPICA_IBM_MMR";
		case FILE_AFPICA_ABIC:
			return "AFPICA_ABIC";
		case FILE_LEAD_MRC:
			return "LEAD_MRC";
		case FILE_TIF_LEAD_MRC:
			return "TIF_LEAD_MRC";
		case FILE_TXT:
			return "TXT";
		case FILE_PDF_LEAD_MRC:
			return "PDF_LEAD_MRC";
		case FILE_HDP:
			return "HDP";
		case FILE_HDP_GRAY:
			return "HDP_GRAY";
		case FILE_HDP_CMYK:
			return "HDP_CMYK";
		case FILE_PNG_ICO:
			return "PNG_ICO";
		case FILE_XPS:
			return "XPS";
		case FILE_JPX:
			return "JPX";
		case FILE_XPS_JPEG:
			return "XPS_JPEG";
		case FILE_XPS_JPEG_422:
			return "XPS_JPEG_422";
		case FILE_XPS_JPEG_411:
			return "XPS_JPEG_411";
		case FILE_MNG:
			return "MNG";
		case FILE_MNG_GRAY:
			return "MNG_GRAY";
		case FILE_MNG_JNG:
			return "MNG_JNG";
		case FILE_MNG_JNG_411:
			return "MNG_JNG_411";
		case FILE_MNG_JNG_422:
			return "MNG_JNG_422";
		case FILE_RAS_PDF_CMYK:
			return "RAS_PDF_CMYK";
		case FILE_RAS_PDF_LZW_CMYK:
			return "RAS_PDF_LZW_CMYK";
		case FILE_MIF:
			return "MIF";
		case FILE_E00:
			return "E00";
		case FILE_TDB:
			return "TDB";
		case FILE_TDB_VISTA:
			return "TDB_VISTA";
		case FILE_SNP:
			return "SNP";
		}

		UCLIDException ue("ELI25417", "Unrecognized file format ID.");
		ue.addDebugInfo("Format ID", nFormat);
		throw ue;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25424");
}
//-------------------------------------------------------------------------------------------------
void addFormatDebugInfo(UCLIDException& rUex, int nFormat)
{
	try
	{
		// Try to add the format string
		rUex.addDebugInfo("File Format", getStringFromFormat(nFormat));
	}
	catch(...)
	{
		rUex.addDebugInfo("File Format", "Unrecognized Format");
	}

	rUex.addDebugInfo("File Format ID", nFormat);
}
//-------------------------------------------------------------------------------------------------