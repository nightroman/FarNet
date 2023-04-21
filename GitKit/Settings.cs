using FarNet;

namespace GitKit;

public class Settings : ModuleSettings<Settings.Data>
{
	public static Settings Default { get; } = new Settings();

	public class Data
	{
		public string DiffTool { get; set; } = @"%LOCALAPPDATA%\Programs\Microsoft VS Code\bin\code.cmd";
		public string DiffToolArguments { get; set; } = "--wait --diff \"%1\" \"%2\"";

		public bool UseGitCredentials { get; set; }

		public int CommitsPageLimit { get; set; } = 100;

		public int ShaPrefixLength { get; set; } = 7;
	}
}
