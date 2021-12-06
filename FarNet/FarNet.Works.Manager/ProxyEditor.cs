
// FarNet plugin for Far Manager
// Copyright (c) Roman Kuzmin

using System;
using System.Collections;
using System.IO;

namespace FarNet.Works
{
	public sealed class ProxyEditor : ProxyAction, IModuleEditor
	{
		readonly int idMask = 0;
		string _Mask;

		new ModuleEditorAttribute Attribute => (ModuleEditorAttribute)base.Attribute;
		public override ModuleItemKind Kind => ModuleItemKind.Editor;

		internal ProxyEditor(ModuleManager manager, BinaryReader reader)
			: base(manager, reader, new ModuleEditorAttribute())
		{
			// [1]
			Attribute.Mask = reader.ReadString();

			Init();
		}

		internal sealed override void WriteCache(BinaryWriter writer)
		{
			base.WriteCache(writer);

			// [1]
			writer.Write(Attribute.Mask);
		}

		internal ProxyEditor(ModuleManager manager, Type classType)
			: base(manager, classType, typeof(ModuleEditorAttribute))
		{
			Init();
		}

		public void Invoke(IEditor editor, ModuleEditorEventArgs e)
		{
			Invoking();

			ModuleEditor instance = (ModuleEditor)GetInstance();
			instance.Invoke(editor, e);
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
			return data;
		}

		internal override void LoadConfig(Hashtable data)
		{
			if (data == null)
				_Mask = Attribute.Mask;
			else
				_Mask = data[idMask] as string ?? Attribute.Mask;
		}
	}
}
