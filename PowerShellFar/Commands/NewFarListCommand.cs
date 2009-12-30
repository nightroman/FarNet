/*
PowerShellFar plugin for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System.ComponentModel;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	/// <summary>
	/// New-FarList command.
	/// Creates a list with some properties.
	/// </summary>
	/// <seealso cref="IListMenu"/>
	[Description("Creates a list with some properties.")]
	public class NewFarListCommand : BaseMenuCmdlet
	{
		///
		[Parameter(HelpMessage = "Sets IListMenu.AutoSelect")]
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

		///
		[Parameter(HelpMessage = "Sets IListMenu.Filter")]
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

		///
		[Parameter(HelpMessage = "Sets IListMenu.FilterHistory")]
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

		///
		[Parameter(HelpMessage = "Sets IListMenu.FilterKey")]
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

		///
		[Parameter(HelpMessage = "Sets IListMenu.FilterOptions")]
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

		///
		[Parameter(HelpMessage = "Sets IListMenu.FilterRestore")]
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

		///
		[Parameter(HelpMessage = "Sets IListMenu.Incremental")]
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

		///
		[Parameter(HelpMessage = "Sets IListMenu.IncrementalOptions")]
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

		///
		[Parameter(HelpMessage = "Sets IListMenu.NoShadow")]
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

		///
		[Parameter(HelpMessage = "Sets IListMenu.ScreenMargin")]
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

		///
		[Parameter(HelpMessage = "Sets IListMenu.UsualMargins")]
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

		///
		[Parameter(HelpMessage = "Intelli-list style. Uses $Psf.Settings.Intelli* options.")]
		public SwitchParameter Intelli
		{
			get;
			set;
		}

		internal IListMenu Create()
		{
			IListMenu menu = A.Far.CreateListMenu();
			Init(menu);

			if (Intelli)
				A.Psf.Settings.Intelli(menu);
			else
				A.Psf.Settings.ListMenu(menu);

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

		///
		protected override void BeginProcessing()
		{
			if (Stop())
				return;

			IListMenu menu = Create();
			WriteObject(menu);
		}
	}
}
