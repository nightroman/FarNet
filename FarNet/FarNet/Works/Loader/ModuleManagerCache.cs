
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System.Collections.Generic;
using System.IO;

namespace FarNet.Works;

partial class ModuleManager
{
	// Stored in cache.
	internal long Timestamp;

	// Stored in cache.
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
		writer.Write(CurrentUICultureName());

		// [3]
		writer.Write(_SettingsTypeNames.Count);
		foreach (var typeName in _SettingsTypeNames)
			writer.Write(typeName);

		// [4]
		var hostClassName = GetModuleHostClassName();
		writer.Write(hostClassName ?? string.Empty);

		// [5]
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
		CachedUICulture = reader.ReadString();

		// [3]
		var settingsCount = reader.ReadInt32();
		_SettingsTypeNames.Capacity = settingsCount;
		for (int i = 0; i < settingsCount; i++)
			_SettingsTypeNames.Add(reader.ReadString());

		// [4]
		var hostTypeName = reader.ReadString();
		if (hostTypeName.Length > 0)
			SetModuleHostTypeName(hostTypeName);

		// [5]
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
