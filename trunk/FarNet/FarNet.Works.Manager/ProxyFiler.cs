/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Collections.Generic;

namespace FarNet.Works
{
	public sealed class ProxyFiler : ProxyAction, IModuleFiler
	{
		EventHandler<ModuleFilerEventArgs> _Handler;
		string _Mask;

		internal ProxyFiler(ModuleManager manager, EnumerableReader reader)
			: base(manager, reader, new ModuleFilerAttribute())
		{
			Attribute.Mask = reader.Read();
			Attribute.Creates = bool.Parse(reader.Read());

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

		void Init()
		{
			if (Attribute.Mask == null)
				Attribute.Mask = string.Empty;
		}

		public void Invoke(object sender, ModuleFilerEventArgs e)
		{
			using (Log log = Log.Switch.TraceInfo ? new Log("Invoking {0} Name='{1}' Mode='{2}'", (_Handler != null ? Log.Format(_Handler.Method) : ClassName), e.Name, e.Mode) : null)
			{
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
		}

		public void ResetMask(string value)
		{
			Host.Instance.SaveFarNetValue(Key, "Mask", value);
			_Mask = value ?? string.Empty;
		}

		public sealed override string ToString()
		{
			return Invariant.Format("{0} Mask='{1}'", base.ToString(), Mask);
		}

		internal sealed override void WriteCache(List<string> data)
		{
			base.WriteCache(data);
			data.Add(Attribute.Mask);
			data.Add(Attribute.Creates.ToString());
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
			get
			{
				if (_Mask == null)
					_Mask = Host.Instance.LoadFarNetValue(Key, "Mask", Attribute.Mask).ToString();

				return _Mask;
			}
		}
	}
}
