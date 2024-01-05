using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FarNet.Tools;

class XPathObjectNodeFile : XPathObjectNode
{
	readonly SuperFile _tag;
	readonly string _nodeName;

	public XPathObjectNodeFile(XPathObjectContext context, SuperFile tag) : this(context, tag, null, null, -1)
	{
	}

	XPathObjectNodeFile(
		XPathObjectContext context,
		SuperFile tag,
		XPathObjectNodeFile? parent,
		IList<XPathObjectNode>? siblings,
		int index)
		: base(
			context,
			parent,
			siblings,
			index)
	{
		_tag = tag ?? throw new ArgumentNullException(nameof(tag));
		_nodeName = context.NameTable.Add(tag.IsDirectory ? "Directory" : "File");
	}

	public override object Tag => _tag;

	public override string Name => _nodeName;

	protected override void ActivateAttributes()
	{
		_attributes = XmlAttributes();
	}

	protected override IList<XPathObjectNode> ActivateElements()
	{
		if (_context.CancellationToken.IsCancellationRequested)
			return _emptyElements;

		if (_elements.Target is IList<XPathObjectNode> elements)
			return elements;

		return ActivateSuperFileElements();
	}

	static ReadOnlyCollection<ValueGetter>? s_attributes;
	static ReadOnlyCollection<ValueGetter> XmlAttributes()
	{
		if (s_attributes != null)
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

	IList<XPathObjectNode> ActivateSuperFileElements()
	{
		if (!_tag.IsDirectory)
		{
			_elements.Target = _emptyElements;
			return _emptyElements;
		}

		// progress
		_context.IncrementDirectoryCount?.Invoke(1);

		// explore and get files
		List<XPathObjectNode>? elements = null;
		if (_context.Depth < 0 || _depth < _context.Depth)
		{
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
		}

		if (elements is null || elements.Count == 0)
		{
			_elements.Target = _emptyElements;
			return _emptyElements;
		}
		else
		{
			_elements.Target = elements;
			return elements;
		}
	}
}
