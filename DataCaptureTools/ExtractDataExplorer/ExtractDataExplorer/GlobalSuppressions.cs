﻿// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types")]
[assembly: SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed with composite disposable", Scope = "member", Target = "~M:ExtractDataExplorer.ViewModels.MainWindowViewModel.#ctor(ExtractDataExplorer.Models.MainWindowModel,Extract.Utilities.WPF.IFileBrowserDialogService,Extract.Utilities.WPF.IMessageDialogService,ExtractDataExplorer.Services.IThemingService,ExtractDataExplorer.Services.IAttributeTreeService)")]
[assembly: SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed with composite disposable", Scope = "member", Target = "~M:ExtractDataExplorer.ViewModels.AttributeTreeViewModel.#ctor(DynamicData.Node{ExtractDataExplorer.Models.AttributeModel,System.Guid},System.IObservable{ExtractDataExplorer.Services.IAttributeFilter})")]