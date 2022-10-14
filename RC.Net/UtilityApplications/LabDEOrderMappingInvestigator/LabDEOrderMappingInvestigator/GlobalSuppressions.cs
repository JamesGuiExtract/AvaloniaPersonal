﻿// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types")]
[assembly: SuppressMessage("Security", "CA2326:Do not use TypeNameHandling values other than None", Justification = "This seems to be required by the framework (AutoSuspendHelper)", Scope = "member", Target = "~F:LabDEOrderMappingInvestigator.Services.JsonSuspensionDriver._settings")]
