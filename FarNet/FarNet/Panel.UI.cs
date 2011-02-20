
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
	public partial class Panel
	{
		/// <summary>
		/// Delete action.
		/// </summary>
		public void UIDelete(bool shift)
		{
			if (Explorer != null)
			{
				if (0 == (Explorer.Function & ExplorerFunctions.DeleteFiles))
					return;

				var args = new DeleteFilesArgs(ExplorerModes.None, SelectedFiles);
				Explorer.DeleteFiles(args);
				if (args.Result == JobResult.Ignore)
					return;

				Update(false); //???? selection if not all deleted
				Redraw();
			}
			else
			{
				var args = new FilesEventArgs();
				args.Files = SelectedFiles;
				args.Move = shift;
				if (args.Files.Count == 0)
					return;

				if (DeleteFiles != null)
				{
					DeleteFiles(this, args);
					return; //???? Until DeleteFiles exists and defined then UIDeleteFiles is not used.
				}

				UIDeleteFiles(args);

				Update(false); //???? selection if not all deleted
				Redraw();
			}
		}
		/// <summary>
		/// Deletes the files.
		/// </summary>
		public virtual void UIDeleteFiles(FilesEventArgs args)
		{
			if (args != null)
				args.Ignore = true; //????
		}
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
		public void UIEscape(bool all)
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
		/// Opens the file in the editor.
		/// </summary>
		/// <remarks>
		/// The default method calls <see cref="FarNet.Explorer.ExportFile"/>  to get a temporary file to edit
		/// and <see cref="FarNet.Explorer.ImportFile"/> to save changes when the editor closes.
		/// The explorer should have at least export implemented.
		/// </remarks>
		public virtual void UIEditFile(FarFile file)
		{
			if (file == null)
				return;

			if (Explorer == null) //???? kill when explorer is mandatory
				return;

			if (0 == (Explorer.Function & ExplorerFunctions.ExportFile))
				return;

			var temp = Far.Net.TempName();

			// export
			Log.Source.TraceInformation("ExportFile");
			var argsExport = new ExportFileArgs(ExplorerModes.Edit, file, temp);
			Explorer.ExportFile(argsExport);
			if (argsExport.Result != JobResult.Done)
				return;

			// rename
			if (!string.IsNullOrEmpty(argsExport.FileNameExtension))
			{
				string temp2 = temp + argsExport.FileNameExtension;
				File.Move(temp, temp2);
				temp = temp2;
			}

			// editor
			var editor = Far.Net.CreateEditor();
			editor.DeleteSource = DeleteSource.File;
			editor.FileName = temp;
			editor.Title = file.Name;

			// future
			if (argsExport.CanImport)
			{
				editor.Closed += delegate //???? to use Saved (Far 3), update docs.
				{
					if (editor.TimeOfSave == DateTime.MinValue)
						return;

					var argsImport = new ImportFileArgs(ExplorerModes.Edit, file, temp);
					Log.Source.TraceInformation("ImportFile");
					Explorer.ImportFile(argsImport);
				};
			}
			else
			{
				// to lock
				editor.IsLocked = true;
			}

			// go
			editor.Open();
		}
		/// <summary>
		/// Opens the file in the viewer.
		/// </summary>
		/// <returns>True if it's done.</returns>
		/// <remarks>
		/// The default method calls <see cref="FarNet.Explorer.ExportFile"/> to get a temporary file to view.
		/// The explorer should have it implemented.
		/// </remarks>
		public virtual void UIViewFile(FarFile file)
		{
			if (file == null)
				return;

			if (Explorer == null) //???? kill when explorer is mandatory
				return;

			if (0 == (Explorer.Function & ExplorerFunctions.ExportFile))
				return;

			var temp = Far.Net.TempName();

			var args = new ExportFileArgs(ExplorerModes.View, file, temp);
			Log.Source.TraceInformation("Explorer.ExportFile");
			Explorer.ExportFile(args);
			if (args.Result != JobResult.Done)
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
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
						if (RealNames)
							return;

						var file = CurrentFile;
						if (file != null && !file.IsDirectory)
						{
							e.Ignore = true;
							UIViewFile(file);
						}
					}
					break;
				case VKeyCode.F4:
					if (e.State == KeyStates.None)
					{
						if (RealNames)
							return;

						var file = CurrentFile;
						if (file != null && !file.IsDirectory)
						{
							e.Ignore = true;
							UIEditFile(file);
						}
					}
					break;
				case VKeyCode.Delete:
					{
						if (Far.Net.CommandLine.Length > 0)
							return;
						goto case VKeyCode.F8;
					}
				case VKeyCode.F8:
					{
						if (e.State == KeyStates.None || e.State == KeyStates.Shift)
						{
							if (DeleteFiles != null) //???? to replace by explorers
								return;

							if (RealNames && RealNamesDeleteFiles)
								return;

							e.Ignore = true;
							UIDelete(e.State == KeyStates.Shift);
						}
						return;
					}
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
