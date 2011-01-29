/*
FarNet plugin for Far Manager
Copyright (c) 2005 FarNet Team
*/

using System;
using System.Collections.Generic;

namespace FarNet.Works
{
	public sealed class ProxyEditor : ProxyAction, IModuleEditor
	{
		string _Mask;

		internal ProxyEditor(ModuleManager manager, EnumerableReader reader)
			: base(manager, reader, new ModuleEditorAttribute())
		{
			Attribute.Mask = reader.Read();

			Init();
		}

		internal ProxyEditor(ModuleManager manager, Type classType)
			: base(manager, classType, typeof(ModuleEditorAttribute))
		{
			Init();
		}

		void Init()
		{
			if (Attribute.Mask == null)
				Attribute.Mask = string.Empty;
		}

		public void Invoke(object sender, ModuleEditorEventArgs e)
		{
			Log.Source.TraceInformation("Invoking {0} FileName='{1}'", ClassName, ((IEditor)sender).FileName);
			Invoking();

			ModuleEditor instance = (ModuleEditor)GetInstance();
			instance.Invoke(sender, e);
		}

		public void ResetMask(string value)
		{
			if (value == null)
				throw new ArgumentNullException("value");

			Host.Instance.SaveFarNetValue(Key, "Mask", value);
			_Mask = value;
		}

		public sealed override string ToString()
		{
			return string.Format(null, "{0} Mask='{1}'", base.ToString(), Mask);
		}

		internal sealed override void WriteCache(List<string> data)
		{
			base.WriteCache(data);
			data.Add(Attribute.Mask);
		}

		new ModuleEditorAttribute Attribute
		{
			get { return (ModuleEditorAttribute)base.Attribute; }
		}

		public string DefaultMask
		{
			get { return Attribute.Mask; }
		}

		public override ModuleItemKind Kind
		{
			get { return ModuleItemKind.Editor; }
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
