﻿// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This is standard practice here.")]
[assembly: SuppressMessage("Maintainability", "CA1508:Avoid dead conditional code", Justification = "This rule has many false positives")]
