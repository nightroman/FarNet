using System.Collections.Generic;
using System;

namespace FarManager.Impl
{
	abstract public class StoredItem : IFile
	{
		bool _isAlias;
		bool _isArchive;
		bool _isCompressed;
		bool _isDirectory;
		bool _isEncrypted;
		bool _isFolder = true;
		bool _isHidden;
		bool _isReadOnly;
		bool _isSystem;
		bool _isVolume;
		DateTime _creationTime;
		DateTime _lastAccessTime;
		IFolder _parent;
		long _size;
		string _alternateName = string.Empty;
		string _description = string.Empty;
		string _name = string.Empty;
		string _owner = string.Empty;
		string _path = string.Empty;

		public override string ToString()
		{
			return _path;
		}

		#region IFile Members

		public bool IsReadOnly
		{
			get { return _isReadOnly; }
			set { _isReadOnly = value; }
		}

		public DateTime CreationTime
		{
			get { return _creationTime; }
			set { _creationTime = value; }
		}

		public string Owner
		{
			get { return _owner; }
			set { _owner = value; }
		}

		public bool IsFolder
		{
			get { return _isFolder; }
			set { _isFolder = value; }
		}

		public bool IsEncrypted
		{
			get { return _isEncrypted; }
			set { _isEncrypted = value; }
		}

		public bool IsHidden
		{
			get { return _isHidden; }
			set { _isHidden = value; }
		}

		public string Path
		{
			get { return _path; }
			set { _path = value; }
		}

		public string Description
		{
			get { return _description; }
			set { _description = value; }
		}

		public bool IsAlias
		{
			get { return _isAlias; }
			set { _isAlias = value; }
		}

		public bool IsSystem
		{
			get { return _isSystem; }
			set { _isSystem = value; }
		}

		public bool IsArchive
		{
			get { return _isArchive; }
			set { _isArchive = value; }
		}

		public bool IsCompressed
		{
			get { return _isCompressed; }
			set { _isCompressed = value; }
		}

		public bool IsDirectory
		{
			get { return _isDirectory; }
			set { _isDirectory = value; }
		}

		public long Size
		{
			get { return _size; }
			set { _size = value; }
		}

		public string AlternateName
		{
			get { return _alternateName; }
			set { _alternateName = value; }
		}

		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public DateTime LastAccessTime
		{
			get { return _lastAccessTime; }
			set { _lastAccessTime = value; }
		}

		public IFolder Parent
		{
			get { return _parent; }
			set { _parent = value; }
		}

		public bool IsVolume
		{
			get { return _isVolume; }
			set { _isVolume = value; }
		}

		#endregion
	}

	public class StoredFile : StoredItem, IFile
	{
		public StoredFile()
		{
			IsDirectory = false;
			IsFolder = false;
		}
	}

	public class StoredFolder : StoredItem, IFolder
	{
		List<IFile> _files = new List<IFile>();

		public StoredFolder()
		{
			IsDirectory = true;
			IsFolder = true;
		}

		#region IFolder Members

		public IList<IFile> Files
		{
			get { return _files; }
		}

		#endregion
	}
}
