Attribute VB_Name = "modCreators"
Option Explicit
Public Function createRandomArcSegment(ByRef pParams As IIUnknownVector, _
                                       ByRef pMidPoint As ICartographicPoint, _
                                       ByRef pEndPoint As ICartographicPoint) _
                                       As ISegment

    ' allocate random parameters
    Dim pParam1 As IParameterTypeValuePair
    Dim pParam2 As IParameterTypeValuePair
    Dim pParam3 As IParameterTypeValuePair
    Dim pParam4 As IParameterTypeValuePair
    Set pParam1 = New ParameterTypeValuePair
    Set pParam2 = New ParameterTypeValuePair
    Set pParam3 = New ParameterTypeValuePair
    Set pParam4 = New ParameterTypeValuePair
    pParam1.eParamType = kArcTangentInBearing
    pParam1.strValue = "North"
    pParam2.eParamType = kArcRadius
    pParam2.strValue = "1"
    pParam3.eParamType = kArcDelta
    pParam3.strValue = "180 deg"
    pParam4.eParamType = kArcConcaveLeft
    pParam4.strValue = "0"
     
   ' allocate a new vector object to store the params
    Set pParams = New IUnknownVector
    pParams.push_back pParam1
    pParams.push_back pParam2
    pParams.push_back pParam3
    pParams.push_back pParam4
    
    'create an arc segment
    Dim pArc As IArcSegment
    Set pArc = New ArcSegment
    
    'set the parameters to the arc
    pArc.setParameters pParams
    
    'set the mid point and end point
    Set pMidPoint = New CartographicPoint
    Set pEndPoint = New CartographicPoint
    pMidPoint.dX = 1
    pMidPoint.dY = 1
    pEndPoint.dX = 2
    pEndPoint.dY = 0
    
    'get the Isegment interface and return the arc object
    Dim pSegment As ISegment
    Set pSegment = pArc
    Set createRandomArcSegment = pSegment
End Function

Public Function createRandomLineSegment(ByRef rstrLineBearing As String, _
                                        ByRef rstrLineDistance As String, _
                                        ByRef pEndPoint As ICartographicPoint) _
                                        As ISegment
    ' create a new line object
    Dim pLine As ILineSegment
    Set pLine = New LineSegment
    
    ' create random bearing and distance and set to the line object
    rstrLineBearing = "N"
    rstrLineDistance = "330"
    pLine.setBearingDistance rstrLineBearing, rstrLineDistance
    
    ' determine the ending point of the line with the bearing and distance
    ' if the start point was (0, 0)
    Set pEndPoint = New CartographicPoint
    pEndPoint.dX = 0
    pEndPoint.dY = 330
    
    ' get the segment interface and return to the caller
    Dim pSegment As ISegment
    Set pSegment = pLine 'QI
    Set createRandomLineSegment = pSegment
End Function
Public Function createRandomParameterTypeValuePair(ByRef reCurveParameterType As ECurveParameterType, _
                                                   ByRef rstrValue As String) _
                                                   As IParameterTypeValuePair
    
    Dim pParamValueTypePair As IParameterTypeValuePair
    Set pParamValueTypePair = New ParameterTypeValuePair
    
    ' populate with random data
    reCurveParameterType = kArcDelta
    rstrValue = "43.23 deg"
    pParamValueTypePair.eParamType = reCurveParameterType
    pParamValueTypePair.strValue = rstrValue
    
    Set createRandomParameterTypeValuePair = pParamValueTypePair
    
End Function

Public Function createRandomPoint() As ICartographicPoint
    Dim pPoint As ICartographicPoint
    Set pPoint = New CartographicPoint
    pPoint.dX = Rnd
    pPoint.dY = Rnd
    Set createRandomPoint = pPoint
End Function

Public Function createRandomPart(ByRef pSegments As IIUnknownVector, _
                                 ByRef pStartPoint As ICartographicPoint) _
                                 As IPart

    ' create a random line segment
    Dim pLine As ILineSegment
    Dim strLineBearing As String
    Dim strLineDistance As String
    Dim pLineEndPoint As ICartographicPoint
    Set pLine = createRandomLineSegment(strLineBearing, strLineDistance, pLineEndPoint)
    
    ' create a random arc segment
    Dim pArc As IArcSegment
    Dim pArcParams As IIUnknownVector
    Dim pArcMidPoint As ICartographicPoint
    Dim pArcEndPoint As ICartographicPoint
    Set pArc = createRandomArcSegment(pArcParams, pArcMidPoint, pArcEndPoint)
    
    ' create a part with the line and arc segments
    Dim pPart As IPart
    Set pPart = New Part
    pPart.addSegment pLine
    pPart.addSegment pArc
    
    Set pSegments = New IUnknownVector
    pSegments.push_back pLine
    pSegments.push_back pArc
    
    'set a random starting point
    Set pStartPoint = createRandomPoint()
    pPart.setStartingPoint pStartPoint
    
    ' return the part object to the caller
    Set createRandomPart = pPart
    
End Function

Public Function createRandomFeature() As IFeature
    
    ' create feature object
    Dim pFeature As IFeature
    Set pFeature = New Feature
    pFeature.setFeatureType kPolygon
    
    ' create two random parts
    Dim pPart1 As IPart
    Dim pPart1Segments As IIUnknownVector
    Dim pPart1StartPoint As ICartographicPoint
    Set pPart1 = createRandomPart(pPart1Segments, pPart1StartPoint)
    
    Dim pPart2 As IPart
    Dim pPart2Segments As IIUnknownVector
    Dim pPart2StartPoint As ICartographicPoint
    Set pPart2 = createRandomPart(pPart2Segments, pPart2StartPoint)
    
    ' add the two parts to the feature
    pFeature.addPart pPart1
    pFeature.addPart pPart2
    
    ' return the feature object
    Set createRandomFeature = pFeature
    
End Function
