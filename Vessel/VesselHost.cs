
// FarNet module Vessel
// Copyright (c) Roman Kuzmin

using System.IO;

namespace FarNet.Vessel
{
	[System.Runtime.InteropServices.Guid("ec92417f-317b-4b97-b6d4-bcbd8f2f0f2d")]
	[ModuleHost()]
	public class VesselHost : ModuleHost
	{
		const string NameLogFile1 = "VesselHistory.txt";
		const string NameLogFile2 = "VesselFolders.txt";
		const string NameLogFile3 = "VesselCommands.txt";
		internal static string[] LogPath { get; private set; }
		public override void Connect()
		{
			var dir = Manager.GetFolderPath(SpecialFolder.LocalData, true);
			LogPath = new string[] { Path.Combine(dir, NameLogFile1), Path.Combine(dir, NameLogFile2), Path.Combine(dir, NameLogFile3) };

			// ensure logs
			if (!File.Exists(LogPath[0]))
				Store.CreateLogFile(LogPath[0]);
			if (!File.Exists(LogPath[1]))
				Store.CreateLogFile(LogPath[1]);
			if (!File.Exists(LogPath[2]))
				Store.CreateLogFile(LogPath[2]);
		}
	}
}
