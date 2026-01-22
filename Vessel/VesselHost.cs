using FarNet;

namespace Vessel;

public class VesselHost : ModuleHost
{
	const string NameLogFile1 = "VesselHistory.txt";
	const string NameLogFile2 = "VesselFolders.txt";
	const string NameLogFile3 = "VesselCommands.txt";

	internal static IReadOnlyList<string> LogPath { get; private set; } = null!;

	public VesselHost()
	{
		var dir = Manager.GetFolderPath(SpecialFolder.LocalData, true);
		LogPath = [Path.Combine(dir, NameLogFile1), Path.Combine(dir, NameLogFile2), Path.Combine(dir, NameLogFile3)];

		// ensure logs
		if (!File.Exists(LogPath[0]))
			Store.CreateLogFile(LogPath[0]);
		if (!File.Exists(LogPath[1]))
			Store.CreateLogFile(LogPath[1]);
		if (!File.Exists(LogPath[2]))
			Store.CreateLogFile(LogPath[2]);
	}
}
