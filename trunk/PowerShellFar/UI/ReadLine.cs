
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2014 Roman Kuzmin
*/

using System;
using System.Text;
using FarNet;
using FarNet.Forms;

namespace PowerShellFar.UI
{
	class ReadLine
	{
		public string HelpMessage { get; set; }
		public string History { get; set; }
		public string Prompt { get; set; }
		public bool Password { get; set; }

		public string Text { get { return _Text2 ?? _Edit.Text; } }

		IDialog _Dialog;
		IEdit _Edit;
		string _Text2;

		public bool Show()
		{
			string prompt = Prompt ?? "";
			var size = Far.Api.UI.WindowSize;

			_Dialog = Far.Api.CreateDialog(0, size.Y - 2, size.X - 1, size.Y - 1);
			_Dialog.NoShadow = true;
			_Dialog.KeepWindowTitle = true;

			if (Password)
			{
				_Edit = _Dialog.AddEditPassword(prompt.Length, 0, size.X - 1, string.Empty);
			}
			else
			{
				int right = string.IsNullOrEmpty(History) ? size.X - 1 : size.X - 2;
				_Edit = _Dialog.AddEdit(prompt.Length, 0, right, string.Empty);
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
			
			return _Dialog.Show();
		}
		void OnKey(object sender, KeyPressedEventArgs e)
		{
			switch (e.Key.VirtualKeyCode)
			{
				case KeyCode.Escape:
					if (_Edit.Line.Length > 0)
					{
						e.Ignore = true;
						_Edit.Text = "";
					}
					break;
				case KeyCode.F4:
					e.Ignore = true;
					var args = new EditTextArgs() { Text = _Edit.Text, Title = "Input text" };
					var text = Far.Api.AnyEditor.EditText(args);
					if (text != args.Text)
					{
						_Text2 = text;
						_Dialog.Close();
					}
					break;
				case KeyCode.F1:
					e.Ignore = true;
					if (!string.IsNullOrEmpty(HelpMessage))
						Far.Api.Message(HelpMessage);
					break;
			}
		}
	}
}
