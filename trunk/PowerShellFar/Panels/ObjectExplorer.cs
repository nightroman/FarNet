
/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// .NET objects explorer.
	/// </summary>
	public sealed class ObjectExplorer : FormatExplorer
	{
		const string TypeIdString = "07e4dde7-e113-4622-b2e9-81cf3cda927a";
		///
		public ObjectExplorer()
			: base(new Guid(TypeIdString))
		{
			FileComparer = new FileDataComparer();
			Functions =
				ExplorerFunctions.AcceptFiles |
				ExplorerFunctions.AcceptOther |
				ExplorerFunctions.DeleteFiles |
				ExplorerFunctions.CreateFile |
				ExplorerFunctions.ExportFile;
		}
		///
		public override Panel DoCreatePanel()
		{
			return new ObjectPanel(this);
		}
		///
		public override void DoAcceptFiles(AcceptFilesEventArgs args)
		{
			((ObjectPanel)Panel).DoAcceptFiles(args);
		}
		///
		public override void DoDeleteFiles(DeleteFilesEventArgs args)
		{
			if (args == null) return;

			if (args.UI && (Far.Net.Confirmations & FarConfirmations.Delete) != 0)
			{
				if (Far.Net.Message("Remove object(s) from the panel?", Res.Remove, MsgOptions.None, new string[] { Res.Remove, Res.Cancel }) != 0)
				{
					args.Result = JobResult.Ignore;
					return;
				}
			}

			foreach (FarFile file in args.Files)
				Cache.Remove(file);
		}
		///
		public override void DoExportFile(ExportFileEventArgs args)
		{
			if (args == null) return;

			// use existing file
			string filePath = My.PathEx.TryGetFilePath(args.File.Data);
			if (filePath != null)
			{
				args.UseFileName = filePath;
				args.CanImport = true;
				return;
			}
			
			// write data
			A.WriteFormatList(args.File.Data, args.FileName);
		}
		///
		public override void DoAcceptOther(AcceptOtherEventArgs args)
		{
			if (args == null) return;
			
			var panel = args.Panel as ObjectPanel;
			if (panel == null || panel.IsActive)
			{
				args.Result = JobResult.Ignore;
				return;
			}
			
			panel.AddObjects(A.Psf.InvokeCode("Get-FarItem -Selected")); //????? crap. but...
		}
		/// <summary>
		/// Gets or sets the script getting raw file data objects.
		/// Variables: <c>$this</c> is this explorer.
		/// </summary>
		/// <remarks>
		/// The script returns raw data to be represented as files with the data attached.
		/// It should not operate directly on existing or new files, it is done internally.
		/// <para>
		/// Normally it is used together with custom columns
		/// otherwise default formatting is not always suitable.
		/// </para>
		/// </remarks>
		/// <example>Panel-Job-.ps1, Panel-Process-.ps1</example>
		public ScriptBlock AsGetData { get; set; }
		///
		public override void DoCreateFile(CreateFileEventArgs args)
		{
			var panel = args.Panel as ObjectPanel;
			if (panel == null)
			{
				args.Result = JobResult.Ignore;
				return;
			}

			// prompt for a command
			string code = Far.Net.MacroState == MacroState.None ? A.Psf.InputCode() : Far.Net.Input(null);
			if (string.IsNullOrEmpty(code))
			{
				args.Result = JobResult.Ignore;
				return;
			}

			// invoke the command
			Collection<PSObject> values = A.Psf.InvokeCode(code);
			if (values.Count == 0)
			{
				args.Result = JobResult.Ignore;
				return;
			}

			// add the objects
			panel.AddObjects(values);

			// post the first object
			args.PostData = values[0];
		}
	}
}
