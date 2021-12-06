
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections;
using System.IO;

namespace FarNet.Works
{
	public sealed class ProxyTool : ProxyAction, IModuleTool
	{
		const int idOptions = 1;
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

		internal override Hashtable SaveConfig()
		{
			var data = new Hashtable();
			if (_Options != DefaultOptions)
				data.Add(idOptions, ~(((int)Attribute.Options) & (~((int)_Options))));
			return data;
		}

		internal override void LoadConfig(Hashtable data)
		{
			if (data == null)
			{
				_Options = DefaultOptions;
			}
			else
			{
				var options = data[idOptions];
				if (options == null)
					_Options = DefaultOptions;
				else
					_Options = DefaultOptions & (ModuleToolOptions)options;
			}
		}
	}
}
