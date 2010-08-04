
/*
FarNet.Tools library for FarNet
Copyright (c) 2010 Roman Kuzmin
*/

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
			Dialog = Far.Net.CreateDialog(-1, -1, DLG_XSIZE, DLG_YSIZE);
			Box = Dialog.AddBox(3, 1, 0, 0, string.Empty);
		}

		/// <summary>
		/// Sets the form size.
		/// </summary>
		protected void SetSize(int width, int height)
		{
			Dialog.Rect = new Place(-1, -1, width, height);
			Box.Rect = new Place(3, 1, width - 4, height - 2);
		}

		/// <summary>
		/// Shows the form.
		/// </summary>
		/// <returns>False if the form was cancelled.</returns>
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
