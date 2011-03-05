
/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// Table explorer base class.
	/// </summary>
	public abstract class TableExplorer : PowerExplorer
	{
		///
		protected TableExplorer(Guid typeId) : base(typeId) { }
		/// <include file='doc.xml' path='doc/Columns/*'/>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		internal virtual object[] Columns
		{
			get { return _Columns; }
			set
			{
				if (Cache.Count > 0) throw new InvalidOperationException("Panel must have no files for setting columns.");
				_Columns = value;
			}
		}
		object[] _Columns;
	}
}
