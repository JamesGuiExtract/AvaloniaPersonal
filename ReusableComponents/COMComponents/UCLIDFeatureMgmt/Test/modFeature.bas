Attribute VB_Name = "modFeature"
Option Explicit

Public Sub testFeature()
    
    testMarkAsPolygonFeature
    testMarkAsPolylineFeature
    testPartRelatedFeatures
    
End Sub

Private Sub testMarkAsPolygonFeature()

    Dim pFeature As IFeature
    Set pFeature = New Feature
    pFeature.setFeatureType kPolygon
    
    Dim strTestName As String
    strTestName = "Feature.setFeatureType() - 1"
    If (pFeature.getFeatureType() = kPolygon) Then
        frmProgress.recordPassedTest strTestName
    Else
        frmProgress.recordFailedTest strTestName, "ELI01576"
    End If

    'cleanup
    Set pFeature = Nothing
    
End Sub

Private Sub testMarkAsPolylineFeature()

    Dim pFeature As IFeature
    Set pFeature = New Feature
    pFeature.setFeatureType kPolyline
    
    Dim strTestName As String
    strTestName = "Feature.setFeatureType() - 2"
    If (pFeature.getFeatureType() = kPolyline) Then
        frmProgress.recordPassedTest strTestName
    Else
        frmProgress.recordFailedTest strTestName, "ELI01578"
    End If

    'cleanup
    Set pFeature = Nothing
    
End Sub

Private Sub testPartRelatedFeatures()

    ' create a new feature
    Dim pFeature As IFeature
    Set pFeature = New Feature
    pFeature.setFeatureType kPolyline
    
    ' test to make sure that there are zero parts
    Dim strTestName As String
    strTestName = "Feature.getNumParts() - 1"
    If (pFeature.getNumParts <> 0) Then
        frmProgress.recordFailedTest strTestName, "ELI01580"
    Else
        frmProgress.recordPassedTest strTestName
    End If
    
    ' create two random parts
    Dim pPart1 As IPart
    Dim pPart1Segments As IIUnknownVector
    Dim pPart1StartPoint As ICartographicPoint
    Set pPart1 = createRandomPart(pPart1Segments, pPart1StartPoint)
    
    Dim pPart2 As IPart
    Dim pPart2Segments As IIUnknownVector
    Dim pPart2StartPoint As ICartographicPoint
    Set pPart2 = createRandomPart(pPart2Segments, pPart2StartPoint)
    
    ' add the first part to the feature and
    ' test to make sure that the number of parts is valid
    pFeature.addPart pPart1
    strTestName = "Feature.getNumParts() - 2"
    If (pFeature.getNumParts <> 1) Then
        frmProgress.recordFailedTest strTestName, "ELI01584"
    Else
        frmProgress.recordPassedTest strTestName
    End If
    
    ' add the second part to the feature and
    ' test to make sure that the number of parts is valid
    pFeature.addPart pPart2
    strTestName = "Feature.getNumParts() - 3"
    If (pFeature.getNumParts <> 2) Then
        frmProgress.recordFailedTest strTestName, "ELI01581"
    Else
        frmProgress.recordPassedTest strTestName
    End If
    
    ' get the parts from the feature
    Dim pEnumPart As IEnumPart
    Set pEnumPart = pFeature.getParts
    Dim pRetrievedPart As IPart
    
    ' ensure that the first retrieved part is same as part 1
    Set pRetrievedPart = pEnumPart.Next
    strTestName = "Feature.getParts().next() - 1"
    If (pRetrievedPart.valueIsEqualTo(pPart1)) Then
        frmProgress.recordPassedTest strTestName
    Else
        frmProgress.recordFailedTest strTestName, "ELI01582"
    End If
    
    ' ensure that the second part is same as part 2
    Set pRetrievedPart = pEnumPart.Next
    strTestName = "Feature.getParts().next() - 2"
    If (pRetrievedPart.valueIsEqualTo(pPart2)) Then
        frmProgress.recordPassedTest strTestName
    Else
		frmProgress.recordFailedTest(strTestName, "ELI19492")
    End If
    
    ' ensure that there are no more parts left
    Set pRetrievedPart = pEnumPart.Next
    strTestName = "Feature.getParts().next() - 3"
    If (pRetrievedPart Is Nothing) Then
        frmProgress.recordPassedTest strTestName
    Else
        frmProgress.recordFailedTest strTestName, "ELI01583"
    End If
    
    ' cleanup
    Set pPart1 = Nothing
    Set pPart2 = Nothing
    Set pFeature = Nothing
    Set pRetrievedPart = Nothing
End Sub
