chop_SSN.rsd

  Purpose:
	
	This .rsd file removes the last four digits of both printed and
	handwritten social security numbers.  To be used for projects in
	which the customer only wants to redact the first five digits of
	social security numbers.

  Usage:
    When used as a splitter, this .rsd file will return the trimed
	social security number as an 'item' sub-attribute.
	
  Warning:
	Could have undesirable results if used with non-rectangular 
	region (i.e., multi-line textual strings)
