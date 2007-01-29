using FarManager;
using System.Collections.Generic;
using System;

namespace FarManager.Impl
{
	public class MenuItemCollection : List<IMenuItem>, IMenuItems
	{
		#region IMenuItems Members

		public IMenuItem Add(string text)
		{
			return Add(text, false, false, false);
		}

		public IMenuItem Add(string text, EventHandler onClick)
		{
			MenuItem r = new MenuItem();
			r.Text = text;
			r.OnClick += onClick;
			Add(r);
			return r;
		}

		public IMenuItem Add(string text, bool isSelected, bool isChecked, bool isSeparator)
		{
			MenuItem r = new MenuItem();
			r.Text = text;
			r.Selected = isSelected;
			r.Checked = isChecked;
			r.IsSeparator = isSeparator;
			Add(r);
			return r;
		}

		// private: makes problems in PowerShell: it is called instead of Add(string, EventHandler)
		IMenuItem IMenuItems.Add(string text, bool isSelected)
		{
			return Add(text, isSelected, false, false);
		}

		// private: it was private originally, perhaps it used to make problems, too
		IMenuItem IMenuItems.Add(string text, bool isSelected, bool isChecked)
		{
			return Add(text, isSelected, isChecked, false);
		}

		#endregion
	}
}
