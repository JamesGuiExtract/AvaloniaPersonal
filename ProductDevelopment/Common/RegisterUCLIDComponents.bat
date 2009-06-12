@cd /d %1

SET PDROOT=%LOCAL_VSS_ROOT%\Engineering\ProductDevelopment

regsvr32 /s UCLIDHighlightWindow.dll
regsvr32 /s UCLIDArcMapToolbar.dll
regsvr32 /s ArcGISUtils.dll
regsvr32 /s COMLM.dll
regsvr32 /s UCLIDMCRTextViewer.ocx
regsvr32 /s HighlightedTextIR.dll
regsvr32 /s IFCore.dll
regsvr32 /s InputFinders.dll
regsvr32 /s LandRecordsIV.dll
regsvr32 /s GeneralIV.dll
regsvr32 /s LineTextCorrectors.dll
regsvr32 /s LineTextEvaluators.dll
regsvr32 /s ParagraphTextCorrectors.dll
regsvr32 /s ParagraphTextHandlers.dll
regsvr32 /s ImageEdit.Ocx
regsvr32 /s UCLIDGenericDisplay2.ocx
regsvr32 /s SpotRecognitionIR.dll
regsvr32 /s UCLIDCurveParameter.dll
regsvr32 /s UCLIDDistanceConverter.dll
regsvr32 /s UCLIDExceptionMgmt.dll
regsvr32 /s UCLIDFeatureMgmt.dll
regsvr32 /s UCLIDCOMUtils.dll
regsvr32 /s UCLIDTestingFramework.dll
regsvr32 /s UCLIDTestingFrameworkCore.dll
regsvr32 /s CPenIR.dll
regsvr32 /s UCLIDRasterAndOCRMgmt.dll
regsvr32 /s SSOCR.Dll
regsvr32 /s AMLineTextImageCleaner.dll
regsvr32 /s SubImageHandlers.dll
regsvr32 /s UCLIDMeasurements.dll
regsvr32 /s UCLIDFilters.dll
regsvr32 /s SpeechIRs.dll

regsvr32 /s IcoMapInterfaces.dll
regsvr32 /s IcoMapApp.dll
regsvr32 /s ArcGISIcoMap.dll

regsvr32 /s IEVBScriptParser.dll
regsvr32 /s InputContexts.dll
regsvr32 /s InputTargetFramework.dll
regsvr32 /s RegExprIV.dll
regsvr32 /s SwipeItCore.dll
regsvr32 /s SwipeItForArcGIS.dll

regsvr32 /s IFAttributeEditingCore.dll
regsvr32 /s ArcGISAttributeEditing.dll

regsvr32 /s UCLIDAFCore.dll
regsvr32 /s UCLIDAFValueFinders.dll
regsvr32 /s UCLIDAFValueModifiers.dll
regsvr32 /s UCLIDAFOutputHandlers.dll
regsvr32 /s UCLIDAFSplitters.dll
regsvr32 /s UCLIDAFPreProcessors.dll
regsvr32 /s CountyCustomComponents.dll
regsvr32 /s UCLIDAFUtils.dll

regsvr32 /s AFCoreTest.dll
regsvr32 /s SpotRecIRAutoTest.dll
regsvr32 /s HighlightedTextIRAutoTest.dll
regsvr32 /s CPenIRAutoTest.dll
regsvr32 /s AFSplittersTest.dll
regsvr32 /s AFUtilsTest.dll
regsvr32 /s CountyTester.dll
regsvr32 /s SpatialStringAutomatedTest.dll
regsvr32 /s StringPatternMatcherAutoTest.dll

CALL %LOCAL_VSS_ROOT%\Engineering\ProductDevelopment\Common\ShowRegisteredUCLIDComponents.bat
