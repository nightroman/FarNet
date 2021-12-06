
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections;
using System.IO;

namespace FarNet.Works
{
	public sealed class ProxyCommand : ProxyAction, IModuleCommand
	{
		const int idPrefix = 0;
		string _Prefix;
		readonly EventHandler<ModuleCommandEventArgs> _Handler;

		new ModuleCommandAttribute Attribute => (ModuleCommandAttribute)base.Attribute;
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
				throw new ArgumentNullException("e");

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

		internal override Hashtable SaveConfig()
		{
			var data = new Hashtable();
			if (_Prefix != Attribute.Prefix)
				data.Add(idPrefix, _Prefix);
			return data;
		}

		internal override void LoadConfig(Hashtable data)
		{
			if (data == null)
				_Prefix = Attribute.Prefix;
			else
				_Prefix = data[idPrefix] as string ?? Attribute.Prefix;
		}
	}
}
