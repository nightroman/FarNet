
// FarNet module RightWords
// Copyright (c) Roman Kuzmin

namespace FarNet.RightWords
{
	public class TheHost : ModuleHost
	{
		public static TheHost Instance { get; private set; }

		public TheHost()
		{
			Instance = this;
		}
	}
}
