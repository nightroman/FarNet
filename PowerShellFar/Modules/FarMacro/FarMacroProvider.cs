
/*
PowerShellFar module for Far Manager
Copyright (c) 2006-2012 Roman Kuzmin
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Text;
using FarNet;

namespace FarMacro
{
	[CmdletProvider("FarMacro", ProviderCapabilities.ShouldProcess)]
	public sealed class FarMacroProvider : NavigationCmdletProvider, IContentCmdletProvider
	{
		#region Private

		SortedList<MacroArea, AreaItem> _Areas;
		SortedList<MacroArea, AreaItem> Areas
		{
			get
			{
				if (_Areas == null)
				{
					//_100201_110148 ~ MacroArea add/remove
					_Areas = new SortedList<MacroArea, AreaItem>();
					// fake areas
					_Areas.Add(MacroArea.Vars, new AreaItem(MacroArea.Vars, "Global variables"));
					_Areas.Add(MacroArea.Consts, new AreaItem(MacroArea.Consts, "Global constants"));
					_Areas.Add(MacroArea.Common, new AreaItem(MacroArea.Common, "Lowest priority macros used everywhere"));
					// proper areas
					_Areas.Add(MacroArea.Other, new AreaItem(MacroArea.Other, "Screen capturing mode"));
					_Areas.Add(MacroArea.Shell, new AreaItem(MacroArea.Shell, "File panels"));
					_Areas.Add(MacroArea.Viewer, new AreaItem(MacroArea.Viewer, "File viewer"));
					_Areas.Add(MacroArea.Editor, new AreaItem(MacroArea.Editor, "File editor"));
					_Areas.Add(MacroArea.Dialog, new AreaItem(MacroArea.Dialog, "Dialog boxes"));
					_Areas.Add(MacroArea.Search, new AreaItem(MacroArea.Search, "Quick file search"));
					_Areas.Add(MacroArea.Disks, new AreaItem(MacroArea.Disks, "Drive selection menu"));
					_Areas.Add(MacroArea.MainMenu, new AreaItem(MacroArea.MainMenu, "Main menu"));
					_Areas.Add(MacroArea.Menu, new AreaItem(MacroArea.Menu, "Other menus"));
					_Areas.Add(MacroArea.Help, new AreaItem(MacroArea.Help, "Help system"));
					_Areas.Add(MacroArea.Info, new AreaItem(MacroArea.Info, "Informational panel"));
					_Areas.Add(MacroArea.QView, new AreaItem(MacroArea.QView, "Quick view panel"));
					_Areas.Add(MacroArea.Tree, new AreaItem(MacroArea.Tree, "Folder tree panel"));
					_Areas.Add(MacroArea.FindFolder, new AreaItem(MacroArea.FindFolder, "Folder search panel"));
					_Areas.Add(MacroArea.UserMenu, new AreaItem(MacroArea.UserMenu, "User menu"));
					_Areas.Add(MacroArea.AutoCompletion, new AreaItem(MacroArea.AutoCompletion, "Lowest priority macros used everywhere"));
				}
				return _Areas;
			}
		}

		static bool IsValidName(MacroArea area, string name)
		{
			if (string.IsNullOrEmpty(name))
				return false;

			// allow all names for scalars; but perhaps Vars should be like %%*
			if (area == MacroArea.Consts || area == MacroArea.Vars)
				return true;

			int code = Far.Net.NameToKey(name);
			if (code >= 0)
				return true;

			if (name[0] != '~')
				return false;

			code = Far.Net.NameToKey(name.Substring(1));
			return code >= 0;
		}

		static bool IsValidWay(Way way)
		{
			// _100201_110148 ~ MacroArea min/max change
			if ((int)way.Area < (int)MacroArea.Vars || (int)way.Area > (int)MacroArea.AutoCompletion)
				return false;

			if (way.Name == null)
				return true;

			return IsValidName(way.Area, way.Name);
		}

		static Way NewWay(string path)
		{
			Way way = new Way(path);
			if (!IsValidWay(way))
				throw new ArgumentException("Invalid name.");

			return way;
		}

		#endregion

		#region Drive

		protected override Collection<PSDriveInfo> InitializeDefaultDrives()
		{
			var r = new Collection<PSDriveInfo>();
			r.Add(new PSDriveInfo("FarMacro", ProviderInfo, string.Empty, "Far Manager macros", null));
			return r;
		}

		protected override PSDriveInfo NewDrive(PSDriveInfo drive)
		{
			if (drive == null)
				throw new ArgumentNullException("drive");

			// the only drive
			if (drive.Root.Length == 0)
				return drive;

			WriteError(new ErrorRecord(new NotSupportedException("New drives are not supported."), "Drive", ErrorCategory.InvalidOperation, drive));
			return null;
		}

		protected override PSDriveInfo RemoveDrive(PSDriveInfo drive)
		{
			if (drive == null)
				throw new ArgumentNullException("drive");

			// the only drive
			if (drive.Root.Length == 0)
				return drive;

			throw new InvalidOperationException("Invalid root path.");
		}

		#endregion

		#region Item

		protected override void GetItem(string path)
		{
			Way way = new Way(path);

			if (way.Area == MacroArea.None)
			{
				WriteItemObject(PSDriveInfo, path, true);
				return;
			}

			if (way.Name == null)
			{
				WriteItemObject(Areas[way.Area], path, true);
				return;
			}

			if (way.Area == MacroArea.Consts)
			{
				WriteItemObject(new DictionaryEntry(way, Far.Net.Macro.GetConstant(way.Name)), path, true);
				return;
			}

			if (way.Area == MacroArea.Vars)
			{
				WriteItemObject(new DictionaryEntry(way, Far.Net.Macro.GetVariable(way.Name)), path, true);
				return;
			}

			Macro macro = Far.Net.Macro.GetMacro(way.Area, way.Name);
			if (macro == null)
				throw new FileNotFoundException(path);

			WriteItemObject(macro, path, false);
		}

		protected override void SetItem(string path, object value)
		{
			Way way = new Way(path);
			if (way.Name == null)
				throw new InvalidOperationException("Value can be set only to a macro item.");

			if (value == null)
				throw new ArgumentNullException("value");

			Macro macro = Far.Net.Macro.GetMacro(way.Area, way.Name);
			if (macro == null)
			{
				macro = new Macro();
				macro.Area = way.Area;
				macro.Name = way.Name;
				macro.Sequence = value.ToString();
			}

			Far.Net.Macro.Install(macro);
		}

		protected override bool ItemExists(string path)
		{
			Way way = new Way(path);
			if (way.Area == MacroArea.None)
				return true;

			if (way.Name == null)
				return Areas.ContainsKey(way.Area);

			if (way.Area == MacroArea.Consts)
				return Far.Net.Macro.GetConstant(way.Name) != null;

			if (way.Area == MacroArea.Vars)
				return Far.Net.Macro.GetVariable(way.Name) != null;

			Macro macro = Far.Net.Macro.GetMacro(way.Area, way.Name);
			return macro != null;
		}

		protected override bool IsValidPath(string path)
		{
			Way way = new Way(path);
			return IsValidWay(way);
		}

		#endregion

		#region Container

		protected override void GetChildItems(string path, bool recurse)
		{
			Way way = new Way(path);

			if (way.Area == MacroArea.None)
			{
				foreach (string name in Far.Net.Macro.GetNames(MacroArea.None))
				{
					try
					{
						MacroArea area = (MacroArea)Enum.Parse(typeof(MacroArea), name, true);
						WriteItemObject(Areas[area], area.ToString(), true);

						if (recurse)
							GetChildItems(area.ToString(), true);
					}
					catch (ArgumentException)
					{ }
				}
			}
			else if (way.Area == MacroArea.Consts)
			{
				//! DictionaryEntry is well formatted by default
				foreach (string name in Far.Net.Macro.GetNames(MacroArea.Consts))
					WriteItemObject(new DictionaryEntry(name, Far.Net.Macro.GetConstant(name)), "Consts\\" + name, false);
			}
			else if (way.Area == MacroArea.Vars)
			{
				//! DictionaryEntry is well formatted by default
				foreach (string name in Far.Net.Macro.GetNames(MacroArea.Vars))
					WriteItemObject(new DictionaryEntry(name, Far.Net.Macro.GetVariable(name)), "Vars\\" + name, false);
			}
			else if (way.Name == null)
			{
				foreach (string name in Far.Net.Macro.GetNames(way.Area))
					WriteItemObject(Far.Net.Macro.GetMacro(way.Area, name), way.Area + "\\" + name, false);
			}
			else
			{
				Macro macro = Far.Net.Macro.GetMacro(way.Area, way.Name);
				if (macro == null)
					throw new FileNotFoundException(path);

				WriteItemObject(macro, way.Area + "\\" + way.Name, false);
			}
		}

		protected override void GetChildNames(string path, ReturnContainers returnContainers)
		{
			Way way = new Way(path);

			if (way.Area == MacroArea.None)
			{
				foreach (string name in Far.Net.Macro.GetNames(MacroArea.None))
				{
					try
					{
						MacroArea area = (MacroArea)Enum.Parse(typeof(MacroArea), name, true);
						WriteItemObject(area.ToString(), area.ToString(), true);
					}
					catch (ArgumentException)
					{ }
				}
			}
			else if (way.Name == null)
			{
				foreach (string name in Far.Net.Macro.GetNames(way.Area))
					WriteItemObject(name, path, false);
			}
		}

		// We do not allow removing an area with children. If we say true in here then the core
		// will show an extra prompt which is useless. That is why we cheat in here.
		protected override bool HasChildItems(string path)
		{
			return false;
		}

		static object ConvertItemValue(string itemTypeName, object newItemValue)
		{
			if (string.IsNullOrEmpty(itemTypeName))
				return newItemValue;

			switch ((RegistryValueKind)Kit.ConvertTo(itemTypeName, typeof(RegistryValueKind)))
			{
				case RegistryValueKind.MultiString:
					return Kit.ConvertTo(newItemValue, typeof(string[]));
				case RegistryValueKind.DWord:
					return Kit.ConvertTo(newItemValue, typeof(int));
				case RegistryValueKind.QWord:
					return Kit.ConvertTo(newItemValue, typeof(long));
				default:
					throw new ModuleException(string.Format(null,
						"Invalid type. Valid types: {0}, {1}, {2}.",
						RegistryValueKind.MultiString, RegistryValueKind.DWord, RegistryValueKind.QWord));
			}
		}

		protected override void NewItem(string path, string itemTypeName, object newItemValue)
		{
			Way way = NewWay(path);
			object value = newItemValue ?? string.Empty;

			if (way.Area == MacroArea.Consts && way.Name != null)
			{
				Far.Net.Macro.InstallConstant(way.Name, ConvertItemValue(itemTypeName, value));
				return;
			}

			if (way.Area == MacroArea.Vars && way.Name != null)
			{
				Far.Net.Macro.InstallVariable(way.Name, ConvertItemValue(itemTypeName, value));
				return;
			}

			Macro macro = new Macro();
			macro.Area = way.Area;
			macro.Name = way.Name;
			macro.Sequence = value.ToString();

			Far.Net.Macro.Install(macro);
		}

		protected override void CopyItem(string path, string copyPath, bool recurse)
		{
			Way way = new Way(path);
			if (way.Name == null)
				throw new InvalidOperationException("Areas cannot be copied.");

			Way dst = new Way(copyPath);
			if (dst.Area == MacroArea.Consts)
			{
				if (way.Area != MacroArea.Consts)
					throw new InvalidOperationException(Res.InvalidDestinationPath);

				Far.Net.Macro.InstallConstant(dst.Name, Far.Net.Macro.GetConstant(way.Name));
				return;
			}
			else if (dst.Area == MacroArea.Vars)
			{
				if (way.Area != MacroArea.Vars)
					throw new InvalidOperationException(Res.InvalidDestinationPath);

				Far.Net.Macro.InstallVariable(dst.Name, Far.Net.Macro.GetVariable(way.Name));
				return;
			}
			else if (dst.Area == MacroArea.None)
			{
				throw new InvalidOperationException(Res.InvalidDestinationPath);
			}

			Macro macro = Far.Net.Macro.GetMacro(way.Area, way.Name);
			if (macro == null)
				throw new FileNotFoundException("Macro is not found: " + path);

			macro.Area = dst.Area;
			if (!string.IsNullOrEmpty(dst.Name))
				macro.Name = dst.Name;

			Far.Net.Macro.Install(macro);
		}

		protected override void RemoveItem(string path, bool recurse)
		{
			Way way = new Way(path);

			if (way.Name == null && Far.Net.Macro.GetNames(way.Area).Length > 0)
				throw new RuntimeException("Cannot remove not empty area.");

			if (!ShouldProcess(path, "Remove"))
				return;

			Far.Net.Macro.Remove(way.Area, way.Name);
		}

		#endregion

		#region Navigation

		protected override bool IsItemContainer(string path)
		{
			Way way = new Way(path);
			return way.Name == null;
		}

		protected override string GetChildName(string path)
		{
			Way way = new Way(path);
			if (way.Area == MacroArea.None)
				return string.Empty;
			else if (way.Name == null)
				return way.Area.ToString();
			else
				return way.Name;
		}

		protected override string GetParentPath(string path, string root)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			// If root is specified then the path has to contain
			// the root. If not nothing should be returned
			if (!String.IsNullOrEmpty(root))
				if (!path.Contains(root))
					return null;

			Way way = new Way(path);
			if (way.Area == MacroArea.None)
				return null;
			else if (way.Name == null)
				return string.Empty;
			else
				return way.Area.ToString();
		}

		protected override void MoveItem(string path, string destination)
		{
			Way way = new Way(path);
			if (way.Name == null || way.Area == MacroArea.Consts || way.Area == MacroArea.Vars)
				throw new InvalidOperationException("The item cannot be moved.");

			Way dst = new Way(destination);
			if (dst.Name != null || dst.Area == MacroArea.None || dst.Area == MacroArea.Consts || dst.Area == MacroArea.Vars)
				throw new InvalidOperationException("Invalid destination: " + destination);

			Macro macro = Far.Net.Macro.GetMacro(way.Area, way.Name);
			if (macro == null)
				throw new InvalidOperationException("Source macro is not found.");

			macro.Area = dst.Area;
			Far.Net.Macro.Install(macro);
			Far.Net.Macro.Remove(way.Area, way.Name);
		}

		protected override void RenameItem(string path, string newName)
		{
			Way way = new Way(path);

			if (!IsValidName(way.Area, newName))
				throw new ArgumentException("Invalid new name: " + newName);

			if (way.Name == null)
				throw new InvalidOperationException("This item cannot be renamed.");

			if (way.Area == MacroArea.Consts)
			{
				// noop if names are identical
				if (newName == way.Name)
					return;

				//! 3 steps in exactly this order
				object value = Far.Net.Macro.GetConstant(way.Name);
				Far.Net.Macro.Remove(MacroArea.Consts, way.Name);
				Far.Net.Macro.InstallConstant(newName, value);
				return;
			}

			if (way.Area == MacroArea.Vars)
			{
				// noop if names are identical
				if (newName == way.Name)
					return;

				//! 3 steps in exactly this order
				object value = Far.Net.Macro.GetVariable(way.Name);
				Far.Net.Macro.Remove(MacroArea.Vars, way.Name);
				Far.Net.Macro.InstallVariable(newName, value);
				return;
			}

			Macro macro = Far.Net.Macro.GetMacro(way.Area, way.Name);
			if (macro == null)
				throw new InvalidOperationException("Source macro is not found.");

			macro.Name = newName;
			Far.Net.Macro.Install(macro);
			Far.Net.Macro.Remove(way.Area, way.Name);
			Far.Net.Macro.Load();
		}

		#endregion Navigation

		#region Content

		public void ClearContent(string path)
		{
			Way way = new Way(path);

			if (way.Area == MacroArea.Consts)
			{
				Far.Net.Macro.InstallConstant(way.Name, string.Empty);
				return;
			}

			if (way.Area == MacroArea.Vars)
			{
				Far.Net.Macro.InstallVariable(way.Name, string.Empty);
				return;
			}

			Macro macro = Far.Net.Macro.GetMacro(way.Area, way.Name);
			if (macro == null)
				throw new FileNotFoundException("Macro is not found: " + path);

			macro.Sequence = string.Empty;
			Far.Net.Macro.Install(macro);
		}

		public object ClearContentDynamicParameters(string path)
		{
			return null;
		}

		public IContentReader GetContentReader(string path)
		{
			Way way = new Way(path);

			if (way.Area == MacroArea.Consts)
				return new Reader(Far.Net.Macro.GetConstant(way.Name).ToString());
			else if (way.Area == MacroArea.Vars)
				return new Reader(Far.Net.Macro.GetVariable(way.Name).ToString());

			Macro macro = Far.Net.Macro.GetMacro(way.Area, way.Name);
			if (macro == null)
				throw new FileNotFoundException("Macro is not found: " + path);

			return new Reader(macro.Sequence);
		}

		public object GetContentReaderDynamicParameters(string path)
		{
			return null;
		}

		public IContentWriter GetContentWriter(string path)
		{
			Way way = new Way(path);

			if (way.Area == MacroArea.Consts || way.Area == MacroArea.Vars)
				return new Writer(way.Area, way.Name);

			Macro macro = Far.Net.Macro.GetMacro(way.Area, way.Name);
			if (macro == null)
				throw new FileNotFoundException("Macro is not found: " + path);

			return new Writer(macro);
		}

		public object GetContentWriterDynamicParameters(string path)
		{
			return null;
		}

		#endregion Content Methods

	}

	class Reader : IContentReader
	{
		string Value;
		bool Done;

		public Reader(string value)
		{
			Value = value;
		}

		public void Close()
		{ }

		public IList Read(long readCount)
		{
			if (readCount > 0)
				throw new NotSupportedException();

			var r = new List<string>(1);
			if (!Done)
			{
				r.Add(Value);
				Done = true;
			}

			return r;
		}

		public void Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}
	}

	class Writer : IContentWriter
	{
		Macro Macro;

		MacroArea Area;
		string Name;

		public Writer(Macro macro)
		{
			Macro = macro;
		}

		public Writer(MacroArea area, string name)
		{
			Area = area;
			Name = name;
		}

		public void Close()
		{ }

		public void Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public IList Write(IList content)
		{
			if (content == null)
				throw new ArgumentNullException("content");

			StringBuilder sb = new StringBuilder();
			foreach (object line in content)
			{
				if (line != null)
				{
					if (sb.Length > 0)
						sb.AppendLine();

					sb.Append(line.ToString());
				}
			}

			if (Area == MacroArea.Consts)
			{
				Far.Net.Macro.InstallConstant(Name, sb.ToString());
			}
			else if (Area == MacroArea.Vars)
			{
				Far.Net.Macro.InstallVariable(Name, sb.ToString());
			}
			else
			{
				Macro.Sequence = sb.ToString();
				Far.Net.Macro.Install(Macro);
			}

			return content;
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}
	}
}
