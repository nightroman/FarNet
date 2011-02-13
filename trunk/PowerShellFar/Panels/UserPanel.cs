
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

		#region Delete
		ScriptBlock _Delete;

		/// <summary>
		/// Handler to delete files. $_ = <see cref="FilesEventArgs"/>
		/// </summary>
		public void SetDelete(ScriptBlock handler)
		{
			_Delete = handler;
		}

		internal override void DeleteFiles2(IList<FarFile> files, bool shift)
		{
			//! do not call base which removes items
			if (_Delete != null)
				A.InvokeScriptReturnAsIs(_Delete, this, new FilesEventArgs() { Files = files, Move = shift });
		}

		#endregion

		#region Write
		ScriptBlock _Write;

		/// <summary>
		/// Handler to write an object data to a file. $_ = <see cref="WriteEventArgs"/>
		/// </summary>
		public void SetWrite(ScriptBlock handler)
		{
			_Write = handler;
		}

		internal override void WriteFile(FarFile file, string path)
		{
			if (_Write == null)
				base.WriteFile(file, path);
			else
				A.InvokeScriptReturnAsIs(_Write, this, new WriteEventArgs(file, path));
		}

		#endregion

		#region GetData
		ScriptBlock _GetData;

		/// <summary>
		/// Sets the handler called to get the objects.
		/// </summary>
		/// <remarks>
		/// The handler returns the objects to be shown in the panel.
		/// It should not operate directly on existing or new panel files, it is done internally.
		/// <para>
		/// Normally this handler is used together with custom <see cref="FormatPanel.Columns"/>
		/// otherwise default data formatting will not always be suitable.
		/// </para>
		/// </remarks>
		/// <example>Panel-Job-.ps1, Panel-Process-.ps1</example>
		public void SetGetData(ScriptBlock handler)
		{
			_GetData = handler;
		}

		internal override object GetData()
		{
			if (_GetData == null)
				return base.GetData();
			else
				return A.InvokeScript(_GetData, this, null);
		}

		#endregion
	}

	/// <summary>
	/// Panel file event arguments.
	/// </summary>
	public class FileEventArgs : EventArgs
	{
		///
		public FileEventArgs(FarFile file)
		{
			_File = file;
		}

		///
		public FileEventArgs(FarFile file, bool alternative)
		{
			_File = file;
			_Alternative = alternative;
		}

		FarFile _File;
		/// <summary>
		/// Panel file instance being processed.
		/// </summary>
		public FarFile File
		{
			get { return _File; }
		}

		bool _Alternative;
		/// <summary>
		/// Alternative action flag.
		/// </summary>
		public bool Alternative
		{
			get { return _Alternative; }
			set { _Alternative = value; }
		}
	}

	/// <summary>
	/// Arguments of Write event.
	/// </summary>
	public class WriteEventArgs : FileEventArgs
	{
		///
		public WriteEventArgs(FarFile file, string path)
			: base(file)
		{
			_path = path;
		}

		string _path;
		/// <summary>
		/// File system path where data are written to.
		/// </summary>
		public string Path
		{
			get { return _path; }
		}
	}

}
