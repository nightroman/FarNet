
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2014 Roman Kuzmin
*/

using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace FarNet
{
	/// <summary>
	/// INTERNAL
	/// </summary>
	public static class Log
	{
		/// <summary>
		/// INTERNAL
		/// </summary>
		public static TraceSource Source { get { return _Source; } }
		static readonly TraceSource _Source = new TraceSource("FarNet");
		/// <summary>
		/// INTERNAL
		/// </summary>
		/// <param name="error">The exception to format.</param>
		public static string FormatException(Exception error)
		{
			if (error == null)
				throw new ArgumentNullException("error");

			//?? _090901_055134 Regex is used to fix bad PS V1 strings; check V2
			Regex re = new Regex("[\r\n]+");
			string info =
				error.GetType().Name + ":" + Environment.NewLine +
				re.Replace(error.Message, Environment.NewLine) + Environment.NewLine;

			// get an error record
			if (error.GetType().FullName.StartsWith("System.Management.Automation.", StringComparison.Ordinal))
			{
				object errorRecord = GetPropertyValue(error, "ErrorRecord");
				if (errorRecord != null)
				{
					// process the error record
					object ii = GetPropertyValue(errorRecord, "InvocationInfo");
					if (ii != null)
					{
						object pm = GetPropertyValue(ii, "PositionMessage");
						if (pm != null)
							//?? 090517 Added Trim(), because a position message starts with an empty line
							info += re.Replace(pm.ToString().Trim(), Environment.NewLine) + Environment.NewLine;
					}
				}
			}

			if (error.InnerException != null)
				info += Environment.NewLine + FormatException(error.InnerException);

			return info;
		}
		/// <summary>
		/// INTERNAL
		/// </summary>
		/// <param name="error">The error message.</param>
		public static void TraceError(string error)
		{
			Source.TraceEvent(TraceEventType.Error, 0, error);
		}
		// return: null if not written or formatted error info
		/// <summary>
		/// INTERNAL
		/// </summary>
		/// <param name="error">The exception to trace.</param>
		public static string TraceException(Exception error)
		{
			// no job?
			if (null == error || !Source.Switch.ShouldTrace(TraceEventType.Error))
				return null;

			// find the last dot
			string type = error.GetType().FullName;
			int i = type.LastIndexOf('.');

			// system error: trace as error
			string r = null;
			if (i >= 0 && type.Substring(0, i) == "System")
			{
				r = FormatException(error);
				Source.TraceEvent(TraceEventType.Error, 0, r);
			}
			// other error: trace as warning
			else if (Source.Switch.ShouldTrace(TraceEventType.Warning))
			{
				r = FormatException(error);
				Source.TraceEvent(TraceEventType.Warning, 0, r);
			}

			return r;
		}
		// Gets a property value or null
		static object GetPropertyValue(object obj, string name)
		{
			try
			{
				return obj.GetType().InvokeMember(name, BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance, null, obj, null, CultureInfo.InvariantCulture);
			}
			catch (MemberAccessException e)
			{
				TraceException(e);
				return null;
			}
		}
	}
}
