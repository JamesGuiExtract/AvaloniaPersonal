// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Catching genral exceptions is normal here")]
[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "No plans to localize")]
[assembly: SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "This will be disposed by the plugin manager", Scope = "member", Target = "~F:Extract.SQLCDBEditor.Plugins.AlternateTestNameManager._addAKAButton")]
[assembly: SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "This will be disposed by the plugin manager", Scope = "member", Target = "~F:Extract.SQLCDBEditor.Plugins.AlternateTestNameManager._ignoreAKAButton")]
[assembly: SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "This will be disposed by the plugin manager", Scope = "member", Target = "~F:Extract.SQLCDBEditor.Plugins.AlternateTestNameManager._unIgnoreAKAButton")]
