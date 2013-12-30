
/*
FarNet.Tools library for FarNet
Copyright (c) 2010 Roman Kuzmin
*/

using System;
using FarNet.Forms;

namespace FarNet.Tools
{
	/// <summary>
	/// Base class of utility forms.
	/// </summary>
	public abstract class Form
	{
		const int DLG_XSIZE = 78;
		const int DLG_YSIZE = 22;
		IBox Box { get; set; }

		/// <summary>
		/// The dialog used by form.
		/// </summary>
		protected IDialog Dialog { get; private set; }
		/// <summary>
		/// Gets or sets the form title.
		/// </summary>
		public virtual string Title
		{
			get { return Box.Text; }
			set { Box.Text = value; }
		}
		/// <summary>
		/// Creates the <see cref="Dialog"/> instance.
		/// </summary>
		protected Form()
		{
			Dialog = Far.Api.CreateDialog(-1, -1, DLG_XSIZE, DLG_YSIZE);
			Box = Dialog.AddBox(3, 1, 0, 0, string.Empty);
		}
		/// <summary>
		/// Sets the form size.
		/// </summary>
		/// <param name="height">The form height.</param>
		/// <param name="width">The form width.</param>
		protected void SetSize(int width, int height)
		{
			if (width <= 4 || height <= 2) throw new ArgumentException("Too small value.");
			
			Dialog.Rect = new Place(-1, -1, width, height);
			Box.Rect = new Place(3, 1, width - 4, height - 2);
		}
		/// <summary>
		/// Shows the form.
		/// </summary>
		/// <returns>False if the form was canceled.</returns>
		public virtual bool Show()
		{
			return Dialog.Show();
		}
		/// <summary>
		/// Closes the form.
		/// </summary>
		public virtual void Close()
		{
			Dialog.Close();
		}

	}
}
