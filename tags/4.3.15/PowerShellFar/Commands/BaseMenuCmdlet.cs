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
		public string Title
		{
			get { return _Title; }
			set { _Title = value; }
		}
		string _Title;

		///
		[Parameter(Position = 1, HelpMessage = "Items to add to IAnyMenu.Items")]
		public FarItem[] Items
		{
			get { return _Items; }
			set { _Items = value; }
		}
		FarItem[] _Items;

		///
		[Parameter(HelpMessage = "Sets IAnyMenu.AutoAssignHotkeys")]
		public SwitchParameter AutoAssignHotkeys
		{
			get { return _AutoAssignHotkeys; }
			set { _AutoAssignHotkeys = value; }
		}
		SwitchParameter _AutoAssignHotkeys;

		///
		[Parameter(HelpMessage = "Sets IAnyMenu.Bottom")]
		public string Bottom
		{
			get { return _Bottom; }
			set { _Bottom = value; }
		}
		string _Bottom;

		///
		[Parameter(HelpMessage = "Sets IAnyMenu.HelpTopic")]
		public string HelpTopic
		{
			get { return _HelpTopic; }
			set { _HelpTopic = value; }
		}
		string _HelpTopic;

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
		public SwitchParameter SelectLast
		{
			get { return _SelectLast; }
			set { _SelectLast = value; }
		}
		SwitchParameter _SelectLast;

		///
		[Parameter(HelpMessage = "Sets IAnyMenu.ShowAmpersands")]
		public SwitchParameter ShowAmpersands
		{
			get { return _ShowAmpersands; }
			set { _ShowAmpersands = value; }
		}
		SwitchParameter _ShowAmpersands;

		///
		[Parameter(HelpMessage = "Sets IAnyMenu.WrapCursor")]
		public SwitchParameter WrapCursor
		{
			get { return _WrapCursor; }
			set { _WrapCursor = value; }
		}
		SwitchParameter _WrapCursor;

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
			menu.Title = _Title;
			menu.Bottom = _Bottom;
			menu.HelpTopic = _HelpTopic;
			if (_setSelected)
				menu.Selected = _Selected;
			if (_setX)
				menu.X = _X;
			if (_setY)
				menu.Y = _Y;

			menu.AutoAssignHotkeys = _AutoAssignHotkeys;
			menu.SelectLast = _SelectLast;
			menu.ShowAmpersands = _ShowAmpersands;
			menu.WrapCursor = _WrapCursor;

			if (_Items != null)
			{
				foreach (FarItem item in _Items)
					menu.Items.Add(item);
			}
		}
	}
}
