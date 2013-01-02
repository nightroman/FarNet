
/*
FarNet module Vessel
Copyright (c) 2011-2013 Roman Kuzmin
*/

using System.Diagnostics;

namespace FarNet.Vessel
{
	public static class Logger
	{
		public static TraceSource Source { get { return _Source; } }
		static readonly TraceSource _Source = new TraceSource("Vessel", SourceLevels.All);
	}
}
