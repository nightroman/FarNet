namespace FarNet.Works;

partial class ModuleManager
{
	// Cached data.
	internal long Timestamp { get; set; }
	internal bool ToUseEditors { get; private set; }
	internal string? CachedUICulture { get; private set; }

	// Actions from cache or reflection for caching.
	internal List<ProxyAction> ProxyActions = [];

	// Called after loading.
	internal void DropCache()
	{
		ProxyActions = null!;
		CachedUICulture = null;
	}

	internal void WriteCache(BinaryWriter writer)
	{
		// [1]
		writer.Write(Timestamp);

		// [2]
		writer.Write(ToUseEditors);

		// [3]
		writer.Write(CurrentUICultureName());

		// [4]
		var settingsTypeNames = GetSettingsTypeNames();
		writer.Write(settingsTypeNames.Length);
		foreach (string typeName in settingsTypeNames)
			writer.Write(typeName);

		// [5]
		var hostClassName = GetHostTypeName();
		writer.Write(hostClassName ?? string.Empty);

		// [6]
		writer.Write(ProxyActions.Count);
		foreach (var proxy in ProxyActions)
		{
			writer.Write((int)proxy.Kind);
			proxy.WriteCache(writer);
		}
	}

	internal void ReadCache(BinaryReader reader)
	{
		// [1]
		Timestamp = reader.ReadInt64();

		// [2]
		ToUseEditors = reader.ReadBoolean();

		// [3]
		CachedUICulture = reader.ReadString();

		// [4]
		var settingsCount = reader.ReadInt32();
		for (int i = 0; i < settingsCount; i++)
			AddSettingsTypeName(reader.ReadString());

		// [5]
		var hostTypeName = reader.ReadString();
		if (hostTypeName.Length > 0)
			SetHostTypeName(hostTypeName);

		// [6]
		var actionCount = reader.ReadInt32();
		for (int i = 0; i < actionCount; i++)
		{
			var kind = (ModuleItemKind)reader.ReadInt32();
			ProxyAction action = kind switch
			{
				ModuleItemKind.Command => new ProxyCommand(this, reader),
				ModuleItemKind.Editor => new ProxyEditor(this, reader),
				ModuleItemKind.Drawer => new ProxyDrawer(this, reader),
				ModuleItemKind.Tool => new ProxyTool(this, reader),
				_ => throw new ModuleException(),
			};
			ProxyActions.Add(action);
		}
	}
}
