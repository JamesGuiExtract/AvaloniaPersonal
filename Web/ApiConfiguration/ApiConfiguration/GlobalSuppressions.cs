﻿// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types")]
[assembly: SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "Disposed with composite disposable",
    Scope = "member", Target = "~F:Extract.Web.ApiConfiguration.ViewModels.ApiConfigMgmtViewModel._configurationsSourceCache")]
