// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Catching genral exceptions is normal here")]
[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "No plans to localize")]
[assembly: SuppressMessage("Design", "CA1065:Do not raise exceptions in unexpected locations", Justification = "This triggers when catching/wrapping/throwing which is not a valid warning")]
