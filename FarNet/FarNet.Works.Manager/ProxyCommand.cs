/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FarNet.Works
{
	public sealed class ProxyCommand : ProxyAction, IModuleCommand
	{
		EventHandler<ModuleCommandEventArgs> _Handler;
		string _Prefix;

		internal ProxyCommand(ModuleManager manager, EnumerableReader reader)
			: base(manager, reader, new ModuleCommandAttribute())
		{
			Attribute.Prefix = reader.Read();

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

		void Init()
		{
			// solid prefix!
			if (string.IsNullOrEmpty(Attribute.Prefix))
				throw new ModuleException("Empty command prefix is not valid.");

			// get the working prefix now, it is needed for the command registration
			_Prefix = Host.Instance.LoadFarNetValue(Key, "Prefix", Attribute.Prefix).ToString();
		}

		public void Invoke(object sender, ModuleCommandEventArgs e)
		{
			if (e == null)
				throw new ArgumentNullException("e");

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

		public void ResetPrefix(string value)
		{
			if (string.IsNullOrEmpty(value))
				value = DefaultPrefix;

			Host.Instance.InvalidateProxyCommand();

			Host.Instance.SaveFarNetValue(Key, "Prefix", value);
			_Prefix = value;
		}

		public sealed override string ToString()
		{
			return Invariant.Format("{0} Prefix='{1}'", base.ToString(), Prefix);
		}

		internal sealed override void WriteCache(List<string> data)
		{
			base.WriteCache(data);
			data.Add(Attribute.Prefix);
		}

		new ModuleCommandAttribute Attribute
		{
			get { return (ModuleCommandAttribute)base.Attribute; }
		}

		public string DefaultPrefix
		{
			get { return Attribute.Prefix; }
		}

		public override ModuleItemKind Kind
		{
			get { return ModuleItemKind.Command; }
		}

		public string Prefix
		{
			get { return _Prefix; }
		}
	}
}
