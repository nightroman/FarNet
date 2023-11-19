
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.IO;

namespace FarNet.Works;

sealed class ProxyCommand : ProxyAction, IModuleCommand
{
	readonly EventHandler<ModuleCommandEventArgs>? _Handler;
	string _Prefix = string.Empty;

	ModuleCommandAttribute Attribute => (ModuleCommandAttribute)ActionAttribute;
	public override ModuleItemKind Kind => ModuleItemKind.Command;

	internal ProxyCommand(ModuleManager manager, BinaryReader reader)
		: base(manager, reader, new ModuleCommandAttribute())
	{
		// [1]
		Attribute.Prefix = reader.ReadString();
	}

	internal sealed override void WriteCache(BinaryWriter writer)
	{
		base.WriteCache(writer);

		// [1]
		writer.Write(Attribute.Prefix);
	}

	internal ProxyCommand(ModuleManager manager, Type classType)
		: base(manager, classType, typeof(ModuleCommandAttribute))
	{
	}

	public ProxyCommand(ModuleManager manager, Guid id, ModuleCommandAttribute attribute, EventHandler<ModuleCommandEventArgs> handler)
		: base(manager, id, (ModuleCommandAttribute)attribute.Clone())
	{
		_Handler = handler;
	}

	public void Invoke(object sender, ModuleCommandEventArgs e)
	{
		ArgumentNullException.ThrowIfNull(e);

		Invoking();

		if (_Handler is null)
		{
			var instance = (ModuleCommand)GetInstance();
			instance.Invoke(sender, e);
		}
		else
		{
			_Handler(sender, e);
		}
	}

	public sealed override string ToString()
	{
		return $"{base.ToString()} Prefix='{Prefix}'";
	}

	public string Prefix
	{
		get => _Prefix;
		set
		{
			if (string.IsNullOrEmpty(value))
				value = Attribute.Prefix;

			Host.Instance.InvalidateProxyCommand();
			_Prefix = value;
		}
	}

	internal Config.Command? SaveConfig()
	{
		if (_Prefix == Attribute.Prefix)
			return null;

		return new Config.Command { Id = Id, Prefix = _Prefix };
	}

	internal void LoadConfig(Config.Module? config)
	{
		Config.Command? data;
		if (config is null || (data = config.GetCommand(Id)) is null)
		{
			_Prefix = Attribute.Prefix;
		}
		else
		{
			_Prefix = data.Prefix ?? Attribute.Prefix;
		}
	}
}
