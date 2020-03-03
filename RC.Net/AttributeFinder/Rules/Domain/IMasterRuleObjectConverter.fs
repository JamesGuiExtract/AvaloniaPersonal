namespace Extract.AttributeFinder.Rules.Domain

open System

type IMasterRuleObjectConverter =
  abstract ToDto: obj -> (obj * string) option
  abstract FromDto: obj -> obj option
