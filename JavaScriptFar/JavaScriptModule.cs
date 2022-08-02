
// JavaScriptFar module for Far Manager
// Copyright (c) Roman Kuzmin

using FarNet;

namespace JavaScriptFar;

public class JavaScriptModule : ModuleHost
{
	public static JavaScriptModule Instance { get; private set; }
	public static string Root { get; private set; }

	public JavaScriptModule()
	{
		Instance = this;
		Root = Manager.GetFolderPath(SpecialFolder.RoamingData, true);
	}
}
