/*
PowerShellFar plugin for Far Manager
Copyright (c) 2006 Roman Kuzmin
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Management.Automation.Runspaces;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace PowerShellFar
{
	///
	[Serializable]
	public class PluginException : FarNet.PluginException
	{
		///
		public PluginException() { }
		///
		public PluginException(string message) : base(message) { }
		///
		public PluginException(string message, Exception innerException) : base(message, innerException) { }
		///
		protected PluginException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	/// <summary>
	/// Parameters. Use them to avoid typos.
	/// </summary>
	static class Prm
	{
		public const string
			Confirm = "Confirm",
			Force = "Force",
			Recurse = "Recurse";

		public static CommandParameter EAContinue { get { return new CommandParameter("ErrorAction", ActionPreference.Continue); } }
		public static CommandParameter EASilentlyContinue { get { return new CommandParameter("ErrorAction", ActionPreference.SilentlyContinue); } }
		public static CommandParameter EAStop { get { return new CommandParameter("ErrorAction", ActionPreference.Stop); } }
	}

	/// <summary>
	/// Helper methods.
	/// </summary>
	static class Kit
	{
		/// <summary>
		/// Formats using the current culture.
		/// </summary>
		public static string Format(string format, params object[] args)
		{
			return string.Format(CultureInfo.CurrentCulture, format, args);
		}

		public static string ToString<T>(T value) where T : IConvertible
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public static string ToString(DateTime value, string format)
		{
			return value.ToString(format, CultureInfo.InvariantCulture);
		}

		public static string ToUpper(string value)
		{
			return value.ToUpper(CultureInfo.InvariantCulture);
		}

		// Compares strings ignoring case.
		public static int Compare(string strA, string strB)
		{
			return string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase);
		}

		// Escapes a literal string to be used as a wildcard.
		//! It is a workaround:
		// 1) Rename-Item has no -LiteralPath --> we have to escape wildcards (anyway it fails e.g. "name`$][").
		// 2) BUG in [Management.Automation.WildcardPattern]::Escape(): e.g. `` is KO ==>.
		// '``' -like [Management.Automation.WildcardPattern]::Escape('``') ==> False
		public static string EscapeWildcard(string literal)
		{
			if (_reEscapeWildcard == null)
				_reEscapeWildcard = new Regex(@"([`\[\]\*\?])");
			return _reEscapeWildcard.Replace(literal, "`$1");
		}
		static Regex _reEscapeWildcard;

		public static Regex RegexTableSeparator
		{
			get
			{
				if (_RegexTableSeparator == null)
					//! Table separator may start with spaces if a column is right aligned.
					_RegexTableSeparator = new Regex(@"^\s*--");
				return _RegexTableSeparator;
			}
		}
		static Regex _RegexTableSeparator;

		public static void FormatMessageLines(List<string> lines, string message, int width, int height)
		{
			Regex format = null;
			foreach (string s1 in Regex.Split(message.Replace('\t', ' '), "\r\n|\r|\n"))
			{
				if (s1.Length <= width)
				{
					lines.Add(s1);
				}
				else
				{
					if (format == null)
						format = new Regex("(.{0," + width + "}(?:\\s|$))");
					string[] s3 = format.Split(s1);
					foreach (string s2 in s3)
					{
						if (s2.Length > 0)
						{
							lines.Add(s2);
							if (lines.Count >= height)
								return;
						}
					}
				}
				if (lines.Count >= height)
					return;
			}
		}

		//?? _090901_055134 Check in V2 (bad for viewer and notepad)
		/// <summary>
		/// Formats a position message.
		/// </summary>
		public static string PositionMessage(string message)
		{
			return message.Trim().Replace("\n", "\r\n");
		}
	}

	/// <summary>
	/// Resources.
	/// </summary>
	static class Res
	{
		public const string
			InvokeSelectedCode = "Invoke selected code",
			BackgroundJobs = "Background jobs",
			// main menu
			MenuInvokeInputCode = "&1. Invoke input code... ", // use right margin spaces
			MenuInvokeSelectedCode = "&2. " + InvokeSelectedCode,
			MenuBackgroundJobs = "&3. " + BackgroundJobs + "...",
			MenuCommandHistory = "&4. Command history...",
			MenuEditorConsole = "&5. Editor console",
			MenuPowerPanel = "&6. Power panel...",
			MenuTabExpansion = "&7. TabExpansion",
			MenuSnapin = "&8. Modules+...",
			MenuDebugger = "&9. Debugger...",
			MenuError = "&0. Errors...",
			MenuUserMenu = "&-. User tools...",
			MenuUserCommand = "&=. User code",
			// errors
			AskSaveModified = "Would you like to save modified data?",
			EditorConsoleCannotComplete = "Editor console can't complete the command\nbecause its window is not current at this moment.",
			LogError = "Cannot write to the log; ensure the path is valid and the file is not busy.",
			PropertyIsNotSettable = "Note: this property is not settable, changes will be lost.",
			NeedsEditor = "Editor is not opened or its window is not current.",
			NotSupportedByProvider = "Operation is not supported by the provider.",
			NoUserMenu = "You did not define your user menu $Psf.UserMenu.\nPlease, see help and example script Profile-.ps1",
			PropertyIsNotSettableNow = "The property is not settable at this moment.",
			CanNotClose = "Cannot close the session at this time.",
			// others
			Cancel = "Cancel",
			Delete = "Delete",
			PromptCode = "Enter PowerShell code",
			Remove = "Remove",
			CtrlC = "Cancel key is pressed.",
			// history
			PowerShellFarPrompt = "PowerShellFarPrompt",
			// main name
			Name = "PowerShellFar";
	}

	class PowerPath
	{
		readonly PathInfo _p;
		public PowerPath(PathInfo p)
		{
			_p = p;
		}

		string _Path;
		/// <summary>
		/// System friendly path.
		/// </summary>
		public string Path
		{
			get
			{
				if (_Path == null)
				{
					_Path = _p.ProviderPath;
					if (!_Path.StartsWith("\\\\", StringComparison.Ordinal))
					{
						_Path = _p.Path;
						if (_Path.Length == 0 || _Path == "\\")
							_Path = _p.Drive.Name + ":";
					}
				}
				return _Path;
			}
		}

		public ProviderInfo Provider
		{
			get { return _p.Provider; }
		}

		public PSDriveInfo Drive
		{
			get { return _p.Drive; }
		}
	}

	class DataLookup
	{
		string[] _namePairs;

		public DataLookup(string[] namePairs)
		{
			_namePairs = namePairs;
		}

		public void Invoke(object sender, FileEventArgs e)
		{
			// lookup data panel (should be checked, user could use another)
			DataPanel dp = sender as DataPanel;
			if (dp == null) throw new InvalidOperationException("Event sender is not a DataPanel.");

			// destination row (should be valid, checked on creation by us)
			DataRow drSet = (DataRow)((MemberPanel)dp.Parent).Value.BaseObject;

			// the source row
			DataRow drGet = (DataRow)e.File.Data;

			// copy data using name pairs
			for (int i = 0; i < _namePairs.Length; i += 2)
				drSet[_namePairs[i]] = drGet[_namePairs[i + 1]];
		}
	}

	/// <summary>
	/// User actions.
	/// </summary>
	enum UserAction
	{
		/// <summary>None.</summary>
		None,
		/// <summary>Enter is pressed.</summary>
		Enter,
		/// <summary>CtrlR is pressed.</summary>
		CtrlR,
	}

	/// <summary>
	/// Standard message box button set.
	/// </summary>
	public enum ButtonSet
	{
		///
		Ok,
		///
		OkCancel,
		///
		AbortRetryIgnore,
		///
		YesNo,
		///
		YesNoCancel,
		///
		RetryCancel
	}
}

namespace My
{
	/// <summary>
	/// My System.IO.Path extensions.
	/// </summary>
	/// <remarks>
	/// System.IO.Path is not OK due to invalid file system chars that are valid for other providers.
	/// </remarks>
	static class PathEx
	{
		public static bool IsPSFile(string fileName)
		{
			return
				fileName.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase) ||
				fileName.EndsWith(".psm1", StringComparison.OrdinalIgnoreCase) ||
				fileName.EndsWith(".psd1", StringComparison.OrdinalIgnoreCase);
		}

		public static bool IsPath(string name)
		{
			return name.StartsWith("\\\\", StringComparison.Ordinal) || (name.Length > 3 && name[1] == ':');
		}

		public static string Combine(string path, string file)
		{
			if (path == null)
				return file;
			if (path.EndsWith("\\", StringComparison.Ordinal))
				return path + file;
			// 090824
			if (path.EndsWith("::", StringComparison.Ordinal))
				return path + file;
			else
				return path + "\\" + file;
		}

		public static string GetFileName(string path)
		{
			int i = path.LastIndexOf('\\');
			if (i < 0)
				return path;

			return path.Substring(i + 1);
		}

		public static string GetDirectoryName(string path)
		{
			int i = path.LastIndexOf('\\');
			if (i < 0)
				return string.Empty;

			return path.Substring(0, i);
		}

		/// <summary>
		/// Tries to recognize an existing file path by an object.
		/// </summary>
		/// <param name="value">Any object, e.g. FileInfo, String.</param>
		/// <returns>Existing file path or null.</returns>
		/// <remarks>
		/// _091202_073429
		/// </remarks>
		public static string TryGetFilePath(object value)
		{
			FileInfo fi = PowerShellFar.Cast<FileInfo>.From(value);
			if (fi != null)
				return fi.FullName;

			string path;
			if (LanguagePrimitives.TryConvertTo<string>(value, out path))
			{
				// looks like a full path
				if (path.Length > 3 && path.Substring(1, 2) == ":\\" || path.StartsWith("\\\\", StringComparison.OrdinalIgnoreCase))
				{
					if (File.Exists(path))
						return path;
				}
			}

			return null;
		}

	}

	/// <summary>
	/// My System.Management.Automation.ProviderInfo extensions.
	/// </summary>
	static class ProviderInfoEx
	{
		public static bool HasContent(ProviderInfo provider)
		{
			return provider.ImplementingType.GetInterface("IContentCmdletProvider") != null;
		}

		public static bool HasDynamicProperty(ProviderInfo provider)
		{
			return provider.ImplementingType.GetInterface("IDynamicPropertyCmdletProvider") != null;
		}

		public static bool HasProperty(ProviderInfo provider)
		{
			return provider.ImplementingType.GetInterface("IPropertyCmdletProvider") != null;
		}

		public static bool IsNavigation(ProviderInfo provider)
		{
			//! 'is' does not work, because we work just on a type, not an instance
			return provider.ImplementingType.IsSubclassOf(typeof(NavigationCmdletProvider));
		}
	}
}
