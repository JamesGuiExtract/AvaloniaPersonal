module DTO

/// Used by ClassifyCandidates and MLNetQueue for communicating requests
type PredictionRequest = {
  InputDataFile: string
  OutputDataFile: string
}
