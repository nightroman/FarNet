
/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace FarNet
{
	public partial class Panel : IPanel
	{
		/// <summary>
		/// Called before <see cref="UIEscape"/>.
		/// </summary>
		/// <remarks>
		/// If <see cref="PanelEventArgs.Ignore"/> = true then the core does nothing.
		/// Otherwise it calls <see cref="UIEscape"/> to close the panel.
		/// </remarks>
		public event EventHandler<PanelKeyEventArgs> Escaping;
		/// <summary>
		/// Called when [Esc] or [ShiftEsc] is pressed and the command line is empty.
		/// </summary>
		/// <remarks>
		/// By default it closes the the panel itself or with all parent panels.
		/// The panel may override this method or use the <see cref="Escaping"/> event.
		/// </remarks>
		public virtual void UIEscape(bool all)
		{
			if (!CanClose())
				return;

			if (all || _Parent == null)
			{
				// _090321_210416 We do not call Redraw(0, 0) to reset cursor to 0 any more.
				// See Mantis 1114: why it was needed. Now FarNet panels restore original state.

				// ask parents
				if (all)
				{
					for (var parent = _Parent; parent != null; parent = parent._Parent)
						if (!parent.CanClose())
							return;
				}

				// close
				_Panel.Close();
			}
			else
			{
				CloseChild();
			}
		}
		/// <summary>
		/// Opens the current file in the editor.
		/// </summary>
		/// <remarks>
		/// The default method calls <see cref="FarNet.Explorer.ExportFile"/>  to get a temporary file to edit
		/// and <see cref="FarNet.Explorer.ImportFile"/> to save changes when the editor closes.
		/// The explorer should have at least export implemented.
		/// </remarks>
		public virtual void UIEditCurrentFile()
		{
			var file = CurrentFile;
			if (file == null)
				return;

			if (!Explorer.CanExportFile(file))
				return;

			var temp = Far.Net.TempName();
			if (TempFileExtension != null)
				temp += TempFileExtension;

			var args = new ExportFileArgs();
			args.File = file;
			args.FileName = temp;

			Log.Source.TraceInformation("ExportFile");
			Explorer.ExportFile(args);
			if (args.Result != ExplorerResult.Done)
				return;

			var editor = Far.Net.CreateEditor();
			editor.DeleteSource = DeleteSource.File;
			editor.FileName = args.FileName;
			editor.Title = file.Name;

			if (Explorer.CanExportFile(file))
			{
				editor.Closed += delegate //???? to use Saved (Far 3), update docs.
				{
					if (editor.TimeOfSave == DateTime.MinValue)
						return;

					var args2 = new ImportFileArgs();
					args2.File = file;
					args2.FileName = temp;

					Log.Source.TraceInformation("ImportFile");
					Explorer.ImportFile(args2);
				};
			}
			else
			{
				editor.Saving += delegate
				{
					Far.Net.Message(string.Format(null, "Saving to the file to be deleted:\r{0}", temp), "Warning");
				};
			}

			editor.Open();
		}
		/// <summary>
		/// Opens the current file in the viewer.
		/// </summary>
		/// <remarks>
		/// The default method calls <see cref="FarNet.Explorer.ExportFile"/> to get a temporary file to view.
		/// The explorer should have it implemented.
		/// </remarks>
		public virtual void UIViewCurrentFile()
		{
			var file = CurrentFile;
			if (file == null)
				return;

			if (!Explorer.CanExportFile(file))
				return;

			var temp = Far.Net.TempName();

			var args = new ExportFileArgs();
			args.File = file;
			args.FileName = temp;

			Log.Source.TraceInformation("Explorer.ExportFile");
			Explorer.ExportFile(args);
			if (args.Result != ExplorerResult.Done)
				return;

			var viewer = Far.Net.CreateViewer();
			viewer.DeleteSource = DeleteSource.File;
			viewer.FileName = args.FileName;
			viewer.Title = file.Name;
			viewer.Open();
		}
		/// <summary>
		/// Called when a key is pressed.
		/// </summary>
		/// <remarks>
		/// Set <see cref="PanelEventArgs.Ignore"/> = true if the module processes the key itself.
		/// <para>
		/// The event is not called on some special keys processed internally.
		/// Normally panels are not interested in these keys,
		/// otherwise they should use the <see cref="KeyPressing"/> event.
		/// </para>
		/// <para>
		/// In Far Manager this event is called from the <c>ProcessKeyW</c> without the <c>PKF_PREPROCESS</c> flag.
		/// </para>
		/// </remarks>
		public event EventHandler<PanelKeyEventArgs> KeyPressed;
		/// <summary>
		/// Called when a key is pressed. See <see cref="KeyPressed"/>.
		/// </summary>
		public virtual void UIKeyPressed(PanelKeyEventArgs e)
		{
			if (e == null) throw new ArgumentNullException("e");

			if (KeyPressed != null)
			{
				KeyPressed(this, e);
				if (e.Ignore)
					return;
			}

			switch (e.Code)
			{
				case VKeyCode.F3:
					if (e.State == KeyStates.None)
					{
						e.Ignore = true;
						UIViewCurrentFile();
					}
					break;
				case VKeyCode.F4:
					if (e.State == KeyStates.None)
					{
						e.Ignore = true;
						UIEditCurrentFile();
					}
					break;
				case VKeyCode.Escape:
					if ((e.State == KeyStates.None || e.State == KeyStates.Shift) && Far.Net.CommandLine.Length == 0)
					{
						if (Escaping != null)
						{
							Escaping(this, e);
							if (e.Ignore)
								return;
						}

						UIEscape(e.State == KeyStates.Shift);
					}
					break;
			}
		}
	}
}
