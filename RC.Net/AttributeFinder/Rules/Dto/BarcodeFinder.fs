﻿namespace Extract.AttributeFinder.Rules.Dto

type BarType =
| BAR_EAN = 0
| BAR_EAN_SUPPL = 1
| BAR_UPC_A = 2
| BAR_UPC_E = 3
| BAR_ITF = 4
| BAR_ITF_CDT = 5
| BAR_C39 = 6
| BAR_C39_CDT = 7
| BAR_C39_SST = 8
| BAR_C39_EXT = 9
| BAR_C128 = 10
| BAR_C128_CDT = 11
| BAR_CB = 12
| BAR_CB_NO_SST = 13
| BAR_POSTNET = 14
| BAR_A2of5 = 15
| BAR_UCC128 = 16
| BAR_2of5 = 17
| BAR_C93 = 18
| BAR_PATCH = 19
| BAR_PDF417 = 20
| BAR_PLANET = 21
| BAR_C32 = 22
| BAR_DMATRIX = 23
| BAR_C39_NSS = 24
| BAR_4STATE = 25
| BAR_QR = 26
| BAR_MAT25 = 27
| BAR_4STATE_DK1 = 28
| BAR_AZTEC = 29
| BAR_CODE11 = 30
| BAR_ITAPOST25 = 31
| BAR_MSI = 32
| BAR_BOOKLAND = 33
| BAR_ITF14 = 34
| BAR_EAN14 = 35
| BAR_SSCC18 = 36
| BAR_DATABAR_LTD = 37
| BAR_DATABAR_EXP = 38
| BAR_4STATE_USPS = 39
| BAR_4STATE_AUSPOST = 40
| BAR_SIZE = 41


type BarcodeFinder = {
  Types: BarType list
  InheritOCRParameters: bool
}