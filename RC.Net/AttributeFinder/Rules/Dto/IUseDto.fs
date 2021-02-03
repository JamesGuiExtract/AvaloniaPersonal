namespace Extract.AttributeFinder.Rules.Dto

// Interface for rule objects that use a DTO for loading/saving their settings
type IUseDto<'TDto> =
  // Get/set the settings for this object
  abstract DataTransferObject: 'TDto with get, set
