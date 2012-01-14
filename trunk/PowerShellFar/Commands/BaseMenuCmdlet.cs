
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2012 Roman Kuzmin
*/

using System.Management.Automation;
using System.Diagnostics.CodeAnalysis;
using FarNet;

namespace PowerShellFar.Commands
{
	/// <summary>
	/// Common features of menu cmdlets.
	/// </summary>
	class BaseMenuCmdlet : BaseCmdlet
	{
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[Parameter(Position = 0)]
		public string Title { get; set; }
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		[Parameter(Position = 1)]
		public FarItem[] Items { get; set; }
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[Parameter]
		public SwitchParameter AutoAssignHotkeys { get; set; }
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[Parameter]
		public string Bottom { get; set; }
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[Parameter]
		public string HelpTopic { get; set; }
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
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
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[Parameter]
		public SwitchParameter SelectLast { get; set; }
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[Parameter]
		public SwitchParameter ShowAmpersands { get; set; }
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
		[Parameter]
		public SwitchParameter WrapCursor { get; set; }
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
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
		[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
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
