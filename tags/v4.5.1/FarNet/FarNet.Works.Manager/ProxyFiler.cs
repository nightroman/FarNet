
/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Collections;

namespace FarNet.Works
{
	public sealed class ProxyFiler : ProxyAction, IModuleFiler
	{
		EventHandler<ModuleFilerEventArgs> _Handler;
		string _Mask;
		internal ProxyFiler(ModuleManager manager, EnumerableReader reader)
			: base(manager, reader, new ModuleFilerAttribute())
		{
			Attribute.Mask = (string)reader.Read();
			Attribute.Creates = (bool)reader.Read();

			Init();
		}
		internal ProxyFiler(ModuleManager manager, Type classType)
			: base(manager, classType, typeof(ModuleFilerAttribute))
		{
			Init();
		}
		internal ProxyFiler(ModuleManager manager, Guid id, ModuleFilerAttribute attribute, EventHandler<ModuleFilerEventArgs> handler)
			: base(manager, id, (ModuleFilerAttribute)attribute.Clone())
		{
			_Handler = handler;

			Init();
		}
		public void Invoke(object sender, ModuleFilerEventArgs e)
		{
			if (e == null)
				throw new ArgumentNullException("e");

			Log.Source.TraceInformation("Invoking {0} Name='{1}' Mode='{2}'", this, e.Name, e.Mode);
			Invoking();

			if (_Handler != null)
			{
				_Handler(sender, e);
			}
			else
			{
				ModuleFiler instance = (ModuleFiler)GetInstance();
				instance.Invoke(sender, e);
			}
		}
		public void ResetMask(string value)
		{
			_Mask = value ?? string.Empty;
		}
		public sealed override string ToString()
		{
			return string.Format(null, "{0} Mask='{1}'", base.ToString(), Mask);
		}
		internal sealed override void WriteCache(IList data)
		{
			base.WriteCache(data);
			data.Add(Attribute.Mask);
			data.Add(Attribute.Creates);
		}
		new ModuleFilerAttribute Attribute
		{
			get { return (ModuleFilerAttribute)base.Attribute; }
		}
		public bool Creates
		{
			get { return Attribute.Creates; }
		}
		public string DefaultMask
		{
			get { return Attribute.Mask; }
		}
		public override ModuleItemKind Kind
		{
			get { return ModuleItemKind.Filer; }
		}
		public string Mask
		{
			get { return _Mask; }
		}
		void Init()
		{
			if (Attribute.Mask == null)
				Attribute.Mask = string.Empty;
		}
		int idMask = 0;
		internal override Hashtable SaveData()
		{
			var data = new Hashtable();
			if (_Mask != DefaultMask)
				data.Add(idMask, _Mask);
			return data;
		}
		internal override void LoadData(Hashtable data)
		{
			if (data == null)
				_Mask = DefaultMask;
			else
				_Mask = data[idMask] as string ?? DefaultMask;
		}
	}
}
