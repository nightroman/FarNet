
/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Collections.Generic;

namespace FarNet.Works
{
	///
	public interface IPanelWorks : IPanel
	{
		#region Properties
		///
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
		IList<FarFile> Files { get; set; }
		///
		bool IsOpened { get; }
		///
		bool IsPushed { get; }
		///
		string StartDirectory { get; }
		///
		string Title { get; set; }
		///
		int WorksId { get; }
		///
		Panel AnotherPanel { get; }
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
		bool UseFilter { get; set; }
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
		void SetKeyBar(string[] labels);
		///
		void SetKeyBarCtrl(string[] labels);
		///
		void SetKeyBarAlt(string[] labels);
		///
		void SetKeyBarShift(string[] labels);
		///
		void SetKeyBarCtrlShift(string[] labels);
		///
		void SetKeyBarAltShift(string[] labels);
		///
		void SetKeyBarCtrlAlt(string[] labels);
		///
		PanelPlan GetPlan(PanelViewMode mode);
		///
		void SetPlan(PanelViewMode mode, PanelPlan plan);
		///
		string PanelDirectory { get; set; }
	}
}
