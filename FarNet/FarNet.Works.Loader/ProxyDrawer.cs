
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.IO;

namespace FarNet.Works
{
	public sealed class ProxyDrawer : ProxyAction, IModuleDrawer
	{
		readonly Action<IEditor, ModuleDrawerEventArgs> _Handler;
		string _Mask;
		int _Priority;

		new ModuleDrawerAttribute Attribute => (ModuleDrawerAttribute)base.Attribute;
		public override ModuleItemKind Kind => ModuleItemKind.Drawer;

		internal ProxyDrawer(ModuleManager manager, BinaryReader reader)
			: base(manager, reader, new ModuleDrawerAttribute())
		{
			// [1]
			Attribute.Mask = reader.ReadString();
			// [2]
			Attribute.Priority = reader.ReadInt32();

			Init();
		}

		internal sealed override void WriteCache(BinaryWriter writer)
		{
			base.WriteCache(writer);

			// [1]
			writer.Write(Attribute.Mask);
			// [2]
			writer.Write(Attribute.Priority);
		}

		internal ProxyDrawer(ModuleManager manager, Type classType)
			: base(manager, classType, typeof(ModuleDrawerAttribute))
		{
			Init();
		}

		public ProxyDrawer(ModuleManager manager, Guid id, ModuleDrawerAttribute attribute, Action<IEditor, ModuleDrawerEventArgs> handler)
			: base(manager, id, (attribute == null ? null : (ModuleDrawerAttribute)attribute.Clone()))
		{
			_Handler = handler;

			Init();
		}

		public Action<IEditor, ModuleDrawerEventArgs> CreateHandler()
		{
			if (_Handler != null)
				return _Handler;

			Invoking();
			ModuleDrawer instance = (ModuleDrawer)GetInstance();
			return instance.Invoke;
		}

		public sealed override string ToString()
		{
			return $"{base.ToString()} Mask='{Mask}'";
		}

		public string Mask
		{
			get => _Mask;
			set => _Mask = value ?? throw new ArgumentNullException("value");
		}

		public int Priority
		{
			get => _Priority;
			set => _Priority = value;
		}

		void Init()
		{
			if (Attribute.Mask == null)
				Attribute.Mask = string.Empty;
		}

		internal Configuration.Drawer SaveConfig()
		{
			var data = new Configuration.Drawer();
			bool save = false;

			if (_Mask != Attribute.Mask)
			{
				data.Mask = _Mask;
				save = true;
			}

			if (_Priority != Attribute.Priority)
			{
				data.Priority = _Priority.ToString();
				save = true;
			}

			if (save)
			{
				data.Id = Id;
				return data;
			}

			return null;
		}

		internal void LoadConfig(Configuration.Module moduleData)
		{
			Configuration.Drawer data;
			if (moduleData != null && (data = moduleData.GetDrawer(Id)) != null)
			{
				_Mask = data.Mask ?? Attribute.Mask;
				_Priority = data.Priority is null ? Attribute.Priority : int.Parse(data.Priority);
			}
			else
			{
				_Mask = Attribute.Mask;
				_Priority = Attribute.Priority;
			}
		}
	}
}
