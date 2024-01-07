using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FarNet.Tools;

class XPathObjectNodeFile : XPathObjectNode
{
	static ReadOnlyCollection<ValueGetter>? s_attributes;

	readonly XPathObjectContextFile _context;
	readonly SuperFile _tag;
	readonly string _name;

	public XPathObjectNodeFile(XPathObjectContextFile context, SuperFile tag) : this(context, tag, null, null, -1)
	{
	}

	XPathObjectNodeFile(
		XPathObjectContextFile context,
		SuperFile tag,
		XPathObjectNodeFile? parent,
		IList<XPathObjectNode>? siblings,
		int index)
		: base(
			parent,
			siblings,
			index)
	{
		_context = context ?? throw new ArgumentNullException(nameof(context));
		_tag = tag ?? throw new ArgumentNullException(nameof(tag));
		_name = context.NameTable.Add(tag.IsDirectory ? "Directory" : "File");
	}

	public override object Tag => _tag;

	public override string Name => _name;

	protected override IList<ValueGetter> GetAttributes()
	{
		if (s_attributes is { })
			return s_attributes;

		ValueGetter[] attrs =
		[
			new("Name", file => ((FarFile)file).Name),
			new("Description", file => ((FarFile)file).Description),
			new("Owner", file => ((FarFile)file).Owner),
			new("Length", file => ((FarFile)file).Length),
			new("CreationTime", file => ((FarFile)file).CreationTime),
			new("LastAccessTime", file => ((FarFile)file).LastAccessTime),
			new("LastWriteTime", file => ((FarFile)file).LastWriteTime),
			new("ReadOnly", file => ((FarFile)file).IsReadOnly),
			new("Hidden", file => ((FarFile)file).IsHidden),
			new("System", file => ((FarFile)file).IsSystem),
			new("Archive", file => ((FarFile)file).IsArchive),
			new("Compressed", file => ((FarFile)file).IsCompressed),
			new("ReparsePoint", file => ((FarFile)file).IsReparsePoint),
		];

		s_attributes = new ReadOnlyCollection<ValueGetter>(attrs);
		return s_attributes;
	}

	protected override IList<XPathObjectNode> GetElements()
	{
		if (!_tag.IsDirectory || _context.Exclude is { } exclude && exclude(_tag.Explorer, _tag.File))
			return EmptyElements;

		// progress
		_context.IncrementDirectoryCount?.Invoke(1);

		// explore and get files
		List<XPathObjectNode>? elements = null;
		Explorer? explorer2 = SuperExplorer.ExploreSuperDirectory(_tag.Explorer, ExplorerModes.Find, _tag);
		if (explorer2 is not null)
		{
			var args = new GetFilesEventArgs(ExplorerModes.Find);
			var files2 = explorer2.GetFiles(args);
			var count = files2 is ICollection collection ? collection.Count : 0;

			elements = new(count);
			foreach (var file2 in files2)
			{
				// filter files
				if (!file2.IsDirectory && (_context.SkipFiles || _context.Filter is not null && !_context.Filter(explorer2, file2)))
					continue;

				// add
				elements.Add(new XPathObjectNodeFile(
					_context,
					new SuperFile(explorer2, file2),
					this,
					elements,
					elements.Count));
			}
		}

		if (elements is null || elements.Count == 0)
			return EmptyElements;

		return elements;
	}
}
