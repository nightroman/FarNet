
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using FarNet.Forms;
using System;

namespace PowerShellFar.UI
{
	class ReadLine
	{
		public string HelpMessage { get; set; }
		public string History { get; set; }
		public string Prompt { get; set; }
		public bool Password { get; set; }

		public string Text => _Text2 ?? _Edit.Text;

		IDialog _Dialog;
		IEdit _Edit;
		string _Text2;
		Func<bool> _show;

		public bool Show()
		{
			string prompt = Prompt ?? "";
			var size = Far.Api.UI.WindowSize;

			_Dialog = Far.Api.CreateDialog(0, size.Y - 1, size.X - 1, size.Y - 1);
			_Dialog.TypeId = new Guid(Guids.ReadLineDialog);
			_Dialog.NoShadow = true;
			_Dialog.KeepWindowTitle = true;

			if (Password)
			{
				_Edit = _Dialog.AddEditPassword(prompt.Length, 0, size.X - 1, string.Empty);
			}
			else
			{
				//! make 1 wider with history, to hide the arrow
				_Edit = _Dialog.AddEdit(prompt.Length, 0, size.X - 1, string.Empty);
				_Edit.History = History;
			}
			_Edit.Coloring += Coloring.ColorEditAsConsole;

			if (prompt.Length > 0)
			{
				var uiText = _Dialog.AddText(0, 0, prompt.Length - 1, prompt);
				uiText.Coloring += Coloring.ColorTextAsConsole;
			}

			var uiArea = _Dialog.AddText(0, 1, size.X - 1, string.Empty);
			uiArea.Coloring += Coloring.ColorTextAsConsole;

			// hotkeys
			_Edit.KeyPressed += OnKey;

			// ignore clicks outside
			_Dialog.MouseClicked += (sender, e) =>
			{
				if (e.Control == null)
					e.Ignore = true;
			};

			for (; ; )
			{
				bool isDesktop = Far.Api.Window.Kind == WindowKind.Desktop;

				//! or edit box history clears console text
				if (isDesktop)
					Far.Api.UI.SetUserScreen(Far.Api.UI.SetUserScreen(0));

				bool yes = _Dialog.Show();

				if (!yes)
					return false;

				if (_show == null)
					return true;

				//! or editor makes noise, e.g. Colorer "Reloading..." message, [1]
				int screen = 0;
				if (isDesktop)
					screen = Far.Api.UI.SetUserScreen(0);

				yes = _show();

				//! do after [1] or it shows panels
				if (isDesktop)
					Far.Api.UI.SetUserScreen(screen);

				_show = null;
				if (yes)
					return true;
			}
		}

		void OnKey(object sender, KeyPressedEventArgs e)
		{
			switch (e.Key.VirtualKeyCode)
			{
				case KeyCode.Escape:
					// clear the text or exit
					if (_Edit.Line.Length > 0)
					{
						e.Ignore = true;
						_Edit.Text = "";
					}
					break;
				case KeyCode.F1:
					// show the help message
					e.Ignore = true;
					if (!string.IsNullOrEmpty(HelpMessage))
						Far.Api.Message(HelpMessage);
					break;
				case KeyCode.F4:
					_show = () =>
					{
						var args = new EditTextArgs() { Text = _Edit.Text, Title = "Input text" };
						var text = Far.Api.AnyEditor.EditText(args);
						if (text == args.Text)
							return false;

						_Text2 = text;
						return true;
					};
					e.Ignore = true;
					_Dialog.Close();
					break;
			}
		}
	}
}
