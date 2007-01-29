using FarManager;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Text;
using System;

namespace FarManager.Impl
{
	public class BaseEditor : IAnyEditor
	{
		#region IAnyEditor

		public event EventHandler AfterOpen;

		public void FireAfterOpen(IEditor sender)
		{
			if (AfterOpen != null)
				AfterOpen(sender, EventArgs.Empty);
		}

		public event EventHandler<RedrawEventArgs> OnRedraw;

		public void FireOnRedraw(IEditor sender, int mode)
		{
			if (OnRedraw != null)
				OnRedraw(sender, new RedrawEventArgs(mode));
		}

		public event EventHandler BeforeSave;

		public void FireBeforeSave(IEditor sender)
		{
			if (BeforeSave != null)
				BeforeSave(sender, EventArgs.Empty);
		}

		public event EventHandler AfterClose;

		public void FireAfterClose(IEditor sender)
		{
			if (AfterClose != null)
				AfterClose(sender, EventArgs.Empty);
		}

		public event EventHandler<KeyEventArgs> OnKey;

		public void FireOnKey(IEditor sender, Key key)
		{
			if (OnKey != null)
				OnKey(sender, new KeyEventArgs(key));
		}

		public event EventHandler<MouseEventArgs> OnMouse;

		public void FireOnMouse(IEditor sender, Mouse mouse)
		{
			if (OnMouse != null)
				OnMouse(sender, new MouseEventArgs(mouse));
		}

		#endregion
	}
}
