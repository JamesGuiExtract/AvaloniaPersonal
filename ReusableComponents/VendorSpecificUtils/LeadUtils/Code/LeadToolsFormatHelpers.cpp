#include "stdafx.h"
#include "LeadToolsFormatHelpers.h"

#include <UCLIDException.h>
#include <StringCSIS.h>
#include <l_bitmap.h>

//-------------------------------------------------------------------------------------------------
L_INT getFormatFromString(string strFormat)
{
	// Map of strings to formats
	static map<string, L_INT> smapStringToFormat;

	try
	{
		// Ensure the string is upper case
		makeUpperCase(strFormat);

		// If the format string contains FILE_ remove it
		size_t nPos = strFormat.find("FILE_");
		if (nPos != string::npos)
		{
			strFormat = strFormat.substr(nPos+5);
		}

		// If the map has not been filled yet, fill it
		if (smapStringToFormat.empty())
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
			smapStringToFormat["AVI"] = FILE_AVI;
			smapStringToFormat["WAV"] = FILE_WAV;
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
			smapStringToFormat["SID"] = FILE_SID;
			smapStringToFormat["WMZ"] = FILE_WMZ;
			smapStringToFormat["DJVU"] = FILE_DJVU;
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

		}

		map<string, L_INT>::iterator it = smapStringToFormat.find(strFormat);
		if (it != smapStringToFormat.end())
		{
			return it->second;
		}

		UCLIDException uex("ELI25416", "Unknown file format string!");
		uex.addDebugInfo("File Format", strFormat);
		throw uex;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25423");
}
//-------------------------------------------------------------------------------------------------
string getStringFromFormat(L_INT nFormat)
{
	// Map of formats to strings
	static map<L_INT, string> smapFormatToString;

	try
	{

		// If the map has not been filled yet, fill it
		if (smapFormatToString.empty())
		{
			smapFormatToString[FILE_PCX] = "PCX";
			smapFormatToString[FILE_GIF] = "GIF";
			smapFormatToString[FILE_TIF] = "TIF";
			smapFormatToString[FILE_TGA] = "TGA";
			smapFormatToString[FILE_CMP] = "CMP";
			smapFormatToString[FILE_BMP] = "BMP";
			smapFormatToString[FILE_BITMAP] = "BITMAP";
			smapFormatToString[FILE_JPEG] = "JPEG";
			smapFormatToString[FILE_TIF_JPEG] = "TIF_JPEG";
			smapFormatToString[FILE_OS2] = "OS2";
			smapFormatToString[FILE_WMF] = "WMF";
			smapFormatToString[FILE_EPS] = "EPS";
			smapFormatToString[FILE_TIFLZW] = "TIFLZW";
			smapFormatToString[FILE_LEAD] = "LEAD";
			smapFormatToString[FILE_JPEG_411] = "JPEG_411";
			smapFormatToString[FILE_TIF_JPEG_411] = "TIF_JPEG_411";
			smapFormatToString[FILE_JPEG_422] = "JPEG_422";
			smapFormatToString[FILE_TIF_JPEG_422] = "TIF_JPEG_422";
			smapFormatToString[FILE_CCITT] = "CCITT";
			smapFormatToString[FILE_LEAD1BIT] = "LEAD1BIT";
			smapFormatToString[FILE_CCITT_GROUP3_1DIM] = "CCITT_GROUP3_1DIM";
			smapFormatToString[FILE_CCITT_GROUP3_2DIM] = "CCITT_GROUP3_2DIM";
			smapFormatToString[FILE_CCITT_GROUP4] = "CCITT_GROUP4";
			smapFormatToString[FILE_LEAD1BITA] = "LEAD1BITA";
			smapFormatToString[FILE_ABC] = "ABC";
			smapFormatToString[FILE_CALS] = "CALS";
			smapFormatToString[FILE_MAC] = "MAC";
			smapFormatToString[FILE_IMG] = "IMG";
			smapFormatToString[FILE_MSP] = "MSP";
			smapFormatToString[FILE_WPG] = "WPG";
			smapFormatToString[FILE_RAS] = "RAS";
			smapFormatToString[FILE_PCT] = "PCT";
			smapFormatToString[FILE_PCD] = "PCD";
			smapFormatToString[FILE_DXF] = "DXF";
			smapFormatToString[FILE_AVI] = "AVI";
			smapFormatToString[FILE_WAV] = "WAV";
			smapFormatToString[FILE_FLI] = "FLI";
			smapFormatToString[FILE_CGM] = "CGM";
			smapFormatToString[FILE_EPSTIFF] = "EPSTIFF";
			smapFormatToString[FILE_EPSWMF] = "EPSWMF";
			smapFormatToString[FILE_CMPNOLOSS] = "CMPNOLOSS";
			smapFormatToString[FILE_FAX_G3_1D] = "FAX_G3_1D";
			smapFormatToString[FILE_FAX_G3_2D] = "FAX_G3_2D";
			smapFormatToString[FILE_FAX_G4] = "FAX_G4";
			smapFormatToString[FILE_WFX_G3_1D] = "WFX_G3_1D";
			smapFormatToString[FILE_WFX_G4] = "WFX_G4";
			smapFormatToString[FILE_ICA_G3_1D] = "ICA_G3_1D";
			smapFormatToString[FILE_ICA_G3_2D] = "ICA_G3_2D";
			smapFormatToString[FILE_ICA_G4] = "ICA_G4";
			smapFormatToString[FILE_ICA_IBM_MMR] = "ICA_IBM_MMR";
			smapFormatToString[FILE_ICA_UNCOMPRESSED] = "ICA_UNCOMPRESSED";
			smapFormatToString[FILE_ICA_ABIC] = "ICA_ABIC";
			smapFormatToString[FILE_OS2_2] = "OS2_2";
			smapFormatToString[FILE_PNG] = "PNG";
			smapFormatToString[FILE_PSD] = "PSD";
			smapFormatToString[FILE_RAWICA_G3_1D] = "RAWICA_G3_1D";
			smapFormatToString[FILE_RAWICA_G3_2D] = "RAWICA_G3_2D";
			smapFormatToString[FILE_RAWICA_G4] = "RAWICA_G4";
			smapFormatToString[FILE_RAWICA_IBM_MMR] = "RAWICA_IBM_MMR";
			smapFormatToString[FILE_RAWICA_UNCOMPRESSED] = "RAWICA_UNCOMPRESSED";
			smapFormatToString[FILE_RAWICA_ABIC] = "RAWICA_ABIC";
			smapFormatToString[FILE_FPX] = "FPX";
			smapFormatToString[FILE_FPX_SINGLE_COLOR] = "FPX_SINGLE_COLOR";
			smapFormatToString[FILE_FPX_JPEG] = "FPX_JPEG";
			smapFormatToString[FILE_FPX_JPEG_QFACTOR] = "FPX_JPEG_QFACTOR";
			smapFormatToString[FILE_BMP_RLE] = "BMP_RLE";
			smapFormatToString[FILE_TIF_CMYK] = "TIF_CMYK";
			smapFormatToString[FILE_TIFLZW_CMYK] = "TIFLZW_CMYK";
			smapFormatToString[FILE_TIF_PACKBITS] = "TIF_PACKBITS";
			smapFormatToString[FILE_TIF_PACKBITS_CMYK] = "TIF_PACKBITS_CMYK";
			smapFormatToString[FILE_DICOM_GRAY] = "DICOM_GRAY";
			smapFormatToString[FILE_DICOM_COLOR] = "DICOM_COLOR";
			smapFormatToString[FILE_WIN_ICO] = "WIN_ICO";
			smapFormatToString[FILE_WIN_CUR] = "WIN_CUR";
			smapFormatToString[FILE_TIF_YCC] = "TIF_YCC";
			smapFormatToString[FILE_TIFLZW_YCC] = "TIFLZW_YCC";
			smapFormatToString[FILE_TIF_PACKBITS_YCC] = "TIF_PACKBITS_YCC";
			smapFormatToString[FILE_EXIF] = "EXIF";
			smapFormatToString[FILE_EXIF_YCC] = "EXIF_YCC";
			smapFormatToString[FILE_EXIF_JPEG_422] = "EXIF_JPEG_422";
			smapFormatToString[FILE_AWD] = "AWD";
			smapFormatToString[FILE_FASTEST] = "FASTEST";
			smapFormatToString[FILE_EXIF_JPEG_411] = "EXIF_JPEG_411";
			smapFormatToString[FILE_PBM_ASCII] = "PBM_ASCII";
			smapFormatToString[FILE_PBM_BINARY] = "PBM_BINARY";
			smapFormatToString[FILE_PGM_ASCII] = "PGM_ASCII";
			smapFormatToString[FILE_PGM_BINARY] = "PGM_BINARY";
			smapFormatToString[FILE_PPM_ASCII] = "PPM_ASCII";
			smapFormatToString[FILE_PPM_BINARY] = "PPM_BINARY";
			smapFormatToString[FILE_CUT] = "CUT";
			smapFormatToString[FILE_XPM] = "XPM";
			smapFormatToString[FILE_XBM] = "XBM";
			smapFormatToString[FILE_IFF_ILBM] = "IFF_ILBM";
			smapFormatToString[FILE_IFF_CAT] = "IFF_CAT";
			smapFormatToString[FILE_XWD] = "XWD";
			smapFormatToString[FILE_CLP] = "CLP";
			smapFormatToString[FILE_JBIG] = "JBIG";
			smapFormatToString[FILE_EMF] = "EMF";
			smapFormatToString[FILE_ANI] = "ANI";
			smapFormatToString[FILE_LASERDATA] = "LASERDATA";
			smapFormatToString[FILE_INTERGRAPH_RLE] = "INTERGRAPH_RLE";
			smapFormatToString[FILE_INTERGRAPH_VECTOR] = "INTERGRAPH_VECTOR";
			smapFormatToString[FILE_DWG] = "DWG";
			smapFormatToString[FILE_DICOM_RLE_GRAY] = "DICOM_RLE_GRAY";
			smapFormatToString[FILE_DICOM_RLE_COLOR] = "DICOM_RLE_COLOR";
			smapFormatToString[FILE_DICOM_JPEG_GRAY] = "DICOM_JPEG_GRAY";
			smapFormatToString[FILE_DICOM_JPEG_COLOR] = "DICOM_JPEG_COLOR";
			smapFormatToString[FILE_CALS4] = "CALS4";
			smapFormatToString[FILE_CALS2] = "CALS2";
			smapFormatToString[FILE_CALS3] = "CALS3";
			smapFormatToString[FILE_XWD10] = "XWD10";
			smapFormatToString[FILE_XWD11] = "XWD11";
			smapFormatToString[FILE_FLC] = "FLC";
			smapFormatToString[FILE_KDC] = "KDC";
			smapFormatToString[FILE_DRW] = "DRW";
			smapFormatToString[FILE_PLT] = "PLT";
			smapFormatToString[FILE_TIF_CMP] = "TIF_CMP";
			smapFormatToString[FILE_TIF_JBIG] = "TIF_JBIG";
			smapFormatToString[FILE_TIF_DXF_R13] = "TIF_DXF_R13";
			smapFormatToString[FILE_TIF_DXF_R12] = "TIF_DXF_R12";
			smapFormatToString[FILE_TIF_UNKNOWN] = "TIF_UNKNOWN";
			smapFormatToString[FILE_SGI] = "SGI";
			smapFormatToString[FILE_SGI_RLE] = "SGI_RLE";
			smapFormatToString[FILE_VECTOR_DUMP] = "VECTOR_DUMP";
			smapFormatToString[FILE_DWF] = "DWF";
			smapFormatToString[FILE_RAS_PDF] = "RAS_PDF";
			smapFormatToString[FILE_RAS_PDF_G3_1D] = "RAS_PDF_G3_1D";
			smapFormatToString[FILE_RAS_PDF_G3_2D] = "RAS_PDF_G3_2D";
			smapFormatToString[FILE_RAS_PDF_G4] = "RAS_PDF_G4";
			smapFormatToString[FILE_RAS_PDF_JPEG] = "RAS_PDF_JPEG";
			smapFormatToString[FILE_RAS_PDF_JPEG_422] = "RAS_PDF_JPEG_422";
			smapFormatToString[FILE_RAS_PDF_JPEG_411] = "RAS_PDF_JPEG_411";
			smapFormatToString[FILE_RAS_PDF_LZW] = "RAS_PDF_LZW";
			smapFormatToString[FILE_RAS_PDF_JBIG2] = "RAS_PDF_JBIG2";
			smapFormatToString[FILE_RAW] = "RAW";
			smapFormatToString[FILE_RASTER_DUMP] = "RASTER_DUMP";
			smapFormatToString[FILE_TIF_CUSTOM] = "TIF_CUSTOM";
			smapFormatToString[FILE_RAW_RGB] = "RAW_RGB";
			smapFormatToString[FILE_RAW_RLE4] = "RAW_RLE4";
			smapFormatToString[FILE_RAW_RLE8] = "RAW_RLE8";
			smapFormatToString[FILE_RAW_BITFIELDS] = "RAW_BITFIELDS";
			smapFormatToString[FILE_RAW_PACKBITS] = "RAW_PACKBITS";
			smapFormatToString[FILE_RAW_JPEG] = "RAW_JPEG";
			smapFormatToString[FILE_FAX_G3_1D_NOEOL] = "FAX_G3_1D_NOEOL";
			smapFormatToString[FILE_RAW_LZW] = "RAW_LZW";
			smapFormatToString[FILE_JP2] = "JP2";
			smapFormatToString[FILE_J2K] = "J2K";
			smapFormatToString[FILE_CMW] = "CMW";
			smapFormatToString[FILE_TIF_J2K] = "TIF_J2K";
			smapFormatToString[FILE_TIF_CMW] = "TIF_CMW";
			smapFormatToString[FILE_MRC] = "MRC";
			smapFormatToString[FILE_GERBER] = "GERBER";
			smapFormatToString[FILE_WBMP] = "WBMP";
			smapFormatToString[FILE_JPEG_LAB] = "JPEG_LAB";
			smapFormatToString[FILE_JPEG_LAB_411] = "JPEG_LAB_411";
			smapFormatToString[FILE_JPEG_LAB_422] = "JPEG_LAB_422";
			smapFormatToString[FILE_GEOTIFF] = "GEOTIFF";
			smapFormatToString[FILE_TIF_LEAD1BIT] = "TIF_LEAD1BIT";
			smapFormatToString[FILE_TIF_MRC] = "TIF_MRC";
			smapFormatToString[FILE_TIF_ABC] = "TIF_ABC";
			smapFormatToString[FILE_NAP] = "NAP";
			smapFormatToString[FILE_JPEG_RGB] = "JPEG_RGB";
			smapFormatToString[FILE_JBIG2] = "JBIG2";
			smapFormatToString[FILE_ABIC] = "ABIC";
			smapFormatToString[FILE_TIF_ABIC] = "TIF_ABIC";
			smapFormatToString[FILE_TIF_JBIG2] = "TIF_JBIG2";
			smapFormatToString[FILE_TIF_ZIP] = "TIF_ZIP";
			smapFormatToString[FILE_SVG] = "SVG";
			smapFormatToString[FILE_NITF] = "NITF";
			smapFormatToString[FILE_PTOCA] = "PTOCA";
			smapFormatToString[FILE_SCT] = "SCT";
			smapFormatToString[FILE_PCL] = "PCL";
			smapFormatToString[FILE_AFP] = "AFP";
			smapFormatToString[FILE_SHP] = "SHP";
			smapFormatToString[FILE_SMP] = "SMP";
			smapFormatToString[FILE_SMP_G3_1D] = "SMP_G3_1D";
			smapFormatToString[FILE_SMP_G3_2D] = "SMP_G3_2D";
			smapFormatToString[FILE_SMP_G4] = "SMP_G4";
			smapFormatToString[FILE_VWPG] = "VWPG";
			smapFormatToString[FILE_VWPG1] = "VWPG1";
			smapFormatToString[FILE_CMX] = "CMX";
			smapFormatToString[FILE_TGA_RLE] = "TGA_RLE";
			smapFormatToString[FILE_KDC_120] = "KDC_120";
			smapFormatToString[FILE_KDC_40] = "KDC_40";
			smapFormatToString[FILE_KDC_50] = "KDC_50";
			smapFormatToString[FILE_DCS] = "DCS";
			smapFormatToString[FILE_PSP] = "PSP";
			smapFormatToString[FILE_PSP_RLE] = "PSP_RLE";
			smapFormatToString[FILE_TIFX_JBIG] = "TIFX_JBIG";
			smapFormatToString[FILE_TIFX_JBIG_T43] = "TIFX_JBIG_T43";
			smapFormatToString[FILE_TIFX_JBIG_T43_ITULAB] = "TIFX_JBIG_T43_ITULAB";
			smapFormatToString[FILE_TIFX_JBIG_T43_GS] = "TIFX_JBIG_T43_GS";
			smapFormatToString[FILE_TIFX_FAX_G4] = "TIFX_FAX_G4";
			smapFormatToString[FILE_TIFX_FAX_G3_1D] = "TIFX_FAX_G3_1D";
			smapFormatToString[FILE_TIFX_FAX_G3_2D] = "TIFX_FAX_G3_2D";
			smapFormatToString[FILE_TIFX_JPEG] = "TIFX_JPEG";
			smapFormatToString[FILE_ECW] = "ECW";
			smapFormatToString[FILE_RAS_RLE] = "RAS_RLE";
			smapFormatToString[FILE_SVG_EMBED_IMAGES] = "SVG_EMBED_IMAGES";
			smapFormatToString[FILE_DXF_R13] = "DXF_R13";
			smapFormatToString[FILE_CLP_RLE] = "CLP_RLE";
			smapFormatToString[FILE_DCR] = "DCR";
			smapFormatToString[FILE_DICOM_J2K_GRAY] = "DICOM_J2K_GRAY";
			smapFormatToString[FILE_DICOM_J2K_COLOR] = "DICOM_J2K_COLOR";
			smapFormatToString[FILE_FIT] = "FIT";
			smapFormatToString[FILE_CRW] = "CRW";
			smapFormatToString[FILE_DWF_TEXT_AS_POLYLINE] = "DWF_TEXT_AS_POLYLINE";
			smapFormatToString[FILE_CIN] = "CIN";
			smapFormatToString[FILE_PCL_TEXT_AS_POLYLINE] = "PCL_TEXT_AS_POLYLINE";
			smapFormatToString[FILE_EPSPOSTSCRIPT] = "EPSPOSTSCRIPT";
			smapFormatToString[FILE_INTERGRAPH_CCITT_G4] = "INTERGRAPH_CCITT_G4";
			smapFormatToString[FILE_SFF] = "SFF";
			smapFormatToString[FILE_IFF_ILBM_UNCOMPRESSED] = "IFF_ILBM_UNCOMPRESSED";
			smapFormatToString[FILE_IFF_CAT_UNCOMPRESSED] = "IFF_CAT_UNCOMPRESSED";
			smapFormatToString[FILE_RTF_RASTER] = "RTF_RASTER";
			smapFormatToString[FILE_SID] = "SID";
			smapFormatToString[FILE_WMZ] = "WMZ";
			smapFormatToString[FILE_DJVU] = "DJVU";
			smapFormatToString[FILE_AFPICA_G3_1D] = "AFPICA_G3_1D";
			smapFormatToString[FILE_AFPICA_G3_2D] = "AFPICA_G3_2D";
			smapFormatToString[FILE_AFPICA_G4] = "AFPICA_G4";
			smapFormatToString[FILE_AFPICA_UNCOMPRESSED] = "AFPICA_UNCOMPRESSED";
			smapFormatToString[FILE_AFPICA_IBM_MMR] = "AFPICA_IBM_MMR";
			smapFormatToString[FILE_AFPICA_ABIC] = "AFPICA_ABIC";
			smapFormatToString[FILE_LEAD_MRC] = "LEAD_MRC";
			smapFormatToString[FILE_TIF_LEAD_MRC] = "TIF_LEAD_MRC";
			smapFormatToString[FILE_TXT] = "TXT";
			smapFormatToString[FILE_PDF_LEAD_MRC] = "PDF_LEAD_MRC";
			smapFormatToString[FILE_HDP] = "HDP";
			smapFormatToString[FILE_HDP_GRAY] = "HDP_GRAY";
			smapFormatToString[FILE_HDP_CMYK] = "HDP_CMYK";
			smapFormatToString[FILE_PNG_ICO] = "PNG_ICO";
			smapFormatToString[FILE_XPS] = "XPS";
			smapFormatToString[FILE_JPX] = "JPX";
			smapFormatToString[FILE_XPS_JPEG] = "XPS_JPEG";
			smapFormatToString[FILE_XPS_JPEG_422] = "XPS_JPEG_422";
			smapFormatToString[FILE_XPS_JPEG_411] = "XPS_JPEG_411";
			smapFormatToString[FILE_MNG] = "MNG";
			smapFormatToString[FILE_MNG_GRAY] = "MNG_GRAY";
			smapFormatToString[FILE_MNG_JNG] = "MNG_JNG";
			smapFormatToString[FILE_MNG_JNG_411] = "MNG_JNG_411";
			smapFormatToString[FILE_MNG_JNG_422] = "MNG_JNG_422";
			smapFormatToString[FILE_RAS_PDF_CMYK] = "RAS_PDF_CMYK";
			smapFormatToString[FILE_RAS_PDF_LZW_CMYK] = "RAS_PDF_LZW_CMYK";
			smapFormatToString[FILE_MIF] = "MIF";
			smapFormatToString[FILE_E00] = "E00";
			smapFormatToString[FILE_TDB] = "TDB";
			smapFormatToString[FILE_TDB_VISTA] = "TDB_VISTA";
			smapFormatToString[FILE_SNP] = "SNP";
		}

		map<L_INT, string>::iterator it = smapFormatToString.find(nFormat);
		if (it != smapFormatToString.end())
		{
			return it->second;
		}

		UCLIDException ue("ELI25417", "Unrecognized file format ID!");
		ue.addDebugInfo("Format ID", nFormat);
		throw ue;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25424");
}
//-------------------------------------------------------------------------------------------------
void addFormatDebugInfo(UCLIDException& rUex, L_INT nFormat)
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