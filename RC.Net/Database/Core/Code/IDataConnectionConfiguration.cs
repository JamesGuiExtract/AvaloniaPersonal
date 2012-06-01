//------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.CodeAnalysis;

[module: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Microsoft.Data.ConnectionUI")]
namespace Microsoft.Data.ConnectionUI
{
    /// <summary>
    /// An interface for DataConnection Dialog UI configurations based on the sample project
    /// in the Data Connnection Dialog code made available by Microsoft
    /// (http://archive.msdn.microsoft.com/Connection)
    /// The changes made to the initial interface file are:
    /// - Added XML comments
    /// </summary>
	public interface IDataConnectionConfiguration
	{
        /// <summary>
        /// Gets the selected source.
        /// </summary>
        string SelectedSource
        {
            get;
        }

        /// <summary>
        /// Saves the selected source.
        /// </summary>
        /// <param name="source">The source.</param>
        void SaveSelectedSource(string source);

        /// <summary>
        /// Gets the selected provider.
        /// </summary>
        string SelectedProvider
        {
            get;
        }

        /// <summary>
        /// Saves the selected provider.
        /// </summary>
        /// <param name="provider">The provider.</param>
        void SaveSelectedProvider(string provider);
	}
}
