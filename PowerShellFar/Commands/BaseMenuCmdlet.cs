
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using System.Management.Automation;
using FarNet;

namespace PowerShellFar.Commands
{
	/// <summary>
	/// Common features of menu cmdlets.
	/// </summary>
	class BaseMenuCmdlet : BaseCmdlet
	{
		[Parameter(Position = 0)]
		public string Title { get; set; }

		[Parameter(Position = 1)]
		public FarItem[] Items { get; set; }

		[Parameter]
		public SwitchParameter AutoAssignHotkeys { get; set; }

		[Parameter]
		public string Bottom { get; set; }

		[Parameter]
		public string HelpTopic { get; set; }
		[Parameter]
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

		[Parameter]
		public SwitchParameter SelectLast { get; set; }

		[Parameter]
		public SwitchParameter ShowAmpersands { get; set; }

		[Parameter]
		public SwitchParameter WrapCursor { get; set; }

		[Parameter]
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

		[Parameter]
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

		//! it is common for menus and lists but set separately, in lists after preferences if _set*
		[Parameter]
		public SwitchParameter NoShadow
		{
			get { return _NoShadow; }
			set
			{
				_NoShadow = value;
				_setNoShadow = true;
			}
		}
		protected SwitchParameter _NoShadow;
		protected bool _setNoShadow;

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
