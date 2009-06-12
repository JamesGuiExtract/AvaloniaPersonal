@cd /d %1

@SET PDROOT=%LOCAL_VSS_ROOT%\Engineering\ProductDevelopment

@copy %PDROOT%\InputFunnel\Misc\InputFunnelProgIds.txt+%PDROOT%\IcoMapCore\Misc\IcoMapCoreProgIds.txt+%PDROOT%\IcoMapESRI\Misc\IcoMapESRIProgIds.txt+%PDROOT%\IFAttributeEditing\Misc\IFAttributeEditingProgIds.txt+%PDROOT%\SwipeIt\ArcGIS\Misc\ArcGISSwipeItProgIds.txt+%PDROOT%\AttributeFinder\Misc\AFCoreProgIds.txt c:\temp\ProgIds.txt
@DisplayCOMDllLocation C:\temp\ProgIds.txt > c:\temp\a.txt

@notepad c:\temp\a.txt
@del c:\temp\a.txt
@exit
