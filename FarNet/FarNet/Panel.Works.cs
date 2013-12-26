
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2013 Roman Kuzmin
*/

using System;
using System.Collections.Generic;

namespace FarNet.Works
{
	/// <summary>
	/// For internal use.
	/// </summary>
	public interface IPanelWorks : IPanel
	{
		///
		Explorer MyExplorer { get; }
		///
		void Navigate(Explorer explorer);
		#region Properties
		///
		bool IsOpened { get; }
		///
		bool IsPushed { get; }
		///
		string StartDirectory { get; }
		///
		string Title { get; set; }
		///
		Panel TargetPanel { get; }
		#endregion
		#region Core
		///
		bool CompareFatTime { get; set; }
		///
		bool PreserveCase { get; set; }
		///
		bool RawSelection { get; set; }
		///
		bool RealNamesDeleteFiles { get; set; }
		///
		bool RealNamesExportFiles { get; set; }
		///
		bool RealNamesImportFiles { get; set; }
		///
		bool RealNamesMakeDirectory { get; set; }
		///
		bool RightAligned { get; set; }
		///
		bool ShowNamesOnly { get; set; }
		///
		bool NoFilter { get; set; }
		///
		PanelHighlighting Highlighting { get; set; }
		#endregion
		#region Methods
		///
		void Open();
		///
		void OpenReplace(Panel current);
		///
		void PostData(object data);
		///
		void PostFile(FarFile file);
		///
		void PostName(string name);
		#endregion
		#region Other Info
		///
		string FormatName { get; set; }
		///
		string HostFile { get; set; }
		#endregion
		///
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		DataItem[] InfoItems { get; set; }
		///
		void SetKeyBars(KeyBar[] bars);
		///
		PanelPlan GetPlan(PanelViewMode mode);
		///
		void SetPlan(PanelViewMode mode, PanelPlan plan);
		///
		string CurrentLocation { get; set; }
	}
}
