Trim.rsd et al.

  Purpose:
    For use in ID Shield ® rules to trim a Spatial String so that the left and right sides of its
    bounding rectangle contain significant concentrations of black pixels.

    This is useful as a modifier for Spatial Strings created as image regions when only partial
    item coverage is desired (e.g., only first five digits of an SSN redacted), or to eliminate
    'blank' false positives (i.e., when a string fails to meet black pixel concentrations at any
    point, Trim.rsd will not return anything).

  Usage:
    As a splitter to create a subattribute that is a modified ("trimmed") version of the attribute
    split or from within a splitter or as a finding rule to find a trimmed version of the input
    Spatial String.

    For an example of usage, see LA East Baton Rouge - ACS rules from which these files were
    extracted.
    
Warning:
    Could have undesirable results if used with non-rectangular (i.e., multi-line textual strings)
