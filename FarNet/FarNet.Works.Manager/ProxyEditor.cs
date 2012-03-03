
/*
FarNet plugin for Far Manager
Copyright (c) 2005-2012 FarNet Team
*/

using System;
using System.Collections;

namespace FarNet.Works
{
	public sealed class ProxyEditor : ProxyAction, IModuleEditor
	{
		string _Mask;
		internal ProxyEditor(ModuleManager manager, EnumerableReader reader)
			: base(manager, reader, new ModuleEditorAttribute())
		{
			Attribute.Mask = (string)reader.Read();

			Init();
		}
		internal ProxyEditor(ModuleManager manager, Type classType)
			: base(manager, classType, typeof(ModuleEditorAttribute))
		{
			Init();
		}
		public void Invoke(object sender, ModuleEditorEventArgs e)
		{
			Log.Source.TraceInformation("Invoking {0} FileName='{1}'", ClassName, ((IEditor)sender).FileName);
			Invoking();

			ModuleEditor instance = (ModuleEditor)GetInstance();
			instance.Invoke(sender, e);
		}
		public sealed override string ToString()
		{
			return string.Format(null, "{0} Mask='{1}'", base.ToString(), Mask);
		}
		internal sealed override void WriteCache(IList data)
		{
			base.WriteCache(data);
			data.Add(Attribute.Mask);
		}
		new ModuleEditorAttribute Attribute
		{
			get { return (ModuleEditorAttribute)base.Attribute; }
		}
		public override ModuleItemKind Kind
		{
			get { return ModuleItemKind.Editor; }
		}
		public string Mask
		{
			get { return _Mask; }
			set
			{
				if (value == null) throw new ArgumentNullException("value");
				_Mask = value;
			}
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
			if (_Mask != Attribute.Mask)
				data.Add(idMask, _Mask);
			return data;
		}
		internal override void LoadData(Hashtable data)
		{
			if (data == null)
				_Mask = Attribute.Mask;
			else
				_Mask = data[idMask] as string ?? Attribute.Mask;
		}
	}
}
