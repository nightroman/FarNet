
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FarNet.Works
{
	/// <summary>
	/// INTERNAL
	/// </summary>
	public abstract class Far2
	{
		static Far2 _Host;
		/// <summary>
		/// INTERNAL
		/// </summary>
		public static Far2 Api
		{
			get { return _Host; }
			set
			{
				if (_Host != null) throw new InvalidOperationException();
				_Host = value;
			}
		}
		/// <summary>
		/// Creates the module panel.
		/// </summary>
		public abstract IPanelWorks CreatePanel(Panel panel, Explorer explorer);
		/// <summary>
		/// Waits for posted steps to be invoked.
		/// </summary>
		public abstract Task WaitSteps();
		/// <summary>
		/// Posts macro and gets its wait handle.
		/// </summary>
		public abstract WaitHandle PostMacroWait(string macro);
	}
	/// <summary>
	/// INTERNAL
	/// </summary>
	public static class Tasks2
	{
		///
		public static async Task<object> Wait(string message, Func<bool> job)
		{
			if (await Tasks.Wait(50, 5000, job))
				return null;
			else
				throw new Exception(message);
		}
	}
	/// <summary>
	/// INTERNAL
	/// </summary>
	public sealed class DelegateToString
	{
		readonly Delegate _handler;
		/// <summary>
		/// INTERNAL
		/// </summary>
		/// <param name="handler">INTERNAL</param>
		public DelegateToString(Delegate handler)
		{
			_handler = handler;
		}
		/// <summary>
		/// INTERNAL
		/// </summary>
		public override string ToString()
		{
			var sb = new StringBuilder();
			foreach (var it in _handler.GetInvocationList())
			{
				if (sb.Length > 0)
					sb.AppendLine();

				if (it == null)
				{
					sb.Append("[null]");
				}
				else
				{
					sb.Append(it.Method.ReflectedType.FullName);
					sb.Append(".");
					sb.Append(it.Method.Name);
				}
			}
			return sb.ToString();
		}
	}

	/// <summary>
	/// INTERNAL
	/// </summary>
	public enum FormatMessageMode
	{
		/// <summary>
		/// Cut wide lines.
		/// </summary>
		Cut,
		/// <summary>
		/// Wrap lines by words.
		/// </summary>
		Word
	}

	/// <summary>
	/// INTERNAL
	/// </summary>
	public static class Kit
	{
		/// <summary>
		/// INTERNAL
		/// </summary>
		// Joins two strings with a space. Either string may be null or empty.
		public static string JoinText(string head, string tail)
		{
			if (string.IsNullOrEmpty(head))
				return tail ?? string.Empty;
			if (string.IsNullOrEmpty(tail))
				return head ?? string.Empty;
			return head + " " + tail;
		}
		/// <summary>
		/// INTERNAL
		/// </summary>
		public static Exception UnwrapAggregateException(Exception exn)
		{
			if (exn is AggregateException aggregate && aggregate.InnerExceptions.Count == 1)
				return aggregate.InnerExceptions[0];
			else
				return exn;
		}
		/// <summary>
		/// %TEMP%\GUID.tmp or GUID.extension
		/// </summary>
		public static string TempFileName(string extension)
		{
			var name = Guid.NewGuid().ToString("N");
			if (string.IsNullOrEmpty(extension))
				name += ".tmp";
			else if (extension[0] == '.')
				name += extension;
			else
				name += "." + extension;
			return Path.GetTempPath() + name;
		}
		/// <summary>
		/// INTERNAL
		/// </summary>
		public static string[] SplitLines(string value)
		{
			if (value == null)
				return new string[] { string.Empty };

			//! Regex is twice slower
			return value.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
		}
		/// <summary>
		/// INTERNAL
		/// </summary>
		/// <param name="lines">Output lines.</param>
		/// <param name="message">Input string.</param>
		/// <param name="width">Maximum line width.</param>
		/// <param name="height">Maximum line count (message text area height).</param>
		/// <param name="mode">Formatting mode.</param>
		/// <remarks>
		/// Formats the string as a limited number of lines of limited width.
		/// </remarks>
		public static void FormatMessage(IList<string> lines, string message, int width, int height, FormatMessageMode mode)
		{
			if (lines == null) throw new ArgumentNullException("lines");
			if (message == null) throw new ArgumentNullException("message");

			Regex format = null;
			foreach (var line in Kit.SplitLines(message.Replace('\t', ' ')))
			{
				if (line.Length <= width)
				{
					lines.Add(line);
				}
				else if (mode == FormatMessageMode.Cut)
				{
					lines.Add(line.Substring(0, width));
				}
				else
				{
					if (format == null)
						format = new Regex("(.{0," + width + "}(?:\\s|$))");
					string[] s3 = format.Split(line);
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
		/// <summary>
		/// INTERNAL Hashes the files using the comparer, counts dupes.
		/// </summary>
		public static Dictionary<FarFile, int> HashFiles(IEnumerable files, IEqualityComparer<FarFile> comparer)
		{
			if (files == null) throw new ArgumentNullException("files");

			var hash = new Dictionary<FarFile, int>(comparer);
			foreach (FarFile file in files)
			{
				try
				{
					hash.Add(file, 1);
				}
				catch (ArgumentException)
				{
					++hash[file];
				}
			}
			return hash;
		}
		/// <summary>
		/// Gets true if a string is not a valid file system file name.
		/// </summary>
		public static bool IsInvalidFileName(string name)
		{
			if (string.IsNullOrEmpty(name))
				return true;

			if (_invalidName == null)
				_invalidName = new Regex("[" + Regex.Escape(new string(Path.GetInvalidFileNameChars())) + @"]|[\s.]$|^(?:CON|PRN|AUX|NUL|(?:COM|LPT)[1-9])$", RegexOptions.IgnoreCase);

			return _invalidName.IsMatch(name);
		}
		static Regex _invalidName;
		/// <summary>
		/// Interactively fixes an invalid file name.
		/// </summary>
		/// <param name="name">An invalid file name.</param>
		/// <returns>A valid file name or null if canceled.</returns>
		public static string FixInvalidFileName(string name)
		{
			for (; ; )
			{
				name = Far.Api.Input("Correct file name", null, "Invalid file name", name);
				if (null == name)
					return null;

				if (IsInvalidFileName(name))
					continue;

				return name;
			}
		}
		/// <summary>
		/// Gets or sets the default macro output mode.
		/// </summary>
		public static bool MacroOutput { get; set; }
	}
	/// <summary>
	/// INTERNAL
	/// </summary>
	public static class Test
	{
		static void AssertNormalPanel(IPanel panel, string active)
		{
			if (panel.IsPlugin)
			{
				throw new InvalidOperationException($"Expected {active} panel type: Native, actual: Plugin.");
			}
			if (panel.Kind != PanelKind.File)
			{
				throw new InvalidOperationException($"Expected {active} panel kind: File, actual: {panel.Kind}.");
			}
			if (!panel.IsVisible)
			{
				throw new InvalidOperationException($"Expected {active} panel state: Visible, actial: Hidden.");
			}
		}
		/// <summary>
		/// INTERNAL
		/// </summary>
		public static void AssertNormalState()
		{
			//! test kind first
			if (Far.Api.Window.Kind != WindowKind.Panels)
			{
				throw new InvalidOperationException($"Expected window: Panels, actual: {Far.Api.Window.Kind}.");
			}
			// 2 = Panels + Desktop
			if (Far.Api.Window.Count != 2)
			{
				throw new InvalidOperationException("Expected no windows but Panels.");
			}
			AssertNormalPanel(Far.Api.Panel, "active");
			AssertNormalPanel(Far.Api.Panel2, "passive");
		}
	}
}
