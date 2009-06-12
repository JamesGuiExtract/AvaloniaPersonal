Attribute VB_Name = "modLineSegment"
Option Explicit
Private Sub testGetCoordsFromParams()

    'create random line segment
    Dim line1 As ILineSegment
    Dim strBearing As String
    Dim strDistance As String
    Dim pEndPoint As ICartographicPoint
    Set line1 = createRandomLineSegment(strBearing, strDistance, pEndPoint)
    
    Dim p1 As ICartographicPoint
    Dim p2 As ICartographicPoint
    Set p1 = New CartographicPoint
    p1.dX = 0
    p1.dY = 0
    line1.getCoordsFromParams p1, p2
    
    'verify the value of the returned endpoint is correct
    Dim strTestName As String
    strTestName = "LineSegment::getCoordsFromParams()"
    If (p2.valueIsEqualTo(pEndPoint)) Then
        frmProgress.recordPassedTest strTestName
    Else
        frmProgress.recordFailedTest strTestName, "ELI01522"
    End If
End Sub

Private Sub testGetSegmentType()

    'create random line segment
    Dim line1 As ILineSegment
    Dim strBearing As String
    Dim strDistance As String
    Dim pEndPoint As ICartographicPoint
    Set line1 = createRandomLineSegment(strBearing, strDistance, pEndPoint)
    
    Dim segment As ISegment
    Set segment = line1 'QI
    
    Dim segmentType As ESegmentType
    segmentType = segment.getSegmentType()
    
    'ensure that the returned segment type is kLineSegment
    Dim strTestName As String
    strTestName = "LineSegment/ISegment.getSegmentType"
    If (segmentType <> kLine) Then
        frmProgress.recordFailedTest strTestName, "ELI01528"
    Else
        frmProgress.recordPassedTest strTestName
    End If
    
    Set line1 = Nothing
End Sub

Public Sub testLineSegment()
    
    testSetGetBearingDistance
    testSetParamsFromCoords
    testGetSegmentType
    testGetCoordsFromParams
    
End Sub

Private Sub testSetGetBearingDistance()

    'create random line segment
    Dim line1 As ILineSegment
    Dim strBearing As String
    Dim strDistance As String
    Dim pEndPoint As ICartographicPoint
    Set line1 = createRandomLineSegment(strBearing, strDistance, pEndPoint)
    
    'test the setBearingDistance and getBearingDistance methods
    Dim strRetrievedBearing As String
    Dim strRetrievedDistance As String
    line1.getBearingDistance strRetrievedBearing, strRetrievedDistance
    
    'test to see if the line bearing was retrieved successfully
    Dim strTestName As String
    strTestName = "LineSegment.getBearingDistance() - strBearing"
    If (strRetrievedBearing <> strBearing) Then
        frmProgress.recordFailedTest strTestName, "ELI01526"
    Else
        frmProgress.recordPassedTest strTestName
    End If
    
    ' test to see if the line distance was retrieved successfully
    strTestName = "LineSegment.getBearingDistance() - strDistance"
    If (strRetrievedDistance <> strDistance) Then
        frmProgress.recordFailedTest strTestName, "ELI01527"
    Else
        frmProgress.recordPassedTest strTestName
    End If

End Sub

Private Sub testSetParamsFromCoords()
    
    'create random line segment
    Dim line1 As ILineSegment
    Dim strBearing As String
    Dim strDistance As String
    Dim pEndPoint As ICartographicPoint
    Set line1 = createRandomLineSegment(strBearing, strDistance, pEndPoint)
    
    Dim p1 As ICartographicPoint
    Dim p2 As ICartographicPoint
    Set p1 = New CartographicPoint
    Set p2 = New CartographicPoint
    p1.dX = 0
    p1.dY = 0
    p2.dX = pEndPoint.dX
    p2.dY = pEndPoint.dY
    
    ' create a new line segment
    Dim line2 As ILineSegment
    Set line2 = New LineSegment
    line2.setParamsFromCoords p1, p2
    Dim strRetrievedBearing As String
    Dim strRetrievedDistance As String
    line2.getBearingDistance strRetrievedBearing, strRetrievedDistance
    
    ' test to see if the line bearing was retrieved successfully
    Dim strTestName As String
    strTestName = "LineSegment.setParamsFromCoords() - strBearing"
    If (strRetrievedBearing <> strBearing) Then
        frmProgress.recordFailedTest strTestName, "ELI01529"
    Else
        frmProgress.recordPassedTest strTestName
    End If
    
    ' test to see if the line distance was retrieved successfully
    strTestName = "LineSegment.setParamsFromCoords() - strDistance"
    Dim dDiff As Double
    dDiff = strRetrievedDistance - strDistance
    If (Abs(dDiff) < 0.00000001) Then
        frmProgress.recordPassedTest strTestName
    Else
        frmProgress.recordFailedTest strTestName, "ELI01530"
    End If

End Sub


