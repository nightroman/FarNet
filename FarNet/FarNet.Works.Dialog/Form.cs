/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using FarNet.Forms;

namespace FarNet.Works
{
	public class Form
	{
		const int DLG_XSIZE = 78;
		const int DLG_YSIZE = 22;

		protected IBox Box { get; private set; }
		protected IDialog Dialog { get; private set; }

		public virtual string Title
		{
			get
			{
				return Box.Text;
			}
			set
			{
				Box.Text = value;
			}
		}

		protected Form()
		{
			Dialog = Far.Net.CreateDialog(-1, -1, DLG_XSIZE, DLG_YSIZE);
			Box = Dialog.AddBox(3, 1, 0, 0, string.Empty);
		}

		public virtual bool Show()
		{
			return Dialog.Show();
		}

	}
}
