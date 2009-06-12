The output handlers which were previously
in SpecialFinder.rsd and BuiltinFinder.rsd
were moved to UseBuiltinSplitter.rsd and
UseSpecialSplitter.rsd to enable the same
output handlers to be used whether text is
found with the rubberband tool or by the
attribute finding rules.

The output handlers used by LegalDescription.rsd must
be copied to RubberbandLegal.rsd and care must be taken
to change the paths to rsd files used by the output handlers.