/*
PowerShellFar plugin for Far Manager
Copyright (C) 2006-2009 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Collections.Generic;
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
		{
			Panel.Info.Title = string.Empty;
		}

		internal override bool CanClose()
		{
			return true;
		}

		#region Delete
		ScriptBlock _Delete;

		/// <summary>
		/// Handler to delete files. $_ = <see cref="FilesEventArgs"/>
		/// </summary>
		public void SetDelete(ScriptBlock handler)
		{
			_Delete = handler;
		}

		internal override void DeleteFiles(IList<FarFile> files, bool shift)
		{
			//! do not call base which removes items
			if (_Delete != null)
				InvokeThisScript(_Delete, new FilesEventArgs(files, OperationModes.None, shift));
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
				InvokeThisScript(_Write, new WriteEventArgs(file, path));
		}

		#endregion

		#region GetData
		ScriptBlock _GetData;

		/// <summary>
		/// Handler called to get data.
		/// </summary>
		/// <remarks>
		/// This handler should create <see cref="IPluginPanel.Files"/> (recreate completely or update existing).
		/// <para>
		/// One possible scenario is to clear the file list and then call <see cref="ObjectPanel.AddObjects"/> one or more times.
		/// </para>
		/// <para>
		/// In another scenario new files may be created and added to the list.
		/// In this case you should create files by <see cref="ObjectPanel.NewFile"/> method.
		/// </para>
		/// </remarks>
		/// <example>Panel-Process-.ps1</example>
		public void SetGetData(ScriptBlock handler)
		{
			_GetData = handler;
		}

		internal override bool OnGettingData()
		{
			if (_GetData == null)
			{
				return base.OnGettingData();
			}
			else
			{
				InvokeThisScript(_GetData, null);
				return Map != null;
			}
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
		/// File to be processed.
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
		/// File path.
		/// </summary>
		public string Path
		{
			get { return _path; }
		}
	}

}
