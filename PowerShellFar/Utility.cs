
// PowerShellFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace PowerShellFar
{
	/// <summary>
	/// For internal use and testing.
	/// </summary>
	public static class Zoo
	{
		///
		public static Meta[] TablePanelSetupColumns(object[] columns)
		{
			return Format.SetupColumns(columns);
		}

		/// <summary>
		/// Key: prefix with spaces or spaces.
		/// Value: command with no leading spaces.
		/// Public for Test-SplitCommandWithPrefix-.ps1
		/// </summary>
		public static KeyValuePair<string, string> SplitCommandWithPrefix(string text)
		{
			var match = Regex.Match(text, @"(\s*)(?:(\w+):)?\s*");
			Debug.Assert(match.Success);

			int index;
			var prefix = match.Groups[2].Value;
			if (prefix.Length == 0 ||
				prefix.Equals(Entry.CommandInvoke1.Prefix, StringComparison.OrdinalIgnoreCase) ||
				prefix.Equals(Entry.CommandInvoke2.Prefix, StringComparison.OrdinalIgnoreCase))
			{
				//: no prefix | ps: | vps:
				index = match.Length;
			}
			else
			{
				//: unknown prefix
				index = match.Groups[1].Length;
			}

			if (index > 0)
				return new(text.Substring(0, index), text.Substring(index));
			else
				return new(string.Empty, text);
		}

		///
		public static Process StartExternalViewer(string fileName)
		{
			string externalViewerFileName = Settings.Default.ExternalViewerFileName;
			string externalViewerArguments;

			// try the user defined viewer
			if (!string.IsNullOrEmpty(externalViewerFileName))
			{
				externalViewerArguments = string.Format(null, Settings.Default.ExternalViewerArguments, fileName);
				try
				{
					return My.ProcessEx.Start(externalViewerFileName, externalViewerArguments);
				}
				catch (Win32Exception)
				{
					Far.Api.Message(
						"Cannot start the external viewer.",
						Res.Me, MessageOptions.LeftAligned | MessageOptions.Warning);
				}
			}

			// use default external viewer
			externalViewerFileName = Process.GetCurrentProcess().MainModule.FileName;
			externalViewerArguments = "/w- /ro /m /p /v \"" + fileName + "\"";
			return My.ProcessEx.Start(externalViewerFileName, externalViewerArguments);
		}

		#region Transcript
		const string TextTranscriptFileExistsNoClobber = "File {0} already exists and {1} was specified.";
		const string TextTranscriptFileReadOnly = "Transcription file is read only.";
		const string TextTranscriptFileMissing = "Transcription file is missing.";
		const string TextTranscriptInProgress = "Transcription has already been started. Use the Stop-Transcript command to stop transcription.";
		const string TranscriptNotInProgress = "Transcription has not been started. Use the Start-Transcript command to start transcription.";
		const string TextTranscriptStarted = "Transcript started, output file is {0}";
		const string TextTranscriptStopped = "Transcript stopped, output file is {0}";

		///
		public static void ShowTranscript(bool internalViewer)
		{
			if (TranscriptOutputWriter.LastFileName == null)
				throw new InvalidOperationException(TranscriptNotInProgress);

			if (!File.Exists(TranscriptOutputWriter.LastFileName))
				throw new InvalidOperationException(TextTranscriptFileMissing);

			if (internalViewer)
			{
				var viewer = Far.Api.CreateViewer();
				viewer.Title = Path.GetFileName(TranscriptOutputWriter.LastFileName);
				viewer.FileName = TranscriptOutputWriter.LastFileName;
				viewer.CodePage = 1200;
				viewer.Open();
			}
			else
			{
				StartExternalViewer(TranscriptOutputWriter.LastFileName);
			}
		}

		//! Start-Transcript and Stop-Transcript get PSObject(string) with note property Path.
		static PSObject GetTranscriptResult(string format, string path)
		{
			var res = PSObject.AsPSObject(string.Format(format, path));
			res.Properties.Add(new PSNoteProperty("Path", path));
			return res;
		}

		///
		public static PSObject StopTranscript(bool force)
		{
			if (A.Psf.Transcript == null)
			{
				if (force)
					return null;

				throw new InvalidOperationException(TranscriptNotInProgress);
			}

			A.Psf.Transcript.Close();
			A.Psf.Transcript = null;

			return GetTranscriptResult(TextTranscriptStopped, TranscriptOutputWriter.LastFileName);
		}

		///
		public static PSObject StartTranscript(string path, bool append, bool force, bool noClobber)
		{
			if (A.Psf.Transcript != null)
				throw new InvalidOperationException(TextTranscriptInProgress);

			if (string.IsNullOrEmpty(path))
			{
				path = Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.Personal),
					string.Format("PowerShell_transcript.{0:yyyyMMddHHmmss}.txt", DateTime.Now));
			}

			if (File.Exists(path))
			{
				if (noClobber && !append)
					throw new InvalidOperationException(string.Format(TextTranscriptFileExistsNoClobber, path, "NoClobber"));

				var fileInfo = new FileInfo(path);
				if ((fileInfo.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
				{
					if (!force)
						throw new InvalidOperationException(TextTranscriptFileReadOnly);

					fileInfo.Attributes &= ~FileAttributes.ReadOnly;
				}
			}

			A.Psf.Transcript = new TranscriptOutputWriter(path, append);

			return GetTranscriptResult(TextTranscriptStarted, path);
		}
		#endregion
	}

	[Serializable]
	class ErrorException : ModuleException
	{
		public ErrorException() { }
		public ErrorException(string message) : base(message) { }
		public ErrorException(string message, Exception innerException) : base(message, innerException) { }
		protected ErrorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	/// <summary>
	/// Helper methods.
	/// </summary>
	static class Kit
	{
		/// <summary>
		/// Converts with culture.
		/// </summary>
		public static string ToString<T>(T value) where T : IConvertible //! IConvertible is not CLS-compliant
		{
			return value.ToString(CultureInfo.CurrentCulture);
		}

		/// <summary>
		/// Converts with culture.
		/// </summary>
		public static string ToString(DateTime value, string format)
		{
			return value.ToString(format, CultureInfo.CurrentCulture);
		}

		// Compares strings OrdinalIgnoreCase.
		public static bool Equals(string strA, string strB)
		{
			return string.Equals(strA, strB, StringComparison.OrdinalIgnoreCase);
		}

		// Escapes a literal string to be used as a wildcard.
		//! It is a workaround:
		// 1) Rename-Item has no -LiteralPath --> we have to escape wildcards (anyway it fails e.g. "name`$][").
		// 2) BUG in [Management.Automation.WildcardPattern]::Escape(): e.g. `` is KO ==>.
		// '``' -like [Management.Automation.WildcardPattern]::Escape('``') ==> False
		static Regex _reEscapeWildcard;
		public static string EscapeWildcard(string literal)
		{
			if (_reEscapeWildcard == null)
				_reEscapeWildcard = new Regex(@"([`\[\]\*\?])");
			return _reEscapeWildcard.Replace(literal, "`$1");
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
	/// Extra <see cref="PathInfo"/>.
	/// </summary>
	class PathInfoEx
	{
		readonly PathInfo _PathInfo;
		string _Path;

		///
		public PathInfoEx(string path)
		{
			var core = A.Psf.Engine.SessionState.Path;
			if (string.IsNullOrEmpty(path) || path == ".")
				_PathInfo = core.CurrentLocation;
			else
				// 3 times faster than push/set/pop location; NB: it is slow anyway
				_PathInfo = core.GetResolvedPSPathFromPSPath(Kit.EscapeWildcard(path))[0];
		}

		internal PathInfoEx(PathInfo pathInfo)
		{
			_PathInfo = pathInfo;
		}

		/// <summary>
		/// Gets the friendly path.
		/// </summary>
		public string Path
		{
			get //_110318_140817
			{
				if (_Path == null)
				{
					_Path = _PathInfo.ProviderPath;
					if (!_Path.StartsWith("\\\\", StringComparison.Ordinal))
					{
						_Path = _PathInfo.Path;
						if ((_Path.Length == 0 || _Path == "\\") && _PathInfo.Drive != null)
							_Path = _PathInfo.Drive.Name + ":\\";
					}
				}
				return _Path;
			}
		}

		/// <summary>
		/// Gets the provider info.
		/// </summary>
		public ProviderInfo Provider
		{
			get { return _PathInfo.Provider; }
		}

		/// <summary>
		/// Gets the drive name or null.
		/// </summary>
		internal string DriveName //! 110227 PathInfo.Drive can be null even if a drive exists
		{
			get
			{
				return _PathInfo.Drive?.Name;
			}
		}
	}

	class DataLookup
	{
		readonly string[] _namePairs;

		public DataLookup(string[] namePairs)
		{
			_namePairs = namePairs;
		}

		public void Invoke(object sender, OpenFileEventArgs e)
		{
			// lookup data panel (should be checked, user could use another)
			if (!(sender is DataPanel dp))
				throw new InvalidOperationException("Event sender is not a data panel object.");

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
		Enter
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

	class RunArgs
	{
		public string Code { get; private set; }
		public OutputWriter Writer { get; set; }
		public bool AddHistory { get; set; }
		public bool NoOutReason { get; set; }
		public bool UseLocalScope { get; set; }
		public object[] Arguments { get; set; }
		public Exception Reason { get; set; }

		public RunArgs(string code)
		{
			Code = code;
		}
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

		/// <summary>
		/// Does a string looks like a file system path?
		/// </summary>
		public static bool IsFSPath(string name)
		{
			return name.StartsWith("\\\\", StringComparison.Ordinal) || (name.Length > 3 && name[1] == ':');
		}

		public static string Combine(string path, string file)
		{
			// no path or special fake path, e.g. in object panel
			if (path == null || path == "*")
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
		public static string TryGetFilePath(object value) //_091202_073429
		{
			var fileInfo = PowerShellFar.Cast<FileInfo>.From(value);
			if (fileInfo != null)
				return fileInfo.FullName;

			if (LanguagePrimitives.TryConvertTo<string>(value, out string path))
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

		/// <summary>
		/// Tries to recognize an existing file path by an object.
		/// </summary>
		/// <param name="value">Any object, e.g. DirectoryInfo, String.</param>
		/// <returns>Existing directory path or null.</returns>
		public static string TryGetDirectoryPath(object value)
		{
			var directoryInfo = PowerShellFar.Cast<DirectoryInfo>.From(value);
			if (directoryInfo != null)
				return directoryInfo.FullName;

			if (LanguagePrimitives.TryConvertTo<string>(value, out string path))
			{
				// looks like a full path
				if (path.Length > 3 && path.Substring(1, 2) == ":\\" || path.StartsWith("\\\\", StringComparison.OrdinalIgnoreCase))
				{
					if (Directory.Exists(path))
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

	static class ProcessEx
	{
		/// <summary>
		/// Just a wrapper and helper to watch calls.
		/// </summary>
		public static Process Start(string fileName, string arguments)
		{
			return Process.Start(new ProcessStartInfo() { FileName = fileName, Arguments = arguments, UseShellExecute = true });
		}

		/// <summary>
		/// Opens the default browser.
		/// </summary>
		/// <remarks>
		/// https://brockallen.com/2016/09/24/process-start-for-urls-on-net-core/
		/// </remarks>
		public static void OpenBrowser(string url)
		{
			try
			{
				Process.Start(url);
			}
			catch
			{
				// hack because of this: https://github.com/dotnet/corefx/issues/10361
				url = url.Replace("&", "^&");
				Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
			}
		}
	}
}
