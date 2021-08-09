﻿// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task",
    Justification = "This warning is intended for libraries and this is a unit test project")]

[assembly: SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores",
    Justification = "This is a unit test project and underscores improve test name readability")]
