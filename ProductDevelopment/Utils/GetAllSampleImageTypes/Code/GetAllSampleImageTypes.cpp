// GetAllSampleImageTypes.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "GetAllSampleImageTypes.h"

#include <cpputil.h>
#include <UCLIDException.h>
#include <UCLIDExceptionDlg.h>
#include <MiscLeadUtils.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

#include <map>
#include <string>
#include <utility>
#include <fstream>
#include <iostream>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

// The one and only application object

CWinApp theApp;

using namespace std;

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
string gstrLIST_FILE = "\\FileInformationList.csv";
string gstrCOUNT_FILE = "\\FileTypeCount.csv";

//--------------------------------------------------------------------------------------------------
// Helper functions
//--------------------------------------------------------------------------------------------------
// PURPOSE: To convert the LeadTools file format enum to a string
string getFormatAsString(L_INT liFormatEnum)
{
	string strFormatString = "";

	switch (liFormatEnum)
	{
	case FILE_UNKNOWN_FORMAT:
		strFormatString = "UNKNOWN_FORMAT";
		break;

	case FILE_PCX:
		strFormatString = "PCX";
		break;

	case FILE_GIF:
		strFormatString = "GIF";
		break;

	case FILE_TIF:
		strFormatString = "TIF";
		break;

	case FILE_TGA:
		strFormatString = "TGA";
		break;

	case FILE_CMP:
		strFormatString = "CMP";
		break;

	case FILE_BMP:
		strFormatString = "BMP";
		break;

	case FILE_BITMAP:
		strFormatString = "BITMAP";
		break;

	case FILE_JPEG:
		strFormatString = "JPEG";
		break;

	case FILE_TIF_JPEG:
		strFormatString = "TIF_JPEG";
		break;

	case FILE_BIN:
		strFormatString = "BIN";
		break;

	case FILE_HANDLE:
		strFormatString = "HANDLE";
		break;

	case FILE_OS2:
		strFormatString = "OS2";
		break;

	case FILE_WMF:
		strFormatString = "WMF";
		break;

	case FILE_EPS:
		strFormatString = "EPS";
		break;

	case FILE_TIFLZW:
		strFormatString = "TIFLZW";
		break;

	case FILE_LEAD:
		strFormatString = "LEAD";
		break;

	case FILE_JPEG_411:
		strFormatString = "JPEG_411";
		break;

	case FILE_TIF_JPEG_411:
		strFormatString = "TIF_JPEG_411";
		break;

	case FILE_JPEG_422:
		strFormatString = "JPEG_422";
		break;

	case FILE_TIF_JPEG_422:
		strFormatString = "TIF_JPEG_422";
		break;

	case FILE_CCITT:
		strFormatString = "CCITT";
		break;

	case FILE_LEAD1BIT:
		strFormatString = "LEAD1BIT";
		break;

	case FILE_CCITT_GROUP3_1DIM:
		strFormatString = "CCITT_GROUP3_1DIM";
		break;

	case FILE_CCITT_GROUP3_2DIM:
		strFormatString = "CCITT_GROUP3_2DIM";
		break;

	case FILE_CCITT_GROUP4:
		strFormatString = "CCITT_GROUP4";
		break;

	case FILE_LEAD1BITA:
		strFormatString = "LEAD1BITA";
		break;

	case FILE_ABC:
		strFormatString = "ABC";
		break;

	case FILE_CALS:
		strFormatString = "CALS";
		break;

	case FILE_MAC:
		strFormatString = "MAC";
		break;

	case FILE_IMG:
		strFormatString = "IMG";
		break;

	case FILE_MSP:
		strFormatString = "MSP";
		break;

	case FILE_WPG:
		strFormatString = "WPG";
		break;

	case FILE_RAS:
		strFormatString = "RAS";
		break;

	case FILE_PCT:
		strFormatString = "PCT";
		break;

	case FILE_PCD:
		strFormatString = "PCD";
		break;

	case FILE_DXF:
		strFormatString = "DXF";
		break;

	case FILE_AVI:
		strFormatString = "AVI";
		break;

	case FILE_WAV:
		strFormatString = "WAV";
		break;

	case FILE_FLI:
		strFormatString = "FLI";
		break;

	case FILE_CGM:
		strFormatString = "CGM";
		break;

	case FILE_EPSTIFF:
		strFormatString = "EPSTIFF";
		break;

	case FILE_EPSWMF:
		strFormatString = "EPSWMF";
		break;

	case FILE_CMPNOLOSS:
		strFormatString = "CMPNOLOSS";
		break;

	case FILE_FAX_G3_1D:
		strFormatString = "FAX_G3_1D";
		break;

	case FILE_FAX_G3_2D:
		strFormatString = "FAX_G3_2D";
		break;

	case FILE_FAX_G4:
		strFormatString = "FAX_G4";
		break;

	case FILE_WFX_G3_1D:
		strFormatString = "WFX_G3_1D";
		break;

	case FILE_WFX_G4:
		strFormatString = "WFX_G4";
		break;

	case FILE_ICA_G3_1D:
		strFormatString = "ICA_G3_1D";
		break;

	case FILE_ICA_G3_2D:
		strFormatString = "ICA_G3_2D";
		break;

	case FILE_ICA_G4:
		strFormatString = "ICA_G4";
		break;

	case FILE_OS2_2:
		strFormatString = "OS2_2";
		break;

	case FILE_PNG:
		strFormatString = "PNG";
		break;

	case FILE_PSD:
		strFormatString = "PSD";
		break;

	case FILE_RAWICA_G3_1D:
		strFormatString = "RAWICA_G3_1D";
		break;

	case FILE_RAWICA_G3_2D:
		strFormatString = "RAWICA_G3_2D";
		break;

	case FILE_RAWICA_G4:
		strFormatString = "RAWICA_G4";
		break;

	case FILE_FPX:
		strFormatString = "FPX";
		break;

	case FILE_FPX_SINGLE_COLOR:
		strFormatString = "FPX_SINGLE_COLOR";
		break;

	case FILE_FPX_JPEG:
		strFormatString = "FPX_JPEG";
		break;

	case FILE_FPX_JPEG_QFACTOR:
		strFormatString = "FPX_JPEG_QFACTOR";
		break;

	case FILE_BMP_RLE:
		strFormatString = "BMP_RLE";
		break;

	case FILE_TIF_CMYK:
		strFormatString = "TIF_CMYK";
		break;

	case FILE_TIFLZW_CMYK:
		strFormatString = "TIFLZW_CMYK";
		break;

	case FILE_TIF_PACKBITS:
		strFormatString = "TIF_PACKBITS";
		break;

	case FILE_TIF_PACKBITS_CMYK:
		strFormatString = "TIF_PACKBITS_CMYK";
		break;

	case FILE_DICOM_GRAY:
		strFormatString = "DICOM_GRAY";
		break;

	case FILE_DICOM_COLOR:
		strFormatString = "DICOM_COLOR";
		break;

	case FILE_WIN_ICO:
		strFormatString = "WIN_ICO";
		break;

	case FILE_WIN_CUR:
		strFormatString = "WIN_CUR";
		break;

	case FILE_TIF_YCC:
		strFormatString = "TIF_YCC";
		break;

	case FILE_TIFLZW_YCC:
		strFormatString = "TIFLZW_YCC";
		break;

	case FILE_TIF_PACKBITS_YCC:
		strFormatString = "TIF_PACKBITS_YCC";
		break;

	case FILE_EXIF:
		strFormatString = "EXIF";
		break;

	case FILE_EXIF_YCC:
		strFormatString = "EXIF_YCC";
		break;

	case FILE_EXIF_JPEG_422:
		strFormatString = "EXIF_JPEG_422";
		break;

	case FILE_AWD:
		strFormatString = "AWD";
		break;

	case FILE_FASTEST:
		strFormatString = "FASTEST";
		break;

	case FILE_EXIF_JPEG_411:
		strFormatString = "EXIF_JPEG_411";
		break;

	case FILE_PBM_ASCII:
		strFormatString = "PBM_ASCII";
		break;

	case FILE_PBM_BINARY:
		strFormatString = "PBM_BINARY";
		break;

	case FILE_PGM_ASCII:
		strFormatString = "PGM_ASCII";
		break;

	case FILE_PGM_BINARY:
		strFormatString = "PGM_BINARY";
		break;

	case FILE_PPM_ASCII:
		strFormatString = "PPM_ASCII";
		break;

	case FILE_PPM_BINARY:
		strFormatString = "PPM_BINARY";
		break;

	case FILE_CUT:
		strFormatString = "CUT";
		break;

	case FILE_XPM:
		strFormatString = "XPM";
		break;

	case FILE_XBM:
		strFormatString = "XBM";
		break;

	case FILE_IFF_ILBM:
		strFormatString = "IFF_ILBM";
		break;

	case FILE_IFF_CAT:
		strFormatString = "IFF_CAT";
		break;

	case FILE_XWD:
		strFormatString = "XWD";
		break;

	case FILE_CLP:
		strFormatString = "CLP";
		break;

	case FILE_JBIG:
		strFormatString = "JBIG";
		break;

	case FILE_EMF:
		strFormatString = "EMF";
		break;

	case FILE_ICA_IBM_MMR:
		strFormatString = "ICA_IBM_MMR";
		break;

	case FILE_RAWICA_IBM_MMR:
		strFormatString = "RAWICA_IBM_MMR";
		break;

	case FILE_ANI:
		strFormatString = "ANI";
		break;

	case FILE_LASERDATA:
		strFormatString = "LASERDATA";
		break;

	case FILE_INTERGRAPH_RLE:
		strFormatString = "INTERGRAPH_RLE";
		break;

	case FILE_INTERGRAPH_VECTOR:
		strFormatString = "INTERGRAPH_VECTOR";
		break;

	case FILE_DWG:
		strFormatString = "DWG";
		break;

	case FILE_DICOM_RLE_GRAY:
		strFormatString = "DICOM_RLE_GRAY";
		break;

	case FILE_DICOM_RLE_COLOR:
		strFormatString = "DICOM_RLE_COLOR";
		break;

	case FILE_DICOM_JPEG_GRAY:
		strFormatString = "DICOM_JPEG_GRAY";
		break;

	case FILE_DICOM_JPEG_COLOR:
		strFormatString = "DICOM_JPEG_COLOR";
		break;

	case FILE_CALS4:
		strFormatString = "CALS4";
		break;

	case FILE_CALS2:
		strFormatString = "CALS2";
		break;

	case FILE_CALS3:
		strFormatString = "CALS3";
		break;

	case FILE_XWD10:
		strFormatString = "XWD10";
		break;

	case FILE_XWD11:
		strFormatString = "XWD11";
		break;

	case FILE_FLC:
		strFormatString = "FLC";
		break;

	case FILE_KDC:
		strFormatString = "KDC";
		break;

	case FILE_DRW:
		strFormatString = "DRW";
		break;

	case FILE_PLT:
		strFormatString = "PLT";
		break;

	case FILE_TIF_CMP:
		strFormatString = "TIF_CMP";
		break;

	case FILE_TIF_JBIG:
		strFormatString = "TIF_JBIG";
		break;

	case FILE_TIF_DXF_R13:
		strFormatString = "TIF_DXF_R13";
		break;

	case FILE_TIF_DXF_R12:
		strFormatString = "TIF_DXF_R12";
		break;

	case FILE_TIF_UNKNOWN:
		strFormatString = "TIF_UNKNOWN";
		break;

	case FILE_SGI:
		strFormatString = "SGI";
		break;

	case FILE_SGI_RLE:
		strFormatString = "SGI_RLE";
		break;

	case FILE_VECTOR_DUMP:
		strFormatString = "VECTOR_DUMP";
		break;

	case FILE_DWF:
		strFormatString = "DWF";
		break;

	case FILE_RAS_PDF:
		strFormatString = "RAS_PDF";
		break;

	case FILE_RAS_PDF_G3_1D:
		strFormatString = "RAS_PDF_G3_1D";
		break;

	case FILE_RAS_PDF_G3_2D:
		strFormatString = "RAS_PDF_G3_2D";
		break;

	case FILE_RAS_PDF_G4:
		strFormatString = "RAS_PDF_G4";
		break;

	case FILE_RAS_PDF_JPEG:
		strFormatString = "RAS_PDF_JPEG";
		break;

	case FILE_RAS_PDF_JPEG_422:
		strFormatString = "RAS_PDF_JPEG_422";
		break;

	case FILE_RAS_PDF_JPEG_411:
		strFormatString = "RAS_PDF_JPEG_411";
		break;

	case FILE_RAS_PDF_LZW:
		strFormatString = "RAS_PDF_LZW";
		break;

	case FILE_RAS_PDF_JBIG2:
		strFormatString = "RAS_PDF_JBIG2";
		break;

	case FILE_RAW:
		strFormatString = "RAW";
		break;

	case FILE_RASTER_DUMP:
		strFormatString = "RASTER_DUMP";
		break;

	case FILE_TIF_CUSTOM:
		strFormatString = "TIF_CUSTOM";
		break;

	case FILE_RAW_RGB:
		strFormatString = "RAW_RGB";
		break;

	case FILE_RAW_RLE4:
		strFormatString = "RAW_RLE4";
		break;

	case FILE_RAW_RLE8:
		strFormatString = "RAW_RLE8";
		break;

	case FILE_RAW_BITFIELDS:
		strFormatString = "RAW_BITFIELDS";
		break;

	case FILE_RAW_PACKBITS:
		strFormatString = "RAW_PACKBITS";
		break;

	case FILE_RAW_JPEG:
		strFormatString = "RAW_JPEG";
		break;

	case FILE_FAX_G3_1D_NOEOL:
		strFormatString = "FAX_G3_1D_NOEOL";
		break;

	case FILE_RAW_LZW:
		strFormatString = "RAW_LZW";
		break;

	case FILE_JP2:
		strFormatString = "JP2";
		break;

	case FILE_J2K:
		strFormatString = "J2K";
		break;

	case FILE_CMW:
		strFormatString = "CMW";
		break;

	case FILE_TIF_J2K:
		strFormatString = "TIF_J2K";
		break;

	case FILE_TIF_CMW:
		strFormatString = "TIF_CMW";
		break;

	case FILE_MRC:
		strFormatString = "MRC";
		break;

	case FILE_GERBER:
		strFormatString = "GERBER";
		break;

	case FILE_WBMP:
		strFormatString = "WBMP";
		break;

	case FILE_JPEG_LAB:
		strFormatString = "JPEG_LAB";
		break;

	case FILE_JPEG_LAB_411:
		strFormatString = "JPEG_LAB_411";
		break;

	case FILE_JPEG_LAB_422:
		strFormatString = "JPEG_LAB_422";
		break;

	case FILE_GEOTIFF:
		strFormatString = "GEOTIFF";
		break;

	case FILE_TIF_LEAD1BIT:
		strFormatString = "TIF_LEAD1BIT";
		break;

	case FILE_TIF_MRC:
		strFormatString = "TIF_MRC";
		break;

	case FILE_TIF_ABC:
		strFormatString = "TIF_ABC";
		break;

	case FILE_NAP:
		strFormatString = "NAP";
		break;

	case FILE_JPEG_RGB:
		strFormatString = "JPEG_RGB";
		break;

	case FILE_JBIG2:
		strFormatString = "JBIG2";
		break;

	case FILE_ICA_ABIC:
		strFormatString = "ICA_ABIC";
		break;

	case FILE_ABIC:
		strFormatString = "ABIC";
		break;

	case FILE_TIF_ABIC:
		strFormatString = "TIF_ABIC";
		break;

	case FILE_TIF_JBIG2:
		strFormatString = "TIF_JBIG2";
		break;

	case FILE_TIF_ZIP:
		strFormatString = "TIF_ZIP";
		break;

	case FILE_AMI_PRO_20:
		strFormatString = "AMI_PRO_20";
		break;

	case FILE_AMI_PRO_30:
		strFormatString = "AMI_PRO_30";
		break;

	case FILE_ASCII_SMART:
		strFormatString = "ASCII_SMART";
		break;

	case FILE_ASCII_STANDARD:
		strFormatString = "ASCII_STANDARD";
		break;

	case FILE_ASCII_STANDARD_DOS:
		strFormatString = "ASCII_STANDARD_DOS";
		break;

	case FILE_ASCII_STRIPPED:
		strFormatString = "ASCII_STRIPPED";
		break;

	case FILE_DBASE_IV_10:
		strFormatString = "DBASE_IV_10";
		break;

	case FILE_DCA_RFT:
		strFormatString = "DCA_RFT";
		break;

	case FILE_DCA_RFT_DW_5:
		strFormatString = "DCA_RFT_DW_5";
		break;

	case FILE_EXCEL_MAC:
		strFormatString = "EXCEL_MAC";
		break;

	case FILE_EXCEL_30:
		strFormatString = "EXCEL_30";
		break;

	case FILE_EXCEL_40:
		strFormatString = "EXCEL_40";
		break;

	case FILE_EXCEL_50:
		strFormatString = "EXCEL_50";
		break;

	case FILE_EXCEL_OFFICE97:
		strFormatString = "EXCEL_OFFICE97";
		break;

	case FILE_FRAMEMAKER:
		strFormatString = "FRAMEMAKER";
		break;

	case FILE_HTML_20:
		strFormatString = "HTML_20";
		break;

	case FILE_HTML_EDITOR_20:
		strFormatString = "HTML_EDITOR_20";
		break;

	case FILE_HTML_NETSCAPE_20:
		strFormatString = "HTML_NETSCAPE_20";
		break;

	case FILE_INTERLEAF:
		strFormatString = "INTERLEAF";
		break;

	case FILE_LOTUS123:
		strFormatString = "LOTUS123";
		break;

	case FILE_LOTUS_WORD_PRO:
		strFormatString = "LOTUS_WORD_PRO";
		break;

	case FILE_MULTIMATE_ADV_II:
		strFormatString = "MULTIMATE_ADV_II";
		break;

	case FILE_POSTSCRIPT:
		strFormatString = "POSTSCRIPT";
		break;

	case FILE_PROFESSIONAL_WRITE_20:
		strFormatString = "PROFESSIONAL_WRITE_20";
		break;

	case FILE_PROFESSIONAL_WRITE_22:
		strFormatString = "PROFESSIONAL_WRITE_22";
		break;

	case FILE_QUATTRA_PRO:
		strFormatString = "QUATTRA_PRO";
		break;

	case FILE_RTF:
		strFormatString = "RTF";
		break;

	case FILE_RTF_MAC:
		strFormatString = "RTF_MAC";
		break;

	case FILE_RTF_WORD_60:
		strFormatString = "RTF_WORD_60";
		break;

	case FILE_WINDOWS_WRITE:
		strFormatString = "WINDOWS_WRITE";
		break;

	case FILE_WORD_WINDOWS_2X:
		strFormatString = "WORD_WINDOWS_2X";
		break;

	case FILE_WORD_WINDOWS_60:
		strFormatString = "WORD_WINDOWS_60";
		break;

	case FILE_WORD_OFFICE97:
		strFormatString = "WORD_OFFICE97";
		break;

	case FILE_WORDPERFECT_DOS_42:
		strFormatString = "WORDPERFECT_DOS_42";
		break;

	case FILE_WORDPERFECT_WINDOWS:
		strFormatString = "WORDPERFECT_WINDOWS";
		break;

	case FILE_WORDPERFECT_WINDOWS_60:
		strFormatString = "WORDPERFECT_WINDOWS_60";
		break;

	case FILE_WORDPERFECT_WINDOWS_61:
		strFormatString = "WORDPERFECT_WINDOWS_61";
		break;

	case FILE_WORDPERFECT_WINDOWS_7X:
		strFormatString = "WORDPERFECT_WINDOWS_7X";
		break;

	case FILE_WORDSTAR_WINDOWS_1X:
		strFormatString = "WORDSTAR_WINDOWS_1X";
		break;

	case FILE_WORKS:
		strFormatString = "WORKS";
		break;

	case FILE_XDOC:
		strFormatString = "XDOC";
		break;

	case FILE_MOV:
		strFormatString = "MOV";
		break;

	case FILE_MIDI:
		strFormatString = "MIDI";
		break;

	case FILE_MPEG1:
		strFormatString = "MPEG1";
		break;

	case FILE_AU:
		strFormatString = "AU";
		break;

	case FILE_AIFF:
		strFormatString = "AIFF";
		break;

	case FILE_MPEG2:
		strFormatString = "MPEG2";
		break;

	case FILE_SVG:
		strFormatString = "SVG";
		break;

	case FILE_NITF:
		strFormatString = "NITF";
		break;

	case FILE_PTOCA:
		strFormatString = "PTOCA";
		break;

	case FILE_SCT:
		strFormatString = "SCT";
		break;

	case FILE_PCL:
		strFormatString = "PCL";
		break;

	case FILE_AFP:
		strFormatString = "AFP";
		break;

	case FILE_ICA_UNCOMPRESSED:
		strFormatString = "ICA_UNCOMPRESSED";
		break;

	case FILE_RAWICA_UNCOMPRESSED:
		strFormatString = "RAWICA_UNCOMPRESSED";
		break;

	case FILE_SHP:
		strFormatString = "SHP";
		break;

	case FILE_SMP:
		strFormatString = "SMP";
		break;

	case FILE_SMP_G3_1D:
		strFormatString = "SMP_G3_1D";
		break;

	case FILE_SMP_G3_2D:
		strFormatString = "SMP_G3_2D";
		break;

	case FILE_SMP_G4:
		strFormatString = "SMP_G4";
		break;

	case FILE_VWPG:
		strFormatString = "VWPG";
		break;

	case FILE_VWPG1:
		strFormatString = "VWPG1";
		break;

	case FILE_CMX:
		strFormatString = "CMX";
		break;

	case FILE_TGA_RLE:
		strFormatString = "TGA_RLE";
		break;

	case FILE_KDC_120:
		strFormatString = "KDC_120";
		break;

	case FILE_KDC_40:
		strFormatString = "KDC_40";
		break;

	case FILE_KDC_50:
		strFormatString = "KDC_50";
		break;

	case FILE_DCS:
		strFormatString = "DCS";
		break;

	case FILE_PSP:
		strFormatString = "PSP";
		break;

	case FILE_PSP_RLE:
		strFormatString = "PSP_RLE";
		break;

	case FILE_TIFX_JBIG:
		strFormatString = "TIFX_JBIG";
		break;

	case FILE_TIFX_JBIG_T43:
		strFormatString = "TIFX_JBIG_T43";
		break;

	case FILE_TIFX_JBIG_T43_ITULAB:
		strFormatString = "TIFX_JBIG_T43_ITULAB";
		break;

	case FILE_TIFX_JBIG_T43_GS:
		strFormatString = "TIFX_JBIG_T43_GS";
		break;

	case FILE_TIFX_FAX_G4:
		strFormatString = "TIFX_FAX_G4";
		break;

	case FILE_TIFX_FAX_G3_1D:
		strFormatString = "TIFX_FAX_G3_1D";
		break;

	case FILE_TIFX_FAX_G3_2D:
		strFormatString = "TIFX_FAX_G3_2D";
		break;

	case FILE_TIFX_JPEG:
		strFormatString = "TIFX_JPEG";
		break;

	case FILE_ECW:
		strFormatString = "ECW";
		break;

	case FILE_RAS_RLE:
		strFormatString = "RAS_RLE";
		break;

	case FILE_SVG_EMBED_IMAGES:
		strFormatString = "SVG_EMBED_IMAGES";
		break;

	case FILE_DXF_R13:
		strFormatString = "DXF_R13";
		break;

	case FILE_CLP_RLE:
		strFormatString = "CLP_RLE";
		break;

	case FILE_DCR:
		strFormatString = "DCR";
		break;

	case FILE_DICOM_J2K_GRAY:
		strFormatString = "DICOM_J2K_GRAY";
		break;

	case FILE_DICOM_J2K_COLOR:
		strFormatString = "DICOM_J2K_COLOR";
		break;

	case FILE_FIT:
		strFormatString = "FIT";
		break;

	case FILE_CRW:
		strFormatString = "CRW";
		break;

	case FILE_DWF_TEXT_AS_POLYLINE:
		strFormatString = "DWF_TEXT_AS_POLYLINE";
		break;

	case FILE_CIN:
		strFormatString = "CIN";
		break;

	case FILE_PCL_TEXT_AS_POLYLINE:
		strFormatString = "PCL_TEXT_AS_POLYLINE";
		break;

	case FILE_EPSPOSTSCRIPT:
		strFormatString = "EPSPOSTSCRIPT";
		break;

	case FILE_INTERGRAPH_CCITT_G4:
		strFormatString = "INTERGRAPH_CCITT_G4";
		break;

	case FILE_SFF:
		strFormatString = "SFF";
		break;

	case FILE_IFF_ILBM_UNCOMPRESSED:
		strFormatString = "IFF_ILBM_UNCOMPRESSED";
		break;

	case FILE_IFF_CAT_UNCOMPRESSED:
		strFormatString = "IFF_CAT_UNCOMPRESSED";
		break;

	case FILE_RTF_RASTER:
		strFormatString = "RTF_RASTER";
		break;

	case FILE_SID:
		strFormatString = "SID";
		break;

	case FILE_WMZ:
		strFormatString = "WMZ";
		break;

	case FILE_DJVU:
		strFormatString = "DJVU";
		break;

	case FILE_AFPICA_G3_1D:
		strFormatString = "AFPICA_G3_1D";
		break;

	case FILE_AFPICA_G3_2D:
		strFormatString = "AFPICA_G3_2D";
		break;

	case FILE_AFPICA_G4:
		strFormatString = "AFPICA_G4";
		break;

	case FILE_AFPICA_UNCOMPRESSED:
		strFormatString = "AFPICA_UNCOMPRESSED";
		break;

	case FILE_AFPICA_IBM_MMR:
		strFormatString = "AFPICA_IBM_MMR";
		break;

	case FILE_LEAD_MRC:
		strFormatString = "LEAD_MRC";
		break;

	case FILE_TIF_LEAD_MRC:
		strFormatString = "TIF_LEAD_MRC";
		break;

	case FILE_TXT:
		strFormatString = "TXT";
		break;

	case FILE_PDF_LEAD_MRC:
		strFormatString = "PDF_LEAD_MRC";
		break;

	case FILE_HDP:
		strFormatString = "HDP";
		break;

	case FILE_HDP_GRAY:
		strFormatString = "HDP_GRAY";
		break;

	case FILE_HDP_CMYK:
		strFormatString = "HDP_CMYK";
		break;

	case FILE_PNG_ICO:
		strFormatString = "PNG_ICO";
		break;

	case FILE_XPS:
		strFormatString = "XPS";
		break;

	case FILE_JPX:
		strFormatString = "JPX";
		break;

	case FILE_XPS_JPEG:
		strFormatString = "XPS_JPEG";
		break;

	case FILE_XPS_JPEG_422:
		strFormatString = "XPS_JPEG_422";
		break;

	case FILE_XPS_JPEG_411:
		strFormatString = "XPS_JPEG_411";
		break;

	case FILE_MNG:
		strFormatString = "MNG";
		break;

	case FILE_MNG_GRAY:
		strFormatString = "MNG_GRAY";
		break;

	case FILE_MNG_JNG:
		strFormatString = "MNG_JNG";
		break;

	case FILE_MNG_JNG_411:
		strFormatString = "MNG_JNG_411";
		break;

	case FILE_MNG_JNG_422:
		strFormatString = "MNG_JNG_422";
		break;

	case FILE_RAS_PDF_CMYK:
		strFormatString = "RAS_PDF_CMYK";
		break;

	case FILE_RAS_PDF_LZW_CMYK:
		strFormatString = "RAS_PDF_LZW_CMYK";
		break;

	default:
		strFormatString = "Unrecognized Format";
		break;
	}

	return strFormatString;
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To display the usage for this program
void displayUsage()
{
	string strMessage = "GetAllSampleImageTypes.exe <base directory> <output directory> [/?]\n";
	strMessage += "Usage:\n";
	strMessage += "-------------\n";
	strMessage += "Required Arguments:\n";
	strMessage += "\t<base directory>: the directory to start the search from\n";
	strMessage += "\t<output directory>: the directory to copy the sample files to\n";
	strMessage += "Optional Argument:\n";
	strMessage += "\t/?: display this help\n";

	MessageBox(NULL, strMessage.c_str(), "Usage", MB_OK | MB_ICONWARNING);
}
//--------------------------------------------------------------------------------------------------
void getImagesInOutputDirectory(string strOut, multimap<L_INT, L_INT>& rmmapFileTypes,
								map<L_INT, map<L_INT, long>>& rmapFileTypeCount)
{
	try
	{
		// make sure strOut ends in a slash
		if (strOut[strOut.length() - 1] != '\\' || strOut[strOut.length() - 1] != '/')
		{
			strOut += '\\';
		}

		WIN32_FIND_DATA fileInfo;
		HANDLE fileHandle;
		BOOL bFindFileResult = TRUE;

		fileHandle = FindFirstFile((strOut + "*.*").c_str(), &fileInfo);
		if (fileHandle == INVALID_HANDLE_VALUE)
		{
			return;
		}

		do
		{
			string strFileName(fileInfo.cFileName);
			if (strFileName != "." && strFileName != "..")
			{
				// since this is the destination folder, do not search sub-directories
				if (!((fileInfo.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) > 0))
				{
					strFileName = strOut + strFileName;

					try
					{
						string strExt = getExtensionFromFullPath(strFileName);

						if (isImageFileExtension(strExt) || isNumericExtension(strExt))
						{
							FILEINFO flInfo = GetLeadToolsSizedStruct<FILEINFO>(0);

							// get the file info for the image
							throwExceptionIfNotSuccess(L_FileInfo(_bstr_t(strFileName.c_str()), 
								&flInfo, sizeof(flInfo), 0, NULL), "ELI20327", 
								"Failed to load file info", strFileName);

							// add the information to the multimap
							rmmapFileTypes.insert(
								pair<L_INT, L_INT>(flInfo.Format, flInfo.BitsPerPixel));

							// update the file type count
							rmapFileTypeCount[flInfo.Format][flInfo.BitsPerPixel]++;
						}
					}
					catch(UCLIDException& ue)
					{
						ue.log();
					}
				}
			}

			bFindFileResult = FindNextFile(fileHandle, &fileInfo);
		}
		while(bFindFileResult);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20328");
}
//--------------------------------------------------------------------------------------------------
void getUniqueImageFilesAndCopyToOutputDirectory(string strRoot, const string& strOut,
												 multimap<L_INT, L_INT>& rmmapFileTypes,
												 map<L_INT, map<L_INT, long>>& rmapFileTypeCount)
{
	try
	{
		// make sure root directory ends in a slash
		if (strRoot[strRoot.length() - 1] != '\\' || strRoot[strRoot.length() - 1] != '/')
		{
			strRoot += '\\';
		}

		WIN32_FIND_DATA fileInfo;
		HANDLE fileHandle;
		BOOL bFindFileResult = TRUE;

		fileHandle = FindFirstFile((strRoot + "*.*").c_str(), &fileInfo);
		if (fileHandle == INVALID_HANDLE_VALUE)
		{
			return;
		}

		// go through all the files/dirs in the current directory
		do
		{
			string strFileName(fileInfo.cFileName);
			if (strFileName != "." && strFileName != "..")
			{
				// if it is a directory, then recursively find files
				if ((fileInfo.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) > 0)
				{
					// do not check the directories marked "badimages" as this will cause
					// leadtools to crash 
					if (strFileName == "BadImages")
					{
						return;
					}

					getUniqueImageFilesAndCopyToOutputDirectory(strRoot+strFileName, strOut,
						rmmapFileTypes, rmapFileTypeCount);
				}
				// its a file, check its image type
				else
				{
					// add path information to file name
					strFileName = strRoot + strFileName;

					try
					{
						// check if it is an image file
						string strExt = getExtensionFromFullPath(strFileName);

						if (isImageFileExtension(strExt) || isNumericExtension(strExt))
						{
							FILEINFO flInfo = GetLeadToolsSizedStruct<FILEINFO>(0);

							// get the file info for the image
							throwExceptionIfNotSuccess(L_FileInfo(_bstr_t(strFileName.c_str()), 
								&flInfo, sizeof(flInfo), 0, NULL), "ELI20204", 
								"Failed to load file info", strFileName);

							// update the file type counts
							rmapFileTypeCount[flInfo.Format][flInfo.BitsPerPixel]++;

							// get the range of values for this file format
							pair<multimap<L_INT, L_INT>::iterator, multimap<L_INT, L_INT>::iterator>
								prIts = rmmapFileTypes.equal_range(flInfo.Format);
							
							// flag for whether we have copied a file with this format and bits
							// per pixel yet
							bool bExists = false;

							// loop through all entries for this format
							for (multimap<L_INT, L_INT>::iterator it = prIts.first;
								it != prIts.second; it++)
							{
								// if we found an entry with the same bits per pixel, set
								// the existence flag and break from the loop
								if ((*it).second == flInfo.BitsPerPixel)
								{
									bExists = true;
									break;
								}
							}
							
							// if this is a new format, copy it to a new path
							if (!bExists)
							{
								// add the information to the multimap
								rmmapFileTypes.insert(
									pair<L_INT, L_INT>(flInfo.Format, flInfo.BitsPerPixel));

								// add the files format, bits per pixel, and name to the file list
								string strListFile = strOut + gstrLIST_FILE;
								ofstream fOut(strListFile.c_str(), ios::app);
								fOut << getFormatAsString(flInfo.Format) << ",";
								fOut << asString(flInfo.BitsPerPixel) << " bpp,";
								fOut << strFileName << endl;
								fOut.close();

								// copy the file to the output location
								copyFileToNewPath(strFileName, strOut);
							}
						}
					}
					CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20412");
				}
			}

			bFindFileResult = FindNextFile(fileHandle, &fileInfo);
		}
		while (bFindFileResult);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20329");
}
//--------------------------------------------------------------------------------------------------
void validateLicense()
{
	VALIDATE_LICENSE(gnEXTRACT_CORE_OBJECTS, "ELI20205", "GetAllSampleImageTypes");
}

//--------------------------------------------------------------------------------------------------
// Main application
//--------------------------------------------------------------------------------------------------
int _tmain(int argc, TCHAR* argv[], TCHAR* envp[])
{
	int nRetCode = 0;

	try
	{
		try
		{
			// initialize MFC and print and error on failure
			if (!AfxWinInit(::GetModuleHandle(NULL), NULL, ::GetCommandLine(), 0))
			{
				// TODO: change error code to suit your needs
				_tprintf(_T("Fatal Error: MFC initialization failed\n"));
				nRetCode = 1;
			}
			else
			{
				// Setup exception handling
				UCLIDExceptionDlg exceptionDlg;
				UCLIDException::setExceptionHandler( &exceptionDlg );

				if (argc != 3)
				{
					displayUsage();

					nRetCode = 1;
				}
				else
				{
					// get the input directoy
					string strInputDirectory(argv[1]);
					
					// check if it is an absolute or relative path
					if (!isAbsolutePath(strInputDirectory))
					{
						// build the absolute path from the relative path
						string strOut = getCurrentDirectory();
						strOut += "\\" + strInputDirectory;
						simplifyPathName(strOut);
						strInputDirectory = strOut;
					}

					// ensure the input directory exists
					if (!isValidFolder(strInputDirectory))
					{
						UCLIDException ue("ELI20206", "Invalid input directory!");
						ue.addDebugInfo("Input directory", strInputDirectory);
						throw ue;
					}

					// get the output directory
					string strOutputDirectory(argv[2]);

					// check if it is an absolute or relative path
					if (!isAbsolutePath(strOutputDirectory))
					{
						// build the absolute path from the relative path
						string strOut = getCurrentDirectory();
						strOut += "\\" + strOutputDirectory;
						simplifyPathName(strOut);
						strOutputDirectory = strOut;
					}

					// multimap to hold image format and image bits per pixel
					multimap<L_INT, L_INT> mmapFileTypesAndBitRates;

					// map to hold the file type count
					map<L_INT, map<L_INT, long>> mapFileTypeCount;

					// init license management
					LicenseManagement::sGetInstance().loadLicenseFilesFromFolder(
						LICENSE_MGMT_PASSWORD);

					// check license
					validateLicense();

					// if the output directory doesn't exist, create it
					if (!isValidFolder(strOutputDirectory))
					{
						createDirectory(strOutputDirectory);
					}
					else
					{
						// check the output directory for images and add them to the list
						// first
						getImagesInOutputDirectory(strOutputDirectory, mmapFileTypesAndBitRates,
							mapFileTypeCount);
					}
						
					// get the file names and copy to the output directory
					getUniqueImageFilesAndCopyToOutputDirectory(strInputDirectory, 
						strOutputDirectory, mmapFileTypesAndBitRates, mapFileTypeCount);

					// open the file type count file for writing
					string strListFile = strOutputDirectory + gstrCOUNT_FILE;
					ofstream fOut(strListFile.c_str(), ios::out);

					// loop through the map of file types
					for (map<L_INT, map<L_INT, long>>::iterator it1 = mapFileTypeCount.begin();
						it1 != mapFileTypeCount.end(); it1++)
					{
						// for each file type, loop through the bits per pixel
						for (map<L_INT, long>::iterator it2 = it1->second.begin();
							it2 != it1->second.end(); it2++)
						{
							// write the file type, the bpp, and the count
							fOut << getFormatAsString(it1->first) << "," << it2->first; 
							fOut << "bpp," << it2->second << endl;
						}
					}
					fOut.close();
					waitForFileAccess(strListFile, giMODE_READ_ONLY);

					cout << "Finished!\n";
				}
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20207");
	}
	catch(UCLIDException& ue)
	{
		ue.display();
		nRetCode = 1;
	}

	return nRetCode;
}
//--------------------------------------------------------------------------------------------------