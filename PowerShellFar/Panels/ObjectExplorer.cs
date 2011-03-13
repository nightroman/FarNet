
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
				ExplorerFunctions.ExportFile |
				ExplorerFunctions.OpenFile;
		}
		///
		public override Panel DoCreatePanel()
		{
			return new ObjectPanel(this);
		}
		///
		public override void DoAcceptFiles(AcceptFilesEventArgs args)
		{
			if (args == null) return;
			
			var panel = args.Panel as ObjectPanel;
			if (panel == null)
				args.Result = JobResult.Ignore;
			else
				panel.AddObjects(args.FilesData);
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
			
			// text
			args.UseText = A.InvokeFormatList(args.File.Data);
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
			
			panel.AddObjects(A.InvokeCode("Get-FarItem -Selected")); //????? crap. but...
		}
		/// <summary>
		/// Gets or sets the script getting raw file data objects.
		/// Variables: <c>$this</c> is this explorer, <c>$_</c> is <see cref="ExplorerEventArgs"/>.
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
			if (args == null) return;
			
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
			Collection<PSObject> values = A.InvokeCode(code);
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
		internal override object GetData(ExplorerEventArgs args)
		{
			if (AsGetData != null)
				return A.InvokeScript(AsGetData, this, args);

			var panel = args.Panel as ObjectPanel;
			var Files = Cache;
			try
			{
				if (panel != null)
				{
					//???? it works but looks like a hack
					if (panel.UserWants != UserAction.CtrlR && _AddedValues == null && (Map != null || Files.Count > 0 && Files[0] is SetFile))
						return Files;
				}

				if (Map == null || Columns == null)
				{
					if (Files.Count == 0)
						return _AddedValues ?? new Collection<PSObject>();

					var result = new Collection<PSObject>();
					foreach (FarFile file in Files)
						result.Add(PSObject.AsPSObject(file.Data));
					if (_AddedValues != null)
						foreach (PSObject value in _AddedValues)
							result.Add(value);

					return result;
				}

				// _100330_191639
				if (_AddedValues == null)
					return Files;

				var map = Map;
				var files = new List<FarFile>(_AddedValues.Count);
				foreach (PSObject value in _AddedValues)
					files.Add(new MapFile(value, map));

				return files;
			}
			finally
			{
				_AddedValues = null;
			}
		}
		Collection<PSObject> _AddedValues;
		internal Collection<PSObject> AddedValues
		{
			get { return _AddedValues ?? (_AddedValues = new Collection<PSObject>()); }
		}
		///
		public override void DoOpenFile(OpenFileEventArgs args)
		{
			if (args == null) return;

			PSObject psData = PSObject.AsPSObject(args.File.Data);

			// case: linear type: do not enter, there is no much sense
			if (Converter.IsLinearType(psData.BaseObject.GetType()))
				return;

			// case: enumerable (string is excluded by linear type case)
			IEnumerable ie = Cast<IEnumerable>.From(args.File.Data);
			if (ie != null)
			{
				ObjectPanel op = new ObjectPanel();
				op.AddObjects(ie);
				op.OpenChild(args.Panel);
				return;
			}

			// case: group
			PSPropertyInfo pi = psData.Properties["Group"];
			if (pi != null && pi.Value is IEnumerable && !(pi.Value is string))
			{
				ObjectPanel op = new ObjectPanel();
				op.AddObjects(pi.Value as IEnumerable);
				op.OpenChild(args.Panel);
				return;
			}

			// case: ManagementClass
			if (psData.BaseObject.GetType().FullName == "System.Management.ManagementClass")
			{
				pi = psData.Properties[Word.Name];
				if (pi != null && pi.Value != null)
				{
					var values = A.InvokeCode("Get-WmiObject -Class $args[0] -ErrorAction SilentlyContinue", pi.Value.ToString());
					ObjectPanel op = new ObjectPanel();
					op.AddObjects(values);
					op.OpenChild(args.Panel);
					return;
				}
			}

			// open lookup/members
			var panel = args.Panel as ObjectPanel;
			if (panel != null)
			{
				panel.OpenFileMembers(args.File);
				return;
			}

			// open members
			var explorer = new MemberExplorer(args.File.Data);
			explorer.OpenPanelChild(args.Panel);
		}
	}
}
