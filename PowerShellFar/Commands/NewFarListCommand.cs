
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2012 Roman Kuzmin
*/

using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	class NewFarListCommand : BaseMenuCmdlet
	{
		[Parameter()]
		public SwitchParameter AutoSelect
		{
			get { return _AutoSelect; }
			set
			{
				_AutoSelect = value;
				_setAutoSelect = true;
			}
		}
		SwitchParameter _AutoSelect;
		bool _setAutoSelect;
		[Parameter()]
		public string Filter
		{
			get { return _Filter; }
			set
			{
				_Filter = value;
				_setFilter = true;
			}
		}
		string _Filter;
		bool _setFilter;
		[Parameter()]
		public string FilterHistory
		{
			get { return _FilterHistory; }
			set
			{
				_FilterHistory = value;
				_setFilterHistory = true;
			}
		}
		string _FilterHistory;
		bool _setFilterHistory;
		[Parameter()]
		public int FilterKey
		{
			get { return _FilterKey; }
			set
			{
				_FilterKey = value;
				_setFilterKey = true;
			}
		}
		int _FilterKey;
		bool _setFilterKey;
		[Parameter()]
		public PatternOptions FilterOptions
		{
			get { return _FilterOptions; }
			set
			{
				_FilterOptions = value;
				_setFilterOptions = true;
			}
		}
		PatternOptions _FilterOptions;
		bool _setFilterOptions;
		[Parameter()]
		public SwitchParameter FilterRestore
		{
			get { return _FilterRestore; }
			set
			{
				_FilterRestore = value;
				_setFilterRestore = true;
			}
		}
		SwitchParameter _FilterRestore;
		bool _setFilterRestore;
		[Parameter()]
		public string Incremental
		{
			get { return _Incremental; }
			set
			{
				_Incremental = value;
				_setIncremental = true;
			}
		}
		string _Incremental;
		bool _setIncremental;
		[Parameter()]
		public PatternOptions IncrementalOptions
		{
			get { return _IncrementalOptions; }
			set
			{
				_IncrementalOptions = value;
				_setIncrementalOptions = true;
			}
		}
		PatternOptions _IncrementalOptions;
		bool _setIncrementalOptions;
		[Parameter()]
		public SwitchParameter NoShadow
		{
			get { return _NoShadow; }
			set
			{
				_NoShadow = value;
				_setNoShadow = true;
			}
		}
		SwitchParameter _NoShadow;
		bool _setNoShadow;
		[Parameter()]
		public int ScreenMargin
		{
			get { return _ScreenMargin; }
			set
			{
				_ScreenMargin = value;
				_setScreenMargin = true;
			}
		}
		int _ScreenMargin;
		bool _setScreenMargin;
		[Parameter()]
		public SwitchParameter UsualMargins
		{
			get { return _UsualMargins; }
			set
			{
				_UsualMargins = value;
				_setUsualMargins = true;
			}
		}
		SwitchParameter _UsualMargins;
		bool _setUsualMargins;
		[Parameter()]
		public SwitchParameter Popup { get; set; }
		internal IListMenu Create()
		{
			IListMenu menu = Far.Net.CreateListMenu();
			Init(menu);

			if (Popup)
				Settings.Default.PopupMenu(menu);
			else
				Settings.Default.ListMenu(menu);

			if (_setAutoSelect)
				menu.AutoSelect = _AutoSelect;
			if (_setFilter)
				menu.Filter = _Filter;
			if (_setFilterHistory)
				menu.FilterHistory = _FilterHistory;
			if (_setFilterKey)
				menu.FilterKey = _FilterKey;
			if (_setFilterOptions)
				menu.FilterOptions = _FilterOptions;
			if (_setFilterRestore)
				menu.FilterRestore = _FilterRestore;
			if (_setIncremental)
				menu.Incremental = _Incremental;
			if (_setIncrementalOptions)
				menu.IncrementalOptions = _IncrementalOptions;
			if (_setNoShadow)
				menu.NoShadow = _NoShadow;
			if (_setScreenMargin)
				menu.ScreenMargin = _ScreenMargin;
			if (_setUsualMargins)
				menu.UsualMargins = _UsualMargins;

			return menu;
		}
		protected override void BeginProcessing()
		{
			IListMenu menu = Create();
			WriteObject(menu);
		}
	}
}
