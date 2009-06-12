Attribute VB_Name = "modMain"
Option Explicit
Public Declare Sub Sleep Lib "kernel32" (ByVal dwMilliseconds As Long)
Public Sub Main()
    frmProgress.Show False
    
    Randomize
    
    'test each of the co-classes
    testCartographicPoint
    testLineSegment
    testParameterTypeValuePair
    testArcSegment
    testEnumSegment
    testPart
    testEnumPart
    testFeature
    testCommaDelimitedFeatureAttributeDataInterpreter
    
done:
    MsgBox "Test complete."
    
    Unload frmProgress
    End
End Sub

Public Sub HandleError()
    Dim ex As ICOMUCLIDException
    Set ex = New COMUCLIDException
    MsgBox Err.Description
    ex.createFromString "ELI02699", Err.Description
    ex.display
End Sub
