
/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Collections.Generic;
using System.Globalization;

namespace FarNet.Works
{
	public sealed class ProxyTool : ProxyAction, IModuleTool
	{
		EventHandler<ModuleToolEventArgs> _Handler;
		string _Hotkey;
		ModuleToolOptions _Options;
		bool _OptionsValid;

		internal ProxyTool(ModuleManager manager, EnumerableReader reader)
			: base(manager, reader, new ModuleToolAttribute())
		{
			Attribute.Options = (ModuleToolOptions)int.Parse(reader.Read(), CultureInfo.InvariantCulture);
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

		public void ResetHotkey(string value)
		{
			// set
			SetValidHotkey(value);

			// save
			Host.Instance.SaveFarNetValue(Key, "Hotkey", _Hotkey);
		}

		public void ResetOptions(ModuleToolOptions value)
		{
			// unregister the current
			Host.Instance.UnregisterProxyTool(this);

			Host.Instance.SaveFarNetValue(Key, "Options", ~(((int)Attribute.Options) & (~((int)value))));
			_Options = value;
			_OptionsValid = true;

			// register new
			Host.Instance.RegisterProxyTool(this);
		}

		void SetValidHotkey(string value)
		{
			switch (value.Length)
			{
				case 0:
					_Hotkey = " ";
					break;
				case 1:
					_Hotkey = value;
					break;
				default:
					_Hotkey = value.Substring(0, 1);
					break;
			}
		}

		public sealed override string ToString()
		{
			return string.Format(null, "{0} Options='{1}'", base.ToString(), Attribute.Options);
		}

		internal sealed override void WriteCache(List<string> data)
		{
			base.WriteCache(data);
			data.Add(((int)Attribute.Options).ToString(CultureInfo.InvariantCulture));
		}

		new ModuleToolAttribute Attribute
		{
			get { return (ModuleToolAttribute)base.Attribute; }
		}

		public ModuleToolOptions DefaultOptions
		{
			get { return Attribute.Options; }
		}

		public string Hotkey
		{
			get
			{
				// load once
				if (_Hotkey == null)
				{
					string value = Host.Instance.LoadFarNetValue(Key, "Hotkey", string.Empty).ToString();
					SetValidHotkey(value);
				}

				return _Hotkey;
			}
		}

		public override ModuleItemKind Kind
		{
			get { return ModuleItemKind.Tool; }
		}

		public ModuleToolOptions Options
		{
			get
			{
				if (!_OptionsValid)
				{
					// merge with the default options
					_Options = Attribute.Options & (ModuleToolOptions)Host.Instance.LoadFarNetValue(Key, "Options", Attribute.Options);
					_OptionsValid = true;
				}

				return _Options;
			}
		}
	}
}
