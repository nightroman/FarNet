
/*
PowerShellFar module for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using FarNet;

namespace PowerShellFar
{
	/// <summary>
	/// Panel exploring any user objects.
	/// </summary>
	public sealed class UserPanel : ObjectPanel
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public UserPanel()
		{ }
		//! Do nothing instead of removing objects. A script is used to do the job. 
		internal override void DoDeleteFiles(FilesEventArgs args)
		{
			args.Ignore = true;
		}
		#region WriteFile
		/// <summary>
		/// Gets or sets the script to write a file.
		/// Variables: <c>$this</c> is this panel, <c>$_</c> is <see cref="WriteEventArgs"/>.
		/// </summary>
		public ScriptBlock AsWriteFile { get; set; }
		/// <summary>
		/// Writes the file using <see cref="AsWriteFile"/> or the default method.
		/// </summary>
		internal override void WriteFile(FarFile file, string path)
		{
			if (AsWriteFile != null)
			{
				var args = new WriteEventArgs() { File = file, Path = path };
				A.InvokeScriptReturnAsIs(AsWriteFile, this, args);
				if (args.Result != JobResult.Default)
					return;
			}
			
			base.WriteFile(file, path);
		}
		#endregion
		#region AsFiles
		/// <summary>
		/// Gets or sets the script called to get the objects to be wrapped by files.
		/// Variables: <c>$this</c> is this panel.
		/// </summary>
		/// <remarks>
		/// The script returns the objects to be shown in the panel as files.
		/// It should not operate directly on existing or new panel files, it is done internally.
		/// <para>
		/// Normally this handler is used together with custom <see cref="FormatPanel.Columns"/>
		/// otherwise default data formatting is not always suitable.
		/// </para>
		/// </remarks>
		/// <example>Panel-Job-.ps1, Panel-Process-.ps1</example>
		public ScriptBlock AsFiles { get; set; }
		internal override object GetData()
		{
			if (AsFiles == null)
				return base.GetData();
			else
				return A.InvokeScript(AsFiles, this, null);
		}
		#endregion
	}

	/// <summary>
	/// Panel file event arguments.
	/// </summary>
	public class FileEventArgs : EventArgs
	{
		/// <summary>
		/// Job result.
		/// </summary>
		public JobResult Result { get; set; }
		/// <summary>
		/// Panel file instance being processed.
		/// </summary>
		public FarFile File { get; set; }
	}

	/// <summary>
	/// Arguments of Write event.
	/// </summary>
	public class WriteEventArgs : FileEventArgs
	{
		/// <summary>
		/// File system path where data are written to.
		/// </summary>
		public string Path { get; set; }
	}

}
