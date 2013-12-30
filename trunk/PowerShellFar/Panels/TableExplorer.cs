
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2013 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// Abstract table explorer.
	/// </summary>
	public abstract class TableExplorer : PowerExplorer
	{
		/// <inheritdoc/>
		protected TableExplorer(Guid typeId) : base(typeId) { }
		/// <include file='doc.xml' path='doc/Columns/*'/>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		internal virtual object[] Columns { get; set; }
	}
}
