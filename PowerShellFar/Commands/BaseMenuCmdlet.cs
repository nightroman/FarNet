/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	/// <summary>
	/// Common features of menu cmdlets.
	/// </summary>
	public class BaseMenuCmdlet : BaseCmdlet
	{
		///
		[Parameter(Position = 0, HelpMessage = "Sets IAnyMenu.Title")]
		public string Title { get; set; }

		///
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		[Parameter(Position = 1, HelpMessage = "Items to add to IAnyMenu.Items")]
		public FarItem[] Items { get; set; }

		///
		[Parameter(HelpMessage = "Sets IAnyMenu.AutoAssignHotkeys")]
		public SwitchParameter AutoAssignHotkeys { get; set; }

		///
		[Parameter(HelpMessage = "Sets IAnyMenu.Bottom")]
		public string Bottom { get; set; }

		///
		[Parameter(HelpMessage = "Sets IAnyMenu.HelpTopic")]
		public string HelpTopic { get; set; }

		///
		[Parameter(HelpMessage = "Sets IAnyMenu.Selected")]
		public int Selected
		{
			get { return _Selected; }
			set
			{
				_Selected = value;
				_setSelected = true;
			}
		}
		int _Selected;
		bool _setSelected;

		///
		[Parameter(HelpMessage = "Sets IAnyMenu.SelectLast")]
		public SwitchParameter SelectLast { get; set; }

		///
		[Parameter(HelpMessage = "Sets IAnyMenu.ShowAmpersands")]
		public SwitchParameter ShowAmpersands { get; set; }

		///
		[Parameter(HelpMessage = "Sets IAnyMenu.WrapCursor")]
		public SwitchParameter WrapCursor { get; set; }

		///
		[Parameter(HelpMessage = "Sets IAnyMenu.X")]
		public int X
		{
			get { return _X; }
			set
			{
				_X = value;
				_setX = true;
			}
		}
		int _X;
		bool _setX;

		///
		[Parameter(HelpMessage = "Sets IAnyMenu.Y")]
		public int Y
		{
			get { return _Y; }
			set
			{
				_Y = value;
				_setY = true;
			}
		}
		int _Y;
		bool _setY;

		///
		internal void Init(IAnyMenu menu)
		{
			menu.Title = Title;
			menu.Bottom = Bottom;
			menu.HelpTopic = HelpTopic;
			if (_setSelected)
				menu.Selected = _Selected;
			if (_setX)
				menu.X = _X;
			if (_setY)
				menu.Y = _Y;

			menu.AutoAssignHotkeys = AutoAssignHotkeys;
			menu.SelectLast = SelectLast;
			menu.ShowAmpersands = ShowAmpersands;
			menu.WrapCursor = WrapCursor;

			if (Items != null)
			{
				foreach (FarItem item in Items)
					menu.Items.Add(item);
			}
		}
	}
}
