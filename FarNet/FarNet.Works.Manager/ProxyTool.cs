
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2012 FarNet Team
*/

using System;
using System.Collections;

namespace FarNet.Works
{
	public sealed class ProxyTool : ProxyAction, IModuleTool
	{
		EventHandler<ModuleToolEventArgs> _Handler;
		ModuleToolOptions _Options;
		internal ProxyTool(ModuleManager manager, EnumerableReader reader)
			: base(manager, reader, new ModuleToolAttribute())
		{
			Attribute.Options = (ModuleToolOptions)reader.Read();
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

			Log.Source.TraceInformation("Invoking {0} From='{1}'", this, e.From);
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
			return string.Format(null, "{0} Options='{1}'", base.ToString(), Attribute.Options);
		}
		internal sealed override void WriteCache(IList data)
		{
			base.WriteCache(data);
			data.Add((int)Attribute.Options);
		}
		new ModuleToolAttribute Attribute
		{
			get { return (ModuleToolAttribute)base.Attribute; }
		}
		public ModuleToolOptions DefaultOptions
		{
			get { return Attribute.Options; }
		}
		public override ModuleItemKind Kind
		{
			get { return ModuleItemKind.Tool; }
		}
		public ModuleToolOptions Options
		{
			get { return _Options; }
			set
			{
				// unregister the current
				Host.Instance.UnregisterProxyTool(this);

				_Options = value;

				// register new
				Host.Instance.RegisterProxyTool(this);
			}
		}
		const int idOptions = 1;
		internal override Hashtable SaveData()
		{
			var data = new Hashtable();
			if (_Options != DefaultOptions)
				data.Add(idOptions, ~(((int)Attribute.Options) & (~((int)_Options)))); 
			return data;
		}
		internal override void LoadData(Hashtable data)
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
