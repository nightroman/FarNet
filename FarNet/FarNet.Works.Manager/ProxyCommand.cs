
/*
FarNet plugin for Far Manager
Copyright (c) 2006-2015 Roman Kuzmin
*/

using System;
using System.Collections;

namespace FarNet.Works
{
	public sealed class ProxyCommand : ProxyAction, IModuleCommand
	{
		EventHandler<ModuleCommandEventArgs> _Handler;
		string _Prefix;
		internal ProxyCommand(ModuleManager manager, EnumerableReader reader)
			: base(manager, reader, new ModuleCommandAttribute())
		{
			Attribute.Prefix = (string)reader.Read();

			Init();
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
			if (e == null) throw new ArgumentNullException("e");

			Log.Source.TraceInformation("Invoking {0} Command='{1}'", this, e.Command);
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
			return string.Format(null, "{0} Prefix='{1}'", base.ToString(), Prefix);
		}
		internal sealed override void WriteCache(IList data)
		{
			base.WriteCache(data);
			data.Add(Attribute.Prefix);
		}
		new ModuleCommandAttribute Attribute
		{
			get { return (ModuleCommandAttribute)base.Attribute; }
		}
		public override ModuleItemKind Kind
		{
			get { return ModuleItemKind.Command; }
		}
		public string Prefix
		{
			get { return _Prefix; }
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
		const int idPrefix = 0;
		internal override Hashtable SaveData()
		{
			var data = new Hashtable();
			if (_Prefix != Attribute.Prefix)
				data.Add(idPrefix, _Prefix);
			return data;
		}
		internal override void LoadData(Hashtable data)
		{
			if (data == null)
				_Prefix = Attribute.Prefix;
			else
				_Prefix = data[idPrefix] as string ?? Attribute.Prefix;
		}
	}
}
