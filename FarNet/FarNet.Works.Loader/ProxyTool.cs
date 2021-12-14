
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.IO;

namespace FarNet.Works
{
	public sealed class ProxyTool : ProxyAction, IModuleTool
	{
		ModuleToolOptions _Options;
		readonly EventHandler<ModuleToolEventArgs> _Handler;

		new ModuleToolAttribute Attribute => (ModuleToolAttribute)base.Attribute;
		public ModuleToolOptions DefaultOptions => Attribute.Options;
		public override ModuleItemKind Kind => ModuleItemKind.Tool;

		internal ProxyTool(ModuleManager manager, BinaryReader reader)
			: base(manager, reader, new ModuleToolAttribute())
		{
			// [1]
			Attribute.Options = (ModuleToolOptions)reader.ReadInt32();
		}

		internal sealed override void WriteCache(BinaryWriter writer)
		{
			base.WriteCache(writer);

			// [1]
			writer.Write((int)Attribute.Options);
		}

		internal ProxyTool(ModuleManager manager, Type classType)
			: base(manager, classType, typeof(ModuleToolAttribute))
		{ }

		internal ProxyTool(ModuleManager manager, Guid id, ModuleToolAttribute attribute, EventHandler<ModuleToolEventArgs> handler)
			: base(manager, id, (ModuleToolAttribute)attribute.Clone())
		{
			_Handler = handler;
		}

		public void Invoke(object sender, ModuleToolEventArgs e)
		{
			if (e == null)
				throw new ArgumentNullException("e");

			Invoking();

			if (_Handler != null)
			{
				_Handler(sender, e);
			}
			else
			{
				ModuleTool instance = (ModuleTool)GetInstance();
				instance.Invoke(sender, e);
			}
		}

		public sealed override string ToString()
		{
			return $"{base.ToString()} Options='{Attribute.Options}'";
		}

		public ModuleToolOptions Options
		{
			get => _Options;
			set
			{
				// unregister the current
				Host.Instance.UnregisterProxyTool(this);

				_Options = value;

				// register new
				Host.Instance.RegisterProxyTool(this);
			}
		}

		internal Config.Tool SaveConfig()
		{
			if (_Options == DefaultOptions)
				return null;

			return new Config.Tool { Id = Id, Options = ((int)_Options).ToString() };
		}

		internal void LoadConfig(Config.Module config)
		{
			Config.Tool data;
			if (config != null && (data = config.GetTool(Id)) != null)
			{
				var options = data.Options;
				if (options is null)
					_Options = DefaultOptions;
				else
					_Options = DefaultOptions & (ModuleToolOptions)int.Parse(options);
			}
			else
			{
				_Options = DefaultOptions;
			}
		}
	}
}
