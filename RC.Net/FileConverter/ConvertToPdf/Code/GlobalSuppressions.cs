// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types")]
[assembly: SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "False positive", Scope = "member", Target = "~M:Extract.FileConverter.ConvertToPdf.MimeKitEmailToPdfConverter.PdfPacket.#ctor(System.Collections.Generic.IList{Extract.FileConverter.ConvertToPdf.EmailPartFileRecord})")]
