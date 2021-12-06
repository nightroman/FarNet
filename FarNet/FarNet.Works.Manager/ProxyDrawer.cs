
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections;
using System.IO;

namespace FarNet.Works
{
	public sealed class ProxyDrawer : ProxyAction, IModuleDrawer
	{
		const int idMask = 0, idPriority = 1;
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

		internal override Hashtable SaveConfig()
		{
			var data = new Hashtable();

			if (_Mask != Attribute.Mask)
				data.Add(idMask, _Mask);

			if (_Priority != Attribute.Priority)
				data.Add(idPriority, _Priority);

			return data;
		}

		internal override void LoadConfig(Hashtable data)
		{
			if (data == null)
			{
				_Mask = Attribute.Mask;
				_Priority = Attribute.Priority;
			}
			else
			{
				_Mask = data[idMask] as string ?? Attribute.Mask;
				_Priority = (int)(data[idPriority] ?? Attribute.Priority);
			}
		}
	}
}
