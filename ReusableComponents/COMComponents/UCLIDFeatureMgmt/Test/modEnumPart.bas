Attribute VB_Name = "modEnumPart"
Option Explicit

Public Sub testEnumPart()
    
    ' create two random parts
    Dim pPart1 As IPart
    Dim pPart1Segments As IIUnknownVector
    Dim pPart1StartPoint As ICartographicPoint
    Set pPart1 = createRandomPart(pPart1Segments, pPart1StartPoint)
    
    Dim pPart2 As IPart
    Dim pPart2Segments As IIUnknownVector
    Dim pPart2StartPoint As ICartographicPoint
    Set pPart2 = createRandomPart(pPart2Segments, pPart2StartPoint)
    
    ' create a new enumPart object
    Dim pEnumPart As IEnumPart
    Set pEnumPart = New EnumPart
    
    ' get the modifier interface
    Dim pEnumPartModifier As IEnumPartModifier
    Set pEnumPartModifier = pEnumPart 'QI
    
    ' add the parts to the enumpart
    pEnumPartModifier.addPart pPart1
    pEnumPartModifier.addPart pPart2
    ' reset the enumerator and ensure that the first part is the
    ' same as pPart1
    pEnumPart.Reset
    Dim pRetrievedPart As IPart
    Set pRetrievedPart = pEnumPart.Next
    Dim strTestName As String
    strTestName = "EnumPart.next()"
    If (pRetrievedPart.valueIsEqualTo(pPart1)) Then
        frmProgress.recordPassedTest strTestName
    Else
        frmProgress.recordFailedTest strTestName, "ELI01573"
    End If
    
    ' test to make sure that the second part is the same as pPart2
    strTestName = "EnumPart.next() - 2"
    Set pRetrievedPart = pEnumPart.Next
    If (pRetrievedPart.valueIsEqualTo(pPart2)) Then
        frmProgress.recordPassedTest strTestName
    Else
        frmProgress.recordFailedTest strTestName, "ELI01574"
    End If
    
    'ensure that there are no more parts in EnumPart
    Set pRetrievedPart = pEnumPart.Next
    If (Not pRetrievedPart Is Nothing) Then
        frmProgress.recordFailedTest strTestName, "ELI01575"
    Else
        frmProgress.recordPassedTest strTestName
    End If
        
    'cleanup
    Set pPart1 = Nothing
    Set pPart2 = Nothing
    Set pEnumPart = Nothing
    Set pEnumPartModifier = Nothing
End Sub


