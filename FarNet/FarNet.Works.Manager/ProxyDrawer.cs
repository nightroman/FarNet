
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2012 FarNet Team
*/

using System;
using System.Collections;

namespace FarNet.Works
{
	public sealed class ProxyDrawer : ProxyAction, IModuleDrawer
	{
		const int idMask = 0, idPriority = 1;
		readonly EventHandler<ModuleDrawerEventArgs> _Handler;
		string _Mask;
		int _Priority;
		internal ProxyDrawer(ModuleManager manager, EnumerableReader reader)
			: base(manager, reader, new ModuleDrawerAttribute())
		{
			Attribute.Mask = (string)reader.Read();
			Attribute.Priority = (int)reader.Read();

			Init();
		}
		internal ProxyDrawer(ModuleManager manager, Type classType)
			: base(manager, classType, typeof(ModuleDrawerAttribute))
		{
			Init();
		}
		public ProxyDrawer(ModuleManager manager, Guid id, ModuleDrawerAttribute attribute, EventHandler<ModuleDrawerEventArgs> handler)
			: base(manager, id, (attribute == null ? null : (ModuleDrawerAttribute)attribute.Clone()))
		{
			_Handler = handler;

			Init();
		}
		public EventHandler<ModuleDrawerEventArgs> CreateHandler()
		{
			if (_Handler != null)
				return _Handler;
			
			Invoking();
			ModuleDrawer instance = (ModuleDrawer)GetInstance();
			return instance.Invoke;
		}
		public sealed override string ToString()
		{
			return string.Format(null, "{0} Mask='{1}'", base.ToString(), Mask);
		}
		internal sealed override void WriteCache(IList data)
		{
			base.WriteCache(data);
			data.Add(Attribute.Mask);
			data.Add(Attribute.Priority);
		}
		new ModuleDrawerAttribute Attribute
		{
			get { return (ModuleDrawerAttribute)base.Attribute; }
		}
		public override ModuleItemKind Kind
		{
			get { return ModuleItemKind.Drawer; }
		}
		public string Mask
		{
			get { return _Mask; }
		}
		public string DefaultMask
		{
			get { return Attribute.Mask; }
		}
		public void ResetMask(string value)
		{
			if (value == null) throw new ArgumentNullException("value");

			_Mask = value;
		}
		public int Priority
		{
			get { return _Priority; }
		}
		public int DefaultPriority
		{
			get { return Attribute.Priority; }
		}
		public void ResetPriority(int value)
		{
			_Priority = value;
		}
		void Init()
		{
			if (Attribute.Mask == null)
				Attribute.Mask = string.Empty;
		}
		internal override Hashtable SaveData()
		{
			var data = new Hashtable();

			if (_Mask != DefaultMask)
				data.Add(idMask, _Mask);

			if (_Priority != DefaultPriority)
				data.Add(idPriority, _Priority);

			return data;
		}
		internal override void LoadData(Hashtable data)
		{
			if (data == null)
			{
				_Mask = DefaultMask;
				_Priority = DefaultPriority;
			}
			else
			{
				_Mask = data[idMask] as string ?? DefaultMask;
				_Priority = (int)(data[idPriority] ?? DefaultPriority);
			}
		}
	}
}
