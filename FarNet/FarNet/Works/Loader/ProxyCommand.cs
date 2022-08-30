
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.IO;

namespace FarNet.Works;

sealed class ProxyCommand : ProxyAction, IModuleCommand
{
	string _Prefix;
	readonly EventHandler<ModuleCommandEventArgs> _Handler;

	ModuleCommandAttribute Attribute => (ModuleCommandAttribute)ActionAttribute;
	public override ModuleItemKind Kind => ModuleItemKind.Command;

	internal ProxyCommand(ModuleManager manager, BinaryReader reader)
		: base(manager, reader, new ModuleCommandAttribute())
	{
		// [1]
		Attribute.Prefix = reader.ReadString();

		Init();
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
		Init();
	}

	public ProxyCommand(ModuleManager manager, Guid id, ModuleCommandAttribute attribute, EventHandler<ModuleCommandEventArgs> handler)
		: base(manager, id, (attribute == null ? null : (ModuleCommandAttribute)attribute.Clone()))
	{
		_Handler = handler;

		Init();
	}

	public void Invoke(object sender, ModuleCommandEventArgs e)
	{
		if (e == null)
			throw new ArgumentNullException(nameof(e));

		Invoking();

		if (_Handler != null)
		{
			_Handler(sender, e);
		}
		else
		{
			ModuleCommand instance = (ModuleCommand)GetInstance();
			instance.Invoke(sender, e);
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
			if (string.IsNullOrEmpty(value)) value = Attribute.Prefix;
			Host.Instance.InvalidateProxyCommand();
			_Prefix = value;
		}
	}

	void Init()
	{
		// solid prefix!
		if (string.IsNullOrEmpty(Attribute.Prefix))
			throw new ModuleException("Empty command prefix is not valid.");
	}

	internal Config.Command SaveConfig()
	{
		if (_Prefix == Attribute.Prefix)
			return null;

		return new Config.Command { Id = Id, Prefix = _Prefix };
	}

	internal void LoadConfig(Config.Module config)
	{
		Config.Command data;
		if (config != null && (data = config.GetCommand(Id)) != null)
		{
			_Prefix = data.Prefix ?? Attribute.Prefix;
		}
		else
		{
			_Prefix = Attribute.Prefix;
		}
	}
}
