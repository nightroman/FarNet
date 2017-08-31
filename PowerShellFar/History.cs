
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using FarNet.Tools;

namespace PowerShellFar
{
	static class History
	{
		static readonly HistoryLog _log = new HistoryLog(Entry.LocalData + "\\PowerShellFarHistory.log", Settings.Default.MaximumHistoryCount);
		internal static HistoryLog Log { get { return _log; } }
		/// <summary>
		/// Gets history lines.
		/// </summary>
		public static string[] ReadLines()
		{
			return _log.ReadLines();
		}
		/// <summary>
		/// Add a new history line.
		/// </summary>
		public static void AddLine(string value)
		{
			_log.AddLine(value);
		}
		/// <summary>
		/// For Actor.
		/// </summary>
		public static void ShowHistory()
		{
			var m = new UI.CommandHistoryMenu(string.Empty);
			string code = m.Show();
			if (code == null)
				return;

			// insert to command lines
			switch (Far.Api.Window.Kind)
			{
				case WindowKind.Panels:
					Far.Api.CommandLine.Text = Entry.CommandInvoke1.Prefix + ": " + code;
					return;
				case WindowKind.Editor:
					var editor = Far.Api.Editor;
					if (!(editor.Host is Interactive))
						break;
					editor.GoToEnd(true);
					editor.InsertText(code);
					editor.Redraw();
					return;
				case WindowKind.Dialog:
					var dialog = Far.Api.Dialog;
					var typeId = dialog.TypeId;
					if (typeId != UI.InputDialog.TypeId)
						break;
					var line = Far.Api.Line;
					if (line == null || line.IsReadOnly)
						break;
					line.Text = code;
					return;
			}

			// show "Invoke commands"
			var ui = new UI.InputDialog() { Title = Res.Me, History = Res.History, Prompt = new string[] { Res.InvokeCommands }, Text = code };
			if (!ui.Show())
				return;

			// invoke input
			A.Psf.Act(ui.Text, null, true);
		}
	}
}
